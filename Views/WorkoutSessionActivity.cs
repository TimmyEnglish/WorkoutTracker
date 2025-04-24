using Android.Content;
using AndroidX.AppCompat.App;
using WorkoutTracker.Data;
using WorkoutTracker.Models;

namespace WorkoutTracker.Views
{
    [Activity(Label = "Workout Session")]
    public class WorkoutSessionActivity : AppCompatActivity
    {
        private TextView txtExerciseName, txtSetNumber;
        private EditText edtWeight, edtReps;
        private Button btnNextSet, btnAddExercise;
        private List<(int ExerciseId, string ExerciseName, int Sets)> workoutExercises = new();
        private int currentExerciseIndex = 0, currentSetNumber = 1;

        protected override async void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            SetContentView(Resource.Layout.activity_workout_session);

            txtExerciseName = FindViewById<TextView>(Resource.Id.txtExerciseName);
            txtSetNumber = FindViewById<TextView>(Resource.Id.txtSetNumber);
            edtWeight = FindViewById<EditText>(Resource.Id.edtWeight);
            edtReps = FindViewById<EditText>(Resource.Id.edtReps);
            btnNextSet = FindViewById<Button>(Resource.Id.btnNextSet);
            btnAddExercise = FindViewById<Button>(Resource.Id.btnAddExercise);

            // Load exercises from intent
            string exerciseData = Intent.GetStringExtra("ExerciseSets");
            workoutExercises = await ParseExerciseData(exerciseData);

            LoadNextSet();

            btnNextSet.Click += async (s, e) => await SaveCurrentSet();
            btnAddExercise.Click += (s, e) => AddExercise();
        }

        private void AddExercise()
        {
            Intent intent = new Intent(this, typeof(SelectExerciseActivity));
            StartActivityForResult(intent, 100); // Request code 100
        }

        protected override void OnActivityResult(int requestCode, Result resultCode, Intent data)
        {
            base.OnActivityResult(requestCode, resultCode, data);

            if (requestCode == 100 && resultCode == Result.Ok)
            {
                int exerciseId = data.GetIntExtra("ExerciseId", -1);
                string exerciseName = data.GetStringExtra("ExerciseName");
                int sets = data.GetIntExtra("Sets", 3);

                workoutExercises.Add((exerciseId, exerciseName, sets));
                LoadNextSet();
            }
        }

        private async Task SaveCurrentSet()
        {
            if (!double.TryParse(edtWeight.Text, out double weight) || !int.TryParse(edtReps.Text, out int reps))
            {
                Toast.MakeText(this, "Please enter valid weight and reps.", ToastLength.Short).Show();
                return;
            }

            var (exerciseId, _, totalSets) = workoutExercises[currentExerciseIndex];
            var exerciseName = workoutExercises[currentExerciseIndex].ExerciseName;

            var logEntry = new WorkoutLog
            {
                ExerciseId = exerciseId,
                ExerciseName = exerciseName,
                Reps = reps,
                Weight = weight,
                Date = DateTime.Now
            };

            await DatabaseHelper.Instance.AddWorkoutLogAsync(logEntry);

            if (currentSetNumber < totalSets)
            {
                currentSetNumber++;
            }
            else
            {
                currentSetNumber = 1;
                currentExerciseIndex++;
            }

            LoadNextSet();
        }

        private void LoadNextSet()
        {
            if (workoutExercises.Count == 0)
            {
                txtExerciseName.Text = "Please add exercises to start your workout.";
                txtSetNumber.Text = "";
                edtReps.Visibility = Android.Views.ViewStates.Gone;
                edtWeight.Visibility = Android.Views.ViewStates.Gone;
                btnNextSet.Visibility = Android.Views.ViewStates.Gone;
                return;
            }
            if (currentExerciseIndex >= workoutExercises.Count)
            {
                Toast.MakeText(this, "Workout complete!", ToastLength.Long).Show();
                Finish();
                return;
            }

            var (_, exerciseName, totalSets) = workoutExercises[currentExerciseIndex];

            txtExerciseName.Text = exerciseName;
            txtSetNumber.Text = $"Set {currentSetNumber} of {totalSets}";
            edtReps.Visibility = Android.Views.ViewStates.Visible;
            edtWeight.Visibility = Android.Views.ViewStates.Visible;
            btnNextSet.Visibility = Android.Views.ViewStates.Visible;
            edtWeight.Text = "";
            edtReps.Text = "";

            bool isLastSetOfLastExercise =
                currentExerciseIndex == workoutExercises.Count - 1 && currentSetNumber == totalSets;

            btnNextSet.Text = isLastSetOfLastExercise ? "Finish Workout" : "Next Set";
        }

        private async Task<List<(int ExerciseId, string ExerciseName, int Sets)>> ParseExerciseData(string exerciseData)
        {
            var list = new List<(int, string, int)>();
            if (string.IsNullOrEmpty(exerciseData)) return list;

            var entries = exerciseData.Split(',');
            foreach (var entry in entries)
            {
                var parts = entry.Split(':');
                if (parts.Length >= 2 &&
                    int.TryParse(parts[0], out int exerciseId) &&
                    int.TryParse(parts[1], out int sets))
                {
                    var exercise = await DatabaseHelper.Instance.GetExerciseByIdAsync(exerciseId);
                    if (exercise != null)
                        list.Add((exercise.Id, exercise.Name, sets));
                }
            }
            return list;
        }
    }
}
