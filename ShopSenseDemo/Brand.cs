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
    }
}
