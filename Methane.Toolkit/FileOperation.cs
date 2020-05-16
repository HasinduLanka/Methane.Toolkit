using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using static Methane.Toolkit.Common;
using System.Text;
using HtmlAgilityPack;
using System.Net.Http;
using System.Resources;
using System.Runtime.Intrinsics.X86;

namespace Methane.Toolkit
{
    public class FileOperation
    {

        public string OutputPath;

        public CSA csa;
        IEnumerator<string> bodyPipeline;

        public List<Action<string, string>> operations = new List<Action<string, string>>();

        public int runningThreads = 0;
        public int AllowedThrds;

        public void PromptParamenters()
        {

            Log("");
            Log("    . . . . . . . . . . . . . . . .  .  ");
            Log("               Methane                  ");
            Log("            Bulk File Operation         ");
            Log("    . . . . . . . . . . . . . . . .  .  ");
            Log("");




            ChooseOutputPath:

            OutputPath = Prompt("Enter output path (Directory) or ~ to use  \"Output/\" ");
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
                LogError(ex);
                goto ChooseOutputPath;
            }

            bool AllowMultithreading = true;
            operations = new List<Action<string, string>>();

            Log("Please use the following tool to create File name list");
            csa = new CSA();
            csa.PromptParamenters();

            SelectOp:
            Log("\t \tFile operations are");
            Log("\t \t \t copy. Copy file");
            Log("\t \t \t move. Move file");
            Log("\t \t \t delete. Delete file");
            Log("\t \t \t mkdir. Create new Directory");
            Log("\t \t \t rmdir. Delete Directory with content");
            Log("\t \t \t merge. Merge files to create one file");
            Log("\t \t \t aes. Encrypt or Decrypt file AES");

            string op = Prompt("Enter Index of Operation to use");

            // operations.Add((string inpath, string outpath) => { });
            switch (op)
            {//                
                case "copy":
                    operations.Add(new Action<string, string>((infile, outpath) =>
                    {
                        outpath += Path.GetFileName(infile);
                        File.Copy(infile, outpath, true);
                        Log("Copied " + infile + " -> " + outpath);
                    }));
                    break;
                case "move":
                    operations.Add(new Action<string, string>((infile, outpath) =>
                    {
                        outpath += Path.GetFileName(infile);
                        File.Move(infile, outpath, true);
                        Log("Moved " + infile + " -> " + outpath);
                    }));
                    break;
                case "delete":
                    operations.Add(new Action<string, string>((infile, outpath) =>
                    {
                        File.Delete(infile);
                        Log("Deleted " + infile);
                    }));
                    break;
                case "mkdir":
                    operations.Add(new Action<string, string>((infile, outpath) =>
                    {
                        Directory.CreateDirectory(infile);
                        Log("Created " + infile);
                    }));
                    break;

                case "rmdir":
                    operations.Add(new Action<string, string>((infile, outpath) =>
                    {
                        Directory.Delete(infile, true);
                        Log("Deleted" + infile);
                    }));
                    break;

                case "aes":
                    string aesKey = Prompt("Enter encryption key in BASE64 or ~filename to read it from file");
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
                                LogError(ex, "Cannot read key file");
                                goto SelectOp;
                            }

                        }
                        else
                        {
                            Log($"File not found  - {aesKey}");
                            goto SelectOp;
                        }
                    }
                    else
                    {
                        key = Convert.FromBase64String(aesKey);
                    }

                    string aesIV = Prompt("Enter IV in BASE64 or ~filename to read it from file or just [Enter] to use 16 null bytes");
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
                                LogError(ex, "Cannot read IV file");
                                goto SelectOp;
                            }

                        }
                        else
                        {
                            Log($"File not found  - {aesIV}");
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

                    if (Prompt("[e]ncrypt or [D]ecrypt ? (Enter char)").ToLower() == "e")
                    {
                        operations.Add(new Action<string, string>((infile, outpath) =>
                        {
                            outpath += Path.GetFileName(infile);
                            if (File.Exists(infile))
                            {
                                try
                                {
                                    File.WriteAllBytes(outpath, aes.Encrypt(File.ReadAllBytes(infile)));
                                    Log("Encrypted " + infile + " -> " + outpath);
                                }
                                catch (Exception ex)
                                {
                                    LogError(ex, $"Error while Encrypting in bulk - File {infile} to {outpath}");
                                }
                            }
                            else
                            {
                                Log($"File not found {infile}");
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
                                    Log("Decrypted " + infile + " -> " + outpath);
                                }
                                catch (Exception ex)
                                {
                                    LogError(ex, $"Error while Encrypting in bulk - File {infile} to {outpath}");
                                }
                            }
                            else
                            {
                                Log($"File not found {infile}");
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
                        Log("Merge " + infile + " -> " + outpath);
                    }));
                    break;

                default:
                    Log("Invalid selection");
                    goto SelectOp;
            }

            if (AllowMultithreading)
            {
                PromptHowManyThreads:
                if (!int.TryParse(Prompt("How many threads to use?"), out AllowedThrds)) goto PromptHowManyThreads;
            }
            else
            {
                AllowedThrds = 1;
            }
            Log("File operations standby    :-) ");


        }


        public void Run()
        {

            Log("");
            Log("    . . . . . . . . . . . . . . . .  .  ");
            Log("               Methane                  ");
            Log("          Bulk File Operation           ");
            Log("    . . . . . . . . . . . . . . . .  .  ");
            Log("");



            Log("Bulk File Operations Running...");


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
                Log($"!!! Error {ex} - {ex.Message} @ {ex.StackTrace}");
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
                    LogError(ex);
                }

            }

        }


    }

}