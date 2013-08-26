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
    }

    [DataContract]
    public class Size
    {
        [DataMember]
        public string canonical { get; set; }
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
        public List<string> categories { get; set; }

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

        public string colorString { get; set; }
        public string sizeString { get; set; }
        public string AffiliateUrl {get; set;}

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
            Image img = new Image();
            img.url = dr["ImageUrl"].ToString();
            p.images.Add(img);
            p.url = dr["Url"].ToString();
            p.price = (Decimal)dr["Price"];
            p.salePrice = (Decimal)dr["SalePrice"];
            p.description = dr["Description"].ToString();
            p.loves = int.Parse(dr["Loves"].ToString());

            //p.colorString =  dr["Color"].ToString().TrimEnd(',');
            
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

            return p;
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
                    foreach (string categoryid in p.categories)
                    {
                        categoryList += "<Category pId=\"" + p.id + "\" cId=\"" + categoryid + "\" />";
                    }
                    categoryList += "</CategoryList>";

                    string colorList = "<ColorList>";
                    foreach (Color color in p.colors)
                    {
                        //store the image url for hte right color
                        if (color.canonical !=null)
                        {
                            string imageUrl = p.images[3].url.Replace("'", "\"");
                            if (color.images != null)
                                imageUrl = color.images[3].url.Replace("'", "\"");
                            colorList += "<Color pId=\"" + p.id + "\" cId=\"" + color.canonical[0] + "\" image=\"" + imageUrl + "\" />";
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
                                                                "', @extractDate='" + p.extractDate + "', @categoryList='" + categoryList + "',@affiliateUrl=N'" + p.AffiliateUrl + "'";

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
