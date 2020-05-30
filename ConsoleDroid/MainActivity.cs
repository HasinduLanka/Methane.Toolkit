using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Timers;
using Android.App;
using Android.Graphics;
using Android.Graphics.Drawables;
using Android.OS;
using Android.Runtime;
using Android.Support.Design.Widget;
using Android.Support.V7.App;
using Android.Views;
using Android.Widget;
using Xamarin.Essentials;

namespace ConsoleDroid
{

    [Activity(Label = "Methane Toolkit", WindowSoftInputMode = SoftInput.AdjustResize, Theme = "@style/AppTheme.NoActionBar", MainLauncher = true)]
    public class MainActivity : AppCompatActivity
    {
        CoordinatorLayout CLIViewContainer;
        public CLIManager manager;


        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            Xamarin.Essentials.Platform.Init(this, savedInstanceState);
            SetContentView(Resource.Layout.activity_main);


            CLIViewContainer = FindViewById<CoordinatorLayout>(Resource.Id.DroidCLIView);

            System.Environment.CurrentDirectory = System.Environment.GetFolderPath(System.Environment.SpecialFolder.LocalApplicationData);

            Dictionary<string, Action<object>> progs = new Dictionary<string, Action<object>>
            {
                { "Methane toolkit", AddNewMethaneToolKit },
                { "Speed Tester with BruteForce Gen", AddNewMethaneToolKitBFLGSpeedTest },
                { "Methane Rapid Downloader", AddNewMethaneToolKitDownloader }
            };


            manager = new CLIManager(this, CLIViewContainer, progs, ref BackPressed);

        }
        public static void AddNewMethaneToolKit(object uniUI)
        {
            UniUI.IUniUI ui = (UniUI.IUniUI)uniUI;

            Methane.Toolkit.Program p = new Methane.Toolkit.Program();
            p.Main(Array.Empty<string>(), ui);
        }

        public static void AddNewMethaneToolKitBFLGSpeedTest(object uniUI)
        {
            UniUI.IUniUI ui = (UniUI.IUniUI)uniUI;

            Methane.Toolkit.Program p = new Methane.Toolkit.Program();
            p.Main(new string[] { "#|bflg|c|1|3|8|1" }, ui);
        }

        public static void AddNewMethaneToolKitDownloader(object uniUI)
        {
            UniUI.IUniUI ui = (UniUI.IUniUI)uniUI;

            Methane.Toolkit.Program p = new Methane.Toolkit.Program();
            p.Main(new string[] { "#|download|||~|" }, ui);
        }


        public event Func<bool> BackPressed;
        public override void OnBackPressed()
        {
            if (BackPressed())
            {
                base.OnBackPressed();
            }
        }



        public override void OnRequestPermissionsResult(int requestCode, string[] permissions, [GeneratedEnum] Android.Content.PM.Permission[] grantResults)
        {
            Xamarin.Essentials.Platform.OnRequestPermissionsResult(requestCode, permissions, grantResults);

            base.OnRequestPermissionsResult(requestCode, permissions, grantResults);
        }
    }
}

