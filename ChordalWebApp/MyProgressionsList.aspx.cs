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
    public partial class MyProgressionsList : System.Web.UI.Page
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
                BindProgressionsGrid();
            }
        }

        private void BindProgressionsGrid()
        {
            int userId = Convert.ToInt32(Session["UserID"]);
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
                        (SELECT COUNT(*) FROM ProgressionChordEvents pce WHERE pce.ProgressionID = p.ProgressionID) AS ChordEventCount
                    FROM Progressions p
                    WHERE p.UserID = @UserID
                    ORDER BY p.UploadDate DESC";

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
                       
                        System.Diagnostics.Trace.WriteLine("Error fetching progressions: " + ex.Message);
                        // Angry message to make me remember lol
                    }
                }
            }

            // Add a computed column for KeySignature for display
            dt.Columns.Add("KeySignature", typeof(string));
            foreach (DataRow row in dt.Rows)
            {
                int keyRoot = Convert.ToInt32(row["KeyRoot"]);
                bool isMajor = Convert.ToBoolean(row["IsKeyMajor"]);
                row["KeySignature"] = GetKeySignatureString(keyRoot, isMajor);

                // Handle DBNull for ProgressionTitle if the ISNULL in SQL didn't catch it (shouldn't happen now)
                if (row["ProgressionTitle"] == DBNull.Value)
                {
                    row["ProgressionTitle"] = "Untitled Progression";
                }
            }

            if (dt.Rows.Count > 0)
            {
                gvProgressions.DataSource = dt;
                gvProgressions.DataBind();
                gvProgressions.Visible = true;
                phNoProgressions.Visible = false;
            }
            else
            {
                gvProgressions.Visible = false;
                phNoProgressions.Visible = true;
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

        protected void gvProgressions_RowCommand(object sender, GridViewCommandEventArgs e)
        {
            if (e.CommandName == "ViewDetails")
            {
                int progressionID = Convert.ToInt32(e.CommandArgument);
                Response.Redirect($"ProgressionDetailView.aspx?ProgID={progressionID}");
            }
            /* Handle Delete command later if you add it
               else if (e.CommandName == "DeleteProgression")
               {
                   int progressionID = Convert.ToInt32(e.CommandArgument);
                   DeleteProgression(progressionID);
                   BindProgressionsGrid(); // Rebind grid after delete
               }*/
        }

        /* Placeholder for DeleteProgression - implement if needed
           private void DeleteProgression(int progressionID)
           {
              // Add ADO.NET code to delete from Progressions and ProgressionChordEvents
              // Ensure UserID from session matches the owner of progressionID for security
              // Do this lateeeeeeeeeerr
          } */
    }
}