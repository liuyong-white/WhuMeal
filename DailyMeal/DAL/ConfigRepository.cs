using System;
using System.IO;
using DailyMeal.Model;
using Newtonsoft.Json;

namespace DailyMeal.DAL
{
    public class ConfigRepository
    {
        private static readonly string ConfigDirectory = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Config");
        private static readonly string ConfigFilePath = Path.Combine(ConfigDirectory, "appsettings.json");

        public AppSetting LoadSettings()
        {
            try
            {
                if (!File.Exists(ConfigFilePath))
                    return new AppSetting();
                string json = File.ReadAllText(ConfigFilePath);
                return JsonConvert.DeserializeObject<AppSetting>(json) ?? new AppSetting();
            }
            catch
            {
                return new AppSetting();
            }
        }

        public void SaveSettings(AppSetting settings)
        {
            try
            {
                if (!Directory.Exists(ConfigDirectory))
                    Directory.CreateDirectory(ConfigDirectory);
                string json = JsonConvert.SerializeObject(settings, Formatting.Indented);
                File.WriteAllText(ConfigFilePath, json);
            }
            catch { }
        }
    }
}
