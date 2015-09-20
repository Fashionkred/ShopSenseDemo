using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.Serialization;
using System.Data.SqlClient;
using System.Data;
using System.Net;
using System.IO;
using System.Runtime.Serialization.Json;

namespace ShopSenseDemo
{
    [DataContract]
    public sealed class TagType
    {

        [DataMember]
        public readonly String name;

        [DataMember]
        public String description { get; set; }
        private readonly int value;

        public static readonly TagType ENTERTAINMENT = new TagType(6, "ENTERTAINMENT");
        public static readonly TagType HOWTOWEAR = new TagType(1, "HOW TO WEAR IT");
        public static readonly TagType STYLEICONS = new TagType(2, "STYLE ICONS");
        public static readonly TagType DESIGNERS = new TagType(3, "DESIGNERS");
        public static readonly TagType TRENDING = new TagType(0, "TRENDING");
        public static readonly TagType SITUATIONS = new TagType(5, "SITUATIONS");
        public static readonly TagType VIBES = new TagType(4, "VIBES");
        
        private TagType(int value, String name)
        {
            this.name = name;
            this.value = value;
        }

        public override String ToString()
        {
            return name;
        }

        

    }

    [DataContract]
    public class Tag
    {

        [DataMember]
        public long id { get; set; }

        [DataMember]
        public string name { get; set; }

        [DataMember]
        public string imageUrl { get; set; }

        [DataMember]
        public string BigBannerUrl { get; set; }

        [DataMember]
        public string SwatchUrl { get; set; }

        [DataMember]
        public string description { get; set; }

        public int IsFollowing { get; set; }

        public int HasLooks { get; set; }

        public TagType type { get; set; }

        public bool IsEditorial { get; set; }

        public static string GetTags()
        {
            string tagFile = "https://s3-us-west-2.amazonaws.com/fkconfigs/themes_trending.csv";
            System.Net.ServicePointManager.Expect100Continue = false;
            using (WebClient client = new WebClient())
            {
                try
                {
                    using (Stream stream = client.OpenRead(tagFile))
                    {
                        using (StreamReader reader = new StreamReader(stream))
                        {
                            return reader.ReadToEnd();
                        }
                    }
                }
                catch (WebException ex)
                {
                    
                    if (ex.Response is HttpWebResponse)
                    {
                        if ((ex.Response as HttpWebResponse).StatusCode == HttpStatusCode.NotFound)
                        {
                            return null;
                        }
                    }
                    
                }
            }

            return null;
        }

        public static HomeTheme GetEditorialTags()
        {
            string tagFile = "https://s3-us-west-2.amazonaws.com/fkconfigs/homeThemes.json";
            System.Net.ServicePointManager.Expect100Continue = false;
            HomeTheme hpThemes = new HomeTheme();
            Schedule themeSchedule = new Schedule();
            using (WebClient client = new WebClient())
            {
                try
                {
                    using (Stream stream = client.OpenRead(tagFile))
                    {
                        DataContractJsonSerializer ser = new DataContractJsonSerializer(typeof(Schedule));
                        //stream.Position = 0;
                        themeSchedule = (Schedule)ser.ReadObject(stream);
                    }
                }
                catch (WebException ex)
                { }
                
            }

            foreach (HomeTheme theme in themeSchedule.schedule)
            {
                DateTime scheduleDate = DateTime.Parse(theme.date);
                if (scheduleDate < DateTime.UtcNow)
                {
                    hpThemes = theme;
                    
                }
                else
                    break;
            }
            return hpThemes;
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

        public static Tag GetTagFromSqlReader(SqlDataReader dr)
        {
            Tag tag = new Tag();

            tag.id = long.Parse(dr["Id"].ToString());
            tag.name = dr["Name"].ToString().Replace("''", "'");
            tag.imageUrl = dr["ImageUrl"].ToString();
            tag.description = dr["Description"].ToString().Replace("''", "'");
            tag.BigBannerUrl = dr["BigBannerUrl"].ToString();
            tag.SwatchUrl = dr["SwatchUrl"].ToString();
            if (!string.IsNullOrEmpty(dr["Type"].ToString()))
            {
                int type = int.Parse(dr["Type"].ToString());
                switch (type)
                {
                    case 1: tag.type = TagType.HOWTOWEAR;
                        break;
                    case 2: tag.type = TagType.STYLEICONS;
                        break;
                    case 3: tag.type = TagType.DESIGNERS;
                        break;
                    case 4: tag.type = TagType.VIBES;
                        break;
                    case 5: tag.type = TagType.SITUATIONS;
                        break;
                    case 6: tag.type = TagType.ENTERTAINMENT;
                        break;  
                    case 0:
                        tag.type = TagType.TRENDING;
                        break;
                }
                if (ColumnExists(dr, "TypeDescription"))
                {
                    tag.type.description = dr["TypeDescription"].ToString().Replace("''", "'");
                }
            }

            
            if (ColumnExists(dr, "following") && !string.IsNullOrEmpty(dr["following"].ToString()))
            {
                tag.IsFollowing = int.Parse(dr["following"].ToString());
            }
            if (ColumnExists(dr, "Editorial") && !string.IsNullOrEmpty(dr["Editorial"].ToString()))
            {
                tag.IsEditorial = bool.Parse(dr["Editorial"].ToString());
            }
            if (ColumnExists(dr, "hasLooks") && !string.IsNullOrEmpty(dr["hasLooks"].ToString()))
            {
                tag.HasLooks = int.Parse(dr["hasLooks"].ToString());
            }

            //if imageurl and bannerurl are empty - then default to amazon S3 url
            //if (string.IsNullOrEmpty(tag.imageUrl) && tag.IsEditorial)
                tag.imageUrl = "https://s3-us-west-2.amazonaws.com/fkcontentpics/themes/" + tag.name + "_small.jpg";

            //if (string.IsNullOrEmpty(tag.BigBannerUrl) && tag.IsEditorial)
                tag.BigBannerUrl = "https://s3-us-west-2.amazonaws.com/fkcontentpics/themes/" + tag.name + "_big.jpg";

            return tag;
        }

        public static List<Tag> GetTagsFromSqlReader(SqlDataReader dr)
        {
            List<Tag> tags = new List<Tag>();

            while (dr.Read())
            {
                Tag tag = Tag.GetTagFromSqlReader(dr);
                tags.Add(tag);
            }

            return tags;
        }

        public static List<Tag> getAllHashtags(long userId, string db, int offset = 1, int limit = 10)
        {
            List<Tag> popularTags = new List<Tag>();

            string query = "EXEC [stp_SS_GetPopularTags] @userId=" + userId + ",@offset=" + offset + ",@limit=" + limit;

            SqlConnection myConnection = new SqlConnection(db);

            try
            {
                myConnection.Open();
                using (SqlDataAdapter adp = new SqlDataAdapter(query, myConnection))
                {
                    SqlCommand cmd = adp.SelectCommand;
                    cmd.CommandTimeout = 300000;
                    System.Data.SqlClient.SqlDataReader dr = cmd.ExecuteReader();

                    popularTags = Tag.GetTagsFromSqlReader(dr);
                }
            }
            finally
            {
                myConnection.Close();
            }

            return popularTags;
        }
 
        public static List<Tag> getPopularHashtags(long userId, string db, int offset = 1, int limit = 10)
        {
            List<Tag> popularTags = new List<Tag>();

            string hpTags = GetTags();

            string query = "EXEC [stp_SS_GetPopularTags] @userId=" + userId + ",@offset=" + offset + ",@limit=" + limit + ",@hpTags='" + hpTags.Replace("'", "''") + "'" ;

            SqlConnection myConnection = new SqlConnection(db);

            try
            {
                myConnection.Open();
                using (SqlDataAdapter adp = new SqlDataAdapter(query, myConnection))
                {
                    SqlCommand cmd = adp.SelectCommand;
                    cmd.CommandTimeout = 300000;
                    System.Data.SqlClient.SqlDataReader dr = cmd.ExecuteReader();

                    popularTags = Tag.GetTagsFromSqlReader(dr);
                }
            }
            finally
            {
                myConnection.Close();
            }

            return popularTags;
        }

        public static Dictionary<TagType, List<Tag>> getFeaturedHashtags(long userId, string db)
        {
            Dictionary<TagType, List<Tag>> featuredTags = new Dictionary<TagType, List<Tag>>();

            string query = "EXEC [stp_SS_GetFeaturedTags] @userId=" + userId;

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
                        if (!featuredTags.ContainsKey(tag.type))
                        {
                            List<Tag> tags = new List<Tag>();
                            tags.Add(tag);
                            featuredTags.Add(tag.type, tags);
                        }
                        else
                        {
                            featuredTags[tag.type].Add(tag);
                        }
                    }

                    //dr.NextResult();
                    //while (dr.Read())
                    //{
                    //    Tag tag = Tag.GetTagFromSqlReader(dr);
                    //    if (!featuredTags.ContainsKey(tag.type))
                    //    {
                    //        List<Tag> tags = new List<Tag>();
                    //        tags.Add(tag);
                    //        featuredTags.Add(tag.type, tags);
                    //    }
                    //    else
                    //    {
                    //        featuredTags[tag.type].Add(tag);
                    //    }
                    //}
                }
            }
            finally
            {
                myConnection.Close();
            }

            return featuredTags;
        }

        public static Dictionary<TagType, List<Tag>> getFeaturedHashtagsv2(long userId, string db)
        {
            Dictionary<TagType, List<Tag>> featuredTags = new Dictionary<TagType, List<Tag>>();

            string hpTags = GetTags();

            string query = "EXEC [stp_SS_GetFeaturedTagsv2] @userId=" + userId + ",@hpTags='" + hpTags.Replace("'", "''") + "'";

            SqlConnection myConnection = new SqlConnection(db);

            try
            {
                myConnection.Open();
                using (SqlDataAdapter adp = new SqlDataAdapter(query, myConnection))
                {
                    SqlCommand cmd = adp.SelectCommand;
                    cmd.CommandTimeout = 300000;
                    System.Data.SqlClient.SqlDataReader dr = cmd.ExecuteReader();

                    //while (dr.Read())
                    //{
                    //    Tag tag = Tag.GetTagFromSqlReader(dr);
                    //    tag.type = TagType.TRENDING;
                        
                    //    if (!featuredTags.ContainsKey(tag.type))
                    //    {
                    //        List<Tag> tags = new List<Tag>();
                    //        tags.Add(tag);
                            
                    //        featuredTags.Add(tag.type, tags);
                    //    }
                    //    else
                    //    {
                    //        featuredTags[tag.type].Add(tag);
                    //    }
                    //}

                    //dr.NextResult();
                    while (dr.Read())
                    {
                        Tag tag = Tag.GetTagFromSqlReader(dr);
                        if (!featuredTags.ContainsKey(tag.type))
                        {
                            List<Tag> tags = new List<Tag>();
                            tags.Add(tag);
                            featuredTags.Add(tag.type, tags);
                        }
                        else
                        {
                            featuredTags[tag.type].Add(tag);
                        }
                    }
                }
            }
            finally
            {
                myConnection.Close();
            }

            return featuredTags;
        }

        public static Dictionary<int, List<Object>> getHPFeaturedTags(long userId, string db)
        {
            Dictionary<int, List<Object>> featuredTags = new Dictionary<int, List<Object>>();

            HomeTheme edTags = GetEditorialTags();
            //add the editorial info

            List<Object> editorialInfo = new List<Object>();
            editorialInfo.Add(edTags.editionType);
            editorialInfo.Add(edTags.editionNumber);
            editorialInfo.Add(edTags.headline);
            featuredTags.Add(0,editorialInfo);

            string hpTags = string.Empty;

            foreach (Theme theme in edTags.themes)
            {
                hpTags += (theme.themeName + ",");
            }
            hpTags = hpTags.TrimEnd(',');

            string query = "EXEC [stp_SS_GetHPFeaturedTags] @userId=" + userId + ",@hpTags='" + hpTags.Replace("'", "''") + "'";

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
                        int slot = 0;
                        if (ColumnExists(dr, "slot") && !string.IsNullOrEmpty(dr["slot"].ToString()))
                        {
                            slot = int.Parse(dr["slot"].ToString());
                        }
                        if (!featuredTags.ContainsKey(slot))
                        {
                            List<Object> tags = new List<Object>();
                            tags.Add(tag);
                            featuredTags.Add(slot, tags);
                        }
                        else
                        {
                            featuredTags[slot].Add(tag);
                        }
                    }
                }
            }
            finally
            {
                myConnection.Close();
            }

            return featuredTags;
        }

        public static Dictionary<string, object> GetTagMetaInfo(long userId, long tagId, int noOfLooks, int noOfItems, int noOfStylists, string db)
        {
            Dictionary<string, object> metaInfo = new Dictionary<string, object>();

            string query = "EXEC [stp_SS_GetTagMetaInfo] @tagId=" + tagId + ",@userId=" + userId + ",@noOfLooks=" + noOfLooks +",@noOfItems=" + noOfItems + ",@noOfStylists=" + noOfStylists;

            SqlConnection myConnection = new SqlConnection(db);

            try
            {
                myConnection.Open();
                using (SqlDataAdapter adp = new SqlDataAdapter(query, myConnection))
                {
                    SqlCommand cmd = adp.SelectCommand;
                    cmd.CommandTimeout = 300000;
                    System.Data.SqlClient.SqlDataReader dr = cmd.ExecuteReader();
                   
                    //Get popular looks
                    List<Look> popularLooks = new List<Look>();

                    int counter = 0; int countLooks = 0;
                    while (dr.Read())
                    {
                        countLooks = int.Parse(dr["total"].ToString());
                    }
                    dr.NextResult();
                    do
                    {
                        Look look = Look.GetLookFromSqlReader(dr);
                        popularLooks.Add(look);
                        counter++;
                        if (counter >= noOfLooks || counter >= countLooks)
                            break;
                    } while (dr.NextResult());
                    metaInfo.Add("Popular Looks", popularLooks);

                    /*Get fresh looks
                    List<Look> recentLooks = new List<Look>();
                    dr.NextResult();
                    counter = 0; countLooks = 0;
                    while (dr.Read())
                    {
                        countLooks = int.Parse(dr["total"].ToString());
                    }
                    dr.NextResult();
                    do
                    {
                        Look look = Look.GetLookFromSqlReader(dr);
                        recentLooks.Add(look);
                        counter++;
                        if (counter >= noOfLooks || counter >= countLooks)
                            break;
                    } while (dr.NextResult());
                    metaInfo.Add("Recent Looks", recentLooks);*/

                    //Get top stylists
                    List<UserProfile> stylists = new List<UserProfile>();
                    dr.NextResult();
                    while (dr.Read())
                    {
                        UserProfile user = UserProfile.GetUserFromSqlReader(dr);
                        stylists.Add(user);
                    }
                    metaInfo.Add("Top Stylists", stylists);

                    /*Get top products
                    List<Product> topItems = new List<Product>();
                    dr.NextResult();

                    while (dr.Read())
                    {
                        topItems.Add(Product.GetProductFromSqlDataReader(dr));
                    }
                    metaInfo.Add("Top Items", topItems);*/

                    dr.NextResult();
                    Tag tag = new Tag();
                    while (dr.Read())
                    {
                        tag = Tag.GetTagFromSqlReader(dr);
                    }
                    metaInfo.Add("Tag", tag);
                }
            }
            finally
            {
                myConnection.Close();
            }

            return metaInfo;
        }

        public static Tag GetTagByName(string tagName, long userId, string db)
        {
            Tag tag = new Tag();

            string query = "EXEC [stp_SS_GetTagByName] @tagName=N'" + tagName + "', @userId=" + userId;

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
                        tag = Tag.GetTagFromSqlReader(dr);
                    }
                }
            }
            finally
            {
                myConnection.Close();
            }

            return tag;
        }
    }
}
