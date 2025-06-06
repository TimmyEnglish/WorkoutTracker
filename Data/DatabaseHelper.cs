﻿using Android.Util;
using SQLite;
using WorkoutTracker.Models;

namespace WorkoutTracker.Data
{
    public class DatabaseHelper
    {
        private static DatabaseHelper? _instance;
        private static readonly object _lock = new object();
        private static bool _isDatabaseReset = false;
        private readonly SQLiteAsyncConnection _database;
        string dbPath = Path.Combine(Android.App.Application.Context.FilesDir.AbsolutePath, "workout.db");

        public static DatabaseHelper Instance
        {
            get
            {
                lock (_lock)
                {
                    return _instance ??= new DatabaseHelper();
                }
            }
        }
        public DatabaseHelper()
        {
            if (!_isDatabaseReset)
            {
                //ResetDatabase(); // TEMPORARY
                _isDatabaseReset = true;
            }
            _database = new SQLiteAsyncConnection(dbPath, SQLiteOpenFlags.ReadWrite | SQLiteOpenFlags.Create);
            InitializeDatabase();
        }
        private void ResetDatabase()
        {
            try
            {
                if (File.Exists(dbPath))
                {
                    File.Delete(dbPath);
                    Log.Debug("DatabaseHelper", "Database deleted successfully.");
                }
            }
            catch (Exception ex)
            {
                Log.Error("DatabaseHelper", $"Error deleting database: {ex.Message}");
            }
        }
        private async void InitializeDatabase()
        {
            await _database.CreateTableAsync<Exercise>();
            await _database.CreateTableAsync<WorkoutTemplate>();
            await _database.CreateTableAsync<WorkoutLog>();

            Log.Debug("DatabaseHelper", "Database initialized.");

            if (await IsDatabaseEmpty())
            {
                await AddDefaultData();
            }

            try
            {
                string destPath = Path.Combine(Android.OS.Environment.ExternalStorageDirectory.AbsolutePath, "Download", "workout.db");
                File.Copy(dbPath, destPath, true);
            }
            catch (Exception ex)
            {
                Log.Error("DatabaseHelper", $"Error copying database: {ex.Message}");
            }
        }
        private async Task<bool> IsDatabaseEmpty()
        {
            var count = await _database.Table<WorkoutTemplate>().CountAsync();
            return count == 0;
        }
        private async Task AddDefaultData()
        {
            try
            {
                var exercises = new List<Exercise>
                {
                    new Exercise { Name = "Push-ups" },
                    new Exercise { Name = "Squats" },
                    new Exercise { Name = "Bench Press" },
                    new Exercise { Name = "Deadlift" }
                };

                await _database.InsertAllAsync(exercises);

                var template = new WorkoutTemplate
                {
                    Name = "Beginner Routine",
                    ExerciseSets = "1:3,2:3,3:3,4:3"
                };

                await _database.InsertAsync(template);
                Log.Debug("DatabaseHelper", "Default data added.");
            }
            catch (Exception ex)
            {
                Log.Error("DatabaseHelper", $"Error adding default data: {ex.Message}");
            }
        }

        // --- Exercise CRUD ---

        public async Task<int> AddExerciseAsync(Exercise exercise)
        {
            try
            {
                return await _database.InsertAsync(exercise);
            }
            catch (Exception ex)
            {
                Log.Error("DatabaseHelper", $"Error adding exercise: {ex.Message}");
                return -1;
            }
        }
        public async Task DeleteExerciseAsync(Exercise exercise)
        {
            try
            {
                await _database.DeleteAsync(exercise);
            }
            catch (Exception ex)
            {
                Log.Error("DatabaseHelper", $"Error deleting exercise: {ex.Message}");
            }
        }
        public async Task<List<Exercise>> GetExercisesAsync()
        {
            try
            {
                return await _database.Table<Exercise>().ToListAsync();
            }
            catch (Exception ex)
            {
                Log.Error("DatabaseHelper", $"Error fetching exercises: {ex.Message}");
                return new List<Exercise>();
            }
        }
        public async Task<Exercise?> GetExerciseByIdAsync(int id)
        {
            try
            {
                return await _database.Table<Exercise>().FirstOrDefaultAsync(e => e.Id == id);
            }
            catch (Exception ex)
            {
                Log.Error("DatabaseHelper", $"Error fetching exercise by ID: {ex.Message}");
                return null;
            }
        }

        // --- WorkoutTemplate CRUD ---

        public async Task<int> AddWorkoutTemplateAsync(WorkoutTemplate template)
        {
            try
            {
                return await _database.InsertAsync(template);
            }
            catch (Exception ex)
            {
                Log.Error("DatabaseHelper", $"Error adding workout template: {ex.Message}");
                return -1;
            }
        }
        public async Task<int> AddWorkoutTemplateAndReturnIdAsync(WorkoutTemplate template)
        {
            try
            {
                await _database.InsertAsync(template);
                return template.Id;
            }
            catch (Exception ex)
            {
                Log.Error("DatabaseHelper", $"Error adding workout template and returning ID: {ex.Message}");
                return -1;
            }
        }
        public async Task<List<WorkoutTemplate>> GetWorkoutTemplatesAsync()
        {
            try
            {
                return await _database.Table<WorkoutTemplate>().ToListAsync();
            }
            catch (Exception ex)
            {
                Log.Error("DatabaseHelper", $"Error fetching workout templates: {ex.Message}");
                return new List<WorkoutTemplate>();
            }
        }
        public async Task<WorkoutTemplate?> GetWorkoutTemplateByIdAsync(int id)
        {
            try
            {
                return await _database.Table<WorkoutTemplate>().FirstOrDefaultAsync(t => t.Id == id);
            }
            catch (Exception ex)
            {
                Log.Error("DatabaseHelper", $"Error fetching workout template by ID: {ex.Message}");
                return null;
            }
        }
        public async Task UpdateWorkoutTemplateAsync(WorkoutTemplate template)
        {
            try
            {
                await _database.UpdateAsync(template);
            }
            catch (Exception ex)
            {
                Log.Error("DatabaseHelper", $"Error updating workout template: {ex.Message}");
            }
        }
        public async Task DeleteWorkoutTemplateAsync(int id)
        {
            try
            {
                var template = await _database.Table<WorkoutTemplate>().Where(t => t.Id == id).FirstOrDefaultAsync();
                if (template != null)
                {
                    await _database.DeleteAsync(template);
                }
            }
            catch (Exception ex)
            {
                Log.Error("DatabaseHelper", $"Error deleting workout template: {ex.Message}");
            }
        }

        // --- WorkoutLog ---

        public async Task<int> AddWorkoutLogAsync(int exerciseId, int reps, double weight)
        {
            try
            {
                var log = new WorkoutLog
                {
                    ExerciseId = exerciseId,
                    Weight = weight,
                    Reps = reps,
                    Date = DateTime.Now
                };

                return await _database.InsertAsync(log);
            }
            catch (Exception ex)
            {
                Log.Error("DatabaseHelper", $"Error adding workout log: {ex.Message}");
                return -1;
            }
        }
        public async Task<List<WorkoutLog>> GetWorkoutLogsByExerciseAsync(int exerciseId)
        {
            try
            {
                return await _database.Table<WorkoutLog>().Where(l => l.ExerciseId == exerciseId).ToListAsync();
            }
            catch (Exception ex)
            {
                Log.Error("DatabaseHelper", $"Error fetching workout logs: {ex.Message}");
                return new List<WorkoutLog>();
            }
        }
        public async Task<List<WorkoutLog>> GetAllWorkoutLogsAsync()
        {
            return await _database.Table<WorkoutLog>().ToListAsync();
        }
        public async Task UpdateWorkoutLogAsync(WorkoutLog log)
        {
            await _database.UpdateAsync(log);
        }
        public async Task DeleteWorkoutLogAsync(int logId)
        {
            try
            {
                var log = await _database.Table<WorkoutLog>().Where(l => l.Id == logId).FirstOrDefaultAsync();
                if (log != null)
                {
                    await _database.DeleteAsync(log);
                    Log.Debug("DatabaseHelper", $"Deleted workout log with ID {logId}.");
                }
            }
            catch (Exception ex)
            {
                Log.Error("DatabaseHelper", $"Error deleting workout log: {ex.Message}");
            }
        }

        // --- Other ---

        public async Task CloseDatabase()
        {
            await _database.CloseAsync();
            Log.Debug("DatabaseHelper", "Database connection closed.");
        }
    }
}
