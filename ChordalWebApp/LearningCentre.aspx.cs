using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace ChordalWebApp
{
    public partial class LearningCentre : System.Web.UI.Page
    {
        private int? CurrentUserId => Session["UserID"] != null ? (int?)Convert.ToInt32(Session["UserID"]) : null;

        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack)
            {
                LoadLearningCentre();
            }
        }

        private void LoadLearningCentre()
        {
            LoadCategories();

            if (CurrentUserId.HasValue)
            {
                LoadProgressSummary();
                LoadRecommendations();
            }
        }

        private void LoadProgressSummary()
        {
            try
            {
                DataTable progressData = LearningCentreHelper.GetUserLearningProgress(CurrentUserId.Value);

                if (progressData != null && progressData.Rows.Count > 0)
                {
                    int totalLessons = 0;
                    int completedLessons = 0;
                    int inProgressLessons = 0;

                    foreach (DataRow row in progressData.Rows)
                    {
                        totalLessons += Convert.ToInt32(row["TotalLessons"]);
                        completedLessons += Convert.ToInt32(row["CompletedLessons"]);
                        int started = Convert.ToInt32(row["StartedLessons"]);
                        int completed = Convert.ToInt32(row["CompletedLessons"]);
                        inProgressLessons += (started - completed);
                    }

                    litTotalLessons.Text = totalLessons.ToString();
                    litCompletedLessons.Text = completedLessons.ToString();
                    litInProgressLessons.Text = inProgressLessons.ToString();

                    if (totalLessons > 0)
                    {
                        double progressPercentage = ((double)completedLessons / totalLessons) * 100;
                        litOverallProgress.Text = progressPercentage.ToString("F0");
                    }
                    else
                    {
                        litOverallProgress.Text = "0";
                    }

                    pnlProgressSummary.Visible = true;
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Trace.WriteLine($"LoadProgressSummary Error: {ex}");
            }
        }

        private void LoadRecommendations()
        {
            try
            {
                DataTable recommendations = LearningCentreHelper.GetRecommendedLessons(CurrentUserId.Value, 3);

                if (recommendations != null && recommendations.Rows.Count > 0)
                {
                    rptRecommended.DataSource = recommendations;
                    rptRecommended.DataBind();
                    pnlRecommended.Visible = true;
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Trace.WriteLine($"LoadRecommendations Error: {ex}");
            }
        }

        private void LoadCategories()
        {
            try
            {
                DataTable categories = CurrentUserId.HasValue
                    ? LearningCentreHelper.GetUserLearningProgress(CurrentUserId.Value)
                    : LearningCentreHelper.GetLessonCategories();

                if (categories != null && categories.Rows.Count > 0)
                {
                    rptCategories.DataSource = categories;
                    rptCategories.DataBind();
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Trace.WriteLine($"LoadCategories Error: {ex}");
            }
        }

        protected void rptCategories_ItemDataBound(object sender, RepeaterItemEventArgs e)
        {
            if (e.Item.ItemType == ListItemType.Item || e.Item.ItemType == ListItemType.AlternatingItem)
            {
                DataRowView drv = e.Item.DataItem as DataRowView;
                if (drv != null)
                {
                    int categoryId = Convert.ToInt32(drv["CategoryID"]);

                    Repeater rptLessons = e.Item.FindControl("rptLessons") as Repeater;
                    if (rptLessons != null)
                    {
                        DataTable lessons = LearningCentreHelper.GetLessonsByCategory(categoryId, CurrentUserId);
                        rptLessons.DataSource = lessons;
                        rptLessons.DataBind();
                    }
                }
            }
        }

        protected string GetLessonStatusClass(object status)
        {
            if (status == null || status == DBNull.Value)
                return "not-started";

            string statusStr = status.ToString();

            if (statusStr == "Completed")
                return "completed";
            else if (statusStr == "InProgress")
                return "in-progress";
            else
                return "not-started";
        }

        protected string GetLessonStatusIcon(object status)
        {
            if (status == null || status == DBNull.Value)
                return "○";

            string statusStr = status.ToString();

            if (statusStr == "Completed")
                return "✓";
            else if (statusStr == "InProgress")
                return "📖";
            else
                return "○";
        }
    }
}