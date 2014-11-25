using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.Serialization;
using System.Data.SqlClient;

namespace ShopSenseDemo
{
    [DataContract]
    public class Comment
    {
        [DataMember]
        public long id { get; set; }

        [DataMember]
        public UserProfile commenter { get; set; }

        [DataMember]
        public double commentTime { get; set; }

        [DataMember]
        public string commentText { set; get;}

        //get list of reStylers
        public static List<Comment> GetComments(string db, long lookId, int offset = 1, int limit = 20)
        {
            List<Comment> comments = new List<Comment>();

            string query = "EXEC [stp_SS_GetComments] @lookId=" + lookId + ",@offset=" + offset + ",@limit=" + limit;
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
                        Comment comment = new Comment();
                        comment.id = long.Parse(dr["CommentId"].ToString());
                        comment.commenter = UserProfile.GetUserFromSqlReader(dr);
                        comment.commentText = dr["CommentText"].ToString().Replace("''", "'");
                        DateTime commentTime = DateTime.Parse(dr["CommentCreateTime"].ToString());
                        comment.commentTime =  (commentTime - new DateTime(1970, 1, 1).ToLocalTime()).TotalSeconds;
                        comments.Add(comment);
                    }
                }
            }
            finally
            {
                myConnection.Close();
            }
            return comments;
        }

        //add comment
        public static bool AddComment(long userId, long lookId, string comment, string db)
        {
            bool isSuccess = false;

            string commentString = comment.Replace("'", "''").Substring(0, Math.Min(500, comment.Length));
            string query = "EXEC [stp_SS_AddComment] @uid=" + userId + ", @lid=" + lookId + ", @comment=N'" + commentString.Replace("'", "''") + "'";
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

        public static bool DeleteComment(string db, long uId, long lookId, long commentId)
        {
            string query = "EXEC [stp_SS_DeleteComment] @lid=" + lookId + ",@uid=" + uId + ",@cid=" + commentId;
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

    }
}
