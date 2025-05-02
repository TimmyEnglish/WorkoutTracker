using AndroidX.AppCompat.App;
using Android.Content;
using WorkoutTracker.Data;
using WorkoutTracker.Models;
using Android.App;
using Android.Widget;
using System.Linq;

namespace WorkoutTracker.Views
{
    [Activity(Label = "Manage Templates")]
    public class CreateTemplateActivity : AppCompatActivity
    {
        private const int REQUEST_CODE_SELECT_EXERCISE = 1001;

        private Spinner? spnTemplates;
        private Button? btnNewTemplate, btnAddExercise, btnSaveChanges, btnDeleteTemplate;
        private ListView? lvExercises;

        private List<WorkoutTemplate> templates = new();
        private Dictionary<int, int> exerciseSetData = new(); // ExerciseId -> Sets
        private List<string> selectedExerciseNames = new(); // "Exercise Name - Sets"
        private ArrayAdapter<string>? exerciseAdapter;
        private ArrayAdapter<string>? templateAdapter;

        private int selectedTemplateId = -1;

        protected override async void OnCreate(Bundle? savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            SetContentView(Resource.Layout.activity_create_template);

            spnTemplates = FindViewById<Spinner>(Resource.Id.spnTemplates);
            btnNewTemplate = FindViewById<Button>(Resource.Id.btnNewTemplate);
            btnAddExercise = FindViewById<Button>(Resource.Id.btnAddExercise);
            btnSaveChanges = FindViewById<Button>(Resource.Id.btnSaveChanges);
            btnDeleteTemplate = FindViewById<Button>(Resource.Id.btnDeleteTemplate);
            lvExercises = FindViewById<ListView>(Resource.Id.lvExercises);

            exerciseAdapter = new ArrayAdapter<string>(this, Android.Resource.Layout.SimpleListItem1, new List<string>());
            lvExercises.Adapter = exerciseAdapter;

            btnNewTemplate.Click += (s, e) => ShowCreateTemplateDialog();
            btnAddExercise.Click += (s, e) => AddExercise();
            btnSaveChanges.Click += async (s, e) => await SaveTemplateChangesAsync();
            btnDeleteTemplate.Click += async (s, e) => await DeleteTemplateAsync();

            lvExercises.ItemLongClick += (s, e) => RemoveExerciseAt(e.Position);

            spnTemplates.ItemSelected += (s, e) =>
            {
                if (e.Position >= 0 && e.Position < templates.Count)
                {
                    selectedTemplateId = templates[e.Position].Id;
                    LoadExercisesForTemplateAsync(selectedTemplateId);
                }
            };

            await LoadTemplatesAsync();
        }

        private async Task LoadTemplatesAsync()
        {
            templates = await DatabaseHelper.Instance.GetWorkoutTemplatesAsync();
            templateAdapter = new ArrayAdapter<string>(this, Android.Resource.Layout.SimpleSpinnerItem, templates.Select(t => t.Name).ToList());
            templateAdapter.SetDropDownViewResource(Android.Resource.Layout.SimpleSpinnerDropDownItem);
            spnTemplates!.Adapter = templateAdapter;
        }

        private void ShowCreateTemplateDialog()
        {
            Android.App.AlertDialog.Builder dialog = new Android.App.AlertDialog.Builder(this);
            dialog.SetTitle("New Template");

            var input = new EditText(this)
            {
                Hint = "Enter template name"
            };
            dialog.SetView(input);

            dialog.SetPositiveButton("Create", async (sender, args) =>
            {
                string name = input.Text.Trim();
                if (!string.IsNullOrEmpty(name))
                {
                    var newTemplate = new WorkoutTemplate
                    {
                        Name = name,
                        ExerciseSets = "" // Empty for now
                    };
                    int newId = await DatabaseHelper.Instance.AddWorkoutTemplateAndReturnIdAsync(newTemplate);
                    await LoadTemplatesAsync();
                    // Select newly created template
                    int newPosition = templates.FindIndex(t => t.Id == newId);
                    if (newPosition >= 0)
                        spnTemplates!.SetSelection(newPosition);
                    Toast.MakeText(this, "Template created!", ToastLength.Short).Show();
                }
                else
                {
                    Toast.MakeText(this, "Name cannot be empty.", ToastLength.Short).Show();
                }
            });

            dialog.SetNegativeButton("Cancel", (sender, args) => { });

            dialog.Show();
        }

        private void AddExercise()
        {
            if (selectedTemplateId != -1)
            {
                var intent = new Intent(this, typeof(SelectExerciseActivity));
                StartActivityForResult(intent, REQUEST_CODE_SELECT_EXERCISE);
            }
            else
            {
                Toast.MakeText(this, "Please select a template first.", ToastLength.Short).Show();
            }
        }

        private async void LoadExercisesForTemplateAsync(int templateId)
        {
            var template = await DatabaseHelper.Instance.GetWorkoutTemplateByIdAsync(templateId);

            if (template != null)
            {
                exerciseSetData.Clear();
                selectedExerciseNames.Clear();

                if (!string.IsNullOrEmpty(template.ExerciseSets))
                {
                    var sets = template.ExerciseSets.Split(',');
                    foreach (var item in sets)
                    {
                        var parts = item.Split(':');
                        if (parts.Length == 2 &&
                            int.TryParse(parts[0], out int exerciseId) &&
                            int.TryParse(parts[1], out int setCount))
                        {
                            var exercise = await DatabaseHelper.Instance.GetExerciseByIdAsync(exerciseId);
                            if (exercise != null)
                            {
                                exerciseSetData[exerciseId] = setCount;
                                selectedExerciseNames.Add($"{exercise.Name} - Sets: {setCount}");
                            }
                        }
                    }
                }
                UpdateExerciseList();
            }
        }

        private void UpdateExerciseList()
        {
            exerciseAdapter!.Clear();
            exerciseAdapter!.AddAll(selectedExerciseNames);
            exerciseAdapter!.NotifyDataSetChanged();
        }

        private async Task SaveTemplateChangesAsync()
        {
            if (selectedTemplateId == -1)
            {
                Toast.MakeText(this, "No template selected.", ToastLength.Short).Show();
                return;
            }

            string formattedExerciseSets = string.Join(",", exerciseSetData.Select(ex => $"{ex.Key}:{ex.Value}"));

            var updatedTemplate = new WorkoutTemplate
            {
                Id = selectedTemplateId,
                Name = templates.First(t => t.Id == selectedTemplateId).Name,
                ExerciseSets = formattedExerciseSets
            };

            await DatabaseHelper.Instance.UpdateWorkoutTemplateAsync(updatedTemplate);
            Toast.MakeText(this, "Template updated.", ToastLength.Short).Show();
        }

        private async Task DeleteTemplateAsync()
        {
            if (selectedTemplateId != -1)
            {
                await DatabaseHelper.Instance.DeleteWorkoutTemplateAsync(selectedTemplateId);
                await LoadTemplatesAsync();
                selectedTemplateId = -1;
                exerciseSetData.Clear();
                selectedExerciseNames.Clear();
                UpdateExerciseList();
                Toast.MakeText(this, "Template deleted.", ToastLength.Short).Show();
            }
        }

        private void RemoveExerciseAt(int position)
        {
            if (position >= 0 && position < selectedExerciseNames.Count)
            {
                int exerciseId = exerciseSetData.Keys.ElementAt(position);
                exerciseSetData.Remove(exerciseId);
                selectedExerciseNames.RemoveAt(position);
                UpdateExerciseList();
            }
        }

        protected override void OnActivityResult(int requestCode, Result resultCode, Intent? data)
        {
            base.OnActivityResult(requestCode, resultCode, data);

            if (requestCode == REQUEST_CODE_SELECT_EXERCISE && resultCode == Result.Ok && data != null)
            {
                int exerciseId = data.GetIntExtra("ExerciseId", -1);
                string exerciseName = data.GetStringExtra("ExerciseName");
                int sets = data.GetIntExtra("Sets", 3);

                if (exerciseId != -1 && !string.IsNullOrEmpty(exerciseName))
                {
                    if (!exerciseSetData.ContainsKey(exerciseId))
                    {
                        exerciseSetData[exerciseId] = sets;
                        selectedExerciseNames.Add($"{exerciseName} - Sets: {sets}");
                        UpdateExerciseList();
                    }
                }
            }
        }
    }
}
