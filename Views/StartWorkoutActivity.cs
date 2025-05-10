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

                var templateNames = templates.Select(t => t.Name).ToList();
                adapter = new ArrayAdapter<string>(this, Android.Resource.Layout.SimpleSpinnerDropDownItem, templateNames);
                spnTemplates.Adapter = adapter;

                // Select the first template by default
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
        private async void BtnStartFromTemplate_Click(object? sender, EventArgs e)
        {
            if (templates == null || templates.Count == 0)
            {
                Toast.MakeText(this, "No templates available!", ToastLength.Short).Show();
                return;
            }

            // Get the selected template and load its exercises
            selectedTemplate = templates[spnTemplates.SelectedItemPosition];
            await LoadWorkoutExercises(selectedTemplate);

            string cleanedData = CleanExerciseSetString(selectedTemplate.ExerciseSets);

            Intent intent;
            intent = new Intent(this, typeof(WorkoutSessionActivity));

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
