using System.Collections.Generic;
using System.Data.SQLite;
using Dapper;
using DailyMeal.Model;

namespace DailyMeal.DAL
{
    public class MealDAL
    {
        private BaseDAL _base = new BaseDAL();

        public List<Meal> GetAll()
        {
            using (var conn = _base.GetConnection())
            {
                conn.Open();
                return conn.Query<Meal>("SELECT m.*, s.StallName FROM Meal m LEFT JOIN Stall s ON m.StallId = s.Id ORDER BY m.Id").AsList();
            }
        }

        public List<Meal> GetByStallId(int stallId)
        {
            using (var conn = _base.GetConnection())
            {
                conn.Open();
                return conn.Query<Meal>("SELECT m.*, s.StallName FROM Meal m LEFT JOIN Stall s ON m.StallId = s.Id WHERE m.StallId = @StallId ORDER BY m.Id", new { StallId = stallId }).AsList();
            }
        }

        public Meal GetById(int id)
        {
            using (var conn = _base.GetConnection())
            {
                conn.Open();
                return conn.QueryFirstOrDefault<Meal>("SELECT m.*, s.StallName FROM Meal m LEFT JOIN Stall s ON m.StallId = s.Id WHERE m.Id = @Id", new { Id = id });
            }
        }

        public int Insert(Meal meal)
        {
            using (var conn = _base.GetConnection())
            {
                conn.Open();
                return conn.ExecuteScalar<int>("INSERT INTO Meal (MealName, StallId, Calorie, Price, Remark, IsSystem) VALUES (@MealName, @StallId, @Calorie, @Price, @Remark, @IsSystem); SELECT last_insert_rowid();", meal);
            }
        }

        public void Update(Meal meal)
        {
            using (var conn = _base.GetConnection())
            {
                conn.Open();
                conn.Execute("UPDATE Meal SET MealName = @MealName, StallId = @StallId, Calorie = @Calorie, Price = @Price, Remark = @Remark, IsSystem = @IsSystem WHERE Id = @Id", meal);
            }
        }

        public void Delete(int id)
        {
            using (var conn = _base.GetConnection())
            {
                conn.Open();
                conn.Execute("DELETE FROM Meal WHERE Id = @Id", new { Id = id });
            }
        }

        public void NullifyMealIdByStallIds(List<int> stallIds, SQLiteConnection conn, SQLiteTransaction trans)
        {
            conn.Execute("UPDATE MealRecord SET MealId = NULL WHERE StallId IN @StallIds", new { StallIds = stallIds }, trans);
        }

        public int GetCountByStallId(int stallId)
        {
            using (var conn = _base.GetConnection())
            {
                conn.Open();
                return conn.ExecuteScalar<int>("SELECT COUNT(*) FROM Meal WHERE StallId = @StallId", new { StallId = stallId });
            }
        }

        public List<int> GetMealIdsByStallIds(List<int> stallIds)
        {
            using (var conn = _base.GetConnection())
            {
                conn.Open();
                return conn.Query<int>("SELECT Id FROM Meal WHERE StallId IN @StallIds", new { StallIds = stallIds }).AsList();
            }
        }
    }
}
