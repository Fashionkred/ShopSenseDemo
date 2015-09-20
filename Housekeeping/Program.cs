using System;
using System.Data.SqlClient;
using System.IO;
using System.Net;
using System.Runtime.Serialization.Json;

namespace Housekeeping
{
    internal class Program
    {
        public const string db = "Data Source=startcult.com;Initial Catalog=Fashionkred;Persist Security Info=True;User ID=admin;Password=Fk711101";

        public const string everyday = "https://s3-us-west-2.amazonaws.com/fkconfigs/brands_everyday.csv";

        public const string youth = "https://s3-us-west-2.amazonaws.com/fkconfigs/brands_youth.csv";

        public const string grownUp = "https://s3-us-west-2.amazonaws.com/fkconfigs/brands_grownup.csv";

        public const string runway = "https://s3-us-west-2.amazonaws.com/fkconfigs/brands_runway.csv";

        public const string boutique = "https://s3-us-west-2.amazonaws.com/fkconfigs/brands_boutique.csv";

        public const string shoes = "https://s3-us-west-2.amazonaws.com/fkconfigs/brands_shoes.csv";

        public const string ENTERTAINMENT = "https://s3-us-west-2.amazonaws.com/fkconfigs/themes_entertainment.csv";

        public const string HOWTOWEAR = "https://s3-us-west-2.amazonaws.com/fkconfigs/themes_howtowear.csv";

        public const string STYLEICONS = "https://s3-us-west-2.amazonaws.com/fkconfigs/themes_styleicons.csv";

        public const string DESIGNERS = "https://s3-us-west-2.amazonaws.com/fkconfigs/themes_designers.csv";

        public const string VIBES = "https://s3-us-west-2.amazonaws.com/fkconfigs/themes_vibes.csv";

        public const string SITUATIONS = "https://s3-us-west-2.amazonaws.com/fkconfigs/themes_situations.csv";

        public const string editorialTagFile = "https://s3-us-west-2.amazonaws.com/fkconfigs/editorialTags.json";

        public const string tagDescriptionFile = "https://s3-us-west-2.amazonaws.com/fkconfigs/themeDescriptions.json";

        public const string tagTypeDescriptionFile = "https://s3-us-west-2.amazonaws.com/fkconfigs/themeCategories.json";

        public static void GenTopBrands()
        {
            SqlConnection myConnection = new SqlConnection(db);
            string query = "EXEC [stp_SS_GenTopBrands]";
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

        public static string GetTagFile(string fileName)
        {
            string result;
            using (WebClient client = new WebClient())
            {
                try
                {
                    using (Stream stream = client.OpenRead(fileName))
                    {
                        using (StreamReader reader = new StreamReader(stream))
                        {
                            result = reader.ReadToEnd();
                            return result;
                        }
                    }
                }
                catch (WebException ex)
                {
                    if (ex.Response is HttpWebResponse)
                    {
                        if ((ex.Response as HttpWebResponse).StatusCode == HttpStatusCode.NotFound)
                        {
                            result = null;
                            return result;
                        }
                    }
                }
            }
            result = null;
            return result;
        }

        public static void UpdateFeaturedBrands(string brandFile, int type)
        {
            string brands = Program.GetTagFile(brandFile);
            string query = string.Concat(new object[]
			{
				"EXEC [stp_SS_UpdateFeaturedBrands] @brands=N'",
				brands.Replace("'", "''"),
				"', @type=",
				type
			});
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

        public static void UpdateFeaturedTags(string tagFile, int type)
        {
            string tags = Program.GetTagFile(tagFile);
            string query = string.Concat(new object[]
			{
				"EXEC [stp_SS_UpdateFeaturedTags] @tags=N'",
				tags.Replace("'", "''"),
				"', @type=",
				type
			});
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

        public static void DeleteOldProducts()
        {
            SqlConnection myConnection = new SqlConnection(db);
            string query = "EXEC [stp_SS_DelOldProducts]";
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

        public static void BackupDb()
        {
            SqlConnection myConnection = new SqlConnection(db);
            string query = "EXEC [stp_SS_BackupDb]";
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

        public static void UpdateFeaturedBrands()
        {
            Program.UpdateFeaturedBrands(everyday, 1);
            Program.UpdateFeaturedBrands(youth, 2);
            Program.UpdateFeaturedBrands(grownUp, 3);
            Program.UpdateFeaturedBrands(runway, 4);
            Program.UpdateFeaturedBrands(boutique, 5);
            Program.UpdateFeaturedBrands(shoes, 6);
        }

        public static void UpdateTagDescription()
        {
            TagDescriptions tags = new TagDescriptions();
            using (WebClient client = new WebClient())
            {
                try
                {
                    using (Stream stream = client.OpenRead(tagDescriptionFile))
                    {
                        DataContractJsonSerializer ser = new DataContractJsonSerializer(typeof(TagDescriptions));
                        tags = (TagDescriptions)ser.ReadObject(stream);
                    }
                }
                catch (WebException ex)
                {
                }
            }
            foreach (TagDescription tag in tags.themes)
            {
                string query = string.Concat(new string[]
				{
					"EXEC [stp_SS_UpdateTagDescription] @name=N'",
					tag.name,
					"', @desc=N'",
					tag.description.Replace("'", "''"),
					"'"
				});
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

        public static void UpdateTagtype()
        {
            TagDescriptions tags = new TagDescriptions();
            using (WebClient client = new WebClient())
            {
                try
                {
                    using (Stream stream = client.OpenRead(tagTypeDescriptionFile))
                    {
                        DataContractJsonSerializer ser = new DataContractJsonSerializer(typeof(TagDescriptions));
                        tags = (TagDescriptions)ser.ReadObject(stream);
                    }
                }
                catch (WebException ex)
                {
                }
            }
            int position = 1;
            foreach (TagDescription tag in tags.themes)
            {
                string query = string.Concat(new object[]
				{
					"EXEC [stp_SS_UpdateTagType] @name=N'",
					tag.name,
					"', @desc=N'",
					tag.description.Replace("'", "''"),
					"', @position=",
					position++
				});
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

        public static void UpdateFeaturedTags()
        {
            Program.UpdateFeaturedTags(HOWTOWEAR, 1);
            Program.UpdateFeaturedTags(STYLEICONS, 2);
            Program.UpdateFeaturedTags(DESIGNERS, 3);
            Program.UpdateFeaturedTags(VIBES, 4);
            Program.UpdateFeaturedTags(SITUATIONS, 5);
            Program.UpdateFeaturedTags(ENTERTAINMENT, 6);
        }

        public static void UpdateEditorialTags()
        {
            EditorialTags tags = new EditorialTags();
            using (WebClient client = new WebClient())
            {
                try
                {
                    using (Stream stream = client.OpenRead(editorialTagFile))
                    {
                        DataContractJsonSerializer ser = new DataContractJsonSerializer(typeof(EditorialTags));
                        tags = (EditorialTags)ser.ReadObject(stream);
                    }
                }
                catch (WebException ex_52)
                {
                }
            }
            foreach (EditorialTag tag in tags.tagList)
            {
                foreach (long lookId in tag.lookIds)
                {
                    string query = string.Concat(new object[]
					{
						"EXEC [stp_SS_UpdateLookTagMap] @tName=N'",
						tag.themeName,
						"', @lId=",
						lookId
					});
                    if (tag.action.ToLower() == "add")
                    {
                        query += ", @type=1";
                    }
                    else if (tag.action.ToLower() == "remove")
                    {
                        query += ", @type=0";
                    }
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

        private static void Main(string[] args)
        {
            Program.UpdateTagDescription();
            Program.UpdateEditorialTags();
            Program.UpdateTagtype();
            Program.UpdateFeaturedTags();
            Program.DeleteOldProducts();
            Program.UpdateFeaturedBrands();
            Program.GenTopBrands();
        }
    }
}
