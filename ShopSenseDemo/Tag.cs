using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ShopSenseDemo
{
    public class Tag
    {
        public long id { get; set; }

        public string name { get; set; }

        public Tag(long tagId, string tagName)
        {
            id = tagId;
            name = tagName;
        }
    }
}
