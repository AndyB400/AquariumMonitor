using System;
using System.Collections.Generic;
using System.Linq;
using Dapper;
using AquariumMonitor.DAL.Interfaces;
using AquariumMonitor.Models;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace AquariumMonitor.DAL
{
    public class MeasurementRepository : BaseRepository, IMeasurementRepository
    {
        private const string GetByAllForAquariumQuery = @"SELECT Id,UnitId,AquariumId,Value,Taken,UserId,[RowVersion]
                                                        FROM Measurements m
                                                        WHERE m.[AquariumId] = @aquariumId
                                                        AND m.UserId = @userId
                                                        AND (m.Deleted IS NULL OR m.Deleted = 0);";

        private const string GetQuery = @"SELECT m.Id,AquariumId,Value,Taken,UserId,U.Id AS UnitId,U.Name
                                        ,mt.Id AS MeasurementTypeId,mt.Name,m.[RowVersion]
                                        FROM Measurements m
                                        JOIN Units u ON m.UnitId = u.Id
                                        JOIN MeasurementTypes mt ON mt.Name = m.MeasurementType
                                        WHERE m.Id = @id
                                        AND (m.Deleted IS NULL OR m.Deleted = 0);";

        private const string InsertQuery = @"INSERT INTO Measurements (MeasurementTypeId,UnitId,AquariumId,Value,Taken,UserId)
                                             VALUES (@MeasurementTypeId,@UnitId,@AquariumId,@Value,@Taken,@UserId);
                                             SELECT Id, RowVersion FROM Measurements WHERE Id = CAST(SCOPE_IDENTITY() AS INT);";

        private const string UpdateQuery = @"UPDATE Measurements
                                            SET MeasurementTypeId = @MeasurementTypeId,
                                            UnitId = @UnitId,
                                            AquariumId = @AquariumId,
                                            Value = @Value,
                                            Taken = @Taken
                                            WHERE Id = @Id;
                                            SELECT RowVersion FROM Measurements WHERE Id = @Id;";

        private const string DeleteQuery = @"UPDATE Measurements SET Deleted = 1 WHERE Id = @Id;";

        public MeasurementRepository(IConnectionFactory connectionFactory,
            ILogger<MeasurementRepository> logger) : base(connectionFactory, logger)
        {

        }

        public async Task<bool> Add(Measurement measurement)
        {
            using (var connection = _connectionFactory.GetOpenConnection())
            {
                var result = await connection.QueryFirstAsync(InsertQuery, new {
                    UnitId = measurement.Unit.Id,
                    MeasurementTypeId = measurement.MeasurementType.Id,
                    measurement.AquariumId,
                    measurement.Value,
                    measurement.Taken,
                    measurement.UserId
                });

                measurement.Id = result.Id;
                measurement.RowVersion = result.RowVersion;
            }
            return true;
        }

        public async Task Update(Measurement measurement)
        {
            using (var connection = _connectionFactory.GetOpenConnection())
            {
                measurement.RowVersion = await connection.QueryFirstAsync<byte[]>(UpdateQuery, measurement);
            }
        }

        public async Task<Measurement> Get(int Id)
        {
            Measurement measurement = null;

            using (var connection = _connectionFactory.GetOpenConnection())
            {
                measurement = (await connection.QueryAsync<Measurement, Unit, MeasurementType, Measurement>(GetQuery, (m, u, mt) =>
                {
                    m.Unit = u;
                    m.MeasurementType = mt;
                    return m;
                }
                , new { Id }
                , splitOn: "UnitId,MeasurementTypeId"
                )).FirstOrDefault();
            }
            return measurement;
        }

        public async Task<List<Measurement>> GetForAquarium(int userId, int aquariumId)
        {
            var measurements = new List<Measurement>();

            using (var connection = _connectionFactory.GetOpenConnection())
            {
                measurements = (await connection.QueryAsync<Measurement>(GetByAllForAquariumQuery, new { userId, aquariumId })).ToList();
            }
            return measurements;
        }

        public async Task Delete(int id)
        {
            using (var connection = _connectionFactory.GetOpenConnection())
            {
                await connection.ExecuteAsync(DeleteQuery, new { id });
            }
        }
    }
}
