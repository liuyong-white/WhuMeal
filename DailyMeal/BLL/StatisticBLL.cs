using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DailyMeal.Model;
using DailyMeal.DAL;

namespace DailyMeal.BLL
{
    public class StatisticBLL
    {
        private MealRecordDAL _recordDal = new MealRecordDAL();
        private MealRecordBuddyDAL _recordBuddyDal = new MealRecordBuddyDAL();
        private CanteenDAL _canteenDal = new CanteenDAL();
        private StallDAL _stallDal = new StallDAL();
        private ConfigRepository _configRepo = new ConfigRepository();

        public async Task<StatisticResult> CalculateCanteenStatsAsync(PeriodType period, DateTime? startDate, DateTime? endDate)
        {
            return await Task.Run(() =>
            {
                var range = GetDateRange(period, startDate, endDate);
                var records = _recordDal.GetByDateRange(range.start, range.end);
                var canteens = _canteenDal.GetAll();

                var result = new StatisticResult
                {
                    TotalCount = records.Count,
                    TotalExpense = records.Sum(r => r.Price),
                    TotalCalorie = records.Sum(r => r.Calorie)
                };

                var grouped = records.GroupBy(r => r.CanteenId);
                foreach (var g in grouped)
                {
                    var canteen = canteens.FirstOrDefault(c => c.Id == g.Key);
                    result.CanteenStats.Add(new CanteenStat
                    {
                        CanteenId = g.Key,
                        CanteenName = canteen?.CanteenName ?? "未知食堂",
                        Count = g.Count(),
                        TotalExpense = g.Sum(r => r.Price),
                        TotalCalorie = g.Sum(r => r.Calorie),
                        Percentage = records.Count > 0 ? (double)g.Count() / records.Count * 100 : 0
                    });
                }

                return result;
            });
        }

        public async Task<StatisticResult> CalculateStallStatsAsync(int canteenId, PeriodType period, DateTime? startDate, DateTime? endDate)
        {
            return await Task.Run(() =>
            {
                var range = GetDateRange(period, startDate, endDate);
                var records = _recordDal.GetByDateRange(range.start, range.end)
                    .Where(r => r.CanteenId == canteenId).ToList();
                var stalls = _stallDal.GetByCanteenId(canteenId);

                var result = new StatisticResult
                {
                    TotalCount = records.Count,
                    TotalExpense = records.Sum(r => r.Price),
                    TotalCalorie = records.Sum(r => r.Calorie)
                };

                var grouped = records.GroupBy(r => r.StallId);
                foreach (var g in grouped)
                {
                    var stall = stalls.FirstOrDefault(s => s.Id == g.Key);
                    result.StallStats.Add(new StallStat
                    {
                        StallId = g.Key,
                        StallName = stall?.StallName ?? "未知档口",
                        Count = g.Count(),
                        TotalExpense = g.Sum(r => r.Price),
                        TotalCalorie = g.Sum(r => r.Calorie),
                        Percentage = records.Count > 0 ? (double)g.Count() / records.Count * 100 : 0
                    });
                }

                return result;
            });
        }

        public async Task<ReportData> GenerateReportAsync(ReportType type, DateTime startDate, DateTime endDate)
        {
            return await Task.Run(() =>
            {
                var records = _recordDal.GetByDateRange(startDate, endDate);
                var data = new ReportData
                {
                    TotalCount = records.Count,
                    TotalExpense = records.Sum(r => r.Price),
                    AvgCalorie = records.Count > 0 ? records.Average(r => r.Calorie) : 0,
                    TopLocation = ""
                };

                if (records.Count > 0)
                {
                    var canteenGroup = records.GroupBy(r => r.CanteenName);
                    var topCanteen = canteenGroup.OrderByDescending(g => g.Count()).FirstOrDefault();
                    if (topCanteen != null)
                    {
                        data.MostFrequentCanteen = topCanteen.Key;
                        data.MostFrequentCanteenCount = topCanteen.Count();
                        data.TopLocation = topCanteen.Key;
                    }

                    var expenseCanteen = canteenGroup.OrderByDescending(g => g.Sum(r => r.Price)).FirstOrDefault();
                    if (expenseCanteen != null)
                    {
                        data.HighestExpenseCanteen = expenseCanteen.Key;
                        data.HighestExpenseCanteenAmount = expenseCanteen.Sum(r => r.Price);
                    }

                    var stallGroup = records.GroupBy(r => r.StallName);
                    var topStall = stallGroup.OrderByDescending(g => g.Count()).FirstOrDefault();
                    if (topStall != null)
                    {
                        data.MostFrequentStall = topStall.Key;
                        data.MostFrequentStallCount = topStall.Count();
                    }

                    var expenseStall = stallGroup.OrderByDescending(g => g.Sum(r => r.Price)).FirstOrDefault();
                    if (expenseStall != null)
                    {
                        data.HighestExpenseStall = expenseStall.Key;
                        data.HighestExpenseStallAmount = expenseStall.Sum(r => r.Price);
                    }

                    var allBuddies = new List<int>();
                    foreach (var rec in records)
                    {
                        var buddies = _recordBuddyDal.GetByRecordId(rec.Id);
                        allBuddies.AddRange(buddies.Where(b => b.BuddyId != 1).Select(b => b.BuddyId));
                    }
                    if (allBuddies.Count > 0)
                    {
                        var topBuddyId = allBuddies.GroupBy(b => b).OrderByDescending(g => g.Count()).First().Key;
                        var buddy = new DinnerBuddyDAL().GetById(topBuddyId);
                        if (buddy != null)
                        {
                            data.TopBuddy = buddy.Name;
                            data.TopBuddyCount = allBuddies.Count(b => b == topBuddyId);
                        }
                    }
                }

                return data;
            });
        }

        public async Task<TodayOverview> GetTodayOverviewAsync()
        {
            return await Task.Run(() =>
            {
                var todayRecords = _recordDal.GetTodayRecords();
                return new TodayOverview
                {
                    MealCount = todayRecords.Count,
                    TotalExpense = todayRecords.Sum(r => r.Price),
                    TotalCalorie = todayRecords.Sum(r => r.Calorie)
                };
            });
        }

        private (DateTime start, DateTime end) GetDateRange(PeriodType period, DateTime? startDate, DateTime? endDate)
        {
            if (startDate.HasValue && endDate.HasValue)
                return (startDate.Value, endDate.Value);

            var now = DateTime.Now;
            switch (period)
            {
                case PeriodType.Week:
                    var weekStart = now.Date.AddDays(-(int)now.DayOfWeek);
                    return (weekStart, weekStart.AddDays(7));
                case PeriodType.Month:
                    return (new DateTime(now.Year, now.Month, 1), new DateTime(now.Year, now.Month, 1).AddMonths(1));
                case PeriodType.Year:
                    return (new DateTime(now.Year, 1, 1), new DateTime(now.Year + 1, 1, 1));
                case PeriodType.Semester:
                    var settings = _configRepo.LoadSettings();
                    if (settings.SemesterStartDate.HasValue && settings.SemesterEndDate.HasValue)
                        return (settings.SemesterStartDate.Value, settings.SemesterEndDate.Value);
                    return (new DateTime(now.Year, 1, 1), new DateTime(now.Year + 1, 1, 1));
                default:
                    return (now.Date, now.Date.AddDays(1));
            }
        }
    }
}
