namespace DailyMeal.Model
{
    public class CanteenStat
    {
        public int CanteenId { get; set; }
        public string CanteenName { get; set; }
        public int Count { get; set; }
        public decimal TotalExpense { get; set; }
        public decimal TotalCalorie { get; set; }
        public double Percentage { get; set; }
    }
}
