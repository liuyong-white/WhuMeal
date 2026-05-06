using System;
using System.Threading.Tasks;
using System.Windows.Forms;
using DailyMeal.BLL;
using DailyMeal.DAL;
using DailyMeal.Helper;
using DailyMeal.Model;
using DailyMeal.UI;

namespace DailyMeal
{
    static class Program
    {
        public static SoundBLL SoundBLL { get; private set; }
        public static AppSetting CurrentSettings { get; set; }

        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            Application.ThreadException += Application_ThreadException;
            AppDomain.CurrentDomain.UnhandledException += CurrentDomain_UnhandledException;

            try
            {
                var baseDal = new BaseDAL();
                baseDal.EnsureDatabaseInitialized();
                baseDal.EnsureSystemDataInitialized();

                var configRepo = new ConfigRepository();
                CurrentSettings = configRepo.LoadSettings();

                SoundBLL = new SoundBLL();
                _ = SoundBLL.PlayAsync(Model.SoundType.Startup);

                Application.Run(new MainForm());
            }
            catch (Exception ex)
            {
                ExceptionHelper.HandleThreadException(ex);
            }
        }

        private static void Application_ThreadException(object sender, System.Threading.ThreadExceptionEventArgs e)
        {
            ExceptionHelper.HandleThreadException(e.Exception);
        }

        private static void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            if (e.ExceptionObject is Exception ex)
                ExceptionHelper.HandleUnhandledException(ex);
        }
    }
}
