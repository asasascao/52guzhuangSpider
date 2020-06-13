using System;
using System.Net;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Threading;
using System.Windows;

namespace _52guzhuangSpider
{
    public class HttpHelper
    {
        static HttpHelper m_httpHelper=null;
        public static HttpHelper GetInstance()
        {
            if(m_httpHelper==null)
            {
                m_httpHelper=new HttpHelper();
            }
            return m_httpHelper;
        }

        public delegate void HttpHandler(HttpWebResponse httpWebResponse);
        public event HttpHandler HttpEvent;

        //IE7
        static string _UserAgent_IE7_x64 = "Mozilla/4.0 (compatible; MSIE 7.0; Windows NT 6.1; WOW64; Trident/5.0; SLCC2; .NET CLR 2.0.50727; .NET CLR 3.5.30729; .NET CLR 3.0.30729; Media Center PC 6.0; InfoPath.3; .NET4.0C; .NET4.0E)";
        //IE8
        static string _UserAgent_IE8_x64 = "Mozilla/4.0 (compatible; MSIE 8.0; Windows NT 6.1; WOW64; Trident/4.0; SLCC2; .NET CLR 2.0.50727; .NET CLR 3.5.30729; .NET CLR 3.0.30729; Media Center PC 6.0; InfoPath.3; .NET4.0C; .NET4.0E";
        //IE9 x64
        static string _UserAgent_IE9_x64 = "Mozilla/5.0 (compatible; MSIE 9.0; Windows NT 6.1; WOW64; Trident/5.0)"; // x64
        //IE9 x86
        static string _UserAgent_IE9_x86 = "Mozilla/5.0 (compatible; MSIE 9.0; Windows NT 6.1; Trident/5.0)"; // x86
        //Chrome
        static string _UserAgent_Chrome = "Mozilla/5.0 (Windows; U; Windows NT 6.1; en-US) AppleWebKit/533.4 (KHTML, like Gecko) Chrome/5.0.375.99 Safari/533.4";
        //Mozilla Firefox
        static string _UserAgent_Firefox = "Mozilla/5.0 (Windows; U; Windows NT 6.1; rv:1.9.2.6) Gecko/20100625 Firefox/3.6.6";
        //Edge
        static string _UserAgent_Edge = "User-Agent : Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/46.0.2486.0 Safari/537.36Edge/13.10586";

        string[] cookieFieldArr = { "expires", "domain", "secure", "path", "httponly", "version" };
        private const int m_def_timeout = 30 * 1000;
        private const int m_def_readwrite_timeout = 30 * 1000;

        public HttpHelper()
        {
            System.Net.ServicePointManager.DefaultConnectionLimit = 200;
        }

        private string GetRandomUserAgent()
        {
            #region 获取随机UserAgent
            Random random = new Random(int.Parse(DateTime.Now.ToString("HHmmss")));
            var random_num=random.Next(0, 4);
            if(random_num==0)
            {
                return _UserAgent_IE9_x64;
            }
            else if(random_num==1)
            {
                return _UserAgent_Chrome;
            }
            else if(random_num==2)
            {
                 return _UserAgent_Firefox;
            }
            else if(random_num==3)
            {
                return _UserAgent_Edge;
            }
            else
            {
                return _UserAgent_IE8_x64;
            }
            #endregion
        }

        public HttpWebResponse GetUrlResponse(string url,int timeout = m_def_timeout,int readWriteTimeout = m_def_readwrite_timeout)
        {
            HttpWebResponse resp = null;
            HttpWebRequest req = (HttpWebRequest)WebRequest.Create(url);
            req.AllowAutoRedirect = true;
            req.Accept = "*/*";
            req.KeepAlive = true;
            req.UserAgent =GetRandomUserAgent();
            req.Headers["Accept-Encoding"] = "gzip, deflate";
            req.AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate;
            req.Proxy = null;
            if (timeout > 0)
            {
                req.Timeout = timeout;
            }
            if (readWriteTimeout > 0)
            {
                req.ReadWriteTimeout = readWriteTimeout;
            }
            req.Method = "GET";
            if (req != null)
            {
                try
                {
                    resp = (HttpWebResponse)req.GetResponse();
                }
                catch (WebException webEx)
                {
                    if (webEx.Status == WebExceptionStatus.Timeout)
                    {
                        resp = null;
                    }
                }
            }
            return resp;
        }
    
        public void GetUrlResponseSync(string url,int timeout = m_def_timeout,int readWriteTimeout = m_def_readwrite_timeout)
        {
            Thread http_thread=new Thread(new ParameterizedThreadStart(GetUrlResp));
            http_thread.IsBackground = true;
            object paraObj = new object[] { url, timeout, readWriteTimeout };
            http_thread.Start(paraObj);
        }

        private void GetUrlResp(object paraObj)
        {
            object[] paraObj_js = (object[])paraObj;
            string url = (string)paraObj_js[0];
            int timeout = (int)paraObj_js[1];
            int readWriteTimeout = (int)paraObj_js[2];
            HttpWebResponse httpWebResponse = GetUrlResponse(url, timeout, readWriteTimeout);
            HttpEvent(httpWebResponse);
        }
    }
}