using System;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace ChordalWebApp
{
    public partial class MyProgressionsListEnhanced : System.Web.UI.Page
    {
        private static readonly string[] NoteNames = { "C", "C#", "D", "D#", "E", "F", "F#", "G", "G#", "A", "A#", "B" };

        protected void Page_Load(object sender, EventArgs e)
        {
            if (Session["UserID"] == null)
            {
                Response.Redirect("~/Login.aspx?ReturnUrl=" + Server.UrlEncode(Request.Url.PathAndQuery));
                return;
            }

            if (!IsPostBack)
            {
                LoadCategoryFilterDropdown();
                BindProgressions();
            }
        }

        protected void btnSearch_Click(object sender, EventArgs e)
        {
            ViewState["SearchTerm"] = txtSearch.Text.Trim();
            BindProgressions();
        }

        protected void btnClearSearch_Click(object sender, EventArgs e)
        {
            txtSearch.Text = "";
            ddlCategoryFilter.SelectedIndex = 0;
            ViewState["SearchTerm"] = null;
            pnlResultsInfo.Visible = false;
            BindProgressions();
        }

        protected void ddlCategoryFilter_SelectedIndexChanged(object sender, EventArgs e)
        {
            BindProgressions();
        }

        protected void rptProgressions_ItemCommand(object source, RepeaterCommandEventArgs e)
        {
            if (e.CommandName == "ViewDetails")
            {
                int progressionId = Convert.ToInt32(e.CommandArgument);
                Response.Redirect($"ProgressionDetailView.aspx?ProgID={progressionId}");
            }
            else if (e.CommandName == "Categorize")
            {
                int progressionId = Convert.ToInt32(e.CommandArgument);
                ShowCategoryModal(progressionId);
            }
            else if (e.CommandName == "Share")
            {
                int progressionId = Convert.ToInt32(e.CommandArgument);
                Response.Redirect("ShareProgression.aspx?id=" + progressionId);
            }
        }

        protected void btnAssignCategory_Click(object sender, EventArgs e)
        {
            int progressionId = Convert.ToInt32(hfSelectedProgressionId.Value);
            string categoryId = ddlCategories.SelectedValue;
            int userId = Convert.ToInt32(Session["UserID"]);

            string connectionString = ConfigurationManager.ConnectionStrings["ConnectionString"].ConnectionString;

            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                try
                {
                    conn.Open();

                    string updateQuery = @"
                        UPDATE Progressions 
                        SET CategoryID = @CategoryID 
                        WHERE ProgressionID = @ProgressionID AND UserID = @UserID";

                    using (SqlCommand cmd = new SqlCommand(updateQuery, conn))
                    {
                        cmd.Parameters.AddWithValue("@CategoryID",
                            string.IsNullOrEmpty(categoryId) ? (object)DBNull.Value : categoryId);
                        cmd.Parameters.AddWithValue("@ProgressionID", progressionId);
                        cmd.Parameters.AddWithValue("@UserID", userId);

                        cmd.ExecuteNonQuery();

                        // Get category name for notification
                        string categoryName = "None";
                        if (!string.IsNullOrEmpty(categoryId))
                        {
                            string getCatQuery = "SELECT CategoryName FROM ProgressionCategories WHERE CategoryID = @CategoryID";
                            using (SqlCommand catCmd = new SqlCommand(getCatQuery, conn))
                            {
                                catCmd.Parameters.AddWithValue("@CategoryID", categoryId);
                                categoryName = catCmd.ExecuteScalar()?.ToString() ?? "Unknown";
                            }
                        }

                        // Log notification
                        NotificationHelper.LogNotification(userId, "System",
                            $"Assigned progression to category: {categoryName}", conn);
                    }

                    pnlCategoryModal.Visible = false;
                    LoadCategoryFilterDropdown();
                    BindProgressions();
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Trace.WriteLine("AssignCategory Error: " + ex.ToString());
                }
            }
        }

        protected void btnCancelCategory_Click(object sender, EventArgs e)
        {
            pnlCategoryModal.Visible = false;
        }

        private void ShowCategoryModal(int progressionId)
        {
            hfSelectedProgressionId.Value = progressionId.ToString();
            LoadCategoriesDropdown(progressionId);
            pnlCategoryModal.Visible = true;
        }

        private void LoadCategoriesDropdown(int progressionId)
        {
            int userId = Convert.ToInt32(Session["UserID"]);
            string connectionString = ConfigurationManager.ConnectionStrings["ConnectionString"].ConnectionString;

            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                try
                {
                    conn.Open();

                    // Get current category of progression
                    string currentCategoryId = null;
                    string getCurrentQuery = "SELECT CategoryID FROM Progressions WHERE ProgressionID = @ProgressionID";
                    using (SqlCommand cmd = new SqlCommand(getCurrentQuery, conn))
                    {
                        cmd.Parameters.AddWithValue("@ProgressionID", progressionId);
                        var result = cmd.ExecuteScalar();
                        if (result != null && result != DBNull.Value)
                        {
                            currentCategoryId = result.ToString();
                        }
                    }

                    // Load categories
                    string query = "SELECT CategoryID, CategoryName FROM ProgressionCategories WHERE UserID = @UserID ORDER BY CategoryName";
                    using (SqlCommand cmd = new SqlCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@UserID", userId);

                        ddlCategories.Items.Clear();
                        ddlCategories.Items.Add(new ListItem("-- No Category --", ""));

                        using (SqlDataReader reader = cmd.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                string catId = reader["CategoryID"].ToString();
                                string catName = reader["CategoryName"].ToString();
                                ListItem item = new ListItem(catName, catId);
                                ddlCategories.Items.Add(item);

                                if (currentCategoryId == catId)
                                {
                                    item.Selected = true;
                                }
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Trace.WriteLine("LoadCategoriesDropdown Error: " + ex.ToString());
                }
            }
        }

        private void LoadCategoryFilterDropdown()
        {
            int userId = Convert.ToInt32(Session["UserID"]);
            string connectionString = ConfigurationManager.ConnectionStrings["ConnectionString"].ConnectionString;

            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                try
                {
                    conn.Open();

                    string query = @"
                        SELECT 
                            pc.CategoryID, 
                            pc.CategoryName, 
                            pc.Color,
                            COUNT(p.ProgressionID) AS Count
                        FROM ProgressionCategories pc
                        LEFT JOIN Progressions p ON pc.CategoryID = p.CategoryID AND p.UserID = @UserID
                        WHERE pc.UserID = @UserID
                        GROUP BY pc.CategoryID, pc.CategoryName, pc.Color
                        ORDER BY pc.CategoryName";

                    using (SqlCommand cmd = new SqlCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@UserID", userId);

                        ddlCategoryFilter.Items.Clear();
                        ddlCategoryFilter.Items.Add(new ListItem("All Categories", ""));

                        using (SqlDataReader reader = cmd.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                string catId = reader["CategoryID"].ToString();
                                string catName = reader["CategoryName"].ToString();
                                int count = Convert.ToInt32(reader["Count"]);
                                ddlCategoryFilter.Items.Add(new ListItem($"{catName} ({count})", catId));
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Trace.WriteLine("LoadCategoryFilterDropdown Error: " + ex.ToString());
                }
            }
        }

        private void BindProgressions()
        {
            int userId = Convert.ToInt32(Session["UserID"]);
            string searchTerm = ViewState["SearchTerm"]?.ToString();
            string categoryFilter = ddlCategoryFilter.SelectedValue;

            string connectionString = ConfigurationManager.ConnectionStrings["ConnectionString"].ConnectionString;
            DataTable dt = new DataTable();

            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                string query = @"
                    SELECT 
                        p.ProgressionID, 
                        ISNULL(p.ProgressionTitle, 'Untitled Progression') AS ProgressionTitle, 
                        p.KeyRoot, 
                        p.IsKeyMajor, 
                        p.UploadDate,
                        pc.CategoryName,
                        pc.Color AS CategoryColor,
                        (SELECT COUNT(*) FROM ProgressionChordEvents pce WHERE pce.ProgressionID = p.ProgressionID) AS ChordEventCount
                    FROM Progressions p
                    LEFT JOIN ProgressionCategories pc ON p.CategoryID = pc.CategoryID
                    WHERE p.UserID = @UserID";

                // Add search filter
                if (!string.IsNullOrEmpty(searchTerm))
                {
                    query += @" AND (
                        p.ProgressionTitle LIKE @SearchTerm 
                        OR EXISTS (
                            SELECT 1 FROM ProgressionChordEvents pce 
                            WHERE pce.ProgressionID = p.ProgressionID 
                            AND (pce.ChordName LIKE @SearchTerm OR pce.RomanNumeral LIKE @SearchTerm)
                        )
                    )";
                }

                // Add category filter
                if (!string.IsNullOrEmpty(categoryFilter))
                {
                    query += " AND p.CategoryID = @CategoryID";
                }

                query += " ORDER BY p.UploadDate DESC";

                using (SqlCommand cmd = new SqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@UserID", userId);

                    if (!string.IsNullOrEmpty(searchTerm))
                    {
                        cmd.Parameters.AddWithValue("@SearchTerm", "%" + searchTerm + "%");
                    }

                    if (!string.IsNullOrEmpty(categoryFilter))
                    {
                        cmd.Parameters.AddWithValue("@CategoryID", categoryFilter);
                    }

                    try
                    {
                        conn.Open();
                        SqlDataAdapter da = new SqlDataAdapter(cmd);
                        da.Fill(dt);
                    }
                    catch (Exception ex)
                    {
                        System.Diagnostics.Trace.WriteLine("BindProgressions Error: " + ex.ToString());
                    }
                }
            }

            // Add computed column for key signature
            dt.Columns.Add("KeySignature", typeof(string));
            foreach (DataRow row in dt.Rows)
            {
                int keyRoot = Convert.ToInt32(row["KeyRoot"]);
                bool isMajor = Convert.ToBoolean(row["IsKeyMajor"]);
                row["KeySignature"] = GetKeySignatureString(keyRoot, isMajor);
            }

            // Update results count
            litResultsCount.Text = dt.Rows.Count > 0
                ? $"<strong>{dt.Rows.Count}</strong> progression{(dt.Rows.Count != 1 ? "s" : "")} found"
                : "No progressions found";

            // Update results info banner
            if (!string.IsNullOrEmpty(searchTerm) || !string.IsNullOrEmpty(categoryFilter))
            {
                pnlResultsInfo.Visible = true;
                if (!string.IsNullOrEmpty(searchTerm))
                {
                    litResultsInfo.Text = $"Found {dt.Rows.Count} progression(s) matching \"{Server.HtmlEncode(searchTerm)}\"";
                }
                else
                {
                    string categoryName = GetCategoryName(categoryFilter);
                    litResultsInfo.Text = $"Showing {dt.Rows.Count} progression(s) in category \"{Server.HtmlEncode(categoryName)}\"";
                }
            }
            else
            {
                pnlResultsInfo.Visible = false;
            }

            if (dt.Rows.Count > 0)
            {
                rptProgressions.DataSource = dt;
                rptProgressions.DataBind();
                rptProgressions.Visible = true;
                phNoProgressions.Visible = false;
            }
            else
            {
                rptProgressions.Visible = false;
                phNoProgressions.Visible = true;

                // Customize message based on context
                if (!string.IsNullOrEmpty(searchTerm))
                {
                    litNoProgressionsMessage.Text = $"No progressions found matching \"{Server.HtmlEncode(searchTerm)}\". Try a different search term.";
                }
                else if (!string.IsNullOrEmpty(categoryFilter))
                {
                    string categoryName = GetCategoryName(categoryFilter);
                    litNoProgressionsMessage.Text = $"No progressions in category \"{Server.HtmlEncode(categoryName)}\". Assign progressions to this category or select a different one.";
                }
                else
                {
                    litNoProgressionsMessage.Text = "You haven't uploaded any chord progressions yet.";
                }
            }
        }

        private string GetCategoryName(string categoryId)
        {
            if (string.IsNullOrEmpty(categoryId))
                return "All";

            string connectionString = ConfigurationManager.ConnectionStrings["ConnectionString"].ConnectionString;
            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                try
                {
                    conn.Open();
                    string query = "SELECT CategoryName FROM ProgressionCategories WHERE CategoryID = @CategoryID";
                    using (SqlCommand cmd = new SqlCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@CategoryID", categoryId);
                        return cmd.ExecuteScalar()?.ToString() ?? "Unknown";
                    }
                }
                catch
                {
                    return "Unknown";
                }
            }
        }

        private string GetKeySignatureString(int keyRoot, bool isMajor)
        {
            if (keyRoot >= 0 && keyRoot < NoteNames.Length)
            {
                return NoteNames[keyRoot] + (isMajor ? " Major" : " Minor");
            }
            return "Unknown";
        }
    }
}
