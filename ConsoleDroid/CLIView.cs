using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Timers;
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
    public class CLIView
    {
        public int ID;
        readonly Activity activity;

        public readonly CoordinatorLayout Parent;

        readonly RelativeLayout PageParent;
        readonly RelativeLayout ScrollViewParent;
        readonly LinearLayout BottomParent;
        readonly ScrollView scrollView;

        readonly FlowLayout promptPanel;
        readonly EditText promptQ;
        readonly EditText promptA;

        readonly EditText console;
        readonly TextView Status;
        readonly FloatingActionButton FAB;

        public UniUI.ManagedCLI CLI;
        readonly Timer HeartBeat;

        public static Android.Graphics.Color DefaultBackColor = new Android.Graphics.Color(21, 21, 21);

        public CLIView(int id, CoordinatorLayout parentView, Activity ThisActivity, Android.Graphics.Color BackColor)
        {
            ID = id;
            activity = ThisActivity;

            Parent = new CoordinatorLayout(activity);
            parentView.AddView(Parent, new CoordinatorLayout.LayoutParams(CoordinatorLayout.LayoutParams.MatchParent, CoordinatorLayout.LayoutParams.MatchParent));

            Parent.SetBackgroundColor(DefaultBackColor);

            PageParent = new RelativeLayout(activity);
            Parent.AddView(PageParent, new CoordinatorLayout.LayoutParams(ViewGroup.LayoutParams.MatchParent, ViewGroup.LayoutParams.MatchParent));

            ScrollViewParent = new RelativeLayout(activity) { Id = 12345 };
            ScrollViewParent.SetBackgroundColor(DefaultBackColor);

            PageParent.AddView(ScrollViewParent, new RelativeLayout.LayoutParams(ViewGroup.LayoutParams.MatchParent, ViewGroup.LayoutParams.WrapContent));


            scrollView = new ScrollView(activity);
            //scrollView.SetBackgroundColor(Android.Graphics.Color.DarkGreen);

            ScrollViewParent.AddView(scrollView, new RelativeLayout.LayoutParams(ViewGroup.LayoutParams.MatchParent, ViewGroup.LayoutParams.MatchParent));

            console = new EditText(activity)
            {
                InputType = Android.Text.InputTypes.Null,
                Focusable = false,
                OverScrollMode = OverScrollMode.Always,
                ScrollBarStyle = ScrollbarStyles.InsideInset,
                VerticalScrollBarEnabled = true
            };
            console.SetTextIsSelectable(false);
            console.SetSingleLine(false);
            console.SetBackgroundColor(DefaultBackColor);
            console.SetTextColor(Android.Graphics.Color.White);

            console.Text = " Starting ConsoleDroid... ";

            scrollView.AddView(console, new ScrollView.LayoutParams(ViewGroup.LayoutParams.MatchParent, ViewGroup.LayoutParams.WrapContent));



            BottomParent = new LinearLayout(activity) { Id = 12346 };
            //BottomParent.SetBackgroundColor(Android.Graphics.Color.Cyan);
            PageParent.AddView(BottomParent, new RelativeLayout.LayoutParams(ViewGroup.LayoutParams.MatchParent, ViewGroup.LayoutParams.WrapContent));

            var ScrollParams = ScrollViewParent.LayoutParameters as RelativeLayout.LayoutParams;
            var BottomParams = BottomParent.LayoutParameters as RelativeLayout.LayoutParams;

            ScrollParams.AddRule(LayoutRules.AlignParentTop);
            ScrollParams.AddRule(LayoutRules.Above, 12346);
            BottomParams.AddRule(LayoutRules.AlignParentBottom);

            promptPanel = new FlowLayout(activity);
            // promptPanel.SetBackgroundColor(Android.Graphics.Color.Blue);

            BottomParent.AddView(promptPanel, new LinearLayout.LayoutParams(ViewGroup.LayoutParams.MatchParent, ViewGroup.LayoutParams.WrapContent));
            LinearLayout.LayoutParams promptPanelParams = promptPanel.LayoutParameters as LinearLayout.LayoutParams;
            promptPanelParams.Gravity = GravityFlags.Top;

            promptPanel.Visibility = ViewStates.Invisible;


            promptQ = new EditText(activity)
            {
                InputType = Android.Text.InputTypes.Null,
                Focusable = true
            };
            promptQ.SetTextIsSelectable(true);
            promptQ.SetSingleLine(false);
            promptQ.SetBackgroundColor(BackColor);
            promptQ.SetTextColor(Android.Graphics.Color.Yellow);
            promptQ.Text = "How old are you?";
            // promptQ.SetBackgroundColor(Android.Graphics.Color.Blue);

            promptPanel.AddView(promptQ, new FlowLayout.LayoutParams(ViewGroup.LayoutParams.MatchParent, ViewGroup.LayoutParams.WrapContent));

            promptA = new EditText(activity)
            {
                InputType = Android.Text.InputTypes.ClassText,
                Focusable = true
            };
            promptA.SetBackgroundColor(BackColor);
            promptA.SetTextColor(Android.Graphics.Color.LightGoldenrodYellow);
            promptA.SetImeActionLabel("enter", Android.Views.InputMethods.ImeAction.Send);
            promptA.EditorAction += PromptA_EditorAction;
            // promptA.SetBackgroundColor(Android.Graphics.Color.DarkBlue);

            promptPanel.AddView(promptA, new FlowLayout.LayoutParams(ViewGroup.LayoutParams.MatchParent, ViewGroup.LayoutParams.WrapContent));

            promptPanel.Visibility = ViewStates.Gone;



            Status = new TextView(activity)
            {
                TextAlignment = TextAlignment.Center,
                Gravity = GravityFlags.Bottom,
                Text = "status"
            };

            Status.SetBackgroundColor(BackColor);
            Status.SetTextColor(Android.Graphics.Color.Azure);
            BottomParent.AddView(Status, new LinearLayout.LayoutParams(ViewGroup.LayoutParams.MatchParent, ViewGroup.LayoutParams.WrapContent));

            LinearLayout.LayoutParams statusParams = Status.LayoutParameters as LinearLayout.LayoutParams;
            statusParams.Gravity = GravityFlags.Bottom;


            FAB = new FloatingActionButton(activity);
            FAB.SetBackgroundColor(BackColor);
            FAB.SetColorFilter(BackColor);
            FAB.SetOutlineSpotShadowColor(BackColor);
            FAB.SetOutlineAmbientShadowColor(BackColor);

            Parent.AddView(FAB);
            CoordinatorLayout.LayoutParams FABLayout = FAB.LayoutParameters as CoordinatorLayout.LayoutParams;
            FABLayout.Width = -2; FABLayout.Height = -2;
            FABLayout.SetMargins(160, 160, 20, 240);
            FABLayout.Gravity = (int)(GravityFlags.Bottom | GravityFlags.End);
            FAB.SetImageResource(Resource.Drawable.round_pause_circle_filled_24);
            FAB.Click += FabOnClick;





            CLI = new UniUI.ManagedCLI(SetConsoleText, SetStatus, Prompt);

            HeartBeat = new Timer(3000);
            HeartBeat.Elapsed += HeartBeat_Elapsed;
            HeartBeat.Start();
        }


        public void SetConsoleText(string s)
        {
            activity.RunOnUiThread(() =>
            {

                console.Text = s;
                console.RefreshDrawableState();
                //scrollView.ScrollTo(0, (int)((console.LineHeight + console.LineSpacingExtra) * (console.LineCount + 1)));
                scrollView.FullScroll(FocusSearchDirection.Down);

            });
        }

        public void SetStatus(string s)
        {
            activity.RunOnUiThread(() =>
            {
                Status.Text = s;
            });
        }






        bool IsPrompting = false;
        bool PromptAChanged = false;
        readonly Array PromptLock = new int[10];

        public string Prompt(string q)
        {
            while (IsPrompting)
            {
                System.Threading.Thread.Sleep(100);
            }

            lock (PromptLock)
            {
                IsPrompting = true;
                PromptAChanged = false;

                activity.RunOnUiThread(() =>
                {
                    promptPanel.Visibility = ViewStates.Visible;
                    promptQ.Text = q;
                    promptA.Text = "";

                    // scrollViewParams.AddRule(LayoutRules.)
                    //scrollView.LayoutParameters.Height = stackView.Height - promptPanel.Height;
                    promptA.RequestFocus();
                });



                while (!PromptAChanged)
                {
                    System.Threading.Thread.Sleep(500);

                    activity.RunOnUiThread(() =>
                    {
                        // scrollViewParams.Gravity = GravityFlags.Top;
                        // scrollView.LayoutParameters.Height = stackView.Height - promptPanel.Height;
                        // scrollView.FullScroll(FocusSearchDirection.Down);
                        // scrollView.RefreshDrawableState();
                    });
                }
                string res = promptA.Text;
                PromptAChanged = false;

                activity.RunOnUiThread(() =>
                {
                    promptQ.Text = "";
                    promptPanel.Visibility = ViewStates.Gone;
                });


                IsPrompting = false;
                return res;
            }
        }


        private void PromptA_EditorAction(object sender, TextView.EditorActionEventArgs e)
        {
            PromptAChanged = true;
        }

        private void FabOnClick(object sender, EventArgs eventArgs)
        {
            // View view = (View)sender;
            // Snackbar.Make(view, "Replace with your own action", Snackbar.LengthLong)
            //     .SetAction("Action", (Android.Views.View.IOnClickListener)null).Show();


            if (CLI.ToggleHold())
            {
                console.Focusable = true;
                console.SetTextIsSelectable(true);
                FAB.SetImageResource(Resource.Drawable.round_play_circle_filled_24);
            }
            else
            {
                console.SetTextIsSelectable(false);
                console.Focusable = false;
                FAB.SetImageResource(Resource.Drawable.round_pause_circle_filled_24);
            }
        }


        uint HeartBeats = 0;
        private void HeartBeat_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
        {
            HeartBeats++;

            CLI.SetStatus($"{GC.GetTotalMemory(false) / 1048576} MB ");

            if (!CLI.IsOnHold)
            {
                activity.RunOnUiThread(() =>
                {
                    scrollView.FullScroll(FocusSearchDirection.Down);

                });
            }


            if (HeartBeats % 8 == 0)
            {

            }

        }


    }
}