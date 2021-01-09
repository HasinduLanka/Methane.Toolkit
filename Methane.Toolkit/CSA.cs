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
    public class CSA : IPipe
    {
        //Remember to set this if you JSONCopy it
        [System.Text.Json.Serialization.JsonIgnore] public UniUI.IUniUI UI { get; set; }
        public CSA(UniUI.IUniUI ui)
        {
            UI = ui;
        }

        public CSA()
        {
            UI = new UniUI.NoUI();
        }

        bool UsePipes;
        public List<BFLG> bflgs { get; set; }
        public List<FileLineReader> DicFiles { get; set; }
        public List<IncrementalIntGen> IncrementalInts { get; set; }

        public string mask { get; set; }

        public bool IsBuilt { get; set; }

        public List<CSAFeed> Feeds { get; set; }

        public void PromptParameters()
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

                        BFLG bflg = UI.Lab?.Request<BFLG>();
                        if (bflg == null)
                        {
                            bflg = new(UI);
                            bflg.PromptParameters();
                        }

                        bflg.BuildFromParameters();

                        bflgs.Add(bflg);

                        if (UI.Prompt("Add another BFLG? [y]|[N]").ToUpper() == "Y")
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

                    UI.Log($"File feed *f{DicFiles.Count}*");
                    FileLineReader FLR = new FileLineReader(UI);
                    FLR.PromptParameters();
                    DicFiles.Add(FLR);

                    if (UI.Prompt("Add another Dictionary pipeline? [y]|[N]").ToUpper() == "Y")
                    {
                        goto PromptFileName;
                    }


                    UI.Log($"{DicFiles.Count} Files ready");
                }


                IncrementalInts = null;
                if (UI.Prompt("Add Incremental 64bit Integer pipelines?  [y|N]").ToUpper() == "Y")
                {
                    IncrementalInts = new List<IncrementalIntGen>();

                PromptStart:
                    UI.Log($"Creating Incremental Int pipeline *i{IncrementalInts.Count}*");

                    IncrementalIntGen IIG = new IncrementalIntGen(UI);
                    IIG.PromptParameters();
                    IncrementalInts.Add(IIG);

                    if (UI.Prompt("Add another Incremental Int pipeline? [y]|[N]").ToUpper() == "Y")
                    {
                        goto PromptStart;
                    }


                }

                mask = UI.Prompt("Enter Mask string. (Substitutions : *b0* for BFLG0, *f0* for File0 ...) (Ex: 'user=*f0*&pass=ABC*b0*XYZ*f1*' )");
            }


        }


        public void BuildFromParameters()
        {
            if (!IsBuilt)
            {
                GenFeeds();

                IsBuilt = true;
            }
        }
        public void RunService()
        {

        }

        public IEnumerator<string> RunIterator()
        {
            return GenerateEnumerable().GetEnumerator();

        }
        public IEnumerable<string> GenerateEnumerable()
        {


            string CMask = mask;
            if (Feeds.Count > 0)
                return ReOccuringFeedHierachy(0, CMask);
            else
                return ReturnPlainText();
        }


        private void GenFeeds()
        {
            Feeds = new List<CSAFeed>();

            if (DicFiles != null)
                for (int nfile = 0; nfile < DicFiles.Count; nfile++)
                {

                    Feeds.Add(new CSAFeed($"*f{nfile}*", DicFiles[nfile]));
                }

            if (bflgs != null)
                for (int nBFLG = 0; nBFLG < bflgs.Count; nBFLG++)
                {
                    Feeds.Add(new CSAFeed($"*b{nBFLG}*", bflgs[nBFLG]));
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
            var Pipe = feed.Pipe.RunIterator();

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
                yield break;
                // if (nFeed != 0)
                // {
                //     // yield return UpMask;
                //     yield break;

                // }
                // else
                // {
                //     //End of Top Feed
                //     yield break;
                // }
            }




        }






        public IWorkerType WorkerType => IWorkerType.PipeReusable;
        public string JSONCopy()
        {
            return Core.ToJSON(this);
        }
    }






    public class CSAFeed
    {
        public string Mask { get; set; }
        // public IEnumerator<string> PipeCreator { get; set; }
        public IPipe Pipe { get; set; }



        public CSAFeed() { }

        public CSAFeed(string mask, IPipe pipeCreator)
        {
            Mask = mask;
            Pipe = pipeCreator;
        }


    }

}
