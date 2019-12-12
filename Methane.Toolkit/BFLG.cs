﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Timers;
using static Methane.Toolkit.Common;
using System.Net;
using System.Diagnostics;
using System.Text;

namespace Methane.Toolkit
{
    public class BFLG
    {
        public int min = 1;
        public int max = 8;
        public string familiesPrompt = "3";
        public string FileName = null;
        public bool UseFile = false;
        public bool MultiThreaded = false;

        public List<char> Chars = new List<char>();
        public StreamWriter SW;

        public string prefix = null;
        public string sufix = null;
        public bool hasSufix = false;


        public System.Timers.Timer Tmr = new Timer(1000);
        public Stopwatch stopwatch = new Stopwatch();
        public bool needProgressReport = false;


        // Public FS As FileStream
        public void PrompParamenters()
        {
            Log("");
            Log("......................................");
            Log("Methane BruteForce word List Generator");
            Log("......................................");
            Log("");
            Log("");
            Log("Select charactor families to include in words  (index seperated by commas)    Extra : >ABC for prefix ABC; <XYZ to sufix XYZ");
            Log(" 1. English CAPITAL letters");
            Log(" 2. English simple letters");
            Log(" 3. Numbers");
            Log(" 4. symbols 1 { - _ }");
            Log(" 4B. symbols 1B { _ - @ # $ & }");
            Log(@" 5. symbols 2 { | \ ! @ # $ % & * ? < > / ' }");
            Log(@" 6. All symbols { `~!@#$%^&*()_+-={}[]|\:;'""<>?,./ }");
            Log(" 7. Spaces");

            familiesPrompt = Prompt("Enter indexes : ");

            min = Math.Max(1, int.Parse(Prompt("\n Enter the min word charactors")));
            max = Math.Min(100, int.Parse(Prompt(" Enter the max word charactors")));

            if (UseFile)
            {
                FileName = Prompt("Enter file name to save : ");
                Log($"Selected {Path.GetFullPath(FileName)}");
                SW = new StreamWriter(FileName, false, System.Text.Encoding.UTF7);
            }

            string thrChoice = Prompt("How many threads to use? (Enter 0|1 for single threaded. Enter amount for multiple threads. (Default is 16 - multithreaded)");

            if (int.TryParse(thrChoice, out int choice))
            {
                if (choice <= 1)
                {
                    MultiThreaded = false;
                }
                else
                {
                    MultiThreaded = true;
                    nOfThreadsToUse = choice;
                }
            }
            else
            {
                MultiThreaded = true;
                nOfThreadsToUse = 16;
            }



        }

        public void BuildFromParamenters()
        {
            string[] families = familiesPrompt.Replace(" ", "").Split(",");

            foreach (var family in families)
            {
                if (family.StartsWith('>'))
                    prefix = family.TrimStart('>');
                else if (family.StartsWith('<'))
                {
                    sufix = family.TrimStart('<');
                    hasSufix = true;
                }
                else
                    switch (family)
                    {
                        case "1":
                            {
                                Chars.AddRange("QWERTYUIOPASDFGHJKLZXCVBNM".ToArray());
                                break;
                            }

                        case "2":
                            {
                                Chars.AddRange("qwertyuiopasdfghjklzxcvbnm".ToArray());
                                break;
                            }

                        case "3":
                            {
                                Chars.AddRange("0123456789".ToArray());
                                break;
                            }

                        case "4":
                            {
                                Chars.AddRange("-_".ToArray());
                                break;
                            }

                        case "4B":
                            {
                                Chars.AddRange("_-@#$&".ToArray());
                                break;
                            }

                        case "5":
                            {
                                Chars.AddRange(@" | \ ! @ # $ % & * ? < > / ' ".Replace(" ", "").ToArray());
                                break;
                            }

                        case "6":
                            {
                                Chars.AddRange(@"`~!@#$%^&*()_+-={}[]|\:;'""<>?,./".ToArray());
                                break;
                            }

                        case "7":
                            {
                                Chars.Add(' ');
                                break;
                            }

                    }
            }




            total = 0;
            for (var L = min; L <= max; L++)
                total += (ulong)Math.Pow(Chars.Count, L);
            ntotal = 0;


            ulong fsize = CalculateFileSize();
            Log($"That means {total} words or {(((float)fsize) / (1024 * 1024)).ToString("0.0")} MB  ({fsize} bytes)!");



        }

        public ulong ntotal;
        public ulong total;



        public void GenerateToFile()
        {
            Log("BFLG writing to file");
            needProgressReport = true;
            GenerateToStream(SW);
        }

        public void GenerateToStream(StreamWriter stream)
        {
            try
            {
                if (MultiThreaded)
                {
                    GenerateToStreamThreaded(stream);
                }
                else
                {
                    GenerateToStreamReal(stream);
                }
            }
            catch (Exception)
            {


                try
                {
                    stream.Flush();
                }
                catch (Exception)
                {
                }

                throw;
            }

            stream.Flush();

            Log("Done");
        }

        private void GenerateToStreamReal(StreamWriter stream)
        {
            foreach (string S in GenerateEnumerable())
            {
                stream.WriteLine(S);
            }

        }



        public int nOfThreadsToUse = 32;
        private int runningThreads = 0;
        private StreamWriter ThrdSW;
        private IEnumerator<string> ThrItr;
        public bool ThrLock = false;
        private void GenerateToStreamThreaded(StreamWriter stream)
        {
            runningThreads = 0;
            ThrdSW = stream;

            ThrItr = GenerateIterator();

            for (int n = 0; n < nOfThreadsToUse; n++)
            {
                System.Threading.Thread thr = new System.Threading.Thread(new System.Threading.ThreadStart(GenerateToStreamThreadedThrVoid)) { Name = $"BFLG toStream {n}" };
                runningThreads++;
                thr.Start();
                System.Threading.Thread.Sleep(100);
            }


            while (runningThreads != 0)
            {
                System.Threading.Thread.Sleep(500);
            }

            Log("All threads completed.");

        }

        private void GenerateToStreamThreadedThrVoid()
        {
            StringBuilder sb = new StringBuilder();
            int lineCount = 0;
            int lineLimit = 2000;

            Log($"Thr {System.Threading.Thread.CurrentThread.Name} started");

            while (true)
            {
                //while (ThrLock)
                //{
                //    System.Threading.Thread.Sleep(1);
                //}

                //ThrLock = true;

                //Log($"Thr {System.Threading.Thread.CurrentThread.Name} before lock");

                CollectLines:


                lock (ThrItr)
                {
                    //Log($"Thr {System.Threading.Thread.CurrentThread.Name} inside lock");


                    for (int i = 0; i < lineLimit; i++)
                    {
                        if (ThrItr.MoveNext())
                        {
                            sb.AppendLine(ThrItr.Current);
                            lineCount++;
                            // Log($"Thr {System.Threading.Thread.CurrentThread.Name} - {s}");
                        }
                        else
                        {
                            if (lineCount == 0) //Nothing to save
                            {
                                Log($"Thr {System.Threading.Thread.CurrentThread.Name} end reached");
                                goto endVoid;
                            }
                            else
                            {
                                Log($"Thr {System.Threading.Thread.CurrentThread.Name} end reached. Saving...");
                                goto saveAndEndVoid;
                            }
                        }

                    }



                    // Log($"Thr {System.Threading.Thread.CurrentThread.Name} got s");
                }

                //ThrLock = false;
                //Log($"Thr {System.Threading.Thread.CurrentThread.Name} write line {sb.Length}");

                if (ThrLock && lineCount < 10000)
                {
                    lineLimit = 1000;
                    goto CollectLines;
                }



                string s = sb.ToString();
                sb.Clear();
                lineCount = 0;
                lineLimit = 1000;

                lock (ThrdSW)
                {
                    ThrLock = true;
                    ThrdSW.WriteLine(s);
                    ThrLock = false;
                }

            }


            saveAndEndVoid:

            string s2 = sb.ToString();
            sb.Clear();

            lock (ThrdSW)
            {
                ThrdSW.WriteLine(s2);
            }



            endVoid:

            runningThreads--;
            Log($"{nOfThreadsToUse - runningThreads} of {nOfThreadsToUse} threads completed");

        }



        ulong current;

        public IEnumerator<string> GenerateIterator()
        {

            var ChCount = Chars.Count;

            string S;
            current = 0;


            CheckAndStartReporter();

            for (var L = min; L <= max; L++)
            {
                int LMinus1 = L - 1;

                short[] A = new short[L];
                A[0] = -1;

                short Ai;

                for (current = 1UL; current < Math.Pow(ChCount, L) + 1; current++) // A HUGE NUMBER
                {
                    S = prefix;


                    A[0] += 1;

                    for (var i = 0; i < L; i++)
                    {
                        Ai = A[i];

                        if (Ai == ChCount && i != LMinus1)
                        {
                            A[i + 1] += 1;
                            A[i] = 0;
                            Ai = 0;
                        }

                        S += Chars[Ai];
                    }


                    if (hasSufix)
                        S += sufix;

                    //SW.WriteLine(S);
                    yield return S;
                }

                ntotal += current;

            }
            current = 0;
        }

        public IEnumerable<string> GenerateEnumerable()
        {
            var ChCount = Chars.Count;

            string S;
            current = 0;

            CheckAndStartReporter();

            for (var L = min; L <= max; L++)
            {
                int LMinus1 = L - 1;

                short[] A = new short[L];
                A[0] = -1;

                short Ai;

                for (current = 1UL; current < Math.Pow(ChCount, L) + 1; current++) // A HUGE NUMBER
                {
                    S = prefix;


                    A[0] += 1;

                    for (var i = 0; i < L; i++)
                    {
                        Ai = A[i];

                        if (Ai == ChCount && i != LMinus1)
                        {
                            A[i + 1] += 1;
                            A[i] = 0;
                            Ai = 0;
                        }

                        S += Chars[Ai];
                    }


                    if (hasSufix)
                        S += sufix;

                    //SW.WriteLine(S);
                    yield return S;
                }

                ntotal += current;

            }
            current = 0;
        }


        public ulong CalculateFileSize()
        {
            ulong length = 0;
            var ChCount = Chars.Count;

            for (var L = min; L <= max; L++)
            {
                length += (ulong)(Math.Pow(ChCount, L) * 8);
            }

            return length;
        }

        private void CheckAndStartReporter()
        {
            if (needProgressReport)
            {
                Tmr = new Timer(1000);
                Tmr.Elapsed += ProgressReport;
                Tmr.Start();
                stopwatch.Start();
            }
        }

        private ulong lastCurrent = 0;
        private long lastMillies = 1;
        public void ProgressReport(object sender, ElapsedEventArgs e)
        {

            if (total == 0)
            {
                return;
            }

            long millisecond = stopwatch.ElapsedMilliseconds;

            ulong n = ntotal + current;
            Log($"Generating {n} of {total} \t {(n * 100.0F / (float)total).ToString("0.0")}% \t speed : {((n - lastCurrent) * 1000f / (millisecond - lastMillies)).ToString("0")} per second");


            lastCurrent = n;
            lastMillies = millisecond;

            if (ntotal >= total)
            {
                Log("\n ____________________________\n Completed\n ____________________________");
                Tmr.Stop();
                stopwatch.Reset();
            }
        }


        public BFLG()
        {

        }


        public BFLG(int MIN, int MAX, string FAMILIESPROMPT, string FILENAME = "get")
        {
            min = MIN;
            max = MAX;
            familiesPrompt = FAMILIESPROMPT;
            FileName = FILENAME;
        }
    }
}