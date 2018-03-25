using System;
using System.Collections.Generic;
using System.Linq;
using Dapper;
using AquariumMonitor.DAL.Interfaces;
using AquariumMonitor.Models;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace AquariumMonitor.DAL
{
    public class WaterChangeRepository : BaseRepository, IWaterChangeRepository
    {
        private const string GetByAllForAquariumQuery = @"SELECT wc.Id,AquariumId,PercentageChanged,Changed,a.UserId,wc.[RowVersion]
                                                        FROM WaterChanges wc
                                                        JOIN Aquariums a ON a.Id = wc.AquariumId
                                                        WHERE wc.[AquariumId] = @aquariumId
                                                        AND (wc.Deleted IS NULL OR wc.Deleted = 0);";
        private const string GetQuery = @"SELECT wc.Id,AquariumId,PercentageChanged,Changed,a.UserId,wc.[RowVersion]
                                        FROM WaterChanges wc
                                        JOIN Aquariums a ON a.Id = wc.AquariumId
                                        AND wc.Id = @id
                                        AND (wc.Deleted IS NULL OR wc.Deleted = 0);";

        private const string InsertQuery = @"INSERT INTO WaterChanges (UserId,AquariumId,PercentageChanged,Changed)
                                             VALUES (@UserId,@AquariumId,@PercentageChanged,@Changed);
                                             SELECT Id, RowVersion FROM WaterChanges WHERE Id = CAST(SCOPE_IDENTITY() AS INT);";

        private const string UpdateQuery = @"UPDATE WaterChanges SET PercentageChanged = @PercentageChanged,Changed = @Changed
                                             WHERE Id = @Id;
                                             SELECT RowVersion FROM WaterChanges WHERE Id = @Id";

        private const string DeleteQuery = @"UPDATE WaterChanges SET Deleted = 1 WHERE Id = @Id;";

        public WaterChangeRepository(IConnectionFactory connectionFactory,
            ILogger<WaterChangeRepository> logger) : base(connectionFactory, logger)
        {
            
        }

        public async Task Add(WaterChange waterChange)
        {
            using (var connection = _connectionFactory.GetOpenConnection())
            {
                var result = await connection.QueryFirstAsync(InsertQuery, waterChange);

                waterChange.Id = result.Id;
                waterChange.RowVersion = result.RowVersion;
            }
        }

        public async Task<WaterChange> Get(int Id)
        {
            WaterChange waterChange = null;

            using (var connection = _connectionFactory.GetOpenConnection())
            {
                waterChange = await connection.QueryFirstOrDefaultAsync<WaterChange>(GetQuery, new { Id });
            }
            return waterChange;
        }

        public async Task<List<WaterChange>> GetForAquarium(int userId, int aquariumId)
        {
            var waterChanges = new List<WaterChange>();

            using (var connection = _connectionFactory.GetOpenConnection())
            {
                waterChanges = (await connection.QueryAsync<WaterChange>(GetByAllForAquariumQuery, new { aquariumId })).ToList();
            }
            return waterChanges;
        }

        public async Task Update(WaterChange waterChange)
        {
            using (var connection = _connectionFactory.GetOpenConnection())
            {
                waterChange.RowVersion = await connection.QueryFirstAsync<byte[]>(UpdateQuery, waterChange);
            }
        }

        public async Task Delete(int Id)
        {
            using (var connection = _connectionFactory.GetOpenConnection())
            {
                await connection.ExecuteAsync(DeleteQuery, new { Id });
            }
        }
    }
}
