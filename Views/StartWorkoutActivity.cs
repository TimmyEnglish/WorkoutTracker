using AndroidX.AppCompat.App;
using WorkoutTracker.Data;
using Android.Content;

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
        private WorkoutTemplate? selectedTemplate;

        protected override async void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            SetContentView(Resource.Layout.activity_start_workout);

            spnTemplates = FindViewById<Spinner>(Resource.Id.spnTemplates);
            btnStartWorkout = FindViewById<Button>(Resource.Id.btnStartWorkout);
            lvWorkoutExercises = FindViewById<ListView>(Resource.Id.lvWorkoutExercises);

            db = new DatabaseHelper();

            btnStartWorkout.Click += BtnStartWorkout_Click;
            spnTemplates.ItemSelected += SpnTemplates_ItemSelected;

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

                spnTemplates.SetSelection(0);
                selectedTemplate = templates[0];
                await LoadWorkoutExercises(selectedTemplate);
            }
            catch (Exception ex)
            {
                Toast.MakeText(this, "Error loading templates: " + ex.Message, ToastLength.Long).Show();
            }
        }

        private async void SpnTemplates_ItemSelected(object? sender, AdapterView.ItemSelectedEventArgs e)
        {
            if (templates == null || templates.Count == 0) return;
            selectedTemplate = templates[e.Position];
            await LoadWorkoutExercises(selectedTemplate);
        }

        private void BtnStartWorkout_Click(object? sender, EventArgs e)
        {
            if (selectedTemplate == null)
            {
                Toast.MakeText(this, "Please select a workout template first!", ToastLength.Short).Show();
                return;
            }

            string cleanedData = CleanExerciseSetString(selectedTemplate.ExerciseSets);
            Intent intent = new Intent(this, typeof(WorkoutSessionActivity));
            intent.PutExtra("ExerciseSets", cleanedData);
            StartActivity(intent);
        }

        private string CleanExerciseSetString(string exerciseSets)
        {
            if (string.IsNullOrEmpty(exerciseSets)) return string.Empty;

            var cleanedEntries = exerciseSets
                .Split(',')
                .Select(entry =>
                {
                    var parts = entry.Split(':');
                    return parts.Length >= 2 ? $"{parts[0]}:{parts[1]}" : null;
                })
                .Where(x => x != null);

            return string.Join(",", cleanedEntries!);
        }

        private async Task LoadWorkoutExercises(WorkoutTemplate template)
        {
            if (lvWorkoutExercises == null) return;
            lvWorkoutExercises.Adapter = null;

            var exerciseList = new List<string>();

            if (!string.IsNullOrEmpty(template.ExerciseSets))
            {
                var setEntries = template.ExerciseSets.Split(',');

                foreach (var entry in setEntries)
                {
                    var parts = entry.Split(':');
                    if (parts.Length >= 2 &&
                        int.TryParse(parts[0], out int exerciseId) &&
                        int.TryParse(parts[1], out int setCount))
                    {
                        var exercise = await db.GetExerciseByIdAsync(exerciseId);
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

            RunOnUiThread(() =>
            {
                var adapter = new ArrayAdapter<string>(this, Android.Resource.Layout.SimpleListItem1, exerciseList);
                lvWorkoutExercises.Adapter = adapter;
                adapter.NotifyDataSetChanged();
            });
        }
    }
}
