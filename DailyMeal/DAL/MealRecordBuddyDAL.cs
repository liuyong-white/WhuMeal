using System.Collections.Generic;
using System.Data.SQLite;
using Dapper;
using DailyMeal.Model;

namespace DailyMeal.DAL
{
    public class MealRecordBuddyDAL
    {
        private BaseDAL _base = new BaseDAL();

        public List<MealRecordBuddy> GetByRecordId(int recordId)
        {
            using (var conn = _base.GetConnection())
            {
                conn.Open();
                return conn.Query<MealRecordBuddy>(@"SELECT mrb.*, db.Name AS BuddyName 
                    FROM MealRecordBuddy mrb 
                    LEFT JOIN DinnerBuddy db ON mrb.BuddyId = db.Id 
                    WHERE mrb.RecordId = @RecordId", new { RecordId = recordId }).AsList();
            }
        }

        public List<MealRecordBuddy> GetByBuddyId(int buddyId)
        {
            using (var conn = _base.GetConnection())
            {
                conn.Open();
                return conn.Query<MealRecordBuddy>(@"SELECT mrb.*, db.Name AS BuddyName 
                    FROM MealRecordBuddy mrb 
                    LEFT JOIN DinnerBuddy db ON mrb.BuddyId = db.Id 
                    WHERE mrb.BuddyId = @BuddyId", new { BuddyId = buddyId }).AsList();
            }
        }

        public void InsertBatch(int recordId, List<int> buddyIds)
        {
            using (var conn = _base.GetConnection())
            {
                conn.Open();
                foreach (var buddyId in buddyIds)
                {
                    conn.Execute("INSERT OR IGNORE INTO MealRecordBuddy (RecordId, BuddyId) VALUES (@RecordId, @BuddyId)", new { RecordId = recordId, BuddyId = buddyId });
                }
            }
        }

        public void DeleteByBuddyId(int buddyId)
        {
            using (var conn = _base.GetConnection())
            {
                conn.Open();
                conn.Execute("DELETE FROM MealRecordBuddy WHERE BuddyId = @BuddyId", new { BuddyId = buddyId });
            }
        }

        public void DeleteByRecordIds(List<int> recordIds, SQLiteConnection conn, SQLiteTransaction trans)
        {
            if (recordIds.Count == 0) return;
            conn.Execute("DELETE FROM MealRecordBuddy WHERE RecordId IN @RecordIds", new { RecordIds = recordIds }, trans);
        }

        public int GetCountByRecordIds(List<int> recordIds)
        {
            if (recordIds.Count == 0) return 0;
            using (var conn = _base.GetConnection())
            {
                conn.Open();
                return conn.ExecuteScalar<int>("SELECT COUNT(*) FROM MealRecordBuddy WHERE RecordId IN @RecordIds", new { RecordIds = recordIds });
            }
        }

        public int GetCountByBuddyId(int buddyId)
        {
            using (var conn = _base.GetConnection())
            {
                conn.Open();
                return conn.ExecuteScalar<int>("SELECT COUNT(*) FROM MealRecordBuddy WHERE BuddyId = @BuddyId", new { BuddyId = buddyId });
            }
        }
    }
}
