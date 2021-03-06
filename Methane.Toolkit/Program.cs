﻿using System;
using System.IO;


namespace Methane.Toolkit
{
    public class Program
    {
        public UniUI.RichCLI UI;


        public string workspacePath = "workspace";
        public void Main(string[] args, UniUI.IUniUI cli)
        {
            UI = new UniUI.RichCLI(cli);


            foreach (string arg in args)
            {
                UI.Log(arg);
            }

            if (args != null && args.Length > 0)
            {
                if (args[0].StartsWith("#"))
                {
                    UI.SwitchToCommands(args[0]);
                    Directory.CreateDirectory(workspacePath);
                    Environment.CurrentDirectory = workspacePath;
                }
                else if (File.Exists(args[0]))
                {
                    UI.SwitchToCommandFile(args[0]);
                }
            }
            else
            {
                Directory.CreateDirectory(workspacePath);
                Environment.CurrentDirectory = workspacePath;
            }

        ProgramBegin:


            UI.Log("              _ . _                ");
            UI.Log("                .                  ");
            UI.Log("              . . .                ");
            UI.Log("         ................          ");
            UI.Log("-----------------------------------");
            UI.Log("||             ...               ||");
            UI.Log("||           Methane             ||");
            UI.Log("||       Ethical Hacking         ||");
            UI.Log("||          Tool Kit             ||");
            UI.Log("||             ...               ||");
            UI.Log("-----------------------------------");
            UI.Log("         ................          ");
            UI.Log("              . . .                ");
            UI.Log("                .                  ");
            UI.Log("              _ . _                ");
            UI.Log("");

        selectSubProgram:
            UI.Log("L.lab \t\t Enter Automation Lab");
            UI.Log("1.bfgl \t\t BFLG - BruteForce List Generator");
            UI.Log("2.csa \t\t CSA - Complex String Assembler");
            UI.Log("3.post \t\t RapidPOSTer - HTTP POST form data in bulk");
            UI.Log("4.testpost \t TestPOST - HTTP POST single request");
            UI.Log("5.get \t\t RapidGETer - HTTP GET in bulk");
            UI.Log("6.download \t RapidDownloader - HTTP Download in bulk");
            UI.Log("7.fileops \t RapidFileOps - Do file operations in bulk");
            UI.Log("GP. BulkGETPOST - Download, alt and POST form data. (Code it)");
            UI.Log("t. Type and Run Recorded Command set in textbox");
            UI.Log("c. Run Command file");
            UI.Log("r. Record Command file");
            UI.Log("er. End Recording Command file");

            string subProgram = UI.Prompt("Enter Index to begin");

            switch (subProgram.ToLower())
            {
                case "l":
                case "lab":
                    RunLab();
                    break;

                case "1":
                case "bflg":
                    RunBFLG();
                    break;

                case "2":
                case "csa":
                    RunCSA();
                    break;

                case "3":
                case "post":
                    RunRapidPOSTer();
                    break;

                case "4":
                case "testpost":
                    RunTestPOST();
                    break;

                case "5":
                case "get":
                    RunRapidGETer();
                    break;

                case "6":
                case "download":
                    RunRapidDownloader();
                    break;

                case "7":
                case "fileops":
                    RunRapidFileOps();
                    break;

                case "GP":
                    RunGETPOST();
                    break;

                case "t":
                    UI.SwitchToCommands(UI.Prompt("Type or Paste your command set here (Include seperator after #  Ex: #|bflg|c)"));
                    goto ProgramBegin;

                case "c":
                    UI.SwitchToCommandFile(UI.Prompt("Enter input command file name (nothing to use cmd.in)"));
                    goto ProgramBegin;

                case "r":
                    UI.BeginUIRecord(UI.Prompt("Enter recording command file name (nothing to use cmd.in)"));
                    UI.Log("\n \n Always use string indexes for recorded files. This will Help your record to be compatible with updates \n ");
                    goto ProgramBegin;

                case "er":
                    UI.EndUIRecord();
                    goto ProgramBegin;

                default:
                    goto selectSubProgram;

            }


            if (UI.Prompt("Program End. Restart? [y]|[N]") != "y") return;
            goto ProgramBegin;

        }



        private void RunLab()
        {

            Lab lab = new Lab(UI);
            lab.PromptParameters();
            lab.BuildFromParameters();
            lab.RunService();
        }



        private void RunBFLG()
        {
            BFLG bfgl = new BFLG(UI)
            {
                UseFile = UI.Prompt("Write results to [C]onsole window or to a [f]ile").ToUpper() == "F"
            };

            bfgl.PromptParameters();
            bfgl.BuildFromParameters();

            if (bfgl.UseFile)
            {
                bfgl.GenerateToFile();
            }
            else
            {
                foreach (string key in bfgl.GenerateEnumerable())
                {
                    UI.Log(key);
                }
            }
        }

        private void RunCSA()
        {
            CSA csa = new CSA(UI);
            csa.PromptParameters();
            csa.BuildFromParameters();

            foreach (string s in csa.GenerateEnumerable())
            {
                UI.Log(s);
            }
        }

        private void RunRapidPOSTer()
        {
            RapidPOSTer poster = new RapidPOSTer(UI);
            poster.PromptParameters();
            poster.BuildFromParameters();
            poster.RunService();
        }

        private void RunRapidGETer()
        {
            RapidGETer geter = new RapidGETer(UI);
            geter.PromptParameters();
            geter.BuildFromParameters();
            geter.RunService();
        }

        private void RunRapidDownloader()
        {
            RapidDownloader downloader = new RapidDownloader(UI);
            downloader.PromptParameters();
            downloader.BuildFromParameters();
            downloader.RunService();
        }
        private void RunRapidFileOps()
        {
            FileOperation ops = new FileOperation(UI);
            ops.PromptParameters();
            ops.BuildFromParameters();
            ops.RunService();
        }

        private void RunTestPOST()
        {
            UI.Log("TestPOST - HTTP POST single request");
            UI.Log(RapidPOSTer.PostForm(UI.Prompt("Enter URL"), UI.Prompt("Enter POST body"), UI.Prompt("Enter Cookies (If any)"), UI.Prompt("Enter headers (If any)")));
        }


        private void RunGETPOST()
        {
            BulkGETPOST bgp = new BulkGETPOST(UI);
            bgp.RunService();
        }

    }
}
