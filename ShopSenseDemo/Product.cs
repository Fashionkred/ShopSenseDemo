using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.Serialization;
using System.Data.SqlClient;
using System.Data;

namespace ShopSenseDemo
{
    [DataContract]
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

        [DataMember]
        public List<Filter> filters { get; set; }

        [DataMember]
        public string FreeTextSearch { get; set; }
    }

    [DataContract]
    public class Filter
    {
        [DataMember]
        public string type { get; set; }

        [DataMember]
        public int id { get; set; }

        [DataMember]
        public string name { get; set; }
    }

    [DataContract]
    public class Image
    {
        [DataMember]
        public string sizeName { get; set; }

        [DataMember]
        public int width { get; set; }

        [DataMember]
        public int height { get; set; }

        [DataMember]
        public string url { get; set; }
    }

    [DataContract]
    public class Color
    {
        [DataMember]
        public List<string> canonical { get; set; }

        [DataMember]
        public List<Image> images {set; get;}

        [DataMember]
        public string name { get; set; }

        [DataMember]
        public string swatchUrl { get; set; }
    }

    [DataContract]
    public class Size
    {
        [DataMember]
        public string canonical { get; set; }
    }

    [DataContract]
    public class ProductColorDetails
    {
        [DataMember]
        public string colorId {set; get;}

        [DataMember]
        public string imageUrl {set; get;}

        [DataMember]
        public string colorName {set; get;}

        [DataMember]
        public string swatchUrl {set; get;}

        [DataMember]
        public bool isDefaultColor { set; get; }
    }

    [DataContract]
    public class Product
    {
        [DataMember]
        public long id { get; set; }

        [DataMember]
        public string name { get; set; }

        [DataMember]
        public string currency { get; set; }

        [DataMember]
        public Decimal price { get; set; }

        [DataMember]
        public string priceLabel { get; set; }

        [DataMember]
        public Decimal salePrice { get; set; }

        [DataMember]
        public string salePriceLabel { get; set; }

        [DataMember]
        public bool inStock { get; set; }

        [DataMember]
        public string retailer { get; set; }

        [DataMember]
        public int retailerId { get; set; }

        [DataMember]
        public string retailerUrl { get; set; }

        [DataMember]
        public string locale { get; set; }

        [DataMember]
        public string description { get; set; }

        [DataMember]
        public string brandName { get; set; }

        [DataMember]
        public int brandId { get; set; }

        [DataMember]
        public string brandUrl { get; set; }

        [DataMember]
        public string url { get; set; }
 
        [DataMember]
        public List<Image> images { get; set; }

        [DataMember]
        public List<Color> colors { get; set; }

        [DataMember]
        public List<Size> sizes { get; set; }

        [DataMember]
        public List<Category> categories { get; set; }

        [DataMember]
        public List<string> categoryNames { get; set; }

        [DataMember]
        public string seeMoreLabel { get; set; }

        [DataMember]
        public string seeMoreUrl { get; set; }

        [DataMember]
        public string extractDate { get; set; }

        [DataMember]
        public int loves { get; set; }

        public string colorId { get; set; }

        public bool isCover { get; set; }
        public string sizeString { get; set; }
        public string AffiliateUrl {get; set;}
        public string swatchUrl { get; set; }

        public bool inCloset { get; set; }

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

        public static Product GetProductFromSqlDataReader(SqlDataReader dr)
        {
            Product p = new Product();

            p.id = long.Parse(dr["id"].ToString());
            p.name = dr["Name"].ToString();
            p.images = new List<Image>();
            p.categories = new List<Category>();
            p.colors = new List<Color>();
            Image img = new Image();
            img.url = GetBestImageUrl(dr["ImageUrl"].ToString());
            p.images.Add(img);
            p.url = dr["Url"].ToString();
            p.price = (Decimal)dr["Price"];
            p.salePrice = (Decimal)dr["SalePrice"];
            p.loves = int.Parse(dr["Loves"].ToString());
            p.brandId = int.Parse(dr["BrandId"].ToString());
            p.retailerId = int.Parse(dr["RetailerId"].ToString());
            p.sizeString =  dr["Size"].ToString().TrimEnd(',');
            

            if (dr["BrandName"] != null)
            {
                p.brandName = dr["BrandName"].ToString();
            }
            
            if (dr["RetailerName"] != null)
            {
                p.retailer = dr["RetailerName"].ToString();
            }

            if (!string.IsNullOrEmpty(dr["AffiliateUrl"].ToString()))
            {
                p.AffiliateUrl = dr["AffiliateUrl"].ToString();
                //p.url = p.AffiliateUrl;
            }
            if ( ColumnExists(dr, "ColorImageUrl") &&  !string.IsNullOrEmpty(dr["ColorImageUrl"].ToString()))
            {
                Image colorImg = new Image();
                colorImg.url = dr["ColorImageUrl"].ToString();
                p.images[0] = colorImg;
            }

            if (ColumnExists(dr, "CategoryId") && !string.IsNullOrEmpty(dr["CategoryId"].ToString()))
            {
                Category cat = new Category();
                cat.id = dr["CategoryId"].ToString();
                cat.name = dr["CategoryName"].ToString();
                p.categories.Add(cat);
            }

            if (ColumnExists(dr, "IsCover") && !string.IsNullOrEmpty(dr["IsCover"].ToString()))
            {
                string isCover = dr["IsCover"].ToString();
                p.isCover = isCover == "True" ? true : false;
            }

            //IF color id is present - then take that otherwise choose default color 
            if (ColumnExists(dr, "ColorId") && !string.IsNullOrEmpty(dr["ColorId"].ToString()))
            {
                Color color = new Color();
                color.canonical = new List<string>();
                color.canonical.Add(dr["ColorId"].ToString());

                if (ColumnExists(dr, "ColorName") && !string.IsNullOrEmpty(dr["ColorName"].ToString()))
                {
                    color.name = dr["ColorName"].ToString();
                }

                p.colors.Add(color);
            }
            else if (ColumnExists(dr, "DefaultColorId") && !string.IsNullOrEmpty(dr["DefaultColorId"].ToString()))
            {
                Color color = new Color();
                color.canonical = new List<string>();
                color.canonical.Add(dr["DefaultColorId"].ToString());
                p.colors.Add(color);
            }

            if (ColumnExists(dr, "SwatchUrl") && !string.IsNullOrEmpty(dr["SwatchUrl"].ToString()))
            {
                p.swatchUrl = dr["SwatchUrl"].ToString();
            }

            if (ColumnExists(dr, "InCloset") && !string.IsNullOrEmpty(dr["InCloset"].ToString()))
            {
                p.inCloset = true;
            }

            return p;
        }

        public string GetCategoryId()
        {
            if (categories.Count > 0)
                return categories[0].id;
            else return null;
        }

        public string GetCategoryName()
        {
            if (categories.Count > 0)
                return categories[0].name;
            else return null;
        }

        public string GetColor()
        {
            if (colors.Count > 0)
                return colors[0].canonical[0];
            else return null;
        }
        public string GetColorName()
        {
            if (colors.Count > 0)
                return colors[0].name;
            else return null;
        }
        public static string GetBestImageUrl(string imageUrl)
        {
            string thumbnail = imageUrl.Replace("xim", "pim");
            thumbnail = thumbnail.Replace("resources", "bim");
            
            thumbnail = thumbnail.Replace(".jpg", "_best.jpg");

            return thumbnail;
        }
        public string GetImageUrl()
        {
            string bigPic = this.images[0].url.Replace("pim", "xim");
            return bigPic;
        }
        public string GetNormalImageUrl()
        {
            string normalPic = this.images[0].url.Replace("xim", "pim");
            return normalPic;
        }

        public string GetBrandName()
        {
            string brandName = this.brandName;
            if (string.IsNullOrEmpty(brandName))
            {
                brandName = "UnListed";
            }
            return brandName;
        }

        public string GetName()
        {
            string name = this.name;
            if (this.name.IndexOf(',') >=2 )
            {
                name = name.Substring(this.name.IndexOf(',') + 2);
            }

            if (string.IsNullOrEmpty(name))
                return this.name.Substring(0, 35);
            else
            {
                if (name.Length > 35)
                {
                    return name.Substring(0,33) + "..";
                }
                else
                {
                    return name;
                }
            }
        }

        public string GetThumbnailUrl()
        {
            string thumbnail = this.images[0].url.Replace("xim","pim");
            thumbnail = thumbnail.Replace(".jpg", "_medium.jpg");

            return thumbnail;
        }

        public static Product GetProductById(long id, string db)
        {
            Product p = null;

            string query = "EXEC [stp_SS_GetProduct] @id=" + id;
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
                        p = GetProductFromSqlDataReader(dr);
                    }
                }
            }
            finally
            {
                myConnection.Close();
            }
            return p;

        }

        public static Dictionary<string, List<Product>> GetSimilarProducts(string categoryId,string colorId,long productId, long userId, string db)
        {
            Dictionary<string, List<Product>> similarProducts = new Dictionary<string, List<Product>>();
            string query = "EXEC [stp_SS_GetSimilarProducts] @categoryId=N'" + categoryId + "', @colorId=N'" + colorId + "', @productId=" + productId + ",@userId=" + userId;
            SqlConnection myConnection = new SqlConnection(db);
            try
            {
                myConnection.Open();
                using (SqlDataAdapter adp = new SqlDataAdapter(query, myConnection))
                {
                    SqlCommand cmd = adp.SelectCommand;
                    cmd.CommandTimeout = 300000;
                    System.Data.SqlClient.SqlDataReader dr = cmd.ExecuteReader();
                        
                    List<Product> exactSimilarproducts = new List<Product>();
                        
                    while (dr.Read())
                    {
                        exactSimilarproducts.Add(GetProductFromSqlDataReader(dr));
                    }
                    similarProducts.Add("exact", exactSimilarproducts);
 
                    //Same Color
                    dr.NextResult();
                    List<Product> sameColorProducts = new List<Product>();

                    while (dr.Read())
                    {
                        sameColorProducts.Add(GetProductFromSqlDataReader(dr));
                    }
                    similarProducts.Add("color", sameColorProducts);

                    //Same Brand
                    dr.NextResult();
                    List<Product> sameBrandProducts = new List<Product>();

                    while (dr.Read())
                    {
                        sameBrandProducts.Add(GetProductFromSqlDataReader(dr));
                    }
                    similarProducts.Add("brand", sameBrandProducts);
                }
            }
            finally
            {
                myConnection.Close();
            }
            return similarProducts;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="userId"></param>
        /// <param name="db"></param>
        /// <param name="categoryId">category if selected</param>
        /// <param name="colorId"></param>
        /// <param name="tags">Tags names in a comma separated list</param>
        /// <param name="offset">offset from where to read</param>
        /// <param name="limit">how many to read</param>
        /// <returns></returns>
        public static Dictionary<string, List<Product>> GetPopularProductsByFilters(long userId, string db, string tags = null, string categoryId = null, string colorId = null, 
                                                                                     int offset=1, int limit=20)
        {
            Dictionary<string, List<Product>> popularProducts = new Dictionary<string, List<Product>>();
            string query = "EXEC [stp_SS_GetProductsByFilters] @uId =" + userId + ",@offset=" + offset + ",@limit=" + limit;
            if(categoryId != null)
                query += ",@categoryId=N'" + categoryId + "'";
            if(colorId != null)
                query += ",@colorId=N'" + colorId + "'";
            if (tags != null)
                query += ",@tags='" + tags + "'";

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
                        List<Product> closetItems = new List<Product>();

                        while (dr.Read())
                        {
                            type = dr["type"].ToString();
                            closetItems.Add(Product.GetProductFromSqlDataReader(dr));
                        }
                        if (!string.IsNullOrEmpty(type))
                        {
                            if (!popularProducts.ContainsKey(type))
                                popularProducts.Add(type, closetItems);
                            else
                                popularProducts[type] = closetItems;
                        }

                    } while (dr.NextResult());
                }
            }
            finally
            {
                myConnection.Close();
            }
            return popularProducts;
        }
        
        
        public static Dictionary<string, ProductColorDetails> GetProductColorOptions(long productId, string db)
        {
            Dictionary<string, ProductColorDetails> colorOptions = new Dictionary<string, ProductColorDetails>();

            string query = "EXEC [stp_SS_GetProductColorOptions] @pId=" + productId;
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
                        ProductColorDetails details = new ProductColorDetails();
                        details.colorId = dr["ColorId"].ToString();
                        details.imageUrl = dr["ImageUrl"].ToString();
                        details.swatchUrl = dr["SwatchUrl"].ToString();
                        details.colorName = dr["ColorName"].ToString();
                        string defaultColorId = dr["DefaultColorId"].ToString();
                        details.isDefaultColor = (details.colorId == defaultColorId) ? true : false;
                        colorOptions.Add(details.colorId, details);
                    }
                }
            }
            finally
            {
                myConnection.Close();
            }
            return colorOptions;
        }

        public static List<Product> GetTaggedPopularProducts(long tagId, long uId, string db, int offset = 1, int limit = 20)
        {
            List<Product> products = new List<Product>();

            string query = "EXEC [stp_SS_GetTaggedPopularItems]  @tagId=" + tagId + ",@userId=" + uId + ",@offset=" + offset + ",@limit=" + limit;
            
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
                        products.Add(GetProductFromSqlDataReader(dr));
                    }
                }
            }
            finally
            {
                myConnection.Close();
            }

            return products;
        }

        public static bool HeartItem(long userId, long productId,bool isHeart,string colorId, string catId, string db)
        {
            int heart = isHeart == true ? 1 : 0;
            bool isSuccess = false;

            string query = "EXEC [stp_SS_SaveLove] @uid=" + userId + ", @pid=" + productId + ",@love=" + heart+ ",@colorId='" + colorId + "',@catId='" + catId +"'" ;
            SqlConnection myConnection = new SqlConnection(db);

            Product p = new Product();
            try
            {
                myConnection.Open();
                using (SqlDataAdapter adp = new SqlDataAdapter(query, myConnection))
                {
                    SqlCommand cmd = adp.SelectCommand;
                    cmd.CommandTimeout = 300000;
                    System.Data.SqlClient.SqlDataReader dr = cmd.ExecuteReader();

                    isSuccess = true;
                    
                }

            }
            finally
            {
                myConnection.Close();
            }
            return isSuccess;
        }
    }

    [DataContract]
    public class Products
    {
        [DataMember]
        public QueryDetails queryDetails { get; set; }

        [DataMember]
        public int totalCount { get; set; }

        [DataMember]
        public List<Product> products { get; set; }

        public void SaveProducts(string db)
        {
            SqlConnection myConnection = new SqlConnection(db);
            try
            {
                myConnection.Open();
                foreach (Product p in products)
                {
                    string categoryList = "<CategoryList>";
                    foreach (Category category in p.categories)
                    {
                        categoryList += "<Category pId=\"" + p.id + "\" cId=\"" + category.id + "\" />";
                    }
                    categoryList += "</CategoryList>";

                    string colorList = "<ColorList>";
                    string defaultColorId = null;
                    foreach (Color color in p.colors)
                    {
                        //store the image url for hte right color
                        if (color.canonical !=null)
                        {
                            string imageUrl = p.images[3].url.Replace("'", "\"");
                            if (color.images != null)
                            {
                                imageUrl = color.images[3].url.Replace("'", "\"");
                                if(p.GetImageUrl() == p.images[3].url)
                                    defaultColorId = color.canonical[0];
                            }
                            colorList += "<Color pId=\"" + p.id + "\" cId=\"" + color.canonical[0] + "\" image=\"" + imageUrl + "\" cname=\"" + color.name +
                               "\" surl=\"" + color.swatchUrl + "\" />";
                        }
                    }
                    colorList += "</ColorList>";

                    string sizeList = string.Empty;
                    foreach (Size size in p.sizes)
                    {
                        sizeList += size.canonical;
                        sizeList += ",";
                    }
                    p.AffiliateUrl = AffiliateLink.GetAffiliateLink(p.url);
               
                    string query = "EXEC [stp_SS_SaveProduct] @id=" + p.id + ", @name=N'" + p.name.Replace("'", "\"") + "', @price=" + p.price + ", @saleprice=" + p.salePrice +
                                                                ", @inStock=" + p.inStock + ", @retailerId=" + p.retailerId + ", @url=N'" + p.url.Replace("'", "\"") +
                                                                "', @locale='" + p.locale.Replace("'", "\"") + "', @description=N'" + p.description.Replace("'", "\"") +
                                                                "', @brandId=" + p.brandId + ", @imageUrl=N'" + p.images[3].url.Replace("'", "\"") + "', @color='" + colorList +
                                                                "', @size=N'" + sizeList.Replace("'", "\"") + "', @seeMoreUrl=N'"+ p.seeMoreUrl.Replace("'", "\"") +
                   "', @extractDate='" + p.extractDate + "', @categoryList='" + categoryList + "',@affiliateUrl=N'" + p.AffiliateUrl + "', @defaultColorId=N'" + defaultColorId + "'" ;

                    try
                    {
                        using (SqlDataAdapter adp = new SqlDataAdapter(query, myConnection))
                        {
                            SqlCommand cmd = adp.SelectCommand;
                            cmd.CommandTimeout = 300000;
                            cmd.ExecuteNonQuery();
                        }
                    }
                    catch(Exception ex)
                    {
                    }
                }
            }
            finally
            {
                myConnection.Close();
            }
            
        }

    }
}
