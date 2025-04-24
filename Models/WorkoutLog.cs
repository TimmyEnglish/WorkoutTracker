using SQLite;
using System;

namespace WorkoutTracker.Models
{
    public class WorkoutLog
    {
        [PrimaryKey, AutoIncrement]
        public int Id { get; set; }
        public int ExerciseId { get; set; }
        public string ExerciseName { get; set; }
        public int Reps { get; set; }
        public double Weight { get; set; }
        public DateTime Date { get; set; }
    }
}
