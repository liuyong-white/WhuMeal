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
                int extraRounds = 3 + _random.Next(2);
                int totalSteps = extraRounds * candidates.Count + targetIndex;
                int minDelay = 40;
                int maxDelay = 300;

                for (int i = 0; i < totalSteps; i++)
                {
                    ct.ThrowIfCancellationRequested();

                    double t = (double)i / totalSteps;
                    int delay = (int)(minDelay + (maxDelay - minDelay) * t * t);
                    int currentIndex = i % candidates.Count;
                    var stall = candidates[currentIndex];
                    progress?.Report(new RollProgressInfo
                    {
                        CurrentName = stall.StallName,
                        CurrentIndex = currentIndex
                    });

                    await Task.Delay(delay, ct);
                }

                var target = candidates[targetIndex];
                progress?.Report(new RollProgressInfo
                {
                    CurrentName = target.StallName,
                    CurrentIndex = targetIndex
                });
            }, ct);
        }

        public async Task AnimateFlashRollAsync(List<Stall> candidates, int targetIndex, IProgress<RollStepInfo> progress, CancellationToken ct)
        {
            await Task.Run(async () =>
            {
                int count = candidates.Count;
                int totalSteps = 7 + (count * 2 / 3) + _random.Next(3);
                int minDelay = 60;
                int maxDelay = 220;

                for (int i = 0; i < totalSteps; i++)
                {
                    ct.ThrowIfCancellationRequested();

                    double t = (double)i / totalSteps;
                    int delay = (int)(minDelay + (maxDelay - minDelay) * t * t);

                    int idx;
                    if (i >= totalSteps - 1)
                    {
                        idx = targetIndex;
                    }
                    else
                    {
                        idx = _random.Next(count);
                    }

                    var stall = candidates[idx];
                    string name = $"{stall.CanteenName} - {stall.StallName}";

                    progress?.Report(new RollStepInfo { DisplayName = name, Alpha = 1f });
                    await Task.Delay(delay, ct);
                }

                var final = candidates[targetIndex];
                progress?.Report(new RollStepInfo { DisplayName = $"{final.CanteenName} - {final.StallName}", Alpha = 1f });
            }, ct);
        }

        public async Task AnimateSlotRollAsync(int itemCount, int startIndex, int targetIndex, IProgress<float> progress, CancellationToken ct)
        {
            await Task.Run(async () =>
            {
                int extraRounds = 2 + _random.Next(2);
                float start = startIndex;
                float end = start + extraRounds * itemCount + (targetIndex - startIndex + itemCount) % itemCount;
                if (end <= start) end = start + itemCount;

                float totalDistance = end - start;
                int totalSteps = Math.Max((int)(totalDistance * 8), 80);
                int minDelay = 16;
                int maxDelay = 70;

                for (int i = 0; i <= totalSteps; i++)
                {
                    ct.ThrowIfCancellationRequested();

                    double t = (double)i / totalSteps;
                    double eased = 1 - Math.Pow(1 - t, 3);
                    float currentScroll = (float)(start + totalDistance * eased);
                    progress?.Report(currentScroll);

                    int delay = (int)(minDelay + (maxDelay - minDelay) * t * t);
                    await Task.Delay(delay, ct);
                }

                progress?.Report(end);
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
