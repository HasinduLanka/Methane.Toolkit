using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;

namespace ConsoleDroid
{
    public class SubProgram
    {
        public string name;
        public CLIView view;
        public Thread Thread;
        public Android.Graphics.Color Color;

        public void Terminate()
        {
            Thread.Abort();
        }
    }
}