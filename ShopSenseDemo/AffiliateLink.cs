using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Net;
using System.IO;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Json;
using System.Xml;

/// <summary>
/// Summary description for AffiliateLink
/// </summary>
public class AffiliateLink
{
    public static string affiliateUrl = "http://click.linksynergy.com/fs-bin/click?id=7cbeECDvy68&subid=&offerid=276224.1&type=10&tmpid=8158&RD_PARM1={0}";
    public static string bitlyUrl = "https://api-ssl.bitly.com/v3/shorten?login=o_5vuk3tk0ob&apiKey=R_b5e0995e755fae2c0ef048bf75ff9f96&longUrl={0}&format=xml";

    public static Stream GenerateStreamFromString(string s)
    {
        MemoryStream stream = new MemoryStream();
        StreamWriter writer = new StreamWriter(stream);
        writer.Write(s);
        writer.Flush();
        stream.Position = 0;
        return stream;
    }
    public static string ExecuteGetCommand(string url)
    {
        System.Net.ServicePointManager.Expect100Continue = false;
        using (WebClient client = new WebClient())
        {
            

            try
            {
                using (Stream stream = client.OpenRead(url))
                {
                    using (StreamReader reader = new StreamReader(stream))
                    {
                        return reader.ReadToEnd();
                    }
                }
            }
            catch (WebException ex)
            {
                //
                // Handle HTTP 404 errors gracefully and return a null string to indicate there is no content.
                //
                if (ex.Response is HttpWebResponse)
                {
                    if ((ex.Response as HttpWebResponse).StatusCode == HttpStatusCode.NotFound)
                    {
                        return null;
                    }
                }

                //throw ex;
            }
        }

        return null;
    }
    public static string BitlyShortLink(string longLink)
    {
        string bitlyResponse = ExecuteGetCommand(string.Format(bitlyUrl, longLink));
        XmlDocument doc = new XmlDocument();
        doc.LoadXml(bitlyResponse);
        return doc.SelectSingleNode("//url").InnerText;
    }
    public static string ShortLink(string longLink)
    {
        var httpWebRequest = (HttpWebRequest)WebRequest.Create("https://www.googleapis.com/urlshortener/v1/url?key=AIzaSyAfxhLD4hYGTWqDI7lJ8nKu33FN8PDbFDs");
        httpWebRequest.ContentType = "application/json";
        httpWebRequest.Method = "POST";

        using (var streamWriter = new StreamWriter(httpWebRequest.GetRequestStream()))
        {
            string json = "{\"longUrl\":\"" + longLink + "\"}";
            streamWriter.Write(json);
        }

        var httpResponse = (HttpWebResponse)httpWebRequest.GetResponse();
        using (var streamReader = new StreamReader(httpResponse.GetResponseStream()))
        {
            string responseText = streamReader.ReadToEnd();
            DataContractJsonSerializer ser = new DataContractJsonSerializer(typeof(GoogleShortener));
            GoogleShortener shortUrl = (GoogleShortener)ser.ReadObject(GenerateStreamFromString(responseText));
            return shortUrl.id;
        }

    }

    public static string GetOriginalLink(string url)
    {
        using (WebClient client = new WebClient())
        {

            using (Stream stream = client.OpenRead(url))
            {
                using (StreamReader reader = new StreamReader(stream))
                {
                    string content = reader.ReadToEnd();
                    int startindex = content.IndexOf("RD_PARM1") ;
                    if ( startindex > 0)
                    {
                        content = content.Substring(startindex + 9);
                        int endIndex = content.IndexOf("&amp;");
                        return content.Substring(0, endIndex);
                    }
                }
            }
        }
        return null;
    }

    public static string GetShortAffiliateLink(string shopStyleLink)
    {
        string sourceUrl = GetOriginalLink(shopStyleLink);
        string decodedUrl = HttpUtility.UrlDecode(HttpUtility.UrlDecode(sourceUrl));
        string encodedUrl = HttpUtility.UrlEncode(string.Format(affiliateUrl, decodedUrl));
        //return ShortLink(string.Format(affiliateUrl, decodedUrl));
        return BitlyShortLink(encodedUrl);
    }

    public static string GetAffiliateLink(string shopStyleLink)
    {
        string sourceUrl = GetOriginalLink(shopStyleLink);
        string affiliateLink = string.Format(affiliateUrl, sourceUrl);
        //return ShortLink(string.Format(affiliateUrl, decodedUrl));
        return affiliateLink;
    }
}

[DataContract]
public class GoogleShortener
{
    [DataMember]
    public string id;
}