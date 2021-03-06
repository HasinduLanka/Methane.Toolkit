using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Text;
using UniUI;

namespace Methane.Toolkit
{

    public class Lab : ILab
    {
        [System.Text.Json.Serialization.JsonIgnore] public UniUI.IUniUI UI { get; set; }

        public List<WorkerContainer> Pipes { get; set; } = new List<WorkerContainer>();
        public List<WorkerContainer> Services { get; set; } = new List<WorkerContainer>();
        // public List<WorkerContainer> RWorkers { get; set; } = new List<WorkerContainer>();

        public Lab()
        {
        }

        public Lab(UniUI.IUniUI ui)
        {
            UI = ui;
        }

        public IWorkerType WorkerType => IWorkerType.Service;


        public void BuildFromParameters()
        {
            if (UI != null)
            {
                UI.Lab = this;
            }
        }


        public string JSONCopy()
        {
            return Core.ToJSON(this);
        }

        public void PromptParameters()
        {

        }

        public void RunService()
        {
            if (UI != null)
            {
                UI.Lab = this;
            }
            ShowMenu();
        }

        public void ShowMenu()
        {
        ProgramBegin:
            UI.Log("");
            UI.Log("-----------------------------------");
            UI.Log("||             ...               ||");
            UI.Log("||           Methane             ||");
            UI.Log("||        Automating Lab         ||");
            UI.Log("||             ...               ||");
            UI.Log("-----------------------------------");
            UI.Log("");
            UI.Log("");
            ShowPipes();
            UI.Log("");

            if (Services.Count > 0)
            {
                string in1 = UI.Prompt("[Enter] to add a new worker  |  Input the index of Service worker to Run it  |  [E] to Exit").ToLower().Trim();

                if (in1 == "e")
                {
                    return;
                }
                else if (int.TryParse(in1, out int ini) && ini < Services.Count)
                {
                    WorkerContainer wc = Services[ini];
                    UI.Log($"Building {wc.Name}");

                    wc.Worker.BuildFromParameters();

                    UI.Log($"Running {wc.Name}");

                    wc.Worker.RunService();
                }
                else
                {
                    ShowAddMenu();
                }
            }
            else
            {
                ShowAddMenu();
            }



            goto ProgramBegin;

        }

        public void ShowAddMenu()
        {



            UI.Log("");
            UI.Log("1.bfgl \t\t BFLG - BruteForce List Generator");
            UI.Log("2.csa \t\t CSA - Complex String Assembler");
            UI.Log("3.post \t\t RapidPOSTer - HTTP POST form data in bulk");
            UI.Log("4.print \t PipePrinter - Print output of a Pipe");
            UI.Log("5.get \t\t RapidGETer - HTTP GET in bulk");
            UI.Log("6.download \t RapidDownloader - HTTP Download in bulk");
            UI.Log("7.fileops \t RapidFileOps - Do file operations in bulk");
            UI.Log("m3up \t M3U8 parser - Parse M3U8 files");
            UI.Log("m3ud \t M3U8 Downloader - Download M3U8 files");
            UI.Log("GP. BulkGETPOST - Download, alt and POST form data. (Code it)");
            UI.Log("");
            UI.Log("[Enter] for previous menu");

        

            string subProgram = UI.Prompt("Enter Index to start ");


            switch (subProgram.ToLower())
            {

                case "1":
                case "bflg":

                    AddWorker(new BFLG(UI));
                    break;

                case "2":
                case "csa":
                    AddWorker(new CSA(UI));
                    break;

                case "3":
                case "post":
                    AddWorker(new RapidPOSTer(UI));
                    break;

                case "4":
                case "print":
                    AddWorker(new PipePrinter(UI));
                    break;

                case "5":
                case "get":
                    AddWorker(new RapidGETer(UI));
                    break;

                case "6":
                case "download":
                    AddWorker(new RapidDownloader(UI));
                    break;

                case "7":
                case "fileops":
                    AddWorker(new FileOperation(UI));
                    break;

                case "m3ud":
                    AddWorker(new M3U.M3UDownload(UI));
                    break;

                case "m3up":
                    AddWorker(new M3U.M3UParser(UI));
                    break;


                case "GP":
                    AddWorker(new BulkGETPOST(UI));
                    break;


                default:
                    return;

            }



        }

        private void ShowPipes()
        {
            if (Pipes.Count > 0)
            {
                UI.Log("");
                UI.Log("");
                UI.Log($"{Pipes.Count} Pipes");

                for (int i = 0; i < Pipes.Count; i++)
                {
                    WorkerContainer iwc = Pipes[i];
                    UI.Log($"{i}. {iwc.Name} \t \t {iwc.Worker?.WorkerType} {(iwc.Used ? " Used " : "")}");
                }

            }

            if (Services.Count > 0)
            {
                UI.Log("");
                UI.Log("");
                UI.Log($"{Services.Count} Services");

                for (int i = 0; i < Services.Count; i++)
                {
                    WorkerContainer iwc = Services[i];
                    UI.Log($"{i}. {iwc.Name} \t \t {iwc.Worker?.WorkerType} {(iwc.Used ? " Used " : "")}");
                }
            }

            // if (RWorkers.Count > 0)
            // {
            //     UI.Log("");
            //     UI.Log("");
            //     UI.Log($"{RWorkers.Count} Reusable workers");

            //     for (int i = 0; i < RWorkers.Count; i++)
            //     {
            //         WorkerContainer iwc = RWorkers[i];
            //         UI.Log($"{i}. {iwc.Name} \t \t {iwc.Worker?.WorkerType}");
            //     }
            // }

            UI.Log("");

            if (Services.Count == 0 && Pipes.Count == 0)
            {
                UI.Log("Let's create some Workers!");
            }
        }

        private void AddWorker<T>(T w) where T : IWorker
        {
            string wprefix = $"{w.ToString()}-";
            WorkerContainer wc = new WorkerContainer();
            wc.Name = wprefix + UI.Prompt($"What should we call this worker? Give a name {wprefix}");
            wc.Worker = w;

            UI.Log($"Creating {wc.Name}");

            w.PromptParameters();

            RegisterWorkerToWC(w, wc);
        }

        private void RegisterWorkerToWC<T>(T w, WorkerContainer wc) where T : IWorker
        {
            if ((w.WorkerType & IWorkerType.Reusable) != 0)
            {

                wc.Copy = Core.ToJSON(w);
                // RWorkers.Add(wc);
                UI.Log($"Saved reusable {wc.Name}");

            }

            if ((w.WorkerType & IWorkerType.Pipe) != 0)
            {
                Pipes.Add(wc);
                UI.Log($"Added Pipe {wc.Name}");
            }
            else if ((w.WorkerType & IWorkerType.Service) != 0)
            {
                Services.Add(wc);
                UI.Log($"Added Service {wc.Name}");
            }
            else
            {
                UI.LogError(new Exception(), $"{wc.Name} does not fall into either Pipe or Service");
            }
        }

        public T Request<T>() where T : IWorker
        {

            List<WorkerContainer> Matches = new List<WorkerContainer>();

            UI.Log("");
            UI.Log("");
            if (Pipes.Count > 0)
            {
                for (int i = 0; i < Pipes.Count; i++)
                {
                    WorkerContainer iwc = Pipes[i];
                    if (iwc.Worker is T && (((iwc.Worker.WorkerType & IWorkerType.Reusable) != 0) || !iwc.Used))
                    {
                        int mi = Matches.Count;
                        Matches.Add(iwc);
                        UI.Log($"{mi}. {iwc.Name} \t \t {iwc.Worker?.WorkerType} ");
                    }
                }

            }

            int MatchedPipes = Matches.Count;



            if (Services.Count > 0)
            {
                UI.Log("");

                for (int i = 0; i < Services.Count; i++)
                {
                    WorkerContainer iwc = Services[i];
                    if (iwc.Worker is T && (((iwc.Worker.WorkerType & IWorkerType.Reusable) != 0) || !iwc.Used))
                    {
                        int mi = Matches.Count;
                        Matches.Add(iwc);
                        UI.Log($"{i}. {iwc.Name} \t \t {iwc.Worker?.WorkerType} ");
                    }
                }
            }


            UI.Log("");

            if (Matches.Count == 0)
            {
                return RequestNew<T>();
            }
            else
            {
            PrompIndex:
                string r = UI.Prompt("[Enter] to create a new worker OR input index to use").Trim().ToLower();
                if (string.IsNullOrEmpty(r))
                {
                    return RequestNew<T>();
                }
                else
                {
                    if (int.TryParse(r, out int ri) && ri < Matches.Count)
                    {
                        WorkerContainer workerContainer = Matches[ri];
                        if ((workerContainer.Worker.WorkerType & IWorkerType.Reusable) == 0)
                        {
                            workerContainer.Used = true;

                            Pipes.Remove(workerContainer);
                            Services.Remove(workerContainer);

                        }
                        return (T)workerContainer.Worker;

                    }
                    else
                    {
                        UI.Log("What?");
                        goto PrompIndex;
                    }
                }
            }
        }

        public T RequestNew<T>() where T : IWorker
        {
            T w = Activator.CreateInstance<T>();
            w.UI = UI;
            AddWorker(w);

            return w;

        }

        public void RegisterReusableWorker<T>(T WOriginal, string Name) where T : IWorker
        {

            IWorker w = Core.FromJSON<T>(Core.ToJSON(WOriginal));
            w.UI = UI;

            WorkerContainer wc = new WorkerContainer();
            wc.Name = $"{w.ToString()}";
            wc.Worker = w;

            UI.Log($"Creating {wc.Name}");

            RegisterWorkerToWC(w, wc);
        }
    }


    public class WorkerContainer
    {
        private long hID { get; init; } = DateTime.Now.Ticks;
        public bool Used { get; set; } = false;
        public IWorker Worker { get; set; }
        public string Name { get; set; }
        public string Copy { get; set; } = null;

        public override bool Equals(object obj)
        {
            return obj is WorkerContainer container &&
            hID == container.hID &&
            EqualityComparer<IWorker>.Default.Equals(Worker, container.Worker) &&
            Name == container.Name;

        }

        public override int GetHashCode()
        {
            return HashCode.Combine(hID, Used, Worker, Name, Copy);
        }
    }





}