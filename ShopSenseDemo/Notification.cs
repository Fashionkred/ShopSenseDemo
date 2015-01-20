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
        Contest = 6
    }

    public class Notification
    {
        public UserProfile User { get; set; } // whom the notification is targeted to

        public UserProfile Subject { get; set; } // who caused the notification

        public Look Look { get; set; } // Look associated with the notification

        public NotificationType Type { get; set; } // Type of notification

        public DateTime CreateTime { get; set; } // Time of notification

        public Notification()
        {
            this.User = new UserProfile(); 
            this.Look = new Look();
            this.Subject = new UserProfile(); 
            this.CreateTime = DateTime.UtcNow;
            this.Type = NotificationType.Generic;
        }

        public Notification(long lookId, long subjectId, long userId, NotificationType type)
        {
            this.User = new UserProfile(); this.User.userId = userId;
            this.Look = new Look(); this.Look.id = lookId;
            this.Subject = new UserProfile(); this.Subject.userId = subjectId;
            this.CreateTime = DateTime.UtcNow;
            this.Type = type;
        }

        public static List<Notification> GetLastNotifications(long userId, string db,  int offset = 1, int limit = 20)
        {
            List<Notification> notifications = new List<Notification>();
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
                        Notification notification = new Notification();
                        notification.Type = (NotificationType)int.Parse(dr["Type"].ToString());
                        if (notification.Type == NotificationType.Contest)
                        {
                            notification.Look.contestId = int.Parse(dr["LookId"].ToString());
                            notification.Look.contestName = dr["contestName"].ToString(); 
                        }

                        else
                        {

                            notification.User.userId = userId;

                            notification.Subject.userId = long.Parse(dr["SubjectId"].ToString());
                            notification.Subject.pic = dr["Pic"].ToString();
                            notification.Subject.name = dr["Name"].ToString();


                            if (notification.Type != NotificationType.FollowUser)
                            {
                                long lookId = long.Parse(dr["LookId"].ToString());
                                notification.Look = Look.GetLookById(lookId, userId, db);
                            }

                            notification.CreateTime = DateTime.Parse(dr["CreateDate"].ToString());
                        }
                        notifications.Add(notification);
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
    }
}
