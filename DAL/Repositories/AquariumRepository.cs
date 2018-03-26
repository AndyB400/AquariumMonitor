using System;
using System.Collections.Generic;
using System.Linq;
using Dapper;
using AquariumMonitor.DAL.Interfaces;
using AquariumMonitor.Models;
using System.Threading.Tasks;
using Serilog;

namespace AquariumMonitor.DAL
{
    public class AquariumRepository : BaseRepository, IAquariumRepository
    {
        private const string GetAllForUserQuery = @"SELECT Id,Name,Created,Notes,UserId,OfficalVolume,ActualVolume,Width,Height,[Length], [RowVersion]
                                                    ,AquariumTypeId AS Id, AquariumTypeName As Name,DimesionUnitId As Id, DimesionUnitName As Name,
                                                    VolumeUnitId As Id, VolumeUnitName As Name
                                                    FROM vw_Aquarium a
                                                    WHERE UserId = @userId";
        private const string GetByIdForUserQuery = @"SELECT Id,Name,Created,Notes,UserId,OfficalVolume,ActualVolume,Width,Height,[Length], [RowVersion]
                                                    ,AquariumTypeId AS Id, AquariumTypeName As Name,DimesionUnitId As Id, DimesionUnitName As Name,
                                                    VolumeUnitId As Id, VolumeUnitName As Name
                                                    FROM vw_Aquarium a
                                                    WHERE UserId = @userId
                                                    AND Id = @id";      

        private const string InsertQuery = @"INSERT INTO Aquariums (Name,Notes,UserId,AquariumTypeId,OfficalVolume,ActualVolume,VolumeUnitId,Width,Height,[Length],DimesionUnitId)
                                            VALUES (@Name,@Notes,@UserId,@AquariumTypeId,@OfficalVolume,@ActualVolume,@VolumeUnitId,@Width,@Height,@Length,@DimesionUnitId);
                                            SELECT Id, RowVersion FROM Aquariums WHERE Id = CAST(SCOPE_IDENTITY() AS INT);";

        private const string DeleteQuery = @"UPDATE Aquariums SET Deleted = 1 WHERE Id = @Id;";

        private const string UpdateQuery = @"UPDATE Aquariums SET [Name] = @Name, Notes = @Notes, AquariumTypeId = @AquariumTypeId, OfficalVolume = @OfficalVolume,
                                            ActualVolume = @ActualVolume, VolumeUnitId = @VolumeUnitId, Width = @Width, Height = @Height, [Length] = @Length,
                                            DimesionUnitId = @DimesionUnitId
                                            WHERE Id = @Id;
                                            SELECT RowVersion FROM Aquariums WHERE Id = @Id";

        private string ExistsByIdQuery = "SELECT 'Exists' FROM Aquariums WHERE UserId = @userId AND Id = @id";
        private string ExistsByUsernameQuery = "SELECT 'Exists' FROM Aquariums a JOIN Users u ON a.UserId = u.Id WHERE u.Username = @username AND a.Id = @id";

        public AquariumRepository(IConnectionFactory connectionFactory,
            ILogger logger) : base (connectionFactory, logger)
        {
        }

        public async Task<Aquarium> Get(int userId, int id)
        {
            Aquarium aquarium;

            using (var connection = _connectionFactory.GetOpenConnection())
            {
                aquarium = (await connection.QueryAsync<Aquarium, AquariumType, Unit, Unit, Aquarium>(GetByIdForUserQuery, (a, at, dm, vm) =>
                {
                    a.Type = at;
                    a.DimensionUnit = dm;
                    a.VolumeUnit = vm;
                    a.User = new User { Id = userId };
                    return a;
                }, new { userId, id }
                , splitOn: "Id"
                )).FirstOrDefault();
            }
            return aquarium;
        }

        public async Task<List<Aquarium>> GetForUser(int userId)
        {
            var aquariums = new List<Aquarium>();

            using (var connection = _connectionFactory.GetOpenConnection())
            {
                aquariums = (await connection.QueryAsync<Aquarium, AquariumType, Unit, Unit, Aquarium>(GetAllForUserQuery, (a, at, dm, vm) =>
                {
                    a.Type = at;
                    a.DimensionUnit = dm;
                    a.VolumeUnit = vm;
                    a.User = new User { Id = userId };
                    return a;
                }, new { userId }
                , splitOn: "Id"
                )).ToList();
            }
            return aquariums;
        }

        public async Task Add(Aquarium aquarium)
        {
            using (var connection = _connectionFactory.GetOpenConnection())
            {
                var result = await connection.QueryFirstAsync(InsertQuery, new
                {
                    aquarium.Name,
                    aquarium.Notes,
                    UserId = aquarium.User.Id,
                    AquariumTypeId = aquarium.Type?.Id,
                    aquarium.OfficalVolume,
                    aquarium.ActualVolume,
                    VolumeUnitId = aquarium.VolumeUnit?.Id,
                    aquarium.Width,
                    aquarium.Height,
                    aquarium.Length,
                    DimesionUnitId = aquarium.DimensionUnit?.Id
                });

                aquarium.Id = result.Id;
                aquarium.RowVersion = result.RowVersion;
            }
        }

        public async Task Update(Aquarium aquarium)
        {
            using (var connection = _connectionFactory.GetOpenConnection())
            {
                aquarium.RowVersion = await connection.QueryFirstAsync<byte[]>(UpdateQuery, new { aquarium.Name,
                    aquarium.Notes,
                    UserId = aquarium.User.Id,
                    AquariumTypeId = aquarium.Type.Id,
                    aquarium.OfficalVolume,
                    aquarium.ActualVolume,
                    VolumeUnitId = aquarium.VolumeUnit?.Id,
                    aquarium.Width,
                    aquarium.Height,
                    aquarium.Length,
                    DimesionUnitId = aquarium.DimensionUnit?.Id,
                    aquarium.Id });
            }
        }

        public async Task Delete(int id)
        {
            using (var connection = _connectionFactory.GetOpenConnection())
            {
                await connection.ExecuteAsync(DeleteQuery, new { id });
            }
        }

        public async Task<bool> Exists(string username, int id)
        {
            string exists;

            using (var connection = _connectionFactory.GetOpenConnection())
            {
                exists = await connection.QueryFirstOrDefaultAsync<string>(ExistsByUsernameQuery, new { username, id });
            }
            return exists != null ? true : false;
        }

        public async Task<bool> Exists(int userId, int id)
        {
            string exists;

            using (var connection = _connectionFactory.GetOpenConnection())
            {
                exists = await connection.QueryFirstOrDefaultAsync<string>(ExistsByIdQuery, new { userId, id });
            }
            return exists != null ? true : false;
        }
    }
}
