namespace DailyMeal.Model
{
    public class ReportData
    {
        public int TotalCount { get; set; }
        public decimal TotalExpense { get; set; }
        public decimal AvgCalorie { get; set; }
        public string TopLocation { get; set; }

        public string MostFrequentCanteen { get; set; }
        public int MostFrequentCanteenCount { get; set; }
        public string MostFrequentStall { get; set; }
        public int MostFrequentStallCount { get; set; }
        public string HighestExpenseCanteen { get; set; }
        public decimal HighestExpenseCanteenAmount { get; set; }
        public string HighestExpenseStall { get; set; }
        public decimal HighestExpenseStallAmount { get; set; }
        public string TopBuddy { get; set; }
        public int TopBuddyCount { get; set; }
    }
}
