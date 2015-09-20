using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.Serialization;
using System.Data.SqlClient;
using System.Data;

namespace ShopSenseDemo
{
    public sealed class BrandType
    {

        public readonly String name;
        private readonly int value;

        public static readonly BrandType EVERYDAY = new BrandType(1, "EVERYDAY");
        public static readonly BrandType YOUTH = new BrandType(2, "YOUTH");
        public static readonly BrandType GROWNUP = new BrandType(3, "GROWN UP");
        public static readonly BrandType COUTURE = new BrandType(4, "RUNWAY");
        public static readonly BrandType SPECIALITY = new BrandType(5, "BOUTIQUE");
        public static readonly BrandType SHOESETC = new BrandType(6, "SHOES ETC.");

        private BrandType(int value, String name)
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
    public class Brand
    {
        [DataMember]
        public string name { get; set; }

        [DataMember]
        public long id { get; set; }

        [DataMember]
        public string url { get; set; }

        public int IsFollowing { get; set; }

        public BrandType type { get; set; }

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

        public static Brand GetBrandFromSqlReader(SqlDataReader dr)
        {
            Brand brand = new Brand();

            brand.id = long.Parse(dr["Id"].ToString());
            brand.name = dr["Name"].ToString().Replace("''", "'");
            
            if (!string.IsNullOrEmpty(dr["Type"].ToString()))
            {
                int type = int.Parse(dr["Type"].ToString());
                switch (type)
                {
                    case 1: brand.type = BrandType.EVERYDAY;
                        break;
                    case 2: brand.type = BrandType.YOUTH;
                        break;
                    case 3: brand.type = BrandType.GROWNUP;
                        break;
                    case 4: brand.type = BrandType.COUTURE;
                        break;
                    case 5:
                        brand.type = BrandType.SPECIALITY;
                        break;
                    case 6:
                        brand.type = BrandType.SHOESETC;
                        break;
                }

            }


            if (ColumnExists(dr, "following") && !string.IsNullOrEmpty(dr["following"].ToString()))
            {
                brand.IsFollowing = int.Parse(dr["following"].ToString());
            }
            return brand;
        }

        public static Dictionary<BrandType, List<Brand>> GetFeaturedBrands(long userId, string db)
        {
            Dictionary<BrandType, List<Brand>> featuredBrands = new Dictionary<BrandType, List<Brand>>();

            string query = "EXEC [stp_SS_GetFeaturedBrands] @userId=" + userId;

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
                        Brand brand = Brand.GetBrandFromSqlReader(dr);
                        if (!featuredBrands.ContainsKey(brand.type))
                        {
                            List<Brand> brands = new List<Brand>();
                            brands.Add(brand);
                            featuredBrands.Add(brand.type, brands);
                        }
                        else
                        {
                            featuredBrands[brand.type].Add(brand);
                        }
                    }
                }
            }
            finally
            {
                myConnection.Close();
            }

            return featuredBrands;

        }
    }

    [DataContract]
    public class Brands
    {
        [DataMember]
        public string idPrefix { get; set; }

        [DataMember]
        public List<Brand> brands { get; set; }

        public void SaveBrands(string db)
        {
            foreach (Brand r in brands)
            {
                string query = "EXEC [stp_SS_SaveBrand] @id= " + r.id + ", @name=N'" + r.name.Replace("'", "''") + "', @url=N'" + r.url + "'";
               
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

        public static List<Brand> GetTopBrands(string db)
        {
            List<Brand> topBrands = new List<Brand>();

            string query = "EXEC [stp_SS_GetTopBrands]";

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
                        Brand brand = new Brand();
                        brand.name = dr["name"].ToString().Replace("\"", "'");
                        brand.id = long.Parse(dr["id"].ToString());

                        topBrands.Add(brand);
                    }
                }
            }
            finally
            {
                myConnection.Close();
            }
            return topBrands;

        }

        
    }
}
