using AndroidX.AppCompat.App;
using Android.Content;
using WorkoutTracker.Data;
using WorkoutTracker.Models;

namespace WorkoutTracker.Views
{
    [Activity(Label = "Select Exercise")]
    public class SelectExerciseActivity : AppCompatActivity
    {
        private Button btnAddNewExercise;
        private SearchView searchView;
        private ListView lvExerciseList;
        private List<Exercise> exercises;
        private ArrayAdapter<string> adapter;

        protected override async void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            SetContentView(Resource.Layout.activity_select_exercise);

            searchView = FindViewById<SearchView>(Resource.Id.searchViewExercises);
            lvExerciseList = FindViewById<ListView>(Resource.Id.lvExerciseList);
            btnAddNewExercise = FindViewById<Button>(Resource.Id.btnAddNewExercise);

            await LoadExercises();

            searchView.QueryTextChange += (s, e) =>
            {
                adapter.Filter.InvokeFilter(e.NewText);
            };

            lvExerciseList.ItemClick += (s, e) =>
            {
                var selectedExercise = exercises[e.Position];

                Android.App.AlertDialog.Builder builder = new Android.App.AlertDialog.Builder(this);
                builder.SetTitle($"Select Sets for {selectedExercise.Name}");

                EditText input = new EditText(this)
                {
                    InputType = Android.Text.InputTypes.ClassNumber
                };
                input.Text = "3"; // Default value

                builder.SetView(input);

                builder.SetPositiveButton("OK", (dialog, args) =>
                {
                    int sets;
                    if (int.TryParse(input.Text, out sets) && sets > 0)
                    {
                        var resultIntent = new Intent();
                        resultIntent.PutExtra("ExerciseId", selectedExercise.Id);
                        resultIntent.PutExtra("ExerciseName", selectedExercise.Name);
                        resultIntent.PutExtra("Sets", sets);
                        SetResult(Result.Ok, resultIntent);

                        Finish();
                    }
                });

                builder.SetNegativeButton("Cancel", (dialog, args) => {});

                Android.App.AlertDialog dialog = builder.Create();
                dialog.Show();
            };

            lvExerciseList.ItemLongClick += async (s, e) =>
            {
                var selectedExercise = exercises[e.Position];

                Android.App.AlertDialog.Builder builder = new Android.App.AlertDialog.Builder(this);
                builder.SetTitle("Delete Exercise");
                builder.SetMessage($"Are you sure you want to delete \"{selectedExercise.Name}\"?");
                builder.SetPositiveButton("Delete", async (dialog, args) =>
                {
                    await DatabaseHelper.Instance.DeleteExerciseAsync(selectedExercise);

                    await LoadExercises(); // Refresh the list
                    Toast.MakeText(this, "Exercise deleted.", ToastLength.Short).Show();
                });
                builder.SetNegativeButton("Cancel", (dialog, args) => { });
                builder.Show();
            };


            btnAddNewExercise.Click += (s, e) =>
            {
                Android.App.AlertDialog.Builder builder = new Android.App.AlertDialog.Builder(this);
                builder.SetTitle("New Exercise Name");

                EditText input = new EditText(this)
                {
                    InputType = Android.Text.InputTypes.ClassText
                };

                builder.SetView(input);

                builder.SetPositiveButton("Add", async (dialog, args) =>
                {
                    string exerciseName = input.Text.Trim();
                    if (!string.IsNullOrEmpty(exerciseName))
                    {
                        bool exists = exercises.Any(ex => ex.Name.Equals(exerciseName, StringComparison.OrdinalIgnoreCase));
                        if (exists)
                        {
                            Toast.MakeText(this, "Exercise already exists!", ToastLength.Short).Show();
                        }
                        else
                        {
                            var newExercise = new Exercise { Name = exerciseName };
                            await DatabaseHelper.Instance.AddExerciseAsync(newExercise);

                            await LoadExercises();
                        }
                    }
                });

                builder.SetNegativeButton("Cancel", (dialog, args) => { });

                Android.App.AlertDialog dialog = builder.Create();
                dialog.Show();
            };

        }
        private async Task LoadExercises()
        {
            exercises = await DatabaseHelper.Instance.GetExercisesAsync();
            adapter = new ArrayAdapter<string>(this, Android.Resource.Layout.SimpleListItem1, exercises.ConvertAll(e => e.Name));
            lvExerciseList.Adapter = adapter;
        }
    }
}
