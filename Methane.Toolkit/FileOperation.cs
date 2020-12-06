using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Text;
using HtmlAgilityPack;
using System.Net.Http;
using System.Resources;

namespace Methane.Toolkit
{
    public class FileOperation : IWorker
    {
        [NonSerialized]
        readonly UniUI.IUniUI UI;
        public FileOperation(UniUI.IUniUI ui)
        {
            UI = ui;
        }

        public FileOperation()
        {
            UI = new UniUI.NoUI();
        }

        public string OutputPath;

        public CSA csa;
        IEnumerator<string> bodyPipeline;

        public List<Action<string, string>> operations = new List<Action<string, string>>();

        public int runningThreads = 0;
        public int AllowedThrds;


        public void PromptParameters()
        {

            UI.Log("");
            UI.Log("    . . . . . . . . . . . . . . . .  .  ");
            UI.Log("               Methane                  ");
            UI.Log("            Bulk File Operation         ");
            UI.Log("    . . . . . . . . . . . . . . . .  .  ");
            UI.Log("");




        ChooseOutputPath:

            OutputPath = UI.Prompt("Enter output path (Directory) or ~ to use  \"Output/\" ");
            if (OutputPath == "~") OutputPath = "Output/";

            try
            {
                if (!string.IsNullOrEmpty(OutputPath))
                {
                    Directory.CreateDirectory(OutputPath);
                    if (!OutputPath.EndsWith('/')) OutputPath += '/';
                }
            }
            catch (Exception ex)
            {
                UI.LogError(ex);
                goto ChooseOutputPath;
            }

            bool AllowMultithreading = true;
            operations = new List<Action<string, string>>();

            UI.Log("Please use the following tool to create File name list");
            csa = new CSA(UI);
            csa.PromptParameters();

        SelectOp:
            UI.Log("\t \tFile operations are");
            UI.Log("\t \t \t copy. Copy file");
            UI.Log("\t \t \t move. Move file");
            UI.Log("\t \t \t delete. Delete file");
            UI.Log("\t \t \t mkdir. Create new Directory");
            UI.Log("\t \t \t rmdir. Delete Directory with content");
            UI.Log("\t \t \t merge. Merge files to create one file");
            UI.Log("\t \t \t aes. Encrypt or Decrypt file AES");

            string op = UI.Prompt("Enter Index of Operation to use");

            // operations.Add((string inpath, string outpath) => { });
            switch (op)
            {//                
                case "copy":
                    operations.Add(new Action<string, string>((infile, outpath) =>
                    {
                        outpath += Path.GetFileName(infile);
                        File.Copy(infile, outpath, true);
                        UI.Log("Copied " + infile + " -> " + outpath);
                    }));
                    break;
                case "move":
                    operations.Add(new Action<string, string>((infile, outpath) =>
                    {
                        outpath += Path.GetFileName(infile);
                        File.Move(infile, outpath);
                        UI.Log("Moved " + infile + " -> " + outpath);
                    }));
                    break;
                case "delete":
                    operations.Add(new Action<string, string>((infile, outpath) =>
                    {
                        File.Delete(infile);
                        UI.Log("Deleted " + infile);
                    }));
                    break;
                case "mkdir":
                    operations.Add(new Action<string, string>((infile, outpath) =>
                    {
                        Directory.CreateDirectory(infile);
                        UI.Log("Created " + infile);
                    }));
                    break;

                case "rmdir":
                    operations.Add(new Action<string, string>((infile, outpath) =>
                    {
                        Directory.Delete(infile, true);
                        UI.Log("Deleted" + infile);
                    }));
                    break;

                case "aes":
                    string aesKey = UI.Prompt("Enter encryption key in BASE64 or ~filename to read it from file");
                    byte[] key;
                    if (aesKey.StartsWith('~'))
                    {
                        aesKey = aesKey.TrimStart('~');
                        if (File.Exists(aesKey))
                        {
                            try
                            {
                                key = File.ReadAllBytes(aesKey);
                            }
                            catch (Exception ex)
                            {
                                UI.LogError(ex, "Cannot read key file");
                                goto SelectOp;
                            }

                        }
                        else
                        {
                            UI.Log($"File not found  - {aesKey}");
                            goto SelectOp;
                        }
                    }
                    else
                    {
                        key = Convert.FromBase64String(aesKey);
                    }

                    string aesIV = UI.Prompt("Enter IV in BASE64 or ~filename to read it from file or just [Enter] to use 16 null bytes");
                    byte[] IV;
                    if (aesIV.StartsWith('~'))
                    {
                        aesIV = aesIV.TrimStart('~');
                        if (File.Exists(aesIV))
                        {
                            try
                            {
                                IV = File.ReadAllBytes(aesIV);
                            }
                            catch (Exception ex)
                            {
                                UI.LogError(ex, "Cannot read IV file");
                                goto SelectOp;
                            }

                        }
                        else
                        {
                            UI.Log($"File not found  - {aesIV}");
                            goto SelectOp;
                        }
                    }
                    else if (string.IsNullOrEmpty(aesIV))
                    {
                        IV = new byte[16];
                    }
                    else
                    {
                        IV = Convert.FromBase64String(aesIV);
                    }

                    AesRij aes = new AesRij(key, IV);

                    if (UI.Prompt("[e]ncrypt or [D]ecrypt ? (Enter char)").ToLower() == "e")
                    {
                        operations.Add(new Action<string, string>((infile, outpath) =>
                        {
                            outpath += Path.GetFileName(infile);
                            if (File.Exists(infile))
                            {
                                try
                                {
                                    File.WriteAllBytes(outpath, aes.Encrypt(File.ReadAllBytes(infile)));
                                    UI.Log("Encrypted " + infile + " -> " + outpath);
                                }
                                catch (Exception ex)
                                {
                                    UI.LogError(ex, $"Error while Encrypting in bulk - File {infile} to {outpath}");
                                }
                            }
                            else
                            {
                                UI.Log($"File not found {infile}");
                            }
                        }));
                    }
                    else
                    {
                        operations.Add(new Action<string, string>((infile, outpath) =>
                        {
                            outpath += Path.GetFileName(infile);
                            if (File.Exists(infile))
                            {
                                try
                                {
                                    File.WriteAllBytes(outpath, aes.Decrypt(File.ReadAllBytes(infile)));
                                    UI.Log("Decrypted " + infile + " -> " + outpath);
                                }
                                catch (Exception ex)
                                {
                                    UI.LogError(ex, $"Error while Encrypting in bulk - File {infile} to {outpath}");
                                }
                            }
                            else
                            {
                                UI.Log($"File not found {infile}");
                            }
                        }));
                    }

                    break;

                case "merge":
                    AllowMultithreading = false;
                    operations.Add(new Action<string, string>((infile, outpath) =>
                    {
                        outpath += "Merged" + Path.GetExtension(infile);
                        using (var stream = new FileStream(outpath, FileMode.Append))
                        {
                            Stream ins = File.OpenRead(infile);
                            ins.CopyTo(stream);
                            stream.Close();
                            ins.Close();
                        }
                        UI.Log("Merge " + infile + " -> " + outpath);
                    }));
                    break;

                default:
                    UI.Log("Invalid selection");
                    goto SelectOp;
            }

            if (AllowMultithreading)
            {
            PromptHowManyThreads:
                if (!int.TryParse(UI.Prompt("How many threads to use?"), out AllowedThrds)) goto PromptHowManyThreads;
            }
            else
            {
                AllowedThrds = 1;
            }
            UI.Log("File operations standby    :-) ");


        }

        public void BuildFromParameters()
        {

        }

        public void RunService()
        {

            UI.Log("");
            UI.Log("    . . . . . . . . . . . . . . . .  .  ");
            UI.Log("               Methane                  ");
            UI.Log("          Bulk File Operation           ");
            UI.Log("    . . . . . . . . . . . . . . . .  .  ");
            UI.Log("");



            UI.Log("Bulk File Operations Running...");


            bodyPipeline = csa.RunIterator();

            for (int n = 0; n < AllowedThrds; n++)
            {
                new System.Threading.Thread(new System.Threading.ThreadStart(ThrLoop)) { Name = $"FileOp Thrd {n}" }.Start();
                System.Threading.Thread.Sleep(1000);
            }


            while (runningThreads > 0)
            {
                System.Threading.Thread.Sleep(1000);
            }

        }


        private void ThrLoop()
        {

            runningThreads++;

        bodyPipelineMoveNext:

            string input;

            lock (bodyPipeline)
            {
                if (bodyPipeline.MoveNext())
                {
                    input = bodyPipeline.Current;
                }
                else
                {
                    runningThreads--;
                    return;
                }
            }

            try
            {
                Process(input, OutputPath);



            }
            catch (Exception ex)
            {
                UI.Log($"!!! Error {ex} - {ex.Message} @ {ex.StackTrace}");
            }

            goto bodyPipelineMoveNext;




        }


        public void Process(string inFileName, string outFilePath)
        {
            foreach (var op in operations)
            {
                try
                {
                    op(inFileName, outFilePath);
                }
                catch (Exception ex)
                {
                    UI.LogError(ex);
                }

            }

        }

        IWorkerType IWorker.WorkerType => IWorkerType.ServiceReusable;

    }

}
