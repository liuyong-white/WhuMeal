using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using DailyMeal.Model;
using DailyMeal.DAL;

namespace DailyMeal.BLL
{
    public class MealSelectBLL
    {
        private static readonly Random _random = new Random();
        private StallDAL _stallDal = new StallDAL();
        private MealRecordDAL _recordDal = new MealRecordDAL();
        private MealRecordBuddyDAL _recordBuddyDal = new MealRecordBuddyDAL();
        private SelectionGroupDAL _groupDal = new SelectionGroupDAL();

        public async Task<Stall> RandomSelectAsync(List<int> candidateStallIds)
        {
            return await Task.Run(() =>
            {
                if (candidateStallIds == null || candidateStallIds.Count == 0)
                    return null;
                int idx = _random.Next(candidateStallIds.Count);
                return _stallDal.GetById(candidateStallIds[idx]);
            });
        }

        public async Task AnimateRollAsync(List<Stall> candidates, int targetIndex, IProgress<RollProgressInfo> progress, CancellationToken ct)
        {
            await Task.Run(async () =>
            {
                int totalSteps = 30 + _random.Next(10);
                int fastSteps = totalSteps / 2;
                int midSteps = totalSteps / 4;
                int currentIndex = 0;

                for (int i = 0; i < totalSteps; i++)
                {
                    ct.ThrowIfCancellationRequested();

                    currentIndex = i % candidates.Count;
                    var stall = candidates[currentIndex];
                    progress?.Report(new RollProgressInfo
                    {
                        CurrentName = stall.StallName,
                        CurrentPhoto = stall.StallPhoto,
                        CurrentIndex = currentIndex
                    });

                    int delay;
                    if (i < fastSteps)
                        delay = 50;
                    else if (i < fastSteps + midSteps)
                        delay = 50 + (i - fastSteps) * 30;
                    else
                        delay = 200 + (i - fastSteps - midSteps) * 80;

                    await Task.Delay(delay, ct);
                }

                for (int i = 0; i < candidates.Count * 2 + targetIndex; i++)
                {
                    ct.ThrowIfCancellationRequested();
                    currentIndex = i % candidates.Count;
                    var stall = candidates[currentIndex];
                    progress?.Report(new RollProgressInfo
                    {
                        CurrentName = stall.StallName,
                        CurrentPhoto = stall.StallPhoto,
                        CurrentIndex = currentIndex
                    });
                    await Task.Delay(200, ct);
                }

                var target = candidates[targetIndex];
                progress?.Report(new RollProgressInfo
                {
                    CurrentName = target.StallName,
                    CurrentPhoto = target.StallPhoto,
                    CurrentIndex = targetIndex
                });
            }, ct);
        }

        public async Task<MealRecord> ConfirmSelectionAsync(int stallId, int? mealId, decimal price, decimal calorie, string remark, List<int> buddyIds)
        {
            return await Task.Run(() =>
            {
                var record = new MealRecord
                {
                    StallId = stallId,
                    MealId = mealId,
                    RecordTime = DateTime.Now,
                    Calorie = calorie,
                    Price = price,
                    Remark = remark ?? ""
                };
                record.Id = _recordDal.Insert(record);
                if (buddyIds != null && buddyIds.Count > 0)
                {
                    _recordBuddyDal.InsertBatch(record.Id, buddyIds);
                }
                return record;
            });
        }

        public List<SelectionGroup> GetSelectionGroups()
        {
            return _groupDal.GetAll();
        }

        public async Task<List<Stall>> GetGroupStallsAsync(int groupId)
        {
            return await Task.Run(() =>
            {
                var stallIds = _groupDal.GetStallIdsByGroupId(groupId);
                return stallIds.Select(id => _stallDal.GetById(id)).Where(s => s != null).ToList();
            });
        }

        public async Task SaveSelectionGroupAsync(SelectionGroup group, List<int> stallIds)
        {
            await Task.Run(() =>
            {
                if (group.Id > 0)
                {
                    var existing = _groupDal.GetById(group.Id);
                    if (existing != null && existing.IsSystem)
                        throw new InvalidOperationException("内置分组不可编辑");
                }
                if (group.Id == 0)
                    group.Id = _groupDal.Insert(group);
                else
                    _groupDal.Update(group);
                _groupDal.SaveGroupStalls(group.Id, stallIds);
            });
        }

        public async Task DeleteSelectionGroupAsync(int groupId)
        {
            await Task.Run(() =>
            {
                var group = _groupDal.GetById(groupId);
                if (group != null && group.IsSystem)
                    throw new InvalidOperationException("内置分组不可删除");
                _groupDal.Delete(groupId);
            });
        }
    }
}
