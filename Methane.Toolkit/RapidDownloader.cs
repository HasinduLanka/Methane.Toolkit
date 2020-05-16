using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using static Methane.Toolkit.Common;
using System.Text;
using HtmlAgilityPack;
using System.Net.Http;
using System.Resources;

namespace Methane.Toolkit
{
    public class RapidDownloader
    {

        public bool HasCookie = false;
        public bool HasHeaders = false;
        public string Cookie;
        public string Headers;
        public string SavePath;

        public CSA csa;
        IEnumerator<string> bodyPipeline;

        public int runningThreads = 0;
        public int AllowedThrds;

        public void PromptParamenters()
        {

            Log("");
            Log("    . . . . . . . . . . . . . . . .  .  ");
            Log("               Methane                  ");
            Log("            Bulk HTTP Download          ");
            Log("    . . . . . . . . . . . . . . . .  .  ");
            Log("");


            ChooseCookies:
            Cookie = Prompt("Enter cookies to use OR Enter ~filename to read cookies OR [Enter] not to use cookies");

            HasCookie = Cookie.Length > 0;
            if (Cookie.StartsWith('~'))
            {
                if (Cookie == "~") Cookie = "~cookie.txt";
                try
                {
                    Cookie = File.ReadAllText(Cookie.TrimStart('~'));
                }
                catch (Exception ex)
                {
                    LogError(ex);
                    goto ChooseCookies;
                }
            }


            ChooseHeaders:
            Headers = Prompt("Enter Headers to use OR Enter ~filename to read Headers OR [Enter] not to use Headers");

            HasHeaders = Headers.Length != 0;
            if (Headers.StartsWith('~'))
            {
                if (Headers == "~") Headers = "~headers.txt";
                try
                {
                    Headers = File.ReadAllText(Headers.TrimStart('~'));
                }
                catch (Exception ex)
                {
                    LogError(ex);
                    goto ChooseHeaders;
                }
            }


            ChooseSavePath:

            SavePath = Prompt("Enter path to save files or ~ to use  \"Downloaded/\" ");
            if (SavePath == "~") SavePath = "Downloaded/";

            try
            {
                if (!string.IsNullOrEmpty(SavePath))
                {
                    Directory.CreateDirectory(SavePath);
                    if (!SavePath.EndsWith('/')) SavePath += '/';
                }
            }
            catch (Exception ex)
            {
                LogError(ex);
                goto ChooseSavePath;
            }



            Log("Please use the following tool to create download url list");
            csa = new CSA();
            csa.PromptParamenters();


            PromptHowManyThreads:
            if (!int.TryParse(Prompt("How many threads to use?"), out AllowedThrds)) goto PromptHowManyThreads;

            Log("Rapid Downloader standby    :-) ");


        }


        public void Run()
        {

            Log("");
            Log("    . . . . . . . . . . . . . . . .  .  ");
            Log("               Methane                  ");
            Log("            Bulk HTTP Download          ");
            Log("    . . . . . . . . . . . . . . . .  .  ");
            Log("");



            Log("Rapid Downloader Running...");


            bodyPipeline = csa.RunIterator();

            for (int n = 0; n < AllowedThrds; n++)
            {
                new System.Threading.Thread(new System.Threading.ThreadStart(ThrLoop)) { Name = $"DWN Thrd {n}" }.Start();
                System.Threading.Thread.Sleep(1000);
            }


            while (runningThreads > 0)
            {
                System.Threading.Thread.Sleep(1000);
            }





        }


        private void ThrLoop()
        {

            runningThreads++;

            bodyPipelineMoveNext:

            string url;

            lock (bodyPipeline)
            {
                if (bodyPipeline.MoveNext())
                {
                    url = bodyPipeline.Current;
                }
                else
                {
                    runningThreads--;
                    return;
                }
            }

            try
            {
                Stream resp = GetStream(url);

                int LastSlash = url.LastIndexOf('/') + 1;
                if (LastSlash == url.Length) LastSlash--;
                string url2 = url.Substring(LastSlash);

                int Q = url2.IndexOf('?') + 1;
                if (Q == 0) Q = url2.Length;

                string url3;
                if (url2.Length == 0) url3 = "DownloadedFile";
                else url3 = url2.Substring(0, Q);

                string filename = Path.GetFileNameWithoutExtension(url3);
                string ext = Path.GetExtension(url3) ?? "";
                string file = SavePath + filename + ext;
               
                FileStream fs = File.OpenWrite(file);
                resp.CopyTo(fs);
                fs.Dispose();


                Log($"Downloaded \t {url} \t -> \t {file}");


            }
            catch (Exception ex)
            {
                Log($"!!! Error {ex} - {ex.Message} @ {ex.StackTrace}");
            }

            goto bodyPipelineMoveNext;




        }


        public static Stream GetStream(string url, string Cookie = "", string Headers = "")
        {
            var wr = WebRequest.CreateHttp(url);
            wr.Method = "GET";

            if (Cookie.Length != 0) wr.Headers.Add(HttpRequestHeader.Cookie, Cookie);
            if (Headers.Length != 0)
                foreach (string header in Headers.Split("\n"))
                    wr.Headers.Add(header);

            // Get the response from remote server
            HttpWebResponse httpWebResponse = (HttpWebResponse)wr.GetResponse();
            Stream responseStream = httpWebResponse.GetResponseStream();

            return responseStream;
        }



    }

}