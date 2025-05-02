namespace WorkoutTracker.Models
{
    public class WorkoutExerciseEntry
    {
        public int ExerciseId { get; set; }
        public string ExerciseName { get; set; }
        public List<WorkoutSet> Sets { get; set; } = new();
    }

    public class WorkoutSet
    {
        public double? Weight { get; set; }
        public int? Reps { get; set; }
    }
}
