using Android.Content;
using Android.Content.PM;
using WorkoutTracker.Data;
using WorkoutTracker.Models;

namespace WorkoutTracker.Views
{
    [Activity(
    Label = "Workout Tracker",
    MainLauncher = true,
    ScreenOrientation = ScreenOrientation.Portrait,
    ConfigurationChanges = ConfigChanges.Orientation | ConfigChanges.ScreenSize | ConfigChanges.KeyboardHidden,
    LaunchMode = LaunchMode.SingleTop)]
    public class MainActivity : Activity
    {
        private List<Exercise> exercises;
        private DatabaseHelper db;
        protected override void OnCreate(Bundle? savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            SetContentView(Resource.Layout.activity_main);
            RequestedOrientation = ScreenOrientation.Portrait;

            var btnManageTemplates = FindViewById<Button>(Resource.Id.btnManageTemplates)
                ?? throw new NullReferenceException("btnManageTemplates not found");
            var btnViewStats = FindViewById<Button>(Resource.Id.btnViewStats)
                ?? throw new NullReferenceException("btnViewStats not found");
            var btnNewWorkout = FindViewById<Button>(Resource.Id.btnNewWorkout)
                ?? throw new NullReferenceException("btnStartWorkout not found");

            btnManageTemplates.Click += (s, e) => StartActivity(typeof(CreateTemplateActivity));
            btnViewStats.Click += (s, e) => StartActivity(typeof(ViewStatsActivity));
            btnNewWorkout.Click += (s, e) =>
            {
                var intentTemplate = new Intent(this, typeof(StartWorkoutActivity));
                StartActivity(intentTemplate);
            };
            db = new DatabaseHelper();
        }
        protected override void OnDestroy()
        {
            DatabaseHelper.Instance.CloseDatabase().Wait();
            base.OnDestroy();
        }
        public override void OnBackPressed()
        {
            FinishAffinity();
            Android.OS.Process.KillProcess(Android.OS.Process.MyPid());
        }
    }
}
