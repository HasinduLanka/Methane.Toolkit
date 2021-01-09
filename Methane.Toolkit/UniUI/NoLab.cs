using System;
using System.Collections.Generic;

namespace UniUI
{
    public class NoLab : ILab
    {
        [System.Text.Json.Serialization.JsonIgnore] public UniUI.IUniUI UI { get; set; }


        public IWorkerType WorkerType => IWorkerType.Service;

        public void BuildFromParameters()
        {
            if (UI != null)
            {
                UI.Lab = this;
            }
        }

        public NoLab(UniUI.IUniUI ui)
        {
            UI = ui;
        }


        public void PromptParameters()
        {

        }
        public void RunService()
        {

        }

        public T Request<T>() where T : IWorker
        {
            return RequestNew<T>();
        }

        public T RequestNew<T>() where T : IWorker
        {
            T w = Activator.CreateInstance<T>();
            w.UI = UI;

            return w;
        }

        public void RegisterReusableWorker<T>(T WOriginal, string Name) where T : IWorker
        {

        }
    }


    public class SimplePipe : IPipe
    {


        public IWorkerType WorkerType => IWorkerType.Pipe;

        [System.Text.Json.Serialization.JsonIgnore] public UniUI.IUniUI UI { get; set; }


        public IEnumerable<string> Enum;

        public SimplePipe()
        {
        }
        public SimplePipe(IUniUI uI)
        {
            UI = uI;
        }

        public SimplePipe(IUniUI uI, IEnumerable<string> @enum)
        {
            UI = uI;
            Enum = @enum;
        }

        public void BuildFromParameters()
        {

        }

        public IEnumerable<string> GenerateEnumerable()
        {
            return Enum;
        }

        public void PromptParameters()
        {

        }

        public IEnumerator<string> RunIterator()
        {
            return Enum.GetEnumerator();
        }

        public void RunService()
        {

        }
    }
}
