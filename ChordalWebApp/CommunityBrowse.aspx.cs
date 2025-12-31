using System;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Text;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace ChordalWebApp
{
    public partial class CommunityBrowse : System.Web.UI.Page
    {
        private const int PageSize = 12;

        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack)
            {
                // Check if coming from successful share
                if (Request.QueryString["shared"] == "1")
                {
                    ShowSuccess("Your progression has been successfully shared with the community!");
                }

                LoadProgressions();
            }
        }

        protected void btnSearch_Click(object sender, EventArgs e)
        {
            LoadProgressions();
        }

        protected void ddlSort_SelectedIndexChanged(object sender, EventArgs e)
        {
            LoadProgressions();
        }

        protected void btnClearFilters_Click(object sender, EventArgs e)
        {
            txtSearch.Text = "";
            ddlGenre.SelectedIndex = 0;
            ddlKey.SelectedIndex = 0;
            LoadProgressions();
        }

        protected void rptProgressions_ItemCommand(object source, RepeaterCommandEventArgs e)
        {
            if (e.CommandName == "ViewDetails")
            {
                int sharedProgressionId = Convert.ToInt32(e.CommandArgument);
                Response.Redirect("CommunityProgressionView.aspx?id=" + sharedProgressionId);
            }
        }

        private void LoadProgressions()
        {
            int? currentUserId = Session["UserID"] != null ? (int?)Convert.ToInt32(Session["UserID"]) : null;
            int pageNumber = GetCurrentPage();
            string connectionString = ConfigurationManager.ConnectionStrings["ConnectionString"].ConnectionString;

            try
            {
                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    conn.Open();

                    using (SqlCommand cmd = new SqlCommand("sp_BrowseCommunityProgressions", conn))
                    {
                        cmd.CommandType = CommandType.StoredProcedure;

                        // Add parameters
                        cmd.Parameters.AddWithValue("@CurrentUserID",
                            currentUserId.HasValue ? (object)currentUserId.Value : DBNull.Value);

                        cmd.Parameters.AddWithValue("@SearchTerm",
                            string.IsNullOrWhiteSpace(txtSearch.Text) ? (object)DBNull.Value : txtSearch.Text.Trim());

                        cmd.Parameters.AddWithValue("@Genre",
                            string.IsNullOrEmpty(ddlGenre.SelectedValue) ? (object)DBNull.Value : ddlGenre.SelectedValue);

                        // Parse key filter
                        if (!string.IsNullOrEmpty(ddlKey.SelectedValue))
                        {
                            string[] keyParts = ddlKey.SelectedValue.Split(',');
                            if (keyParts.Length == 2)
                            {
                                cmd.Parameters.AddWithValue("@KeyRoot", int.Parse(keyParts[0]));
                                cmd.Parameters.AddWithValue("@IsKeyMajor", keyParts[1] == "1");
                            }
                            else
                            {
                                cmd.Parameters.AddWithValue("@KeyRoot", DBNull.Value);
                                cmd.Parameters.AddWithValue("@IsKeyMajor", DBNull.Value);
                            }
                        }
                        else
                        {
                            cmd.Parameters.AddWithValue("@KeyRoot", DBNull.Value);
                            cmd.Parameters.AddWithValue("@IsKeyMajor", DBNull.Value);
                        }

                        cmd.Parameters.AddWithValue("@SortBy", ddlSort.SelectedValue);
                        cmd.Parameters.AddWithValue("@PageNumber", pageNumber);
                        cmd.Parameters.AddWithValue("@PageSize", PageSize);

                        using (SqlDataAdapter adapter = new SqlDataAdapter(cmd))
                        {
                            DataTable dt = new DataTable();
                            adapter.Fill(dt);

                            if (dt.Rows.Count > 0)
                            {
                                rptProgressions.DataSource = dt;
                                rptProgressions.DataBind();

                                pnlProgressions.Visible = true;
                                pnlNoResults.Visible = false;

                                // Update results count
                                int totalCount = Convert.ToInt32(dt.Rows[0]["TotalCount"]);
                                int startIndex = ((pageNumber - 1) * PageSize) + 1;
                                int endIndex = Math.Min(pageNumber * PageSize, totalCount);
                                litResultsCount.Text = $"<strong>{totalCount}</strong> progressions found";

                                // Build pagination
                                BuildPagination(totalCount, pageNumber);
                            }
                            else
                            {
                                pnlProgressions.Visible = false;
                                pnlNoResults.Visible = true;
                                pnlPagination.Visible = false;
                                litResultsCount.Text = "0 progressions found";
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                ShowError("Error loading progressions: " + ex.Message);
                System.Diagnostics.Trace.WriteLine("LoadProgressions Error: " + ex.ToString());
                pnlProgressions.Visible = false;
                pnlNoResults.Visible = true;
            }
        }

        private int GetCurrentPage()
        {
            int page = 1;
            if (Request.QueryString["page"] != null)
            {
                int.TryParse(Request.QueryString["page"], out page);
                if (page < 1) page = 1;
            }
            return page;
        }

        private void BuildPagination(int totalCount, int currentPage)
        {
            int totalPages = (int)Math.Ceiling((double)totalCount / PageSize);

            if (totalPages <= 1)
            {
                pnlPagination.Visible = false;
                return;
            }

            pnlPagination.Visible = true;
            StringBuilder sb = new StringBuilder();

            // Build query string for filters
            string queryString = BuildQueryString(false);

            // Previous button
            if (currentPage > 1)
            {
                sb.AppendFormat("<a href='?page={0}{1}'>« Previous</a>", currentPage - 1, queryString);
            }
            else
            {
                sb.Append("<span style='color: #ccc;'>« Previous</span>");
            }

            // Page numbers
            int startPage = Math.Max(1, currentPage - 2);
            int endPage = Math.Min(totalPages, currentPage + 2);

            if (startPage > 1)
            {
                sb.AppendFormat("<a href='?page=1{0}'>1</a>", queryString);
                if (startPage > 2)
                {
                    sb.Append("<span>...</span>");
                }
            }

            for (int i = startPage; i <= endPage; i++)
            {
                if (i == currentPage)
                {
                    sb.AppendFormat("<span class='active'>{0}</span>", i);
                }
                else
                {
                    sb.AppendFormat("<a href='?page={0}{1}'>{0}</a>", i, queryString);
                }
            }

            if (endPage < totalPages)
            {
                if (endPage < totalPages - 1)
                {
                    sb.Append("<span>...</span>");
                }
                sb.AppendFormat("<a href='?page={0}{1}'>{0}</a>", totalPages, queryString);
            }

            // Next button
            if (currentPage < totalPages)
            {
                sb.AppendFormat("<a href='?page={0}{1}'>Next »</a>", currentPage + 1, queryString);
            }
            else
            {
                sb.Append("<span style='color: #ccc;'>Next »</span>");
            }

            litPagination.Text = sb.ToString();
        }

        private string BuildQueryString(bool includePage)
        {
            StringBuilder sb = new StringBuilder();

            if (!string.IsNullOrWhiteSpace(txtSearch.Text))
            {
                sb.AppendFormat("&search={0}", Server.UrlEncode(txtSearch.Text.Trim()));
            }

            if (!string.IsNullOrEmpty(ddlGenre.SelectedValue))
            {
                sb.AppendFormat("&genre={0}", Server.UrlEncode(ddlGenre.SelectedValue));
            }

            if (!string.IsNullOrEmpty(ddlKey.SelectedValue))
            {
                sb.AppendFormat("&key={0}", Server.UrlEncode(ddlKey.SelectedValue));
            }

            if (ddlSort.SelectedValue != "Recent")
            {
                sb.AppendFormat("&sort={0}", Server.UrlEncode(ddlSort.SelectedValue));
            }

            return sb.ToString();
        }

        // Helper methods for rendering
        protected string GetGenreClass(object tags)
        {
            if (tags == DBNull.Value || string.IsNullOrWhiteSpace(tags.ToString()))
                return "";

            // Extract first tag and convert to lowercase for CSS class
            string firstTag = tags.ToString().Split(',')[0].Trim().ToLower();
            return firstTag;
        }

        protected string GetFormattedDate(object shareDate)
        {
            if (shareDate == DBNull.Value)
                return "Unknown date";

            DateTime date = Convert.ToDateTime(shareDate);
            TimeSpan timeSince = DateTime.Now - date;

            if (timeSince.TotalDays < 1)
            {
                if (timeSince.TotalHours < 1)
                {
                    return $"{(int)timeSince.TotalMinutes} minutes ago";
                }
                return $"{(int)timeSince.TotalHours} hours ago";
            }
            else if (timeSince.TotalDays < 7)
            {
                return $"{(int)timeSince.TotalDays} days ago";
            }
            else if (timeSince.TotalDays < 30)
            {
                return $"{(int)(timeSince.TotalDays / 7)} weeks ago";
            }
            else if (timeSince.TotalDays < 365)
            {
                return $"{(int)(timeSince.TotalDays / 30)} months ago";
            }
            else
            {
                return date.ToString("MMM d, yyyy");
            }
        }

        protected string GetKeyName(object keyRoot, object isKeyMajor)
        {
            if (keyRoot == DBNull.Value || isKeyMajor == DBNull.Value)
                return "Unknown";

            string[] noteNames = { "C", "C#", "D", "D#", "E", "F", "F#", "G", "G#", "A", "A#", "B" };
            int root = Convert.ToInt32(keyRoot) % 12;
            bool major = Convert.ToBoolean(isKeyMajor);
            return noteNames[root] + (major ? " Major" : " Minor");
        }

        protected string RenderStars(object averageRating)
        {
            if (averageRating == DBNull.Value)
                return "☆☆☆☆☆";

            double rating = Convert.ToDouble(averageRating);
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

        protected string RenderTags(string tags)
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

        private void ShowSuccess(string message)
        {
            pnlMessages.Visible = true;
            litMessage.Text = "<div class='status-message status-success'>" + Server.HtmlEncode(message) + "</div>";
        }

        private void ShowError(string message)
        {
            pnlMessages.Visible = true;
            litMessage.Text = "<div class='status-message status-error'>" + Server.HtmlEncode(message) + "</div>";
        }
    }
}
