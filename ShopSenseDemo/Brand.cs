using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.Serialization;
using System.Data.SqlClient;

namespace ShopSenseDemo
{
    [DataContract]
    public class Brand
    {
        [DataMember]
        public string name { get; set; }

        [DataMember]
        public long id { get; set; }

        [DataMember]
        public string url { get; set; }

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
                string query = "EXEC [stp_SS_SaveBrand] @id= " + r.id + ", @name=N'" + r.name.Replace("'", "\"") + "', @url=N'" + r.url + "'";
               
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
