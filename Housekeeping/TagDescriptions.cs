using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.Serialization;


namespace Housekeeping
{
    [DataContract]
    public class TagDescription
    {
        [DataMember]
        public string name { get; set; }

        [DataMember]
        public string description { get; set; }

    }

    [DataContract]
    public class TagDescriptions
    {
        [DataMember]
        public List<TagDescription> themes { set; get; }

        public TagDescriptions()
        {
            this.themes = new List<TagDescription>();
        }
    }
}
