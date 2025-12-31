using System;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Web.UI;

namespace ChordalWebApp
{
    public partial class ReportContent : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            // Check if user is logged in
            if (Session["UserID"] == null)
            {
                Response.Redirect("Login.aspx?returnUrl=" + Server.UrlEncode(Request.RawUrl));
                return;
            }

            if (!IsPostBack)
            {
                LoadContentInfo();
            }
        }

        private void LoadContentInfo()
        {
            string contentType = Request.QueryString["type"];
            string contentId = Request.QueryString["id"];

            if (string.IsNullOrEmpty(contentType) || string.IsNullOrEmpty(contentId))
            {
                ShowError("Invalid content reference. Please try again.");
                btnSubmitReport.Enabled = false;
                return;
            }

            hdnContentType.Value = contentType;
            hdnContentID.Value = contentId;

            string connectionString = ConfigurationManager.ConnectionStrings["ConnectionString"].ConnectionString;

            try
            {
                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    conn.Open();

                    if (contentType.ToLower() == "progression")
                    {
                        LoadProgressionInfo(conn, Convert.ToInt32(contentId));
                        lnkCancel.NavigateUrl = $"CommunityProgressionView.aspx?id={contentId}";
                    }
                    else if (contentType.ToLower() == "comment")
                    {
                        LoadCommentInfo(conn, Convert.ToInt32(contentId));
                    }
                    else
                    {
                        ShowError("Unknown content type.");
                        btnSubmitReport.Enabled = false;
                    }
                }
            }
            catch (Exception ex)
            {
                ShowError("Error loading content information: " + ex.Message);
                System.Diagnostics.Trace.WriteLine("LoadContentInfo Error: " + ex.ToString());
                btnSubmitReport.Enabled = false;
            }
        }

        private void LoadProgressionInfo(SqlConnection conn, int sharedProgressionId)
        {
            string query = @"
                SELECT sp.ShareTitle, sp.ShareDescription, u.Username
                FROM SharedProgressions sp
                INNER JOIN Users u ON sp.UserID = u.UserID
                WHERE sp.SharedProgressionID = @SharedProgressionID";

            using (SqlCommand cmd = new SqlCommand(query, conn))
            {
                cmd.Parameters.AddWithValue("@SharedProgressionID", sharedProgressionId);

                using (SqlDataReader reader = cmd.ExecuteReader())
                {
                    if (reader.Read())
                    {
                        litContentType.Text = "Shared Progression";

                        string title = reader["ShareTitle"].ToString();
                        string author = reader["Username"].ToString();
                        string description = reader["ShareDescription"] == DBNull.Value ||
                                           string.IsNullOrWhiteSpace(reader["ShareDescription"].ToString())
                            ? "(No description provided)"
                            : reader["ShareDescription"].ToString();

                        if (description.Length > 100)
                        {
                            description = description.Substring(0, 100) + "...";
                        }

                        litContentPreview.Text = $"<strong>\"{Server.HtmlEncode(title)}\"</strong> by {Server.HtmlEncode(author)}<br/>" +
                                                $"<em>{Server.HtmlEncode(description)}</em>";
                    }
                    else
                    {
                        ShowError("Progression not found.");
                        btnSubmitReport.Enabled = false;
                    }
                }
            }
        }

        private void LoadCommentInfo(SqlConnection conn, int commentId)
        {
            string query = @"
                SELECT c.CommentText, u.Username, sp.ShareTitle
                FROM ProgressionComments c
                INNER JOIN Users u ON c.UserID = u.UserID
                INNER JOIN SharedProgressions sp ON c.SharedProgressionID = sp.SharedProgressionID
                WHERE c.CommentID = @CommentID";

            using (SqlCommand cmd = new SqlCommand(query, conn))
            {
                cmd.Parameters.AddWithValue("@CommentID", commentId);

                using (SqlDataReader reader = cmd.ExecuteReader())
                {
                    if (reader.Read())
                    {
                        litContentType.Text = "Comment";

                        string commentText = reader["CommentText"].ToString();
                        string author = reader["Username"].ToString();
                        string progressionTitle = reader["ShareTitle"].ToString();

                        if (commentText.Length > 150)
                        {
                            commentText = commentText.Substring(0, 150) + "...";
                        }

                        litContentPreview.Text = $"On progression <strong>\"{Server.HtmlEncode(progressionTitle)}\"</strong><br/>" +
                                                $"By {Server.HtmlEncode(author)}: <em>\"{Server.HtmlEncode(commentText)}\"</em>";
                    }
                    else
                    {
                        ShowError("Comment not found.");
                        btnSubmitReport.Enabled = false;
                    }
                }
            }
        }

        protected void btnSubmitReport_Click(object sender, EventArgs e)
        {
            if (!Page.IsValid)
                return;

            int userId = Convert.ToInt32(Session["UserID"]);
            string contentType = hdnContentType.Value;
            int contentId = Convert.ToInt32(hdnContentID.Value);
            string violationType = rblViolationType.SelectedValue;
            string reportDetails = txtReportDetails.Text.Trim();

            string connectionString = ConfigurationManager.ConnectionStrings["ConnectionString"].ConnectionString;

            try
            {
                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    conn.Open();

                    using (SqlCommand cmd = new SqlCommand("sp_ReportContent", conn))
                    {
                        cmd.CommandType = CommandType.StoredProcedure;

                        cmd.Parameters.AddWithValue("@ReporterUserID", userId);

                        if (contentType.ToLower() == "progression")
                        {
                            cmd.Parameters.AddWithValue("@SharedProgressionID", contentId);
                            cmd.Parameters.AddWithValue("@CommentID", DBNull.Value);
                        }
                        else if (contentType.ToLower() == "comment")
                        {
                            cmd.Parameters.AddWithValue("@SharedProgressionID", DBNull.Value);
                            cmd.Parameters.AddWithValue("@CommentID", contentId);
                        }

                        cmd.Parameters.AddWithValue("@ViolationType", violationType);
                        cmd.Parameters.AddWithValue("@ReportDetails", reportDetails);

                        object result = cmd.ExecuteScalar();

                        if (result != null)
                        {
                            int reportId = Convert.ToInt32(result);

                            // Log notification to user
                            NotificationHelper.LogNotification(
                                userId,
                                NotificationHelper.NotificationTypes.System,
                                $"Your content report has been submitted and will be reviewed by our moderation team.",
                                conn
                            );

                            // Show success panel
                            pnlReportForm.Visible = false;
                            pnlSuccess.Visible = true;
                        }
                        else
                        {
                            ShowError("Failed to submit report. Please try again.");
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                ShowError("Error submitting report: " + ex.Message);
                System.Diagnostics.Trace.WriteLine("SubmitReport Error: " + ex.ToString());
            }
        }

        private void ShowError(string message)
        {
            pnlMessages.Visible = true;
            litMessage.Text = "<div class='alert alert-error'>" + Server.HtmlEncode(message) + "</div>";
        }
    }
}
