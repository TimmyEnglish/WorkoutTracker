using Android.App;
using Android.OS;
using Android.Widget;
using AndroidX.AppCompat.App;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using WorkoutTracker.Data;
using WorkoutTracker.Models;

namespace WorkoutTracker.Views
{
    [Activity(Label = "Create Template")]
    public class CreateTemplateActivity : AppCompatActivity
    {
        private EditText? txtTemplateName;
        private Button? btnSaveTemplate;
        private ListView? lvExercises;
        private List<Exercise> exercises = new();
        private ArrayAdapter<string>? adapter;
        private List<Exercise> exercisesList = new List<Exercise>();
        private List<Exercise> selectedExercises = new List<Exercise>();
        private DatabaseHelper db;
        protected override async void OnCreate(Bundle? savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            SetContentView(Resource.Layout.activity_create_template);
            DatabaseHelper db = new DatabaseHelper();

            txtTemplateName = FindViewById<EditText>(Resource.Id.txtTemplateName);
            btnSaveTemplate = FindViewById<Button>(Resource.Id.btnSaveTemplate);
            lvExercises = FindViewById<ListView>(Resource.Id.lvExercises);

            if (btnSaveTemplate != null)
                btnSaveTemplate.Click += BtnSaveTemplate_Click;

            await LoadExercises();
        }
        private async Task LoadExercises()
        {
            try
            {
                Console.WriteLine("LoadExercises() called...");

                var db = new DatabaseHelper();
                var exercises = await db.GetExercisesAsync();
                Console.WriteLine($"Exercises fetched: {exercises.Count}");

                if (exercises == null || exercises.Count == 0)
                {
                    Console.WriteLine("No exercises found!");
                }

                exercisesList = exercises ?? new List<Exercise>(); // Ensure it's never null

                adapter = new ArrayAdapter<string>(this, Android.Resource.Layout.SimpleListItem1, exercisesList.Select(e => e.Name).ToList());
                lvExercises.Adapter = adapter;


                Console.WriteLine("Exercises loaded into ListView!");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"ERROR in LoadExercises: {ex.Message}");
                Console.WriteLine($"STACK TRACE: {ex.StackTrace}");
            }
        }
        private async void BtnSaveTemplate_Click(object? sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtTemplateName.Text))
            {
                Toast.MakeText(this, "Please enter a template name.", ToastLength.Short).Show();
                return;
            }

            if (selectedExercises.Count == 0)
            {
                Toast.MakeText(this, "Please select at least one exercise.", ToastLength.Short).Show();
                return;
            }

            // Format exercises with set counts (assuming each has a default of 3 sets)
            var selectedExerciseSets = selectedExercises
                .Select(exercise => $"{exercise.Id}:3") // Assigning 3 sets for now
                .ToList();

            var template = new WorkoutTemplate
            {
                Name = txtTemplateName.Text,
                ExerciseSets = string.Join(",", selectedExerciseSets) // Format: "1:3,2:3,3:3"
            };

            await db.AddWorkoutTemplateAsync(template);

            Toast.MakeText(this, "Template saved successfully!", ToastLength.Short).Show();
            Finish();
        }
    }
}
