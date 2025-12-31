using System;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Net;
using System.Net.Mail;
using System.Web;
using System.Web.UI;

namespace ChordalWebApp
{
    public partial class ForgotPassword : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack)
            {
                lblStatus.Text = "";
            }
        }

        protected void btnSubmit_Click(object sender, EventArgs e)
        {
            if (!Page.IsValid)
            {
                lblStatus.Text = "Please correct the errors on the page.";
                lblStatus.CssClass = "text-danger";
                return;
            }

            string email = txtEmail.Text.Trim();
            string connectionString = ConfigurationManager.ConnectionStrings["ConnectionString"].ConnectionString;

            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                try
                {
                    conn.Open();

                    // Check if email exists in database
                    string checkEmailQuery = "SELECT UserID, Username, IsEnabled FROM Users WHERE Email = @Email";
                    using (SqlCommand cmd = new SqlCommand(checkEmailQuery, conn))
                    {
                        cmd.Parameters.AddWithValue("@Email", email);
                        
                        using (SqlDataReader reader = cmd.ExecuteReader())
                        {
                            if (reader.Read())
                            {
                                int userId = Convert.ToInt32(reader["UserID"]);
                                string username = reader["Username"].ToString();
                                bool isEnabled = Convert.ToBoolean(reader["IsEnabled"]);
                                reader.Close();

                                if (!isEnabled)
                                {
                                    // For security, don't reveal that account is disabled
                                    DisplaySuccessMessage();
                                    return;
                                }

                                // Generate reset token
                                string resetToken = GenerateResetToken();
                                DateTime expirationTime = DateTime.Now.AddHours(24); // Token valid for 24 hours

                                // Store token in database
                                StoreResetToken(conn, userId, resetToken, expirationTime);

                                // Send email
                                SendResetEmail(email, username, resetToken);

                                // Log notification
                                NotificationHelper.LogNotification(userId, "Password Reset", 
                                    "Password reset link has been sent to your email.", conn);

                                DisplaySuccessMessage();
                            }
                            else
                            {
                                // For security purposes, don't reveal whether email exists
                                DisplaySuccessMessage();
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    lblStatus.Text = "An error occurred. Please try again later.";
                    lblStatus.CssClass = "text-danger";
                    System.Diagnostics.Trace.WriteLine("ForgotPassword Error: " + ex.ToString());
                }
            }
        }

        private void DisplaySuccessMessage()
        {
            pnlEmailForm.Visible = false;
            lblStatus.Text = "If this email is registered, you will receive a password reset link shortly. Please check your inbox (and spam folder).";
            lblStatus.CssClass = "text-success";
        }

        private string GenerateResetToken()
        {
            // Generate a secure random token
            return Guid.NewGuid().ToString("N") + Guid.NewGuid().ToString("N");
        }

        private void StoreResetToken(SqlConnection conn, int userId, string token, DateTime expiration)
        {
            // First, delete any existing reset tokens for this user
            string deleteQuery = "DELETE FROM PasswordResetTokens WHERE UserID = @UserID";
            using (SqlCommand cmd = new SqlCommand(deleteQuery, conn))
            {
                cmd.Parameters.AddWithValue("@UserID", userId);
                cmd.ExecuteNonQuery();
            }

            // Insert new reset token
            string insertQuery = @"
                INSERT INTO PasswordResetTokens (UserID, Token, ExpirationTime, IsUsed, CreatedDate)
                VALUES (@UserID, @Token, @ExpirationTime, 0, GETDATE())";
            
            using (SqlCommand cmd = new SqlCommand(insertQuery, conn))
            {
                cmd.Parameters.AddWithValue("@UserID", userId);
                cmd.Parameters.AddWithValue("@Token", token);
                cmd.Parameters.AddWithValue("@ExpirationTime", expiration);
                cmd.ExecuteNonQuery();
            }
        }

        private void SendResetEmail(string toEmail, string username, string resetToken)
        {
            try
            {
                // Construct reset URL
                string resetUrl = Request.Url.Scheme + "://" + Request.Url.Authority + 
                                  ResolveUrl("~/ResetPassword.aspx?token=" + Server.UrlEncode(resetToken));

                // Email configuration - YOU NEED TO CONFIGURE THESE SETTINGS
                string fromEmail = ConfigurationManager.AppSettings["SmtpFromEmail"] ?? "noreply@chordal.com";
                string smtpHost = ConfigurationManager.AppSettings["SmtpHost"] ?? "smtp.gmail.com";
                int smtpPort = int.Parse(ConfigurationManager.AppSettings["SmtpPort"] ?? "587");
                string smtpUsername = ConfigurationManager.AppSettings["SmtpUsername"] ?? "";
                string smtpPassword = ConfigurationManager.AppSettings["SmtpPassword"] ?? "";

                MailMessage mail = new MailMessage();
                mail.From = new MailAddress(fromEmail, "Chordal Support");
                mail.To.Add(toEmail);
                mail.Subject = "Password Reset Request - Chordal";
                
                mail.Body = $@"
                    <html>
                    <body style='font-family: Arial, sans-serif;'>
                        <div style='max-width: 600px; margin: 0 auto; padding: 20px;'>
                            <h2 style='color: #333;'>Password Reset Request</h2>
                            <p>Hello {Server.HtmlEncode(username)},</p>
                            <p>We received a request to reset your password for your Chordal account.</p>
                            <p>Click the button below to reset your password:</p>
                            <div style='text-align: center; margin: 30px 0;'>
                                <a href='{resetUrl}' style='background-color: #77aaff; color: white; padding: 12px 30px; text-decoration: none; border-radius: 5px; display: inline-block;'>Reset Password</a>
                            </div>
                            <p>Or copy and paste this link into your browser:</p>
                            <p style='word-break: break-all; color: #77aaff;'>{resetUrl}</p>
                            <p><strong>This link will expire in 24 hours.</strong></p>
                            <p>If you did not request a password reset, please ignore this email or contact support if you have concerns.</p>
                            <hr style='margin: 30px 0; border: none; border-top: 1px solid #ddd;' />
                            <p style='color: #666; font-size: 12px;'>
                                This is an automated message from Chordal. Please do not reply to this email.
                            </p>
                        </div>
                    </body>
                    </html>
                ";
                mail.IsBodyHtml = true;

                SmtpClient smtp = new SmtpClient(smtpHost, smtpPort);
                smtp.EnableSsl = true;
                smtp.UseDefaultCredentials = false;
                smtp.Credentials = new NetworkCredential(smtpUsername, smtpPassword);

                // For development/testing: Log email instead of sending
                if (string.IsNullOrEmpty(smtpUsername))
                {
                    System.Diagnostics.Trace.WriteLine("=== PASSWORD RESET EMAIL ===");
                    System.Diagnostics.Trace.WriteLine("To: " + toEmail);
                    System.Diagnostics.Trace.WriteLine("Reset URL: " + resetUrl);
                    System.Diagnostics.Trace.WriteLine("Token: " + resetToken);
                    System.Diagnostics.Trace.WriteLine("=============================");
                }
                else
                {
                    smtp.Send(mail);
                }
            }
            catch (Exception ex)
            {
                // Log the error but don't show it to user for security
                System.Diagnostics.Trace.WriteLine("Email Send Error: " + ex.ToString());
            }
        }
    }
}