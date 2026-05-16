using System;
using System.Collections.Generic;
using System.Data.SQLite;
using System.Threading.Tasks;
using System.Linq;
using Dapper;
using DailyMeal.Model;
using DailyMeal.DAL;
using DailyMeal.Helper;

namespace DailyMeal.BLL
{
    public class DataManageBLL
    {
        private CanteenDAL _canteenDal = new CanteenDAL();
        private StallDAL _stallDal = new StallDAL();
        private DinnerBuddyDAL _buddyDal = new DinnerBuddyDAL();
        private MealRecordDAL _recordDal = new MealRecordDAL();
        private MealRecordBuddyDAL _recordBuddyDal = new MealRecordBuddyDAL();
        private SelectionGroupDAL _groupDal = new SelectionGroupDAL();
        private BaseDAL _baseDal = new BaseDAL();

        public async Task<List<Canteen>> GetAllCanteensAsync()
        {
            return await Task.Run(() => _canteenDal.GetAll());
        }

        public async Task<Canteen> AddCanteenAsync(string name)
        {
            return await Task.Run(() =>
            {
                var canteen = new Canteen { CanteenName = name, IsSystem = false };
                canteen.Id = _canteenDal.Insert(canteen);
                var group = new SelectionGroup { GroupName = name, IsSystem = true };
                group.Id = _groupDal.Insert(group);
                return canteen;
            });
        }

        public async Task UpdateCanteenAsync(Canteen canteen)
        {
            await Task.Run(() =>
            {
                canteen.IsSystem = false;
                _canteenDal.Update(canteen);
            });
        }

        public async Task<CascadeInfo> DeleteCanteenAsync(int canteenId)
        {
            return await Task.Run(() =>
            {
                var impact = CalculateCascadeImpact("Canteen", canteenId);
                var stallIds = _stallDal.GetStallIdsByCanteenId(canteenId);
                var recordIds = stallIds.Count > 0 ? _recordDal.GetRecordIdsByStallIds(stallIds) : new List<int>();
                var canteen = _canteenDal.GetById(canteenId);
                var canteenName = canteen?.CanteenName ?? "";

                _baseDal.ExecuteInTransaction((conn, trans) =>
                {
                    if (stallIds.Count > 0)
                    {
                        _groupDal.DeleteGroupStallsByStallIds(stallIds, conn, trans);
                        if (recordIds.Count > 0)
                            _recordBuddyDal.DeleteByRecordIds(recordIds, conn, trans);
                        _recordDal.DeleteByStallIds(stallIds, conn, trans);
                        conn.Execute("UPDATE MealRecord SET MealId = NULL WHERE StallId IN @StallIds", new { StallIds = stallIds }, trans);
                        conn.Execute("DELETE FROM Meal WHERE StallId IN @StallIds", new { StallIds = stallIds }, trans);
                    }
                    _stallDal.DeleteByCanteenId(canteenId, conn, trans);
                    _groupDal.DeleteByCanteenName(canteenName, conn, trans);
                    conn.Execute("DELETE FROM Canteen WHERE Id = @Id", new { Id = canteenId }, trans);
                });

                return impact;
            });
        }

        public async Task<List<Stall>> GetStallsByCanteenAsync(int canteenId)
        {
            return await Task.Run(() => _stallDal.GetByCanteenId(canteenId));
        }

        public async Task<Stall> AddStallAsync(string name, int canteenId)
        {
            return await Task.Run(() =>
            {
                var stall = new Stall { StallName = name, CanteenId = canteenId, IsSystem = false };
                stall.Id = _stallDal.Insert(stall);
                var canteen = _canteenDal.GetById(canteenId);
                if (canteen != null)
                {
                    var builtinGroup = _groupDal.GetByCanteenName(canteen.CanteenName);
                    if (builtinGroup != null)
                    {
                        _groupDal.AddStallToGroup(builtinGroup.Id, stall.Id);
                    }
                }
                return stall;
            });
        }

        public async Task UpdateStallAsync(Stall stall)
        {
            await Task.Run(() =>
            {
                stall.IsSystem = false;
                _stallDal.Update(stall);
            });
        }

        public async Task<CascadeInfo> DeleteStallAsync(int stallId)
        {
            return await Task.Run(() =>
            {
                var impact = CalculateCascadeImpact("Stall", stallId);
                var stallIds = new List<int> { stallId };
                var recordIds = _recordDal.GetRecordIdsByStallIds(stallIds);

                _baseDal.ExecuteInTransaction((conn, trans) =>
                {
                    _groupDal.DeleteGroupStallsByStallIds(stallIds, conn, trans);
                    if (recordIds.Count > 0)
                        _recordBuddyDal.DeleteByRecordIds(recordIds, conn, trans);
                    _recordDal.DeleteByStallIds(stallIds, conn, trans);
                    conn.Execute("UPDATE MealRecord SET MealId = NULL WHERE StallId = @StallId", new { StallId = stallId }, trans);
                    conn.Execute("DELETE FROM Meal WHERE StallId = @StallId", new { StallId = stallId }, trans);
                    conn.Execute("DELETE FROM Stall WHERE Id = @Id", new { Id = stallId }, trans);
                });

                return impact;
            });
        }

        public async Task<DinnerBuddy> AddBuddyAsync(string name, string photoPath)
        {
            return await Task.Run(() =>
            {
                var buddy = new DinnerBuddy { Name = name, Photo = "", IsSystem = false };
                buddy.Id = _buddyDal.Insert(buddy);
                if (!string.IsNullOrWhiteSpace(photoPath))
                {
                    buddy.Photo = ImageHelper.CopyToLocalStorage(photoPath, "Buddy", buddy.Id);
                    _buddyDal.Update(buddy);
                }
                return buddy;
            });
        }

        public async Task UpdateBuddyAsync(DinnerBuddy buddy)
        {
            await Task.Run(() =>
            {
                if (_buddyDal.IsSelfBuddy(buddy.Id))
                    throw new InvalidOperationException("系统默认记录不可重命名");
                _buddyDal.Update(buddy);
            });
        }

        public async Task DeleteBuddyAsync(int buddyId)
        {
            await Task.Run(() =>
            {
                if (_buddyDal.IsSelfBuddy(buddyId))
                    throw new InvalidOperationException("系统默认记录不可删除");
                _recordBuddyDal.DeleteByBuddyId(buddyId);
                _buddyDal.Delete(buddyId);
            });
        }

        public CascadeInfo CalculateCascadeImpact(string entityType, int entityId)
        {
            var info = new CascadeInfo();
            if (entityType == "Canteen")
            {
                var stallIds = _stallDal.GetStallIdsByCanteenId(entityId);
                info.StallCount = stallIds.Count;
                if (stallIds.Count > 0)
                {
                    info.RecordCount = _recordDal.GetCountByStallIds(stallIds);
                    var recordIds = _recordDal.GetRecordIdsByStallIds(stallIds);
                    info.BuddyLinkCount = recordIds.Count > 0 ? _recordBuddyDal.GetCountByRecordIds(recordIds) : 0;
                    info.GroupLinkCount = _groupDal.GetCountByStallIds(stallIds);
                }
            }
            else if (entityType == "Stall")
            {
                var stallIds = new List<int> { entityId };
                info.RecordCount = _recordDal.GetCountByStallIds(stallIds);
                var recordIds = _recordDal.GetRecordIdsByStallIds(stallIds);
                info.BuddyLinkCount = recordIds.Count > 0 ? _recordBuddyDal.GetCountByRecordIds(recordIds) : 0;
                info.GroupLinkCount = _groupDal.GetCountByStallIds(stallIds);
            }
            return info;
        }
    }
}
