using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace Methane.Toolkit
{
    public class FileLineReader
    {
        public string filename;
        public IEnumerator<string> ReadFileLineByLine()
        {

            StreamReader reader = new StreamReader(filename);

            while (!reader.EndOfStream)
            {
                yield return reader.ReadLine();
            }

            reader.Close();

            yield break;

        }

        public FileLineReader(string filename)
        {
            this.filename = filename;
        }
    }
}
