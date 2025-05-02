using Android.Views;
using AndroidX.RecyclerView.Widget;
using WorkoutTracker.Models;

namespace WorkoutTracker.Views
{
    public class WorkoutSessionAdapter : RecyclerView.Adapter
    {
        private readonly List<WorkoutExerciseEntry> exercises;

        public WorkoutSessionAdapter(List<WorkoutExerciseEntry> exercises)
        {
            this.exercises = exercises;
        }

        public override int ItemCount => exercises.Count;

        public override RecyclerView.ViewHolder OnCreateViewHolder(ViewGroup parent, int viewType)
        {
            var view = LayoutInflater.From(parent.Context).Inflate(Resource.Layout.item_freestyle_exercise, parent, false);
            return new ExerciseViewHolder(view);
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

            public ExerciseViewHolder(View itemView) : base(itemView)
            {
                txtExerciseName = itemView.FindViewById<TextView>(Resource.Id.txtExerciseName);
                layoutSets = itemView.FindViewById<LinearLayout>(Resource.Id.layoutSets);
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

                    // Clear any existing event handlers
                    edtWeight.TextChanged += (s, e) =>
                    {
                        if (double.TryParse(edtWeight.Text, out double weight))
                            set.Weight = weight;
                        else
                            set.Weight = null;
                    };

                    edtReps.TextChanged += (s, e) =>
                    {
                        if (int.TryParse(edtReps.Text, out int reps))
                            set.Reps = reps;
                        else
                            set.Reps = null;
                    };

                    layoutSets.AddView(setView);
                }
            }

        }
    }
}
