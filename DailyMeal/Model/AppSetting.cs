using System;

namespace DailyMeal.Model
{
    public class AppSetting
    {
        public bool SoundEnabled { get; set; } = true;
        public bool InteractiveSoundEnabled { get; set; } = true;
        public string ExportPath { get; set; } = "";
        public DateTime? SemesterStartDate { get; set; }
        public DateTime? SemesterEndDate { get; set; }
    }
}
