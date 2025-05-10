using Android.Views;
using AndroidX.RecyclerView.Widget;
using WorkoutTracker.Models;

namespace WorkoutTracker.Views
{
    public class WorkoutSessionAdapter : RecyclerView.Adapter
    {
        private readonly List<WorkoutExerciseEntry> exercises;
        private readonly Action SaveTemporaryState;
        public WorkoutSessionAdapter(List<WorkoutExerciseEntry> exercises, Action saveTemporaryState)
        {
            this.exercises = exercises;
            SaveTemporaryState = saveTemporaryState;
        }
        public override int ItemCount => exercises.Count;
        public override RecyclerView.ViewHolder OnCreateViewHolder(ViewGroup parent, int viewType)
        {
            var view = LayoutInflater.From(parent.Context).Inflate(Resource.Layout.item_freestyle_exercise, parent, false);
            return new ExerciseViewHolder(view, SaveTemporaryState);
        }
        public override void OnBindViewHolder(RecyclerView.ViewHolder holder, int position)
        {
            if (holder is ExerciseViewHolder vh)
            {
                vh.Bind(exercises[position]);
            }
        }
        private class ExerciseViewHolder : RecyclerView.ViewHolder
        {
            private readonly TextView txtExerciseName;
            private readonly LinearLayout layoutSets;
            private readonly Action SaveTemporaryState;

            public ExerciseViewHolder(View itemView, Action saveTemporaryState) : base(itemView)
            {
                txtExerciseName = itemView.FindViewById<TextView>(Resource.Id.txtExerciseName);
                layoutSets = itemView.FindViewById<LinearLayout>(Resource.Id.layoutSets);
                SaveTemporaryState = saveTemporaryState;
            }

            public void Bind(WorkoutExerciseEntry exercise)
            {
                txtExerciseName.Text = exercise.ExerciseName;
                layoutSets.RemoveAllViews();

                for (int i = 0; i < exercise.Sets.Count; i++)
                {
                    var setIndex = i;
                    var set = exercise.Sets[setIndex];

                    var setView = LayoutInflater.From(ItemView.Context).Inflate(Resource.Layout.item_set_input, layoutSets, false);
                    var edtWeight = setView.FindViewById<EditText>(Resource.Id.edtWeight);
                    var edtReps = setView.FindViewById<EditText>(Resource.Id.edtReps);
                    var txtSetNumber = setView.FindViewById<TextView>(Resource.Id.txtSetNumber);

                    txtSetNumber.Text = $"Set {setIndex + 1}";

                    // Restore saved values
                    edtWeight.Text = set.Weight?.ToString() ?? "";
                    edtReps.Text = set.Reps?.ToString() ?? "";

                    // Unsubscribe from previous events to avoid stacking event handlers
                    edtWeight.TextChanged -= OnWeightTextChanged;
                    edtReps.TextChanged -= OnRepsTextChanged;

                    // Attach new event handlers
                    edtWeight.TextChanged += OnWeightTextChanged;
                    edtReps.TextChanged += OnRepsTextChanged;

                    void OnWeightTextChanged(object sender, Android.Text.TextChangedEventArgs e)
                    {
                        if (double.TryParse(edtWeight.Text, out double weight))
                            set.Weight = weight;
                        else
                            set.Weight = null;

                        SaveTemporaryState.Invoke(); // Save state immediately on text change
                    }

                    void OnRepsTextChanged(object sender, Android.Text.TextChangedEventArgs e)
                    {
                        if (int.TryParse(edtReps.Text, out int reps))
                            set.Reps = reps;
                        else
                            set.Reps = null;

                        SaveTemporaryState.Invoke(); // Save state immediately on text change
                    }

                    layoutSets.AddView(setView);
                }
            }
        }
    }
}
