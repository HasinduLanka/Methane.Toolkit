using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Text;
using HtmlAgilityPack;
using System.Net.Http;
using System.Resources;

namespace UniUI
{
    public interface IWorker : IUniUIObject
    {

        public void PromptParameters();
        public void BuildFromParameters();
        public void RunService();
        public IWorkerType WorkerType { get; }

    }

    public interface IPipe : IWorker
    {
        public IEnumerator<string> RunIterator();
        public IEnumerable<string> GenerateEnumerable();
    }

    public interface ILab : IWorker
    {
        public T Request<T>() where T : IWorker;
        public T RequestNew<T>() where T : IWorker;
    }

   

    [Flags]
    public enum IWorkerType
    {
        None = 0,
        Reusable = 1,
        Pipe = 2,
        Service = 8,
        PipeReusable = Reusable | Pipe,
        ServiceReusable = Reusable | Service
    }
}