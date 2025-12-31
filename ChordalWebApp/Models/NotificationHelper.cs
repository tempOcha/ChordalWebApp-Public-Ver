using System;
using System.Configuration;
using System.Data.SqlClient;
using System.Net;
using System.Net.Mail;

namespace ChordalWebApp
{
    /// <summary>
    /// Helper class for managing user notifications across the Chordal platform.
    /// Handles both database logging and email notifications.
    /// </summary>
    public static class NotificationHelper
    {
        /// <summary>
        /// Logs a notification to the database for a specific user.
        /// </summary>
        /// <param name="userId">The ID of the user receiving the notification</param>
        /// <param name="notificationType">Type of notification (e.g., "Account", "Community", "System")</param>
        /// <param name="message">The notification message content</param>
        /// <param name="existingConnection">Optional existing SQL connection to use (for transactions)</param>
        public static void LogNotification(int userId, string notificationType, string message, SqlConnection existingConnection = null)
        {
            bool shouldCloseConnection = false;
            SqlConnection conn = existingConnection;

            try
            {
                if (conn == null)
                {
                    string connectionString = ConfigurationManager.ConnectionStrings["ConnectionString"].ConnectionString;
                    conn = new SqlConnection(connectionString);
                    conn.Open();
                    shouldCloseConnection = true;
                }

                string insertQuery = @"
                    INSERT INTO UserNotifications (UserID, NotificationType, Message, IsRead, CreatedDate)
                    VALUES (@UserID, @NotificationType, @Message, 0, GETDATE())";

                using (SqlCommand cmd = new SqlCommand(insertQuery, conn))
                {
                    cmd.Parameters.AddWithValue("@UserID", userId);
                    cmd.Parameters.AddWithValue("@NotificationType", notificationType);
                    cmd.Parameters.AddWithValue("@Message", message);
                    cmd.ExecuteNonQuery();
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Trace.WriteLine("LogNotification Error: " + ex.ToString());
                // Don't throw - notification logging should not break main functionality
            }
            finally
            {
                if (shouldCloseConnection && conn != null)
                {
                    conn.Close();
                }
            }
        }

        /// <summary>
        /// Sends an email notification to a user.
        /// </summary>
        /// <param name="toEmail">Recipient email address</param>
        /// <param name="subject">Email subject</param>
        /// <param name="bodyHtml">HTML body content</param>
        /// <param name="userId">Optional user ID for logging</param>
        public static void SendEmailNotification(string toEmail, string subject, string bodyHtml, int? userId = null)
        {
            try
            {
                string fromEmail = ConfigurationManager.AppSettings["SmtpFromEmail"] ?? "noreply@chordal.com";
                string smtpHost = ConfigurationManager.AppSettings["SmtpHost"] ?? "smtp.gmail.com";
                int smtpPort = int.Parse(ConfigurationManager.AppSettings["SmtpPort"] ?? "587");
                string smtpUsername = ConfigurationManager.AppSettings["SmtpUsername"] ?? "";
                string smtpPassword = ConfigurationManager.AppSettings["SmtpPassword"] ?? "";

                MailMessage mail = new MailMessage();
                mail.From = new MailAddress(fromEmail, "Chordal");
                mail.To.Add(toEmail);
                mail.Subject = subject;
                mail.Body = bodyHtml;
                mail.IsBodyHtml = true;

                SmtpClient smtp = new SmtpClient(smtpHost, smtpPort);
                smtp.EnableSsl = true;
                smtp.UseDefaultCredentials = false;
                smtp.Credentials = new NetworkCredential(smtpUsername, smtpPassword);

                // For development: log instead of sending
                if (string.IsNullOrEmpty(smtpUsername))
                {
                    System.Diagnostics.Trace.WriteLine("=== EMAIL NOTIFICATION ===");
                    System.Diagnostics.Trace.WriteLine("To: " + toEmail);
                    System.Diagnostics.Trace.WriteLine("Subject: " + subject);
                    System.Diagnostics.Trace.WriteLine("Body: " + bodyHtml);
                    System.Diagnostics.Trace.WriteLine("========================");
                }
                else
                {
                    smtp.Send(mail);
                }

                // Log to database if userId provided
                if (userId.HasValue)
                {
                    LogNotification(userId.Value, "Email Sent", $"Email notification sent: {subject}");
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Trace.WriteLine("SendEmailNotification Error: " + ex.ToString());
            }
        }

        /// <summary>
        /// Marks a notification as read.
        /// </summary>
        /// <param name="notificationId">The notification ID to mark as read</param>
        public static void MarkAsRead(int notificationId)
        {
            string connectionString = ConfigurationManager.ConnectionStrings["ConnectionString"].ConnectionString;

            try
            {
                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    conn.Open();
                    string updateQuery = "UPDATE UserNotifications SET IsRead = 1 WHERE NotificationID = @NotificationID";

                    using (SqlCommand cmd = new SqlCommand(updateQuery, conn))
                    {
                        cmd.Parameters.AddWithValue("@NotificationID", notificationId);
                        cmd.ExecuteNonQuery();
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Trace.WriteLine("MarkAsRead Error: " + ex.ToString());
            }
        }

        /// <summary>
        /// Gets the count of unread notifications for a user.
        /// </summary>
        /// <param name="userId">The user ID</param>
        /// <returns>Number of unread notifications</returns>
        public static int GetUnreadCount(int userId)
        {
            string connectionString = ConfigurationManager.ConnectionStrings["ConnectionString"].ConnectionString;

            try
            {
                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    conn.Open();
                    string query = "SELECT COUNT(*) FROM UserNotifications WHERE UserID = @UserID AND IsRead = 0";

                    using (SqlCommand cmd = new SqlCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@UserID", userId);
                        return (int)cmd.ExecuteScalar();
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Trace.WriteLine("GetUnreadCount Error: " + ex.ToString());
                return 0;
            }
        }

        /// <summary>
        /// Notification types for categorization.
        /// </summary>
        public static class NotificationTypes
        {
            public const string Account = "Account";
            public const string Community = "Community";
            public const string System = "System";
            public const string PasswordReset = "Password Reset";
            public const string ProgressionShared = "Progression Shared";
            public const string Comment = "Comment";
        }
    }
}