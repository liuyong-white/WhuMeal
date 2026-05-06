using System.Collections.Generic;
using System.Data.SQLite;
using Dapper;
using DailyMeal.Model;

namespace DailyMeal.DAL
{
    public class StallDAL
    {
        private BaseDAL _base = new BaseDAL();

        public List<Stall> GetAll()
        {
            using (var conn = _base.GetConnection())
            {
                conn.Open();
                return conn.Query<Stall>("SELECT s.*, c.CanteenName FROM Stall s LEFT JOIN Canteen c ON s.CanteenId = c.Id ORDER BY s.Id").AsList();
            }
        }

        public List<Stall> GetByCanteenId(int canteenId)
        {
            using (var conn = _base.GetConnection())
            {
                conn.Open();
                return conn.Query<Stall>("SELECT s.*, c.CanteenName FROM Stall s LEFT JOIN Canteen c ON s.CanteenId = c.Id WHERE s.CanteenId = @CanteenId ORDER BY s.Id", new { CanteenId = canteenId }).AsList();
            }
        }

        public Stall GetById(int id)
        {
            using (var conn = _base.GetConnection())
            {
                conn.Open();
                return conn.QueryFirstOrDefault<Stall>("SELECT s.*, c.CanteenName FROM Stall s LEFT JOIN Canteen c ON s.CanteenId = c.Id WHERE s.Id = @Id", new { Id = id });
            }
        }

        public int Insert(Stall stall)
        {
            using (var conn = _base.GetConnection())
            {
                conn.Open();
                return conn.ExecuteScalar<int>("INSERT INTO Stall (StallName, CanteenId, StallPhoto, IsSystem) VALUES (@StallName, @CanteenId, @StallPhoto, @IsSystem); SELECT last_insert_rowid();", stall);
            }
        }

        public void Update(Stall stall)
        {
            using (var conn = _base.GetConnection())
            {
                conn.Open();
                conn.Execute("UPDATE Stall SET StallName = @StallName, CanteenId = @CanteenId, StallPhoto = @StallPhoto, IsSystem = @IsSystem WHERE Id = @Id", stall);
            }
        }

        public void Delete(int id)
        {
            using (var conn = _base.GetConnection())
            {
                conn.Open();
                conn.Execute("DELETE FROM Stall WHERE Id = @Id", new { Id = id });
            }
        }

        public void DeleteByCanteenId(int canteenId, SQLiteConnection conn, SQLiteTransaction trans)
        {
            conn.Execute("DELETE FROM Stall WHERE CanteenId = @CanteenId", new { CanteenId = canteenId }, trans);
        }

        public int GetCountByCanteenId(int canteenId)
        {
            using (var conn = _base.GetConnection())
            {
                conn.Open();
                return conn.ExecuteScalar<int>("SELECT COUNT(*) FROM Stall WHERE CanteenId = @CanteenId", new { CanteenId = canteenId });
            }
        }

        public List<int> GetStallIdsByCanteenId(int canteenId)
        {
            using (var conn = _base.GetConnection())
            {
                conn.Open();
                return conn.Query<int>("SELECT Id FROM Stall WHERE CanteenId = @CanteenId", new { CanteenId = canteenId }).AsList();
            }
        }
    }
}
