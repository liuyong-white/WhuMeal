namespace DailyMeal.Model
{
    public class Meal
    {
        public int Id { get; set; }
        public string MealName { get; set; }
        public int StallId { get; set; }
        public decimal Calorie { get; set; }
        public decimal Price { get; set; }
        public string Remark { get; set; }
        public bool IsSystem { get; set; }

        public string StallName { get; set; }
    }
}
