using System;
using System.Collections.Generic;
using System.ComponentModel.Design;
using System.IO;
using System.Text;

namespace Methane.Toolkit
{
    public class UI
    {
        public UniUI.IUniCLI CLI;
        public UI(UniUI.IUniCLI cli)
        {
            CLI = cli;
        }

        private bool UIFromFile = false;
        private bool UIRecord = false;
        private string UIRecordFile;

        private FileLineReader fileLineReader;
        private IEnumerator<string> Commands;


        public void SwitchToCommandFile(string file)
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

        public void BeginUIRecord(string file)
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
        public void EndUIRecord()
        {
            UIRecord = false;
        }

        public void Log(string s) => CLI.Log(s);

        public void LogSpecial(string s) => CLI.LogSpecial(s);


        public void LogError(Exception ex, string Msg = null) => CLI.LogError(ex, Msg);

        public string Prompt(string s)
        {
            if (UIFromFile)
            {
                if (!Commands.MoveNext())
                {
                    Log(" ___ Command file is over... Switching back to Console mode ___ ");
                    Commands.Dispose();
                    UIFromFile = false;
                    return Prompt(s);
                }

                Log($"\t \t >-> {s} ");
                string cmd = Commands.Current;
                Log($"\t \t <-< {cmd} ");
                return cmd;

            }
            else
            {

                string ans = CLI.Prompt(s);

                if (UIRecord)
                {
                    try
                    {
                        File.AppendAllText(UIRecordFile, ans + Environment.NewLine);
                    }
                    catch (Exception ex)
                    {
                        LogError(ex, $"Failed to record command << {s}");
                    }
                }

                return ans;
            }
        }

        public string TimeStamp { get { return $"{DateTime.Now.Hour:00}:{DateTime.Now.Minute:00}:{DateTime.Now.Second:00)}.{DateTime.Now.Millisecond:00}"; } }

        public Random random = new Random();
    }
}
