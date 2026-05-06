namespace DailyMeal.Model
{
    public class Stall
    {
        public int Id { get; set; }
        public string StallName { get; set; }
        public int CanteenId { get; set; }
        public string StallPhoto { get; set; }
        public bool IsSystem { get; set; }

        public string CanteenName { get; set; }
    }
}
