using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Text;
using HtmlAgilityPack;
using System.Net.Http;
using System.Resources;
using UniUI;

namespace Methane.Toolkit
{
    public class RapidDownloader : IWorker
    {
        [System.Text.Json.Serialization.JsonIgnore] public UniUI.IUniUI UI { get; set; }

        public RapidDownloader(UniUI.IUniUI ui)
        {
            UI = ui;
        }

        public RapidDownloader()
        {
            UI = new UniUI.NoUI();
        }

        public bool HasCookie = false;
        public bool HasHeaders = false;
        public string Cookie;
        public string Headers;
        public string SavePath;

        public IPipe csa;
        IEnumerator<string> bodyPipeline;

        public int runningThreads = 0;
        public int AllowedThrds;

        public int RetryCount;


        public void PromptParameters()
        {

            UI.Log("");
            UI.Log("    . . . . . . . . . . . . . . . .  .  ");
            UI.Log("               Methane                  ");
            UI.Log("            Bulk HTTP Download          ");
            UI.Log("    . . . . . . . . . . . . . . . .  .  ");
            UI.Log("");


        ChooseCookies:
            Cookie = UI.Prompt("Enter cookies to use OR Enter ~filename to read cookies OR [Enter] not to use cookies");

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
                    UI.LogError(ex);
                    goto ChooseCookies;
                }
            }


        ChooseHeaders:
            Headers = UI.Prompt("Enter Headers to use OR Enter ~filename to read Headers OR [Enter] not to use Headers");

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
                    UI.LogError(ex);
                    goto ChooseHeaders;
                }
            }


        ChooseSavePath:

            SavePath = UI.Prompt("Enter path to save files or ~ to use  \"Downloaded/\" ");
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
                UI.LogError(ex);
                goto ChooseSavePath;
            }



            if (csa == null)
            {
                UI.Log("Please use the following tool to create download url list. \n"
                 + " File name suffixes can be inserted at the end of urls inside { }   Ex: http://example.com/file.zip{ABC}\n"
                 + "These downloaded files will be named like fileABC.zip   (Tip : use pipelines for suffixes) \n");
                csa = UI.Lab?.Request<CSA>();
                if (csa == null)
                {
                    csa = new CSA(UI);
                    csa.PromptParameters();
                }
            }


        PromptHowManyThreads:
            if (!int.TryParse(UI.Prompt("How many threads to use?"), out AllowedThrds)) goto PromptHowManyThreads;

            PromptRetryCount:
            if (int.TryParse(UI.Prompt("If failed on network issue, How many times to retry?"), out int retries))
            {
                RetryCount = retries;
            }
            else
            {
                goto PromptRetryCount;
            }

            UI.Log("Rapid Downloader standby    :-) ");


        }

        public void BuildFromParameters()
        {
            csa?.BuildFromParameters();
        }


        public void RunService()
        {

            UI.Log("");
            UI.Log("    . . . . . . . . . . . . . . . .  .  ");
            UI.Log("               Methane                  ");
            UI.Log("            Bulk HTTP Download          ");
            UI.Log("    . . . . . . . . . . . . . . . .  .  ");
            UI.Log("");



            UI.Log("Rapid Downloader Running...");


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

            string pipeFeed;

            lock (bodyPipeline)
            {
                if (bodyPipeline.MoveNext())
                {
                    pipeFeed = bodyPipeline.Current;
                }
                else
                {
                    runningThreads--;
                    return;
                }
            }

            string file = "";
            string url = "";
            int retry = RetryCount;

        RetryDownload:
            try
            {


                url = pipeFeed;



                string urle = System.Web.HttpUtility.UrlPathEncode(url);
                Stream resp = GetStream(urle);

                file = SavePath + URLToFileName(url);

                FileStream fs = File.OpenWrite(file);
                resp.CopyTo(fs);
                fs.Dispose();


                UI.Log($"Downloaded \t {urle} \t -> \t {file}");


            }
            catch (Exception ex)
            {
                UI.Log($"!!! Error {ex} - {ex.Message} {url} -> {file} \n @ {ex.StackTrace}");
                retry--;
                if (retry > 0)
                {
                    UI.Log("Retrying...");
                    goto RetryDownload;
                }
            }

            goto bodyPipelineMoveNext;




        }

        public static string URLToFileName(string url)
        {
            string file;
            int LastSlash = url.LastIndexOf('/') + 1;
            if (LastSlash == url.Length) LastSlash--;
            string url2 = url.Substring(LastSlash);

            int Q = url2.IndexOf('?') + 1;
            if (Q == 0) Q = url2.Length;

            string url3;
            if (url2.Length == 0) url3 = "DownloadedFile";
            else url3 = url2.Substring(0, Q);

            string filename = Path.GetFileNameWithoutExtension(url3);


            if (filename.Length > 20)
            {
                string filename1 = filename.Substring(filename.Length - 10);
                string filename2 = filename.Substring(0, 10);
                filename = filename2 + filename1;
            }

            string ext = Path.GetExtension(url3) ?? "";
            file = filename + ext;
            return file;
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




        public IWorkerType WorkerType => IWorkerType.Service;


        public string JSONCopy()
        {
            return null;
        }
    }

}