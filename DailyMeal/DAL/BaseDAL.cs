using System;
using System.Data.SQLite;
using System.IO;
using System.Linq;
using System.Configuration;
using Dapper;

namespace DailyMeal.DAL
{
    public class BaseDAL
    {
        private static readonly string DbFileName = "DailyMeal.db";
        private static string _dbPath;

        static BaseDAL()
        {
            _dbPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, DbFileName);
        }

        public SQLiteConnection GetConnection()
        {
            return new SQLiteConnection($"Data Source={_dbPath};Version=3;New=False;Compress=True;");
        }

        public void EnsureDatabaseInitialized()
        {
            if (!File.Exists(_dbPath))
            {
                SQLiteConnection.CreateFile(_dbPath);
            }

            using (var conn = GetConnection())
            {
                conn.Open();
                using (var cmd = conn.CreateCommand())
                {
                    cmd.CommandText = GetCreateTableSql();
                    cmd.ExecuteNonQuery();
                }
                using (var cmd = conn.CreateCommand())
                {
                    cmd.CommandText = GetCreateIndexSql();
                    cmd.ExecuteNonQuery();
                }
                MigrateDatabase(conn);
            }
        }

        private void MigrateDatabase(SQLiteConnection conn)
        {
            try
            {
                var columns = conn.Query<string>("SELECT name FROM pragma_table_info('SelectionGroup')").AsList();
                if (!columns.Contains("IsSystem"))
                {
                    conn.Execute("ALTER TABLE SelectionGroup ADD COLUMN IsSystem BOOLEAN NOT NULL DEFAULT 0");
                }
            }
            catch { }
        }

        public void EnsureSystemDataInitialized()
        {
            var canteenDal = new CanteenDAL();
            if (!canteenDal.HasSystemData())
            {
                ExecuteInTransaction((conn, trans) =>
                {
                    InsertSystemCanteens(conn, trans);
                    InsertSystemStalls(conn, trans);
                });
            }
            var buddyDal = new DinnerBuddyDAL();
            buddyDal.EnsureSelfBuddyExists();
            EnsureBuiltinGroups();
        }

        private void EnsureBuiltinGroups()
        {
            var groupDal = new SelectionGroupDAL();
            if (groupDal.HasBuiltinGroups()) return;
            var canteenDal = new CanteenDAL();
            var stallDal = new StallDAL();
            var canteens = canteenDal.GetAll();
            foreach (var canteen in canteens)
            {
                var group = new Model.SelectionGroup { GroupName = canteen.CanteenName, IsSystem = true };
                group.Id = groupDal.Insert(group);
                var stalls = stallDal.GetByCanteenId(canteen.Id);
                if (stalls.Count > 0)
                {
                    groupDal.SaveGroupStalls(group.Id, stalls.Select(s => s.Id).ToList());
                }
            }
        }

        public void ExecuteInTransaction(Action<SQLiteConnection, SQLiteTransaction> action)
        {
            using (var conn = GetConnection())
            {
                conn.Open();
                using (var trans = conn.BeginTransaction())
                {
                    try
                    {
                        action(conn, trans);
                        trans.Commit();
                    }
                    catch
                    {
                        trans.Rollback();
                        throw;
                    }
                }
            }
        }

        private string GetCreateTableSql()
        {
            return @"
CREATE TABLE IF NOT EXISTS Canteen (
    Id INTEGER PRIMARY KEY AUTOINCREMENT,
    CanteenName VARCHAR(50) NOT NULL,
    IsSystem BOOLEAN NOT NULL DEFAULT 0
);

CREATE TABLE IF NOT EXISTS Stall (
    Id INTEGER PRIMARY KEY AUTOINCREMENT,
    StallName VARCHAR(50) NOT NULL,
    CanteenId INTEGER NOT NULL,
    IsSystem BOOLEAN NOT NULL DEFAULT 0,
    FOREIGN KEY (CanteenId) REFERENCES Canteen(Id)
);

CREATE TABLE IF NOT EXISTS Meal (
    Id INTEGER PRIMARY KEY AUTOINCREMENT,
    MealName VARCHAR(50) DEFAULT '',
    StallId INTEGER NOT NULL,
    Calorie REAL NOT NULL,
    Price REAL NOT NULL,
    Remark VARCHAR(200) DEFAULT '',
    IsSystem BOOLEAN NOT NULL DEFAULT 0,
    FOREIGN KEY (StallId) REFERENCES Stall(Id)
);

CREATE TABLE IF NOT EXISTS DinnerBuddy (
    Id INTEGER PRIMARY KEY AUTOINCREMENT,
    Name VARCHAR(20) NOT NULL,
    Photo VARCHAR(200) DEFAULT '',
    IsSystem BOOLEAN NOT NULL DEFAULT 0
);

CREATE TABLE IF NOT EXISTS MealRecord (
    Id INTEGER PRIMARY KEY AUTOINCREMENT,
    StallId INTEGER NOT NULL,
    MealId INTEGER,
    RecordTime DATETIME NOT NULL,
    Calorie REAL NOT NULL,
    Price REAL NOT NULL,
    Remark VARCHAR(200) DEFAULT '',
    FOREIGN KEY (StallId) REFERENCES Stall(Id),
    FOREIGN KEY (MealId) REFERENCES Meal(Id)
);

CREATE TABLE IF NOT EXISTS MealRecordBuddy (
    RecordId INTEGER NOT NULL,
    BuddyId INTEGER NOT NULL,
    PRIMARY KEY (RecordId, BuddyId),
    FOREIGN KEY (RecordId) REFERENCES MealRecord(Id),
    FOREIGN KEY (BuddyId) REFERENCES DinnerBuddy(Id)
);

CREATE TABLE IF NOT EXISTS SelectionGroup (
    Id INTEGER PRIMARY KEY AUTOINCREMENT,
    GroupName VARCHAR(50) NOT NULL,
    IsSystem BOOLEAN NOT NULL DEFAULT 0
);

CREATE TABLE IF NOT EXISTS SelectionGroupStall (
    GroupId INTEGER NOT NULL,
    StallId INTEGER NOT NULL,
    PRIMARY KEY (GroupId, StallId),
    FOREIGN KEY (GroupId) REFERENCES SelectionGroup(Id),
    FOREIGN KEY (StallId) REFERENCES Stall(Id)
);";
        }

        private string GetCreateIndexSql()
        {
            return @"
CREATE INDEX IF NOT EXISTS idx_stall_canteen ON Stall(CanteenId);
CREATE INDEX IF NOT EXISTS idx_meal_stall ON Meal(StallId);
CREATE INDEX IF NOT EXISTS idx_record_stall ON MealRecord(StallId);
CREATE INDEX IF NOT EXISTS idx_record_meal ON MealRecord(MealId);
CREATE INDEX IF NOT EXISTS idx_record_time ON MealRecord(RecordTime);
CREATE INDEX IF NOT EXISTS idx_mrb_record ON MealRecordBuddy(RecordId);
CREATE INDEX IF NOT EXISTS idx_mrb_buddy ON MealRecordBuddy(BuddyId);
CREATE INDEX IF NOT EXISTS idx_sgs_group ON SelectionGroupStall(GroupId);
CREATE INDEX IF NOT EXISTS idx_sgs_stall ON SelectionGroupStall(StallId);
CREATE INDEX IF NOT EXISTS idx_sg_issystem ON SelectionGroup(IsSystem);";
        }

        private void InsertSystemCanteens(SQLiteConnection conn, SQLiteTransaction trans)
        {
            string sql = @"INSERT INTO Canteen (CanteenName, IsSystem) VALUES
('梅园食堂', 1), ('桂园食堂', 1), ('湖滨食堂', 1), ('枫园食堂', 1),
('樱园食堂', 1), ('星园食堂', 1), ('信息学部一食堂', 1), ('医学部杏园食堂', 1),
('工学部一食堂', 1), ('工学部二食堂', 1), ('工学部三食堂', 1);";
            using (var cmd = new SQLiteCommand(sql, conn, trans))
            {
                cmd.ExecuteNonQuery();
            }
        }

        private void InsertSystemStalls(SQLiteConnection conn, SQLiteTransaction trans)
        {
            string sql = @"INSERT INTO Stall (StallName, CanteenId, IsSystem) VALUES
('红烧肉窗口',1,1),('清蒸鱼香肉丝',1,1),('麻婆豆腐',1,1),('酸辣土豆丝',1,1),('糖醋排骨',1,1),('宫保鸡丁',1,1),('西红柿鸡蛋面',1,1),('回锅肉',1,1),('蛋炒饭炒面',1,1),('小笼包煎饼',1,1),
('牛肉面窗口',2,1),('重庆小面',2,1),('热干面热干粉',2,1),('馄饨',2,1),('水饺蒸饺',2,1),('红烧肉套餐',2,1),('香干回锅肉',2,1),('盖浇饭盖浇面',2,1),('小笼包肉包',2,1),
('红烧肉窗口',3,1),('麻婆豆腐',3,1),('糖醋排骨',3,1),('回锅肉',3,1),('西红柿鸡蛋',3,1),('热干面',3,1),('小笼包煎饼',3,1),
('红烧肉套餐',4,1),('牛肉面窗口',4,1),('清蒸鱼香肉丝',4,1),('糖醋排骨',4,1),('热干面热干粉',4,1),('西红柿鸡蛋面',4,1),('宫保鸡丁',4,1),('小笼包肉包',4,1),
('红烧肉窗口',5,1),('重庆面',5,1),('糖醋排骨',5,1),('麻婆豆腐',5,1),('回锅肉',5,1),('牛肉面窗口',5,1),('香干回锅肉',5,1),('馄饨',5,1),('蛋炒饭炒粉',5,1),('小笼包煎饼',5,1),
('红烧肉窗口',6,1),('清蒸鱼香肉丝',6,1),('麻婆豆腐',6,1),('酸辣土豆丝',6,1),('糖醋排骨',6,1),('热干面',6,1),('西红柿鸡蛋面',6,1),('宫保鸡丁',6,1),('红烧肉套餐',6,1),('小笼包肉包',6,1),
('红烧肉窗口',7,1),('重庆小面',7,1),('馄饨',7,1),('麻婆豆腐',7,1),('热干面热干粉',7,1),('西红柿鸡蛋',7,1),('红烧肉套餐',7,1),('小笼包煎饼',7,1),
('红烧肉窗口',8,1),('牛肉面窗口',8,1),('糖醋排骨',8,1),('回锅肉',8,1),('馄饨',8,1),('重庆小面',8,1),('热干面',8,1),('小笼包肉包',8,1),
('红烧肉窗口',9,1),('清蒸鱼香肉丝',9,1),('麻婆豆腐',9,1),('糖醋排骨',9,1),('西红柿鸡蛋',9,1),('热干面热干粉',9,1),('宫保鸡丁',9,1),('小笼包肉包',9,1),
('红烧肉套餐',10,1),('牛肉面窗口',10,1),('重庆小面',10,1),('糖醋排骨',10,1),('回锅肉',10,1),('馄饨',10,1),('热干面',10,1),('小笼包肉包',10,1),
('红烧肉窗口',11,1),('麻婆豆腐',11,1),('糖醋排骨',11,1),('热干面',11,1),('小笼包肉包',11,1);";
            using (var cmd = new SQLiteCommand(sql, conn, trans))
            {
                cmd.ExecuteNonQuery();
            }
        }
    }
}
