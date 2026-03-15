using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using Microsoft.EntityFrameworkCore;
using Persistance.Databases;
using DomainLayer.Models;

namespace El7erafe.Functions
{
    public class CleanupServiceRequests
    {
        private readonly ApplicationDbContext _dbContext;
        private readonly ILogger _logger;

        public CleanupServiceRequests(ApplicationDbContext dbContext, ILoggerFactory loggerFactory)
        {
            _dbContext = dbContext;
            _logger = loggerFactory.CreateLogger<CleanupServiceRequests>();
        }

        [Function("CleanupExpiredRequests")]
        public async Task Run([TimerTrigger("0 */2 * * * *")] TimerInfo myTimer)
        {
            TimeZoneInfo egyptZone = TimeZoneInfo.FindSystemTimeZoneById("Egypt Standard Time");
            DateTime currentTime = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, egyptZone);

            _logger.LogInformation("=====================================================");
            _logger.LogInformation($"CLEANUP START: Local Time is {currentTime:yyyy-MM-dd HH:mm:ss}");

            // 1. Optimization: Only fetch requests that aren't already marked as expired (Status != 2)
            var pendingRequests = await _dbContext.ServiceRequests
                .Where(r => (int)r.Status != 2)
                .ToListAsync();

            _logger.LogInformation($"Found {pendingRequests.Count} active requests to check.");

            int updateCount = 0;

            foreach (var req in pendingRequests)
            {
                DateTime expirationTime;
                DateTime baseDate = req.ServiceDate.ToDateTime(TimeOnly.MinValue);

                if (!req.AvailableTo.HasValue)
                {
                    expirationTime = baseDate.AddDays(1).AddTicks(-1);
                }
                else
                {
                    // Handle midnight crossing (e.g., Starts 10PM, Ends 2AM)
                    if (req.AvailableFrom.HasValue && req.AvailableTo.Value < req.AvailableFrom.Value)
                    {
                        expirationTime = baseDate.AddDays(1).Add(req.AvailableTo.Value.ToTimeSpan());
                    }
                    else
                    {
                        expirationTime = baseDate.Add(req.AvailableTo.Value.ToTimeSpan());
                    }
                }

                bool isExpired = expirationTime < currentTime;

                // --- DEBUG LOGGING ---
                _logger.LogInformation($"ID: {req.Id} | Expiration: {expirationTime:yyyy-MM-dd HH:mm:ss} | Expired? {isExpired}");

                if (isExpired)
                {
                    // 2. Instead of deleting, we change the status to 2
                    req.Status = (DomainLayer.Models.IdentityModule.Enums.ServiceReqStatus)2;
                    updateCount++;
                }
            }

            // 3. Save the updates to Azure SQL
            if (updateCount > 0)
            {
                _logger.LogWarning($"ACTION: Marking {updateCount} requests as Expired (Status 2).");
                await _dbContext.SaveChangesAsync();
                _logger.LogInformation("Database updated successfully.");
            }
            else
            {
                _logger.LogInformation("No new expired requests found.");
            }

            _logger.LogInformation("CLEANUP FINISHED.");
            _logger.LogInformation("=====================================================");
        }
    }
}