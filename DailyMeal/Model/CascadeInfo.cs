namespace DailyMeal.Model
{
    public class CascadeInfo
    {
        public int StallCount { get; set; }
        public int MealCount { get; set; }
        public int RecordCount { get; set; }
        public int BuddyLinkCount { get; set; }
        public int GroupLinkCount { get; set; }

        public string GetDescription()
        {
            var parts = new System.Collections.Generic.List<string>();
            if (StallCount > 0) parts.Add($"{StallCount}个档口");
            if (MealCount > 0) parts.Add($"{MealCount}条餐食");
            if (RecordCount > 0) parts.Add($"{RecordCount}条就餐记录");
            if (BuddyLinkCount > 0) parts.Add($"{BuddyLinkCount}条饭搭子关联");
            if (GroupLinkCount > 0) parts.Add($"{GroupLinkCount}条分组关联");
            return parts.Count > 0 ? "将同时删除" + string.Join("、", parts) : "";
        }
    }
}
