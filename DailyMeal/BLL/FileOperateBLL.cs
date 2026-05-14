using System;
using System.Data.SQLite;
using System.IO;
using System.Threading.Tasks;
using DailyMeal.Model;
using DailyMeal.DAL;
using DailyMeal.Helper;

namespace DailyMeal.BLL
{
    public class FileOperateBLL
    {
        private ConfigRepository _configRepo = new ConfigRepository();
        private static readonly string DbFileName = "DailyMeal.db";

        public async Task ExportReportAsync(string filePath, ReportData data, ExportFormat format, IProgress<int> progress)
        {
            await Task.Run(() =>
            {
                try
                {
                    using (var writer = new StreamWriter(filePath, false, System.Text.Encoding.UTF8))
                    {
                        writer.WriteLine("=== 珞珈食记 统计报告 ===");
                        writer.WriteLine();
                        writer.WriteLine("常规数据:");
                        writer.WriteLine($"总用餐次数,{data.TotalCount}");
                        writer.WriteLine($"总消费金额,{data.TotalExpense:F2}");
                        writer.WriteLine($"平均热量,{data.AvgCalorie:F1}");
                        writer.WriteLine($"高频就餐地点,{data.TopLocation}");
                        writer.WriteLine();
                        writer.WriteLine("趣味数据:");
                        writer.WriteLine($"最常去食堂,{data.MostFrequentCanteen} ({data.MostFrequentCanteenCount}次)");
                        writer.WriteLine($"消费最高食堂,{data.HighestExpenseCanteen} (¥{data.HighestExpenseCanteenAmount:F2})");
                        writer.WriteLine($"最爱档口,{data.MostFrequentStall} ({data.MostFrequentStallCount}次)");
                        writer.WriteLine($"消费最高档口,{data.HighestExpenseStall} (¥{data.HighestExpenseStallAmount:F2})");
                        if (!string.IsNullOrEmpty(data.TopBuddy))
                            writer.WriteLine($"最佳饭搭子,{data.TopBuddy} ({data.TopBuddyCount}次)");
                    }
                    progress?.Report(100);
                }
                catch (IOException) { throw; }
                catch (UnauthorizedAccessException) { throw; }
            });
        }

        public async Task BackupDatabaseAsync(string targetPath, IProgress<int> progress)
        {
            await Task.Run(() =>
            {
                try
                {
                    string dbPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, DbFileName);
                    if (!File.Exists(dbPath))
                        throw new FileNotFoundException("数据库文件不存在");

                    SQLiteConnection.ClearAllPools();
                    progress?.Report(30);

                    string targetFile = Path.Combine(targetPath, $"DailyMeal_backup_{DateTime.Now:yyyyMMddHHmmss}.db");
                    File.Copy(dbPath, targetFile, true);
                    progress?.Report(100);
                }
                catch (IOException) { throw; }
                catch (UnauthorizedAccessException) { throw; }
            });
        }

        public async Task RestoreDatabaseAsync(string sourcePath)
        {
            await Task.Run(() =>
            {
                try
                {
                    string testConnStr = $"Data Source={sourcePath};Version=3;New=False;";
                    using (var testConn = new SQLiteConnection(testConnStr))
                    {
                        testConn.Open();
                    }

                    SQLiteConnection.ClearAllPools();

                    string dbPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, DbFileName);
                    File.Copy(sourcePath, dbPath, true);
                }
                catch (IOException) { throw; }
                catch (UnauthorizedAccessException) { throw; }
                catch (SQLiteException) { throw new InvalidDataException("备份文件不是有效的数据库文件"); }
            });
        }
    }
}
