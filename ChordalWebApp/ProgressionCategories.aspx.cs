using System;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace ChordalWebApp
{
    public partial class ProgressionCategories : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            if (Session["UserID"] == null)
            {
                Response.Redirect("~/Login.aspx?ReturnUrl=" + Server.UrlEncode(Request.Url.PathAndQuery));
                return;
            }

            if (!IsPostBack)
            {
                BindCategories();
                // Initialize status message
                lblStatus.Text = "";
                lblStatus.CssClass = "";
            }
        }

        protected void btnSaveCategory_Click(object sender, EventArgs e)
        {
            if (!Page.IsValid)
            {
                lblStatus.Text = "⚠️ Please correct the errors on the page.";
                lblStatus.CssClass = "status-message error";
                return;
            }

            int userId = Convert.ToInt32(Session["UserID"]);
            string categoryName = txtCategoryName.Text.Trim();
            string description = txtCategoryDescription.Text.Trim();
            string color = hfSelectedColor.Value;
            string editCategoryId = hfEditCategoryId.Value;

            // Debug logging
            System.Diagnostics.Trace.WriteLine($"Saving category with color: {color}");

            string connectionString = ConfigurationManager.ConnectionStrings["ConnectionString"].ConnectionString;

            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                try
                {
                    conn.Open();

                    // Check if category name already exists for this user (except when editing)
                    string checkQuery = editCategoryId == "" ?
                        "SELECT COUNT(*) FROM ProgressionCategories WHERE UserID = @UserID AND CategoryName = @CategoryName" :
                        "SELECT COUNT(*) FROM ProgressionCategories WHERE UserID = @UserID AND CategoryName = @CategoryName AND CategoryID != @CategoryID";

                    using (SqlCommand checkCmd = new SqlCommand(checkQuery, conn))
                    {
                        checkCmd.Parameters.AddWithValue("@UserID", userId);
                        checkCmd.Parameters.AddWithValue("@CategoryName", categoryName);
                        if (editCategoryId != "")
                        {
                            checkCmd.Parameters.AddWithValue("@CategoryID", editCategoryId);
                        }

                        int count = (int)checkCmd.ExecuteScalar();
                        if (count > 0)
                        {
                            lblStatus.Text = "⚠️ A category with this name already exists.";
                            lblStatus.CssClass = "status-message error";
                            return;
                        }
                    }

                    if (editCategoryId == "")
                    {
                        // Insert new category
                        string insertQuery = @"
                            INSERT INTO ProgressionCategories (UserID, CategoryName, Description, Color, CreatedDate)
                            VALUES (@UserID, @CategoryName, @Description, @Color, GETDATE())";

                        using (SqlCommand cmd = new SqlCommand(insertQuery, conn))
                        {
                            cmd.Parameters.AddWithValue("@UserID", userId);
                            cmd.Parameters.AddWithValue("@CategoryName", categoryName);
                            cmd.Parameters.AddWithValue("@Description", string.IsNullOrEmpty(description) ? (object)DBNull.Value : description);
                            cmd.Parameters.AddWithValue("@Color", color);

                            cmd.ExecuteNonQuery();

                            lblStatus.Text = "✓ Category created successfully!";
                            lblStatus.CssClass = "status-message success";

                            // Log notification
                            NotificationHelper.LogNotification(userId, "System",
                                $"Created new category: {categoryName}", conn);
                        }
                    }
                    else
                    {
                        // Update existing category
                        string updateQuery = @"
                            UPDATE ProgressionCategories 
                            SET CategoryName = @CategoryName, 
                                Description = @Description, 
                                Color = @Color
                            WHERE CategoryID = @CategoryID AND UserID = @UserID";

                        using (SqlCommand cmd = new SqlCommand(updateQuery, conn))
                        {
                            cmd.Parameters.AddWithValue("@CategoryName", categoryName);
                            cmd.Parameters.AddWithValue("@Description", string.IsNullOrEmpty(description) ? (object)DBNull.Value : description);
                            cmd.Parameters.AddWithValue("@Color", color);
                            cmd.Parameters.AddWithValue("@CategoryID", editCategoryId);
                            cmd.Parameters.AddWithValue("@UserID", userId);

                            int rowsAffected = cmd.ExecuteNonQuery();

                            if (rowsAffected > 0)
                            {
                                lblStatus.Text = "✓ Category updated successfully!";
                                lblStatus.CssClass = "status-message success";

                                // Log notification
                                NotificationHelper.LogNotification(userId, "System",
                                    $"Updated category: {categoryName}", conn);
                            }
                            else
                            {
                                lblStatus.Text = "❌ Category not found or you don't have permission to edit it.";
                                lblStatus.CssClass = "status-message error";
                            }
                        }
                    }

                    // Clear form
                    ClearForm();
                    BindCategories();
                }
                catch (Exception ex)
                {
                    lblStatus.Text = "❌ Error saving category: " + ex.Message;
                    lblStatus.CssClass = "status-message error";
                    System.Diagnostics.Trace.WriteLine("SaveCategory Error: " + ex.ToString());
                }
            }
        }

        protected void btnCancelEdit_Click(object sender, EventArgs e)
        {
            ClearForm();
            lblStatus.Text = "";
            lblStatus.CssClass = "";
        }

        protected void rptCategories_ItemCommand(object source, RepeaterCommandEventArgs e)
        {
            int categoryId = Convert.ToInt32(e.CommandArgument);
            int userId = Convert.ToInt32(Session["UserID"]);

            if (e.CommandName == "EditCategory")
            {
                LoadCategoryForEdit(categoryId, userId);
            }
            else if (e.CommandName == "DeleteCategory")
            {
                DeleteCategory(categoryId, userId);
            }
        }

        private void LoadCategoryForEdit(int categoryId, int userId)
        {
            string connectionString = ConfigurationManager.ConnectionStrings["ConnectionString"].ConnectionString;

            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                try
                {
                    conn.Open();
                    string query = @"
                        SELECT CategoryName, Description, Color 
                        FROM ProgressionCategories 
                        WHERE CategoryID = @CategoryID AND UserID = @UserID";

                    using (SqlCommand cmd = new SqlCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@CategoryID", categoryId);
                        cmd.Parameters.AddWithValue("@UserID", userId);

                        using (SqlDataReader reader = cmd.ExecuteReader())
                        {
                            if (reader.Read())
                            {
                                hfEditCategoryId.Value = categoryId.ToString();
                                txtCategoryName.Text = reader["CategoryName"].ToString();
                                txtCategoryDescription.Text = reader["Description"] != DBNull.Value ? reader["Description"].ToString() : "";

                                string selectedColor = reader["Color"].ToString();
                                hfSelectedColor.Value = selectedColor;

                                // Debug logging
                                System.Diagnostics.Trace.WriteLine($"Loading category for edit with color: {selectedColor}");

                                litFormTitle.Text = "Edit Category";
                                btnSaveCategory.Text = "Update Category";
                                btnCancelEdit.Visible = true;

                                lblStatus.Text = "ℹ️ Editing category. Make your changes and click Update.";
                                lblStatus.CssClass = "status-message info";

                                // Trigger client-side color selection after page loads
                                ScriptManager.RegisterStartupScript(this, GetType(), "selectEditColor",
                                    $"setTimeout(function() {{ if (window.selectColorGlobal) window.selectColorGlobal('{selectedColor}'); }}, 300);",
                                    true);
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    lblStatus.Text = "❌ Error loading category for edit: " + ex.Message;
                    lblStatus.CssClass = "status-message error";
                    System.Diagnostics.Trace.WriteLine("LoadCategoryForEdit Error: " + ex.ToString());
                }
            }
        }

        private void DeleteCategory(int categoryId, int userId)
        {
            string connectionString = ConfigurationManager.ConnectionStrings["ConnectionString"].ConnectionString;

            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                try
                {
                    conn.Open();

                    // First, get the category name for notification
                    string categoryName = "";
                    string getNameQuery = "SELECT CategoryName FROM ProgressionCategories WHERE CategoryID = @CategoryID AND UserID = @UserID";
                    using (SqlCommand getNameCmd = new SqlCommand(getNameQuery, conn))
                    {
                        getNameCmd.Parameters.AddWithValue("@CategoryID", categoryId);
                        getNameCmd.Parameters.AddWithValue("@UserID", userId);
                        object result = getNameCmd.ExecuteScalar();
                        categoryName = result?.ToString() ?? "Unknown";
                    }

                    // Check if category exists and belongs to user
                    if (categoryName == "Unknown")
                    {
                        lblStatus.Text = "❌ Category not found or you don't have permission to delete it.";
                        lblStatus.CssClass = "status-message error";
                        return;
                    }

                    // Remove category associations from progressions (this will happen automatically with ON DELETE SET NULL)
                    // But we'll do it explicitly for clarity
                    string updateProgressionsQuery = @"
                        UPDATE Progressions 
                        SET CategoryID = NULL 
                        WHERE CategoryID = @CategoryID AND UserID = @UserID";

                    using (SqlCommand updateCmd = new SqlCommand(updateProgressionsQuery, conn))
                    {
                        updateCmd.Parameters.AddWithValue("@CategoryID", categoryId);
                        updateCmd.Parameters.AddWithValue("@UserID", userId);
                        updateCmd.ExecuteNonQuery();
                    }

                    // Delete the category
                    string deleteQuery = @"
                        DELETE FROM ProgressionCategories 
                        WHERE CategoryID = @CategoryID AND UserID = @UserID";

                    using (SqlCommand cmd = new SqlCommand(deleteQuery, conn))
                    {
                        cmd.Parameters.AddWithValue("@CategoryID", categoryId);
                        cmd.Parameters.AddWithValue("@UserID", userId);

                        int rowsAffected = cmd.ExecuteNonQuery();

                        if (rowsAffected > 0)
                        {
                            lblStatus.Text = $"✓ Category '{categoryName}' deleted successfully. Associated progressions have been uncategorized.";
                            lblStatus.CssClass = "status-message success";

                            // Log notification
                            NotificationHelper.LogNotification(userId, "System",
                                $"Deleted category: {categoryName}", conn);
                        }
                        else
                        {
                            lblStatus.Text = "❌ Failed to delete category.";
                            lblStatus.CssClass = "status-message error";
                        }
                    }

                    BindCategories();
                }
                catch (Exception ex)
                {
                    lblStatus.Text = "❌ Error deleting category: " + ex.Message;
                    lblStatus.CssClass = "status-message error";
                    System.Diagnostics.Trace.WriteLine("DeleteCategory Error: " + ex.ToString());
                }
            }
        }

        private void BindCategories()
        {
            int userId = Convert.ToInt32(Session["UserID"]);
            string connectionString = ConfigurationManager.ConnectionStrings["ConnectionString"].ConnectionString;
            DataTable dt = new DataTable();

            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                string query = @"
                    SELECT 
                        pc.CategoryID,
                        pc.CategoryName,
                        pc.Description,
                        pc.Color,
                        COUNT(p.ProgressionID) AS ProgressionCount
                    FROM ProgressionCategories pc
                    LEFT JOIN Progressions p ON pc.CategoryID = p.CategoryID
                    WHERE pc.UserID = @UserID
                    GROUP BY pc.CategoryID, pc.CategoryName, pc.Description, pc.Color
                    ORDER BY pc.CategoryName";

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
                        lblStatus.Text = "❌ Error loading categories: " + ex.Message;
                        lblStatus.CssClass = "status-message error";
                        System.Diagnostics.Trace.WriteLine("BindCategories Error: " + ex.ToString());
                    }
                }
            }

            if (dt.Rows.Count > 0)
            {
                rptCategories.DataSource = dt;
                rptCategories.DataBind();
                rptCategories.Visible = true;
                phNoCategories.Visible = false;
            }
            else
            {
                rptCategories.Visible = false;
                phNoCategories.Visible = true;
            }
        }

        private void ClearForm()
        {
            hfEditCategoryId.Value = "";
            txtCategoryName.Text = "";
            txtCategoryDescription.Text = "";
            hfSelectedColor.Value = "#177364"; // Changed from #77aaff to match your teal color
            litFormTitle.Text = "Create New Category";
            btnSaveCategory.Text = "Create Category";
            btnCancelEdit.Visible = false;

            // Trigger client-side color selection to update UI
            ScriptManager.RegisterStartupScript(this, GetType(), "resetColor",
                "setTimeout(function() { if (window.selectColorGlobal) window.selectColorGlobal('#177364'); }, 100);",
                true);
        }
    }
}
