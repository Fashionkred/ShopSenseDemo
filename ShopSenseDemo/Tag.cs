using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.Serialization;
using System.Data.SqlClient;

namespace ShopSenseDemo
{
    [DataContract]
    public class Tag
    {
        [DataMember]
        public long id { get; set; }

        [DataMember]
        public string name { get; set; }

        [DataMember]
        public string imageUrl { get; set; }

        [DataMember]
        public string prettyName { get; set; }


        public static List<Tag> GetTagsFromSqlReader(SqlDataReader dr)
        {
            List<Tag> tags = new List<Tag>();

            while (dr.Read())
            {
                Tag tag = new Tag();

                tag.id = long.Parse(dr["Id"].ToString());
                tag.name = dr["Name"].ToString();
                tag.imageUrl = dr["ImageUrl"].ToString();
                tag.prettyName = dr["PrettyName"].ToString();
                tags.Add(tag);
            }

            return tags;
        }

        public static List<Tag> getPopularHashtags(long userId, string db, int offset = 1, int limit = 10)
        {
            List<Tag> popularTags = new List<Tag>();
            string query = "EXEC [stp_SS_GetPopularTags] @userId=" + userId + ",@offset=" + offset + ",@limit=" + limit;

            SqlConnection myConnection = new SqlConnection(db);

            try
            {
                myConnection.Open();
                using (SqlDataAdapter adp = new SqlDataAdapter(query, myConnection))
                {
                    SqlCommand cmd = adp.SelectCommand;
                    cmd.CommandTimeout = 300000;
                    System.Data.SqlClient.SqlDataReader dr = cmd.ExecuteReader();

                    popularTags = Tag.GetTagsFromSqlReader(dr);
                }
            }
            finally
            {
                myConnection.Close();
            }

            return popularTags;
        }

        public static Dictionary<string, object> GetTagMetaInfo(long userId, long tagId, int noOfLooks, int noOfItems, int noOfStylists, string db)
        {
            Dictionary<string, object> metaInfo = new Dictionary<string, object>();

            string query = "EXEC [stp_SS_GetTagMetaInfo] @tagId=" + tagId + ",@userId=" + userId + ",@noOfLooks=" + noOfLooks +",@noOfItems=" + noOfItems + ",@noOfStylists=" + noOfStylists;

            SqlConnection myConnection = new SqlConnection(db);

            try
            {
                myConnection.Open();
                using (SqlDataAdapter adp = new SqlDataAdapter(query, myConnection))
                {
                    SqlCommand cmd = adp.SelectCommand;
                    cmd.CommandTimeout = 300000;
                    System.Data.SqlClient.SqlDataReader dr = cmd.ExecuteReader();

                    //Get popular looks
                    List<Look> popularLooks = new List<Look>();

                    int counter = 0;
                    do
                    {
                        Look look = Look.GetLookFromSqlReader(dr);
                        popularLooks.Add(look);
                        counter++;
                        if (counter >= noOfLooks)
                            break;
                    } while (dr.NextResult());
                    metaInfo.Add("Popular Looks", popularLooks);

                    //Get fresh looks
                    List<Look> recentLooks = new List<Look>();
                    dr.NextResult();
                    counter = 0;
                    do
                    {
                        Look look = Look.GetLookFromSqlReader(dr);
                        recentLooks.Add(look);
                        counter++;
                        if (counter >= noOfLooks)
                            break;
                    } while (dr.NextResult());
                    metaInfo.Add("Recent Looks", recentLooks);

                    //Get top stylists
                    List<UserProfile> stylists = new List<UserProfile>();
                    dr.NextResult();
                    while (dr.Read())
                    {
                        UserProfile user = new UserProfile();
                        user.id = long.Parse(dr["Id"].ToString());
                        user.pic = dr["Pic"].ToString();
                        user.name = dr["Name"].ToString();
                        stylists.Add(user);
                    }
                    metaInfo.Add("Top Stylists", stylists);

                    //Get top products
                    List<Product> topItems = new List<Product>();
                    dr.NextResult();

                    while (dr.Read())
                    {
                        topItems.Add(Product.GetProductFromSqlDataReader(dr));
                    }
                    metaInfo.Add("Top Items", topItems);
                }
            }
            finally
            {
                myConnection.Close();
            }

            return metaInfo;
        }
    }
}
