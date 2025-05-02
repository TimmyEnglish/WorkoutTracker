using Android.Content;
using Android.Views;
using AndroidX.AppCompat.App;
using AndroidX.RecyclerView.Widget;
using WorkoutTracker.Data;
using WorkoutTracker.Models;

namespace WorkoutTracker.Views
{
    [Activity(Label = "Workout Session")]
    public class WorkoutSessionActivity : AppCompatActivity
    {
        private TextView txtNoExercises;
        private Button btnAddExercise;
        private Button btnFinishWorkout;
        private RecyclerView recyclerView;
        private WorkoutSessionAdapter adapter;
        private List<WorkoutExerciseEntry> exerciseEntries = new();

        protected override async void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            SetContentView(Resource.Layout.activity_workout_session);

            recyclerView = FindViewById<RecyclerView>(Resource.Id.recyclerFreestyleWorkout);
            btnAddExercise = FindViewById<Button>(Resource.Id.btnAddExercise);
            btnFinishWorkout = FindViewById<Button>(Resource.Id.btnFinishWorkout);
            txtNoExercises = FindViewById<TextView>(Resource.Id.txtNoExercises);

            string exerciseData = Intent.GetStringExtra("ExerciseSets") ?? string.Empty;
            exerciseEntries = await LoadExercisesAsync(exerciseData);

            adapter = new WorkoutSessionAdapter(exerciseEntries);
            recyclerView.SetLayoutManager(new LinearLayoutManager(this));
            recyclerView.SetAdapter(adapter);

            UpdateEmptyState();

            btnAddExercise.Click += (s, e) =>
            {
                var intent = new Intent(this, typeof(SelectExerciseActivity));
                StartActivityForResult(intent, 1000);
            };

            btnFinishWorkout.Click += async (s, e) => await SaveWorkoutAsync();
        }
        protected override async void OnActivityResult(int requestCode, Result resultCode, Intent data)
        {
            base.OnActivityResult(requestCode, resultCode, data);

            if (requestCode == 1000 && resultCode == Result.Ok)
            {
                int exerciseId = data.GetIntExtra("ExerciseId", -1);
                string exerciseName = data.GetStringExtra("ExerciseName") ?? string.Empty;
                int sets = data.GetIntExtra("Sets", 3);

                if (exerciseId != -1 && !string.IsNullOrEmpty(exerciseName))
                {
                    var existingEntry = exerciseEntries.FirstOrDefault(e => e.ExerciseId == exerciseId);

                    if (existingEntry != null)
                    {
                        var extraSets = Enumerable.Range(1, sets).Select(_ => new WorkoutSet()).ToList();
                        existingEntry.Sets.AddRange(extraSets);
                    }
                    else
                    {
                        var newEntry = new WorkoutExerciseEntry
                        {
                            ExerciseId = exerciseId,
                            ExerciseName = exerciseName,
                            Sets = Enumerable.Range(1, sets).Select(_ => new WorkoutSet()).ToList()
                        };
                        exerciseEntries.Add(newEntry);
                    }

                    adapter.NotifyDataSetChanged();
                    UpdateEmptyState();
                }
            }
        }
        private void UpdateEmptyState()
        {
            if (exerciseEntries.Count == 0)
            {
                txtNoExercises.Visibility = ViewStates.Visible;
            }
            else
            {
                txtNoExercises.Visibility = ViewStates.Gone;
            }
        }
        private async Task<List<WorkoutExerciseEntry>> LoadExercisesAsync(string exerciseData)
        {
            var list = new List<WorkoutExerciseEntry>();
            if (string.IsNullOrWhiteSpace(exerciseData))
                return list;

            var entries = exerciseData.Split(',');
            foreach (var entry in entries)
            {
                var parts = entry.Split(':');
                if (parts.Length >= 2 &&
                    int.TryParse(parts[0], out int id) &&
                    int.TryParse(parts[1], out int sets))
                {
                    var exercise = await DatabaseHelper.Instance.GetExerciseByIdAsync(id);
                    if (exercise != null)
                    {
                        list.Add(new WorkoutExerciseEntry
                        {
                            ExerciseId = id,
                            ExerciseName = exercise.Name,
                            Sets = Enumerable.Range(1, sets).Select(_ => new WorkoutSet()).ToList()
                        });
                    }
                }
            }

            return list;
        }
        private async Task SaveWorkoutAsync()
        {
            foreach (var entry in exerciseEntries)
            {
                foreach (var set in entry.Sets)
                {
                    if (set.Weight > 0 && set.Reps > 0)
                    {
                        var log = new WorkoutLog
                        {
                            ExerciseId = entry.ExerciseId,
                            ExerciseName = entry.ExerciseName,
                            Weight = set.Weight ?? 0.0,
                            Reps = set.Reps ?? 0,
                            Date = DateTime.Now
                        };
                        await DatabaseHelper.Instance.AddWorkoutLogAsync(log);
                    }
                }
            }

            Toast.MakeText(this, "Workout saved!", ToastLength.Long).Show();
            Finish();
        }
    }
}
