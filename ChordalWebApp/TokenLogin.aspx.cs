using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace ChordalWebApp
{
    public partial class TokenLogin : System.Web.UI.Page
    {
        protected bool ShouldAutoRedirect { get; set; } = false;

        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack)
            {
                ProcessTokenAuthentication();
            }
        }

        private void ProcessTokenAuthentication()
        {
            string token = Request.QueryString["token"];
            string email = Request.QueryString["email"];

            // Validate required parameters
            if (string.IsNullOrWhiteSpace(token) || string.IsNullOrWhiteSpace(email))
            {
                ShowError("Invalid authentication request. Missing required parameters.", false);
                return;
            }

            // Decode parameters
            email = HttpUtility.UrlDecode(email);
            token = HttpUtility.UrlDecode(token);

            string connectionString = ConfigurationManager.ConnectionStrings["ConnectionString"].ConnectionString;

            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                try
                {
                    conn.Open();

                    // Get user information
                    string query = @"
                        SELECT UserID, Username, PasswordHash, IsEnabled
                        FROM Users 
                        WHERE Email = @Email";

                    using (SqlCommand cmd = new SqlCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@Email", email);

                        using (SqlDataReader reader = cmd.ExecuteReader())
                        {
                            if (reader.Read())
                            {
                                int userId = Convert.ToInt32(reader["UserID"]);
                                string username = reader["Username"].ToString();
                                string storedPasswordHash = reader["PasswordHash"].ToString();
                                bool isEnabled = Convert.ToBoolean(reader["IsEnabled"]);

                                reader.Close();

                                // Check if account is enabled
                                if (!isEnabled)
                                {
                                    ShowError("Your account has been disabled. Please contact support.", false);
                                    return;
                                }

                                // Validate token and verify password
                                string extractedPassword;
                                if (ValidateTokenAndExtractPassword(token, email, out extractedPassword))
                                {
                                    // Verify the extracted password against stored PBKDF2 hash
                                    if (VerifyPasswordWithPBKDF2(extractedPassword, storedPasswordHash))
                                    {
                                        // Authentication successful
                                        AuthenticateUser(conn, userId, username, email);
                                        ShowSuccess($"Welcome back, {username}!", true);
                                    }
                                    else
                                    {
                                        ShowError("Authentication failed. Invalid credentials.", false);
                                    }
                                }
                                else
                                {
                                    ShowError("Authentication failed. Invalid or expired token.", false);
                                }
                            }
                            else
                            {
                                ShowError("Account not found.", false);
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    ShowError("An error occurred during authentication.", false);
                    System.Diagnostics.Trace.WriteLine("TokenLogin Error: " + ex.ToString());
                }
            }
        }

        private bool ValidateTokenAndExtractPassword(string token, string email, out string password)
        {
            password = null;

            try
            {
                // Token format: Base64(Email:Password:Timestamp)
                byte[] tokenBytes = Convert.FromBase64String(token);
                string decodedToken = Encoding.UTF8.GetString(tokenBytes);

                string[] parts = decodedToken.Split(':');
                if (parts.Length != 3)
                {
                    System.Diagnostics.Trace.WriteLine("Token validation failed: Invalid format");
                    return false;
                }

                string tokenEmail = parts[0];
                string tokenPassword = parts[1];
                string timestampStr = parts[2];

                // Verify email matches
                if (!tokenEmail.Equals(email, StringComparison.OrdinalIgnoreCase))
                {
                    System.Diagnostics.Trace.WriteLine("Token validation failed: Email mismatch");
                    return false;
                }

                // Verify timestamp (token valid for 5 minutes)
                if (!long.TryParse(timestampStr, out long timestamp))
                {
                    System.Diagnostics.Trace.WriteLine("Token validation failed: Invalid timestamp");
                    return false;
                }

                DateTime tokenTime = DateTimeOffset.FromUnixTimeSeconds(timestamp).DateTime;
                DateTime now = DateTime.UtcNow;
                TimeSpan difference = now - tokenTime;

                if (difference.TotalMinutes > 5)
                {
                    System.Diagnostics.Trace.WriteLine("Token validation failed: Token expired (age: " + difference.TotalMinutes + " minutes)");
                    return false;
                }

                if (difference.TotalSeconds < -10)
                {
                    System.Diagnostics.Trace.WriteLine("Token validation failed: Token from future");
                    return false;
                }

                // Token is valid, extract password
                password = tokenPassword;
                return true;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Trace.WriteLine("Token Validation Error: " + ex.ToString());
                return false;
            }
        }

        private bool VerifyPasswordWithPBKDF2(string password, string storedPasswordHash)
        {
            try
            {
                // Use PBKDF2Hash class to verify password
                PBKDF2Hash hashVerifier = new PBKDF2Hash(password, storedPasswordHash);
                return hashVerifier.PasswordCheck;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Trace.WriteLine("PBKDF2 Verification Error: " + ex.ToString());
                return false;
            }
        }

        private void AuthenticateUser(SqlConnection conn, int userId, string username, string email)
        {
            // Set session variables
            Session["UserID"] = userId;
            Session["Username"] = username;

            // Update last login date
            string updateQuery = "UPDATE Users SET LastLoginDate = GETDATE() WHERE UserID = @UserID";
            using (SqlCommand cmd = new SqlCommand(updateQuery, conn))
            {
                cmd.Parameters.AddWithValue("@UserID", userId);
                cmd.ExecuteNonQuery();
            }

            // Log notification for VST login
            try
            {
                NotificationHelper.LogNotification(
                    userId,
                    NotificationHelper.NotificationTypes.Account,
                    "Logged in from VST plugin",
                    conn
                );
            }
            catch (Exception ex)
            {
                // Don't fail authentication if notification fails
                System.Diagnostics.Trace.WriteLine("Notification Error: " + ex.Message);
            }

            // Send email notification
            try
            {
                string emailSubject = "New Login to Your Chordal Account";
                string emailBody = $@"
                    <h2>Login Notification</h2>
                    <p>Hello {username},</p>
                    <p>Your account was accessed from the Chordal VST plugin.</p>
                    <p><strong>Login Details:</strong></p>
                    <ul>
                        <li>Date/Time: {DateTime.Now:yyyy-MM-dd HH:mm:ss}</li>
                        <li>Method: VST Plugin Token Authentication</li>
                    </ul>
                    <p>If you did not authorize this login, please secure your account immediately.</p>
                    <p>Best regards,<br/>The Chordal Team</p>
                ";

                NotificationHelper.SendEmailNotification(email, emailSubject, emailBody, userId);
            }
            catch (Exception ex)
            {
                // Don't fail authentication if email fails
                System.Diagnostics.Trace.WriteLine("Email Notification Error: " + ex.Message);
            }
        }

        /// <summary>
        /// Shows a success message and optionally enables auto-redirect
        /// </summary>
        private void ShowSuccess(string message, bool autoRedirect)
        {
            pnlLoading.Visible = false;
            pnlStatus.Visible = true;

            // Update title
            statusTitle.InnerText = "Authentication Successful";

            // Show success message
            statusMessage.InnerHtml = $"<strong>✓ Success!</strong><br/>{message}";
            statusMessage.Attributes["class"] = "auth-status-message auth-status-success";

            if (autoRedirect)
            {
                pnlRedirectInfo.Visible = true;
                ShouldAutoRedirect = true;
            }
        }

        /// <summary>
        /// Shows an error message and optionally shows login link
        /// </summary>
        private void ShowError(string message, bool showLoginLink)
        {
            pnlLoading.Visible = false;
            pnlStatus.Visible = true;

            // Update title
            statusTitle.InnerText = "Authentication Failed";

            // Show error message
            statusMessage.InnerHtml = $"<strong>✗ Error</strong><br/>{message}";
            statusMessage.Attributes["class"] = "auth-status-message auth-status-error";

            if (showLoginLink)
            {
                pnlLoginPrompt.Visible = true;
            }
        }
    }
}