using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Text;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using Newtonsoft.Json;

namespace ChordalWebApp
{
    public partial class CommunityProgressionView : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack)
            {
                if (Request.QueryString["id"] != null)
                {
                    int sharedProgressionId;
                    if (int.TryParse(Request.QueryString["id"], out sharedProgressionId))
                    {
                        hdnSharedProgressionID.Value = sharedProgressionId.ToString();
                        LoadProgressionDetails(sharedProgressionId);
                        LoadComments(sharedProgressionId);
                    }
                    else
                    {
                        ShowErrorPanel();
                    }
                }
                else
                {
                    ShowErrorPanel();
                }
            }
        }

        private void LoadProgressionDetails(int sharedProgressionId)
        {
            int? userId = Session["UserID"] != null ? (int?)Convert.ToInt32(Session["UserID"]) : null;
            string connectionString = ConfigurationManager.ConnectionStrings["ConnectionString"].ConnectionString;

            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                try
                {
                    conn.Open();

                    using (SqlCommand cmd = new SqlCommand("sp_GetSharedProgressionDetails", conn))
                    {
                        cmd.CommandType = CommandType.StoredProcedure;
                        cmd.Parameters.AddWithValue("@SharedProgressionID", sharedProgressionId);
                        cmd.Parameters.AddWithValue("@ViewerUserID",
                            userId.HasValue ? (object)userId.Value : DBNull.Value);

                        using (SqlDataAdapter adapter = new SqlDataAdapter(cmd))
                        {
                            DataSet ds = new DataSet();
                            adapter.Fill(ds);

                            if (ds.Tables.Count > 0 && ds.Tables[0].Rows.Count > 0)
                            {
                                DataRow row = ds.Tables[0].Rows[0];

                                // Set page title
                                litPageTitle.Text = Server.HtmlEncode(row["ShareTitle"].ToString());
                                litProgressionTitle.Text = Server.HtmlEncode(row["ShareTitle"].ToString());

                                // Author info
                                string username = row["OwnerUsername"].ToString();
                                litAuthorName.Text = Server.HtmlEncode(username);
                                litAuthorInitial.Text = username.Substring(0, 1).ToUpper();

                                // Date
                                litShareDate.Text = Convert.ToDateTime(row["ShareDate"]).ToString("MMMM dd, yyyy");

                                // Key
                                int keyRoot = Convert.ToInt32(row["KeyRoot"]);
                                bool isKeyMajor = Convert.ToBoolean(row["IsKeyMajor"]);
                                litKey.Text = GetKeyName(keyRoot, isKeyMajor);

                                // Stats
                                litViewCount.Text = row["ViewCount"].ToString();
                                litCommentCount.Text = row["CommentCount"].ToString();
                                litCommentCount2.Text = row["CommentCount"].ToString();

                                // Tempo
                                if (row["Tempo"] != DBNull.Value)
                                {
                                    divTempo.Visible = true;
                                    litTempo.Text = row["Tempo"].ToString();
                                }

                                // Category
                                if (row["CategoryName"] != DBNull.Value && !string.IsNullOrEmpty(row["CategoryName"].ToString()))
                                {
                                    divCategory.Visible = true;
                                    litCategory.Text = Server.HtmlEncode(row["CategoryName"].ToString());
                                }

                                // Description
                                string description = row["ShareDescription"] == DBNull.Value ||
                                                   string.IsNullOrWhiteSpace(row["ShareDescription"].ToString())
                                    ? "No description provided."
                                    : row["ShareDescription"].ToString();
                                litDescription.Text = Server.HtmlEncode(description).Replace("\n", "<br/>");

                                // Tags
                                if (row["Tags"] != DBNull.Value && !string.IsNullOrWhiteSpace(row["Tags"].ToString()))
                                {
                                    pnlTags.Visible = true;
                                    litTags.Text = RenderTags(row["Tags"].ToString());
                                }

                                // Rating
                                if (row["AverageRating"] != DBNull.Value)
                                {
                                    double avgRating = Convert.ToDouble(row["AverageRating"]);
                                    litAverageRating.Text = avgRating.ToString("0.0");
                                    litStars.Text = RenderStars(avgRating);
                                }
                                litRatingCount.Text = row["RatingCount"].ToString();

                                // User rating capability
                                if (userId.HasValue)
                                {
                                    pnlUserRating.Visible = true;
                                    if (row["UserRating"] != DBNull.Value)
                                    {
                                        int userRating = Convert.ToInt32(row["UserRating"]);
                                        HighlightUserRating(userRating);
                                        pnlCurrentRating.Visible = true;
                                        litUserRating.Text = userRating.ToString();
                                    }

                                    // Check if user has liked this progression
                                    CheckUserLike(userId.Value, sharedProgressionId, conn);
                                }

                                // Like count
                                litLikeCount.Text = row["LikeCount"].ToString();

                                // Download button
                                if (Convert.ToBoolean(row["AllowDownload"]))
                                {
                                    btnDownload.Visible = true;
                                }

                                // Chord events - FIXED: Use actual database data with NotesCSV
                                if (ds.Tables.Count > 1 && ds.Tables[1].Rows.Count > 0)
                                {
                                    System.Diagnostics.Trace.WriteLine("=== CHORD EVENTS DEBUG ===");
                                    System.Diagnostics.Trace.WriteLine($"Number of chord events: {ds.Tables[1].Rows.Count}");

                                    // Bind chord cards for display
                                    rptChords.DataSource = ds.Tables[1];
                                    rptChords.DataBind();

                                    // Serialize chord data for MIDI visualization
                                    var chordsList = new List<object>();
                                    foreach (DataRow chordRow in ds.Tables[1].Rows)
                                    {
                                        // Get the actual MIDI notes from NotesCSV
                                        string notesCSV = chordRow["NotesCSV"]?.ToString() ?? "";
                                        List<int> midiNotes = new List<int>();

                                        System.Diagnostics.Trace.WriteLine($"Chord: {chordRow["ChordName"]}, NotesCSV: {notesCSV}");

                                        if (!string.IsNullOrEmpty(notesCSV))
                                        {
                                            string[] noteStrings = notesCSV.Split(',');
                                            foreach (string noteStr in noteStrings)
                                            {
                                                if (int.TryParse(noteStr.Trim(), out int midiNote))
                                                {
                                                    midiNotes.Add(midiNote);
                                                }
                                            }
                                        }

                                        System.Diagnostics.Trace.WriteLine($"  Parsed {midiNotes.Count} notes: [{string.Join(", ", midiNotes)}]");

                                        chordsList.Add(new
                                        {
                                            ChordName = chordRow["ChordName"]?.ToString() ?? "",
                                            RomanNumeral = chordRow["RomanNumeral"]?.ToString() ?? "",
                                            ChordFunction = chordRow["ChordFunction"]?.ToString() ?? "",
                                            StartTime = chordRow["StartTime"] != DBNull.Value
                                                ? Convert.ToDouble(chordRow["StartTime"])
                                                : 0.0,
                                            Duration = chordRow["Duration"] != DBNull.Value
                                                ? Convert.ToDouble(chordRow["Duration"])
                                                : 2.0,
                                            Notes = midiNotes, // Actual MIDI note numbers from database
                                            SequenceOrder = chordRow["SequenceOrder"] != DBNull.Value
                                                ? Convert.ToInt32(chordRow["SequenceOrder"])
                                                : 0
                                        });
                                    }

                                    // Store JSON in hidden field for JavaScript
                                    string json = JsonConvert.SerializeObject(chordsList);
                                    hdnProgressionJSON.Value = json;

                                    System.Diagnostics.Trace.WriteLine($"JSON Length: {json.Length} characters");
                                    System.Diagnostics.Trace.WriteLine($"JSON Preview: {json.Substring(0, Math.Min(200, json.Length))}...");
                                }

                                pnlProgressionContent.Visible = true;
                                pnlError.Visible = false;
                            }
                            else
                            {
                                ShowErrorPanel();
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Trace.WriteLine("LoadProgressionDetails Error: " + ex.ToString());
                    ShowErrorPanel();
                }
            }
        }

        private void LoadComments(int sharedProgressionId)
        {
            string connectionString = ConfigurationManager.ConnectionStrings["ConnectionString"].ConnectionString;

            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                try
                {
                    conn.Open();

                    // FIXED: Using correct table name ProgressionComments (not SharedProgressionComments)
                    string query = @"
                        WITH CommentTree AS (
                            SELECT 
                                c.CommentID,
                                c.SharedProgressionID,
                                c.UserID,
                                c.ParentCommentID,
                                c.CommentText,
                                c.CreatedDate,
                                u.Username,
                                0 AS Level
                            FROM ProgressionComments c
                            INNER JOIN Users u ON c.UserID = u.UserID
                            WHERE c.SharedProgressionID = @SharedProgressionID
                                AND c.IsDeleted = 0
                                AND (c.ParentCommentID IS NULL OR c.ParentCommentID = 0)
                            
                            UNION ALL
                            
                            SELECT 
                                c.CommentID,
                                c.SharedProgressionID,
                                c.UserID,
                                c.ParentCommentID,
                                c.CommentText,
                                c.CreatedDate,
                                u.Username,
                                ct.Level + 1
                            FROM ProgressionComments c
                            INNER JOIN Users u ON c.UserID = u.UserID
                            INNER JOIN CommentTree ct ON c.ParentCommentID = ct.CommentID
                            WHERE c.SharedProgressionID = @SharedProgressionID
                                AND c.IsDeleted = 0
                        )
                        SELECT * FROM CommentTree
                        ORDER BY 
                            CASE WHEN ParentCommentID IS NULL OR ParentCommentID = 0 
                                THEN CommentID ELSE ParentCommentID END,
                            Level,
                            CreatedDate";

                    using (SqlCommand cmd = new SqlCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@SharedProgressionID", sharedProgressionId);

                        using (SqlDataAdapter adapter = new SqlDataAdapter(cmd))
                        {
                            DataTable dt = new DataTable();
                            adapter.Fill(dt);

                            if (dt.Rows.Count > 0)
                            {
                                rptComments.DataSource = dt;
                                rptComments.DataBind();
                                pnlComments.Visible = true;
                                pnlNoComments.Visible = false;
                            }
                            else
                            {
                                pnlNoComments.Visible = true;
                                pnlComments.Visible = false;
                            }
                        }
                    }

                    // Show add comment panel if user is logged in
                    if (Session["UserID"] != null)
                    {
                        pnlAddComment.Visible = true;
                        pnlLoginToComment.Visible = false;
                    }
                    else
                    {
                        pnlAddComment.Visible = false;
                        pnlLoginToComment.Visible = true;
                    }
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Trace.WriteLine("LoadComments Error: " + ex.ToString());
                }
            }
        }

        private void CheckUserLike(int userId, int sharedProgressionId, SqlConnection conn)
        {
            // FIXED: Using correct table name ProgressionLikes (not SharedProgressionLikes)
            string query = @"
                SELECT COUNT(*) 
                FROM ProgressionLikes 
                WHERE SharedProgressionID = @SharedProgressionID 
                    AND UserID = @UserID";

            using (SqlCommand cmd = new SqlCommand(query, conn))
            {
                cmd.Parameters.AddWithValue("@SharedProgressionID", sharedProgressionId);
                cmd.Parameters.AddWithValue("@UserID", userId);

                int likeCount = Convert.ToInt32(cmd.ExecuteScalar());
                if (likeCount > 0)
                {
                    likeIcon.InnerHtml = "❤️"; // Filled heart
                    lnkLike.CssClass = "stat-item-modern stat-like liked";
                }
            }
        }

        protected void lnkLike_Click(object sender, EventArgs e)
        {
            if (Session["UserID"] == null)
            {
                Response.Redirect("Login.aspx?returnUrl=" + Server.UrlEncode(Request.RawUrl));
                return;
            }

            int userId = Convert.ToInt32(Session["UserID"]);
            int sharedProgressionId = Convert.ToInt32(hdnSharedProgressionID.Value);
            string connectionString = ConfigurationManager.ConnectionStrings["ConnectionString"].ConnectionString;

            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                try
                {
                    conn.Open();

                    // FIXED: Using correct table name ProgressionLikes
                    string checkQuery = @"
                        SELECT COUNT(*) 
                        FROM ProgressionLikes 
                        WHERE SharedProgressionID = @SharedProgressionID 
                            AND UserID = @UserID";

                    bool alreadyLiked = false;
                    using (SqlCommand cmd = new SqlCommand(checkQuery, conn))
                    {
                        cmd.Parameters.AddWithValue("@SharedProgressionID", sharedProgressionId);
                        cmd.Parameters.AddWithValue("@UserID", userId);
                        alreadyLiked = Convert.ToInt32(cmd.ExecuteScalar()) > 0;
                    }

                    if (alreadyLiked)
                    {
                        // Unlike
                        string deleteQuery = @"
                            DELETE FROM ProgressionLikes 
                            WHERE SharedProgressionID = @SharedProgressionID 
                                AND UserID = @UserID;
                            
                            UPDATE SharedProgressions 
                            SET LikeCount = LikeCount - 1 
                            WHERE SharedProgressionID = @SharedProgressionID;";

                        using (SqlCommand cmd = new SqlCommand(deleteQuery, conn))
                        {
                            cmd.Parameters.AddWithValue("@SharedProgressionID", sharedProgressionId);
                            cmd.Parameters.AddWithValue("@UserID", userId);
                            cmd.ExecuteNonQuery();
                        }
                    }
                    else
                    {
                        // Like
                        string insertQuery = @"
                            INSERT INTO ProgressionLikes (SharedProgressionID, UserID, CreatedDate)
                            VALUES (@SharedProgressionID, @UserID, GETDATE());
                            
                            UPDATE SharedProgressions 
                            SET LikeCount = LikeCount + 1 
                            WHERE SharedProgressionID = @SharedProgressionID;";

                        using (SqlCommand cmd = new SqlCommand(insertQuery, conn))
                        {
                            cmd.Parameters.AddWithValue("@SharedProgressionID", sharedProgressionId);
                            cmd.Parameters.AddWithValue("@UserID", userId);
                            cmd.ExecuteNonQuery();
                        }
                    }

                    // Reload the page to show updated like count
                    Response.Redirect(Request.RawUrl);
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Trace.WriteLine("Like Error: " + ex.ToString());
                }
            }
        }

        protected void RateStar_Click(object sender, EventArgs e)
        {
            if (Session["UserID"] == null)
            {
                Response.Redirect("Login.aspx?returnUrl=" + Server.UrlEncode(Request.RawUrl));
                return;
            }

            LinkButton btn = (LinkButton)sender;
            int rating = Convert.ToInt32(btn.CommandArgument);
            int userId = Convert.ToInt32(Session["UserID"]);
            int sharedProgressionId = Convert.ToInt32(hdnSharedProgressionID.Value);

            string connectionString = ConfigurationManager.ConnectionStrings["ConnectionString"].ConnectionString;

            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                try
                {
                    conn.Open();

                    // FIXED: Using correct table name ProgressionRatings
                    string query = @"
                        IF EXISTS (SELECT 1 FROM ProgressionRatings 
                                   WHERE SharedProgressionID = @SharedProgressionID 
                                   AND UserID = @UserID)
                        BEGIN
                            UPDATE ProgressionRatings
                            SET Rating = @Rating, CreatedDate = GETDATE()
                            WHERE SharedProgressionID = @SharedProgressionID 
                                AND UserID = @UserID
                        END
                        ELSE
                        BEGIN
                            INSERT INTO ProgressionRatings 
                                (SharedProgressionID, UserID, Rating, CreatedDate)
                            VALUES (@SharedProgressionID, @UserID, @Rating, GETDATE())
                        END";

                    using (SqlCommand cmd = new SqlCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@SharedProgressionID", sharedProgressionId);
                        cmd.Parameters.AddWithValue("@UserID", userId);
                        cmd.Parameters.AddWithValue("@Rating", rating);
                        cmd.ExecuteNonQuery();
                    }

                    // Reload the page to show updated rating
                    Response.Redirect(Request.RawUrl);
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Trace.WriteLine("Rating Error: " + ex.ToString());
                }
            }
        }

        protected void rptComments_ItemCommand(object source, RepeaterCommandEventArgs e)
        {
            if (e.CommandName == "Reply")
            {
                int commentId = Convert.ToInt32(e.CommandArgument);

                // Get the username of the comment being replied to
                string connectionString = ConfigurationManager.ConnectionStrings["ConnectionString"].ConnectionString;
                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    conn.Open();
                    // FIXED: Using correct table name
                    string query = @"
                        SELECT u.Username
                        FROM ProgressionComments c
                        INNER JOIN Users u ON c.UserID = u.UserID
                        WHERE c.CommentID = @CommentID";

                    using (SqlCommand cmd = new SqlCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@CommentID", commentId);
                        object result = cmd.ExecuteScalar();
                        if (result != null)
                        {
                            litReplyToUser.Text = Server.HtmlEncode(result.ToString());
                            hdnReplyToCommentID.Value = commentId.ToString();
                            pnlReplyingTo.Visible = true;
                            litCommentTitle.Text = "Reply to Comment";

                            // Scroll to comment form
                            ScriptManager.RegisterStartupScript(this, GetType(), "ScrollToComment",
                                "setTimeout(function() { document.getElementById('" + pnlAddComment.ClientID + "').scrollIntoView({behavior: 'smooth', block: 'center'}); }, 100);",
                                true);
                        }
                    }
                }
            }
        }

        protected void lnkCancelReply_Click(object sender, EventArgs e)
        {
            hdnReplyToCommentID.Value = "0";
            pnlReplyingTo.Visible = false;
            litCommentTitle.Text = "Add a Comment";
        }

        protected void btnPostComment_Click(object sender, EventArgs e)
        {
            if (Session["UserID"] == null)
            {
                Response.Redirect("Login.aspx?returnUrl=" + Server.UrlEncode(Request.RawUrl));
                return;
            }

            int userId = Convert.ToInt32(Session["UserID"]);
            int sharedProgressionId = Convert.ToInt32(hdnSharedProgressionID.Value);
            string commentText = txtComment.Text.Trim();
            int? parentCommentId = hdnReplyToCommentID.Value != "0" ? (int?)Convert.ToInt32(hdnReplyToCommentID.Value) : null;

            if (string.IsNullOrWhiteSpace(commentText))
                return;

            string connectionString = ConfigurationManager.ConnectionStrings["ConnectionString"].ConnectionString;

            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                try
                {
                    conn.Open();

                    // FIXED: Using correct table name
                    string query = @"
                        INSERT INTO ProgressionComments 
                            (SharedProgressionID, UserID, ParentCommentID, CommentText, CreatedDate)
                        VALUES 
                            (@SharedProgressionID, @UserID, @ParentCommentID, @CommentText, GETDATE())";

                    using (SqlCommand cmd = new SqlCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@SharedProgressionID", sharedProgressionId);
                        cmd.Parameters.AddWithValue("@UserID", userId);
                        cmd.Parameters.AddWithValue("@ParentCommentID",
                            parentCommentId.HasValue ? (object)parentCommentId.Value : DBNull.Value);
                        cmd.Parameters.AddWithValue("@CommentText", commentText);
                        cmd.ExecuteNonQuery();
                    }

                    // Clear form and reload
                    txtComment.Text = "";
                    hdnReplyToCommentID.Value = "0";
                    pnlReplyingTo.Visible = false;
                    litCommentTitle.Text = "Add a Comment";

                    // Reload comments
                    LoadComments(sharedProgressionId);
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Trace.WriteLine("PostComment Error: " + ex.ToString());
                }
            }
        }

        protected void lnkReportComment_Click(object sender, EventArgs e)
        {
            LinkButton btn = (LinkButton)sender;
            int commentId = Convert.ToInt32(btn.CommandArgument);
            Response.Redirect("ReportContent.aspx?type=comment&id=" + commentId);
        }

        protected void btnDownload_Click(object sender, EventArgs e)
        {
            int sharedProgressionId = Convert.ToInt32(hdnSharedProgressionID.Value);
            string connectionString = ConfigurationManager.ConnectionStrings["ConnectionString"].ConnectionString;

            try
            {
                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    conn.Open();

                    // Get progression details and chord events
                    string query = @"
                        SELECT 
                            sp.ShareTitle,
                            p.Tempo,
                            p.ProgressionID
                        FROM SharedProgressions sp
                        INNER JOIN Progressions p ON sp.ProgressionID = p.ProgressionID
                        WHERE sp.SharedProgressionID = @SharedProgressionID";

                    string progressionTitle = "";
                    int? tempo = null;
                    int progressionId = 0;

                    using (SqlCommand cmd = new SqlCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@SharedProgressionID", sharedProgressionId);
                        using (SqlDataReader reader = cmd.ExecuteReader())
                        {
                            if (reader.Read())
                            {
                                progressionTitle = reader["ShareTitle"].ToString();
                                tempo = reader["Tempo"] == DBNull.Value ? (int?)null : Convert.ToInt32(reader["Tempo"]);
                                progressionId = Convert.ToInt32(reader["ProgressionID"]);
                            }
                            else
                            {
                                return; // Progression not found
                            }
                        }
                    }

                    // Get chord events
                    string eventsQuery = @"
                        SELECT StartTime, Duration, ChordName, NotesCSV, SequenceOrder
                        FROM ProgressionChordEvents
                        WHERE ProgressionID = @ProgressionID
                        ORDER BY SequenceOrder";

                    List<ChordEventData> chordEvents = new List<ChordEventData>();

                    using (SqlCommand cmd = new SqlCommand(eventsQuery, conn))
                    {
                        cmd.Parameters.AddWithValue("@ProgressionID", progressionId);
                        using (SqlDataReader reader = cmd.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                chordEvents.Add(new ChordEventData
                                {
                                    StartTime = Convert.ToDouble(reader["StartTime"]),
                                    Duration = Convert.ToDouble(reader["Duration"]),
                                    ChordName = reader["ChordName"].ToString(),
                                    NotesCSV = reader["NotesCSV"].ToString(),
                                    SequenceOrder = Convert.ToInt32(reader["SequenceOrder"])
                                });
                            }
                        }
                    }

                    if (chordEvents.Count > 0)
                    {
                        // Generate MIDI file
                        byte[] midiData = MidiExportHelper.ConvertProgressionToMidi(
                            progressionTitle,
                            tempo,
                            chordEvents
                        );

                        // Send MIDI file to user
                        string sanitizedTitle = progressionTitle.Replace(" ", "_");
                        sanitizedTitle = System.Text.RegularExpressions.Regex.Replace(sanitizedTitle, @"[^a-zA-Z0-9_-]", "");
                        string filename = $"{sanitizedTitle}.mid";

                        Response.Clear();
                        Response.ContentType = "audio/midi";
                        Response.AddHeader("Content-Disposition", $"attachment; filename=\"{filename}\"");
                        Response.BinaryWrite(midiData);
                        Response.Flush();
                        Response.End();
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Trace.WriteLine("Download MIDI Error: " + ex.ToString());
                // Show error to user
                ClientScript.RegisterStartupScript(this.GetType(), "downloadError",
                    "alert('Error downloading MIDI file. Please try again.');", true);
            }
        }

        protected void lnkReport_Click(object sender, EventArgs e)
        {
            // Redirect to report content page
            int sharedProgressionId = Convert.ToInt32(hdnSharedProgressionID.Value);
            Response.Redirect("ReportContent.aspx?type=progression&id=" + sharedProgressionId);
        }

        private void HighlightUserRating(int rating)
        {
            lnkStar1.CssClass = rating >= 1 ? "star-btn selected" : "star-btn";
            lnkStar2.CssClass = rating >= 2 ? "star-btn selected" : "star-btn";
            lnkStar3.CssClass = rating >= 3 ? "star-btn selected" : "star-btn";
            lnkStar4.CssClass = rating >= 4 ? "star-btn selected" : "star-btn";
            lnkStar5.CssClass = rating >= 5 ? "star-btn selected" : "star-btn";
        }

        private string GetKeyName(int keyRoot, bool isKeyMajor)
        {
            string[] noteNames = { "C", "C#", "D", "D#", "E", "F", "F#", "G", "G#", "A", "A#", "B" };
            return noteNames[keyRoot % 12] + (isKeyMajor ? " Major" : " Minor");
        }

        private string RenderStars(double rating)
        {
            int fullStars = (int)Math.Floor(rating);
            bool hasHalfStar = (rating - fullStars) >= 0.5;

            StringBuilder stars = new StringBuilder();
            for (int i = 0; i < fullStars; i++)
            {
                stars.Append("★");
            }
            if (hasHalfStar)
            {
                stars.Append("⭐");
            }
            for (int i = fullStars + (hasHalfStar ? 1 : 0); i < 5; i++)
            {
                stars.Append("☆");
            }

            return stars.ToString();
        }

        private string RenderTags(string tags)
        {
            if (string.IsNullOrWhiteSpace(tags))
                return "";

            string[] tagArray = tags.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
            StringBuilder sb = new StringBuilder();

            foreach (string tag in tagArray)
            {
                string cleanTag = Server.HtmlEncode(tag.Trim());
                if (!string.IsNullOrEmpty(cleanTag))
                {
                    sb.AppendFormat("<span class='tag'>{0}</span>", cleanTag);
                }
            }

            return sb.ToString();
        }

        private void ShowErrorPanel()
        {
            pnlProgressionContent.Visible = false;
            pnlError.Visible = true;
        }
    }
}
