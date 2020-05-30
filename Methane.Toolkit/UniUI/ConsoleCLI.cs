using System;
using System.Collections.Generic;
using System.Text;

namespace UniUI
{
    public class ConsoleCLI : UniUI.IUniUI
    {
        public void Clear()
        {
            Console.Clear();
        }


        readonly StringBuilder Cache = new StringBuilder();
        int CacheSize = 0;
        public int MaxCacheSize = 10000;

        public void Log(string s)
        {
            CacheSize++;
            while (LazyUpdating && CacheSize > MaxCacheSize)
                System.Threading.Thread.Sleep(100);

            lock (Cache)
            {
                Cache.AppendLine(s);
            }

            if (!LazyUpdating) new System.Threading.Thread(LazyUpdate).Start();
        }

        public void LogAppend(string s, int numberOfLines = 0)
        {
            CacheSize += numberOfLines;
            while (LazyUpdating && CacheSize > MaxCacheSize)
                System.Threading.Thread.Sleep(100);

            lock (Cache)
            {
                Cache.Append(s);
            }

            if (!LazyUpdating) new System.Threading.Thread(LazyUpdate).Start();
        }


        bool LazyUpdating = false;
        void LazyUpdate()
        {
            if (LazyUpdating) return;

            LazyUpdating = true;

            if (CacheSize < MaxCacheSize)
                System.Threading.Thread.Sleep(500);


            string s;
            lock (Cache)
            {
                s = Cache.ToString();
                Cache.Clear();
                CacheSize = 0;
            }

            Console.Write(s);


            LazyUpdating = false;

        }


        public string Prompt(string s)
        {
            Log($"\t \t {s} ");
            LogAppend("\t >>> \t");
            return Console.ReadLine();
        }

        public void SetStatus(string s)
        {
            Console.Title = s;
        }


        public void Hold()
        {

        }
        public void Unhold()
        {

        }
        public void LogSpecial(string s)
        {
            Log("-_-_-_-_-_-_-_-\n" +
                s +
                "\n_-_-_-_-_-_-_-_-");
        }

        public void LogError(Exception ex, string Msg = "")
        {
            string ThrName = System.Threading.Thread.CurrentThread.Name;

            string LogS = $"{TimeStamp} \t {Msg} {ex} - {ex?.Message} @@ {ex?.StackTrace} \t {(ThrName == null ? null : $"@ {ThrName}")}";

            Console.ForegroundColor = ConsoleColor.Red;
            Console.BackgroundColor = ConsoleColor.DarkYellow;
            Log(LogS);
            Console.ResetColor();
        }


        public static string TimeStamp { get { return $"{DateTime.Now.Hour:00}:{DateTime.Now.Minute:00}:{DateTime.Now.Second:00)}.{DateTime.Now.Millisecond:00}"; } }

    }
}
