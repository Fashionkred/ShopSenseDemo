using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data.SqlClient;

namespace ShopSenseDemo
{
    public enum NotificationType
    {
        Generic = 0,
        LoveLook = 1,
        ReStyle = 2,
        Comment = 3,
        CreateLook = 4,
        FollowUser = 5,
        Contest = 6,
        CreateLookTag=7,
        Marketing = 8
    }

    public class Contest
    {
        public string Image { get; set; }
        public long TagId { get; set; }
        public string Name { get; set; }
        public double CreateTime { get; set; }
        public NotificationType Type { get; set; }
    }

    public class MarketingMessage
    {
        public string Image { get; set; }
        public long TagId { get; set; }
        public string Name { get; set; }
        public double CreateTime { get; set; }
        public NotificationType Type { get; set; }
    }

    /*public class GenericNotification
    {
        public string Image { get; set; }
        public string Message { get; set; }
        public double CreateTime { get; set; }
        public string targetUrl {get; set;}
        public NotificationType Type { get; set; }
    }*/

    public class Notification
    {
        public UserProfile User { get; set; } // whom the notification is targeted to

        public UserProfile Subject { get; set; } // who caused the notification

        public Look Look { get; set; } // Look associated with the notification

        public NotificationType Type { get; set; } // Type of notification

        public double CreateTime { get; set; } // Time of notification

        public Tag Tag { get; set; } // Tag associated if any

        public Notification()
        {
            this.User = new UserProfile(); 
            this.Look = new Look();
            this.Subject = new UserProfile();
            this.CreateTime = (DateTime.UtcNow - new DateTime(1970, 1, 1)).TotalSeconds;
            this.Tag = new Tag();           
            this.Type = NotificationType.Generic;
        }

        public Notification(long lookId, long subjectId, long userId, NotificationType type)
        {
            this.User = new UserProfile(); this.User.userId = userId;
            this.Look = new Look(); this.Look.id = lookId;
            this.Subject = new UserProfile(); this.Subject.userId = subjectId;
            this.CreateTime = (DateTime.UtcNow - new DateTime(1970, 1, 1)).TotalSeconds;
            this.Type = type;
        }

        public static List<Object> GetLastNotifications(long userId, string db,  int offset = 1, int limit = 20)
        {
            List<object> notifications = new List<object>();
            SqlConnection myConnection = new SqlConnection(db);
            try
            {
                myConnection.Open();
                string query = "EXEC [stp_SS_GetLastNotifications] @userId=" +  userId + ",@offset=" + offset + ",@limit=" + limit;
                using (SqlDataAdapter adp = new SqlDataAdapter(query, myConnection))
                {
                    SqlCommand cmd = adp.SelectCommand;
                    cmd.CommandTimeout = 300000;
                    System.Data.SqlClient.SqlDataReader dr = cmd.ExecuteReader();

                    while (dr.Read())
                    {
                        NotificationType  type = (NotificationType)int.Parse(dr["Type"].ToString());
                        if (type == NotificationType.Contest)
                        {
                            Contest contest = new Contest();
                            contest.Name = dr["contestName"].ToString(); 
                            contest.TagId = int.Parse(dr["TagId"].ToString());
                            contest.Image = "https://s3-us-west-2.amazonaws.com/fkcontentpics/" + contest.Name + "_small.png";
                            contest.Type = type;
                            DateTime CreateTime = DateTime.Parse(dr["CreateDate"].ToString());
                            contest.CreateTime = (CreateTime - new DateTime(1970, 1, 1)).TotalSeconds;
                            notifications.Add(contest);
                        }

                        else
                        {
                            Notification notification = new Notification();
                            notification.Type = type;
                            notification.User.userId = userId;

                            notification.Subject.userId = long.Parse(dr["SubjectId"].ToString());
                            notification.Subject.pic = dr["Pic"].ToString();
                            notification.Subject.userName = dr["UserName"].ToString();
                            notification.Subject.IsFollowing = int.Parse(dr["following"].ToString());

                            if (notification.Type != NotificationType.FollowUser)
                            {
                                long lookId = long.Parse(dr["LookId"].ToString());
                                notification.Look = Look.GetLookById(lookId, userId, db);
                            }

                            if (notification.Type == NotificationType.CreateLookTag)
                            {
                                notification.Tag.id = long.Parse(dr["TagId"].ToString());
                                notification.Tag.name = dr["contestName"].ToString();
                            }

                            DateTime CreateTime = DateTime.Parse(dr["CreateDate"].ToString());
                            notification.CreateTime = (CreateTime - new DateTime(1970, 1, 1)).TotalSeconds;
                            notifications.Add(notification);
                        }
                        
                    }
                }
            }
            finally
            {
                myConnection.Close();
            }

            return notifications;
        }

        public static void SaveNotification(Notification notification, string db)
        {
            SqlConnection myConnection = new SqlConnection(db);
            try
            {
                string query = "EXEC [stp_SS_SaveNotification] @userId=" + notification.User.userId + ",@subjectId=" + notification.Subject.userId +
                                ", @type=" + (int)notification.Type;
                if (notification.Look.id != 0)
                {
                    query += (", @lookId=" + notification.Look.id);
                }
                myConnection.Open();
                using (SqlDataAdapter adp = new SqlDataAdapter(query, myConnection))
                {
                    SqlCommand cmd = adp.SelectCommand;
                    cmd.CommandTimeout = 300000;
                    cmd.ExecuteNonQuery();
                }
            }
            finally
            {
                myConnection.Close();
            }

        }


        public static void DeleteNotification(Notification notification, string db)
        {
            SqlConnection myConnection = new SqlConnection(db);
            try
            {
                string query = "EXEC [stp_SS_DeleteNotification] @userId=" + notification.User.userId + ",@subjectId=" + notification.Subject.userId +
                                ", @type=" + (int)notification.Type;
                if (notification.Look.id != 0)
                {
                    query += (", @lookId=" + notification.Look.id);
                }
                myConnection.Open();
                using (SqlDataAdapter adp = new SqlDataAdapter(query, myConnection))
                {
                    SqlCommand cmd = adp.SelectCommand;
                    cmd.CommandTimeout = 300000;
                    cmd.ExecuteNonQuery();
                }
            }
            finally
            {
                myConnection.Close();
            }

        }

        public static void DeleteLookNotification(Notification notification, string db)
        {
            SqlConnection myConnection = new SqlConnection(db);
            try
            {
                string query = "EXEC [stp_SS_DeleteLookNotification]  @lookId=" + notification.Look.id;
                
                myConnection.Open();
                using (SqlDataAdapter adp = new SqlDataAdapter(query, myConnection))
                {
                    SqlCommand cmd = adp.SelectCommand;
                    cmd.CommandTimeout = 300000;
                    cmd.ExecuteNonQuery();
                }
            }
            finally
            {
                myConnection.Close();
            }

        }

        public static bool IsNewNotification(long userId, string db)
        {
            bool newNote = false;

            SqlConnection myConnection = new SqlConnection(db);
            try
            {
                myConnection.Open();
                string query = "EXEC [stp_SS_IsNewNotification] @userId=" + userId;
                using (SqlDataAdapter adp = new SqlDataAdapter(query, myConnection))
                {
                    SqlCommand cmd = adp.SelectCommand;
                    cmd.CommandTimeout = 300000;
                    System.Data.SqlClient.SqlDataReader dr = cmd.ExecuteReader();

                    while (dr.Read())
                    {
                        int total = int.Parse(dr["total"].ToString());
                        if (total > 0)
                            newNote = true;
                    }
                }
            }
            finally
            {
                myConnection.Close();
            }

            return newNote;
        }

        public static void SaveCreateNotification(Notification notification, string db)
        {
            SqlConnection myConnection = new SqlConnection(db);
            try
            {
                string query = "EXEC [stp_SS_SaveCreateNotification] @userId=" + notification.User.userId + ", @type=" + (int)notification.Type;
                if (notification.Look.id != 0)
                {
                    query += (", @lookId=" + notification.Look.id);
                }
                myConnection.Open();
                using (SqlDataAdapter adp = new SqlDataAdapter(query, myConnection))
                {
                    SqlCommand cmd = adp.SelectCommand;
                    cmd.CommandTimeout = 300000;
                    cmd.ExecuteNonQuery();
                }
            }
            finally
            {
                myConnection.Close();
            }

        }
    }
}
