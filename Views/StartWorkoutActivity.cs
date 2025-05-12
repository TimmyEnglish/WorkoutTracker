using AndroidX.AppCompat.App;
using Android.Content;
using WorkoutTracker.Data;
using Android.Content.PM;
using Android.Content.Res;

namespace WorkoutTracker.Views
{
    [Activity(
    Label = "Start Workout",
    ScreenOrientation = ScreenOrientation.Portrait,
    ConfigurationChanges = ConfigChanges.Orientation | ConfigChanges.ScreenSize | ConfigChanges.KeyboardHidden)]

    public class StartWorkoutActivity : AppCompatActivity
    {
        private DatabaseHelper db;
        private Spinner? spnTemplates = null!;
        private Button? btnStartFromTemplate = null!;
        private ListView? lvWorkoutExercises = null!;
        private List<WorkoutTemplate> templates = new();
        private ArrayAdapter<string> adapter = null!;
        private WorkoutTemplate? selectedTemplate;
        protected override async void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            SetContentView(Resource.Layout.activity_start_workout);
            RequestedOrientation = ScreenOrientation.Portrait;

            bool isDarkTheme = (Resources.Configuration.UiMode & UiMode.NightMask) == UiMode.NightYes;
            if (isDarkTheme && SupportActionBar != null)
            {
                var color = Android.Graphics.Color.ParseColor("#222222");
                SupportActionBar.SetBackgroundDrawable(new Android.Graphics.Drawables.ColorDrawable(color));
            }

            spnTemplates = FindViewById<Spinner>(Resource.Id.spnTemplates);
            btnStartFromTemplate = FindViewById<Button>(Resource.Id.btnStartFromTemplate);
            lvWorkoutExercises = FindViewById<ListView>(Resource.Id.lvWorkoutExercises);

            db = new DatabaseHelper();

            btnStartFromTemplate.Click += BtnStartFromTemplate_Click;

            // Load templates and display them on spinner
            await LoadWorkoutTemplates();

            // When a template is selected, load the exercises
            spnTemplates.ItemSelected += SpnTemplates_ItemSelected;
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

                var templateNames = new List<string> { "-- Select a template --" };
                templateNames.AddRange(templates.Select(t => t.Name));
                adapter = new ArrayAdapter<string>(this, Android.Resource.Layout.SimpleSpinnerDropDownItem, templateNames);
                spnTemplates.Adapter = adapter;

                spnTemplates.SetSelection(0); // Placeholder selected by default
                selectedTemplate = null;
                LoadDefaultWorkoutMessage(); // Show default message in ListView
            }
            catch (Exception ex)
            {
                Toast.MakeText(this, "Error loading templates: " + ex.Message, ToastLength.Long).Show();
            }
        }
        private void LoadDefaultWorkoutMessage()
        {
            if (lvWorkoutExercises == null) return;

            var defaultMsg = new List<string> { "You can select a workout template, or start an empty workout." };
            RunOnUiThread(() =>
            {
                var adapter = new ArrayAdapter<string>(this, Android.Resource.Layout.SimpleListItem1, defaultMsg);
                lvWorkoutExercises.Adapter = adapter;
                adapter.NotifyDataSetChanged();
            });
        }
        private async void SpnTemplates_ItemSelected(object? sender, AdapterView.ItemSelectedEventArgs e)
        {
            if (e.Position == 0)
            {
                selectedTemplate = null;
                LoadDefaultWorkoutMessage();
                return;
            }

            if (templates == null || templates.Count == 0) return;

            selectedTemplate = templates[e.Position - 1]; // offset by 1 because of placeholder
            await LoadWorkoutExercises(selectedTemplate);
        }
        private void BtnStartFromTemplate_Click(object? sender, EventArgs e)
        {
            if (spnTemplates == null) return;

            if (spnTemplates.SelectedItemPosition == 0)
            {
                // Placeholder selected — start empty workout
                var intent = new Intent(this, typeof(WorkoutSessionActivity));
                intent.PutExtra("ExerciseSets", string.Empty); // No data passed
                StartActivity(intent);
                return;
            }

            if (templates == null || templates.Count == 0)
            {
                Toast.MakeText(this, "No templates available!", ToastLength.Short).Show();
                return;
            }

            selectedTemplate = templates[spnTemplates.SelectedItemPosition - 1]; // offset by 1
            string cleanedData = CleanExerciseSetString(selectedTemplate.ExerciseSets);

            var workoutIntent = new Intent(this, typeof(WorkoutSessionActivity));
            workoutIntent.PutExtra("ExerciseSets", cleanedData);
            StartActivity(workoutIntent);
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
