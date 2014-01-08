using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.Serialization;
using System.Data.SqlClient;
using System.IO;
using System.Runtime.Serialization.Json;

namespace ShopSenseDemo
{
    [DataContract]
    public class Category
    {
        [DataMember]
        public string id { get; set; }

        [DataMember]
        public string parentId { get; set; }

        [DataMember]
        public string name { get; set; }

        //[DataMember]
        //public int count { get; set; }
        public string imageUrl {get; set;}

        public static Category GetCategoryFromSqlDataReader(SqlDataReader dr)
        {
            Category cat = new Category();
            cat.name = dr["Name"].ToString();
            cat.id = dr["Id"].ToString();
            cat.parentId = dr["ParentId"].ToString();
            cat.imageUrl = dr["ImageUrl"].ToString();
            return cat;
        }

    }

    public class CategoryTree
    {
        public Dictionary<Category, List<Category>> handbagCats;

        public Dictionary<Category, List<Category>> clothingCats;

        public Dictionary<Category, List<Category>> beautyCats;

        public Dictionary<Category, List<Category>> shoeCats; 

        public void LoadCategoryTree(string db)
        {
            handbagCats = new Dictionary<Category, List<Category>>();
            clothingCats =  new Dictionary<Category, List<Category>>();
            shoeCats =  new Dictionary<Category, List<Category>>();
            beautyCats =  new Dictionary<Category, List<Category>>();

            string query = "EXEC [stp_SS_LoadCategoryTree]";

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
                        Category cat = Category.GetCategoryFromSqlDataReader(dr);
                        
                        List<Category> childrenCats = new List<Category>();
                        handbagCats.Add(cat, childrenCats);
                    }

                    dr.NextResult();

                    while (dr.Read())
                    {
                        Category cat = Category.GetCategoryFromSqlDataReader(dr);

                        List<Category> childrenCats = new List<Category>();
                        shoeCats.Add(cat, childrenCats);
                    }

                    dr.NextResult();
                    while (dr.Read())
                    {
                        Category cat = Category.GetCategoryFromSqlDataReader(dr);

                        List<Category> childrenCats = new List<Category>();
                        clothingCats.Add(cat, childrenCats);
                    }

                    dr.NextResult();
                    while (dr.Read())
                    {
                        Category cat = Category.GetCategoryFromSqlDataReader(dr);

                        List<Category> childrenCats = new List<Category>();
                        beautyCats.Add(cat, childrenCats);
                    }

                    dr.NextResult();

                    while (dr.Read())
                    {
                        Category cat = Category.GetCategoryFromSqlDataReader(dr);

                        var parentCat =
                                from c in clothingCats.Keys
                                where c.id == cat.parentId
                                select c;

                        foreach(var pCat in parentCat)
                        {
                            clothingCats[pCat].Add(cat);
                        }
                    }

                    dr.NextResult();

                    while (dr.Read())
                    {
                        Category cat = Category.GetCategoryFromSqlDataReader(dr);

                        var parentCat =
                                from c in beautyCats.Keys
                                where c.id == cat.parentId
                                select c;

                        foreach (var pCat in parentCat)
                        {
                            beautyCats[pCat].Add(cat);
                        }
                    }
                }
            }
            finally
            {
                myConnection.Close();
            }


        }

        public string flattenCatTree(Dictionary<Category, List<Category>> catTree, string treeName)
        {
            string flattenTreeString = "<" + treeName + ">";
            foreach (KeyValuePair<Category, List<Category>> pair in catTree)
            {
                flattenTreeString += "<" + pair.Key.name + " Id=\"" + pair.Key.id + "\" Image=\"" + pair.Key.imageUrl + "\" />";
                foreach (Category childcat in pair.Value)
                {
                    flattenTreeString += "<" + childcat.name + " Id=\"" + childcat.id + "\" Image=\"" + childcat.imageUrl + "\" />";
                }
                flattenTreeString += "</" + pair.Key.name + ">";
            }
            flattenTreeString += "</" + treeName + ">";
            return flattenTreeString;
        }
    }

    /*[DataContract]
    public class QueryDetails
    {
        [DataMember]
        public string category { get; set; }

        [DataMember]
        public string categoryName { get; set; }

        [DataMember]
        public string showSizeFilter { get; set; }

        [DataMember]
        public string showColorFilter { get; set; }

        //[OptionalField]
        //public string FreeTextSearch { get; set; }
    }*/

    [DataContract]
    public class Categories
    {
        [DataMember]
        public QueryDetails queryDetails { get; set; }

        [DataMember]
        public List<Category> categories { get; set; }

        public void SaveCategories(string db)
        {
            foreach (Category cat in categories)
            {
                //Get an image for every category
                string imageUrl = string.Empty;
                string products = Program.ExecuteGetCommand(string.Format(Program.SSProductQueryUrl, cat.id), string.Empty, string.Empty);

                using (Stream s = Program.GenerateStreamFromString(products))
                {
                    DataContractJsonSerializer ser = new DataContractJsonSerializer(typeof(Products));

                    Products SSProducts = (Products)ser.ReadObject(s);
                    imageUrl = SSProducts.products[0].images[2].url;
                }
                string query = "EXEC [stp_SS_SaveCategory] @id=N'" + cat.id.Replace("'", "\"") + "', @parentId=N'" + cat.parentId.Replace("'", "\"")
                                + "', @name=N'" + cat.name.Replace("'", "\"") + "', @imageUrl='" + imageUrl.Replace("'", "\"") + "'";

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
}
