using AndroidX.AppCompat.App;
using WorkoutTracker.Data;
using WorkoutTracker.Models;
using OxyPlot;
using OxyPlot.Series;
using OxyPlot.Xamarin.Android;
using OxyPlot.Axes;
using Android.Content.PM;
using Android.Content.Res;

namespace WorkoutTracker.Views
{
    [Activity(
    Label = "View Stats",
    ScreenOrientation = ScreenOrientation.Portrait,
    ConfigurationChanges = ConfigChanges.Orientation | ConfigChanges.ScreenSize | ConfigChanges.KeyboardHidden)]

    public class ViewStatsActivity : AppCompatActivity
    {
        private Spinner spnExercises;
        private PlotView plotView;
        private ListView lvLogs;
        private List<Exercise> exercises;
        private List<WorkoutLog> currentLogs;
        private DatabaseHelper db;
        protected override async void OnCreate(Bundle? savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            SetContentView(Resource.Layout.activity_view_stats);
            RequestedOrientation = ScreenOrientation.Portrait;

            bool isDarkTheme = (Resources.Configuration.UiMode & UiMode.NightMask) == UiMode.NightYes;
            if (isDarkTheme && SupportActionBar != null)
            {
                var color = Android.Graphics.Color.ParseColor("#222222");
                SupportActionBar.SetBackgroundDrawable(new Android.Graphics.Drawables.ColorDrawable(color));
            }

            db = new DatabaseHelper();
            spnExercises = FindViewById<Spinner>(Resource.Id.spnExercises);
            plotView = FindViewById<PlotView>(Resource.Id.plotView);
            lvLogs = FindViewById<ListView>(Resource.Id.lvLogs);

            spnExercises.ItemSelected += SpnExercises_ItemSelected;
            lvLogs.ItemLongClick += LvLogs_ItemLongClick;

            await LoadExercisesAsync();
        }
        private async Task LoadExercisesAsync()
        {
            exercises = await db.GetExercisesAsync();
            var exerciseNames = exercises.Select(e => e.Name).ToList();

            var adapter = new ArrayAdapter<string>(this, Android.Resource.Layout.SimpleSpinnerDropDownItem, exerciseNames);
            spnExercises.Adapter = adapter;
        }
        private async void SpnExercises_ItemSelected(object? sender, AdapterView.ItemSelectedEventArgs e)
        {
            int selectedExerciseId = exercises[e.Position].Id;
            currentLogs = await db.GetWorkoutLogsByExerciseAsync(selectedExerciseId);

            UpdatePlot(currentLogs);
            UpdateList(currentLogs);
        }
        private void UpdatePlot(List<WorkoutLog> logs)
        {
            bool isDarkTheme = (Resources.Configuration.UiMode & UiMode.NightMask) == UiMode.NightYes;

            if (logs.Count == 0)
            {
                plotView.Model = new PlotModel
                {
                    Title = "No data entries yet",
                    TextColor = isDarkTheme ? OxyColors.LightGray : OxyColors.Black,
                    PlotAreaBorderColor = isDarkTheme ? OxyColors.DarkGray : OxyColors.LightGray,
                    Background = isDarkTheme ? OxyColor.FromRgb(10, 10, 10) : OxyColors.White
                };
                return;
            }

            var model = new PlotModel
            {
                Title = "Weight and Reps Over Time",
                TextColor = isDarkTheme ? OxyColors.LightGray : OxyColors.Black,
                PlotAreaBorderColor = isDarkTheme ? OxyColors.DarkGray : OxyColors.LightGray,
                Background = isDarkTheme ? OxyColor.FromRgb(10, 10, 10) : OxyColors.White,
                TitleColor = isDarkTheme ? OxyColors.LightGray : OxyColors.Black
            };

            var weightSeries = new LineSeries
            {
                Title = "Weight (kg)",
                MarkerType = MarkerType.Circle,
                YAxisKey = "WeightAxis",
                Color = OxyColor.Parse(isDarkTheme ? "#FF5555" : "#D32F2F"),
                MarkerFill = OxyColor.Parse(isDarkTheme ? "#FF9999" : "#FFCDD2"),
                MarkerSize = 4
            };

            var repsSeries = new LineSeries
            {
                Title = "Reps",
                MarkerType = MarkerType.Square,
                YAxisKey = "RepsAxis",
                Color = OxyColor.Parse(isDarkTheme ? "#AAAAFF" : "#303F9F"),
                MarkerFill = OxyColor.Parse(isDarkTheme ? "#CCCCFF" : "#C5CAE9"),
                MarkerSize = 4
            };

            foreach (var log in logs.OrderBy(l => l.Date))
            {
                var time = DateTimeAxis.ToDouble(log.Date);
                weightSeries.Points.Add(new DataPoint(time, log.Weight));
                repsSeries.Points.Add(new DataPoint(time, log.Reps));
            }

            // Padding
            double maxWeight = logs.Max(l => l.Weight);
            double maxReps = logs.Max(l => l.Reps);
            double weightMaxWithPadding = maxWeight + Math.Max(1, maxWeight * 0.4);
            double repsMaxWithPadding = maxReps + Math.Max(1, maxReps * 0.4);

            model.Axes.Add(new DateTimeAxis
            {
                Position = AxisPosition.Bottom,
                StringFormat = "MM/dd",
                Title = "Date",
                IntervalType = DateTimeIntervalType.Days,
                MajorGridlineStyle = LineStyle.Solid,
                MinorGridlineStyle = LineStyle.Dot,
                IsZoomEnabled = true,
                IsPanEnabled = true,
                MinimumPadding = 0.20,  
                MaximumPadding = 0.20, 
                TextColor = isDarkTheme ? OxyColors.LightGray : OxyColors.Black,
                TitleColor = isDarkTheme ? OxyColors.LightGray : OxyColors.Black,
                MajorGridlineColor = OxyColor.Parse(isDarkTheme ? "#333333" : "#CCCCCC"),
                MinorGridlineColor = OxyColor.Parse(isDarkTheme ? "#222222" : "#EEEEEE")
            });

            model.Axes.Add(new LinearAxis
            {
                Position = AxisPosition.Left,
                Title = "Weight (kg)",
                Key = "WeightAxis",
                Minimum = 0,
                AbsoluteMinimum = 0,
                Maximum = weightMaxWithPadding,
                TextColor = isDarkTheme ? OxyColors.LightGray : OxyColors.Black,
                TitleColor = isDarkTheme ? OxyColors.LightGray : OxyColors.Black,
                MajorGridlineStyle = LineStyle.Solid,
                MinorGridlineStyle = LineStyle.Dot,
                MajorGridlineColor = OxyColor.Parse(isDarkTheme ? "#333333" : "#CCCCCC"),
                MinorGridlineColor = OxyColor.Parse(isDarkTheme ? "#222222" : "#EEEEEE")
            });

            model.Axes.Add(new LinearAxis
            {
                Position = AxisPosition.Right,
                Title = "Reps",
                Key = "RepsAxis",
                Minimum = 0,
                AbsoluteMinimum = 0,
                Maximum = repsMaxWithPadding,
                TextColor = isDarkTheme ? OxyColors.LightGray : OxyColors.Black,
                TitleColor = isDarkTheme ? OxyColors.LightGray : OxyColors.Black,
                MajorGridlineStyle = LineStyle.Solid,
                MinorGridlineStyle = LineStyle.Dot,
                MajorGridlineColor = OxyColor.Parse(isDarkTheme ? "#333333" : "#CCCCCC"),
                MinorGridlineColor = OxyColor.Parse(isDarkTheme ? "#222222" : "#EEEEEE")
            });

            model.Series.Add(weightSeries);
            model.Series.Add(repsSeries);

            plotView.Model = model;
        }
        private void UpdateList(List<WorkoutLog> logs)
        {
            var items = logs
                .OrderByDescending(log => log.Date)
                .Select(log =>
                    $"Date: {log.Date:yyyy-MM-dd}\nWeight: {log.Weight} kg, Reps: {log.Reps}")
                .ToList();

            var adapter = new ArrayAdapter<string>(this, Android.Resource.Layout.SimpleListItem1, items);
            lvLogs.Adapter = adapter;
        }
        private async void LvLogs_ItemLongClick(object? sender, AdapterView.ItemLongClickEventArgs e)
        {
            var logToDelete = currentLogs[e.Position];

            var confirm = new AndroidX.AppCompat.App.AlertDialog.Builder(this, Resource.Style.MyAlertDialogTheme)
                .SetTitle("Delete Entry")
                .SetMessage("Are you sure you want to delete this workout entry?")
                .SetPositiveButton("Delete", async (_, _) =>
                {
                    await db.DeleteWorkoutLogAsync(logToDelete.Id);
                    currentLogs.RemoveAt(e.Position);
                    UpdateList(currentLogs);
                    UpdatePlot(currentLogs);
                })
                .SetNegativeButton("Cancel", (s, args) => { })
                .Create();

            confirm.Show();
        }
    }
}
