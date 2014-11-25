using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data.SqlClient;

namespace ShopSenseDemo
{
    public enum NotificationType
    {
        LoveLook = 1,
        FollowUser =2,
        CreateLook = 3,
        TopLook =4,
        Welcome = 5,
        ReStyle = 6
    }

    public class Notification
    {
        public UserProfile User { get; set; } // whom the notification is targeted to

        public UserProfile Subject { get; set; } // who caused the notification

        public Look Look { get; set; } // Look associated with the notification

        public NotificationType Type { get; set; } // Type of notification

        public DateTime CreateTime { get; set; } // Time of notification

       

        public static List<Notification> GetLastNotifications(UserProfile user, string db)
        {
            List<Notification> notifications = new List<Notification>();
            SqlConnection myConnection = new SqlConnection(db);
            try
            {
                myConnection.Open();
                string query = "EXEC [stp_SS_GetLastNotifications] @userId=" +  user.userId;
                using (SqlDataAdapter adp = new SqlDataAdapter(query, myConnection))
                {
                    SqlCommand cmd = adp.SelectCommand;
                    cmd.CommandTimeout = 300000;
                    System.Data.SqlClient.SqlDataReader dr = cmd.ExecuteReader();

                    while (dr.Read())
                    {
                        Notification notification = new Notification();
                        notification.User = user;

                        notification.Subject = new UserProfile();
                        notification.Subject.userId = long.Parse(dr["SubjectId"].ToString());
                        notification.Subject.pic = dr["Pic"].ToString();
                        notification.Subject.name = dr["Name"].ToString();

                        notification.Type = (NotificationType)int.Parse(dr["Type"].ToString());

                        notification.Look = new Look();
                        notification.Look.id = long.Parse(dr["LookId"].ToString());

                        notification.CreateTime = DateTime.Parse(dr["CreateDate"].ToString());

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
                if (notification.Look != null)
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
