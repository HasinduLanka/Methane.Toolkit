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
        readonly UniUI.IUniUI UI;
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
            BFLG b = new(UI);
            b.PromptParameters();

            Pipe = b;


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