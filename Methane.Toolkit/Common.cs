using System;
using System.Collections.Generic;
using System.ComponentModel.Design;
using System.IO;
using System.Text;

namespace Methane.Toolkit
{
    public static class Common
    {
        private static bool UIFromFile = false;
        private static bool UIRecord = false;
        private static string UIRecordFile;

        private static FileLineReader fileLineReader;
        private static IEnumerator<string> Commands;

        public static void SwitchToCommandFile(string file)
        {
            fileLineReader?.ReadFileLineByLine().Dispose();


            if (string.IsNullOrEmpty(file)) file = "cmd.in";

            if (!File.Exists(file)) { Log(file + " not found. Failed to enter Command File Mode"); return; }

            try
            {
                fileLineReader = new FileLineReader(file);
                Commands = fileLineReader.ReadFileLineByLine();
                UIFromFile = true;

                LogSpecial("Switched to Command file " + file);
            }
            catch (Exception ex)
            {
                LogError(ex, "cannot Switch to Command File Mode");
            }
        }

        public static void BeginUIRecord(string file)
        {
            if (string.IsNullOrEmpty(file)) file = "cmd.in";
            if (File.Exists(file))
            {
                Log(file + " Exists. Overwriting...");
                File.Delete(file);
            }

            UIRecordFile = file;
            UIRecord = true;
        }
        public static void EndUIRecord()
        {
            UIRecord = false;
        }

        public static void Log(string s)
        {
            //string ThrName = System.Threading.Thread.CurrentThread.Name;

            // string LogS = $"{TimeStamp} \t {s} \t {(ThrName == null ? null : $"@ {ThrName}")}";

            //Console.WriteLine(LogS);
            Console.WriteLine(s);

        }

        public static void LogSpecial(string s)
        {
            Log(s);

            try
            {
                File.AppendAllText($"SpecialLog{System.Threading.Thread.CurrentThread.ManagedThreadId}.txt", s + Environment.NewLine);
            }
            catch (Exception)
            {
            }
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

            string LogS = $"{TimeStamp} \t {Msg} {ex} - {ex.Message} @@ {ex.StackTrace} \t {(ThrName == null ? null : $"@ {ThrName}")}";

            Console.ForegroundColor = ConsoleColor.Red;
            Console.BackgroundColor = ConsoleColor.DarkYellow;
            Console.WriteLine(LogS);
            Console.ResetColor();

        }

        public static string Prompt(string s)
        {
            if (UIFromFile)
            {
                if (!Commands.MoveNext())
                {
                    Console.WriteLine(" ___ Command file is over... Switching back to Console mode ___ ");
                    Commands.Dispose();
                    UIFromFile = false;
                    return Prompt(s);
                }

                Console.WriteLine($"\t \t >-> {s} ");
                string cmd = Commands.Current;
                Console.WriteLine($"\t \t <-< {cmd} ");
                return cmd;

            }
            else
            {
                Console.WriteLine($"\t \t {s} ");
                Console.Write("\t >>> \t");
                string cmd = Console.ReadLine();

                if (UIRecord)
                {
                    try
                    {
                        File.AppendAllText(UIRecordFile, cmd + Environment.NewLine);
                    }
                    catch (Exception ex)
                    {
                        LogError(ex, $"Failed to record command << {s}");
                    }
                }

                return cmd;
            }
        }

        public static string TimeStamp { get { return $"{DateTime.Now.Hour:00}:{DateTime.Now.Minute:00}:{DateTime.Now.Second:00)}.{DateTime.Now.Millisecond:00}"; } }

        public static Random random = new Random();
    }
}
