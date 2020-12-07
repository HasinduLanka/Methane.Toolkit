using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Text;
using HtmlAgilityPack;
using System.Net.Http;
using System.Resources;
using UniUI;

namespace Methane.Toolkit
{
    public class PipePrinter : IWorker
    {
        [System.Text.Json.Serialization.JsonIgnore] public UniUI.IUniUI UI { get; set; }
        public IWorkerType WorkerType => IWorkerType.Service;

        public IPipe Pipe { get; set; }


        public PipePrinter()
        {

        }
        public PipePrinter(UniUI.IUniUI ui)
        {
            UI = ui;
        }

        public PipePrinter(IUniUI uI, IPipe pipe) : this(uI)
        {
            Pipe = pipe;
        }

        public void BuildFromParameters()
        {
            Pipe.BuildFromParameters();
        }

        public void PromptParameters()
        {
        PrompIndex:
            string r = UI.Prompt("What you want to print? \n \t \t Enter [bflg] to BruteForce List OR [csa] to Assemble multiple pipes into a Complex String Assembler ").Trim().ToLower();

            switch (r)
            {
                case "csa":
                case "c":
                    CSA c = UI.Lab?.Request<CSA>();
                    if (c == null)
                    {
                        c = new(UI);
                        c.PromptParameters();
                    }
                    Pipe = c;
                    break;

                case "bflg":
                case "b":
                    BFLG b = UI.Lab?.Request<BFLG>();
                    if (b == null)
                    {
                        b = new(UI);
                        b.PromptParameters();
                    }
                    Pipe = b;
                    break;

                default:
                    goto PrompIndex;
            }


        }

        public void RunService()
        {
            UI.Log($"Begin {Pipe.ToString()}");
            UI.Log("");

            foreach (string item in Pipe.GenerateEnumerable())
            {
                UI.Log(item);
            }

            UI.Log("");
            UI.Log($"End {Pipe.ToString()}");

        }
    }
}