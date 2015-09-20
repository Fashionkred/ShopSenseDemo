using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.Serialization;

namespace Housekeeping
{
    [DataContract]
    public class EditorialTag
    {
        [DataMember]
        public string themeName { get; set; }

        [DataMember]
        public List<long> lookIds { get; set; }

        [DataMember]
        public string action { get; set; }

        public EditorialTag(string themeName, List<long> lookIds, string action)
        {
            this.themeName = themeName;
            this.lookIds = lookIds;
            this.action = action;
        }
    }

    [DataContract]
    public class EditorialTags
    {
        [DataMember]
        public List<EditorialTag> tagList { set; get; }

        public EditorialTags()
        {
            this.tagList = new List<EditorialTag>();
        }
    }
}
