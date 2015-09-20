using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;
using System.Net;
using System.IO;
using System.Web;
using System.Runtime.Serialization.Json;
using System.Threading;
using System.Data.SqlClient;

namespace ShopSenseDemo
{
    //public class AdCampaigns
    //{
    //    public string keywords;
    //    public string sentiment;
    //    //public string adCopy;
    //}


    //public class TweetResponse
    //{
    //    public string username;
    //    public string tweet;
    //}

    public class Program
    {

        public static string SSRetailerUrl = "http://api.shopstyle.com/api/v2/retailers?pid=uid5264-8690292-48";
        public static string SSBrandUrl = "http://api.shopstyle.com/api/v2/brands?pid=uid5264-8690292-48";
        public static string SSCategoryUrl = "http://api.shopstyle.com/api/v2/categories?pid=uid5264-8690292-48";
        public static string SSProductv2Url = "http://api.shopstyle.com/api/v2/products?pid=uid5264-8690292-48&cat={0}&limit=1&fl=r1";

        public static string SSProductQueryUrl = "http://api.shopstyle.com/action/apiSearch?pid=uid5264-8690292-48&format=json&cat={0}&count=100&fl={1}&fl={2}";

        //public static string connectionString = "Data Source=mssql2008.reliablesite.net,14333,14333;Initial Catalog=facebook;Persist Security Info=True;User ID=nirveek_de;Password=Fashionkred123";
        //public static string connectionString = "server=localhost;Database=Fashionkred;Integrated Security=True;";
        public static string connectionString ="Data Source=startcult.com;Initial Catalog=Fashionkred;Persist Security Info=True;User ID=admin;Password=Fk711101";

        public static string ExecuteGetCommand(string url, string userName, string password)
        {
            System.Net.ServicePointManager.Expect100Continue = false;
            using (WebClient client = new WebClient())
            {
                if (!string.IsNullOrEmpty(userName) && !string.IsNullOrEmpty(password))
                {
                    client.Credentials = new NetworkCredential(userName, password);
                }

                try
                {
                    using (Stream stream = client.OpenRead(url))
                    {
                        using (StreamReader reader = new StreamReader(stream))
                        {
                            return reader.ReadToEnd();
                        }
                    }
                }
                catch (WebException ex)
                {
                    //
                    // Handle HTTP 404 errors gracefully and return a null string to indicate there is no content.
                    //
                    if (ex.Response is HttpWebResponse)
                    {
                        if ((ex.Response as HttpWebResponse).StatusCode == HttpStatusCode.NotFound)
                        {
                            return null;
                        }
                    }

                    //throw ex;
                }
            }

            return null;
        }

        public static Stream GenerateStreamFromString(string s)
        {
            MemoryStream stream = new MemoryStream();
            StreamWriter writer = new StreamWriter(stream);
            writer.Write(s);
            writer.Flush();
            stream.Position = 0;
            return stream;
        }

        public static void StoreRetailers()
        {
            string retailers = ExecuteGetCommand(SSRetailerUrl, string.Empty, string.Empty);

            string cleanretailers = retailers.Replace("\r\n", "");

            using (Stream s = GenerateStreamFromString(cleanretailers))
            {
                DataContractJsonSerializer ser = new DataContractJsonSerializer(typeof(Retailers));

                Retailers SSRetailers = (Retailers)ser.ReadObject(s);

                SSRetailers.SaveRetailers(connectionString);

            }

        }

        public static void StoreBrands()
        {
            string brands = ExecuteGetCommand(SSBrandUrl, string.Empty, string.Empty);

            //string cleanretailers = brands.Replace("\r\n", "");

            using (Stream s = GenerateStreamFromString(brands))
            {
                DataContractJsonSerializer ser = new DataContractJsonSerializer(typeof(Brands));

                Brands SSRetailers = (Brands)ser.ReadObject(s);

                SSRetailers.SaveBrands(connectionString);

            }

        }

        public static void StoreCategories()
        {
            string categories = ExecuteGetCommand(SSCategoryUrl, string.Empty, string.Empty);

            using (Stream s = GenerateStreamFromString(categories))
            {
                DataContractJsonSerializer ser = new DataContractJsonSerializer(typeof(Categories));

                Categories SSCategories = (Categories)ser.ReadObject(s);

                SSCategories.SaveCategories(connectionString);

            }

        }

        public static void GetProducts(string category, string filter, string colorfilter)
        {
            string url = string.Format(SSProductQueryUrl, category, filter, colorfilter);
            string products = ExecuteGetCommand(url, string.Empty, string.Empty);

            using (Stream s = GenerateStreamFromString(products))
            {
                DataContractJsonSerializer ser = new DataContractJsonSerializer(typeof(Products));
                s.Position = 0;
                Products SSProducts = (Products)ser.ReadObject(s);

                SSProducts.SaveProducts(connectionString);

            }
        }

        public static void UpdateAffiliateLinks(int retailerId)
        {
            //Get product id, url
            string query = "SELECT id,Url from [nirveek_de].[SS_Product] where AffiliateUrl is null and retailerid=" + retailerId;

            SqlConnection myConnection = new SqlConnection(connectionString);
            List<Product> products = new List<Product>();
                    
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
                        Product p = new Product();

                        p.id = long.Parse(dr["id"].ToString());
                        p.url = dr["Url"].ToString(); products.Add(p);
                        //products.Add(p);
                    }
                }
            }
            finally
            {
                myConnection.Close();
            }
            try
            {
                myConnection.Open();
                int i = 0;
                Console.WriteLine(products.Count);
                foreach (Product p in products)
                {
                    i++;
                    //Thread.Sleep(500);
                    
                    p.AffiliateUrl = AffiliateLink.GetAffiliateLink(p.url);
                    query = "update [nirveek_de].[SS_Product] set AffiliateUrl = N'" + p.AffiliateUrl + "' where Id =" + p.id;
                    Console.WriteLine(i);
                    using (SqlDataAdapter adp = new SqlDataAdapter(query, myConnection))
                    {
                        SqlCommand cmd = adp.SelectCommand;
                        cmd.CommandTimeout = 300000;
                        cmd.ExecuteNonQuery();
                    }
                }

            }
            finally
            {
                myConnection.Close();
            }
            //calculate affiliatelink
            //save affilaitelink
        }

       public static void Main(string[] args)
        {
            //StoreRetailers();
            //StoreBrands();
            //StoreCategories();

            //Get products for every color
            List<CanonicalColors> colors = new List<CanonicalColors>() {CanonicalColors.Beige, CanonicalColors.Black, CanonicalColors.Blue, CanonicalColors.Brown,
                CanonicalColors.Gold, CanonicalColors.Gray, CanonicalColors.Green, CanonicalColors.Orange, CanonicalColors.Pink, CanonicalColors.Purple,
                CanonicalColors.Red, CanonicalColors.Silver, CanonicalColors.White, CanonicalColors.Yellow
            };

            //List<string> Retailers = new List<string> { "r1", "r3", "r4", "r5", "r6", "r7", "r8", "r9", "r10" };
            //List<string> Retailers = new List<string> { "r21", "r27", "r105", "r1426", "r483", "r760", "r36" , "r29", "r54", "r76", "r1060", "r717"};
            //List<string> Retailers = new List<string> { "r1426", "r483", "r1674" };
            List<string> Brands = new List<string> {"b172","b309","b345","b398","b421","b582","b613","b890","b957","b981","b728","b1605","b1803","b2160", "b2115","b2331","b2332","b2335","b2339",
               "b2342","b2348","b2723","b2446","b2685","b3395","b3397","b3542","b4409","b5846","b8002","b8004","b12778","b13342","b18501","b18563","b18577","b18520","b29353","b29489", "b30363","b31089","b30810" };

            Dictionary<Category, List<Category>> catTree = Category.GetMetaCategories(connectionString);
            foreach(KeyValuePair<Category, List<Category>> metaCat in catTree)
            {
                foreach (Category cat in metaCat.Value)
                {
                    //if (cat.id == "bridal-dresses")
                    //    continue;

                    foreach (CanonicalColors c in colors)
                    {
                        foreach (string brand in Brands)
                        {
                            Console.WriteLine("Cat:" + cat.name+ "Color: " + c + "Brand: "+ brand );

                            string colorfilter = "c" + c.GetHashCode();
                            GetProducts(cat.id, brand, colorfilter);
                        }
                    }
                }
            }

            //CategoryTree catTree = new CategoryTree();
            //catTree.LoadCategoryTree(connectionString);

            //Console.WriteLine(catTree.flattenCatTree(catTree.handbagCats, "HandBag"));
            //Console.WriteLine(catTree.flattenCatTree(catTree.clothingCats, "Clothing"));
            //Console.WriteLine(catTree.flattenCatTree(catTree.beautyCats, "Beauty"));
            //Console.WriteLine(catTree.flattenCatTree(catTree.shoeCats, "Shoe"));

            //UserProfile.GetClosetProducts(5, connectionString);
            //Product.GetPopularProductsByFilters(5, connectionString);
            //Product.GetSimilarProducts("Dresses", "", 348981075, 4, connectionString);
            /*foreach (CanonicalColors c in colors)
            {
                //Get the color code
                Console.WriteLine("Color: " + c);
                string colorfilter = "c" + c.GetHashCode();
                //Get Evening dresses from Nordstrom's
                GetProducts("evening-dresses", "r1", colorfilter);
                GetProducts("evening-shoes", "r1", colorfilter);
                GetProducts("evening-handbags", "r1", colorfilter);

                Console.WriteLine("Combo: Day Outfit");
                GetProducts("day-dresses", "r1", colorfilter);
                GetProducts("shoulder-bags", "r1", colorfilter);
                GetProducts("flats", "r1", colorfilter);

                Console.WriteLine("Combo: Bridal Outfit");
                GetProducts("bridal-dresses", "r1", colorfilter);
                GetProducts("bridal-shoes", "r1", colorfilter);
                GetProducts("clutches", "r1", colorfilter);

                Console.WriteLine("Combo: Casual Outfit");
                GetProducts("skinny-jeans", "r1", colorfilter);
                GetProducts("tank-tops", "r1", colorfilter);

                //Pull all shoes
                GetProducts("boots", "r1", colorfilter);
                GetProducts("mules-and-clogs", "r1", colorfilter);
                GetProducts("platforms", "r1", colorfilter);
                GetProducts("pumps", "r1", colorfilter);
                GetProducts("sandals", "r1", colorfilter);
                GetProducts("wedges", "r1", colorfilter);
                GetProducts("shoes-athletic", "r1", colorfilter);

                //Pull all dresses
                //GetProducts("black-dresses", "r1", colorfilter);
                //GetProducts("cocktail-dresses", "r1", colorfilter);

                ////pull all pants
                GetProducts("shorts", "r1", colorfilter);
                GetProducts("jeans", "r1", colorfilter);
                GetProducts("skirts", "r1", colorfilter);


                //pull all tops
                GetProducts("womens-tops", "r1", colorfilter);
                GetProducts("pants-shorts", "r1", colorfilter);


                //Get all bags
                GetProducts("hobo-bags", "r1", colorfilter);
                GetProducts("satchels", "r1", colorfilter);
                GetProducts("tote-bags", "r1", colorfilter);
                GetProducts("wallets", "r1", colorfilter);
            }

            UpdateAffiliateLinks(1);
            //Look l = new Look();
            //l = Look.GetRandomLook("evening-dresses", "evening-shoes", 2, connectionString);
            //l = Look.GetLookById(1, connectionString);*/
        }
    

    }
}
