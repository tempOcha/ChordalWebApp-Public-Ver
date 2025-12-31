using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.SqlClient;
using System.Data;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace ChordalWebApp
{
    public partial class ManageModeration : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            // Check if user is admin
            if (Session["IsAdmin"] == null || (bool)Session["IsAdmin"] != true)
            {
                Response.Redirect("~/AdminLogin.aspx");
                return;
            }

            if (!IsPostBack)
            {
                LoadModerationData();
            }
        }

        private void LoadModerationData()
        {
            LoadModerationStatistics();
            LoadReportsQueue();
        }

        private void LoadModerationStatistics()
        {
            string connectionString = ConfigurationManager.ConnectionStrings["ConnectionString"].ConnectionString;

            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                try
                {
                    conn.Open();

                    using (SqlCommand cmd = new SqlCommand("sp_GetModerationStatistics", conn))
                    {
                        cmd.CommandType = CommandType.StoredProcedure;

                        using (SqlDataReader reader = cmd.ExecuteReader())
                        {
                            if (reader.Read())
                            {
                                lblPendingCount.Text = reader["PendingReports"].ToString();
                                lblResolvedCount.Text = reader["ResolvedReports"].ToString();
                                lblDismissedCount.Text = reader["DismissedReports"].ToString();
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine("Load Moderation Statistics Error: " + ex.Message);
                }
            }
        }

        private void LoadReportsQueue()
        {
            string connectionString = ConfigurationManager.ConnectionStrings["ConnectionString"].ConnectionString;
            string statusFilter = ddlStatusFilter.SelectedValue;
            string violationFilter = ddlViolationFilter.SelectedValue;

            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                try
                {
                    conn.Open();

                    string sql = @"
                        SELECT 
                            cr.ReportID,
                            cr.SharedProgressionID,
                            cr.CommentID,
                            cr.ReporterUserID,
                            reporter.Username AS ReporterUsername,
                            cr.ViolationType,
                            cr.ReportDetails,
                            cr.ReportDate,
                            cr.Status,
                            sp.ShareTitle AS ProgressionTitle,
                            sp.UserID AS ContentOwnerID,
                            owner.Username AS ContentOwnerUsername,
                            pc.CommentText,
                            commenter.Username AS CommenterUsername
                        FROM ContentReports cr
                        LEFT JOIN Users reporter ON cr.ReporterUserID = reporter.UserID
                        LEFT JOIN SharedProgressions sp ON cr.SharedProgressionID = sp.SharedProgressionID
                        LEFT JOIN Users owner ON sp.UserID = owner.UserID
                        LEFT JOIN ProgressionComments pc ON cr.CommentID = pc.CommentID
                        LEFT JOIN Users commenter ON pc.UserID = commenter.UserID
                        WHERE 1=1";

                    // Apply status filter
                    if (statusFilter != "All")
                    {
                        sql += " AND cr.Status = @Status";
                    }

                    // Apply violation type filter
                    if (violationFilter != "All")
                    {
                        sql += " AND cr.ViolationType = @ViolationType";
                    }

                    sql += " ORDER BY cr.ReportDate ASC";

                    using (SqlCommand cmd = new SqlCommand(sql, conn))
                    {
                        if (statusFilter != "All")
                        {
                            cmd.Parameters.AddWithValue("@Status", statusFilter);
                        }

                        if (violationFilter != "All")
                        {
                            cmd.Parameters.AddWithValue("@ViolationType", violationFilter);
                        }

                        using (SqlDataAdapter adapter = new SqlDataAdapter(cmd))
                        {
                            DataTable dt = new DataTable();
                            adapter.Fill(dt);

                            if (dt.Rows.Count > 0)
                            {
                                rptReports.DataSource = dt;
                                rptReports.DataBind();
                                pnlEmptyState.Visible = false;
                            }
                            else
                            {
                                rptReports.DataSource = null;
                                rptReports.DataBind();
                                pnlEmptyState.Visible = true;
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine("Load Reports Queue Error: " + ex.Message);
                    ShowMessage("Error loading reports: " + ex.Message, "danger");
                }
            }
        }

        protected void ApplyFilters(object sender, EventArgs e)
        {
            LoadReportsQueue();
        }

        protected void rptReports_ItemCommand(object source, RepeaterCommandEventArgs e)
        {
            int reportId = Convert.ToInt32(e.CommandArgument);
            int moderatorId = Convert.ToInt32(Session["UserID"]);

            switch (e.CommandName)
            {
                case "ViewDetails":
                    ViewReportDetails(reportId);
                    break;

                case "Approve":
                    ApplyModerationAction(reportId, moderatorId, "Approve", "Report reviewed - content approved");
                    break;

                case "Warn":
                    ApplyModerationAction(reportId, moderatorId, "Warn", "Warning issued to content owner");
                    break;

                case "Remove":
                    ApplyModerationAction(reportId, moderatorId, "Remove", "Content removed due to policy violation");
                    break;
            }
        }

        private void ViewReportDetails(int reportId)
        {
            string connectionString = ConfigurationManager.ConnectionStrings["ConnectionString"].ConnectionString;

            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                try
                {
                    conn.Open();

                    using (SqlCommand cmd = new SqlCommand("sp_GetReportDetails", conn))
                    {
                        cmd.CommandType = CommandType.StoredProcedure;
                        cmd.Parameters.AddWithValue("@ReportID", reportId);

                        using (SqlDataReader reader = cmd.ExecuteReader())
                        {
                            if (reader.Read())
                            {
                                // Build detailed view
                                string details = "<div style='line-height: 1.8;'>";
                                details += $"<p><strong>Report ID:</strong> {reader["ReportID"]}</p>";
                                details += $"<p><strong>Report Date:</strong> {Convert.ToDateTime(reader["ReportDate"]).ToString("MMMM dd, yyyy hh:mm tt")}</p>";
                                details += $"<p><strong>Status:</strong> {reader["Status"]}</p>";
                                details += $"<p><strong>Violation Type:</strong> {reader["ViolationType"]}</p>";
                                details += $"<p><strong>Reporter:</strong> {reader["ReporterUsername"]} ({reader["ReporterEmail"]})</p>";
                                details += $"<p><strong>Report Details:</strong><br/>{reader["ReportDetails"]}</p>";

                                if (reader["ProgressionTitle"] != DBNull.Value)
                                {
                                    details += $"<hr/><h4>Reported Content: Shared Progression</h4>";
                                    details += $"<p><strong>Title:</strong> {reader["ProgressionTitle"]}</p>";
                                    details += $"<p><strong>Description:</strong> {reader["ProgressionDescription"]}</p>";
                                    details += $"<p><strong>Tags:</strong> {reader["ProgressionTags"]}</p>";
                                    details += $"<p><strong>Owner:</strong> {reader["ContentOwnerUsername"]} ({reader["ContentOwnerEmail"]})</p>";
                                    details += $"<p><strong>Content Status:</strong> {reader["ProgressionStatus"]}</p>";
                                }
                                else if (reader["CommentText"] != DBNull.Value)
                                {
                                    details += $"<hr/><h4>Reported Content: Comment</h4>";
                                    details += $"<p><strong>Comment:</strong> {reader["CommentText"]}</p>";
                                    details += $"<p><strong>Posted By:</strong> {reader["CommenterUsername"]} ({reader["CommenterEmail"]})</p>";
                                    details += $"<p><strong>Posted Date:</strong> {Convert.ToDateTime(reader["CommentDate"]).ToString("MMMM dd, yyyy hh:mm tt")}</p>";
                                    bool isDeleted = Convert.ToBoolean(reader["CommentDeleted"]);
                                    details += $"<p><strong>Status:</strong> {(isDeleted ? "Deleted" : "Active")}</p>";
                                }

                                if (reader["AdminNotes"] != DBNull.Value && !string.IsNullOrEmpty(reader["AdminNotes"].ToString()))
                                {
                                    details += $"<hr/><p><strong>Admin Notes:</strong><br/>{reader["AdminNotes"]}</p>";
                                }

                                if (reader["ModeratorUsername"] != DBNull.Value)
                                {
                                    details += $"<p><strong>Moderated By:</strong> {reader["ModeratorUsername"]}</p>";
                                    if (reader["ResolvedDate"] != DBNull.Value)
                                    {
                                        details += $"<p><strong>Resolved Date:</strong> {Convert.ToDateTime(reader["ResolvedDate"]).ToString("MMMM dd, yyyy hh:mm tt")}</p>";
                                    }
                                }

                                details += "</div>";

                                pnlModalDetails.Controls.Clear();
                                pnlModalDetails.Controls.Add(new LiteralControl(details));

                                // Show modal via JavaScript
                                ScriptManager.RegisterStartupScript(this, GetType(), "ShowModal", "showModal();", true);
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine("View Report Details Error: " + ex.Message);
                    ShowMessage("Error loading report details: " + ex.Message, "danger");
                }
            }
        }

        private void ApplyModerationAction(int reportId, int moderatorId, string action, string adminNotes)
        {
            string connectionString = ConfigurationManager.ConnectionStrings["ConnectionString"].ConnectionString;

            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                try
                {
                    conn.Open();

                    using (SqlCommand cmd = new SqlCommand("sp_ApplyModerationAction", conn))
                    {
                        cmd.CommandType = CommandType.StoredProcedure;
                        cmd.Parameters.AddWithValue("@ReportID", reportId);
                        cmd.Parameters.AddWithValue("@ModeratorUserID", moderatorId);
                        cmd.Parameters.AddWithValue("@Action", action);
                        cmd.Parameters.AddWithValue("@AdminNotes", adminNotes);

                        cmd.ExecuteNonQuery();
                    }

                    // Show success message
                    string actionMessage = action == "Approve" ? "Report dismissed - content approved" :
                                         action == "Warn" ? "Warning sent to user" :
                                         "Content removed successfully";
                    ShowMessage(actionMessage, "success");

                    // Reload data
                    LoadModerationData();
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine("Apply Moderation Action Error: " + ex.Message);
                    ShowMessage("Error applying moderation action: " + ex.Message, "danger");
                }
            }
        }

        private void ShowMessage(string message, string type)
        {
            lblMessage.Text = message;
            lblMessage.CssClass = type == "success" ? "alert alert-success" : "alert alert-danger";
            lblMessage.Style["background-color"] = type == "success" ? "#d4edda" : "#f8d7da";
            lblMessage.Style["color"] = type == "success" ? "#155724" : "#721c24";
            lblMessage.Style["border"] = type == "success" ? "1px solid #c3e6cb" : "1px solid #f5c6cb";
            lblMessage.Visible = true;
        }
    }
}