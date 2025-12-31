using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Services;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace ChordalWebApp
{
    public partial class BasicTriads : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            // Check authentication
            if (Session["UserID"] == null)
            {
                Response.Redirect("~/Login.aspx");
                return;
            }

            if (!IsPostBack)
            {
                LoadLessonData();
            }
        }

        private void LoadLessonData()
        {
            int userId = Convert.ToInt32(Session["UserID"]);

            try
            {
                // Get lesson metadata
                var lesson = LearningCentreHelper.GetLessonBySlug("basic-triads", userId);

                if (lesson != null)
                {
                    hdnLessonID.Value = lesson.LessonID.ToString();

                    // Mark lesson as started if not already
                    if (!lesson.IsStarted)
                    {
                        LearningCentreHelper.MarkLessonStarted(userId, lesson.LessonID);
                    }

                    // Check if lesson is already completed
                    if (lesson.IsCompleted)
                    {
                        btnMarkComplete.Text = "✓ Completed";
                        btnMarkComplete.Enabled = false;
                    }

                    // Load tutorial exercises and check completion status
                    LoadTutorialExercises(lesson.LessonID, userId);
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Trace.WriteLine($"LoadLessonData Error: {ex}");
            }
        }

        private void LoadTutorialExercises(int lessonId, int userId)
        {
            try
            {
                var exercises = LearningCentreHelper.GetTutorialExercises(lessonId, userId);

                bool allCompleted = true;
                int exerciseIndex = 1;

                if (exercises != null && exercises.Rows.Count > 0)
                {
                    foreach (System.Data.DataRow row in exercises.Rows)
                    {
                        int exerciseId = Convert.ToInt32(row["ExerciseID"]);

                        // Check if this exercise is completed
                        bool isCompleted = false;
                        if (row.Table.Columns.Contains("IsCompleted") && row["IsCompleted"] != DBNull.Value)
                        {
                            isCompleted = Convert.ToBoolean(row["IsCompleted"]);
                        }

                        if (!isCompleted)
                        {
                            allCompleted = false;
                        }

                        // Map to hidden fields
                        switch (exerciseIndex)
                        {
                            case 1:
                                hdnExercise1ID.Value = exerciseId.ToString();
                                break;
                            case 2:
                                hdnExercise2ID.Value = exerciseId.ToString();
                                break;
                            case 3:
                                hdnExercise3ID.Value = exerciseId.ToString();
                                break;
                        }

                        exerciseIndex++;
                        if (exerciseIndex > 3) break;
                    }
                }
                else
                {
                    allCompleted = false;
                }

                // Set tutorial completed flag
                hdnTutorialCompleted.Value = allCompleted.ToString().ToLower();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Trace.WriteLine($"LoadTutorialExercises Error: {ex}");
            }
        }

        protected void btnMarkComplete_Click(object sender, EventArgs e)
        {
            if (Session["UserID"] == null)
            {
                Response.Redirect("Login.aspx");
                return;
            }

            int userId = Convert.ToInt32(Session["UserID"]);

            try
            {
                if (!string.IsNullOrEmpty(hdnLessonID.Value))
                {
                    int lessonId = Convert.ToInt32(hdnLessonID.Value);

                    // Mark lesson as completed
                    LearningCentreHelper.MarkLessonCompleted(userId, lessonId);

                    // Update UI
                    btnMarkComplete.Text = "✓ Completed";
                    btnMarkComplete.Enabled = false;
                    lblCompleteMessage.Text = "Lesson completed! Great job!";
                    lblCompleteMessage.Visible = true;
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Trace.WriteLine($"btnMarkComplete_Click Error: {ex}");
                lblCompleteMessage.Text = "Error marking lesson as complete. Please try again.";
                lblCompleteMessage.ForeColor = System.Drawing.Color.Red;
                lblCompleteMessage.Visible = true;
            }
        }

        /// <summary>
        /// Web method to record tutorial attempts via AJAX
        /// </summary>
        [WebMethod]
        public static void RecordTutorialAttempt(int exerciseId, bool isCorrect, double score)
        {
            try
            {
                // Get user ID from session
                if (HttpContext.Current.Session["UserID"] == null)
                {
                    throw new Exception("User not authenticated");
                }

                int userId = Convert.ToInt32(HttpContext.Current.Session["UserID"]);

                // Record the attempt using the existing stored procedure
                LearningCentreHelper.RecordTutorialAttempt(userId, exerciseId, isCorrect, score);

                System.Diagnostics.Trace.WriteLine($"Tutorial attempt recorded: UserID={userId}, ExerciseID={exerciseId}, IsCorrect={isCorrect}, Score={score}");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Trace.WriteLine($"RecordTutorialAttempt WebMethod Error: {ex}");
                throw;
            }
        }
    }
}