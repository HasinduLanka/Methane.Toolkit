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

    [Activity(Label = "Methane Toolkit", Theme = "@style/AppTheme.NoActionBar", MainLauncher = true)]
    public class MainActivity : AppCompatActivity
    {
        CoordinatorLayout CLIViewContainer;
        public CLIView CLIView;
        public Dictionary<int, SubProgram> Programs = new Dictionary<int, SubProgram>();


        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            Xamarin.Essentials.Platform.Init(this, savedInstanceState);
            SetContentView(Resource.Layout.activity_main);


            CLIViewContainer = FindViewById<CoordinatorLayout>(Resource.Id.DroidCLIView);

            System.Environment.CurrentDirectory = System.Environment.GetFolderPath(System.Environment.SpecialFolder.LocalApplicationData);
            AddCLI(0, AddNewMethaneToolKit);
            //ShowCLI(0);

        }

        public void AddCLI(int id, ParameterizedThreadStart prog)
        {
            RemoveCLI(id);


            CLIView view = new CLIView(id, CLIViewContainer, this, CLIView.DefaultBackColor);
            Thread thread = new Thread(new ParameterizedThreadStart(prog));

            SubProgram program = new SubProgram() { view = view, Thread = thread };
            Programs[id] = program;
            program.Thread.Start(view.CLI);

        }


        public static void AddNewMethaneToolKit(object uniUI)
        {
            UniUI.IUniCLI ui = (UniUI.IUniCLI)uniUI;

            Methane.Toolkit.Program p = new Methane.Toolkit.Program();
            p.Main(Array.Empty<string>(), ui);
        }

        public void RemoveCLI(int id)
        {
            if (Programs.TryGetValue(id, out SubProgram old))
            {
                old.Terminate();
                RunOnUiThread(() =>
                {
                    old.view.Parent.Visibility = ViewStates.Gone;
                    old.view.Parent.Dispose();
                });
                Programs.Remove(id);
            }
        }

        public void ShowCLI(int id)
        {
            if (Programs.TryGetValue(id, out SubProgram prog))
            {

                RunOnUiThread(() =>
            {
                CLIViewContainer.AddView(prog.view.Parent);
                prog.view.Parent.Visibility = ViewStates.Visible;
            });

            }

        }

        public void HideCLI(int id)
        {
            if (Programs.TryGetValue(id, out SubProgram prog))
            {

                RunOnUiThread(() =>
                {
                    CLIViewContainer.RemoveView(prog.view.Parent);
                    prog.view.Parent.Visibility = ViewStates.Invisible;
                });

            }

        }

        public override void OnBackPressed()
        {
            AddCLI(0, AddNewMethaneToolKit);
        }

        public override void OnRequestPermissionsResult(int requestCode, string[] permissions, [GeneratedEnum] Android.Content.PM.Permission[] grantResults)
        {
            Xamarin.Essentials.Platform.OnRequestPermissionsResult(requestCode, permissions, grantResults);

            base.OnRequestPermissionsResult(requestCode, permissions, grantResults);
        }
    }
}

