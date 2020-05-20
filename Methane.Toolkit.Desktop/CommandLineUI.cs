using System;
using System.Collections.Generic;
using System.Text;

namespace UniUI
{
    public class CommandLineUI : UniUI.IUniCLI
    {
        public void Clear()
        {
            Console.Clear();
        }



        public void Log(string s)
        {
            Console.WriteLine(s);
        }
        public void LogAppend(string s)
        {
            Console.Write(s);
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
