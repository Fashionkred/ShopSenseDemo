using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.Serialization;
using System.Data.SqlClient;

namespace ShopSenseDemo
{
    [DataContract]
    public class Retailer
    {
        [DataMember]
        public string name { get; set; }

        [DataMember]
        public long id { get; set; }

        [DataMember]
        public string url { get; set; }

        [DataMember]
        public string deeplinkSupport { get; set; }
    }

    [DataContract]
    public class Retailers
    {
        [DataMember]
        public string idPrefix { get; set; }

        [DataMember]
        public List<Retailer> retailers { get; set; }

        public void SaveRetailers(string db)
        {
            foreach (Retailer r in retailers)
            {
                string query = "EXEC [stp_SS_SaveRetailer] @id= " + r.id + ", @name=N'" + r.name.Replace("'", "\"") + "', @url=N'" + r.url + "', @deeplink=";
                
                if(r.deeplinkSupport == "true")
                {
                    query += "1";
                }
                else
                {
                    query += "0";
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
}
