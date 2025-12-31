using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Web;

namespace ChordalWebApp
{
    /// <summary>
    /// Helper class for Learning Centre functionality
    /// Handles lesson progress tracking and recommendations
    /// </summary>
    public static class LearningCentreHelper
    {
        private static string ConnectionString => ConfigurationManager.ConnectionStrings["ConnectionString"].ConnectionString;

        #region Progress Tracking

        /// <summary>
        /// Marks a lesson as started for a user
        /// </summary>
        public static void MarkLessonStarted(int userId, int lessonId)
        {
            using (SqlConnection conn = new SqlConnection(ConnectionString))
            {
                try
                {
                    conn.Open();

                    using (SqlCommand cmd = new SqlCommand("sp_MarkLessonStarted", conn))
                    {
                        cmd.CommandType = CommandType.StoredProcedure;
                        cmd.Parameters.AddWithValue("@UserID", userId);
                        cmd.Parameters.AddWithValue("@LessonID", lessonId);

                        cmd.ExecuteNonQuery();
                    }
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Trace.WriteLine($"MarkLessonStarted Error: {ex}");
                    throw;
                }
            }
        }

        /// <summary>
        /// Marks a lesson as completed for a user
        /// </summary>
        public static void MarkLessonCompleted(int userId, int lessonId, SqlConnection existingConnection = null)
        {
            bool shouldCloseConnection = false;
            SqlConnection conn = existingConnection;

            try
            {
                if (conn == null)
                {
                    conn = new SqlConnection(ConnectionString);
                    conn.Open();
                    shouldCloseConnection = true;
                }

                using (SqlCommand cmd = new SqlCommand("sp_MarkLessonCompleted", conn))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@UserID", userId);
                    cmd.Parameters.AddWithValue("@LessonID", lessonId);

                    cmd.ExecuteNonQuery();
                }

                // Get lesson title for notification
                string lessonTitle = GetLessonTitle(lessonId, conn);

                // Log notification
                NotificationHelper.LogNotification(
                    userId,
                    NotificationHelper.NotificationTypes.System,
                    $"Congratulations! You completed the lesson: {lessonTitle}",
                    conn
                );
            }
            catch (Exception ex)
            {
                System.Diagnostics.Trace.WriteLine($"MarkLessonCompleted Error: {ex}");
                throw;
            }
            finally
            {
                if (shouldCloseConnection && conn != null)
                {
                    conn.Close();
                }
            }
        }

        /// <summary>
        /// Records a tutorial exercise attempt
        /// </summary>
        public static void RecordTutorialAttempt(int userId, int exerciseId, bool isCorrect, double? score = null)
        {
            using (SqlConnection conn = new SqlConnection(ConnectionString))
            {
                try
                {
                    conn.Open();

                    using (SqlCommand cmd = new SqlCommand("sp_RecordTutorialAttempt", conn))
                    {
                        cmd.CommandType = CommandType.StoredProcedure;
                        cmd.Parameters.AddWithValue("@UserID", userId);
                        cmd.Parameters.AddWithValue("@ExerciseID", exerciseId);
                        cmd.Parameters.AddWithValue("@IsCorrect", isCorrect);
                        cmd.Parameters.AddWithValue("@Score", score.HasValue ? (object)score.Value : DBNull.Value);

                        cmd.ExecuteNonQuery();
                    }
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Trace.WriteLine($"RecordTutorialAttempt Error: {ex}");
                    throw;
                }
            }
        }

        /// <summary>
        /// Gets user's overall learning progress summary
        /// </summary>
        public static DataTable GetUserLearningProgress(int userId)
        {
            DataTable dt = new DataTable();

            using (SqlConnection conn = new SqlConnection(ConnectionString))
            {
                try
                {
                    conn.Open();

                    using (SqlCommand cmd = new SqlCommand("sp_GetUserLearningProgress", conn))
                    {
                        cmd.CommandType = CommandType.StoredProcedure;
                        cmd.Parameters.AddWithValue("@UserID", userId);

                        using (SqlDataAdapter da = new SqlDataAdapter(cmd))
                        {
                            da.Fill(dt);
                        }
                    }
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Trace.WriteLine($"GetUserLearningProgress Error: {ex}");
                    throw;
                }
            }

            return dt;
        }

        /// <summary>
        /// Gets recommended next lessons for a user
        /// </summary>
        public static DataTable GetRecommendedLessons(int userId, int maxResults = 5)
        {
            DataTable dt = new DataTable();

            using (SqlConnection conn = new SqlConnection(ConnectionString))
            {
                try
                {
                    conn.Open();

                    using (SqlCommand cmd = new SqlCommand("sp_GetRecommendedLessons", conn))
                    {
                        cmd.CommandType = CommandType.StoredProcedure;
                        cmd.Parameters.AddWithValue("@UserID", userId);
                        cmd.Parameters.AddWithValue("@MaxResults", maxResults);

                        using (SqlDataAdapter da = new SqlDataAdapter(cmd))
                        {
                            da.Fill(dt);
                        }
                    }
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Trace.WriteLine($"GetRecommendedLessons Error: {ex}");
                    throw;
                }
            }

            return dt;
        }

        #endregion

        #region Lesson Data Retrieval

        /// <summary>
        /// Gets all lesson categories
        /// </summary>
        public static DataTable GetLessonCategories()
        {
            DataTable dt = new DataTable();

            using (SqlConnection conn = new SqlConnection(ConnectionString))
            {
                try
                {
                    conn.Open();

                    string query = @"
                        SELECT CategoryID, CategoryName, Description, DisplayOrder
                        FROM LessonCategories
                        ORDER BY DisplayOrder";

                    using (SqlCommand cmd = new SqlCommand(query, conn))
                    using (SqlDataAdapter da = new SqlDataAdapter(cmd))
                    {
                        da.Fill(dt);
                    }
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Trace.WriteLine($"GetLessonCategories Error: {ex}");
                    throw;
                }
            }

            return dt;
        }

        /// <summary>
        /// Gets lessons for a specific category
        /// </summary>
        public static DataTable GetLessonsByCategory(int categoryId, int? userId = null)
        {
            DataTable dt = new DataTable();

            using (SqlConnection conn = new SqlConnection(ConnectionString))
            {
                try
                {
                    conn.Open();

                    string query = @"
                        SELECT 
                            l.LessonID,
                            l.LessonSlug,
                            l.LessonTitle,
                            l.Description,
                            l.DifficultyLevel,
                            l.EstimatedMinutes,
                            l.HasTutorial,
                            " + (userId.HasValue ? @"
                            ulp.Status,
                            CASE WHEN ulp.ProgressID IS NOT NULL THEN 1 ELSE 0 END AS IsStarted,
                            CASE WHEN ulp.Status = 'Completed' THEN 1 ELSE 0 END AS IsCompleted
                            " : "NULL AS Status, 0 AS IsStarted, 0 AS IsCompleted") + @"
                        FROM Lessons l" +
                        (userId.HasValue ? @"
                        LEFT JOIN UserLessonProgress ulp ON l.LessonID = ulp.LessonID AND ulp.UserID = @UserID" : "") + @"
                        WHERE l.CategoryID = @CategoryID
                        ORDER BY l.DisplayOrder";

                    using (SqlCommand cmd = new SqlCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@CategoryID", categoryId);
                        if (userId.HasValue)
                        {
                            cmd.Parameters.AddWithValue("@UserID", userId.Value);
                        }

                        using (SqlDataAdapter da = new SqlDataAdapter(cmd))
                        {
                            da.Fill(dt);
                        }
                    }
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Trace.WriteLine($"GetLessonsByCategory Error: {ex}");
                    throw;
                }
            }

            return dt;
        }

        /// <summary>
        /// Gets lesson metadata by slug
        /// </summary>
        public static LessonMetadata GetLessonBySlug(string lessonSlug, int? userId = null)
        {
            using (SqlConnection conn = new SqlConnection(ConnectionString))
            {
                try
                {
                    conn.Open();

                    string query = @"
                        SELECT 
                            l.LessonID,
                            l.CategoryID,
                            l.LessonSlug,
                            l.LessonTitle,
                            l.Description,
                            l.DifficultyLevel,
                            l.EstimatedMinutes,
                            l.HasTutorial,
                            lc.CategoryName" +
                            (userId.HasValue ? @",
                            ulp.Status,
                            CASE WHEN ulp.ProgressID IS NOT NULL THEN 1 ELSE 0 END AS IsStarted,
                            CASE WHEN ulp.Status = 'Completed' THEN 1 ELSE 0 END AS IsCompleted
                            " : "") + @"
                        FROM Lessons l
                        INNER JOIN LessonCategories lc ON l.CategoryID = lc.CategoryID" +
                        (userId.HasValue ? @"
                        LEFT JOIN UserLessonProgress ulp ON l.LessonID = ulp.LessonID AND ulp.UserID = @UserID" : "") + @"
                        WHERE l.LessonSlug = @LessonSlug";

                    using (SqlCommand cmd = new SqlCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@LessonSlug", lessonSlug);
                        if (userId.HasValue)
                        {
                            cmd.Parameters.AddWithValue("@UserID", userId.Value);
                        }

                        using (SqlDataReader reader = cmd.ExecuteReader())
                        {
                            if (reader.Read())
                            {
                                return new LessonMetadata
                                {
                                    LessonID = Convert.ToInt32(reader["LessonID"]),
                                    CategoryID = Convert.ToInt32(reader["CategoryID"]),
                                    LessonSlug = reader["LessonSlug"].ToString(),
                                    LessonTitle = reader["LessonTitle"].ToString(),
                                    Description = reader["Description"]?.ToString(),
                                    DifficultyLevel = reader["DifficultyLevel"].ToString(),
                                    EstimatedMinutes = reader["EstimatedMinutes"] != DBNull.Value ? Convert.ToInt32(reader["EstimatedMinutes"]) : (int?)null,
                                    HasTutorial = Convert.ToBoolean(reader["HasTutorial"]),
                                    CategoryName = reader["CategoryName"].ToString(),
                                    Status = userId.HasValue && reader["Status"] != DBNull.Value ? reader["Status"].ToString() : null,
                                    IsStarted = userId.HasValue && Convert.ToBoolean(reader["IsStarted"]),
                                    IsCompleted = userId.HasValue && Convert.ToBoolean(reader["IsCompleted"])
                                };
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Trace.WriteLine($"GetLessonBySlug Error: {ex}");
                    throw;
                }
            }

            return null;
        }

        /// <summary>
        /// Gets tutorial exercises for a lesson
        /// </summary>
        public static DataTable GetTutorialExercises(int lessonId, int? userId = null)
        {
            DataTable dt = new DataTable();

            using (SqlConnection conn = new SqlConnection(ConnectionString))
            {
                try
                {
                    conn.Open();

                    string query = @"
                        SELECT 
                            te.ExerciseID,
                            te.ExerciseType,
                            te.ExerciseSlug,
                            te.Instructions,
                            te.DisplayOrder" +
                            (userId.HasValue ? @",
                            utp.AttemptCount,
                            utp.CorrectCount,
                            utp.BestScore,
                            utp.IsCompleted
                            " : "") + @"
                        FROM TutorialExercises te" +
                        (userId.HasValue ? @"
                        LEFT JOIN UserTutorialProgress utp ON te.ExerciseID = utp.ExerciseID AND utp.UserID = @UserID" : "") + @"
                        WHERE te.LessonID = @LessonID
                        ORDER BY te.DisplayOrder";

                    using (SqlCommand cmd = new SqlCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@LessonID", lessonId);
                        if (userId.HasValue)
                        {
                            cmd.Parameters.AddWithValue("@UserID", userId.Value);
                        }

                        using (SqlDataAdapter da = new SqlDataAdapter(cmd))
                        {
                            da.Fill(dt);
                        }
                    }
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Trace.WriteLine($"GetTutorialExercises Error: {ex}");
                    throw;
                }
            }

            return dt;
        }

        #endregion

        #region Helper Methods

        private static string GetLessonTitle(int lessonId, SqlConnection conn)
        {
            string query = "SELECT LessonTitle FROM Lessons WHERE LessonID = @LessonID";

            using (SqlCommand cmd = new SqlCommand(query, conn))
            {
                cmd.Parameters.AddWithValue("@LessonID", lessonId);
                return cmd.ExecuteScalar()?.ToString() ?? "Unknown Lesson";
            }
        }

        #endregion
    }

    #region Data Models

    /// <summary>
    /// Lesson metadata model
    /// </summary>
    public class LessonMetadata
    {
        public int LessonID { get; set; }
        public int CategoryID { get; set; }
        public string LessonSlug { get; set; }
        public string LessonTitle { get; set; }
        public string Description { get; set; }
        public string DifficultyLevel { get; set; }
        public int? EstimatedMinutes { get; set; }
        public bool HasTutorial { get; set; }
        public string CategoryName { get; set; }

        // User-specific progress data
        public string Status { get; set; } // null, "InProgress", or "Completed"
        public bool IsStarted { get; set; }
        public bool IsCompleted { get; set; }
    }

    #endregion
}