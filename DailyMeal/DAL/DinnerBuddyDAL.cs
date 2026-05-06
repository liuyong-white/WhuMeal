using System.Collections.Generic;
using System.Data.SQLite;
using Dapper;
using DailyMeal.Model;

namespace DailyMeal.DAL
{
    public class DinnerBuddyDAL
    {
        private BaseDAL _base = new BaseDAL();

        public List<DinnerBuddy> GetAll()
        {
            using (var conn = _base.GetConnection())
            {
                conn.Open();
                return conn.Query<DinnerBuddy>("SELECT * FROM DinnerBuddy ORDER BY Id").AsList();
            }
        }

        public DinnerBuddy GetById(int id)
        {
            using (var conn = _base.GetConnection())
            {
                conn.Open();
                return conn.QueryFirstOrDefault<DinnerBuddy>("SELECT * FROM DinnerBuddy WHERE Id = @Id", new { Id = id });
            }
        }

        public int Insert(DinnerBuddy buddy)
        {
            using (var conn = _base.GetConnection())
            {
                conn.Open();
                return conn.ExecuteScalar<int>("INSERT INTO DinnerBuddy (Name, Photo, IsSystem) VALUES (@Name, @Photo, @IsSystem); SELECT last_insert_rowid();", buddy);
            }
        }

        public void Update(DinnerBuddy buddy)
        {
            using (var conn = _base.GetConnection())
            {
                conn.Open();
                conn.Execute("UPDATE DinnerBuddy SET Name = @Name, Photo = @Photo, IsSystem = @IsSystem WHERE Id = @Id", buddy);
            }
        }

        public void Delete(int id)
        {
            using (var conn = _base.GetConnection())
            {
                conn.Open();
                conn.Execute("DELETE FROM DinnerBuddy WHERE Id = @Id", new { Id = id });
            }
        }

        public void EnsureSelfBuddyExists()
        {
            using (var conn = _base.GetConnection())
            {
                conn.Open();
                var exists = conn.ExecuteScalar<bool>("SELECT COUNT(*) > 0 FROM DinnerBuddy WHERE IsSystem = 1 AND Name = '自己'");
                if (!exists)
                {
                    conn.Execute("INSERT INTO DinnerBuddy (Name, Photo, IsSystem) VALUES ('自己', '', 1)");
                }
            }
        }

        public bool IsSelfBuddy(int id)
        {
            using (var conn = _base.GetConnection())
            {
                conn.Open();
                return conn.ExecuteScalar<bool>("SELECT COUNT(*) > 0 FROM DinnerBuddy WHERE Id = @Id AND IsSystem = 1 AND Name = '自己'", new { Id = id });
            }
        }
    }
}
