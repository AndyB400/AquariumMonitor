using System;
using BusinessLogic.Interfaces;
using Microsoft.Extensions.Logging;

namespace BusinessLogic.Adapters
{
    public class LoggerAdapter<T> : ILoggerAdapter<T>
    {
        private readonly ILogger<T> _logger;

        public LoggerAdapter(ILogger<T> logger)
        {
            _logger = logger;
        }

        public void Error(Exception ex, string message, params object[] args)
        {
            _logger.LogError(ex, message, args);
        }

        public void Information(string message)
        {
            _logger.LogInformation(message);
        }

        public void Warning(string message)
        {
            _logger.LogWarning(message);
        }
    }
}
