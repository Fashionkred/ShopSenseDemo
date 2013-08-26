using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.Serialization;
using System.Data.SqlClient;

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

        [DataMember]
        public int count { get; set; }

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
            foreach (Category r in categories)
            {
                string query = "EXEC [stp_SS_SaveCategory] @id=N'" + r.id.Replace("'", "\"") + "', @parentId=N'" + r.parentId.Replace("'", "\"") + "', @name=N'" + r.name.Replace("'", "\"") + "'";

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
