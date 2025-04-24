using AndroidX.AppCompat.App;
using Android.Content;
using WorkoutTracker.Data;
using WorkoutTracker.Models;

namespace WorkoutTracker.Views
{
    [Activity(Label = "Create Template")]
    public class CreateTemplateActivity : AppCompatActivity
    {
        private const int REQUEST_CODE_SELECT_EXERCISE = 1001;

        private EditText? txtTemplateName;
        private Button? btnAddExercise, btnSaveTemplate;
        private ListView? lvExercises;

        private Dictionary<int, int> exerciseSetData = new Dictionary<int, int>(); // ExerciseId -> Sets
        private List<Exercise> exercises = new();
        private ArrayAdapter<string>? adapter;

        private List<string> selectedExerciseNames = new List<string>(); //ExerciseName - Sets

        private DatabaseHelper db;

        protected override async void OnCreate(Bundle? savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            SetContentView(Resource.Layout.activity_create_template);
            DatabaseHelper db = new DatabaseHelper();

            txtTemplateName = FindViewById<EditText>(Resource.Id.txtTemplateName);
            btnAddExercise = FindViewById<Button>(Resource.Id.btnAddExercise);
            btnSaveTemplate = FindViewById<Button>(Resource.Id.btnSaveTemplate);
            lvExercises = FindViewById<ListView>(Resource.Id.lvExercises);

            adapter = new ArrayAdapter<string>(this, Android.Resource.Layout.SimpleListItem1, new List<string>());
            lvExercises.Adapter = adapter;

            btnSaveTemplate.Click += async (s, e) =>
            {
                if (string.IsNullOrWhiteSpace(txtTemplateName.Text) || !exerciseSetData.Any())
                {
                    Toast.MakeText(this, "Please enter a template name and add at least one exercise.", ToastLength.Short).Show();
                    return;
                }

                // "1:3,2:3" (ExerciseId:SetCount)
                string formattedExerciseSets = string.Join(",", exerciseSetData.Select(ex => $"{ex.Key}:{ex.Value}"));

                var template = new WorkoutTemplate
                {
                    Name = txtTemplateName.Text.Trim(),
                    ExerciseSets = formattedExerciseSets
                };

                await DatabaseHelper.Instance.AddWorkoutTemplateAsync(template);
                Toast.MakeText(this, "Template saved!", ToastLength.Short).Show();
                Finish();
            };

            btnAddExercise.Click += (s, e) =>
            {
                var intent = new Intent(this, typeof(SelectExerciseActivity));
                StartActivityForResult(intent, REQUEST_CODE_SELECT_EXERCISE);
            };
        }
        protected override void OnActivityResult(int requestCode, Result resultCode, Intent data)
        {
            base.OnActivityResult(requestCode, resultCode, data);

            if (requestCode == REQUEST_CODE_SELECT_EXERCISE && resultCode == Result.Ok && data != null)
            {
                int exerciseId = data.GetIntExtra("ExerciseId", -1);
                string exerciseName = data.GetStringExtra("ExerciseName");
                int sets = data.GetIntExtra("Sets", 3); // Default to 3

                if (exerciseId != -1 && !string.IsNullOrEmpty(exerciseName))
                {
                    AddExerciseToList(exerciseId, exerciseName, sets);
                }
            }
        }
        private void AddExerciseToList(int exerciseId, string exerciseName, int sets)
        {
            if (!exerciseSetData.ContainsKey(exerciseId))
            {
                exerciseSetData[exerciseId] = sets;

                string formattedEntry = $"{exerciseName} - Sets: {sets}";
                selectedExerciseNames.Add(formattedEntry);

                UpdateExerciseList();
            }
        }

        private void UpdateExerciseList()
        {
            adapter?.Clear();
            adapter?.AddAll(selectedExerciseNames);
            adapter?.NotifyDataSetChanged();
        }
    }
}

