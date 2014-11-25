using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.Serialization;
using System.Data.SqlClient;
using System.Data;

namespace ShopSenseDemo
{
    public sealed class TagType
    {

        public readonly String name;
        private readonly int value;

        public static readonly TagType ENTERTAINMENT = new TagType(1, "ENTERTAINMENT");
        public static readonly TagType STYLETRENDS = new TagType(2, "STYLE TRENDS");
        public static readonly TagType PEOPLENEWSEVENTS = new TagType(3, "PEOPLE, NEWS & EVENTS");
        public static readonly TagType DESIGNERS = new TagType(4, "DESIGNERS");

        private TagType(int value, String name)
        {
            this.name = name;
            this.value = value;
        }

        public override String ToString()
        {
            return name;
        }

        

    }

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
        public string BigBannerUrl { get; set; }

        [DataMember]
        public string SwatchUrl { get; set; }

        [DataMember]
        public string prettyName { get; set; }

        public int IsFollowing { get; set; }

        public TagType type { get; set; }


        public static bool ColumnExists(IDataReader reader, string columnName)
        {
            for (int i = 0; i < reader.FieldCount; i++)
            {
                if (reader.GetName(i) == columnName)
                {
                    return true;
                }
            }

            return false;
        }

        public static Tag GetTagFromSqlReader(SqlDataReader dr)
        {
            Tag tag = new Tag();

            tag.id = long.Parse(dr["Id"].ToString());
            tag.name = dr["Name"].ToString().Replace("''", "'");
            tag.imageUrl = dr["ImageUrl"].ToString();
            tag.prettyName = dr["PrettyName"].ToString();
            tag.BigBannerUrl = dr["BigBannerUrl"].ToString();
            tag.SwatchUrl = dr["SwatchUrl"].ToString();
            if (!string.IsNullOrEmpty(dr["Type"].ToString()))
            {
                int type = int.Parse(dr["Type"].ToString());
                switch (type)
                {
                    case 1: tag.type = TagType.ENTERTAINMENT;
                        break;
                    case 2: tag.type = TagType.STYLETRENDS;
                        break;
                    case 3: tag.type = TagType.PEOPLENEWSEVENTS;
                        break;
                    case 4: tag.type = TagType.DESIGNERS;
                        break;
                }

            }

            
            if (ColumnExists(dr, "following") && !string.IsNullOrEmpty(dr["following"].ToString()))
            {
                tag.IsFollowing = int.Parse(dr["following"].ToString());
            }
            return tag;
        }

        public static List<Tag> GetTagsFromSqlReader(SqlDataReader dr)
        {
            List<Tag> tags = new List<Tag>();

            while (dr.Read())
            {
                Tag tag = Tag.GetTagFromSqlReader(dr);
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

                    int counter = 0; int countLooks = 0;
                    while (dr.Read())
                    {
                        countLooks = int.Parse(dr["total"].ToString());
                    }
                    dr.NextResult();
                    do
                    {
                        Look look = Look.GetLookFromSqlReader(dr);
                        popularLooks.Add(look);
                        counter++;
                        if (counter >= noOfLooks || counter >= countLooks)
                            break;
                    } while (dr.NextResult());
                    metaInfo.Add("Popular Looks", popularLooks);

                    /*Get fresh looks
                    List<Look> recentLooks = new List<Look>();
                    dr.NextResult();
                    counter = 0; countLooks = 0;
                    while (dr.Read())
                    {
                        countLooks = int.Parse(dr["total"].ToString());
                    }
                    dr.NextResult();
                    do
                    {
                        Look look = Look.GetLookFromSqlReader(dr);
                        recentLooks.Add(look);
                        counter++;
                        if (counter >= noOfLooks || counter >= countLooks)
                            break;
                    } while (dr.NextResult());
                    metaInfo.Add("Recent Looks", recentLooks);*/

                    //Get top stylists
                    List<UserProfile> stylists = new List<UserProfile>();
                    dr.NextResult();
                    while (dr.Read())
                    {
                        UserProfile user = UserProfile.GetUserFromSqlReader(dr);
                        stylists.Add(user);
                    }
                    metaInfo.Add("Top Stylists", stylists);

                    /*Get top products
                    List<Product> topItems = new List<Product>();
                    dr.NextResult();

                    while (dr.Read())
                    {
                        topItems.Add(Product.GetProductFromSqlDataReader(dr));
                    }
                    metaInfo.Add("Top Items", topItems);*/

                    dr.NextResult();
                    Tag tag = new Tag();
                    while (dr.Read())
                    {
                        tag = Tag.GetTagFromSqlReader(dr);
                    }
                    metaInfo.Add("Tag", tag);
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
