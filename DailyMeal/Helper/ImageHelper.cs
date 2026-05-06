using System;
using System.Drawing;
using System.IO;

namespace DailyMeal.Helper
{
    public static class ImageHelper
    {
        private static readonly string ImagesDirectory = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Images");
        private static readonly string DefaultPlaceholder = Path.Combine(ImagesDirectory, "default_placeholder.png");

        public static string CopyToLocalStorage(string sourcePath, string category, int entityId)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(sourcePath) || !File.Exists(sourcePath))
                    return "";

                string targetDir = Path.Combine(ImagesDirectory, category);
                if (!Directory.Exists(targetDir))
                    Directory.CreateDirectory(targetDir);

                string extension = Path.GetExtension(sourcePath);
                string fileName = $"{category}_{entityId}_{DateTime.Now:yyyyMMddHHmmss}{extension}";
                string targetPath = Path.Combine(targetDir, fileName);
                File.Copy(sourcePath, targetPath, true);
                return targetPath;
            }
            catch
            {
                return "";
            }
        }

        public static void DeleteLocalImage(string localPath)
        {
            try
            {
                if (!string.IsNullOrWhiteSpace(localPath) && File.Exists(localPath))
                    File.Delete(localPath);
            }
            catch { }
        }

        public static Image LoadImage(string localPath)
        {
            try
            {
                if (!string.IsNullOrWhiteSpace(localPath) && File.Exists(localPath))
                    return Image.FromFile(localPath);
            }
            catch { }

            try
            {
                if (File.Exists(DefaultPlaceholder))
                    return Image.FromFile(DefaultPlaceholder);
            }
            catch { }

            return null;
        }
    }
}
