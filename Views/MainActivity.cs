using Android.App;
using Android.Content;
using Android.OS;
using Android.Widget;
using WorkoutTracker.Data;
using WorkoutTracker.Models;

namespace WorkoutTracker.Views
{
    [Activity(Label = "Workout Tracker", MainLauncher = true)]
    public class MainActivity : Activity
    {
        private List<Exercise> exercises;
        private DatabaseHelper db;
        protected override void OnCreate(Bundle? savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            SetContentView(Resource.Layout.activity_main);

            var btnCreateTemplate = FindViewById<Button>(Resource.Id.btnCreateTemplate)
                ?? throw new NullReferenceException("btnCreateTemplate not found");
            var btnViewStats = FindViewById<Button>(Resource.Id.btnViewStats)
                ?? throw new NullReferenceException("btnViewStats not found");
            var btnNewWorkout = FindViewById<Button>(Resource.Id.btnNewWorkout)
                ?? throw new NullReferenceException("btnStartWorkout not found");

            btnCreateTemplate.Click += (s, e) => StartActivity(typeof(CreateTemplateActivity));
            btnViewStats.Click += (s, e) => StartActivity(typeof(ViewStatsActivity));
            btnNewWorkout.Click += (s, e) =>
            {
                ShowNewWorkoutDialog();
            };
            db = new DatabaseHelper();
        }
        private void ShowNewWorkoutDialog()
        {
            AlertDialog.Builder dialogBuilder = new AlertDialog.Builder(this);
            dialogBuilder.SetTitle("New Workout")
                .SetItems(new string[] { "Start from Template", "Start Blank Workout" }, (sender, e) =>
                {
                    switch (e.Which)
                    {
                        case 0:
                            var intentTemplate = new Intent(this, typeof(StartWorkoutActivity));
                            StartActivity(intentTemplate);
                            break;

                        case 1:
                            var intentBlank = new Intent(this, typeof(WorkoutSessionActivity));
                            StartActivity(intentBlank);
                            break;
                    }
                });

            dialogBuilder.Create().Show();
        }
        protected override void OnDestroy()
        {
            DatabaseHelper.Instance.CloseDatabase().Wait();
            base.OnDestroy();
        }

    }
}
