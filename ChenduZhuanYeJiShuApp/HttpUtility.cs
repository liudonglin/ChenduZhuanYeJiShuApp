using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;

namespace ChenduZhuanYeJiShuApp
{
    public static class HttpUtility
    {
        public static CookieContainer AllCookie = new CookieContainer();

        public const string HOST = "web.chinahrt.com";

        public static string HttpGet(string Url, Encoding encoding = null, IDictionary<string, string> parameters = null)
        {
            if (parameters != null && parameters.Count > 0)
            {
                StringBuilder buffer = new StringBuilder();
                int i = 0;
                foreach (string key in parameters.Keys)
                {
                    if (i > 0)
                    {
                        buffer.AppendFormat("&{0}={1}", key, parameters[key]);
                    }
                    else
                    {
                        buffer.AppendFormat("{0}={1}", key, parameters[key]);
                        i++;
                    }
                }

                if (Url.Contains("?"))
                {
                    Url += "&" + buffer.ToString();
                }
                else
                {
                    Url += "?" + buffer.ToString();
                }
            }

            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(Url);
            request.Method = "GET";
            HttpWebResponse response = (HttpWebResponse)request.GetResponse();

            var cookieStr = response.Headers["Set-Cookie"];

            Stream myResponseStream = response.GetResponseStream();
            if (encoding == null)
            {
                encoding = Encoding.Default;
            }
            StreamReader myStreamReader = new StreamReader(myResponseStream, encoding);
            string retString = myStreamReader.ReadToEnd();
            myStreamReader.Close();
            myResponseStream.Close();

            return retString;
        }

        public static string HttpPost(string Url, Encoding encoding = null, IDictionary<string, string> parameters = null,bool isAjax=false)
        {
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(Url);
            request.Method = "POST";
            request.ContentType = "application/x-www-form-urlencoded";
            request.CookieContainer = AllCookie;
            if (isAjax)
            {
                request.Headers.Add("X-Requested-With", "XMLHttpRequest");
            }
            string postDataStr = string.Empty;
            //发送POST数据  
            if (parameters != null && parameters.Count > 0)
            {
                StringBuilder buffer = new StringBuilder();
                int i = 0;
                foreach (string key in parameters.Keys)
                {
                    if (i > 0)
                    {
                        buffer.AppendFormat("&{0}={1}", key, parameters[key]);
                    }
                    else
                    {
                        buffer.AppendFormat("{0}={1}", key, parameters[key]);
                        i++;
                    }
                }
                postDataStr = buffer.ToString();
            }

            request.ContentLength = Encoding.Default.GetByteCount(postDataStr);
            Stream myRequestStream = request.GetRequestStream();
            StreamWriter myStreamWriter = new StreamWriter(myRequestStream, Encoding.Default);
            myStreamWriter.Write(postDataStr);
            myStreamWriter.Close();

            HttpWebResponse response = (HttpWebResponse)request.GetResponse();
            Stream myResponseStream = response.GetResponseStream();
            if (encoding == null)
            {
                encoding = Encoding.Default;
            }
            StreamReader myStreamReader = new StreamReader(myResponseStream, encoding);
            string retString = myStreamReader.ReadToEnd();
            myStreamReader.Close();
            myResponseStream.Close();

            return retString;
        }
    }
}
