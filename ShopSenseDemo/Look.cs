using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data.SqlClient;
using System.Runtime.Serialization;

namespace ShopSenseDemo
{
    [DataContract]
    public class Look
    {
        [DataMember]
        public long id { get; set; }

        [DataMember]
        public List<Product> products { get; set; }

        [DataMember]
        public int upVote { get; set; }

        [DataMember]
        public int downVote { get; set; }

        [DataMember]
        public UserProfile creator { get; set; }

        [DataMember]
        public List<Tag> tags { get; set; }

        [DataMember]
        public string title { get; set; }

        [DataMember]
        public int viewCount { get; set; }

        [DataMember]
        public int restyleCount { get; set; }

        [DataMember]
        public UserProfile originalCreator { set; get; }

        [DataMember]
        public bool isLoved { get; set; }
        
        [DataMember]
        public bool isReStyled { get; set; }

        [DataMember]
        public long originalLookId { get; set; }

        [DataMember]
        public int shareCount { get; set; }

        //TODO: Deprecate contest id and name
        public int contestId { get; set; }
        
        public string contestName { get; set; }

      
        public string TagsFormatted()
        {
            string tagsFormatted = string.Empty;
            if (tags.Count > 0)
            {
                tagsFormatted = tags[0].name;
                for (int i = 1; i < tags.Count; i++)
                    tagsFormatted += ", " + tags[i].name;
            }
            return tagsFormatted;
        }

        public static Look GetLookFromSqlReader(SqlDataReader dr)
        {
            Look look = new Look();
            look.products = new List<Product>();
            look.tags = new List<Tag>();
            
            while (dr.Read())
            {
                if (dr != null)
                {
                    int vote = int.Parse(dr["Vote"].ToString());
                    if (vote != 2)
                        look.isLoved = true;
                }
            }
            dr.NextResult();

            while (dr.Read())
            {
                if (dr != null)
                {
                     look.isReStyled = true;
                }
            }
            dr.NextResult();

            while (dr.Read())
            {
                look.id = int.Parse(dr["Id"].ToString());
                look.upVote = int.Parse(dr["UpVote"].ToString());
                look.downVote = int.Parse(dr["DownVote"].ToString());
                look.title = dr["Title"].ToString();
                look.restyleCount = int.Parse(dr["ReStyleCount"].ToString());
                look.viewCount = int.Parse(dr["ViewCount"].ToString());
                look.shareCount = int.Parse(dr["ShareCount"].ToString());

                if (!string.IsNullOrEmpty(dr["OriginalLook"].ToString()))
                {
                    look.originalLookId = int.Parse(dr["OriginalLook"].ToString());
                }

                if (!string.IsNullOrEmpty(dr["contestId"].ToString()))
                {
                    look.contestId = int.Parse(dr["contestId"].ToString());
                }
            }

            //if a look was found
            if (look.id != 0)
            {
                dr.NextResult();

                // read the creator
                while (dr.Read())
                {
                    look.creator = UserProfile.GetUserFromSqlReader(dr);
                }

                //read original User
                dr.NextResult();
                while(dr.Read())
                {
                    look.originalCreator = UserProfile.GetUserFromSqlReader(dr);
                }

                // read the tags
                dr.NextResult();
                while (dr.Read())
                {
                    long tagId = long.Parse(dr["id"].ToString());
                    string tagName = dr["Name"].ToString();
                    Tag tag = new Tag(tagId, tagName);
                    look.tags.Add(tag);
                }
                   
                //read the products
                if (dr.NextResult())
                {  
                    while (dr.Read())
                    {
                        look.products.Add(Product.GetProductFromSqlDataReader(dr));
                    }
                }
            }

            return look;
        }
        public static List<Look> GetHPLooks(string db, long uId, long contestId)
        {
            List<Look> looks = new List<Look>();

            string query = "EXEC [stp_SS_GetFollowedLooks] @userId=" + uId;
            //if (contestId != 0)
            //{
            //    query += (", @contestId=" + contestId);
            //}

            SqlConnection myConnection = new SqlConnection(db);

            try
            {
                myConnection.Open();
                using (SqlDataAdapter adp = new SqlDataAdapter(query, myConnection))
                {
                    SqlCommand cmd = adp.SelectCommand;
                    cmd.CommandTimeout = 300000;
                    System.Data.SqlClient.SqlDataReader dr = cmd.ExecuteReader();

                    looks = Look.GetLooksFromSqlReader(dr);
                }
            }
            finally
            {
                myConnection.Close();
            }

            return looks;
        }

        public static List<Look> GetTaggedLooks(string db, long uId, long tagId)
        {
            List<Look> looks = new List<Look>();

            string query = "EXEC [stp_SS_GetTaggedLooks] @tagId=" + tagId + ",@userId=" + uId;
            

            SqlConnection myConnection = new SqlConnection(db);

            try
            {
                myConnection.Open();
                using (SqlDataAdapter adp = new SqlDataAdapter(query, myConnection))
                {
                    SqlCommand cmd = adp.SelectCommand;
                    cmd.CommandTimeout = 300000;
                    System.Data.SqlClient.SqlDataReader dr = cmd.ExecuteReader();

                    looks = Look.GetLooksFromSqlReader(dr);
                }
            }
            finally
            {
                myConnection.Close();
            }

            return looks;
        }


        //Get looks for homepage
        public static List<Look> GetLooksFromSqlReader(SqlDataReader dr)
        {
            List<Look> looks = new List<Look>();

            do
            {
                Look look = Look.GetLookFromSqlReader(dr);
                looks.Add(look);
            } while (dr.NextResult());

            return looks;
        }
        //Get a random product combo from the specified category and retailer
        
        public static Look GetRandomLook(int contestId, long userId, string db)
        {
            //Outfit choosing logic
            //Find outfits that are created/voted by friends which the user hasn't voted yet- if not try to find recently created best voted outfit

            Look look = new Look();
            look.products = new List<Product>();

            string query = "EXEC [stp_SS_GetRandomLook] @contestId=" + contestId + ", @userId =" + userId;
            SqlConnection myConnection = new SqlConnection(db);
            try
            {
                myConnection.Open();
                using (SqlDataAdapter adp = new SqlDataAdapter(query, myConnection))
                {
                    SqlCommand cmd = adp.SelectCommand;
                    cmd.CommandTimeout = 300000;
                    System.Data.SqlClient.SqlDataReader dr = cmd.ExecuteReader();

                    look = GetLookFromSqlReader(dr);
                }
            }
            finally
            {
                myConnection.Close();
            }
            return look;
        }

        //Get look By specific id
        public static Look GetLookById(long id, long userId, string db)
        {
            Look look = new Look();
            look.products = new List<Product>();
            
            string query = "EXEC [stp_SS_GetLookWithUserId] @id=" + id + ", @uid=" + userId;
            SqlConnection myConnection = new SqlConnection(db);
            try
            {
                myConnection.Open();
                using (SqlDataAdapter adp = new SqlDataAdapter(query, myConnection))
                {
                    SqlCommand cmd = adp.SelectCommand;
                    cmd.CommandTimeout = 300000;
                    System.Data.SqlClient.SqlDataReader dr = cmd.ExecuteReader();
                    look = GetLookFromSqlReader(dr);
                }
            }
            finally
            {
                myConnection.Close();
            }
            return look;
        }

        //Get look containing a particular product - helps product.aspx redirect
        public static Look GetLookByProductId(long id, string db)
        {
            Look look = new Look();
            look.products = new List<Product>();

            string query = "EXEC [stp_SS_GetLookByProduct] @id=" + id;
            SqlConnection myConnection = new SqlConnection(db);
            try
            {
                myConnection.Open();
                using (SqlDataAdapter adp = new SqlDataAdapter(query, myConnection))
                {
                    SqlCommand cmd = adp.SelectCommand;
                    cmd.CommandTimeout = 300000;
                    System.Data.SqlClient.SqlDataReader dr = cmd.ExecuteReader();

                    while (dr.Read())
                    {
                        look.id = int.Parse(dr["Id"].ToString());
                        look.upVote = int.Parse(dr["UpVote"].ToString());
                        look.downVote = int.Parse(dr["DownVote"].ToString());
                    }
                }
            }
            finally
            {
                myConnection.Close();
            }
            return look;
        }
    }
}
