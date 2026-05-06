using System;
using System.Collections.Generic;
using System.Data.SQLite;
using Dapper;
using DailyMeal.Model;

namespace DailyMeal.DAL
{
    public class MealRecordDAL
    {
        private BaseDAL _base = new BaseDAL();

        public List<MealRecord> GetAll()
        {
            using (var conn = _base.GetConnection())
            {
                conn.Open();
                return conn.Query<MealRecord>(@"SELECT r.*, s.StallName, s.CanteenId, c.CanteenName, m.MealName 
                    FROM MealRecord r 
                    LEFT JOIN Stall s ON r.StallId = s.Id 
                    LEFT JOIN Canteen c ON s.CanteenId = c.Id 
                    LEFT JOIN Meal m ON r.MealId = m.Id 
                    ORDER BY r.RecordTime DESC").AsList();
            }
        }

        public List<MealRecord> GetByDateRange(DateTime start, DateTime end)
        {
            using (var conn = _base.GetConnection())
            {
                conn.Open();
                return conn.Query<MealRecord>(@"SELECT r.*, s.StallName, s.CanteenId, c.CanteenName, m.MealName 
                    FROM MealRecord r 
                    LEFT JOIN Stall s ON r.StallId = s.Id 
                    LEFT JOIN Canteen c ON s.CanteenId = c.Id 
                    LEFT JOIN Meal m ON r.MealId = m.Id 
                    WHERE r.RecordTime >= @Start AND r.RecordTime < @End 
                    ORDER BY r.RecordTime DESC", new { Start = start, End = end }).AsList();
            }
        }

        public List<MealRecord> GetByStallIds(List<int> stallIds)
        {
            using (var conn = _base.GetConnection())
            {
                conn.Open();
                return conn.Query<MealRecord>(@"SELECT r.*, s.StallName, s.CanteenId, c.CanteenName, m.MealName 
                    FROM MealRecord r 
                    LEFT JOIN Stall s ON r.StallId = s.Id 
                    LEFT JOIN Canteen c ON s.CanteenId = c.Id 
                    LEFT JOIN Meal m ON r.MealId = m.Id 
                    WHERE r.StallId IN @StallIds 
                    ORDER BY r.RecordTime DESC", new { StallIds = stallIds }).AsList();
            }
        }

        public MealRecord GetById(int id)
        {
            using (var conn = _base.GetConnection())
            {
                conn.Open();
                return conn.QueryFirstOrDefault<MealRecord>(@"SELECT r.*, s.StallName, s.CanteenId, c.CanteenName, m.MealName 
                    FROM MealRecord r 
                    LEFT JOIN Stall s ON r.StallId = s.Id 
                    LEFT JOIN Canteen c ON s.CanteenId = c.Id 
                    LEFT JOIN Meal m ON r.MealId = m.Id 
                    WHERE r.Id = @Id", new { Id = id });
            }
        }

        public int Insert(MealRecord record)
        {
            using (var conn = _base.GetConnection())
            {
                conn.Open();
                return conn.ExecuteScalar<int>("INSERT INTO MealRecord (StallId, MealId, RecordTime, Calorie, Price, Remark) VALUES (@StallId, @MealId, @RecordTime, @Calorie, @Price, @Remark); SELECT last_insert_rowid();", record);
            }
        }

        public void Update(MealRecord record)
        {
            using (var conn = _base.GetConnection())
            {
                conn.Open();
                conn.Execute("UPDATE MealRecord SET StallId = @StallId, MealId = @MealId, RecordTime = @RecordTime, Calorie = @Calorie, Price = @Price, Remark = @Remark WHERE Id = @Id", record);
            }
        }

        public void Delete(int id)
        {
            using (var conn = _base.GetConnection())
            {
                conn.Open();
                conn.Execute("DELETE FROM MealRecord WHERE Id = @Id", new { Id = id });
            }
        }

        public void DeleteByStallIds(List<int> stallIds, SQLiteConnection conn, SQLiteTransaction trans)
        {
            conn.Execute("DELETE FROM MealRecord WHERE StallId IN @StallIds", new { StallIds = stallIds }, trans);
        }

        public int GetCountByStallIds(List<int> stallIds)
        {
            using (var conn = _base.GetConnection())
            {
                conn.Open();
                return conn.ExecuteScalar<int>("SELECT COUNT(*) FROM MealRecord WHERE StallId IN @StallIds", new { StallIds = stallIds });
            }
        }

        public List<MealRecord> GetTodayRecords()
        {
            var today = DateTime.Today;
            var tomorrow = today.AddDays(1);
            return GetByDateRange(today, tomorrow);
        }

        public List<int> GetRecordIdsByStallIds(List<int> stallIds)
        {
            using (var conn = _base.GetConnection())
            {
                conn.Open();
                return conn.Query<int>("SELECT Id FROM MealRecord WHERE StallId IN @StallIds", new { StallIds = stallIds }).AsList();
            }
        }
    }
}
