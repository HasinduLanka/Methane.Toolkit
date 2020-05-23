using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using UniUI;

namespace Methane.Toolkit
{
    /// <summary>
    /// Complex String Assembler
    /// </summary>
    public class CSA
    {
        readonly UniUI.IUniCLI UI;
        public CSA(UniUI.IUniCLI ui)
        {
            UI = ui;
        }


        bool UsePipes;
        public List<BFLG> bflgs;
        public List<FileLineReader> DicFiles;
        public List<IEnumerator<string>> IncrementalInts;

        string mask;

        List<CSAFeed> Feeds;


        public void PromptParamenters()
        {
            UI.Log("");
            UI.Log("    . . . . . . . . . . . . . . . .  .  ");
            UI.Log("               Methane                  ");
            UI.Log("       Complex String Assembler         ");
            UI.Log("    . . . . . . . . . . . . . . . .  .  ");
            UI.Log("");


            UI.Log("There are 3 pipeline types to use as substitutions Brute-Force, Dictionary file, Incremental Integers. You can add multiple pipelines from any type");
            mask = UI.Prompt("Just press [Enter] to use these pipelines OR Enter the plain text to use");
            UsePipes = string.IsNullOrEmpty(mask);

            if (UsePipes)
            {

                bflgs = null;
                if (UI.Prompt("Add Brute-Force pipelines?  [y|N]").ToUpper() == "Y")
                {
                    bflgs = new List<BFLG>();

                    SetupBFLG:
                    try
                    {
                        UI.Log($"Creating BFLG *b{bflgs.Count}*");

                        BFLG bflg = new BFLG(UI);
                        bflg.PrompParamenters();
                        bflg.BuildFromParamenters();

                        bflgs.Add(bflg);

                        if (UI.Prompt("Add another? [y]|[N]").ToUpper() == "Y")
                        {
                            goto SetupBFLG;
                        }

                    }
                    catch (Exception ex)
                    {
                        UI.LogError(ex);
                        if (UI.Prompt("Try again? [y]|[N] ").ToUpper() == "Y")
                            goto SetupBFLG;
                    }



                    UI.Log($"{bflgs.Count} Password pipelines standby");
                }



                DicFiles = null;
                if (UI.Prompt("Add Dictionary pipelines?  [y|N]").ToUpper() == "Y")
                {
                    DicFiles = new List<FileLineReader>();

                    PromptFileName:
                    string DicFileName = UI.Prompt($"File feed *f{DicFiles.Count}*. Enter the filename to read : ");

                    if (File.Exists(DicFileName))
                    {
                        DicFiles.Add(new FileLineReader(DicFileName));

                        if (UI.Prompt("Add another? [y]|[N]").ToUpper() == "Y")
                        {
                            goto PromptFileName;
                        }
                    }
                    else
                    {
                        if (UI.Prompt("That file does not exist. Try again? [y]|[N]").ToUpper() == "Y")
                            goto PromptFileName;
                    }

                    UI.Log($"{DicFiles.Count} Files ready");
                }


                IncrementalInts = null;
                if (UI.Prompt("Add Incremental 64bit Integer pipelines?  [y|N]").ToUpper() == "Y")
                {
                    IncrementalInts = new List<IEnumerator<string>>();

                    PromptStart:

                    UI.Log($"Creating Incremental Int pipeline *i{IncrementalInts.Count}*");

                    if (!long.TryParse(UI.Prompt("Enter start integer"), out long start))
                    {
                        UI.Log("Can't read");
                        goto PromptStart;
                    }

                    PromptEnd:
                    if (!long.TryParse(UI.Prompt("Enter end integer (Inclusive)"), out long end))
                    {
                        UI.Log("Can't read");
                        goto PromptEnd;
                    }

                    IncrementalInts.Add(IncrementalInt(start, end).GetEnumerator());

                    if (UI.Prompt("Add another? [y]|[N]").ToUpper() == "Y")
                    {
                        goto PromptStart;
                    }


                }

                mask = UI.Prompt("Enter Mask string. (Substitutions : *b0* for BFLG0, *f0* for File0 ...) (Ex: 'user=*f0*&pass=ABC*b0*XYZ*f1*' )");
            }


        }





        public IEnumerator<string> RunIterator()
        {
            return RunEnumerable().GetEnumerator();

        }
        public IEnumerable<string> RunEnumerable()
        {

            Run();

            string CMask = mask;
            if (Feeds.Count > 0)
                return ReOccuringFeedHierachy(0, CMask);
            else
                return ReturnPlainText();
        }


        private void Run()
        {
            Feeds = new List<CSAFeed>();

            if (DicFiles != null)
                for (int nfile = 0; nfile < DicFiles.Count; nfile++)
                {

                    Feeds.Add(new CSAFeed($"*f{nfile}*", DicFiles[nfile].ReadFileLineByLine()));
                }

            if (bflgs != null)
                for (int nBFLG = 0; nBFLG < bflgs.Count; nBFLG++)
                {
                    Feeds.Add(new CSAFeed($"*b{nBFLG}*", bflgs[nBFLG].GenerateIterator()));
                }

            if (IncrementalInts != null)
                for (int nII = 0; nII < IncrementalInts.Count; nII++)
                {
                    Feeds.Add(new CSAFeed($"*i{nII}*", IncrementalInts[nII]));
                }


        }
        private IEnumerable<string> ReturnPlainText()
        {
            yield return mask;
        }

        private IEnumerable<string> ReOccuringFeedHierachy(int nFeed, string UpMask)
        {
            CSAFeed feed = Feeds[nFeed];
            var Pipe = feed.PipeCreator;

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
                    yield break;
                }
                else
                {
                    //End of Top Feed
                    yield break;
                }
            }




        }

        public static IEnumerable<string> IncrementalInt(long start, long end)
        {
            end++;
            for (long i = start; i < end; i++)
            {
                yield return i.ToString();
            }
        }



    }
    public class CSAFeed
    {
        public string Mask;
        public IEnumerator<string> PipeCreator;



        public CSAFeed() { }

        public CSAFeed(string mask, IEnumerator<string> pipeCreator)
        {
            Mask = mask;
            PipeCreator = pipeCreator;
        }
    }
}
