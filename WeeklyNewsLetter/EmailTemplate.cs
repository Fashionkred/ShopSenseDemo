using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.Serialization;

namespace WeeklyNewsLetter
{
    [DataContract]
    public class EmailTemplate
    {
        [DataMember]
        public string subject { get; set; }

        [DataMember]
        public string date { get; set; }

        [DataMember]
        public string templateName { get; set; }

        [DataMember]
        public string preHeaderText { get; set; }

        [DataMember]
        public string headline { get; set; }
        [DataMember]
        public string description { get; set; }
        [DataMember]
        public List<long> lookIds { get; set; }
        [DataMember]
        public List<string> tags { get; set; }

        //public EmailTemplate(string link, string sDate, string eDate, string iphAsset, string ipdAsset, int iphWidth, int ipdWidth)
        //{
        //    this.subject = link;
        //    this.date = sDate;
        //    this.templateName = eDate;
        //    this.preHeaderText = iphAsset;
        //    this.headline = ipdAsset;
        //    this.iPhoneHeight = iphWidth;
        //    this.iPadHeight = ipdWidth;
        //    this.tagId = 0;
        //}
    }
    [DataContract]
    public class EmailTemplates
    {
        [DataMember]
        public List<EmailTemplate> emails { get; set; }
    }

}
