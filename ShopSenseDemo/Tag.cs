using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.Serialization;

namespace ShopSenseDemo
{
    [DataContract]
    public class Tag
    {
        [DataMember]
        public long id { get; set; }

        [DataMember]
        public string name { get; set; }

        public Tag(long tagId, string tagName)
        {
            id = tagId;
            name = tagName;
        }
    }
}
