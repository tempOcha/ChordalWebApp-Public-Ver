using System;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Web.UI;

namespace ChordalWebApp
{
    public partial class ShareProgression : System.Web.UI.Page
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
                // Get progression ID from query string
                if (Request.QueryString["id"] != null)
                {
                    int progressionId;
                    if (int.TryParse(Request.QueryString["id"], out progressionId))
                    {
                        hdnProgressionID.Value = progressionId.ToString();
                        LoadProgressionDetails(progressionId);
                    }
                    else
                    {
                        ShowError("Invalid progression ID");
                        btnShare.Enabled = false;
                    }
                }
                else
                {
                    ShowError("No progression specified");
                    btnShare.Enabled = false;
                }
            }
        }

        private void LoadProgressionDetails(int progressionId)
        {
            int userId = Convert.ToInt32(Session["UserID"]);
            string connectionString = ConfigurationManager.ConnectionStrings["ConnectionString"].ConnectionString;

            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                try
                {
                    conn.Open();

                    // Load progression details
                    string query = @"
                        SELECT 
                            p.ProgressionTitle,
                            p.KeyRoot,
                            p.IsKeyMajor,
                            p.UploadDate,
                            (SELECT COUNT(*) FROM ProgressionChordEvents WHERE ProgressionID = p.ProgressionID) AS ChordCount
                        FROM Progressions p
                        WHERE p.ProgressionID = @ProgressionID AND p.UserID = @UserID";

                    using (SqlCommand cmd = new SqlCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@ProgressionID", progressionId);
                        cmd.Parameters.AddWithValue("@UserID", userId);

                        using (SqlDataReader reader = cmd.ExecuteReader())
                        {
                            if (reader.Read())
                            {
                                // Display progression details
                                string title = reader["ProgressionTitle"] == DBNull.Value ?
                                             "Untitled Progression" : reader["ProgressionTitle"].ToString();
                                litProgressionTitle.Text = Server.HtmlEncode(title);

                                // Set default share title to progression title
                                if (string.IsNullOrEmpty(txtShareTitle.Text))
                                {
                                    txtShareTitle.Text = title;
                                }

                                // Display key
                                int keyRoot = Convert.ToInt32(reader["KeyRoot"]);
                                bool isKeyMajor = Convert.ToBoolean(reader["IsKeyMajor"]);
                                litKey.Text = GetKeyName(keyRoot, isKeyMajor);

                                // Display chord count
                                litChordCount.Text = reader["ChordCount"].ToString();

                                // Display upload date
                                DateTime uploadDate = Convert.ToDateTime(reader["UploadDate"]);
                                litUploadDate.Text = uploadDate.ToString("MMMM dd, yyyy");
                            }
                            else
                            {
                                ShowError("Progression not found or you don't have permission to share it");
                                btnShare.Enabled = false;
                            }
                        }
                    }

                    // Check if already shared and pre-fill form
                    string checkSharedQuery = @"
                        SELECT ShareTitle, ShareDescription, Tags, Visibility, AllowDownload
                        FROM SharedProgressions
                        WHERE ProgressionID = @ProgressionID AND UserID = @UserID";

                    using (SqlCommand cmd = new SqlCommand(checkSharedQuery, conn))
                    {
                        cmd.Parameters.AddWithValue("@ProgressionID", progressionId);
                        cmd.Parameters.AddWithValue("@UserID", userId);

                        using (SqlDataReader reader = cmd.ExecuteReader())
                        {
                            if (reader.Read())
                            {
                                // Pre-fill with existing share data
                                txtShareTitle.Text = reader["ShareTitle"].ToString();
                                txtShareDescription.Text = reader["ShareDescription"] == DBNull.Value ?
                                                          "" : reader["ShareDescription"].ToString();
                                txtTags.Text = reader["Tags"] == DBNull.Value ?
                                              "" : reader["Tags"].ToString();

                                string visibility = reader["Visibility"].ToString();
                                rbPublic.Checked = (visibility == "Public");
                                rbRegisteredOnly.Checked = (visibility == "RegisteredOnly");

                                chkAllowDownload.Checked = Convert.ToBoolean(reader["AllowDownload"]);

                                ShowWarning("This progression is already shared. Your changes will update the existing share.");
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    ShowError("Error loading progression: " + ex.Message);
                    System.Diagnostics.Trace.WriteLine("LoadProgressionDetails Error: " + ex.ToString());
                    btnShare.Enabled = false;
                }
            }
        }

        protected void btnShare_Click(object sender, EventArgs e)
        {
            if (!Page.IsValid)
                return;

            int userId = Convert.ToInt32(Session["UserID"]);
            int progressionId = Convert.ToInt32(hdnProgressionID.Value);
            string connectionString = ConfigurationManager.ConnectionStrings["ConnectionString"].ConnectionString;

            try
            {
                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    conn.Open();

                    using (SqlCommand cmd = new SqlCommand("sp_ShareProgression", conn))
                    {
                        cmd.CommandType = CommandType.StoredProcedure;

                        cmd.Parameters.AddWithValue("@ProgressionID", progressionId);
                        cmd.Parameters.AddWithValue("@UserID", userId);
                        cmd.Parameters.AddWithValue("@ShareTitle", txtShareTitle.Text.Trim());
                        cmd.Parameters.AddWithValue("@ShareDescription",
                            string.IsNullOrWhiteSpace(txtShareDescription.Text) ?
                            (object)DBNull.Value : txtShareDescription.Text.Trim());
                        cmd.Parameters.AddWithValue("@Tags",
                            string.IsNullOrWhiteSpace(txtTags.Text) ?
                            (object)DBNull.Value : txtTags.Text.Trim());

                        string visibility = rbPublic.Checked ? "Public" : "RegisteredOnly";
                        cmd.Parameters.AddWithValue("@Visibility", visibility);
                        cmd.Parameters.AddWithValue("@AllowDownload", chkAllowDownload.Checked);

                        object result = cmd.ExecuteScalar();

                        if (result != null)
                        {
                            int sharedProgressionId = Convert.ToInt32(result);

                            // Log notification
                            NotificationHelper.LogNotification(
                                userId,
                                NotificationHelper.NotificationTypes.ProgressionShared,
                                "Your progression \"" + txtShareTitle.Text.Trim() + "\" has been shared with the community.",
                                conn
                            );

                            // Redirect to community browse or shared progression view
                            Response.Redirect("CommunityBrowse.aspx?shared=1");
                        }
                        else
                        {
                            ShowError("Failed to share progression. Please try again.");
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                ShowError("Error sharing progression: " + ex.Message);
                System.Diagnostics.Trace.WriteLine("ShareProgression Error: " + ex.ToString());
            }
        }

        protected void btnCancel_Click(object sender, EventArgs e)
        {
            // Return to progressions list
            Response.Redirect("MyProgressionsListEnhanced.aspx");
        }

        private string GetKeyName(int keyRoot, bool isKeyMajor)
        {
            string[] noteNames = { "C", "C#", "D", "D#", "E", "F", "F#", "G", "G#", "A", "A#", "B" };
            string noteName = noteNames[keyRoot % 12];
            string quality = isKeyMajor ? " Major" : " Minor";
            return noteName + quality;
        }

        private void ShowSuccess(string message)
        {
            pnlMessages.Visible = true;
            litMessage.Text = "<div class='alert alert-success'>" + Server.HtmlEncode(message) + "</div>";
        }

        private void ShowError(string message)
        {
            pnlMessages.Visible = true;
            litMessage.Text = "<div class='alert alert-error'>" + Server.HtmlEncode(message) + "</div>";
        }

        private void ShowWarning(string message)
        {
            pnlMessages.Visible = true;
            litMessage.Text = "<div class='alert alert-warning'>" + Server.HtmlEncode(message) + "</div>";
        }
    }
}