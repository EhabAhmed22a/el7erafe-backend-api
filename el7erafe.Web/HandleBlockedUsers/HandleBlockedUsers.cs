using System;
using System.Data.SqlClient;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;

namespace El7erafe.Functions
{
    public class HandleBlockedUsers
    {
        // Add a private field to hold the logger instance
        private readonly ILogger<HandleBlockedUsers> _logger;

        // Add a constructor that takes the logger via DI
        public HandleBlockedUsers(ILogger<HandleBlockedUsers> logger)
        {
            _logger = logger;
        }

        [Function("HandleBlockedUsers")]
        // Remove the ILogger parameter 'log' from here
        public void Run([TimerTrigger("0 */1 * * * *")] TimerInfo myTimer)
        {
            // Use the private field _logger instead of the 'log' parameter
            _logger.LogInformation($"Function started at: {DateTime.UtcNow}");

            string connStr = Environment.GetEnvironmentVariable("SqlConnection")!;

            if (string.IsNullOrEmpty(connStr))
            {
                _logger.LogError("ERROR: SqlConnection environment variable is empty!");
                return;
            }

            _logger.LogInformation($"Connection string obtained successfully.");

            try
            {
                using (SqlConnection conn = new SqlConnection(connStr))
                {
                    conn.Open();
                    _logger.LogInformation("Successfully connected to database!");

                    using (SqlCommand cmd = new SqlCommand("DELETE FROM BlockedUsers WHERE Id = 15", conn))
                    {
                        int rowsAffected = cmd.ExecuteNonQuery();
                        _logger.LogInformation($"Deleted {rowsAffected} record(s)");
                    }
                }

                _logger.LogInformation("Function completed successfully!");
            }
            catch (SqlException ex)
            {
                _logger.LogError($"SQL Error #{ex.Number}: {ex.Message}");
                _logger.LogError(ex.ToString());
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error: {ex.Message}");
                _logger.LogError(ex.ToString());
            }
        }
    }
}
