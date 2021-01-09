using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using UniUI;

namespace Methane.Toolkit.M3U
{

    public class M3UDownload : IWorker
    {
        public IWorkerType WorkerType => IWorkerType.Service;

        [System.Text.Json.Serialization.JsonIgnore] public UniUI.IUniUI UI { get; set; }

        M3UParser P;
        RapidDownloader RD;

        public M3UDownload()
        {
            UI = new NoUI();
        }

        public M3UDownload(UniUI.IUniUI ui)
        {
            UI = ui;
        }


        public void PromptParameters()
        {
            UI.Log("");
            UI.Log("    . . . . . . . . . . . . . . . .  .  ");
            UI.Log("               Methane                  ");
            UI.Log("       M3U8 Parser and Downloader       ");
            UI.Log("    . . . . . . . . . . . . . . . .  .  ");
            UI.Log("");

            P = new M3UParser(UI);
            P.PromptParameters();

            RD = new RapidDownloader(UI);
            RD.csa = P.Pp;
            RD.PromptParameters();


        }
        public void BuildFromParameters()
        {
            P.BuildFromParameters();
            RD.BuildFromParameters();
        }

        public void RunService()
        {
            P.RunService();
            RD.RunService();
        }
    }



    public class M3UParser : IPipe
    {
        public IWorkerType WorkerType => IWorkerType.Service;

        [System.Text.Json.Serialization.JsonIgnore] public UniUI.IUniUI UI { get; set; }



        public M3U m3U;
        public IPipe Pp;


        public M3UParser()
        {
            UI = new NoUI();
        }

        public M3UParser(UniUI.IUniUI ui)
        {
            UI = ui;
        }


        public void PromptParameters()
        {
            UI.Log("");
            UI.Log(" . . . . . . . . . . . ");
            UI.Log("    M3U8 Parser        ");
            UI.Log(" . . . . . . . . . . . ");
            UI.Log("");



        Choosesrc:
            string src = UI.Prompt("Enter your M3U8 file URL or Enter ~filename.m3u8 to read from file. \n (You can also input the file to this CLI by replacing new lines with \\n)");

            if (src.StartsWith('~'))
            {
                try
                {
                    m3U = M3U.FromFile(src.TrimStart('~'));
                }
                catch (Exception ex)
                {
                    UI.LogError(ex);
                    goto Choosesrc;
                }
            }
            else if (src.StartsWith("#EXTM3U"))
            {
                try
                {
                    m3U = M3U.FromString(src.Replace("\\n", Environment.NewLine));
                    UI.Log("Parsed M3U from string");
                }
                catch (System.Exception ex)
                {
                    UI.LogError(ex);
                    goto Choosesrc;
                }
            }
            else
            {
                try
                {
                    m3U = M3U.FromURL(src);
                }
                catch (System.Exception ex)
                {
                    UI.LogError(ex);
                    goto Choosesrc;
                }

            }


            var p = m3U.GetCSA(UI);
            if (p.Item1)
            {
                Pp = p.Item2;
            }
            else
            {
                goto Choosesrc;
            }
        }
        public void BuildFromParameters()
        {
            //Do not Build CSA. It's a custom build
            // Pp.IsBuilt = true;
        }

        public void RunService()
        {
            //Not neccesory
            Pp.RunService();
        }

        public IEnumerator<string> RunIterator()
        {
            return Pp.RunIterator();
        }

        public IEnumerable<string> GenerateEnumerable()
        {
            return Pp.GenerateEnumerable();
        }
    }

    public class M3U : IPipe
    {
        public string TargetDuration { get; private set; }
        public bool AllowCache { get; private set; }
        public string PlaylistType { get; private set; }
        public EncryptionKey Key { get; private set; }
        public string Version { get; private set; }
        public string MediaSequence { get; private set; }
        public Chunk[] Chunks { get; private set; }


        public IWorkerType WorkerType => IWorkerType.PipeReusable;

        [System.Text.Json.Serialization.JsonIgnore] public UniUI.IUniUI UI { get; set; }

        public (bool, IPipe) GetCSA(IUniUI UI)
        {
            if (Chunks == null || Chunks.Length == 0) return (false, null);

            string cu = Chunks[0].Url;
            string cul = cu.ToLower();

            if (cul.StartsWith("http"))
            {
                if (UI.Prompt($"The first chunk URL {cu} looks good. Press [Enter] to use that URL as it is OR input 'csa' to create a CSA using this URL").Trim() != string.Empty)
                {
                    return (true, MakeNewCSA(UI, cu));
                }
                else
                {
                    CSA csa = new CSA(UI);
                    csa.mask = "*chunk*";
                    CSAFeed cf = new CSAFeed("*chunk*", this);
                    csa.Feeds = new List<CSAFeed>() { cf };
                    csa.IsBuilt = true;

                    //TODO: RegisterReusableWorker
                    // UI?.Lab?.RegisterReusableWorker(csa, $"CSA for M3U downloader {csa.mask}");
                    return (true, csa);
                }

            }
            else
            {
                UI.Log($"The first chunk URL {cu} is not in the correct format. ");
                return (true, MakeNewCSA(UI, cu));
            }




        }

        private CSA MakeNewCSA(IUniUI UI, string cu)
        {
            {
                UI.Log($"Use the following tool to assemble this into https://example.com/path/to/page/file1.ts format");

                CSA csa = new CSA(UI);
                CSAFeed cf = new CSAFeed("*chunk*", this);
                csa.Feeds = new List<CSAFeed>() { cf };
                csa.IsBuilt = true;

            //TODO: RegisterReusableWorker
            // UI?.Lab?.RegisterReusableWorker(csa, $"CSA for M3U downloader {csa.mask}");

            GetMask:
                csa.mask = UI.Prompt($"Enter URL Mask string. (substitute : *chunk* to replace the chunk URL) (Ex: https://example.com/chunks/*chunk*  will turn into https://example.com/chunks/{cu})");

                if (UI.Prompt($"URLs will be like {csa.mask.Replace("*chunk*", cu)}  Please make sure this URL works.   [Enter] to continue or [c] to change mask").Trim() != string.Empty)
                {
                    goto GetMask;
                }

                return csa;

            }
        }

        // private int TestURL(string url)
        // {
        //     WebClient wc = new WebClient();
        //     byte[] b = wc.DownloadData(url);
        //     return b.Length;
        // }

        public IEnumerable<string> ChunksEnumerable()
        {
            foreach (Chunk c in Chunks)
            {
                yield return c.Url;
            }
        }

        public static M3U FromFile(string path)
        {
            var s = File.ReadAllText(path);
            return FromString(s);
        }
        public static M3U FromURL(string url)
        {
            WebClient wc = new WebClient();
            var s = wc.DownloadString(url);
            return FromString(s);
        }
        public static M3U FromString(string s)
        {
            // TODO : string.split is inefficient
            string[] lines = s.Split('\n');
            M3U Out = new M3U();
            List<Chunk> _chunks = new List<Chunk>();

            for (int i = 0; i < lines.Length; i++)
            {
                string line = lines[i];

                if (line.Contains("TARGETDURATION"))
                    Out.TargetDuration = line.Split(':')[1];

                if (line.Contains("ALLOW-CACHE"))
                    Out.AllowCache = line.Split(':')[1] == "YES";

                if (line.Contains("PLAYLIST-TYPE"))
                    Out.PlaylistType = line.Split(':')[1];

                if (line.Contains("KEY"))
                    // TODO : Possible - key isn't in this file nor the url
                    Out.Key = new EncryptionKey(line.Split(':')[1] + (line.Split(':').Count() > 1 ? (":" + line.Split(':')[2]) : ""));

                if (line.Contains("VERSION"))
                    Out.Version = line.Split(':')[1];

                if (line.Contains("MEDIA-SEQUENCE"))
                    Out.MediaSequence = line.Split(':')[1];

                if (line.Contains("EXTINF"))
                {
                    // TODO : Possible URL without http:// part or domain
                    _chunks.Add(new Chunk(line, lines[i + 1]));
                    i = i + 1;
                }
            }
            Out.Chunks = _chunks.ToArray();
            return Out;
        }

        public IEnumerator<string> RunIterator()
        {
            return GenerateEnumerable().GetEnumerator();
        }

        public IEnumerable<string> GenerateEnumerable()
        {
            return ChunksEnumerable();
        }

        public void PromptParameters()
        {
            
        }

        public void BuildFromParameters()
        {
            
        }

        public void RunService()
        {
            
        }
    }







    public class EncryptionKey
    {
        public EncryptionMethod EncryptionMethod { get; private set; }
        private string KeyPath { get; set; }
        public bool IsFromUrl { get; set; }
        public string Key { get; set; }

        public EncryptionKey(string info)
        {
            string[] parts = info.Split(',');

            var Method = parts[0].ToString().Split('=')[1];
            KeyPath = parts[1].ToString().Split('=')[1];
            if (Method.Contains("AES"))
            {
                if (Method.Contains("128")) EncryptionMethod = EncryptionMethod.AES128;
                else if (Method.Contains("256")) EncryptionMethod = EncryptionMethod.AES256;
            }
            if (parts[1].Contains("URI"))
            {
                IsFromUrl = true;
                KeyPath = KeyPath.Substring(1, KeyPath.Length - 2);
                Key = GetKeyFromUrl(KeyPath);
            }
            else KeyPath = string.Empty;
        }

        private string GetKeyFromUrl(string url)
        {
            WebClient wc = new WebClient();
            return wc.DownloadString(url);
        }
    }


    public class Chunk
    {
        public double Length { get; private set; }
        public string Url { get; private set; }

        public Chunk(string infoline, string urlline)
        {
            Length = double.Parse(infoline.Split(':')[1].Replace(",", ""));
            Url = urlline;
        }
    }



    public enum EncryptionMethod
    {
        Unknown,
        AES128,
        AES256
    }



}
