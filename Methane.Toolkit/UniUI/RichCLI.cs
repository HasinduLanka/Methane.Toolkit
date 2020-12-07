using System;
using System.Collections.Generic;
using System.ComponentModel.Design;
using System.IO;
using System.Text;

namespace UniUI
{
    public class RichCLI : UniUI.IUniUI
    {
        public UniUI.IUniUI CLI;
        public RichCLI(UniUI.IUniUI cli)
        {
            CLI = cli;
        }

        private bool UIFromCmds = false;
        private bool UIRecord = false;
        private string UIRecordFile;

        private IEnumerator<string> Commands;


        public void SwitchToCommandFile(string file)
        {
            // fileLineReader?.ReadFileLineByLine().Dispose();


            if (string.IsNullOrEmpty(file)) file = "cmd.in";

            if (!File.Exists(file)) { Log(file + " not found. Failed to enter Command File Mode"); return; }

            try
            {
                FileLineReader fileLineReader = new FileLineReader(file);
                Commands = fileLineReader.ReadFileLineByLine();
                UIFromCmds = true;

                LogSpecial("Switched to Command file " + file);
            }
            catch (Exception ex)
            {
                LogError(ex, "cannot Switch to Command File Mode");
            }
        }

        public void SwitchToCommands(string cmds)
        {
            UIFromCmds = true;


            if (cmds.Length > 2 && cmds.StartsWith('#'))
            {
                Commands = ReadLinesSeperated(cmds, 2, cmds[1]);
            }
            else
            {
                Commands = ReadLineByLine(cmds);
            }


        }

        private IEnumerator<string> ReadLineByLine(string cmds)
        {


            int last = 0;
            for (int n = 0; n < cmds.Length; n++)
            {
                char c = cmds[n];
                if (c == '\n')
                {
                    if (n > 0 && cmds[n - 1] == '\r')
                    {
                        yield return cmds[last..(n - 1)];
                    }
                    else
                    {
                        yield return cmds[last..n];
                    }

                    last = n + 1;

                }
            }
            if (last < cmds.Length)
                yield return cmds[last..cmds.Length];
        }


        private IEnumerator<string> ReadLinesSeperated(string cmds, int startFrom, char seperator)
        {

            int last = startFrom;
            for (int n = startFrom; n < cmds.Length; n++)
            {
                char c = cmds[n];
                if (c == seperator)
                {
                    yield return cmds[last..n];

                    last = n + 1;
                }
            }
            if (last < cmds.Length)
                yield return cmds[last..cmds.Length];
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
            if (UIFromCmds)
            {
                if (!Commands.MoveNext())
                {
                    Log(" ___ Command set is over ___");
                    Log("Switching back to Console mode");
                    Commands.Dispose();
                    UIFromCmds = false;
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

        public void LogAppend(string s, int numberOfLines = 0)
        {
            CLI.LogAppend(s, numberOfLines);
        }

        public void SetStatus(string s)
        {
            CLI.SetStatus(s);
        }

        public void Clear()
        {
            CLI.Clear();
        }

        public void Hold()
        {
            CLI.Hold();
        }

        public void Unhold()
        {
            CLI.Unhold();
        }


        public ILab Lab { get => lab; set => lab = value; }
        private ILab lab;
    }
}
