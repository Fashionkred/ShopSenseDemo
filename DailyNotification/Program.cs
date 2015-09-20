using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.IO;
using System.Linq;
using System.Text;
using PushSharp;
using PushSharp.Apple;
using PushSharp.Core;
using System.Data.SqlClient;

namespace DailyNotification
{
    public class Program
    {
        public static string db = "Data Source=startcult.com;Initial Catalog=Fashionkred;Persist Security Info=True;User ID=admin;Password=Fk711101";

        public static void SetLastRun(DateTime dt)
        {
            string query = "EXEC [stp_SS_SaveNotificationRunTime] @datetime='" + dt.ToString() + "'";
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
            }
            finally
            {
                myConnection.Close();
            }

            
        }
        public static List<string> GetTokens()
        {
            string query = "EXEC [stp_SS_GetBatchNotifications]";
            SqlConnection myConnection = new SqlConnection(db);
            List<string> tokens = new List<string>();

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
                        string token = dr["Token"].ToString();
                        tokens.Add(token);
                    }
                }
            }
            finally
            {
                myConnection.Close();
            }

            return tokens;
        }
        static void Main(string[] args)
        {
            var push = new PushBroker();

            //Wire up the events for all the services that the broker registers
            push.OnNotificationSent += NotificationSent;
            push.OnChannelException += ChannelException;
            push.OnServiceException += ServiceException;
            push.OnNotificationFailed += NotificationFailed;
            push.OnDeviceSubscriptionExpired += DeviceSubscriptionExpired;
            push.OnDeviceSubscriptionChanged += DeviceSubscriptionChanged;
            push.OnChannelCreated += ChannelCreated;
            push.OnChannelDestroyed += ChannelDestroyed;


            //------------------------------------------------
            //IMPORTANT NOTE about Push Service Registrations
            //------------------------------------------------
            //Some of the methods in this sample such as 'RegisterAppleServices' depend on you referencing the correct
            //assemblies, and having the correct 'using PushSharp;' in your file since they are extension methods!!!

            // If you don't want to use the extension method helpers you can register a service like this:
            //push.RegisterService<WindowsPhoneToastNotification>(new WindowsPhonePushService());

            //If you register your services like this, you must register the service for each type of notification
            //you want it to handle.  In the case of WindowsPhone, there are several notification types!



            var appleCert = @"C:\apns_prod_cert.p12";
            push.RegisterAppleService(new ApplePushChannelSettings(true, appleCert,"pass1234")); //Extension method

            DateTime dt = DateTime.UtcNow;

            List<string> tokens = GetTokens();
            foreach (string token in tokens)
            {
                push.QueueNotification(new AppleNotification()
                                           .ForDeviceToken(token)
                    //.WithAlert("Hello World!")
                                           .WithBadge(1));
                //.WithSound("sound.caf"));
            }
            Console.WriteLine("Waiting for Queue to Finish...");

            //Stop and wait for the queues to drains
            push.StopAllServices();

            SetLastRun(dt);

            Console.WriteLine("Queue Finished, press return to exit...");
            //Console.ReadLine();	
        }
        static void DeviceSubscriptionChanged(object sender, string oldSubscriptionId, string newSubscriptionId, INotification notification)
        {
            //Currently this event will only ever happen for Android GCM
            Console.WriteLine("Device Registration Changed:  Old-> " + oldSubscriptionId + "  New-> " + newSubscriptionId + " -> " + notification);
        }

        static void NotificationSent(object sender, INotification notification)
        {
            Console.WriteLine("Sent: " + sender + " -> " + notification);
        }

        static void NotificationFailed(object sender, INotification notification, Exception notificationFailureException)
        {
            
            Console.WriteLine("Failure: " + sender + " -> " + notificationFailureException.Message + " -> " + notification);
            NotificationFailureException ex = (NotificationFailureException)notificationFailureException;
            Console.WriteLine(ex.ErrorStatusDescription);
        }

        static void ChannelException(object sender, IPushChannel channel, Exception exception)
        {
            Console.WriteLine("Channel Exception: " + sender + " -> " + exception);
        }

        static void ServiceException(object sender, Exception exception)
        {
            Console.WriteLine("Service Exception: " + sender + " -> " + exception);
        }

        static void DeviceSubscriptionExpired(object sender, string expiredDeviceSubscriptionId, DateTime timestamp, INotification notification)
        {
            Console.WriteLine("Device Subscription Expired: " + sender + " -> " + expiredDeviceSubscriptionId);
        }

        static void ChannelDestroyed(object sender)
        {
            Console.WriteLine("Channel Destroyed for: " + sender);
        }

        static void ChannelCreated(object sender, IPushChannel pushChannel)
        {
            Console.WriteLine("Channel Created for: " + sender);
        }
    }
}
