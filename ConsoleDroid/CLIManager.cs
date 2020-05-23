using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Support.Design.Internal;
using Android.Support.Design.Widget;
using Android.Views;
using Android.Widget;

namespace ConsoleDroid
{
    public class CLIManager
    {
        readonly Activity activity;
        int nPrograms = 0;
        public Dictionary<int, SubProgram> Programs = new Dictionary<int, SubProgram>();
        public Dictionary<string, Action<object>> ExcecutablePrograms;
        public CoordinatorLayout Parent;


        ScrollView scrollView;
        FlowLayout flowLayout;
        Dictionary<int, FlowLayout> ProgramMenu;

        FlowLayout NewProg;
        Spinner combo;

        public bool IsMenuShown = true;
        readonly Random Random = new Random();

        public CLIManager(Activity ThisActivity, CoordinatorLayout ViewContainer, Dictionary<string, Action<object>> excecutablePrograms, ref Func<bool> backpressed)
        {
            backpressed = BackPressed;
            activity = ThisActivity;
            Parent = ViewContainer;
            ExcecutablePrograms = excecutablePrograms;

            List<string> ProgramNames = excecutablePrograms.Keys.ToList();


            activity.RunOnUiThread(() =>
            {
                scrollView = new ScrollView(activity);

                flowLayout = new FlowLayout(activity);
                scrollView.AddView(flowLayout, new ViewGroup.LayoutParams(ViewGroup.LayoutParams.MatchParent, ViewGroup.LayoutParams.WrapContent));

                ProgramMenu = new Dictionary<int, FlowLayout>();

                NewProg = new FlowLayout(activity);

                combo = new Spinner(activity)
                {
                    Adapter = new ArrayAdapter(activity, Android.Resource.Layout.SimpleSpinnerItem, ProgramNames)
                };
                NewProg.AddView(combo, new FlowLayout.LayoutParams(FlowLayout.LayoutParams.MatchParent, FlowLayout.LayoutParams.MatchParent));


                Button BtnAdd = new Button(activity) { Text = "Add Program" };
                NewProg.AddView(BtnAdd, new FlowLayout.LayoutParams(FlowLayout.LayoutParams.MatchParent, FlowLayout.LayoutParams.MatchParent));
                BtnAdd.Click += BtnAdd_Click;


            });

            ShowMenu();
        }



        private void BtnAdd_Click(object sender, EventArgs e)
        {
            if (combo.SelectedItemPosition < 0)
            {
                activity.RunOnUiThread(() =>
                {
                    combo.SetSelection(0, true);
                });
                return;
            }


            nPrograms++;
            var ele = ExcecutablePrograms.ElementAt(combo.SelectedItemPosition);
            AddCLI(nPrograms, ele.Key, ele.Value);

            HideMenu();
        }

        public bool BackPressed()
        {
            if (IsMenuShown)
                return true;

            ShowMenu();
            return false;

        }


        public void ShowMenu()
        {
            IsMenuShown = true;

            activity.RunOnUiThread(() =>
            {
                Parent.RemoveAllViews();
                Parent.AddView(scrollView, new ViewGroup.LayoutParams(ViewGroup.LayoutParams.MatchParent, ViewGroup.LayoutParams.WrapContent));

                combo.SetSelection(0, false);

                flowLayout.RemoveAllViews();
                foreach (var fl in ProgramMenu.Values)
                {
                    fl.Dispose();
                }


                flowLayout.AddView(NewProg, new FlowLayout.LayoutParams(FlowLayout.LayoutParams.MatchParent, FlowLayout.LayoutParams.WrapContent));

                ProgramMenu = new Dictionary<int, FlowLayout>();

                foreach (var prog in Programs)
                {
                    FlowLayout pfl = new FlowLayout(activity);
                    pfl.SetBackgroundColor(prog.Value.Color);
                    flowLayout.AddView(pfl, new FlowLayout.LayoutParams(FlowLayout.LayoutParams.MatchParent, FlowLayout.LayoutParams.WrapContent));

                    TextView pname = new TextView(activity) { Text = prog.Key.ToString() + ". " + prog.Value.name };
                    pfl.AddView(pname, new ViewGroup.LayoutParams(ViewGroup.LayoutParams.MatchParent, ViewGroup.LayoutParams.MatchParent));


                    Button BtnOpen = new Button(activity) { Text = "Open", Tag = prog.Key };
                    pfl.AddView(BtnOpen, new FlowLayout.LayoutParams(FlowLayout.LayoutParams.MatchParent, FlowLayout.LayoutParams.MatchParent));

                    BtnOpen.Click += ((object sender, EventArgs e) =>
                    {
                        HideMenu();
                        ShowCLI(prog.Key);
                    });

                    Button BtnKill = new Button(activity) { Text = "Terminate", Tag = prog.Key };
                    pfl.AddView(BtnKill, new FlowLayout.LayoutParams(FlowLayout.LayoutParams.MatchParent, FlowLayout.LayoutParams.MatchParent));

                    BtnKill.Click += ((object sender, EventArgs e) =>
                    {
                        RemoveCLI(prog.Key);
                    });


                    ProgramMenu.Add(prog.Key, pfl);
                }



            });
        }

        public void HideMenu()
        {
            IsMenuShown = false;
            activity.RunOnUiThread(() =>
            {
                Parent.RemoveView(scrollView);

            });
        }


        /// <summary>
        /// Prog must be a Prog(UniUI.UniCLI obj)
        /// </summary>
        public void AddCLI(int id, string name, Action<object> prog)
        {
            RemoveCLI(id);


            int grey = Random.Next(0, 10);
            var Color = new Android.Graphics.Color(grey + Random.Next(0, 50), grey + Random.Next(0, 50), grey + Random.Next(0, 50));


            CLIView view = new CLIView(id, Parent, activity, Color);
            Thread thread = new Thread(new ParameterizedThreadStart(prog));

            SubProgram program = new SubProgram()
            {
                Thread = thread,
                name = name,
                Color = Color,
                view = view
            };


            Programs[id] = program;
            program.Thread.Start(view.CLI);

        }




        public void RemoveCLI(int id)
        {
            if (Programs.TryGetValue(id, out SubProgram old))
            {
                old.Terminate();
                activity.RunOnUiThread(() =>
                {
                    old.view.Parent.Visibility = ViewStates.Gone;
                    old.view.Parent.Dispose();
                });
                Programs.Remove(id);

                if (IsMenuShown)
                {
                    ShowMenu();
                }
            }
        }

        public void ShowCLI(int id)
        {
            if (Programs.TryGetValue(id, out SubProgram prog))
            {

                activity.RunOnUiThread(() =>
                {
                    Parent.AddView(prog.view.Parent);
                    prog.view.Parent.Visibility = ViewStates.Visible;
                });

            }

        }

        public void HideCLI(int id)
        {
            if (Programs.TryGetValue(id, out SubProgram prog))
            {

                activity.RunOnUiThread(() =>
                {
                    Parent.RemoveView(prog.view.Parent);
                    prog.view.Parent.Visibility = ViewStates.Invisible;
                });

            }

        }

    }
}