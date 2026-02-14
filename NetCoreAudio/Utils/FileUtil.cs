using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;

namespace NetCoreAudio.Utils
{
    internal static class FileUtil
    {
        private const string TempDirName = "temp";
        private const int MaxPathLengthBeforeFallback = 128;

        public static string CheckFileToPlay(string originalFileName)
        {
            var fileNameToReturn = originalFileName;
            if (originalFileName.Contains(" ") || originalFileName.Length > MaxPathLengthBeforeFallback)
            {
                if (!WindowsUtil.TryGetShortPath(originalFileName, out var shortPath))
                {
                    Directory.CreateDirectory(TempDirName);
                    fileNameToReturn = Path.Combine(TempDirName, BuildTempFileName(originalFileName));
                    File.Copy(originalFileName, fileNameToReturn, true);
                }
                else
                {
                    fileNameToReturn = shortPath;
                }
            }

            return fileNameToReturn;
        }

        private static string BuildTempFileName(string originalFileName)
        {
            var extension = Path.GetExtension(originalFileName);
            var baseName = Path.GetFileNameWithoutExtension(originalFileName).Replace(" ", "");
            if (baseName.Length > 80)
                baseName = baseName.Substring(0, 80);

            using var sha256 = SHA256.Create();
            var hashBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(Path.GetFullPath(originalFileName)));
            var hash = BitConverter.ToString(hashBytes).Replace("-", string.Empty).Substring(0, 10);
            return $"{baseName}_{hash}{extension}";
        }

        public static void ClearTempFiles()
        {
            if (Directory.Exists(TempDirName))
                Directory.Delete(TempDirName, true);
        }
    }
}
