using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace ChordalWebApp
{
    public partial class Lesson : System.Web.UI.Page
    {
        // Properties for CSS class binding
        protected string DifficultyLevel { get; set; }
        protected string LessonStatus { get; set; }

        protected void Page_Load(object sender, EventArgs e)
        {
            // Check authentication
            if (Session["UserID"] == null)
            {
                Response.Redirect("Login.aspx?redirect=" + Server.UrlEncode(Request.RawUrl));
                return;
            }

            if (!IsPostBack)
            {
                string slug = Request.QueryString["slug"];

                if (string.IsNullOrEmpty(slug))
                {
                    ShowError("No lesson specified. Please select a lesson from the Learning Centre.");
                    return;
                }

                // Check if this lesson has a dedicated ASPX page
                if (HasDedicatedAspxPage(slug))
                {
                    // Redirect to the dedicated ASPX page
                    Response.Redirect(GetAspxPageUrl(slug));
                    return;
                }

                //LoadLessonBySlug(slug);
            }
        }

        /// <summary>
        /// Checks if a lesson has a dedicated ASPX page
        /// </summary>
        private bool HasDedicatedAspxPage(string slug)
        {
            // List of lessons with dedicated ASPX pages
            string[] aspxLessons = new string[]
            {
                "basic-triads"
                
            };

            return aspxLessons.Contains(slug.ToLower());
        }

        /// <summary>
        /// Gets the URL for a lesson's dedicated ASPX page
        /// </summary>
        private string GetAspxPageUrl(string slug)
        {
            // Convert slug to PascalCase page name
            // basic-triads -> BasicTriads.aspx
            string[] parts = slug.Split('-');
            string pageName = string.Join("", parts.Select(p =>
                char.ToUpper(p[0]) + p.Substring(1).ToLower()
            ));

            return $"{pageName}.aspx";
        }

        
        private void LoadLessonBySlug(string slug)
        {
            int userId = Convert.ToInt32(Session["UserID"]);

            // Use LearningCentreHelper to get lesson metadata
            var lessonMetadata = LearningCentreHelper.GetLessonBySlug(slug, userId);

            if (lessonMetadata == null)
            {
                ShowError($"Lesson '{slug}' not found in our database. Please check the URL or return to the Learning Centre.");
                return;
            }

            // Store for later use
            ViewState["LessonID"] = lessonMetadata.LessonID;
            ViewState["LessonSlug"] = slug;

            // Set lesson metadata
            litPageTitle.Text = lessonMetadata.LessonTitle + " - Chordal Learning";
            litLessonTitle.Text = lessonMetadata.LessonTitle;
            litLessonTitleBreadcrumb.Text = lessonMetadata.LessonTitle;
            litLessonDescription.Text = lessonMetadata.Description;
            litCategoryName.Text = lessonMetadata.CategoryName;
            litDifficulty.Text = lessonMetadata.DifficultyLevel;
            litEstimatedMinutes.Text = lessonMetadata.EstimatedMinutes.HasValue
                ? lessonMetadata.EstimatedMinutes.ToString()
                : "N/A";

            // Store for CSS class binding
            DifficultyLevel = lessonMetadata.DifficultyLevel;
            LessonStatus = lessonMetadata.Status;

            // Show interactive badge if tutorial available
            if (lessonMetadata.HasTutorial)
            {
                pnlInteractiveBadge.Visible = true;
            }

            // Show status badge if started or completed
            if (!string.IsNullOrEmpty(lessonMetadata.Status) && lessonMetadata.Status != "Not Started")
            {
                pnlStatusBadge.Visible = true;
                litStatus.Text = lessonMetadata.Status;

                if (lessonMetadata.IsCompleted)
                {
                    btnMarkComplete.Text = "✓ Completed";
                    btnMarkComplete.Enabled = false;
                }
            }

            // Load the static HTML lesson content
            LoadLessonContent(slug);

            // Mark lesson as started if not already
            if (!lessonMetadata.IsStarted)
            {
                LearningCentreHelper.MarkLessonStarted(userId, lessonMetadata.LessonID);
            }

            // Show the lesson content
            pnlLessonContent.Visible = true;
            pnlError.Visible = false;
        }

        private void LoadLessonContent(string slug)
        {
            try
            {
                // Construct path to static HTML lesson file
                string lessonFilePath = Server.MapPath($"~/Lessons/{slug}.html");

                if (!File.Exists(lessonFilePath))
                {
                    ShowError($"Lesson content file '{slug}.html' not found. This lesson may not be available yet.");
                    return;
                }

                // Read the HTML content
                string htmlContent = File.ReadAllText(lessonFilePath);

                // Inject the content
                litLessonContent.Text = htmlContent;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Trace.WriteLine($"LoadLessonContent Error: {ex}");
                ShowError("Error loading lesson content. Please try again later.");
            }
        }

        

        private void ShowError(string message)
        {
            litErrorMessage.Text = message;
            pnlError.Visible = true;
            pnlLessonContent.Visible = false;
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
                if (ViewState["LessonID"] != null)
                {
                    int lessonId = Convert.ToInt32(ViewState["LessonID"]);

                    // Mark lesson as completed
                    LearningCentreHelper.MarkLessonCompleted(userId, lessonId);

                    // Update UI
                    btnMarkComplete.Text = "✓ Completed";
                    btnMarkComplete.Enabled = false;
                    lblCompleteMessage.Text = "Lesson marked as complete!";
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
    }
}