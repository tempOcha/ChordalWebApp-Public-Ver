using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Configuration; // For ConnectionString
using System.Data.SqlClient; // For ADO.NET
using Newtonsoft.Json; // For JSON parsing

using ChordalWebApp.Models;


namespace ChordalWebApp
{
    public partial class UploadProgression : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            if (Session["UserID"] == null) // Check for UserID specifically
            {
                Response.Redirect("~/Login.aspx?ReturnUrl=" + Server.UrlEncode(Request.Url.PathAndQuery));
            }
            if (!IsPostBack) // Clear status only on initial load
            {
                lblUploadStatus.Text = "";
                litJsonContent.Text = "";
                litJsonContent.Visible = false;
            }
        }

        protected void btnUploadFile_Click(object sender, EventArgs e)
        {
            if (jsonFileUpload.HasFile)
            {
                if (Path.GetExtension(jsonFileUpload.FileName).ToLower() != ".json")
                {
                    lblUploadStatus.Text = "❌ Error: Please select a .json file.";
                    lblUploadStatus.CssClass = "upload-status visible error";
                    ScriptManager.RegisterStartupScript(this, GetType(), "showError", "showUploadError();", true);
                    return;
                }

                try
                {
                    string jsonContent = new StreamReader(jsonFileUpload.FileContent).ReadToEnd();
                    JsonProgressionFile uploadedProgression = JsonConvert.DeserializeObject<JsonProgressionFile>(jsonContent);

                    if (uploadedProgression == null || uploadedProgression.ChordEvents == null)
                    {
                        lblUploadStatus.Text = "❌ Error: Invalid JSON structure or no chord events found.";
                        lblUploadStatus.CssClass = "upload-status visible error";
                        ScriptManager.RegisterStartupScript(this, GetType(), "showError", "showUploadError();", true);
                        return;
                    }

                    int userId = Convert.ToInt32(Session["UserID"]);
                    SaveProgressionToDatabase(uploadedProgression, userId);

                    string progressionTitle = string.IsNullOrEmpty(uploadedProgression.ProgressionTitle) ? "Untitled" : uploadedProgression.ProgressionTitle;
                    lblUploadStatus.Text = "✓ Success! Progression '" + progressionTitle + "' uploaded and saved successfully!";
                    lblUploadStatus.CssClass = "upload-status visible success";

                    // Trigger the wave animation on success
                    ScriptManager.RegisterStartupScript(this, GetType(), "showSuccess", "showUploadSuccess(); showUploadProgress();", true);

                    // Optional: Show JSON preview
                    // litJsonContent.Text = JsonConvert.SerializeObject(uploadedProgression, Formatting.Indented);
                    // litJsonContent.Visible = true;
                    // ScriptManager.RegisterStartupScript(this, GetType(), "showPreview", "document.querySelector('.json-preview-section').classList.add('visible');", true);
                }
                catch (JsonReaderException jsonEx)
                {
                    lblUploadStatus.Text = "❌ Error parsing JSON file: " + jsonEx.Message;
                    lblUploadStatus.CssClass = "upload-status visible error";
                    ScriptManager.RegisterStartupScript(this, GetType(), "showError", "showUploadError();", true);
                }
                catch (Exception ex)
                {
                    lblUploadStatus.Text = "❌ Error processing file: " + ex.Message;
                    lblUploadStatus.CssClass = "upload-status visible error";
                    ScriptManager.RegisterStartupScript(this, GetType(), "showError", "showUploadError();", true);
                }
            }
            else
            {
                lblUploadStatus.Text = "⚠️ Please select a file to upload.";
                lblUploadStatus.CssClass = "upload-status visible info";
                ScriptManager.RegisterStartupScript(this, GetType(), "showInfo", "showUploadInfo();", true);
            }
        }

        // The actual method to save it

        private void SaveProgressionToDatabase(JsonProgressionFile progressionData, int userId)
        {
            string connectionString = ConfigurationManager.ConnectionStrings["ConnectionString"].ConnectionString;
            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                conn.Open();

                using (SqlTransaction transaction = conn.BeginTransaction())
                {
                    try
                    {
                        // 1. Insert into Progressions table
                        string insertProgressionQuery = @"
                            INSERT INTO Progressions (UserID, ProgressionTitle, KeyRoot, IsKeyMajor, Tempo, Timestamp, ChordalVersion, UploadDate)
                            OUTPUT INSERTED.ProgressionID
                            VALUES (@UserID, @ProgressionTitle, @KeyRoot, @IsKeyMajor, @Tempo, @Timestamp, @ChordalVersion, GETDATE());";

                        int newProgressionID;
                        using (SqlCommand cmdProgression = new SqlCommand(insertProgressionQuery, conn, transaction))
                        {
                            cmdProgression.Parameters.AddWithValue("@UserID", userId);
                            cmdProgression.Parameters.AddWithValue("@ProgressionTitle", string.IsNullOrEmpty(progressionData.ProgressionTitle) ? (object)DBNull.Value : progressionData.ProgressionTitle);
                            cmdProgression.Parameters.AddWithValue("@KeyRoot", progressionData.KeyRoot);
                            cmdProgression.Parameters.AddWithValue("@IsKeyMajor", progressionData.IsKeyMajor);
                            cmdProgression.Parameters.AddWithValue("@Tempo", progressionData.Tempo.HasValue ? (object)progressionData.Tempo.Value : (object)DBNull.Value);
                            cmdProgression.Parameters.AddWithValue("@Timestamp", progressionData.Timestamp.HasValue ? (object)progressionData.Timestamp.Value : (object)DBNull.Value);
                            cmdProgression.Parameters.AddWithValue("@ChordalVersion", string.IsNullOrEmpty(progressionData.ChordalVersion) ? (object)DBNull.Value : progressionData.ChordalVersion);

                            newProgressionID = (int)cmdProgression.ExecuteScalar();
                        }

                        // 2. Insert into ProgressionChordEvents table
                        string insertEventQuery = @"
                            INSERT INTO ProgressionChordEvents 
                                (ProgressionID, StartTime, Duration, ChordName, NotesCSV, RootNoteChroma, Quality, Extensions, RomanNumeral, ChordFunction, SequenceOrder)
                            VALUES 
                                (@ProgressionID, @StartTime, @Duration, @ChordName, @NotesCSV, @RootNoteChroma, @Quality, @Extensions, @RomanNumeral, @ChordFunction, @SequenceOrder);";

                        int sequence = 0;
                        foreach (var chordEvent in progressionData.ChordEvents)
                        {
                            using (SqlCommand cmdEvent = new SqlCommand(insertEventQuery, conn, transaction))
                            {
                                cmdEvent.Parameters.AddWithValue("@ProgressionID", newProgressionID);
                                cmdEvent.Parameters.AddWithValue("@StartTime", chordEvent.StartTime);
                                cmdEvent.Parameters.AddWithValue("@Duration", chordEvent.Duration);
                                cmdEvent.Parameters.AddWithValue("@ChordName", chordEvent.ChordInfo.Name);

                                // Convert notes array to CSV string
                                string notesCsv = (string)(chordEvent.ChordInfo.Notes != null ? string.Join(",", chordEvent.ChordInfo.Notes) : (object)DBNull.Value);
                                cmdEvent.Parameters.AddWithValue("@NotesCSV", notesCsv);

                                cmdEvent.Parameters.AddWithValue("@RootNoteChroma", chordEvent.ChordInfo.RootNote);
                                cmdEvent.Parameters.AddWithValue("@Quality", chordEvent.ChordInfo.Quality);
                                cmdEvent.Parameters.AddWithValue("@Extensions", string.IsNullOrEmpty(chordEvent.ChordInfo.Extensions) ? (object)DBNull.Value : chordEvent.ChordInfo.Extensions);
                                cmdEvent.Parameters.AddWithValue("@RomanNumeral", chordEvent.ChordInfo.RomanNumeral);
                                cmdEvent.Parameters.AddWithValue("@ChordFunction", chordEvent.ChordInfo.Function);
                                cmdEvent.Parameters.AddWithValue("@SequenceOrder", sequence++);

                                cmdEvent.ExecuteNonQuery();
                            }
                        }
                        transaction.Commit(); // Commit if all inserts were successful
                    }
                    catch (Exception ex)
                    {
                        transaction.Rollback();

                        throw new Exception("Error saving progression to database: " + ex.Message, ex);
                    }
                }
            }
        }
    }
}