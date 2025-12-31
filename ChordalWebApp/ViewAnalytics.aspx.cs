using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.SqlClient;
using System.Data;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Text;
using Newtonsoft.Json;

namespace ChordalWebApp
{
    public partial class ViewAnalytics : System.Web.UI.Page
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
                LoadAnalytics();
                LoadChartData();
            }
        }

        protected void btnApplyFilter_Click(object sender, EventArgs e)
        {
            LoadAnalytics();
            LoadChartData();
        }

        private void LoadAnalytics()
        {
            string timeRange = ddlTimeRange.SelectedValue;
            string connectionString = ConfigurationManager.ConnectionStrings["ConnectionString"].ConnectionString;

            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                try
                {
                    conn.Open();

                    // Load main analytics
                    LoadSystemAnalytics(conn, timeRange);

                    // Load top content and contributors
                    LoadTopContent(conn);
                    LoadTopContributors(conn);
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine("Load Analytics Error: " + ex.Message);
                    ShowMessage("Error loading analytics: " + ex.Message, "danger");
                }
            }
        }

        private void LoadSystemAnalytics(SqlConnection conn, string timeRange)
        {
            using (SqlCommand cmd = new SqlCommand("sp_GetSystemAnalytics", conn))
            {
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@TimeRange", timeRange);

                using (SqlDataReader reader = cmd.ExecuteReader())
                {
                    // User Statistics
                    if (reader.Read())
                    {
                        lblTotalUsers.Text = FormatNumber(reader["TotalUsers"]);
                        lblNewUsers.Text = FormatNumber(reader["NewUsers"]);
                        lblAdminUsers.Text = FormatNumber(reader["AdminUsers"]);
                        lblActiveUsers.Text = FormatNumber(reader["ActiveUsersInPeriod"]);
                    }

                    // Content Statistics
                    if (reader.NextResult() && reader.Read())
                    {
                        lblTotalProgressions.Text = FormatNumber(reader["TotalProgressions"]);
                        lblSharedProgressions.Text = FormatNumber(reader["TotalSharedProgressions"]);
                        lblPublishedProgressions.Text = FormatNumber(reader["PublishedProgressions"]);
                        lblUnderReview.Text = FormatNumber(reader["UnderReviewProgressions"]);
                    }

                    // Community Statistics
                    if (reader.NextResult() && reader.Read())
                    {
                        lblTotalViews.Text = FormatNumber(reader["TotalViews"]);
                        lblTotalLikes.Text = FormatNumber(reader["TotalLikes"]);
                        lblTotalComments.Text = FormatNumber(reader["TotalComments"]);
                        lblNewComments.Text = FormatNumber(reader["NewComments"]);
                    }
                }
            }
        }

        private void LoadTopContent(SqlConnection conn)
        {
            using (SqlCommand cmd = new SqlCommand("sp_GetContentEngagementAnalytics", conn))
            {
                cmd.CommandType = CommandType.StoredProcedure;

                using (SqlDataAdapter adapter = new SqlDataAdapter(cmd))
                {
                    DataSet ds = new DataSet();
                    adapter.Fill(ds);

                    if (ds.Tables.Count > 0 && ds.Tables[0].Rows.Count > 0)
                    {
                        gvTopContent.DataSource = ds.Tables[0];
                        gvTopContent.DataBind();
                    }
                    else
                    {
                        gvTopContent.DataSource = null;
                        gvTopContent.DataBind();
                    }
                }
            }
        }

        private void LoadTopContributors(SqlConnection conn)
        {
            using (SqlCommand cmd = new SqlCommand("sp_GetContentEngagementAnalytics", conn))
            {
                cmd.CommandType = CommandType.StoredProcedure;

                using (SqlDataAdapter adapter = new SqlDataAdapter(cmd))
                {
                    DataSet ds = new DataSet();
                    adapter.Fill(ds);

                    if (ds.Tables.Count > 1 && ds.Tables[1].Rows.Count > 0)
                    {
                        gvTopContributors.DataSource = ds.Tables[1];
                        gvTopContributors.DataBind();
                    }
                    else
                    {
                        gvTopContributors.DataSource = null;
                        gvTopContributors.DataBind();
                    }
                }
            }
        }

        /// <summary>
        /// Load chart data from database and serialize to JSON for JavaScript
        /// </summary>
        private void LoadChartData()
        {
            string connectionString = ConfigurationManager.ConnectionStrings["ConnectionString"].ConnectionString;

            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                try
                {
                    conn.Open();

                    // Load User Growth Data (Last 30 days)
                    LoadUserGrowthData(conn);

                    // Load Content Activity Data (Last 4 weeks)
                    LoadContentActivityData(conn);

                    // Load Engagement Data (Last 7 days)
                    LoadEngagementData(conn);
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine("Load Chart Data Error: " + ex.Message);
                }
            }
        }

        /// <summary>
        /// Load user growth data from sp_GetUserGrowthAnalytics
        /// </summary>
        private void LoadUserGrowthData(SqlConnection conn)
        {
            using (SqlCommand cmd = new SqlCommand("sp_GetUserGrowthAnalytics", conn))
            {
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@Days", 30);

                using (SqlDataReader reader = cmd.ExecuteReader())
                {
                    var labels = new List<string>();
                    var newUsers = new List<int>();
                    var cumulativeUsers = new List<int>();

                    while (reader.Read())
                    {
                        DateTime date = Convert.ToDateTime(reader["Date"]);
                        labels.Add(date.ToString("MMM dd"));
                        newUsers.Add(Convert.ToInt32(reader["NewUsers"]));
                        cumulativeUsers.Add(Convert.ToInt32(reader["CumulativeUsers"]));
                    }

                    var chartData = new
                    {
                        labels = labels,
                        newUsers = newUsers,
                        cumulativeUsers = cumulativeUsers
                    };

                    hfUserGrowthData.Value = JsonConvert.SerializeObject(chartData);
                }
            }
        }

        /// <summary>
        /// Load content activity data (last 4 weeks)
        /// </summary>
        private void LoadContentActivityData(SqlConnection conn)
        {
            // Query to get progressions created and shared by week
            string query = @"
                WITH WeekNumbers AS (
                    SELECT 1 AS WeekNum, DATEADD(DAY, -28, CAST(GETDATE() AS DATE)) AS StartDate, DATEADD(DAY, -22, CAST(GETDATE() AS DATE)) AS EndDate
                    UNION ALL SELECT 2, DATEADD(DAY, -21, CAST(GETDATE() AS DATE)), DATEADD(DAY, -15, CAST(GETDATE() AS DATE))
                    UNION ALL SELECT 3, DATEADD(DAY, -14, CAST(GETDATE() AS DATE)), DATEADD(DAY, -8, CAST(GETDATE() AS DATE))
                    UNION ALL SELECT 4, DATEADD(DAY, -7, CAST(GETDATE() AS DATE)), CAST(GETDATE() AS DATE)
                )
                SELECT 
                    wn.WeekNum,
                    'Week ' + CAST(wn.WeekNum AS VARCHAR(1)) AS WeekLabel,
                    COALESCE(COUNT(DISTINCT p.ProgressionID), 0) AS ProgressionsCreated,
                    COALESCE(COUNT(DISTINCT sp.SharedProgressionID), 0) AS ProgressionsShared
                FROM WeekNumbers wn
                LEFT JOIN Progressions p ON CAST(p.UploadDate AS DATE) BETWEEN wn.StartDate AND wn.EndDate
                LEFT JOIN SharedProgressions sp ON CAST(sp.ShareDate AS DATE) BETWEEN wn.StartDate AND wn.EndDate
                GROUP BY wn.WeekNum
                ORDER BY wn.WeekNum";

            using (SqlCommand cmd = new SqlCommand(query, conn))
            {
                using (SqlDataReader reader = cmd.ExecuteReader())
                {
                    var labels = new List<string>();
                    var created = new List<int>();
                    var shared = new List<int>();

                    while (reader.Read())
                    {
                        labels.Add(reader["WeekLabel"].ToString());
                        created.Add(Convert.ToInt32(reader["ProgressionsCreated"]));
                        shared.Add(Convert.ToInt32(reader["ProgressionsShared"]));
                    }

                    var chartData = new
                    {
                        labels = labels,
                        created = created,
                        shared = shared
                    };

                    hfContentActivityData.Value = JsonConvert.SerializeObject(chartData);
                }
            }
        }

        /// <summary>
        /// Load engagement data (last 7 days)
        /// </summary>
        private void LoadEngagementData(SqlConnection conn)
        {
            // Query to get views, likes, and comments by day
            string query = @"
                WITH Last7Days AS (
                    SELECT CAST(DATEADD(DAY, -6, GETDATE()) AS DATE) AS Date
                    UNION ALL SELECT CAST(DATEADD(DAY, -5, GETDATE()) AS DATE)
                    UNION ALL SELECT CAST(DATEADD(DAY, -4, GETDATE()) AS DATE)
                    UNION ALL SELECT CAST(DATEADD(DAY, -3, GETDATE()) AS DATE)
                    UNION ALL SELECT CAST(DATEADD(DAY, -2, GETDATE()) AS DATE)
                    UNION ALL SELECT CAST(DATEADD(DAY, -1, GETDATE()) AS DATE)
                    UNION ALL SELECT CAST(GETDATE() AS DATE)
                )
                SELECT 
                    d.Date,
                    DATENAME(dw, d.Date) AS DayLabel,
                    -- Approximate views (using shared progression view count)
                    COALESCE(SUM(CASE WHEN CAST(sp.ShareDate AS DATE) = d.Date THEN sp.ViewCount ELSE 0 END), 0) AS Views,
                    -- Likes created on this day
                    COALESCE(COUNT(DISTINCT CASE WHEN CAST(pl.CreatedDate AS DATE) = d.Date THEN pl.LikeID END), 0) AS Likes,
                    -- Comments created on this day
                    COALESCE(COUNT(DISTINCT CASE WHEN CAST(pc.CreatedDate AS DATE) = d.Date THEN pc.CommentID END), 0) AS Comments
                FROM Last7Days d
                LEFT JOIN SharedProgressions sp ON CAST(sp.ShareDate AS DATE) <= d.Date
                LEFT JOIN ProgressionLikes pl ON CAST(pl.CreatedDate AS DATE) = d.Date
                LEFT JOIN ProgressionComments pc ON CAST(pc.CreatedDate AS DATE) = d.Date AND pc.IsDeleted = 0
                GROUP BY d.Date
                ORDER BY d.Date";

            using (SqlCommand cmd = new SqlCommand(query, conn))
            {
                using (SqlDataReader reader = cmd.ExecuteReader())
                {
                    var labels = new List<string>();
                    var views = new List<int>();
                    var likes = new List<int>();
                    var comments = new List<int>();

                    while (reader.Read())
                    {
                        labels.Add(reader["DayLabel"].ToString().Substring(0, 3)); // Mon, Tue, etc.
                        views.Add(Convert.ToInt32(reader["Views"]));
                        likes.Add(Convert.ToInt32(reader["Likes"]));
                        comments.Add(Convert.ToInt32(reader["Comments"]));
                    }

                    var chartData = new
                    {
                        labels = labels,
                        views = views,
                        likes = likes,
                        comments = comments
                    };

                    hfEngagementData.Value = JsonConvert.SerializeObject(chartData);
                }
            }
        }

        protected void btnExportCSV_Click(object sender, EventArgs e)
        {
            Response.Clear();
            Response.ContentType = "text/csv";
            Response.AddHeader("Content-Disposition",
                "attachment; filename=Chordal_Analytics_" + DateTime.Now.ToString("yyyyMMdd") + ".csv");

            StringBuilder csv = new StringBuilder();

            // Header
            csv.AppendLine("Chordal Analytics Report");
            csv.AppendLine("Generated: " + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));
            csv.AppendLine("");

            // User Metrics
            csv.AppendLine("USER STATISTICS");
            csv.AppendLine("Metric,Value");
            csv.AppendLine("Total Users," + lblTotalUsers.Text);
            csv.AppendLine("New Users," + lblNewUsers.Text);
            csv.AppendLine("Active Users," + lblActiveUsers.Text);
            csv.AppendLine("Admin Users," + lblAdminUsers.Text);
            csv.AppendLine("");

            // Content Metrics
            csv.AppendLine("CONTENT STATISTICS");
            csv.AppendLine("Metric,Value");
            csv.AppendLine("Total Progressions," + lblTotalProgressions.Text);
            csv.AppendLine("Shared Progressions," + lblSharedProgressions.Text);
            csv.AppendLine("Published Progressions," + lblPublishedProgressions.Text);
            csv.AppendLine("Under Review," + lblUnderReview.Text);
            csv.AppendLine("");

            // Community Metrics
            csv.AppendLine("COMMUNITY STATISTICS");
            csv.AppendLine("Metric,Value");
            csv.AppendLine("Total Views," + lblTotalViews.Text);
            csv.AppendLine("Total Likes," + lblTotalLikes.Text);
            csv.AppendLine("Total Comments," + lblTotalComments.Text);
            csv.AppendLine("");

            // Tables
            csv.AppendLine("TOP PROGRESSIONS");
            csv.AppendLine("Title,Author,Views,Likes,Comments");
            foreach (GridViewRow row in gvTopContent.Rows)
            {
                csv.AppendLine(string.Join(",",
                    row.Cells[0].Text,
                    row.Cells[1].Text,
                    row.Cells[2].Text,
                    row.Cells[3].Text,
                    row.Cells[4].Text
                ));
            }

            Response.Write(csv.ToString());
            Response.End();
        }

        private string FormatNumber(object value)
        {
            if (value == null || value == DBNull.Value)
                return "0";

            try
            {
                long number = Convert.ToInt64(value);

                if (number >= 1000000)
                    return (number / 1000000.0).ToString("0.#") + "M";
                else if (number >= 1000)
                    return (number / 1000.0).ToString("0.#") + "K";
                else
                    return number.ToString("N0");
            }
            catch
            {
                return value.ToString();
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