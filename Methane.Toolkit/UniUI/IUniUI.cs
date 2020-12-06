using System;

namespace UniUI
{
    public interface IUniUI
    {
        void Log(string s);
        void LogAppend(string s, int numberOfLines = 0);
        void LogSpecial(string s);
        void LogError(Exception ex, string Msg = "");

        string Prompt(string s);

        void SetStatus(string s);

        void Clear();

        void Hold();
        void Unhold();
    }

    public class NoUI : IUniUI
    {
        public void Clear()
        {

        }

        public void Hold()
        {

        }

        public void Log(string s)
        {
            Console.WriteLine(s);
        }

        public void LogAppend(string s, int numberOfLines = 0)
        {
            Console.Write(s);
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

        public string Prompt(string s)
        {
            return "";
        }

        public void SetStatus(string s)
        {

        }

        public void Unhold()
        {

        }
    }
}
