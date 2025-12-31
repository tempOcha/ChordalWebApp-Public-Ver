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
    public partial class Guidelines : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack)
            {
                LoadGuidelines();

                // Show admin link if user is admin
                if (Session["IsAdmin"] != null && (bool)Session["IsAdmin"] == true)
                {
                    pnlAdminLink.Visible = true;
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

                    using (SqlCommand cmd = new SqlCommand("sp_GetCommunityGuidelines", conn))
                    {
                        cmd.CommandType = CommandType.StoredProcedure;

                        using (SqlDataAdapter adapter = new SqlDataAdapter(cmd))
                        {
                            DataTable dt = new DataTable();
                            adapter.Fill(dt);

                            if (dt.Rows.Count > 0)
                            {
                                rptGuidelines.DataSource = dt;
                                rptGuidelines.DataBind();
                                pnlGuidelines.Visible = true;
                                pnlEmptyState.Visible = false;

                                // Get most recent update date
                                DateTime lastModified = DateTime.MinValue;
                                foreach (DataRow row in dt.Rows)
                                {
                                    DateTime modified = Convert.ToDateTime(row["LastModified"]);
                                    if (modified > lastModified)
                                        lastModified = modified;
                                }

                                lblLastUpdated.Text = lastModified.ToString("MMMM dd, yyyy");
                            }
                            else
                            {
                                pnlGuidelines.Visible = false;
                                pnlEmptyState.Visible = true;
                                lblLastUpdated.Text = "N/A";
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine("Load Guidelines Error: " + ex.Message);
                    pnlGuidelines.Visible = false;
                    pnlEmptyState.Visible = true;
                }
            }
        }
    }
}