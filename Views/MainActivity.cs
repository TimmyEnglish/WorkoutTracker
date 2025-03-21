using Android.App;
using Android.Content;
using Android.OS;
using Android.Widget;
using WorkoutTracker.Data;

namespace WorkoutTracker.Views
{
    [Activity(Label = "Workout Tracker", MainLauncher = true)]
    public class MainActivity : Activity
    {
        protected override void OnCreate(Bundle? savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            SetContentView(Resource.Layout.activity_main);

            var btnCreateTemplate = FindViewById<Button>(Resource.Id.btnCreateTemplate)
                ?? throw new NullReferenceException("btnCreateTemplate not found");
            var btnViewStats = FindViewById<Button>(Resource.Id.btnViewStats)
                ?? throw new NullReferenceException("btnViewStats not found");
            var btnStartWorkout = FindViewById<Button>(Resource.Id.btnStartWorkout)
                ?? throw new NullReferenceException("btnStartWorkout not found");

            btnCreateTemplate.Click += (s, e) => StartActivity(typeof(CreateTemplateActivity));
            btnViewStats.Click += (s, e) => StartActivity(typeof(ViewStatsActivity));
            btnStartWorkout.Click += (s, e) => StartActivity(typeof(StartWorkoutActivity));
        }
        protected override void OnDestroy()
        {
            DatabaseHelper.Instance.CloseDatabase().Wait();
            base.OnDestroy();
        }

    }
}
