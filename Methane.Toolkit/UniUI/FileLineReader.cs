using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace UniUI
{
    public class FileLineReader : IPipe
    {
        public string filename { get; set; }

        public FileLineReader(string filename, IUniUI uI)
        {
            UI = uI;
            this.filename = filename;
        }
        public FileLineReader(IUniUI uI)
        {
            UI = uI;
        }

        public FileLineReader()
        {
            UI = new UniUI.NoUI();
        }

        public IWorkerType WorkerType => IWorkerType.PipeReusable;


        [System.Text.Json.Serialization.JsonIgnore] public UniUI.IUniUI UI { get; set; }

        public IEnumerable<string> ReadFileLineByLine()
        {

            StreamReader reader = new StreamReader(filename);

            while (!reader.EndOfStream)
            {
                yield return reader.ReadLine();
            }

            reader.Close();

            yield break;

        }

        public IEnumerator<string> RunIterator()
        {
            return GenerateEnumerable().GetEnumerator();
        }

        public IEnumerable<string> GenerateEnumerable()
        {
            return ReadFileLineByLine();
        }

        public void PromptParameters()
        {
        PromptFileName:
            string DicFileName = UI.Prompt($"Enter the filename to read : ");

            if (File.Exists(DicFileName))
            {
                filename = DicFileName;
            }
            else
            {
                if (UI.Prompt("That file does not exist. Try again? [y]|[N]").ToUpper() == "Y")
                    goto PromptFileName;
            }
        }

        public void BuildFromParameters()
        {

        }

        public void RunService()
        {

        }

    }
}
