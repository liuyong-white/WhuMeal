using System;

namespace DailyMeal.Model
{
    public class MealRecord
    {
        public int Id { get; set; }
        public int StallId { get; set; }
        public int? MealId { get; set; }
        public DateTime RecordTime { get; set; }
        public decimal Calorie { get; set; }
        public decimal Price { get; set; }
        public string Remark { get; set; }

        public string StallName { get; set; }
        public int CanteenId { get; set; }
        public string CanteenName { get; set; }
        public string MealName { get; set; }
    }
}
