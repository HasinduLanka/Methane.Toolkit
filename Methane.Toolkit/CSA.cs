using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using static Methane.Toolkit.Common;

namespace Methane.Toolkit
{
    /// <summary>
    /// Complex String Assembler
    /// </summary>
    public class CSA
    {

        bool UseBFLG;
        bool UseDicFile;
        public List<BFLG> bflgs;
        public List<FileLineReader> DicFiles;

        string mask = "user=*f0*&pass=*b0*&submit=";

        List<Feed> Feeds;


        public void PromptParamenters()
        {
            Log("");
            Log("    . . . . . . . . . . . . . . . .  .  ");
            Log("               Methane                  ");
            Log("       Complex String Assembler         ");
            Log("    . . . . . . . . . . . . . . . .  .  ");
            Log("");

            ChoosePassSource:
            string PassSource = Prompt("Which feeds to use? [g]enerate using Brute-Force Password Generator ? or Read from [f]ile ? or [b]oth ?");

            switch (PassSource)
            {
                case "g":
                    UseBFLG = true;
                    break;
                case "f":
                    UseDicFile = true;
                    break;
                case "b":
                    UseBFLG = true;
                    UseDicFile = true;
                    break;
                default:
                    Log("Please select from g|f|b");
                    goto ChoosePassSource;
            }


            if (UseBFLG)
            {
                bflgs = new List<BFLG>();

                Log("Please use the following tool to generate your BruteForce pipeline");

                SetupBFLG:
                try
                {
                    Log($"Create BFLG *b{bflgs.Count}*");

                    BFLG bflg = new BFLG();
                    bflg.PrompParamenters();
                    bflg.BuildFromParamenters();

                    bflgs.Add(bflg);

                    if (Prompt("Add another? [y]|[N]").ToUpper() == "Y")
                    {
                        goto SetupBFLG;
                    }

                }
                catch (Exception ex)
                {
                    LogError(ex);
                    if (Prompt("Try again? [y]|[N] ").ToUpper() == "Y")
                        goto SetupBFLG;
                }



                Log($"{bflgs.Count} Password pipelines standby");
            }


            if (UseDicFile)
            {
                DicFiles = new List<FileLineReader>();

                PromptFileName:
                string DicFileName = Prompt($"File feed *f{DicFiles.Count}*. Enter the filename to read : ");

                if (File.Exists(DicFileName))
                {
                    DicFiles.Add(new FileLineReader(DicFileName));

                    if (Prompt("Add another? [y]|[N]").ToUpper() == "Y")
                    {
                        goto PromptFileName;
                    }
                }
                else
                {
                    if (Prompt("That file does not exist. Try again? [y]|[N]").ToUpper() == "Y")
                        goto PromptFileName;
                }

                Log($"{DicFiles.Count} Files ready");
            }


            mask = Prompt("Enter Mask string. (WildCards : *b0* for BFLG0, *f0* for File0 ...) (Ex: 'user=*f0*&pass=ABC*b0*XYZ*f1*&submit=' )");

        }





        public IEnumerator<string> RunIterator()
        {

            Run();

            string CMask = mask;
            return ReOccuringFeedHierachy(0, CMask).GetEnumerator();

        }
        public IEnumerable<string> RunEnumerable()
        {

            Run();

            string CMask = mask;
            return ReOccuringFeedHierachy(0, CMask);

        }


        private void Run()
        {
            Feeds = new List<Feed>();
            List<int> nFeeds = new List<int>();

            if (UseDicFile)
            {
                for (int nfile = 0; nfile < DicFiles.Count; nfile++)
                {

                    Feeds.Add(new Feed($"*f{nfile}*", new Func<IEnumerator<string>>(DicFiles[nfile].ReadFileLineByLine)));
                    nFeeds.Add(0);
                }
            }



            if (UseBFLG)
            {
                for (int nBFLG = 0; nBFLG < bflgs.Count; nBFLG++)
                {
                    Feeds.Add(new Feed($"*b{nBFLG}*", new Func<IEnumerator<string>>(bflgs[nBFLG].GenerateIterator)));
                    nFeeds.Add(0);
                }
            }
        }

        private IEnumerable<string> ReOccuringFeedHierachy(int nFeed, string UpMask)
        {
            Feed feed = Feeds[nFeed];
            var Pipe = feed.PipeCreator();

            loop:

            if (Pipe.MoveNext())
            {
                string DownMask = UpMask.Replace(feed.Mask, Pipe.Current);

                if (nFeed != Feeds.Count - 1)
                {
                    foreach (string Result in ReOccuringFeedHierachy(nFeed + 1, DownMask))
                    {
                        yield return Result;
                    }
                }
                else
                {
                    //Last feed
                    yield return DownMask;
                }

                goto loop;

            }
            else
            {
                if (nFeed != 0)
                {
                    //Pipe = feed.PipeCreator();
                    //goto loop;
                    yield break;
                }
                else
                {
                    //End of Top Feed
                    yield break;
                }
            }




        }


        public class Feed
        {
            public string Mask;
            public Func<IEnumerator<string>> PipeCreator;



            public Feed() { }

            public Feed(string mask, Func<IEnumerator<string>> pipeCreator)
            {
                Mask = mask;
                PipeCreator = pipeCreator;
            }
        }

    }
}
