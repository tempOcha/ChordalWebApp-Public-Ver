using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace ChordalWebApp
{
    public partial class MyProgress : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            // Check if user is logged in
            if (Session["UserID"] == null)
            {
                Response.Redirect("Login.aspx?returnUrl=" + Server.UrlEncode(Request.Url.PathAndQuery));
                return;
            }

            if (!IsPostBack)
            {
                LoadProgressData();
            }
        }

        /// <summary>
        /// Loads all progress data for the current user
        /// </summary>
        private void LoadProgressData()
        {
            try
            {
                int userId = Convert.ToInt32(Session["UserID"]);

                // Get category-level progress
                DataTable dtProgress = LearningCentreHelper.GetUserLearningProgress(userId);

                if (dtProgress != null && dtProgress.Rows.Count > 0)
                {
                    // Calculate overall statistics
                    CalculateOverallStats(dtProgress);

                    // Generate train station HTML
                    System.Text.StringBuilder trainHtml = new System.Text.StringBuilder();
                    foreach (DataRow row in dtProgress.Rows)
                    {
                        string categoryName = row["CategoryName"].ToString();
                        decimal completionPercentage = Convert.ToDecimal(row["CompletionPercentage"]);
                        int completedLessons = Convert.ToInt32(row["CompletedLessons"]);
                        int totalLessons = Convert.ToInt32(row["TotalLessons"]);

                        trainHtml.Append(GetTrainStationHtml(categoryName, completionPercentage, completedLessons, totalLessons));
                    }
                    litTrainStations.Text = trainHtml.ToString();

                    // Also bind for category summary cards
                    rptCategorySummary.DataSource = dtProgress;
                    rptCategorySummary.DataBind();

                    pnlNoCategoryData.Visible = false;

                    // Check for recent achievements
                    DisplayRecentAchievements(dtProgress);
                }
                else
                {
                    // No progress data yet
                    SetDefaultStats();
                    pnlNoCategoryData.Visible = true;
                }

                // Get recommended lessons
                DataTable dtRecommendations = LearningCentreHelper.GetRecommendedLessons(userId, 6);

                if (dtRecommendations != null && dtRecommendations.Rows.Count > 0)
                {
                    rptRecommendations.DataSource = dtRecommendations;
                    rptRecommendations.DataBind();
                    pnlRecommendations.Visible = true;
                    pnlNoRecommendations.Visible = false;
                }
                else
                {
                    pnlRecommendations.Visible = false;
                    pnlNoRecommendations.Visible = true;
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Trace.WriteLine($"MyProgress LoadProgressData Error: {ex}");
                // Display error message to user
                ShowErrorMessage("Unable to load progress data. Please try again later.");
            }
        }

        /// <summary>
        /// Calculates overall statistics from category progress data
        /// </summary>
        private void CalculateOverallStats(DataTable dtProgress)
        {
            int totalLessons = 0;
            int completedLessons = 0;
            int startedLessons = 0;

            foreach (DataRow row in dtProgress.Rows)
            {
                totalLessons += Convert.ToInt32(row["TotalLessons"]);
                completedLessons += Convert.ToInt32(row["CompletedLessons"]);
                startedLessons += Convert.ToInt32(row["StartedLessons"]);
            }

            // Calculate overall percentage
            decimal overallPercentage = totalLessons > 0
                ? Math.Round((decimal)completedLessons / totalLessons * 100, 0)
                : 0;

            // Set literal values
            litTotalLessons.Text = totalLessons.ToString();
            litCompletedLessons.Text = completedLessons.ToString();
            litInProgressLessons.Text = (startedLessons - completedLessons).ToString();
            litOverallPercentage.Text = overallPercentage.ToString("0");
        }

        /// <summary>
        /// Sets default stats when no progress exists
        /// </summary>
        private void SetDefaultStats()
        {
            litTotalLessons.Text = "0";
            litCompletedLessons.Text = "0";
            litInProgressLessons.Text = "0";
            litOverallPercentage.Text = "0";
        }

        /// <summary>
        /// Displays recent achievements based on completed categories or lessons
        /// </summary>
        private void DisplayRecentAchievements(DataTable dtProgress)
        {
            List<string> achievements = new List<string>();

            foreach (DataRow row in dtProgress.Rows)
            {
                decimal completionPercentage = Convert.ToDecimal(row["CompletionPercentage"]);
                int completedLessons = Convert.ToInt32(row["CompletedLessons"]);
                string categoryName = row["CategoryName"].ToString();

                // Check for category completion
                if (completionPercentage == 100)
                {
                    achievements.Add($"✓ Mastered {categoryName}");
                }
                else if (completionPercentage >= 50)
                {
                    achievements.Add($"⭐ Halfway through {categoryName}");
                }
                else if (completedLessons > 0)
                {
                    achievements.Add($" Started {categoryName}");
                }
            }

            // Display achievements if any
            if (achievements.Count > 0)
            {
                pnlAchievements.Visible = true;
                litAchievements.Text = string.Join("", achievements.Take(5).Select(a =>
                    $"<span class='achievement-badge'>{a}</span>"));
            }
            else
            {
                pnlAchievements.Visible = false;
            }
        }

        /// <summary>
        /// Generates HTML for a train station based on category progress
        /// </summary>
        protected string GetTrainStationHtml(string categoryName, decimal completionPercentage, int completedLessons, int totalLessons)
        {
            string statusClass = "not-started";
            string statusIcon = "○";
            string badgeClass = "not-started";
            string badgeText = "Not Started";

            if (completionPercentage == 100)
            {
                statusClass = "completed";
                statusIcon = "✓";
                badgeClass = "completed";
                badgeText = "Completed";
            }
            else if (completionPercentage > 0)
            {
                statusClass = "in-progress";
                statusIcon = "○";
                badgeClass = "in-progress";
                badgeText = "In Progress";
            }

            string progressBarHtml = completionPercentage > 0
                ? $@"<div class='station-progress'>
                        <div class='station-progress-bar'>
                            <div class='station-progress-fill' style='width: {completionPercentage}%'></div>
                        </div>
                        <div class='station-progress-text'>{completedLessons} of {totalLessons} lessons completed ({completionPercentage:0}%)</div>
                    </div>"
                : "";

            return $@"
                <div class='train-station {statusClass}'>
                    <div class='station-marker'>
                        <div class='station-dot'>{statusIcon}</div>
                    </div>
                    <div class='station-card'>
                        <div class='station-header'>
                            <div class='station-category'>{Server.HtmlEncode(categoryName)}</div>
                            <span class='station-badge {badgeClass}'>{badgeText}</span>
                        </div>
                        <h3 class='station-title'>{Server.HtmlEncode(categoryName)}</h3>
                        <p class='station-description'>
                            {(completionPercentage == 100
                                ? "🎉 Congratulations! You've completed all lessons in this category."
                                : completionPercentage > 0
                                    ? "Keep going! You're making great progress through this category."
                                    : "Ready to start? This category awaits your exploration.")}
                        </p>
                        {progressBarHtml}
                    </div>
                </div>";
        }

        /// <summary>
        /// Displays an error message to the user
        /// </summary>
        private void ShowErrorMessage(string message)
        {
            // Simple implementation - you can enhance this with a more sophisticated notification system
            litTotalLessons.Text = "—";
            litCompletedLessons.Text = "—";
            litInProgressLessons.Text = "—";
            litOverallPercentage.Text = "—";

            pnlNoCategoryData.Visible = true;
            // You could use NotificationHelper here if you want to log the error
        }
    }
}