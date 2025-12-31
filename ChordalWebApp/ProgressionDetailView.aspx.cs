using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.SqlClient;
using System.Data;
using System.Web.UI;
using System.Web.UI.WebControls;
using Newtonsoft.Json;
using System.IO;

namespace ChordalWebApp
{
    public partial class ProgressionDetailView : System.Web.UI.Page
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
                if (Request.QueryString["ProgID"] != null && int.TryParse(Request.QueryString["ProgID"], out int progressionId))
                {
                    LoadProgressionDetails(progressionId);
                }
                else
                {
                    ShowError("Invalid Progression ID.");
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

                    // Query for Progression Details
                    string progressionQuery = @"
                        SELECT ProgressionTitle, KeyRoot, IsKeyMajor, UploadDate, Tempo 
                        FROM Progressions 
                        WHERE ProgressionID = @ProgressionID AND UserID = @UserID;";

                    // Query for Chord Events - SAME AS COMMUNITY VIEW
                    string eventsQuery = @"
                        SELECT ChordName, RomanNumeral, ChordFunction, StartTime, Duration, NotesCSV, SequenceOrder
                        FROM ProgressionChordEvents 
                        WHERE ProgressionID = @ProgressionID 
                        ORDER BY SequenceOrder ASC;";

                    DataSet ds = new DataSet();

                    // Load Progression Details
                    using (SqlCommand cmdProg = new SqlCommand(progressionQuery, conn))
                    {
                        cmdProg.Parameters.AddWithValue("@ProgressionID", progressionId);
                        cmdProg.Parameters.AddWithValue("@UserID", userId);
                        SqlDataAdapter daProg = new SqlDataAdapter(cmdProg);
                        daProg.Fill(ds, "ProgressionInfo");
                    }

                    // Load Chord Events
                    using (SqlCommand cmdEvents = new SqlCommand(eventsQuery, conn))
                    {
                        cmdEvents.Parameters.AddWithValue("@ProgressionID", progressionId);
                        SqlDataAdapter daEvents = new SqlDataAdapter(cmdEvents);
                        daEvents.Fill(ds, "ChordEvents");
                    }

                    if (ds.Tables["ProgressionInfo"] != null && ds.Tables["ProgressionInfo"].Rows.Count > 0)
                    {
                        DataRow progRow = ds.Tables["ProgressionInfo"].Rows[0];

                        // Set title
                        string title = progRow["ProgressionTitle"] == DBNull.Value ? "Untitled Progression" : progRow["ProgressionTitle"].ToString();
                        litProgressionTitle.Text = Server.HtmlEncode(title);
                        litPageTitle.Text = Server.HtmlEncode(title);

                        // Set key
                        int keyRoot = Convert.ToInt32(progRow["KeyRoot"]);
                        bool isMajor = Convert.ToBoolean(progRow["IsKeyMajor"]);
                        litKeySignature.Text = Server.HtmlEncode(GetKeySignatureString(keyRoot, isMajor));

                        // Set date
                        litUploadDate.Text = Convert.ToDateTime(progRow["UploadDate"]).ToString("MMMM dd, yyyy");

                        // Set tempo
                        if (progRow["Tempo"] != DBNull.Value)
                        {
                            litTempo.Text = progRow["Tempo"].ToString();
                            divTempo.Visible = true;
                        }
                        else
                        {
                            divTempo.Visible = false;
                        }

                        // Process chord events - EXACTLY LIKE COMMUNITY VIEW
                        if (ds.Tables["ChordEvents"] != null && ds.Tables["ChordEvents"].Rows.Count > 0)
                        {
                            System.Diagnostics.Trace.WriteLine("=== CHORD EVENTS DEBUG ===");
                            System.Diagnostics.Trace.WriteLine($"Number of chord events: {ds.Tables["ChordEvents"].Rows.Count}");

                            // Bind chord cards for display
                            rptChordEvents.DataSource = ds.Tables["ChordEvents"];
                            rptChordEvents.DataBind();
                            lblNoChords.Visible = false;

                            // Serialize chord data for MIDI visualization
                            var chordsList = new List<object>();
                            foreach (DataRow chordRow in ds.Tables["ChordEvents"].Rows)
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
                        else
                        {
                            lblNoChords.Visible = true;
                            hdnProgressionJSON.Value = "[]";
                        }

                        pnlProgressionDetails.Visible = true;
                        pnlError.Visible = false;
                    }
                    else
                    {
                        ShowError("Progression not found or you do not have permission to view it.");
                    }
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Trace.WriteLine("LoadProgressionDetails Error: " + ex.ToString());
                    ShowError("Error loading progression details: " + ex.Message);
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

        private void ShowError(string message)
        {
            pnlProgressionDetails.Visible = false;
            pnlError.Visible = true;
        }

        protected void btnExportMIDI_Click(object sender, EventArgs e)
        {
            if (Request.QueryString["ProgID"] == null || !int.TryParse(Request.QueryString["ProgID"], out int progressionId))
            {
                ShowError("Invalid Progression ID.");
                return;
            }

            int userId = Convert.ToInt32(Session["UserID"]);
            string connectionString = ConfigurationManager.ConnectionStrings["ConnectionString"].ConnectionString;

            try
            {
                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    conn.Open();

                    // Get progression metadata
                    string progressionQuery = @"
                        SELECT ProgressionTitle, Tempo 
                        FROM Progressions 
                        WHERE ProgressionID = @ProgressionID AND UserID = @UserID";

                    string progressionTitle = "Chord Progression";
                    int? tempo = null;

                    using (SqlCommand cmd = new SqlCommand(progressionQuery, conn))
                    {
                        cmd.Parameters.AddWithValue("@ProgressionID", progressionId);
                        cmd.Parameters.AddWithValue("@UserID", userId);

                        using (SqlDataReader reader = cmd.ExecuteReader())
                        {
                            if (reader.Read())
                            {
                                progressionTitle = reader["ProgressionTitle"] == DBNull.Value
                                    ? "Chord Progression"
                                    : reader["ProgressionTitle"].ToString();

                                if (reader["Tempo"] != DBNull.Value)
                                {
                                    tempo = Convert.ToInt32(reader["Tempo"]);
                                }
                            }
                            else
                            {
                                ShowError("Progression not found or you do not have permission to export it.");
                                return;
                            }
                        }
                    }

                    // Get chord events
                    string eventsQuery = @"
                        SELECT StartTime, Duration, ChordName, NotesCSV, SequenceOrder
                        FROM ProgressionChordEvents 
                        WHERE ProgressionID = @ProgressionID 
                        ORDER BY SequenceOrder ASC";

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
                                    NotesCSV = reader["NotesCSV"]?.ToString() ?? "",
                                    SequenceOrder = Convert.ToInt32(reader["SequenceOrder"])
                                });
                            }
                        }
                    }

                    if (chordEvents.Count == 0)
                    {
                        ShowError("No chord events found in this progression.");
                        return;
                    }

                    // Convert to MIDI
                    byte[] midiData = MidiExportHelper.ConvertProgressionToMidi(
                        progressionTitle,
                        tempo,
                        chordEvents
                    );

                    // Generate filename
                    string safeFileName = progressionTitle.Replace(" ", "_");
                    // Remove invalid filename characters
                    foreach (char c in Path.GetInvalidFileNameChars())
                    {
                        safeFileName = safeFileName.Replace(c.ToString(), "");
                    }
                    safeFileName += ".mid";

                    // Send file to browser
                    Response.Clear();
                    Response.ContentType = "audio/midi";
                    Response.AddHeader("Content-Disposition", $"attachment; filename=\"{safeFileName}\"");
                    Response.BinaryWrite(midiData);

                    // Log notification
                    NotificationHelper.LogNotification(
                        userId,
                        NotificationHelper.NotificationTypes.System,
                        $"Exported progression '{progressionTitle}' to MIDI format",
                        conn
                    );

                    Response.End();
                }
            }
            catch (ArgumentException ex)
            {
                ShowError($"Export Error: {ex.Message}");
                System.Diagnostics.Trace.WriteLine("MIDI Export Error: " + ex.ToString());
            }
            catch (Exception ex)
            {
                ShowError("An error occurred while exporting to MIDI. Please try again.");
                System.Diagnostics.Trace.WriteLine("MIDI Export Error: " + ex.ToString());
            }
        }

        protected void btnShare_Click(object sender, EventArgs e)
        {
            if (Request.QueryString["ProgID"] != null && int.TryParse(Request.QueryString["ProgID"], out int progressionId))
            {
                Response.Redirect($"~/ShareProgression.aspx?id={progressionId}");
            }
        }
    }
}
