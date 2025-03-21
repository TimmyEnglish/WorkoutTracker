using Android.App;
using Android.OS;
using Android.Widget;
using AndroidX.AppCompat.App;
using Android.Util;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using WorkoutTracker.Data;
using WorkoutTracker.Models;

namespace WorkoutTracker.Views
{
    [Activity(Label = "Start Workout")]
    public class StartWorkoutActivity : AppCompatActivity
    {
        private DatabaseHelper db;
        private Spinner? spnTemplates = null!;
        private Button? btnStartWorkout = null!;
        private ListView? lvWorkoutExercises = null!;
        private List<WorkoutTemplate> templates = new();
        private ArrayAdapter<string> adapter = null!;
        private List<string> workoutExercisesList = new List<string>();
        private WorkoutTemplate? selectedTemplate;
        protected override async void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            SetContentView(Resource.Layout.activity_start_workout);

            spnTemplates = FindViewById<Spinner>(Resource.Id.spnTemplates);
            btnStartWorkout = FindViewById<Button>(Resource.Id.btnStartWorkout);
            lvWorkoutExercises = FindViewById<ListView>(Resource.Id.lvWorkoutExercises);

            db = new DatabaseHelper();
            selectedTemplate = null;

            btnStartWorkout.Click += BtnStartWorkout_Click;

            await LoadWorkoutTemplates();
        }
        private async Task LoadWorkoutTemplates()
        {
            try
            {
                templates = await db.GetWorkoutTemplatesAsync();

                if (templates == null || templates.Count == 0)
                {
                    Toast.MakeText(this, "No workout templates found. Add one first!", ToastLength.Long).Show();
                    return;
                }
                var templateNames = templates.Select(t => t.Name).ToList();

                adapter = new ArrayAdapter<string>(this, Android.Resource.Layout.SimpleSpinnerDropDownItem, templateNames);
                spnTemplates.Adapter = adapter;
            }
            catch (Exception ex)
            {
                Toast.MakeText(this, "Error loading templates: " + ex.Message, ToastLength.Long).Show();
            }
            spnTemplates.SetSelection(0);
            selectedTemplate = templates[0];
            await LoadWorkoutExercises(selectedTemplate);
        }
        private async void SpnTemplates_ItemSelected(object? sender, AdapterView.ItemSelectedEventArgs e)
        {
            if (templates.Count == 0) return;

            selectedTemplate = templates[e.Position];
            Console.WriteLine($"Selected Template: {selectedTemplate?.Name}, ExerciseSets: {selectedTemplate?.ExerciseSets}");

            await LoadWorkoutExercises(selectedTemplate);
        }
        private void BtnStartWorkout_Click(object? sender, EventArgs e)
        {
            if (selectedTemplate == null)
            {
                Toast.MakeText(this, "Please select a workout template first!", ToastLength.Short).Show();
                return;
            }

            // Start the workout using selectedTemplate (handle this logic)
            Toast.MakeText(this, $"Starting {selectedTemplate.Name} workout!", ToastLength.Short).Show();
        }
        private async Task LoadWorkoutExercises(WorkoutTemplate template)
        {
            lvWorkoutExercises.Adapter = null; // Clear old data

            var exerciseList = new List<string>();

            if (!string.IsNullOrEmpty(template.ExerciseSets))
            {
                var setEntries = template.ExerciseSets.Split(',');

                foreach (var entry in setEntries)
                {
                    var parts = entry.Split(':');
                    if (parts.Length == 2 && int.TryParse(parts[0], out int exerciseId) && int.TryParse(parts[1], out int setCount))
                    {
                        var exercise = await db.GetExerciseByIdAsync(exerciseId); // Fetch exercise name
                        if (exercise != null)
                        {
                            exerciseList.Add($"{exercise.Name} - {setCount} Sets");
                        }
                    }
                }
            }

            if (exerciseList.Count == 0)
            {
                exerciseList.Add("No exercises found in this template.");
            }

            var adapter = new ArrayAdapter<string>(this, Android.Resource.Layout.SimpleListItem1, exerciseList);
            lvWorkoutExercises.Adapter = adapter;
        }

    }
}
