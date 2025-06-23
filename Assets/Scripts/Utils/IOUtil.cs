
using System.IO;
using System.Text;
using System.Collections.Generic;
using System;

namespace Caddress {

    /// <summary>提供io能力，包含文件/文件夹的操作接口</summary>
    public static class IOUtil {
        /// <summary> 文件夹拷贝 reference : https://docs.microsoft.com/en-us/dotnet/standard/io/how-to-copy-directories </summary>
        /// <param name="sourceDirName">拷贝地址</param>
        /// <param name="destDirName">拷贝至</param>
        /// <param name="copySubDirs">是否拷贝子文件</param>
        public static void DirectoryCopy(string sourceDirName, string destDirName, bool copySubDirs) {
            DirectoryInfo dir = new DirectoryInfo(sourceDirName);
            if (!dir.Exists) {
                throw new DirectoryNotFoundException(
                    "Source directory does not exist or could not be found: "
                    + sourceDirName);
            }
            DirectoryInfo[] dirs = dir.GetDirectories();
            if (!Directory.Exists(destDirName))
                Directory.CreateDirectory(destDirName);
            FileInfo[] files = dir.GetFiles();
            foreach (FileInfo file in files) {
                string temppath = Path.Combine(destDirName, file.Name);
                file.CopyTo(temppath, true);
            }
            if (copySubDirs) {
                foreach (DirectoryInfo subdir in dirs) {
                    string temppath = Path.Combine(destDirName, subdir.Name);
                    DirectoryCopy(subdir.FullName, temppath, copySubDirs);
                }
            }
        }

        /// <summary> 清空文件夹 reference : https://social.msdn.microsoft.com/Forums/vstudio/en-US/9f4270bf-5784-4f83-a0c4-29742b1cb9d2/deleting-all-files-from-a-directory?forum=csharpgeneral  </summary>
        /// <param name="directoryPath">地址</param>
        public static void DirectoryClear(string directoryPath) {
            if (!directoryPath.EndsWith("/"))
                directoryPath += "/";
            if (!Directory.Exists(directoryPath)) {
                Directory.CreateDirectory(directoryPath);
                return;
            }
            System.IO.DirectoryInfo directory = new System.IO.DirectoryInfo(directoryPath);
            foreach (System.IO.FileInfo file in directory.GetFiles())
                file.Delete();
            foreach (System.IO.DirectoryInfo subDirectory in directory.GetDirectories())
                directory.Delete(true);
        }

        /// <summary>
        /// 删除文件夹
        /// </summary>
        public static void DeleteDirectory(string directoryPath) {
            if (!Directory.Exists(directoryPath))
                return;
            Directory.Delete(directoryPath, true);
        }

        /// <summary>文件夹是否存在</summary>
        /// <param name="directoryPath"></param>
        public static bool ExistDirectory(string directoryPath) {
            return Directory.Exists(directoryPath);
        }

        /// <summary>创建文件夹</summary>
        /// <param name="directoryPath"></param>
        public static void CreateDirectory(string directoryPath) {
            Directory.CreateDirectory(directoryPath);
        }

        /// <summary> 文件夹是否为空 </summary>
        public static bool IsEmptyDirectory(string directory) {
            bool exist = Directory.Exists(directory);
            if (!exist)
                return true;
            var subDirs = Directory.GetDirectories(directory);
            var subFiles = Directory.GetFiles(directory);
            return subDirs.Length == 0 && subFiles.Length == 0;
        }

        /// <summary>是否包含文件(递归检测)</summary>
        /// <param name="filePath">文件路径</param>
        /// <param name="directoryPath">根目录</param>
        public static bool ContainsFile(string filePath, string directoryPath) {
            if (!Directory.Exists(directoryPath) || !File.Exists(filePath))
                return false;
            var dir = directoryPath.Replace('\\', '/');
            var srcFile = filePath.Replace('\\', '/');
            return srcFile.Contains(dir);
        }

        public static bool IncludeFile(string dir, string fileName) {
            var files = Directory.GetFiles(dir, fileName);
            return files.Length > 0;
        }

        /// <summary>
        /// 文件拷贝，如果指定路径文件夹不存在则主动创建
        /// </summary>
        /// <param name="sourceFile">源文件路径</param>
        /// <param name="dstFile">目标路径</param>
        /// <param name="overWrite">是否覆盖 overWrite=true则进行覆盖，overWrite=false不进行覆盖</param>
        public static void FileCopy(string sourceFile, string dstFile, bool overWrite = false) {
            if (!File.Exists(sourceFile))
                return;
            FileInfo fileInfo = new FileInfo(dstFile);
            if (!Directory.Exists(fileInfo.Directory.FullName))
                Directory.CreateDirectory(fileInfo.Directory.FullName);
            if (File.Exists(dstFile))
                File.Delete(dstFile);
            File.Copy(sourceFile, dstFile, overWrite);
        }

        public static void RenameFile(string filePath, string newFileName) {
            if (File.Exists(filePath)) {
                string directory = Path.GetDirectoryName(filePath);
                string newFilePath = Path.Combine(directory, newFileName);

                if (!File.Exists(newFilePath)) {
                    File.Move(filePath, newFilePath);
                }
            }
        }

        public static void RenameFolder(string folderPath, string newFolderName) {
            if (Directory.Exists(folderPath)) {
                string parentDirectory = Path.GetDirectoryName(folderPath);
                string newFolderPath = Path.Combine(parentDirectory, newFolderName);

                if (!Directory.Exists(newFolderPath)) {
                    Directory.Move(folderPath, newFolderPath);
                }
            }
        }

        /// <summary>
        /// 文件是否存在
        /// </summary>
        /// <param name="filePath">文件路径</param>
        public static bool ExistFile(string filePath) {
            return File.Exists(filePath);
        }

        /// <summary>
        /// Gets the files.
        /// </summary>
        /// <param name="directory">The directory.</param>
        /// <param name="pattern">The pattern.</param>
        /// <param name="searchSubDir">if set to <c>true</c> [search sub dir].</param>
        /// <returns></returns>
        public static string[] GetFiles(string directory, string pattern, bool searchSubDir = true) {
            if (!Directory.Exists(directory))
                return null;
            return Directory.GetFiles(directory, pattern, searchSubDir ? SearchOption.AllDirectories : SearchOption.TopDirectoryOnly);
        }

        /// <summary>
        /// 如果存在指定文件则删除它
        /// </summary>
        /// <param name="sourceFile">删除已存在的文件</param>
        /// <return>如果成功删除则返回true</return>
        public static bool DeleteFile(string sourceFile) {
            if (File.Exists(sourceFile)) {
                File.Delete(sourceFile);
                return true;
            }
            return false;
        }

        /// <summary>
        /// 返回目录下及子目录下所有文件路径
        /// </summary>
        /// <param name="dstPath"></param>
        /// <returns></returns>
        public static List<string> GetAllFiles(string dstPath) {
            List<string> files = new List<string>();

            try {
                string[] currentFiles = IOUtil.GetFiles(dstPath, "*");
                files.AddRange(currentFiles);

                List<string> directories = new List<string>(Directory.GetDirectories(dstPath));
                foreach (string directory in directories) {
                    files.AddRange(GetAllFiles(directory));
                }
            }
            catch (Exception ex) {
                UnityEngine.Debug.LogError($"Failed to retrieve files: {ex.Message}");
            }
            return files;
        }

        /// <summary>读取文件内容</summary>
        /// <returns>返回文件内容</returns>
        public static string ReadFile(string filePath) {
            if (!File.Exists(filePath)) {
                UnityEngine.Debug.LogErrorFormat("[IBV.Util.IOUtil] : can not find file {0}.", filePath);
                return string.Empty;
            }
            return File.ReadAllText(filePath, Encoding.UTF8);
        }

        /// <summary>向文件写入内容</summary>
        /// <param name="filePath">文件路径</param>
        /// <param name="str">内容</param>
        public static void WriteFile(string filePath, string str) {
            try {
                if (!File.Exists(filePath)) {
                    FileInfo fi = new FileInfo(filePath);
                    if (!fi.Directory.Exists)
                        fi.Directory.Create();
                    var fileStream = File.Create(filePath);
                    fileStream.Close();
                    fileStream.Dispose();
                }
                File.WriteAllText(filePath, str);
            }
            catch (System.Exception e) {
                UnityEngine.Debug.LogErrorFormat("[IBV.Util.IOUtil] : write file {0} failed. error : {1}", filePath, e.Message);
            }
        }

        /// <summary>
        /// 获取文件字节大小
        /// </summary>
        /// <param name="file">文件路径</param>
        /// <returns>返回文件大小单位byte</returns>
        public static long GetFileSize(string file) {
            if (File.Exists(file)) {
                FileInfo fileInfo = new FileInfo(file);
                return fileInfo.Length;
            }
            return 0;
        }

        /// <summary>
        /// 获取文件夹中所有文件的字节大小
        /// </summary>
        /// <param name="directory">文件夹路径</param>
        /// <returns>返回大小单位byte</returns>
        public static long GetDirectorySize(string directory) {
            if (!Directory.Exists(directory))
                return 0;
            DirectoryInfo directoryInfo = new DirectoryInfo(directory);
            long size = 0;
            var files = directoryInfo.GetFiles(@"*.*", SearchOption.AllDirectories);
            foreach (var fileInfo in files)
                size += fileInfo.Length;
            return size;
        }

        /// <summary>
        /// 获取路径下所有文件夹
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        static List<string> allDirsForPath = new List<string>();
        public static List<string> GetAllDirectories(string path) {
            allDirsForPath = new List<string>();
            _CheckDirectory(path);
            return allDirsForPath;
        }
        static void _CheckDirectory(string path) {
            DirectoryInfo root = new DirectoryInfo(path);
            var dics = root.GetDirectories();
            foreach (DirectoryInfo d in dics) {
                if (!d.FullName.Contains(".")) {
                    allDirsForPath.Add(d.FullName);
                    _CheckDirectory(d.FullName);
                }
            }
        }
    }
}