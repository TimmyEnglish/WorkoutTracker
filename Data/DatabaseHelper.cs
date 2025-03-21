using Android.App;
using Android.Util;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using SQLite;
using WorkoutTracker.Models; // Ensure this namespace matches your models

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
        public DatabaseHelper() // TEMPORARY
        {
            //Log.Debug("DatabaseHelper", $"Database Path: {dbPath}"); 
            if (!_isDatabaseReset)
            {
                ResetDatabase();
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
                    Android.Util.Log.Debug("DatabaseHelper", "Database deleted successfully.");
                }
            }
            catch (Exception ex)
            {
                Android.Util.Log.Error("DatabaseHelper", $"Error deleting database: {ex.Message}");
            }
        }
        private async void InitializeDatabase()
        {
            await _database.CreateTableAsync<Exercise>();
            await _database.CreateTableAsync<WorkoutTemplate>();
            await _database.CreateTableAsync<WorkoutLog>();

            Android.Util.Log.Debug("DatabaseHelper", "Database initialized.");

            if (await IsDatabaseEmpty()) // Ensure data is only added once
            {
                await AddDefaultData();
            }
            string destPath = Path.Combine(Android.OS.Environment.ExternalStorageDirectory.AbsolutePath, "Download", "workout.db");

            File.Copy(dbPath, destPath, true);
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

        public async Task<int> AddExerciseAsync(Exercise exercise)
        {
            try
            {
                return await _database.InsertAsync(exercise);
            }
            catch (Exception ex)
            {
                Log.Error("DatabaseHelper", $"Error adding exercise: {ex.Message}");
                return -1; // Return -1 if an error occurs
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
                return new List<Exercise>(); // Return an empty list if an error occurs
            }
        }
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
        public async Task<int> AddWorkoutLogAsync(WorkoutLog log)
        {
            try
            {
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
        public async Task<Exercise?> GetExerciseByIdAsync(int id)
        {
            return await _database.Table<Exercise>().Where(e => e.Id == id).FirstOrDefaultAsync();
        }
        public async Task CloseDatabase()
        {
            await _database.CloseAsync();
            Log.Debug("DatabaseHelper", "Database connection closed.");
        }
    }
}
