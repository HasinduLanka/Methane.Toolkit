using System;
using System.Threading;
using StringBuilder = System.Text.StringBuilder;

namespace UniUI
{
    public class ManagedCLI : UniUI.IUniCLI
    {

        readonly Action<string> SetConsole;
        readonly Action<string> setStatus;
        readonly Func<string, string> prompt;

        string[] Chunks;
        int[] ChunkSizes;

        int curChunkIndex;
        readonly StringBuilder Chunk = new StringBuilder();
        int curChunkSize;

        public StringBuilder Display = new StringBuilder();
        int DisplaySize = 0;
        string DisplayString = "";

        readonly int MaxChunks, ChunkSize, MaxConsoleSize, LinePenalty;

        public ManagedCLI(Action<string> SetConsoleText, Action<string> setStatusText, Func<string, string> promptFunc, int maxChunks = 64, int chunkSize = 256, int linePenalty = 10)
        {
            SetConsole = SetConsoleText;
            setStatus = setStatusText;
            prompt = promptFunc;

            MaxChunks = maxChunks;
            ChunkSize = chunkSize;
            MaxConsoleSize = MaxChunks * ChunkSize;
            LinePenalty = linePenalty;

            Chunks = new string[maxChunks];
            ChunkSizes = new int[maxChunks];
            curChunkIndex = 0;
        }

        public void Log(string s)
        {
            LogAppend(s + System.Environment.NewLine, 1);
        }


        public void LogAppend(string s, int numberOfLines)
        {

            int lineSize = s.Length + (LinePenalty * numberOfLines);

            lock (Display)
            {
                Display.Append(s);
                DisplaySize += lineSize;

                Chunk.Append(s);
                curChunkSize += lineSize;

                if (curChunkSize > ChunkSize)
                {


                    Chunks[curChunkIndex] = Chunk.ToString();
                    Chunk.Clear();


                    ChunkSizes[curChunkIndex] = curChunkSize;

                    curChunkIndex++;

                    if (curChunkIndex == MaxChunks)
                    {
                        curChunkIndex = 0;
                    }

                    curChunkSize = 0;

                    //DisplaySize -= ChunkSizes[curChunkIndex];
                    ChunkSizes[curChunkIndex] = 0;
                    Chunks[curChunkIndex] = "";
                }

                if (LazyUpdating)
                {
                    RectifyDisplay();
                }
                else
                {
                    new Thread(LazyUpdate) { Name = "DroidCLI Lazy Update" }.Start();

                }
            }
        }



        private void RectifyDisplay()
        {



            if (DisplaySize > MaxConsoleSize)
            {
                lock (Display)
                {
                    Display.Clear();
                    DisplaySize = 0;

                    for (int i = curChunkIndex + 1; i < MaxChunks; i++)
                    {
                        Display.Append(Chunks[i]);
                        DisplaySize += ChunkSizes[i];
                    }


                    for (int i = 0; i < curChunkIndex; i++)
                    {
                        Display.Append(Chunks[i]);
                        DisplaySize += ChunkSizes[i];
                    }

                }
            }
        }


        bool LazyUpdating = false;
        void LazyUpdate()
        {
            if (LazyUpdating) return;

            LazyUpdating = true;

            while (IsOnHold)
            {
                System.Threading.Thread.Sleep(200);
            }


            System.Threading.Thread.Sleep(500);




            lock (Display)
            {
                DisplayString = Display.ToString();
            }

            SetConsole(DisplayString);



            LazyUpdating = false;

        }
        public void LogSpecial(string s)
        {
            Log("-_-_-_-_-_-_-_-\n" +
                 s +
                 "\n_-_-_-_-_-_-_-_-");
        }

        public void LogError(Exception ex, string Msg = "")
        {
            string ThrName = System.Threading.Thread.CurrentThread.Name;


            Log("x-------------------x\n" +
 $"{TimeStamp} \t {Msg} {ex} - {ex?.Message} @@ {ex?.StackTrace} \t {(ThrName == null ? null : $"@ {ThrName}")}" +
                 "\nx------------------ - x");
        }

        public static string TimeStamp { get { return $"{DateTime.Now.Hour:00}:{DateTime.Now.Minute:00}:{DateTime.Now.Second:00)}.{DateTime.Now.Millisecond:00}"; } }



        public string Prompt(string s)
        {
            // Hold();
            string res = prompt(s);
            // Unhold();
            Log(s);
            Log("\t\t>>> " + res);
            return res;
        }


        public bool IsOnHold = false;
        public void Hold()
        {
            IsOnHold = true;
        }

        public void Unhold()
        {
            IsOnHold = false;
        }
        public bool ToggleHold()
        {
            IsOnHold = !IsOnHold;
            return IsOnHold;
        }

        public void Clear()
        {

            lock (Display)
            {

                Chunks = new string[MaxChunks];
                ChunkSizes = new int[MaxChunks];
                curChunkIndex = 0;

                Chunk.Clear();
                Display.Clear();

                DisplayString = "";
                new Thread(LazyUpdate) { Name = "DroidCLI Lazy Clear" }.Start();
            }
        }

        public void SetStatus(string s)
        {
            setStatus(s);
        }


    }
}