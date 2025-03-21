using Android.App;
using Android.OS;
using Android.Widget;
using AndroidX.AppCompat.App;
using System.Collections.Generic;
using System.Threading.Tasks;
using WorkoutTracker.Data;
using WorkoutTracker.Models;

namespace WorkoutTracker.Views
{
    [Activity(Label = "View Stats")]
    public class ViewStatsActivity : AppCompatActivity
    {
        private Spinner spnExercises;
        private TextView txtStats;
        private List<Exercise> exercises;
        private DatabaseHelper db;

        protected override async void OnCreate(Bundle? savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            SetContentView(Resource.Layout.activity_view_stats);

            db = new DatabaseHelper();
            spnExercises = FindViewById<Spinner>(Resource.Id.spnExercises);
            txtStats = FindViewById<TextView>(Resource.Id.txtStats);

            spnExercises.ItemSelected += SpnExercises_ItemSelected;
            await LoadExercises();
        }

        private async Task LoadExercises()
        {
            exercises = await db.GetExercisesAsync();
            var exerciseNames = exercises.ConvertAll(e => e.Name);

            var adapter = new ArrayAdapter<string>(this, Android.Resource.Layout.SimpleSpinnerDropDownItem, exerciseNames);
            spnExercises.Adapter = adapter;
        }

        private async void SpnExercises_ItemSelected(object? sender, AdapterView.ItemSelectedEventArgs e)
        {
            int selectedExerciseId = exercises[e.Position].Id;
            var logs = await db.GetWorkoutLogsByExerciseAsync(selectedExerciseId);

            if (logs.Count == 0)
            {
                txtStats.Text = "No data available.";
                return;
            }

            txtStats.Text = "";
            foreach (var log in logs)
            {
                txtStats.Text += $"Date: {log.Date}\nWeight: {log.Weight}kg\nReps: {log.Reps}\n\n";
            }
        }
    }
}
