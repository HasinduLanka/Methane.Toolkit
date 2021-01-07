using System;

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
}
