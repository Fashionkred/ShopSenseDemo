using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.Serialization;

namespace ShopSenseDemo
{
    [DataContract]
    public class Theme
    {
        [DataMember]
        public string themeType { get; set; }

        [DataMember]
        public string themeName { get; set; }
    }
    
    [DataContract]
    public class HomeTheme
    {
        [DataMember]
        public string date { get; set; }

        [DataMember]
        public string editionType { get; set; }

        [DataMember]
        public string editionNumber { get; set; }

        [DataMember]
        public string headline { get; set; }

        [DataMember]
        public List<Theme> themes { get; set; }

        public HomeTheme()
        {
            this.themes = new List<Theme>();
        }
    }

    [DataContract]
    public class Schedule
    {
        [DataMember]
        public List<HomeTheme> schedule { get; set; }

        public Schedule()
        {
            this.schedule = new List<HomeTheme>();
        }
    }
}
