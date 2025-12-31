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
    public partial class ManagePermissions : System.Web.UI.Page
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
                LoadUsers();
            }
        }

        protected void btnSearch_Click(object sender, EventArgs e)
        {
            LoadUsers();
        }

        private void LoadUsers()
        {
            string connectionString = ConfigurationManager.ConnectionStrings["ConnectionString"].ConnectionString;
            string searchTerm = string.IsNullOrWhiteSpace(txtSearch.Text) ? null : txtSearch.Text.Trim();
            string roleFilter = ddlRoleFilter.SelectedValue;

            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                try
                {
                    conn.Open();

                    using (SqlCommand cmd = new SqlCommand("sp_GetAllUsersForPermissions", conn))
                    {
                        cmd.CommandType = CommandType.StoredProcedure;
                        cmd.Parameters.AddWithValue("@SearchTerm", (object)searchTerm ?? DBNull.Value);
                        cmd.Parameters.AddWithValue("@RoleFilter", roleFilter);

                        using (SqlDataAdapter adapter = new SqlDataAdapter(cmd))
                        {
                            DataTable dt = new DataTable();
                            adapter.Fill(dt);

                            if (dt.Rows.Count > 0)
                            {
                                gvUsers.DataSource = dt;
                                gvUsers.DataBind();
                                pnlEmptyState.Visible = false;
                            }
                            else
                            {
                                gvUsers.DataSource = null;
                                gvUsers.DataBind();
                                pnlEmptyState.Visible = true;
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine("Load Users Error: " + ex.Message);
                    ShowMessage("Error loading users: " + ex.Message, "danger");
                }
            }
        }

        protected void gvUsers_RowCommand(object sender, GridViewCommandEventArgs e)
        {
            if (e.CommandName == "EditUser")
            {
                int userId = Convert.ToInt32(e.CommandArgument);
                LoadUserDetails(userId);
                ScriptManager.RegisterStartupScript(this, GetType(), "ShowModal", "showModal();", true);
            }
        }

        private void LoadUserDetails(int userId)
        {
            string connectionString = ConfigurationManager.ConnectionStrings["ConnectionString"].ConnectionString;

            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                try
                {
                    conn.Open();

                    using (SqlCommand cmd = new SqlCommand("sp_GetUserDetailsForPermissions", conn))
                    {
                        cmd.CommandType = CommandType.StoredProcedure;
                        cmd.Parameters.AddWithValue("@UserID", userId);

                        using (SqlDataReader reader = cmd.ExecuteReader())
                        {
                            if (reader.Read())
                            {
                                hfEditUserID.Value = userId.ToString();
                                lblEditUsername.Text = reader["Username"].ToString();
                                lblEditEmail.Text = reader["Email"].ToString();
                                chkIsAdmin.Checked = Convert.ToBoolean(reader["IsAdmin"]);
                                chkIsEnabled.Checked = Convert.ToBoolean(reader["IsEnabled"]);
                                txtAdminNotes.Text = reader["AdminNotes"] != DBNull.Value ? reader["AdminNotes"].ToString() : "";

                                // Load statistics
                                lblStatProgressions.Text = reader["TotalProgressions"].ToString();
                                lblStatShared.Text = reader["SharedProgressions"].ToString();
                                lblStatComments.Text = reader["TotalComments"].ToString();
                                lblStatReports.Text = reader["ReportsReceived"].ToString();
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine("Load User Details Error: " + ex.Message);
                    ShowMessage("Error loading user details: " + ex.Message, "danger");
                }
            }
        }

        protected void btnSavePermissions_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(hfEditUserID.Value))
            {
                ShowMessage("Invalid user ID.", "danger");
                return;
            }

            int userId = Convert.ToInt32(hfEditUserID.Value);
            int adminId = Convert.ToInt32(Session["UserID"]);

            // Prevent admin from disabling their own account
            if (userId == adminId && !chkIsEnabled.Checked)
            {
                ShowMessage("You cannot disable your own account.", "danger");
                ScriptManager.RegisterStartupScript(this, GetType(), "ShowModal", "showModal();", true);
                return;
            }

            // Prevent admin from removing their own admin privileges if they're the only admin
            if (userId == adminId && !chkIsAdmin.Checked)
            {
                if (!IsAnotherAdminExists(userId))
                {
                    ShowMessage("You cannot remove your own admin privileges as you are the only administrator.", "danger");
                    ScriptManager.RegisterStartupScript(this, GetType(), "ShowModal", "showModal();", true);
                    return;
                }
            }

            string connectionString = ConfigurationManager.ConnectionStrings["ConnectionString"].ConnectionString;

            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                try
                {
                    conn.Open();

                    using (SqlCommand cmd = new SqlCommand("sp_UpdateUserPermissions", conn))
                    {
                        cmd.CommandType = CommandType.StoredProcedure;
                        cmd.Parameters.AddWithValue("@UserID", userId);
                        cmd.Parameters.AddWithValue("@IsAdmin", chkIsAdmin.Checked);
                        cmd.Parameters.AddWithValue("@IsEnabled", chkIsEnabled.Checked);
                        cmd.Parameters.AddWithValue("@AdminNotes", string.IsNullOrWhiteSpace(txtAdminNotes.Text) ? (object)DBNull.Value : txtAdminNotes.Text.Trim());
                        cmd.Parameters.AddWithValue("@ModifiedByAdminID", adminId);

                        cmd.ExecuteNonQuery();
                    }

                    ShowMessage("User permissions updated successfully!", "success");
                    LoadUsers();
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine("Save Permissions Error: " + ex.Message);
                    ShowMessage("Error saving permissions: " + ex.Message, "danger");
                    ScriptManager.RegisterStartupScript(this, GetType(), "ShowModal", "showModal();", true);
                }
            }
        }

        private bool IsAnotherAdminExists(int excludeUserId)
        {
            string connectionString = ConfigurationManager.ConnectionStrings["ConnectionString"].ConnectionString;

            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                try
                {
                    conn.Open();

                    string sql = "SELECT COUNT(*) FROM Users WHERE IsAdmin = 1 AND UserID != @ExcludeUserID";
                    using (SqlCommand cmd = new SqlCommand(sql, conn))
                    {
                        cmd.Parameters.AddWithValue("@ExcludeUserID", excludeUserId);
                        int count = (int)cmd.ExecuteScalar();
                        return count > 0;
                    }
                }
                catch
                {
                    return false;
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