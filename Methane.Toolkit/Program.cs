using System;
using static Methane.Toolkit.Common;

namespace Methane.Toolkit
{
    class Program
    {
        static void Main(string[] args)
        {

            _ = args;

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
            Log("1.  BFLG - BruteForce List Generator");
            Log("2.  CSA - Complex String Assembler");
            Log("3.  RapidPOSTer - HTTP POST form data in bulk");
            Log("4.  TestPOST - HTTP POST single request");
            Log("5.  RapidGETer - HTTP GET in bulk");
            Log("GP. BulkGETPOST - Download, alt and POST form data. (Code it)");

            string subProgram = Prompt("Enter Index to begin");

            switch (subProgram)
            {
                case "1":
                    RunBFLG();
                    break;

                case "2":
                    RunCSA();
                    break;

                case "3":
                    RunRapidPOSTer();
                    break;

                case "4":
                    RunTestPOST();
                    break;

                case "5":
                    RunRapidGETer();
                    break;

                case "GP":
                    RunGETPOST();
                    break;

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
