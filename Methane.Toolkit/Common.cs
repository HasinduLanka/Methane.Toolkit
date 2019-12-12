using System;
using System.Collections.Generic;
using System.Text;

namespace Methane.Toolkit
{
    public static class Common
    {
        public static void Log(string s)
        {
            string ThrName = System.Threading.Thread.CurrentThread.Name;

            // string LogS = $"{TimeStamp} \t {s} \t {(ThrName == null ? null : $"@ {ThrName}")}";

            //Console.WriteLine(LogS);
            Console.WriteLine(s);

        }

        public static void LogError(string s)
        {
            string ThrName = System.Threading.Thread.CurrentThread.Name;

            string LogS = $"{TimeStamp} \t {s} \t {(ThrName == null ? null : $"@ {ThrName}")}";

            Console.ForegroundColor = ConsoleColor.Red;
            Console.BackgroundColor = ConsoleColor.DarkYellow;
            Console.WriteLine(LogS);
            Console.ResetColor();

        }

        public static void LogError(Exception ex, string Msg = null)
        {
            string ThrName = System.Threading.Thread.CurrentThread.Name;

            string LogS = $"{TimeStamp} \t {Msg} {ex.ToString()} - {ex.Message} @@ {ex.StackTrace} \t {(ThrName == null ? null : $"@ {ThrName}")}";

            Console.ForegroundColor = ConsoleColor.Red;
            Console.BackgroundColor = ConsoleColor.DarkYellow;
            Console.WriteLine(LogS);
            Console.ResetColor();

        }

        public static string Prompt(string s)
        {
            Console.WriteLine($"\t \t {s} ");
            Console.Write("\t >>> \t");
            return Console.ReadLine();
        }

        public static string TimeStamp { get { return $"{DateTime.Now.Hour.ToString("00")}:{DateTime.Now.Minute.ToString("00")}:{DateTime.Now.Second.ToString("00")}.{DateTime.Now.Millisecond.ToString("00")}"; } }

        public static Random random = new Random();
    }
}
