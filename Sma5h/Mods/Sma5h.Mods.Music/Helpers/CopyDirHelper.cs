using System.IO;

namespace Sma5h.Mods.Music.Helpers
{
    public static class CopyDirHelper
    {
        public static void Copy(string sourceDirectory, string targetDirectory, string searchPattern = "*")
        {
            DirectoryInfo diSource = new DirectoryInfo(sourceDirectory);
            DirectoryInfo diTarget = new DirectoryInfo(targetDirectory);

            CopyAll(diSource, diTarget, searchPattern);
        }

        private static void CopyAll(DirectoryInfo source, DirectoryInfo target, string searchPattern = "*")
        {
            Directory.CreateDirectory(target.FullName);

            foreach (FileInfo fi in source.GetFiles(searchPattern, SearchOption.AllDirectories))
            {
                fi.CopyTo(Path.Combine(target.FullName, fi.Name), true);
            }

            foreach (DirectoryInfo diSourceSubDir in source.GetDirectories())
            {
                DirectoryInfo nextTargetSubDir =
                    target.CreateSubdirectory(diSourceSubDir.Name);
                CopyAll(diSourceSubDir, nextTargetSubDir, searchPattern);
            }
        }
    }
}
