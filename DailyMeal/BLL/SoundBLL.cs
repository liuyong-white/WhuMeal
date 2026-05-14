using System;
using System.Media;
using System.IO;
using System.Threading.Tasks;
using DailyMeal.Model;
using DailyMeal.DAL;

namespace DailyMeal.BLL
{
    public class SoundBLL
    {
        private static SoundPlayer _currentPlayer;
        private static readonly object _lock = new object();
        private ConfigRepository _configRepo = new ConfigRepository();
        private AppSetting _settings;

        public SoundBLL()
        {
            _settings = _configRepo.LoadSettings();
        }

        public async Task PlayAsync(SoundType type)
        {
            await Task.Run(() =>
            {
                try
                {
                    if (!_settings.SoundEnabled)
                        return;
                    if (type == SoundType.Interact && !_settings.InteractiveSoundEnabled)
                        return;

                    string fileName = $"{type.ToString().ToLower()}.wav";
                    string filePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Sound", fileName);

                    if (!File.Exists(filePath))
                        return;

                    var player = new SoundPlayer(filePath);
                    player.Play();
                }
                catch { }
            }).ConfigureAwait(false);
        }

        public void Stop()
        {
            try
            {
                lock (_lock)
                {
                    _currentPlayer?.Stop();
                }
            }
            catch { }
        }

        public void UpdateSettings(bool soundEnabled, bool interactiveSoundEnabled)
        {
            _settings.SoundEnabled = soundEnabled;
            _settings.InteractiveSoundEnabled = interactiveSoundEnabled;
        }

        public AppSetting GetSettings()
        {
            return _settings;
        }

        public void ReloadSettings()
        {
            _settings = _configRepo.LoadSettings();
        }
    }
}
