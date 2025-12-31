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
    public partial class Notifications : System.Web.UI.Page
    {
        private bool showUnreadOnly = false;

        protected void Page_Load(object sender, EventArgs e)
        {
            if (Session["UserID"] == null)
            {
                Response.Redirect("~/Login.aspx?ReturnUrl=" + Server.UrlEncode(Request.Url.PathAndQuery));
                return;
            }

            if (!IsPostBack)
            {
                BindNotifications();
            }
        }

        protected void lnkShowAll_Click(object sender, EventArgs e)
        {
            showUnreadOnly = false;
            ViewState["ShowUnreadOnly"] = false;
            BindNotifications();

            // Update button styles
            lnkShowAll.CssClass = "btn";
            lnkShowAll.Style["background"] = "#77aaff";
            lnkShowAll.Style["color"] = "white";
            lnkShowUnread.CssClass = "btn";
            lnkShowUnread.Style.Remove("background");
            lnkShowUnread.Style.Remove("color");
        }

        protected void lnkShowUnread_Click(object sender, EventArgs e)
        {
            showUnreadOnly = true;
            ViewState["ShowUnreadOnly"] = true;
            BindNotifications();

            // Update button styles
            lnkShowUnread.CssClass = "btn";
            lnkShowUnread.Style["background"] = "#77aaff";
            lnkShowUnread.Style["color"] = "white";
            lnkShowAll.CssClass = "btn";
            lnkShowAll.Style.Remove("background");
            lnkShowAll.Style.Remove("color");
        }

        protected void lnkMarkAllRead_Click(object sender, EventArgs e)
        {
            int userId = Convert.ToInt32(Session["UserID"]);
            string connectionString = ConfigurationManager.ConnectionStrings["ConnectionString"].ConnectionString;

            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                try
                {
                    conn.Open();
                    string updateQuery = "UPDATE UserNotifications SET IsRead = 1 WHERE UserID = @UserID AND IsRead = 0";

                    using (SqlCommand cmd = new SqlCommand(updateQuery, conn))
                    {
                        cmd.Parameters.AddWithValue("@UserID", userId);
                        int rowsAffected = cmd.ExecuteNonQuery();

                        if (rowsAffected > 0)
                        {
                            lblStatus.Text = $"Marked {rowsAffected} notification(s) as read.";
                            lblStatus.CssClass = "text-success";
                        }
                    }

                    BindNotifications();
                }
                catch (Exception ex)
                {
                    lblStatus.Text = "Error marking notifications as read.";
                    lblStatus.CssClass = "text-danger";
                    System.Diagnostics.Trace.WriteLine("MarkAllRead Error: " + ex.ToString());
                }
            }
        }

        protected void rptNotifications_ItemCommand(object source, RepeaterCommandEventArgs e)
        {
            if (e.CommandName == "MarkRead")
            {
                int notificationId = Convert.ToInt32(e.CommandArgument);
                NotificationHelper.MarkAsRead(notificationId);
                BindNotifications();
                lblStatus.Text = "Notification marked as read.";
                lblStatus.CssClass = "text-success";
            }
        }

        private void BindNotifications()
        {
            int userId = Convert.ToInt32(Session["UserID"]);
            string connectionString = ConfigurationManager.ConnectionStrings["ConnectionString"].ConnectionString;
            DataTable dt = new DataTable();

            // Check ViewState for filter
            if (ViewState["ShowUnreadOnly"] != null)
            {
                showUnreadOnly = (bool)ViewState["ShowUnreadOnly"];
            }

            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                string query = @"
                    SELECT NotificationID, UserID, NotificationType, Message, IsRead, CreatedDate
                    FROM UserNotifications
                    WHERE UserID = @UserID";

                if (showUnreadOnly)
                {
                    query += " AND IsRead = 0";
                }

                query += " ORDER BY CreatedDate DESC";

                using (SqlCommand cmd = new SqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@UserID", userId);

                    try
                    {
                        conn.Open();
                        SqlDataAdapter da = new SqlDataAdapter(cmd);
                        da.Fill(dt);
                    }
                    catch (Exception ex)
                    {
                        lblStatus.Text = "Error loading notifications.";
                        lblStatus.CssClass = "text-danger";
                        System.Diagnostics.Trace.WriteLine("BindNotifications Error: " + ex.ToString());
                    }
                }
            }

            if (dt.Rows.Count > 0)
            {
                rptNotifications.DataSource = dt;
                rptNotifications.DataBind();
                rptNotifications.Visible = true;
                phNoNotifications.Visible = false;
            }
            else
            {
                rptNotifications.Visible = false;
                phNoNotifications.Visible = true;
            }
        }
    }
}