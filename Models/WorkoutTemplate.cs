using SQLite;

public class WorkoutTemplate
{
    [PrimaryKey, AutoIncrement]
    public int Id { get; set; }

    public string Name { get; set; } = string.Empty;

    public string ExerciseSets { get; set; } = string.Empty; // Format: "1:3,2:3,3:3" (ExerciseId:SetCount)
}
