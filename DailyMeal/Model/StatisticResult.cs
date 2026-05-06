using System.Collections.Generic;

namespace DailyMeal.Model
{
    public class StatisticResult
    {
        public List<CanteenStat> CanteenStats { get; set; } = new List<CanteenStat>();
        public List<StallStat> StallStats { get; set; } = new List<StallStat>();
        public decimal TotalExpense { get; set; }
        public decimal TotalCalorie { get; set; }
        public int TotalCount { get; set; }
    }
}
