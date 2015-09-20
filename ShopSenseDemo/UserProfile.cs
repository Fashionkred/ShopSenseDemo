using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data.SqlClient;
using System.Runtime.Serialization;
using System.Data;

namespace ShopSenseDemo
{
    public enum Sex
    {
        Female = 0,
        Male
    }
    public enum UserFlags
    {
        None = 0x00,
        PrivateSharing = 0x01,
        UnSubsrcibe = 0x02,
        Stylist = 0x04,
        FeaturedUser = 0x08,
        Whitelisted = 0x12
    }

    [DataContract]
    public class LightUser
    {
         
        public long userId { set; get; }

        public string userName { set; get; }

        public string name { set; get; }

        public string pic { set; get; }

        public string activityType { set; get; }

        public string activityTime { set; get; }

        public static LightUser GetUserFromSqlReader(SqlDataReader dr)
        {
            LightUser user = new LightUser();
            user.userId = long.Parse(dr["Id"].ToString());
            user.pic = dr["pic"].ToString();
            user.userName = dr["UserName"].ToString().Replace("''", "'");
            user.activityType = dr["type"].ToString();
            user.activityTime = dr["time"].ToString();
            if (!string.IsNullOrEmpty(dr["Name"].ToString()))
            {
                user.name = dr["Name"].ToString().Replace("''", "'");
            }

            return user;
        }
    }


    [DataContract]
    public class UserProfile
    {
        public long userId { set; get; }

        public string userName { set; get; }

        public string name { set; get; }

        public int points { set; get; }

        public string pic { set; get; }

        public string emailId { set; get; }

        public Sex gender { set; get; }

        public string location { set; get; }

        public long facebookId { set; get; }

        public string accessToken { set; get; }

        public List<long> facebookFriends { set; get; }

        public string locale { set; get; }

        public UserFlags userFlags { set; get; }

        public string Referral { set; get; }

        public string password { set; get; }

        public string bio { set; get; }

        public string url { set; get; }

        public string fbPage { set; get; }

        public string twitterHandle { set; get; }

        public string PinterestHandle { set; get; }

        public string TumblrHandle { set; get; }

        public string InstagramHandle { set; get; }

        public virtual bool IsPrivate
        {
            get
            {
                return (this.userFlags & UserFlags.PrivateSharing) == UserFlags.PrivateSharing;
            }
        }

        public virtual bool IsStylist
        {
            get
            {
                return (this.userFlags & UserFlags.Stylist) == UserFlags.Stylist;
            }
        }

        public bool IsNew { set; get; }

        public int IsFollowing { set; get; }

        public string userAgent { set; get; }

        public string BigPic()
        {
            if (pic.IndexOf("width=50") > 0)
            {
                return pic.Substring(0, pic.IndexOf("width=50")) + "width=160&height=160";
            }
            else
                return pic;
        }

        public string AvatarPic()
        {
            return pic + "&height=50";
        }

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

        public UserProfile()
        {
            this.name = string.Empty;
            this.points = 0;
            this.pic = string.Empty;
            this.emailId = string.Empty;
            this.gender = Sex.Female;
            this.location = "Unknown";
            this.facebookId = -1;
            this.accessToken = string.Empty;
            this.facebookFriends = new List<long>();
            this.locale = "en_US";
            this.userFlags = UserFlags.None;
            this.accessToken = string.Empty;
        }

        public static UserProfile GetUserFromSqlReader(SqlDataReader dr)
        {
            UserProfile user = new UserProfile();
            user.userId = long.Parse(dr["Id"].ToString());
            user.pic = dr["Pic"].ToString();
            user.userName = dr["UserName"].ToString().Replace("''", "'");
            if (!string.IsNullOrEmpty(dr["Name"].ToString()))
            {
                user.name = dr["Name"].ToString().Replace("''", "'");
            }

            //if (!string.IsNullOrEmpty(dr["Points"].ToString()))
            //{
            //    user.points = int.Parse(dr["Points"].ToString());
            //}
            if (!string.IsNullOrEmpty(dr["Location"].ToString()))
            {
                user.location = dr["Location"].ToString();
            }
            if (!string.IsNullOrEmpty(dr["FacebookId"].ToString()))
            {
                user.facebookId = long.Parse(dr["FacebookId"].ToString());
            }
            if (!string.IsNullOrEmpty(dr["FbAccessToken"].ToString()))
            {
                user.accessToken = dr["FbAccessToken"].ToString();
            }
            if (ColumnExists(dr, "following") && !string.IsNullOrEmpty(dr["following"].ToString()))
            {
                user.IsFollowing = int.Parse(dr["following"].ToString());
            }
            if (ColumnExists(dr, "Email") && !string.IsNullOrEmpty(dr["Email"].ToString()))
            {
                user.emailId = dr["Email"].ToString();
            }
            if (ColumnExists(dr, "Bio") && !string.IsNullOrEmpty(dr["Bio"].ToString()))
            {
                user.bio = dr["Bio"].ToString();
            }
            if (ColumnExists(dr, "Url") && !string.IsNullOrEmpty(dr["Url"].ToString()))
            {
                user.url = dr["Url"].ToString();
            }
            if (ColumnExists(dr, "FbPage") && !string.IsNullOrEmpty(dr["FbPage"].ToString()))
            {
                user.fbPage = dr["FbPage"].ToString();
            }
            if (ColumnExists(dr, "TwName") && !string.IsNullOrEmpty(dr["TwName"].ToString()))
            {
                user.twitterHandle = dr["TwName"].ToString();
            }
            if (ColumnExists(dr, "PinName") && !string.IsNullOrEmpty(dr["PinName"].ToString()))
            {
                user.PinterestHandle = dr["PinName"].ToString();
            }
            if (ColumnExists(dr, "TumName") && !string.IsNullOrEmpty(dr["TumName"].ToString()))
            {
                user.TumblrHandle = dr["TumName"].ToString();
            }
            if (ColumnExists(dr, "InstaName") && !string.IsNullOrEmpty(dr["InstaName"].ToString()))
            {
                user.InstagramHandle = dr["InstaName"].ToString();
            }
            if (ColumnExists(dr, "Createtime") && !string.IsNullOrEmpty(dr["Createtime"].ToString()))
            {
                DateTime createTime = DateTime.Parse(dr["Createtime"].ToString());
                if (DateTime.UtcNow.Subtract(createTime).TotalMinutes > 1)
                    user.IsNew = false;
                else
                    user.IsNew = true;
            }
            if (ColumnExists(dr, "Flags") && !string.IsNullOrEmpty(dr["Flags"].ToString()))
            {
                user.userFlags = (UserFlags) Enum.Parse(typeof(UserFlags), dr["Flags"].ToString());
            }
            return user;
        }

        public static UserProfile GetUserProfileById(long id, string db)
        {
            UserProfile user = new UserProfile();

            string query = "EXEC [stp_SS_GetUser] @id=" + id;
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
                        user = GetUserFromSqlReader(dr);
                    }
                }
            }
            finally
            {
                myConnection.Close();
            }
            return user;
        }

        public static List<long> GetUserFollowersFacebookIds(long id, string db)
        {
            List<long> fbFriendIds = new List<long>();

            string query = "EXEC [stp_SS_GetUserFollowers] @id=" + id;
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
                        long fId = long.Parse(dr["FacebookId"].ToString());
                        fbFriendIds.Add(fId);
                    }
                }
            }
            finally
            {
                myConnection.Close();
            }
            return fbFriendIds;
        }

        public static List<UserProfile> GetTopStylists(long userId, int noOfStylists, string db)
        {
            string query = "EXEC [stp_SS_GetPopularStylists] @userId=" + userId + ",@noOfStylist=" + noOfStylists;

            SqlConnection myConnection = new SqlConnection(db);
            List<UserProfile> stylists = new List<UserProfile>();
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
                        UserProfile user = UserProfile.GetUserFromSqlReader(dr);
                        stylists.Add(user);
                    }
                }
            }
            finally
            {
                myConnection.Close();
            }

            return stylists;
        }
        public static UserProfile SaveOrUpdateUser(UserProfile user, string db)
        {
            UserProfile updatedUser = new UserProfile();
            updatedUser = user;

            int gender = 0;
            if (user.gender == Sex.Male)
                gender = 1;

            string fbFriends = "<Friends>";
            foreach (long id in user.facebookFriends)
            {
                fbFriends += "<Friend Id=\"" + id + "\" />";
            }
            fbFriends += "</Friends>";

            string query = "EXEC [stp_SS_SaveUser] @pic=N'" + user.pic + "', @name=N'" + user.name.Replace("'", "''") + "', @sex=" + gender +
                                                ", @email=N'" + user.emailId + "', @location=N'" + user.location.Replace("'", "\"") +
                                  "', @facebookId=" + user.facebookId + ", @locale='" + user.locale + "', @fbFriends='" + fbFriends + "'" +
                                  ",@flags=" + (int)user.userFlags + ",@token='" + user.accessToken + "'" + ",@referral=N'" + user.Referral + "'" +
                                  ", @password=N'" + user.password + "', @userAgent=N'" + user.userAgent + "'";
            if (!string.IsNullOrEmpty(user.userName))
            {
                query += ", @userName=N'" + user.userName.Replace("'", "''") + "'";
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
                        updatedUser = GetUserFromSqlReader(dr);
                    }
                }
            }
            finally
            {
                myConnection.Close();
            }

            return updatedUser;
        }

        public static UserProfile UpdateUserInfo(UserProfile user, string db)
        {
            UserProfile updatedUser = new UserProfile();
            //updatedUser = user;

            int gender = 0;
            if (user.gender == Sex.Male)
                gender = 1;

            string query = "EXEC [stp_SS_UpdateUserInfo] @id=" + user.userId +  ", @pic=N'" + user.pic + "', @name=N'" + user.name.Replace("'", "''") + "', @sex=" + gender +
                                                ", @email=N'" + user.emailId + "', @location=N'" + user.location.Replace("'", "\"") +
                                                "', @userName=N'" + user.userName + "', @bio=N'" + user.bio + "', @url=N'" + user.url + "', @fbPage='" + user.fbPage +
                                                "', @TwName='" + user.twitterHandle + "', @PinName='" + user.PinterestHandle + "', @TumName='" + user.TumblrHandle + "', @InstaName='"+user.InstagramHandle +"'";

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
                        updatedUser = GetUserFromSqlReader(dr);
                    }
                }
            }
            finally
            {
                myConnection.Close();
            }

            return updatedUser;
        }

        public static IList<Product> GetLovesByUserId(long userId, Look look, long retailerId, string db, out  bool P1Love, out bool P2Love, out bool P3Love)
        {
            IList<Product> loves = new List<Product>();

            string query = "EXEC [stp_SS_GetTopLoves] @UId=" + userId + ", @PId1=" + look.products[0].id + ", @PId2=" + look.products[1].id + ",@RId=" + retailerId;
            if (look.products.Count == 3)
            {
                query += (", @PId3=" + look.products[2].id);
            }
            SqlConnection myConnection = new SqlConnection(db);

            P1Love = false;
            P2Love = false;
            P3Love = false;

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
                        Product p = Product.GetProductFromSqlDataReader(dr);
                        loves.Add(p);
                    }

                    dr.NextResult();
                    while (dr.Read())
                    {
                        if (dr != null)
                            P1Love = true;
                    }

                    dr.NextResult();

                    while (dr.Read())
                    {
                        if (dr != null)
                            P2Love = true;
                    }
                    dr.NextResult();

                    while (dr.Read())
                    {
                        if (dr != null)
                            P3Love = true;
                    }
                }
            }
            finally
            {
                myConnection.Close();
            }

            return loves;
        }

        public static IList<Product> GetLovesByUserId(long userId, Product product, long retailerId, string db)
        {
            IList<Product> loves = new List<Product>();

            string query = "EXEC [stp_SS_GetTopLoves] @UId=" + userId + ", @PId1=" + product.id + ", @PId2=" + product.id + ",@RId=" + retailerId;
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
                        Product p = Product.GetProductFromSqlDataReader(dr);
                        loves.Add(p);
                    }
                }
            }
            finally
            {
                myConnection.Close();
            }

            return loves;
        }

        public static Dictionary<string, IList<Product>> GetLovesByCategory(long userId, string cat1, string cat2, long retailerId, string db, string cat3 = null)
        {
            Dictionary<string, IList<Product>> favorites = new Dictionary<string, IList<Product>>();

            string query = "EXEC [stp_SS_GetLovesByCategory] @UId=" + userId + ", @categoryId1=N'" + cat1 + "', @categoryId2=N'" + cat2 + "', @retailerId=" + retailerId + ",@categoryId3=N'" + cat3 + "'";
            SqlConnection myConnection = new SqlConnection(db);


            try
            {
                myConnection.Open();
                using (SqlDataAdapter adp = new SqlDataAdapter(query, myConnection))
                {
                    SqlCommand cmd = adp.SelectCommand;
                    cmd.CommandTimeout = 300000;
                    System.Data.SqlClient.SqlDataReader dr = cmd.ExecuteReader();

                    int i = 1;
                    do
                    {

                        IList<Product> loves = new List<Product>();

                        while (dr.Read())
                        {
                            Product p = Product.GetProductFromSqlDataReader(dr);
                            loves.Add(p);
                        }

                        if (i == 1)
                            favorites.Add(cat1, loves);
                        else if (i == 2)
                            favorites.Add(cat2, loves);
                        else
                            favorites.Add(cat3, loves);

                        i++;

                    } while (dr.NextResult());
                }
            }
            finally
            {
                myConnection.Close();
            }

            return favorites;
        }

        public static Dictionary<string, IList<Product>> GetLovesByContest(long userId, int contestId, string db)
        {
            Dictionary<string, IList<Product>> favorites = new Dictionary<string, IList<Product>>();

            string query = "EXEC [stp_SS_GetLovesByContest] @UId=" + userId + ", @contestId=" + contestId;
            SqlConnection myConnection = new SqlConnection(db);


            try
            {
                myConnection.Open();
                using (SqlDataAdapter adp = new SqlDataAdapter(query, myConnection))
                {
                    SqlCommand cmd = adp.SelectCommand;
                    cmd.CommandTimeout = 300000;
                    System.Data.SqlClient.SqlDataReader dr = cmd.ExecuteReader();

                    do
                    {

                        IList<Product> loves = new List<Product>();

                        while (dr.Read())
                        {
                            Product p = Product.GetProductFromSqlDataReader(dr);
                            loves.Add(p);
                        }

                        dr.NextResult();
                        while (dr.Read())
                        {
                            string categoryName = dr["categoryName"].ToString();
                            if (favorites.ContainsKey(categoryName))
                            {
                                favorites[categoryName] = loves;
                            }
                            else
                            {
                                favorites.Add(categoryName, loves);
                            }
                        }


                    } while (dr.NextResult());
                }
            }
            finally
            {
                myConnection.Close();
            }

            return favorites;
        }

        /// <summary>
        /// Check if userId2 is a friend of userId1
        /// </summary>
        /// <param name="userId1"></param>
        /// <param name="userId2"></param>
        /// <returns></returns>
        public static bool IsFriend(long userId1, long userId2)
        {
            bool isFriend = false;

            //userId2 is friend of userId1 if userId2 belongs in userId1's following table

            return isFriend;
        }

        public static bool IsFollower(long uId, long followerId, string db)
        {
            bool isFollower = false;

            string query = "EXEC [stp_SS_IsFollower] @uid=" + uId + ", @fid=" + followerId;
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
                        isFollower = true;
                    }
                }
            }
            finally
            {
                myConnection.Close();
            }
            return isFollower;
        }

        //update cookie if this function is successful - so that users doesn't have to log in again for 15 days
        public static UserProfile LogInUser(string email, string hash32Password, string db, string userAgent="iPad")
        {
            UserProfile user = null;
            string query = "EXEC [stp_SS_Login] @email = N'" + email + "', "
                                            + " @password = N'" + hash32Password + "', @userAgent= N'" + userAgent + "'";
            SqlConnection myConnection = new SqlConnection(db);
            try
            {
                myConnection.Open();
                using (SqlDataAdapter adp = new SqlDataAdapter(query, myConnection))
                {
                    SqlCommand cmd = adp.SelectCommand;
                    cmd.CommandTimeout = 300000;
                    SqlDataReader dr = cmd.ExecuteReader();
                    while (dr.Read())
                    {
                        user = UserProfile.GetUserFromSqlReader(dr);
                    }
                }
            }
            finally
            {
                myConnection.Close();
            }

            return user;
        }

        public static UserProfile LogInViaFb(long facebookId, string db,string userAgent = "iPad")
        {
            UserProfile user = null;
            string query = "EXEC [stp_SS_LoginByFb] @facebookId =" + facebookId + ",@userAgent=N'" + userAgent + "'";
            SqlConnection myConnection = new SqlConnection(db);
            try
            {
                myConnection.Open();
                using (SqlDataAdapter adp = new SqlDataAdapter(query, myConnection))
                {
                    SqlCommand cmd = adp.SelectCommand;
                    cmd.CommandTimeout = 300000;
                    SqlDataReader dr = cmd.ExecuteReader();
                    while (dr.Read())
                    {
                        user = UserProfile.GetUserFromSqlReader(dr);
                    }
                }
            }
            finally
            {
                myConnection.Close();
            }

            return user;
        }

        public static Dictionary<string, List<Product>> GetClosetProducts(long userId,  string db, long viewerId= 0)
        {
            return GetClosetProductsv2(userId, db, viewerId, 5);
        }

        public static Dictionary<string, List<Product>> GetClosetProductsv2(long userId, string db, long viewerId = 0, long items =5)
        {
            Dictionary<string, List<Product>> closetByCategories = new Dictionary<string, List<Product>>();
            string query = "EXEC [stp_SS_GetClosetProducts] @userId=" + userId;

            if (viewerId != 0)
                query += ",@viewerId=" + viewerId;

            if(items !=  0)
                query += ",@noOfItems=" + items;

            SqlConnection myConnection = new SqlConnection(db);
            try
            {
                myConnection.Open();
                using (SqlDataAdapter adp = new SqlDataAdapter(query, myConnection))
                {
                    SqlCommand cmd = adp.SelectCommand;
                    cmd.CommandTimeout = 300000;
                    System.Data.SqlClient.SqlDataReader dr = cmd.ExecuteReader();

                    do
                    {
                        string type = string.Empty;
                        string total = string.Empty; ;
                        List<Product> closetItems = new List<Product>();

                        while (dr.Read())
                        {
                            type = dr["type"].ToString();
                            total = dr["total"].ToString();
                            type = type + "," + total;
                            closetItems.Add(Product.GetProductFromSqlDataReader(dr));
                        }
                        if (!string.IsNullOrEmpty(type))
                        {
                            if (!closetByCategories.ContainsKey(type))
                                closetByCategories.Add(type, closetItems);
                            else
                                closetByCategories[type] = closetItems;
                        }

                    } while (dr.NextResult());

                }
            }
            finally
            {
                myConnection.Close();
            }
            return closetByCategories;
        }

        public static Dictionary<string, List<Product>> GetClosetProductsByMetaCat(long userId,string metaCat,  string db, int offset = 1, int limit = 20, long viewerId = 0, bool isv2 = false)
        {
            Dictionary<string, List<Product>> closetByCategories = new Dictionary<string, List<Product>>();
            string query = "EXEC [stp_SS_GetClosetProductsByMetaCat]";
            if(isv2)
                query = "EXEC [stp_SS_GetClosetProductsByMetaCatv2]";
            
            
            query += " @userId=" + userId + ",@metaCat='" + metaCat + "',@offset=" + offset + ",@limit=" + limit;
            if (viewerId != 0)
                query += ",@viewerId=" + viewerId;
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
                        string type = dr["type"].ToString();
                        if (!closetByCategories.ContainsKey(type))
                        {
                            List<Product> closetItems = new List<Product>();
                            closetItems.Add(Product.GetProductFromSqlDataReader(dr));
                            closetByCategories.Add(type, closetItems);

                        }
                            
                        else
                            closetByCategories[type].Add(Product.GetProductFromSqlDataReader(dr));
                    }
                }
            }
            finally
            {
                myConnection.Close();
            }
            return closetByCategories;
        }

        public static List<Product> GetClosetProductsByDate(long userId, string db, int offset = 1, int limit = 20)
        {
            List<Product> closetItems = new List<Product>(); 
            string query = "EXEC [stp_SS_GetClosetProductsByDate] @userId=" + userId + ",@offset=" + offset + ",@limit=" + limit;
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
                        closetItems.Add(Product.GetProductFromSqlDataReader(dr));
                    }
                }
            }
            finally
            {
                myConnection.Close();
            }
            return closetItems;
        }

        public static List<UserProfile> GetTaggedPopularStylists(long tagId, long uId, string db)
        {
            List<UserProfile> users = new List<UserProfile>();

            string query = "EXEC [stp_SS_GetTaggedPopularStylists]  @tagId=" + tagId + ",@userId=" + uId;
            
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
                        UserProfile user = new UserProfile();
                        user.userId = long.Parse(dr["Id"].ToString());
                        user.pic = dr["Pic"].ToString();
                        user.name = dr["Name"].ToString();
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

        public static bool SubscribeUser(long userId, long subscriberId, bool isSubscribe, string db)
        {
            bool isSuccess = false;

            int subscribe = isSubscribe == true ? 1 : 0;

            string query = "EXEC [stp_SS_Subscribe] @uid=" + userId + ", @sid=" + subscriberId + ",@subscribe=" + subscribe;
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

        public static bool SubscribeTags(long userId, string tags, bool isSubscribe, string db)
        {
            bool isSuccess = false;

            int subscribe = isSubscribe == true ? 1 : 0;

            string query = "EXEC [stp_SS_SubscribeTags] @uid=" + userId + ", @tags=N'" + tags.Replace("'", "''") + "',@subscribe=" + subscribe;
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

        public static bool SubscribeFbUsers(long userId, string subscriberFbIds, bool isSubscribe, string db)
        {
            bool isSuccess = false;

            int subscribe = isSubscribe == true ? 1 : 0;

            string query = "EXEC [stp_SS_SubscribeFriends] @uid=" + userId + ", @friendids='" + subscriberFbIds + "',@subscribe=" + subscribe;
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

        
        public static Dictionary<string, List<object>> GetUserProfileInfo(long userId, string view, string db, int offset = 1, int limit = 20, long viewerId=0, int sortType= 0)
        {
            Dictionary<string, List<object>> profileInfo = new Dictionary<string, List<object>>();

            string query = "EXEC [stp_SS_GetUserProfileInfo] @id=" + userId +", @view='" + view + "', @offset=" + offset + ",@limit=" + limit;

            if (viewerId != 0)
                query += ",@viewerId=" + viewerId;
            if (sortType != 0)
                query += ",@sortType=" + sortType;


            SqlConnection myConnection = new SqlConnection(db);
            try
            {
                myConnection.Open();
                using (SqlDataAdapter adp = new SqlDataAdapter(query, myConnection))
                {
                    SqlCommand cmd = adp.SelectCommand;
                    cmd.CommandTimeout = 300000;
                    System.Data.SqlClient.SqlDataReader dr = cmd.ExecuteReader();

                    int likeCount=0 , lookCount = 0;

                    List<object> count = new List<object>();
                    while (dr.Read())
                    {
                        lookCount = int.Parse(dr["LookCount"].ToString());
                        count.Add("looks," + lookCount);
                    }
                    dr.NextResult();
                    while (dr.Read())
                    {
                        int garmentCount = int.Parse(dr["ItemCount"].ToString());
                        count.Add("garments," + garmentCount);
                    }
                    dr.NextResult();
                    while (dr.Read())
                    {
                        likeCount = int.Parse(dr["LikeCount"].ToString());
                        count.Add("hearts," + likeCount);
                    }
                    dr.NextResult();
                    while (dr.Read())
                    {
                        int followerCount = int.Parse(dr["FollowerCount"].ToString());
                        count.Add("follower," + followerCount);
                    }
                    dr.NextResult();
                    while (dr.Read())
                    {
                        int followingCount = int.Parse(dr["FollowingCount"].ToString());
                        count.Add("following," + followingCount);
                    }
                    dr.NextResult();
                    while (dr.Read())
                    {
                        int isFollowing = int.Parse(dr["following"].ToString());
                        count.Add("followstatus," + isFollowing);
                    }
                    dr.NextResult();
                    while (dr.Read())
                    {
                        string followerPic = dr["LastFollowerPic"].ToString();
                        count.Add("lastFollowerPic," + followerPic);
                    }
                    dr.NextResult();
                    while (dr.Read())
                    {
                        string followingPic = dr["LastFollowingPic"].ToString();
                        count.Add("lastFollowingPic," + followingPic);
                    }

                    dr.NextResult();
                    profileInfo.Add("Counts", count);
                    switch (view)
                    {
                        case "profile":
                            List<object> users = new List<object>();
                            profileInfo.Add("Users", users);
                            while (dr.Read())
                            {
                                UserProfile updatedUser = GetUserFromSqlReader(dr);
                                users.Add(updatedUser);
                            }
                            break;
                        case "looks":
                            List<object> looks = new List<object>();
                            profileInfo.Add("Looks", looks);
                            while (dr.Read())
                            {
                                string lookId = dr["Id"].ToString();
                                string isLoved = dr["love"].ToString();
                                looks.Add(lookId + "," + isLoved);
                            }
                            break;
                        case "hearts":
                            List<object> hearts = new List<object>();
                            profileInfo.Add("Hearts", hearts);
                            while (dr.Read())
                            {
                                string lookId = dr["Id"].ToString();
                                string isLoved = dr["love"].ToString();
                                hearts.Add(lookId + "," +  isLoved);
                            }
                            break;
                        case "lookbook":
                            List<object> lookbooks = new List<object>();
                            profileInfo.Add("lookbook", lookbooks);
                            while (dr.Read())
                            {
                                string lookId = dr["Id"].ToString();
                                //string isLoved = dr["love"].ToString();
                                string isCreated = dr["created"].ToString();
                                string rating = dr["OverallRating"].ToString();
                                lookbooks.Add(lookId + "," + isCreated + "," + rating);
                                
                            }
                            //count.Add("LookBookCount," + lookBookCount);
                            break;
                        case "items":
                            do
                            {
                                string type = string.Empty;
                                string total = string.Empty; ;
                                List<object> closetItems = new List<object>();

                                while (dr.Read())
                                {
                                    type = dr["type"].ToString();
                                    total = dr["total"].ToString();
                                    type = type + "," + total;
                                    closetItems.Add(Product.GetProductFromSqlDataReader(dr));
                                }
                                if (!string.IsNullOrEmpty(type))
                                {
                                    if (!profileInfo.ContainsKey(type))
                                        profileInfo.Add(type, closetItems);
                                    else
                                        profileInfo[type] = closetItems;
                                }

                            } while (dr.NextResult());
                   
                            break;
                        case "followers":
                            List<object> followers = new List<object>();
                            profileInfo.Add("followers", followers);
                            while (dr.Read())
                            {
                                UserProfile user = UserProfile.GetUserFromSqlReader(dr);
                                followers.Add(user);
                            }
                            break;
                        case "following":
                            List<object> followingUsers = new List<object>();
                            profileInfo.Add("followingUsers", followingUsers);
                            while (dr.Read())
                            {
                                UserProfile user = UserProfile.GetUserFromSqlReader(dr);
                                followingUsers.Add(user);
                            }

                            dr.NextResult();
                            List<object> followingTags = new List<object>();
                            profileInfo.Add("followingTags", followingTags);
                            while (dr.Read())
                            {
                                Tag tag = Tag.GetTagFromSqlReader(dr);
                                followingTags.Add(tag);
                            }

                            //dr.NextResult();
                            //List<object> followingBrands = new List<object>();
                            //profileInfo.Add("followingBrands", followingBrands);
                            //while (dr.Read())
                            //{
                            //    Brand brand = new Brand();
                            //    brand.name = dr["name"].ToString().Replace("\"", "'");
                            //    brand.id = long.Parse(dr["id"].ToString());
                            //    if (ColumnExists(dr, "following") && !string.IsNullOrEmpty(dr["following"].ToString()))
                            //    {
                            //        brand.IsFollowing = int.Parse(dr["following"].ToString());
                            //    }
                            //    followingBrands.Add(brand);
                            //}
                            break;
                    }
                }
            }
            finally
            {
                myConnection.Close();
            }

            return profileInfo;
        }

        public static Dictionary<string, List<object>> GetUserFollowedBrands(long userId, string db)
        {
            Dictionary<string, List<object>> profileInfo = new Dictionary<string, List<object>>();

            string query = "EXEC [stp_SS_GetFollowedBrands] @id=" + userId ;

            SqlConnection myConnection = new SqlConnection(db);
            try
            {
                myConnection.Open();
                using (SqlDataAdapter adp = new SqlDataAdapter(query, myConnection))
                {
                    SqlCommand cmd = adp.SelectCommand;
                    cmd.CommandTimeout = 300000;
                    System.Data.SqlClient.SqlDataReader dr = cmd.ExecuteReader();

                    int brandCount = 0;

                    List<object> count = new List<object>();
                    while (dr.Read())
                    {
                        brandCount = int.Parse(dr["BrandCount"].ToString());
                        count.Add("looks," + brandCount);
                    }
                    

                    dr.NextResult();
                    profileInfo.Add("Counts", count);
                   
                            
                    List<object> followingBrands = new List<object>();
                    profileInfo.Add("followingBrands", followingBrands);
                    while (dr.Read())
                    {
                        Brand brand = new Brand();
                        brand.name = dr["name"].ToString().Replace("\"", "'");
                        brand.id = long.Parse(dr["id"].ToString());
                        
                        followingBrands.Add(brand);
                    }
                            
                }
            }
            finally
            {
                myConnection.Close();
            }

            return profileInfo;
        }

        public static List<Tag> GetUserFollowedTags(long userId, string db, int offset = 1, int limit = 20, long viewerId = 0)
        {
            List<Tag> tags = new List<Tag>();

            string query = "EXEC [stp_SS_GetUserFollowingTags] @id=" + userId + ", @offset=" + offset + ",@limit=" + limit;

            if (viewerId != 0)
                query += ",@viewerId=" + viewerId;

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
                        Tag tag = Tag.GetTagFromSqlReader(dr);
                        tags.Add(tag);
                    }

                }
            }
            finally
            {
                myConnection.Close();
            }

            return tags;
        }

        public static List<UserProfile> GetUserFollowedUsers(long userId, string db, int offset = 1, int limit = 20, long viewerId = 0)
        {
            List<UserProfile> users = new List<UserProfile>();

            string query = "EXEC [stp_SS_GetUserFollowingUsers] @id=" + userId + ", @offset=" + offset + ",@limit=" + limit;

            if (viewerId != 0)
                query += ",@viewerId=" + viewerId;

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
                        UserProfile user = UserProfile.GetUserFromSqlReader(dr);
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

        public static bool IsEmailUnique(string emailId, string db)
        {
            bool isEmailUnique = true;

            string query = "EXEC [stp_SS_IsEmailUnique] @email='" + emailId + "'";
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
                        isEmailUnique = false;
                    }
                }
            }
            finally
            {
                myConnection.Close();
            }

            return isEmailUnique;
        }
        
        public static bool IsUserNameUnique(string userName, string db)
        {
            bool isUserNameUnique = true;

            string query = "EXEC [stp_SS_IsUserNameUnique] @userName='" + userName + "'";
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
                        isUserNameUnique = false;
                    }
                }
            }
            finally
            {
                myConnection.Close();
            }

            return isUserNameUnique;
        }

        public static bool ChangePassword(string userName, string oldPassword, string newPassword, string db)
        {
            bool isSuccessful = false;

            string query = "EXEC [stp_SS_ChangePassword] @userName='" + userName + "', @oldPassword=N'" + oldPassword + "', @newPassword=N'" + newPassword + "'";
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
                        isSuccessful = true;
                    }
                }
            }
            finally
            {
                myConnection.Close();
            }

            return isSuccessful;
        }

        /*public static bool UpdatePassword(long userId,string newPassword, string db)
        {
            bool isSuccessful = false;

            string query = "EXEC [stp_SS_ForgotPassword] @id=" + userId + ", @newPassword=N'" + newPassword + "'";
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
                        isSuccessful = true;
                    }
                }
            }
            finally
            {
                myConnection.Close();
            }

            return isSuccessful;
        }*/
        public static bool ForgotPassword(string emailId, string db,out string userName, out string newPwd)
        {
            bool isSuccessful = false;

            //new password
            string  newPassword = Guid.NewGuid().ToString().Substring(0,8);
            newPwd = newPassword;
            userName = string.Empty;

            string query = "EXEC [stp_SS_ForgotPassword] @emailId='" + emailId + "', @newPassword=N'" + newPassword + "'";
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
                        isSuccessful = true;
                        userName = dr["UserName"].ToString();
                    }
                }
            }
            finally
            {
                myConnection.Close();
            }

            return isSuccessful;
        }

        public static bool SaveDeviceToken(long userId, string deviceId, string token, string db)
        {
            bool isSuccess = false;

            string query = "EXEC [stp_SS_SaveNotificationDeviceInfo] @userid=" + userId + ", @deviceId='" + deviceId + "',@token='" + token + "'";
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

        public static bool SaveNotificationSeenTime(long userId, string db)
        {
            bool isSuccess = false;

            string query = "EXEC [stp_SS_SaveNotificationSeenTime] @userid=" + userId ;
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



        public static bool SubscribeBrands(long userId, string brands, bool isFollow, string db)
        {
            bool isSuccess = false;

            int subscribe = isFollow == true ? 1 : 0;

            string query = "EXEC [stp_SS_SubscribeBrands] @uid=" + userId + ", @brands=N'" + brands.Replace("'", "''") + "',@subscribe=" + subscribe;
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

        public static bool IsSubscribeBrand(long userId, long brandId,  string db)
        {
            bool isSuccess = false;

            string query = "EXEC [stp_SS_IsFollowedBrand] @userId=" + userId + ", @brandId=" + brandId;
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
                        string count = dr["BrandCount"].ToString();
                        if (count != "0")
                            isSuccess = true;
                    }
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
    
    

