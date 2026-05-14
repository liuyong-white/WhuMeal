using System.Text.RegularExpressions;

namespace DailyMeal.Helper
{
    public static class RegexHelper
    {
        private static readonly string PricePattern = @"^(0|[1-9]\d*)(\.\d{1,2})?$";
        private static readonly string CaloriePattern = @"^[0-9]+(\.\d+)?$";
        private static readonly string BuddyNamePattern = @"^[a-zA-Z0-9\u4e00-\u9fa5]{2,20}$";
        private static readonly string RemarkWhitelistPattern = @"^[a-zA-Z0-9\u4e00-\u9fa5\s，。！？、；：""''（）【】《》·~—…]{1,200}$";
        private static readonly string RemarkBlacklistPattern = @"[<>&'""]";
        private static readonly string EntityNamePattern = @"^[a-zA-Z0-9\u4e00-\u9fa5\s（）]{2,50}$";

        public static (bool isValid, string message) ValidatePrice(string input)
        {
            if (string.IsNullOrWhiteSpace(input))
                return (true, "");
            if (!Regex.IsMatch(input, PricePattern))
                return (false, "请输入有效的消费价格，最多保留2位小数");
            return (true, "");
        }

        public static (bool isValid, string message) ValidateCalorie(string input)
        {
            if (string.IsNullOrWhiteSpace(input))
                return (true, "");
            if (!Regex.IsMatch(input, CaloriePattern))
                return (false, "请输入有效的热量数值（非负数）");
            return (true, "");
        }

        public static (bool isValid, string message) ValidateBuddyName(string input)
        {
            if (string.IsNullOrWhiteSpace(input))
                return (false, "姓名不能为空");
            if (!Regex.IsMatch(input, BuddyNamePattern))
                return (false, "姓名仅允许中文、字母、数字，长度2-20字");
            return (true, "");
        }

        public static (bool isValid, string message) ValidateRemark(string input)
        {
            if (string.IsNullOrEmpty(input))
                return (true, "");
            if (!Regex.IsMatch(input, RemarkWhitelistPattern))
                return (false, "备注仅允许中文、字母、数字、常见标点，长度1-200字");
            if (Regex.IsMatch(input, RemarkBlacklistPattern))
                return (false, "备注包含禁止字符（< > & ' \"）");
            return (true, "");
        }

        public static (bool isValid, string message) ValidateEntityName(string input)
        {
            if (string.IsNullOrWhiteSpace(input))
                return (false, "名称不能为空");
            if (!Regex.IsMatch(input, EntityNamePattern))
                return (false, "名称仅允许中文、字母、数字、空格、括号，长度2-50字");
            return (true, "");
        }

        public static (bool isValid, string message) ValidateGroupName(string input)
        {
            return ValidateEntityName(input);
        }
    }
}
