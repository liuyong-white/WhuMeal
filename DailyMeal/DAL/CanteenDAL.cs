using System.Collections.Generic;
using System.Data.SQLite;
using Dapper;
using DailyMeal.Model;

namespace DailyMeal.DAL
{
    public class CanteenDAL
    {
        private BaseDAL _base = new BaseDAL();

        public List<Canteen> GetAll()
        {
            using (var conn = _base.GetConnection())
            {
                conn.Open();
                return conn.Query<Canteen>("SELECT * FROM Canteen ORDER BY Id").AsList();
            }
        }

        public Canteen GetById(int id)
        {
            using (var conn = _base.GetConnection())
            {
                conn.Open();
                return conn.QueryFirstOrDefault<Canteen>("SELECT * FROM Canteen WHERE Id = @Id", new { Id = id });
            }
        }

        public int Insert(Canteen canteen)
        {
            using (var conn = _base.GetConnection())
            {
                conn.Open();
                return conn.ExecuteScalar<int>("INSERT INTO Canteen (CanteenName, IsSystem) VALUES (@CanteenName, @IsSystem); SELECT last_insert_rowid();", canteen);
            }
        }

        public void Update(Canteen canteen)
        {
            using (var conn = _base.GetConnection())
            {
                conn.Open();
                conn.Execute("UPDATE Canteen SET CanteenName = @CanteenName, IsSystem = @IsSystem WHERE Id = @Id", canteen);
            }
        }

        public void Delete(int id)
        {
            using (var conn = _base.GetConnection())
            {
                conn.Open();
                conn.Execute("DELETE FROM Canteen WHERE Id = @Id", new { Id = id });
            }
        }

        public bool HasSystemData()
        {
            using (var conn = _base.GetConnection())
            {
                conn.Open();
                return conn.ExecuteScalar<bool>("SELECT COUNT(*) > 0 FROM Canteen WHERE IsSystem = 1");
            }
        }
    }
}
