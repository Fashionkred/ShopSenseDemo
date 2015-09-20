using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ShopSenseDemo;
using System.Data.SqlClient;
using Facebook;
using Mandrill;
using System.Runtime.Serialization.Json;
using System.IO;
using System.Net;

namespace WeeklyNewsLetter
{

    class Program
    {
        public const string appId ="155821997899161";
        public const string appSecret = "726ce6b86758dc53c06f86e16414140f";
        public const int  contestId = 4;
        public const string apiKey = "-qH5VX30hJ56Iv5_jROffg";
        //public const string apiKey = "WN3awyeZNLu4In9hqj9vEA";
        
        public const string db ="Data Source=startcult.com;Initial Catalog=Fashionkred;Persist Security Info=True;User ID=admin;Password=Fk711101";
        public const string emailFile = @"https://s3-us-west-2.amazonaws.com/fkconfigs/weeklyEmail_schedule.json";     

        public static void SendRetentionEmail(string templateName, string subject, int days, string userAgent = "iPad")
        {
            List<string> usernames = new List<string>();
            List<string> emails = new List<string>();
            string query = "";
            switch (days)
            {
                case 1:
                    query = "EXEC [stp_SS_Retention1d]";
                    break;
                case 2:
                    query = "EXEC [stp_SS_Retention2d] ";
                    break;
                case 3:
                    query = "EXEC [stp_SS_Retention3d] @userAgent='" + userAgent + "'";
                    break;
                case 4:
                    query = "EXEC [stp_SS_Retention4d] ";
                    break;
                case 7:
                    query = "EXEC [stp_SS_Retention7d] ";
                    break;
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

                    while (dr.Read())
                    {
                        string username = dr["username"].ToString();
                        string email = dr["email"].ToString();
                        //string email = "nirveek@gmail.com";
                       
                        usernames.Add(username);
                        emails.Add(email);
                    }

                }
            }
            finally
            {
                myConnection.Close();
            }

            var api = new MandrillApi(apiKey);

            for (int i = 0; i < usernames.Count(); i++)
            {
                var recipients = new List<EmailAddress>();
                recipients.Add(new EmailAddress(emails[i], usernames[i]));


                var message = new EmailMessage()
                {
                    to = recipients,
                    from_email = "support@startcult.com",
                    from_name = "Cult Collection",
                    subject = subject,

                };
                message.AddGlobalVariable("FNAME", usernames[i]);

                var result = api.SendMessage(message, templateName, null);
                //result.
            }
        }

        public static List<EmailTemplate> GetEligibleTemplates()
        {
            EmailTemplates templates = new EmailTemplates(); templates.emails = new List<EmailTemplate>();

            System.Net.ServicePointManager.Expect100Continue = false;

            using (WebClient client = new WebClient())
            {
                try
                {
                    using (Stream stream = client.OpenRead(emailFile))
                    {
                        DataContractJsonSerializer ser = new DataContractJsonSerializer(typeof(EmailTemplates));
                        //stream.Position = 0;
                        templates = (EmailTemplates)ser.ReadObject(stream);
                    }
                }
                catch (WebException ex)
                { }

            }

            List<EmailTemplate> eligibleTemplate = new List<EmailTemplate>();
            foreach (EmailTemplate email in templates.emails)
            {
                DateTime startDate = DateTime.Parse(email.date);
                if (startDate < DateTime.UtcNow && startDate > DateTime.UtcNow.Subtract(new TimeSpan(24, 0, 0)))
                {
                    eligibleTemplate.Add(email);
                    break;
                }
                else
                    continue;
            }

            return eligibleTemplate;
        }

        public static List<Look> SendEmailToFeaturedLookCreator(List<long> lookIds, string tagName)
        {
            var api = new MandrillApi(apiKey);
            List<Look> looks = new List<Look>();
            
            //Send look owenrs congrats email
            if (lookIds.Count > 0)
            {

                //get look details and fill it in
                for (int l = 1; l <= lookIds.Count; l++)
                {
                    Look look = Look.GetLookById(lookIds[l - 1], 0, db);
                    looks.Add(look);
                    var recipients = new List<EmailAddress>();
                    recipients.Add(new EmailAddress(look.creator.emailId, look.creator.userName));
                    var message = new EmailMessage()
                    {
                        to = recipients,
                        from_email = "support@startcult.com",
                        from_name = "Cult Collection",
                        subject = "Your Look Was Featured",

                    };

                    if (look.id > 0)
                    {
                        message.AddGlobalVariable("L1URL", "http://startcult.com/look.html?lookId=" + look.id + "&userName=CultCollection&url=http://startcult.com/images/100x100_identity_.png&utm_source=email&utm_medium=WeeklyDigest&utm_term=consumption");
                        message.AddGlobalVariable("L1IMG", "http://startcult.com/images/looks/" + look.id + ".jpg");
                        message.AddGlobalVariable("U1", look.creator.userName);
                        message.AddGlobalVariable("S1", (look.upVote + look.restyleCount).ToString());
                        message.AddGlobalVariable("L1DESC", look.title);
                    }
                    message.important = true;
                    var result = api.SendMessage(message, "Your Look was Featured", null);

                    //Make it featured
                    if (!string.IsNullOrEmpty(tagName))
                    {
                        string query = "EXEC [stp_SS_UpdateLookTagMap] @tName=N'" + tagName + "', @lId=" + lookIds[l - 1] + ", @type=1,@featured=1";
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
                }
            }
            return looks;
        }

        public static void SendDigestEmail()
        {
            List<string> usernames = new List<string>();
            List<string> emails = new List<string>();
            List<long> userIds = new List<long>();

            List<EmailTemplate> eligibleTemplate = GetEligibleTemplates();
            if (eligibleTemplate.Count == 0)
                return;

            string query = "EXEC [stp_SS_WeekDigest] ";

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
                        string username = dr["username"].ToString();
                        string email = dr["email"].ToString();
                        //string email = "nirveek@gmail.com";
                        long id = long.Parse(dr["id"].ToString());

                        usernames.Add(username);
                        emails.Add(email);
                        userIds.Add(id);
                    }

                }
            }
            finally
            {
                myConnection.Close();
            }

            var api = new MandrillApi(apiKey);
            foreach (EmailTemplate t in eligibleTemplate)
            {
                List<Look> featuredLooks = new List<Look>();
                List<Look> topLooks = new List<Look>();
                List<Tag> featuredTags = new List<Tag>();

                if (t.templateName == "Weekly Consumption Email" || t.templateName == "Weekly Consumption Email with Editorial")
                {
                    string tagName = string.Empty;
                    if (t.tags.Count > 0)
                        tagName = t.tags[0];

                    featuredLooks = SendEmailToFeaturedLookCreator(t.lookIds, tagName);
                }
                else if (t.templateName == "Weekly Content Email")
                {
                    topLooks = Look.GetPopularLooksOfWeek(db, 1, 1, 3);
                    if (t.tags.Count > 0)
                    {
                        for (int tt = 1; tt <= t.tags.Count; tt++)
                        {
                            Tag tag = Tag.GetTagByName(t.tags[tt - 1], 0, db);
                            featuredTags.Add(tag);
                        }
                    }
                }

                for (int i = 0; i < usernames.Count(); i++)
                {
                    var recipients = new List<EmailAddress>();
                    recipients.Add(new EmailAddress(emails[i], usernames[i]));


                    var message = new EmailMessage()
                    {
                        to = recipients,
                        from_email = "support@startcult.com",
                        from_name = "Cult Collection",
                        subject = t.subject,

                    };
                    message.AddGlobalVariable("FNAME", usernames[i]);
                    message.AddGlobalVariable("EH1", t.preHeaderText);
                    message.AddGlobalVariable("H1", t.headline);
                    message.AddGlobalVariable("SH1", t.description);

                    if (t.templateName == "Weekly Consumption Email" || t.templateName == "Weekly Consumption Email with Editorial")
                    {
                        //get look details and fill it in
                        for (int l = 1; l <= featuredLooks.Count; l++)
                        {
                            Look look = featuredLooks[l-1];
                            if (look.id > 0)
                            {
                                message.AddGlobalVariable("L" + l + "URL", "http://startcult.com/look.html?lookId=" + look.id + "&userName=CultCollection&url=http://startcult.com/images/100x100_identity_.png&utm_source=email&utm_medium=WeeklyDigest&utm_term=consumption");
                                message.AddGlobalVariable("L" + l + "IMG", "http://startcult.com/images/looks/" + look.id + ".jpg");
                                message.AddGlobalVariable("U" + l, look.creator.userName);
                                message.AddGlobalVariable("S" + l, (look.upVote + look.restyleCount).ToString());
                                message.AddGlobalVariable("L" + l + "DESC", look.title);
                            }
                        }

                        //get top brands
                        List<Product> hotItems = Product.GetNewProductsByUser(userIds[i], db, 1, 6);
                        List<string> brandNames = new List<string>();
                        for (int p = 1; p <= hotItems.Count(); p++)
                        {
                            message.AddGlobalVariable("I" + p + "IMG", hotItems[p - 1].GetNormalImageUrl());
                            message.AddGlobalVariable("I" + p, hotItems[p - 1].name);
                            message.AddGlobalVariable("B" + p, hotItems[p - 1].brandName);
                            if (!brandNames.Contains(hotItems[p - 1].brandName))
                                brandNames.Add(hotItems[p - 1].brandName);
                            message.AddGlobalVariable("R" + p, hotItems[p - 1].retailer);
                            message.AddGlobalVariable("P" + p, string.Format("{0:C}",hotItems[p - 1].price));
                            message.AddGlobalVariable("I" + p + "URL", "http://startcult.com/product.html?prodId=" + hotItems[p - 1].id + "&colorId=" + hotItems[p - 1].colors[0].canonical[0]
                                + "&catId=" + hotItems[p - 1].categories[0] + "&user=CultCollection&url=http://startcult.com/images/100x100_identity_.png");
                        }
                        //set header params for top 3 brands
                        message.AddGlobalVariable("EB1", brandNames[0]);
                    }
                    else if (t.templateName == "Weekly Content Email")
                    {
                        //get top looks - fill in the header param
                        //get look details and fill it in
                        for (int l = 1; l <= topLooks.Count; l++)
                        {
                            Look look = topLooks[l - 1];
                            if (look.id > 0)
                            {
                                message.AddGlobalVariable("L" + l + "URL", "http://startcult.com/look.html?lookId=" + look.id + "&userName=CultCollection&url=http://startcult.com/images/100x100_identity_.png&utm_source=email&utm_medium=WeeklyDigest&utm_term=consumption");
                                message.AddGlobalVariable("L" + l + "IMG", "http://startcult.com/images/looks/" + look.id + ".jpg");
                                message.AddGlobalVariable("U" + l, look.creator.userName);
                                message.AddGlobalVariable("S" + l, (look.upVote + look.restyleCount).ToString());
                                message.AddGlobalVariable("L" + l + "DESC", look.title);
                            }
                        }
                        // get personal top 2 looks and replace internal html

                        for (int tt = 1; tt <= featuredTags.Count; tt++)
                        {
                            Tag tag = featuredTags[tt-1];
                            if (tag.id > 0)
                            {
                                message.AddGlobalVariable("T" + tt + "URL", "http://www.startcult.com/tag.html?tagId=" + tag.id);
                                message.AddGlobalVariable("T" + tt + "IMG", tag.imageUrl);
                                message.AddGlobalVariable("T" + tt, tag.name);
                            }
                        }

                    }

                    var result = api.SendMessage(message, t.templateName, null);
                    
                }
            }
        }
        static void Main(string[] args)
        {
            
            //SendRetentionEmail("Welcome Explore 2nd", "How to find best looks", 1);
            //SendRetentionEmail("Welcome Shop 3rd", "How to keep the look", 2);
            
            SendDigestEmail();
            SendRetentionEmail("iPad Day 3", "We're on iPhone too!", 3, "iPad");
            SendRetentionEmail("iPhone Day 3", "We're on iPad too!", 3, "iPho");
            //SendRetentionEmail("Welcome Share 5th", "Share your look and start your cult", 4);

            //SendRetentionEmail("Featured Fashionista", "Cult Collection's Featured Fashionistas", 7);
            
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
