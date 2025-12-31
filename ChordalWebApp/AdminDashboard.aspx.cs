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
    public partial class AdminDashboard : System.Web.UI.Page
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
                LoadAdminDashboard();
            }
        }

        private void LoadAdminDashboard()
        {
            // Display admin name
            if (Session["Username"] != null)
            {
                lblAdminName.Text = Session["Username"].ToString();
            }

            // Load moderation statistics
            LoadModerationStatistics();

            // Display last login
            if (Session["UserID"] != null)
            {
                int userId = Convert.ToInt32(Session["UserID"]);
                LoadLastLoginDate(userId);
            }
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
                                lblPendingReports.Text = reader["PendingReports"].ToString();
                                lblUnderReview.Text = reader["ProgressionsUnderReview"].ToString();
                                lblResolvedReports.Text = reader["ResolvedReports"].ToString();
                                lblRemovedContent.Text = reader["RemovedProgressions"].ToString();
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine("Load Moderation Statistics Error: " + ex.Message);
                    // Set default values on error
                    lblPendingReports.Text = "0";
                    lblUnderReview.Text = "0";
                    lblResolvedReports.Text = "0";
                    lblRemovedContent.Text = "0";
                }
            }
        }

        private void LoadLastLoginDate(int userId)
        {
            string connectionString = ConfigurationManager.ConnectionStrings["ConnectionString"].ConnectionString;

            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                try
                {
                    conn.Open();

                    string sql = "SELECT LastLoginDate FROM Users WHERE UserID = @UserID";
                    using (SqlCommand cmd = new SqlCommand(sql, conn))
                    {
                        cmd.Parameters.AddWithValue("@UserID", userId);

                        object result = cmd.ExecuteScalar();
                        if (result != null && result != DBNull.Value)
                        {
                            DateTime lastLogin = Convert.ToDateTime(result);
                            lblLastLogin.Text = lastLogin.ToString("MMMM dd, yyyy hh:mm tt");
                        }
                        else
                        {
                            lblLastLogin.Text = "First login";
                        }
                    }
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine("Load Last Login Error: " + ex.Message);
                    lblLastLogin.Text = "Unknown";
                }
            }
        }

        protected void btnLogout_Click(object sender, EventArgs e)
        {
            // Log admin logout activity
            if (Session["UserID"] != null)
            {
                int userId = Convert.ToInt32(Session["UserID"]);
                LogAdminActivity(userId, "Admin Logout", "Admin logged out successfully");
            }

            // Clear all admin session data
            Session.Remove("Username");
            Session.Remove("UserID");
            Session.Remove("IsAdmin");
            Session.Remove("AdminEmail");
            Session.Clear();
            Session.Abandon();

            // Redirect to admin login page
            Response.Redirect("~/AdminLogin.aspx");
        }

        private void LogAdminActivity(int userId, string activityType, string activityDetails)
        {
            string connectionString = ConfigurationManager.ConnectionStrings["ConnectionString"].ConnectionString;

            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                try
                {
                    conn.Open();

                    string sql = @"INSERT INTO UserNotifications (UserID, NotificationType, Message, IsRead) 
                                  VALUES (@UserID, @NotificationType, @Message, 1)";

                    using (SqlCommand cmd = new SqlCommand(sql, conn))
                    {
                        cmd.Parameters.AddWithValue("@UserID", userId);
                        cmd.Parameters.AddWithValue("@NotificationType", activityType);
                        cmd.Parameters.AddWithValue("@Message", activityDetails + " at " + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));
                        cmd.ExecuteNonQuery();
                    }
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine("Log Admin Activity Error: " + ex.Message);
                }
            }
        }
    }
}