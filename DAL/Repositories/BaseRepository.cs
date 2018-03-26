using Serilog;

namespace AquariumMonitor.DAL.Interfaces
{
    public abstract class BaseRepository
    {
        protected readonly IConnectionFactory _connectionFactory;
        protected readonly ILogger _logger;

        public BaseRepository(IConnectionFactory connectionFactory, ILogger logger)
        {
            _connectionFactory = connectionFactory;
            _logger = logger;
        }
    }
}
