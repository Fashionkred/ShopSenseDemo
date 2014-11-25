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

        [DataMember]
        public int commentCount { get; set; }

        [DataMember]
        public List<UserProfile> Likers { get; set; }

        [DataMember]
        public List<UserProfile> ReStylers { get; set; }

        [DataMember]
        public List<Comment> comments { get; set; }

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
            look.Likers = new List<UserProfile>();
            look.ReStylers = new List<UserProfile>();
            look.comments = new List<Comment>();
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

            //top likers
            while (dr.Read())
            {
                UserProfile liker = UserProfile.GetUserFromSqlReader(dr);
                look.Likers.Add(liker);
            }

            dr.NextResult();

            //top restylers
            while (dr.Read())
            {
                UserProfile reStyler = UserProfile.GetUserFromSqlReader(dr);
                look.ReStylers.Add(reStyler);
            }
            dr.NextResult();

            while (dr.Read())
            {
                Comment comment = new Comment();
                comment.id = long.Parse(dr["CommentId"].ToString());
                comment.commenter = UserProfile.GetUserFromSqlReader(dr);
                comment.commentText = dr["CommentText"].ToString().Replace("''", "'");
                DateTime commentTime = DateTime.Parse(dr["CommentCreateTime"].ToString());
                comment.commentTime = (commentTime - new DateTime(1970, 1, 1).ToLocalTime()).TotalSeconds;
                look.comments.Add(comment);
            }
            dr.NextResult();

            while (dr.Read())
            {
                look.id = int.Parse(dr["Id"].ToString());
                look.upVote = int.Parse(dr["UpVote"].ToString());
                look.downVote = int.Parse(dr["DownVote"].ToString());
                look.title = dr["Title"].ToString().Replace("''", "'");
                look.restyleCount = int.Parse(dr["ReStyleCount"].ToString());
                look.viewCount = int.Parse(dr["ViewCount"].ToString());
                look.shareCount = int.Parse(dr["ShareCount"].ToString());
                look.commentCount = int.Parse(dr["CommentCount"].ToString());

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
                List<Tag> tags = Tag.GetTagsFromSqlReader(dr);
                look.tags = tags;
                
                   
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

        public static List<Look> GetHomePageLooks(string db, long uId, int offset = 1, int limit = 10)
        {
            List<Look> looks = new List<Look>();
            string query;
            query = "EXEC [stp_SS_GetHomePageLooks] @userId=" + uId + ",@offset=" + offset + ",@limit=" + limit;
            

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

        public static List<Look> GetHPLooks(string db, long uId, int offset=1, int limit=15, bool isPopular = false)
        {
            List<Look> looks = new List<Look>();
            string query;

            if (isPopular)
            {
                query = "EXEC [stp_SS_GetPopularLooks] @userId=" + uId + ",@offset=" + offset + ",@limit=" + limit;
            }
            else
            {
                query = "EXEC [stp_SS_GetFollowedLooks] @userId=" + uId + ",@offset=" + offset + ",@limit=" + limit;
            }

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

        public static List<Look> GetTaggedLooks(string db, long uId, long tagId, int offset = 1, int limit = 20, bool isPopular= false)
        {
            List<Look> looks = new List<Look>();

            string query;

            if (isPopular)
            {
                query = "EXEC [stp_SS_GetTaggedPopularLooks] @tagId=" + tagId + ",@userId=" + uId + ",@offset=" + offset + ",@limit=" + limit;
            }
            else
            {
                query = "EXEC [stp_SS_GetTaggedLooks] @tagId=" + tagId + ",@userId=" + uId + ",@offset=" + offset + ",@limit=" + limit;
            }

            SqlConnection myConnection = new SqlConnection(db);

            try
            {
                myConnection.Open();
                using (SqlDataAdapter adp = new SqlDataAdapter(query, myConnection))
                {
                    SqlCommand cmd = adp.SelectCommand;
                    cmd.CommandTimeout = 300000;
                    System.Data.SqlClient.SqlDataReader dr = cmd.ExecuteReader();
                    int countLooks = 0;
                    while (dr.Read())
                    {
                        countLooks = int.Parse(dr["total"].ToString());
                    }
                    dr.NextResult();
                    looks = Look.GetLooksFromSqlReader(dr);
                }
            }
            finally
            {
                myConnection.Close();
            }

            return looks;
        }

        public static bool DeleteLook(string db, long uId, long lookId, int isEditMode = 0)
        {
            string query = "EXEC [stp_SS_DeleteLook] @lid=" + lookId + ",@uid=" + uId + ",@editLook =" + isEditMode;
            SqlConnection myConnection = new SqlConnection(db);
            bool isSuccess = false;
            try
            {
                myConnection.Open();
                using (SqlDataAdapter adp = new SqlDataAdapter(query, myConnection))
                {
                    SqlCommand cmd = adp.SelectCommand;
                    cmd.CommandTimeout = 300000;
                    System.Data.SqlClient.SqlDataReader dr = cmd.ExecuteReader();
                    isSuccess = true;
                }
            }
            finally
            {
                myConnection.Close();
            }
            return isSuccess;
        }

        public static Look SaveLook(string db, string productMap, long userId, string tagMap, string title, long originalLookId = 0, long editLookId = 0)
        {
            Look look = new Look();

            string query = "EXEC [stp_SS_SaveLook] @product='" + productMap + "', @uid=" + userId + ",@tag='" + tagMap.Replace("'", "''") + "', @title=N'" + title.Replace("'", "''") + "'";
            if (originalLookId!= 0)
            {
                query += (", @originalLook=" + originalLookId);
            }

            if (editLookId!= 0)
            {
                query += (", @editLookId=" + editLookId);
            }
            SqlConnection myConnection = new SqlConnection(db);

            try
            {
                myConnection.Open();
                using (SqlDataAdapter adp = new SqlDataAdapter(query, myConnection))
                {
                    SqlCommand cmd = adp.SelectCommand;
                    cmd.CommandTimeout = 300000;
                    System.Data.SqlClient.SqlDataReader dr = cmd.ExecuteReader();
                    
                    look = Look.GetLookFromSqlReader(dr);
                }
            }
            finally
            {
                myConnection.Close();
            }
            return look;
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

        //heart a look
        public static bool HeartLook(long userId, long lookId, bool isHeart,  string db)
        {
            bool isSuccess = false;

            int heart = isHeart == true ? 1 : 0;

            string query = "EXEC [stp_SS_SaveVote] @uid=" + userId + ", @lid=" + lookId + ", @vote=" + heart + ", @pointinc=" + 0;
            SqlConnection myConnection = new SqlConnection(db);

            try
            {
                myConnection.Open();
                using (SqlDataAdapter adp = new SqlDataAdapter(query, myConnection))
                {
                    SqlCommand cmd = adp.SelectCommand;
                    cmd.CommandTimeout = 300000;
                    cmd.ExecuteNonQuery();
                }
                isSuccess = true;
                
            }
            finally
            {
                myConnection.Close();
            }
            return isSuccess;
        }

        //get list of likers
        public static List<UserProfile> GetLikers(string db,long lookId, int offset = 1, int limit = 20)
        {
            List<UserProfile> likers = new List<UserProfile>();

            string query = "EXEC [stp_SS_GetLikers] @lookId=" + lookId + ",@offset=" + offset + ",@limit=" + limit;
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
                        UserProfile liker = UserProfile.GetUserFromSqlReader(dr);
                        likers.Add(liker);
                    }

                }
            }
            finally
            {
                myConnection.Close();
            }
            return likers;
        }

        //get list of likers
        public static List<LightUser> GetLikersandRestylers(string db, long lookId, int offset = 1, int limit = 20)
        {
            List<LightUser> users = new List<LightUser>();

            string query = "EXEC [stp_SS_GetLikers&Restylers] @lookId=" + lookId + ",@offset=" + offset + ",@limit=" + limit;
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
                        LightUser user = LightUser.GetUserFromSqlReader(dr);
                        users.Add(user);
                    }

                }
            }
            finally
            {
                myConnection.Close();
            }
            return users;
        }

        //get list of reStylers
        public static List<UserProfile> GetReStylers(string db, long lookId, int offset = 1, int limit = 20)
        {
            List<UserProfile> reStylers = new List<UserProfile>();

            string query = "EXEC [stp_SS_GetReStylers] @lookId=" + lookId + ",@offset=" + offset + ",@limit=" + limit;
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
                        UserProfile reStyler = UserProfile.GetUserFromSqlReader(dr);
                        reStylers.Add(reStyler);
                    }

                }
            }
            finally
            {
                myConnection.Close();
            }
            return reStylers;
        }
    }
}
