using Dapper;
using AquariumMonitor.DAL.Interfaces;
using AquariumMonitor.Models;
using System.Threading.Tasks;
using System.Collections.Generic;
using Serilog;

namespace AquariumMonitor.DAL
{
    public class AquariumTypeRepository : BaseRepository, IAquariumTypeRepository
    {
        private const string GetByNameQuery = @"SELECT Id, [Name] FROM AquariumTypes WHERE [Name] = @Name;";

        private const string GetQuery = @"SELECT Id, [Name] FROM AquariumTypes;";

        public AquariumTypeRepository(IConnectionFactory connectionFactory,
            ILogger logger) : base (connectionFactory, logger)
        {
        }

        public async Task<AquariumType> Get(string name)
        {
            AquariumType aquariumType;

            using (var connection = _connectionFactory.GetOpenConnection())
            {
                aquariumType = await connection.QueryFirstOrDefaultAsync<AquariumType>(GetByNameQuery, new { name });
            }
            return aquariumType;
        }

        public async Task<List<AquariumType>> Get()
        {
            List<AquariumType> aquariumType;

            using (var connection = _connectionFactory.GetOpenConnection())
            {
                aquariumType = await connection.QueryFirstOrDefaultAsync<List<AquariumType>>(GetQuery);
            }
            return aquariumType;
        }
    }
}
