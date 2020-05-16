using System;
using System.IO;
using static Methane.Toolkit.Common;

namespace Methane.Toolkit
{
    class Program
    {
        public static string workspacePath = "workspace";
        public static void Main(string[] args)
        {

            foreach (string arg in args)
            {
                Log(arg);
            }

            if (args != null && args.Length > 0)
            {
                if (File.Exists(args[0]))
                {
                    Common.SwitchToCommandFile(args[0]);
                }
            }
            else
            {
                Directory.CreateDirectory(workspacePath);
                Environment.CurrentDirectory = workspacePath;
            }

            ProgramBegin:


            Log("                .                  ");
            Log("                .                  ");
            Log("              . . .                ");
            Log("         ................          ");
            Log("-----------------------------------");
            Log("||             ...               ||");
            Log("||           Methane             ||");
            Log("||       Ethical Hacking         ||");
            Log("||          Tool Kit             ||");
            Log("||             ...               ||");
            Log("-----------------------------------");
            Log("         ................          ");
            Log("              . . .                ");
            Log("                .                  ");
            Log("                .                  ");
            Log("");

            selectSubProgram:
            Log("1.bfgl \t\t BFLG - BruteForce List Generator");
            Log("2.csa \t\t CSA - Complex String Assembler");
            Log("3.post \t\t RapidPOSTer - HTTP POST form data in bulk");
            Log("4.testpost \t TestPOST - HTTP POST single request");
            Log("5.get \t\t RapidGETer - HTTP GET in bulk");
            Log("6.download \t RapidDownloader - HTTP Download in bulk");
            Log("7.fileops \t RapidFileOps - Do file operations in bulk");
            Log("GP. BulkGETPOST - Download, alt and POST form data. (Code it)");
            Log("c. Run Command file");
            Log("r. Record Command file");
            Log("er. End Recording Command file");

            string subProgram = Prompt("Enter Index to begin");

            switch (subProgram.ToLower())
            {
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

                case "c":
                    Common.SwitchToCommandFile(Prompt("Enter input command file name (nothing to use cmd.in)"));
                    goto ProgramBegin;

                case "r":
                    Common.BeginUIRecord(Prompt("Enter recording command file name (nothing to use cmd.in)"));
                    Log("\n \n Always use string indexes for recorded files. This will Help your record to be compatible with updates \n ");
                    goto ProgramBegin;

                case "er":
                    Common.EndUIRecord();
                    goto ProgramBegin;

                default:
                    goto selectSubProgram;

            }


            if (Prompt("Program End. Restart? [y]|[N]") != "y") return; ;
            goto ProgramBegin;

        }

        private static void RunBFLG()
        {
            BFLG bfgl = new BFLG
            {
                UseFile = Prompt("Write results to [C]onsole window or to a [f]ile").ToUpper() == "F"
            };

            bfgl.PrompParamenters();
            bfgl.BuildFromParamenters();

            if (bfgl.UseFile)
            {
                bfgl.GenerateToFile();
            }
            else
            {
                foreach (string key in bfgl.GenerateEnumerable())
                {
                    Log(key);
                }
            }
        }

        private static void RunCSA()
        {
            CSA csa = new CSA();
            csa.PromptParamenters();

            foreach (string s in csa.RunEnumerable())
            {
                Log(s);
            }
        }

        private static void RunRapidPOSTer()
        {
            RapidPOSTer poster = new RapidPOSTer();
            poster.PromptParamenters();
            poster.Run();
        }

        private static void RunRapidGETer()
        {
            RapidGETer geter = new RapidGETer();
            geter.PromptParamenters();
            geter.Run();
        }

        private static void RunRapidDownloader()
        {
            RapidDownloader downloader = new RapidDownloader();
            downloader.PromptParamenters();
            downloader.Run();
        }
        private static void RunRapidFileOps()
        {
            FileOperation ops = new FileOperation();
            ops.PromptParamenters();
            ops.Run();
        }

        private static void RunTestPOST()
        {
            Log("TestPOST - HTTP POST single request");
            Log(RapidPOSTer.PostForm(Prompt("Enter URL"), Prompt("Enter POST body"), Prompt("Enter Cookies (If any)"), Prompt("Enter headers (If any)")));
        }


        private static void RunGETPOST()
        {
            BulkGETPOST bgp = new BulkGETPOST();
            bgp.Run();
        }

    }
}
