using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ShopSenseDemo;
using System.Data.SqlClient;
using Facebook;

namespace WeeklyNewsLetter
{
    class Program
    {
        public const string appId ="155821997899161";
        public const string appSecret = "726ce6b86758dc53c06f86e16414140f";
        public const int  contestId = 4;

        static void Main(string[] args)
        {
            string db = @"Data Source=mssql2008.reliablesite.net,14333,14333;Initial Catalog=facebook;Persist Security Info=True;User ID=nirveek_de;Password=Ns711101";
            string query = "EXEC [stp_SS_NewsLetter] @contestId=" + contestId;
            SqlConnection myConnection = new SqlConnection(db);
            Look look = new Look();
            List<long> subscriberList = new List<long>();
                    
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
                        long fbId = long.Parse(dr["FacebookId"].ToString());
                        subscriberList.Add(fbId);
                    }
                    dr.NextResult();

                    look = Look.GetLookFromSqlReader(dr);
                }
               
                 SendLookNotification(subscriberList, look);
                
            }
            finally
            {
                myConnection.Close();
            }
        }
        public static void SendLookNotification(List<long> subscriberList, Look look)
        {
            var client = new FacebookClient();
            client.AppId = appId;
            client.AppSecret = appSecret;
            
            dynamic response = client.Get("oauth/access_token",
            new
            {
                client_id = appId,
                client_secret = appSecret,
                grant_type = "client_credentials"
            });
            string appAccessToken = response.access_token;
            
            foreach (long subscriber in subscriberList)
            {
                if (subscriber > 0)
                {
                    string path = "/" + subscriber + "/notifications?access_token=" + appAccessToken;
                    string templateString = "#LookOfTheWeek: Check out the top voted look of the week in FashionKred!";

                    var parameters = new Dictionary<string, object>
                    {
                            { "href" ,  "?lid=" + look.id },
                            { "template" ,  templateString },
                            { "ref" ,  "looknotifications" }
                    };

                    try
                    {
                        client.Post(path, parameters);
                    }
                    catch(FacebookOAuthException ex)
                    {
                        //log exception and unsubscribe
                    }
                }
            }
        }
    }
}
