using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data.SqlClient;

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
        UnSubsrcibe = 0x02
    }

    public class UserProfile
    {
        public long id { set; get; }

        public string name { set; get; }

        public int points { set; get; }

        public string pic { set; get; }

        public string email { set; get; }

        public Sex sex { set; get; }

        public string location { set; get; }

        public long facebookId { set; get; }

        public string accessToken { set; get; }

        public List<long> facebookFriends { set; get; }

        public string locale { set; get; }

        public UserFlags userFlags { set; get; }

        public string Referral { set; get; }

        public virtual bool IsPrivate
        {
            get
            {
                return (this.userFlags & UserFlags.PrivateSharing) == UserFlags.PrivateSharing;
            }
        }

        public int IsFollowing { set; get; }

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

        public UserProfile()
        {
            this.name = string.Empty;
            this.points = 0;
            this.pic = string.Empty;
            this.email = string.Empty;
            this.sex = Sex.Female;
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
            user.id = long.Parse(dr["Id"].ToString());
            user.pic = dr["Pic"].ToString();
            user.name = dr["Name"].ToString();
            user.points = int.Parse(dr["Points"].ToString());
            user.location = dr["Location"].ToString();
            user.facebookId = long.Parse(dr["FacebookId"].ToString());
            user.accessToken = dr["FbAccessToken"].ToString();
            
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

        public static UserProfile SaveOrUpdateUser(UserProfile user, string db)
        {
            UserProfile updatedUser = new UserProfile();
            updatedUser = user;

            int gender = 0;
            if (user.sex == Sex.Male)
                gender = 1;

            string fbFriends = "<Friends>";
            foreach (long id in user.facebookFriends)
            {
                fbFriends += "<Friend Id=\"" + id + "\" />";
            }
            fbFriends += "</Friends>";

            string query = "EXEC [stp_SS_SaveUser] @pic=N'" + user.pic + "', @name=N'" + user.name.Replace("'", "\"") + "', @sex=" + gender + 
                                                ", @email=N'" + user.email + "', @location=N'" + user.location.Replace("'", "\"") +
                                  "', @facebookId=" + user.facebookId + ", @locale='" + user.locale + "', @fbFriends='" + fbFriends + "'"+
                                  ",@flags=" + (int)user.userFlags+",@token='"+user.accessToken +"'" +",@referral=N'" + user.Referral + "'";
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

            string query = "EXEC [stp_SS_GetTopLoves] @UId=" + userId + ", @PId1=" + look.products[0].id + ", @PId2=" + look.products[1].id +",@RId=" + retailerId;
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
                        if(dr != null)
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

        public static Dictionary<string,IList<Product>> GetLovesByCategory(long userId, string cat1, string cat2, long retailerId, string db, string cat3=null)
        {
            Dictionary<string, IList<Product>> favorites = new Dictionary<string, IList<Product>>();

            string query = "EXEC [stp_SS_GetLovesByCategory] @UId=" + userId + ", @categoryId1=N'" + cat1 + "', @categoryId2=N'" + cat2 + "', @retailerId="+retailerId + ",@categoryId3=N'"+ cat3 +"'";
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
                        while(dr.Read())
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
            bool isFriend = true;

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
    }

    
    
}
