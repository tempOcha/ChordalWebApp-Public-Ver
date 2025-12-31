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
    public partial class ManageGuidelines : System.Web.UI.Page
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
                LoadGuidelines();
            }
        }

        protected void btnAddNew_Click(object sender, EventArgs e)
        {
            hfEditGuidelineID.Value = "";
            lblModalTitle.Text = "Add New Guideline Section";
            txtSectionTitle.Text = "";
            txtSectionContent.Text = "";
            txtSectionOrder.Text = GetNextOrderNumber().ToString();
            chkIsActive.Checked = true;
            txtChangeNote.Text = "";

            ScriptManager.RegisterStartupScript(this, GetType(), "ShowModal", "showModal();", true);
        }

        protected void rptGuidelinesAdmin_ItemCommand(object source, RepeaterCommandEventArgs e)
        {
            int guidelineId = Convert.ToInt32(e.CommandArgument);

            switch (e.CommandName)
            {
                case "Edit":
                    LoadGuidelineForEdit(guidelineId);
                    break;

                case "Toggle":
                    ToggleGuidelineStatus(guidelineId);
                    break;

                case "Delete":
                    DeleteGuideline(guidelineId);
                    break;
            }
        }

        protected void btnSaveGuideline_Click(object sender, EventArgs e)
        {
            if (!Page.IsValid)
                return;

            int adminId = Convert.ToInt32(Session["UserID"]);
            string connectionString = ConfigurationManager.ConnectionStrings["ConnectionString"].ConnectionString;

            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                try
                {
                    conn.Open();

                    if (string.IsNullOrEmpty(hfEditGuidelineID.Value))
                    {
                        // Add new
                        using (SqlCommand cmd = new SqlCommand("sp_AddGuidelineSection", conn))
                        {
                            cmd.CommandType = CommandType.StoredProcedure;
                            cmd.Parameters.AddWithValue("@SectionTitle", txtSectionTitle.Text.Trim());
                            cmd.Parameters.AddWithValue("@SectionContent", txtSectionContent.Text.Trim());
                            cmd.Parameters.AddWithValue("@SectionOrder", Convert.ToInt32(txtSectionOrder.Text));
                            cmd.Parameters.AddWithValue("@ModifiedByUserID", adminId);

                            cmd.ExecuteNonQuery();
                        }

                        ShowMessage("Guideline section added successfully!", "success");
                    }
                    else
                    {
                        // Update existing
                        int guidelineId = Convert.ToInt32(hfEditGuidelineID.Value);

                        using (SqlCommand cmd = new SqlCommand("sp_UpdateGuidelineSection", conn))
                        {
                            cmd.CommandType = CommandType.StoredProcedure;
                            cmd.Parameters.AddWithValue("@GuidelineID", guidelineId);
                            cmd.Parameters.AddWithValue("@SectionTitle", txtSectionTitle.Text.Trim());
                            cmd.Parameters.AddWithValue("@SectionContent", txtSectionContent.Text.Trim());
                            cmd.Parameters.AddWithValue("@SectionOrder", Convert.ToInt32(txtSectionOrder.Text));
                            cmd.Parameters.AddWithValue("@IsActive", chkIsActive.Checked);
                            cmd.Parameters.AddWithValue("@ModifiedByUserID", adminId);
                            cmd.Parameters.AddWithValue("@ChangeNote",
                                string.IsNullOrWhiteSpace(txtChangeNote.Text) ? (object)DBNull.Value : txtChangeNote.Text.Trim());

                            cmd.ExecuteNonQuery();
                        }

                        ShowMessage("Guideline section updated successfully!", "success");
                    }

                    LoadGuidelines();
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine("Save Guideline Error: " + ex.Message);
                    ShowMessage("Error saving guideline: " + ex.Message, "danger");
                    ScriptManager.RegisterStartupScript(this, GetType(), "ShowModal", "showModal();", true);
                }
            }
        }

        private void LoadGuidelines()
        {
            string connectionString = ConfigurationManager.ConnectionStrings["ConnectionString"].ConnectionString;

            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                try
                {
                    conn.Open();

                    using (SqlCommand cmd = new SqlCommand("sp_GetGuidelinesForAdmin", conn))
                    {
                        cmd.CommandType = CommandType.StoredProcedure;

                        using (SqlDataAdapter adapter = new SqlDataAdapter(cmd))
                        {
                            DataTable dt = new DataTable();
                            adapter.Fill(dt);

                            rptGuidelinesAdmin.DataSource = dt;
                            rptGuidelinesAdmin.DataBind();
                        }
                    }
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine("Load Guidelines Error: " + ex.Message);
                    ShowMessage("Error loading guidelines: " + ex.Message, "danger");
                }
            }
        }

        private void LoadGuidelineForEdit(int guidelineId)
        {
            string connectionString = ConfigurationManager.ConnectionStrings["ConnectionString"].ConnectionString;

            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                try
                {
                    conn.Open();

                    string sql = @"SELECT GuidelineID, SectionTitle, SectionContent, SectionOrder, IsActive 
                                  FROM CommunityGuidelines WHERE GuidelineID = @GuidelineID";

                    using (SqlCommand cmd = new SqlCommand(sql, conn))
                    {
                        cmd.Parameters.AddWithValue("@GuidelineID", guidelineId);

                        using (SqlDataReader reader = cmd.ExecuteReader())
                        {
                            if (reader.Read())
                            {
                                hfEditGuidelineID.Value = guidelineId.ToString();
                                lblModalTitle.Text = "Edit Guideline Section";
                                txtSectionTitle.Text = reader["SectionTitle"].ToString();
                                txtSectionContent.Text = reader["SectionContent"].ToString();
                                txtSectionOrder.Text = reader["SectionOrder"].ToString();
                                chkIsActive.Checked = Convert.ToBoolean(reader["IsActive"]);
                                txtChangeNote.Text = "";

                                ScriptManager.RegisterStartupScript(this, GetType(), "ShowModal", "showModal();", true);
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine("Load Guideline For Edit Error: " + ex.Message);
                    ShowMessage("Error loading guideline for edit: " + ex.Message, "danger");
                }
            }
        }

        private void ToggleGuidelineStatus(int guidelineId)
        {
            string connectionString = ConfigurationManager.ConnectionStrings["ConnectionString"].ConnectionString;

            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                try
                {
                    conn.Open();

                    string sql = "UPDATE CommunityGuidelines SET IsActive = ~IsActive WHERE GuidelineID = @GuidelineID";

                    using (SqlCommand cmd = new SqlCommand(sql, conn))
                    {
                        cmd.Parameters.AddWithValue("@GuidelineID", guidelineId);
                        cmd.ExecuteNonQuery();
                    }

                    ShowMessage("Guideline status toggled successfully!", "success");
                    LoadGuidelines();
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine("Toggle Guideline Error: " + ex.Message);
                    ShowMessage("Error toggling guideline status: " + ex.Message, "danger");
                }
            }
        }

        private void DeleteGuideline(int guidelineId)
        {
            string connectionString = ConfigurationManager.ConnectionStrings["ConnectionString"].ConnectionString;

            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                try
                {
                    conn.Open();

                    using (SqlCommand cmd = new SqlCommand("sp_DeleteGuidelineSection", conn))
                    {
                        cmd.CommandType = CommandType.StoredProcedure;
                        cmd.Parameters.AddWithValue("@GuidelineID", guidelineId);
                        cmd.ExecuteNonQuery();
                    }

                    ShowMessage("Guideline section deleted successfully!", "success");
                    LoadGuidelines();
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine("Delete Guideline Error: " + ex.Message);
                    ShowMessage("Error deleting guideline: " + ex.Message, "danger");
                }
            }
        }

        private int GetNextOrderNumber()
        {
            string connectionString = ConfigurationManager.ConnectionStrings["ConnectionString"].ConnectionString;

            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                try
                {
                    conn.Open();

                    string sql = "SELECT ISNULL(MAX(SectionOrder), 0) + 1 FROM CommunityGuidelines";

                    using (SqlCommand cmd = new SqlCommand(sql, conn))
                    {
                        return (int)cmd.ExecuteScalar();
                    }
                }
                catch
                {
                    return 1;
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