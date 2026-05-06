using System.Collections.Generic;
using System.Data.SQLite;
using System.Linq;
using Dapper;
using DailyMeal.Model;

namespace DailyMeal.DAL
{
    public class SelectionGroupDAL
    {
        private BaseDAL _base = new BaseDAL();

        public List<SelectionGroup> GetAll()
        {
            using (var conn = _base.GetConnection())
            {
                conn.Open();
                return conn.Query<SelectionGroup>("SELECT * FROM SelectionGroup ORDER BY IsSystem DESC, Id").AsList();
            }
        }

        public List<SelectionGroup> GetByIsSystem(bool isSystem)
        {
            using (var conn = _base.GetConnection())
            {
                conn.Open();
                return conn.Query<SelectionGroup>("SELECT * FROM SelectionGroup WHERE IsSystem = @IsSystem ORDER BY Id", new { IsSystem = isSystem }).AsList();
            }
        }

        public SelectionGroup GetByCanteenName(string canteenName)
        {
            using (var conn = _base.GetConnection())
            {
                conn.Open();
                return conn.QueryFirstOrDefault<SelectionGroup>("SELECT * FROM SelectionGroup WHERE GroupName = @GroupName AND IsSystem = 1", new { GroupName = canteenName });
            }
        }

        public bool HasBuiltinGroups()
        {
            using (var conn = _base.GetConnection())
            {
                conn.Open();
                return conn.ExecuteScalar<int>("SELECT COUNT(*) FROM SelectionGroup WHERE IsSystem = 1") > 0;
            }
        }

        public SelectionGroup GetById(int id)
        {
            using (var conn = _base.GetConnection())
            {
                conn.Open();
                return conn.QueryFirstOrDefault<SelectionGroup>("SELECT * FROM SelectionGroup WHERE Id = @Id", new { Id = id });
            }
        }

        public int Insert(SelectionGroup group)
        {
            using (var conn = _base.GetConnection())
            {
                conn.Open();
                return conn.ExecuteScalar<int>("INSERT INTO SelectionGroup (GroupName, IsSystem) VALUES (@GroupName, @IsSystem); SELECT last_insert_rowid();", group);
            }
        }

        public void Update(SelectionGroup group)
        {
            using (var conn = _base.GetConnection())
            {
                conn.Open();
                conn.Execute("UPDATE SelectionGroup SET GroupName = @GroupName WHERE Id = @Id", group);
            }
        }

        public void Delete(int id)
        {
            using (var conn = _base.GetConnection())
            {
                conn.Open();
                conn.Execute("DELETE FROM SelectionGroupStall WHERE GroupId = @Id", new { Id = id });
                conn.Execute("DELETE FROM SelectionGroup WHERE Id = @Id", new { Id = id });
            }
        }

        public List<int> GetStallIdsByGroupId(int groupId)
        {
            using (var conn = _base.GetConnection())
            {
                conn.Open();
                return conn.Query<int>("SELECT StallId FROM SelectionGroupStall WHERE GroupId = @GroupId", new { GroupId = groupId }).AsList();
            }
        }

        public void SaveGroupStalls(int groupId, List<int> stallIds)
        {
            using (var conn = _base.GetConnection())
            {
                conn.Open();
                conn.Execute("DELETE FROM SelectionGroupStall WHERE GroupId = @GroupId", new { GroupId = groupId });
                foreach (var stallId in stallIds)
                {
                    conn.Execute("INSERT OR IGNORE INTO SelectionGroupStall (GroupId, StallId) VALUES (@GroupId, @StallId)", new { GroupId = groupId, StallId = stallId });
                }
            }
        }

        public void DeleteGroupStallsByStallIds(List<int> stallIds, SQLiteConnection conn, SQLiteTransaction trans)
        {
            if (stallIds.Count == 0) return;
            conn.Execute("DELETE FROM SelectionGroupStall WHERE StallId IN @StallIds", new { StallIds = stallIds }, trans);
        }

        public void DeleteByCanteenName(string canteenName, SQLiteConnection conn, SQLiteTransaction trans)
        {
            var groupId = conn.QueryFirstOrDefault<int?>("SELECT Id FROM SelectionGroup WHERE GroupName = @GroupName AND IsSystem = 1", new { GroupName = canteenName }, trans);
            if (groupId.HasValue)
            {
                conn.Execute("DELETE FROM SelectionGroupStall WHERE GroupId = @GroupId", new { GroupId = groupId.Value }, trans);
                conn.Execute("DELETE FROM SelectionGroup WHERE Id = @Id", new { Id = groupId.Value }, trans);
            }
        }

        public void DeleteGroupStallsByGroupId(int groupId, SQLiteConnection conn, SQLiteTransaction trans)
        {
            conn.Execute("DELETE FROM SelectionGroupStall WHERE GroupId = @GroupId", new { GroupId = groupId }, trans);
        }

        public void AddStallToGroup(int groupId, int stallId)
        {
            using (var conn = _base.GetConnection())
            {
                conn.Open();
                conn.Execute("INSERT OR IGNORE INTO SelectionGroupStall (GroupId, StallId) VALUES (@GroupId, @StallId)", new { GroupId = groupId, StallId = stallId });
            }
        }

        public int GetCountByStallIds(List<int> stallIds)
        {
            if (stallIds.Count == 0) return 0;
            using (var conn = _base.GetConnection())
            {
                conn.Open();
                return conn.ExecuteScalar<int>("SELECT COUNT(*) FROM SelectionGroupStall WHERE StallId IN @StallIds", new { StallIds = stallIds });
            }
        }
    }
}
