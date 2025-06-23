
using System;
using System.IO;
using System.Text;
using UnityEngine;
using System.Collections;
using System.Diagnostics;
using Debug = UnityEngine.Debug;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using Ionic.Zip;

namespace Caddress.Tools
{
    public enum SVNCommandType
    {
        QueryRevision,
        QueryStatus,
        QueryRepositoryInfo
    }

    public enum SVNStatus
    {
        Unknown,
        Delete,
        OutOfControl,
        Modify,
        Add,
        Conflict,
        Lock
    }

    public class SVNUpgradeInfo
    {
        public SVNStatus status;
        public string path;
        public string absolutePath;

        public override string ToString()
        {
            return $"{this.status.ToString()}:{path}";
        }
    }

    public class SVNIgnoreCondition
    {
        public bool enable = true;
        public string path;
        public string pattern;

        /// <summary>
        /// SVNIgnoreCondition would convert all backslash(\) to slash(/) automatically
        /// </summary>
        /// <param name="srcCondition"></param>
        public SVNIgnoreCondition( string srcCondition )
        {
            var strArr = srcCondition.Split('@');
            UnityEngine.Assertions.Assert.IsTrue(strArr.Length >= 1 && strArr.Length <= 2);
            if (strArr.Length == 1)
            {
                this.path = strArr[0].Replace('\\','/');
            }
            else if (strArr.Length == 2)
            {
                this.path = strArr[0].Replace('\\', '/');
                this.pattern = strArr[1];
            }
        }

        public bool IsIgnored( string checkPath )
        {
            checkPath = checkPath.Replace('\\','/');
            if (string.IsNullOrEmpty(this.pattern))
            {
                var index = checkPath.IndexOf(this.path,System.StringComparison.OrdinalIgnoreCase);
                return index > -1;
            }
            else
            {
                bool contain = checkPath.IndexOf(this.path, System.StringComparison.OrdinalIgnoreCase) > -1;
                if (!contain)
                    return false;
                var index = checkPath.IndexOf(this.path, System.StringComparison.OrdinalIgnoreCase);
                int startIndex = index + this.path.Length;
                int subLength = checkPath.Length - startIndex;
                var str = checkPath.Substring(startIndex, subLength);
                str = str.TrimStart('/');
                return Regex.IsMatch(str,this.pattern);
            }
        }
    }

    public class SVNRepositoryInfo
    {
        public string localMachinePath = string.Empty;
        public string repoUrl = string.Empty;
        public string repoRootUrl = string.Empty;
        public int localRevision = -1;
        public int remoteRevision = -1;
    }

    public static class SVNTool
    {
        public static string SVNExecutableApplication = @"C:\Program Files (x86)\Subversion\bin\svn.exe";
        public const string EXPORT_FOLDER = "patch";

        private static Process getRevisionProcess = null;
        private static SVNCommandType commandType;
        private static Action<string> revisionCallback = null;

        private static string rootDirectory = null;
        private static Action<SVNUpgradeInfo> statusCallback = null;
        private static List<SVNUpgradeInfo> statusInfos = new List<SVNUpgradeInfo>();

        private static bool svnCommandComplete = false;
        private static Coroutine writeCoroutine = null;
        private static Action SVNCommandCompleted = null;

        private static SVNRepositoryInfo svnRepoInfo = null;
        private static Action<SVNRepositoryInfo> repositoryInfoCallback = null;

        private static Action compressCompletedCallback = null;
        private static Action<float> progressCallback = null;

        // 获取最高版本号
        public static void GetLatestRevision(string repoUrl, System.Action<string> callback)
        {
            revisionCallback = callback;
            SVNCommandCompleted = () =>
            {
                Debug.LogError("SVN query revision successfully.");
            };
            commandType = SVNCommandType.QueryRevision;
            string svnCommand = $"\"{SVNExecutableApplication}\" log {repoUrl}";
            ExecuteCommand(svnCommand);
        }

        // 获取当前路径SVN版本库信息
        public static void GetSVNRepositoryInfo( string directory , Action<SVNRepositoryInfo> callback )
        {
            repositoryInfoCallback = callback;
            SVNCommandCompleted = () =>
            {
                Debug.LogError("SVN query repository information successfully.");
            };
            commandType = SVNCommandType.QueryRepositoryInfo;
            string svnCommand = $"\"{SVNExecutableApplication}\" info {directory}";
            svnRepoInfo = new SVNRepositoryInfo();
            ExecuteCommand(svnCommand);
        }

        // TODO : 应该把打包的步骤从这里移除出去，保证SVNTool后续可在别处使用
        // 获取当前版本库和服务器端的差异信息
        public static void GetStatus(string directory, Action<SVNUpgradeInfo> callback ,bool genPatch = true , string patchName = "patch" ,List<SVNIgnoreCondition> ignoreConditions = null,Action compressCallback = null,Action<float> progress = null )
        {
            rootDirectory = directory;
            statusCallback = callback;
            compressCompletedCallback = compressCallback;
            progressCallback = progress;
            SVNCommandCompleted = () =>
            {
                GenPatch(statusInfos, rootDirectory ,patchName , ignoreConditions);
            };
            commandType = SVNCommandType.QueryStatus;
            string svnCommand = $"\"{SVNExecutableApplication}\" status {directory}";
            statusInfos.Clear();
            if (genPatch)
            {
                // TODO : 这个协程可能会导致打包假死
                svnCommandComplete = false;
                if (null != writeCoroutine)
                    CoroutineUtil.Instance.StopCoroutine(writeCoroutine);
                writeCoroutine = CoroutineUtil.Instance.StartCoroutine(waitForMasterThreadCallback());
            }
            ExecuteCommand(svnCommand);
        }

        #region execute svn command

        private static string ExecuteCommand(string command)
        {
            // UnityEngine.Debug.LogError($"execute command : {command}");
            try
            {
                //string strInput = Console.ReadLine();
                getRevisionProcess = new Process();
                //设置要启动的应用程序
                getRevisionProcess.StartInfo.FileName = "cmd.exe";
                //是否使用操作系统shell启动
                getRevisionProcess.StartInfo.UseShellExecute = false;
                // 接受来自调用程序的输入信息
                getRevisionProcess.StartInfo.RedirectStandardInput = true;
                //输出信息
                getRevisionProcess.StartInfo.RedirectStandardOutput = true;
                // 输出错误
                getRevisionProcess.StartInfo.RedirectStandardError = true;
                //不显示程序窗口
                getRevisionProcess.StartInfo.CreateNoWindow = true;

                getRevisionProcess.StartInfo.StandardOutputEncoding = Encoding.GetEncoding("GB2312");
                getRevisionProcess.StartInfo.StandardErrorEncoding = Encoding.GetEncoding("GB2312");

                getRevisionProcess.StartInfo.WindowStyle = ProcessWindowStyle.Normal;
                getRevisionProcess.StartInfo.WorkingDirectory = @"C:\users\"+ Environment.UserName;
                getRevisionProcess.StartInfo.Arguments = "/c C:\\Windows\\System32\\cmd.exe";
                getRevisionProcess.StartInfo.Verb = "RunAs";
                getRevisionProcess.OutputDataReceived += new DataReceivedEventHandler(ReceiveOutput);
                //启用Exited事件
                getRevisionProcess.EnableRaisingEvents = true;
                //getRevisionProcess.Exited += new EventHandler(Process_Exited);

                //启动程序
                getRevisionProcess.Start();
                getRevisionProcess.BeginOutputReadLine();
                getRevisionProcess.BeginErrorReadLine();
                getRevisionProcess.StandardInput.AutoFlush = true;
                //输入命令
                getRevisionProcess.StandardInput.WriteLine(command);
                getRevisionProcess.StandardInput.WriteLine("exit");

                //获取输出信息
                string strOuput = string.Empty;// getRevisionProcess.StandardOutput.ReadToEnd();
                                               //等待程序执行完退出进程
                getRevisionProcess.WaitForExit();
                //getRevisionProcess.Close();
                //getRevisionProcess.Dispose();
                // callback?.Invoke(strOuput);
                return strOuput;
                // getRevisionProcess.StandardOutput.ReadLine();
                // return string.Empty;
            }
            catch (Exception ex)
            {
                if (null != getRevisionProcess)
                {
                    getRevisionProcess.Kill();
                    getRevisionProcess.Close();
                    getRevisionProcess.Dispose();
                }
                // throw ex;
                Debug.LogWarning($"query svn info failed : {ex.Message}");
                return string.Empty;
            }
            finally
            {

            }
        }

        private static void ReceiveOutput(object sender, DataReceivedEventArgs e)
        {
            if (e.Data == null)
                return;

            if (Regex.IsMatch(e.Data, @">exit\z"))
            {
                getRevisionProcess.Close();
                getRevisionProcess.Dispose();
                svnCommandComplete = true;
                return;
            }

            if (commandType == SVNCommandType.QueryRevision)
                OnQueryRevision(e.Data);
            else if (commandType == SVNCommandType.QueryStatus)
                OnQueryStatus(e.Data);
            else if (commandType == SVNCommandType.QueryRepositoryInfo)
                OnQueryRepositroyInfo(e.Data);
        }

        #endregion

        #region query repository information

        private static void OnQueryRepositroyInfo( string input )
        {
            // 路径: D: \uinnova\Unity\09_Unity_2019_2_17\01_IBV_MAPPING
            // 工作副本根目录: D: \uinnova\Unity\09_Unity_2019_2_17\01_IBV_MAPPING
            // URL: https://192.168.1.99/svn/Unity/IBV2019
            // 正确的相对 URL: ^/ IBV2019
            // 版本库根: https://192.168.1.99/svn/Unity
            // 版本库 UUID: 22a9534b - 7d30 - 644b - 80e1 - 5a99bc71c024
            // 版本: 24620
            // 节点种类: 目录
            // 调度: 正常
            // 最后修改的作者: dongwei
            // 最后修改的版本: 24620
            // 最后修改的时间: 2020 - 08 - 13 19:23:37 + 0800(周四, 2020 - 08 - 13)

            if (string.IsNullOrEmpty(input))
                return;
            input = input.TrimStart().TrimEnd();

            string @out;

            if (TryMatch(input, @"\Asvn: E155007.*", out @out))
                repositoryInfoCallback?.Invoke(null);
            else if (TryMatch(input, @"\A路径: (.*)", out @out))
                svnRepoInfo.localMachinePath = @out;
            else if (TryMatch(input, @"\AURL: (.*)", out @out))
                svnRepoInfo.repoUrl = @out;
            else if (TryMatch(input, @"\A版本库根: (.*)", out @out))
                svnRepoInfo.repoRootUrl = @out;
            else if (TryMatch(input, @"\A版本: (.*)", out @out))
                svnRepoInfo.localRevision = int.Parse(@out);
            else if (TryMatch(input, @"\A最后修改的时间: (.*)", out @out) || TryMatch(input, @"\ALast Changed Date: (.*)", out @out))
                repositoryInfoCallback?.Invoke(svnRepoInfo);
            else
                ;// 
        }

        #endregion

        #region query revision information

        private static void OnQueryRevision(string revisionData)
        {
            var revision = ParseRevision(revisionData);
            if (!string.IsNullOrEmpty(revision))
            {
                getRevisionProcess.Kill();
                getRevisionProcess.Close();
                getRevisionProcess.Dispose();
                getRevisionProcess = null;
                revisionCallback?.Invoke(revision);
            }
        }
        private static string ParseRevision(string input)
        {
            if (Regex.IsMatch(input, @"\A(r\d*) |"))
            {
                var revision = Regex.Match(input, @"\A(r\d*) |").Result("$1");
                return revision;
            }
            return string.Empty;
        }

        #endregion

        #region query file / directory status

        private static void OnQueryStatus(string statusData)
        {
            // A：预定加入到版本库
            // !：被删除
            // M：内容被修改
            // C：发生冲突
            // ?：不在svn的控制中
            // K：被锁定
            var statusInfo = ParseQueryStatus(statusData);
            if (null == statusInfo || statusInfo.status == SVNStatus.Unknown)
                return;
            statusCallback?.Invoke(statusInfo);
            statusInfos.Add(statusInfo);
        }
        private static SVNUpgradeInfo ParseQueryStatus(string input)
        {
            input = input.TrimStart().TrimEnd();
            if (string.IsNullOrEmpty(input))
                return null;
            SVNUpgradeInfo result = new SVNUpgradeInfo();
            if (Regex.IsMatch(input, @"\AA       .*"))
            {
                var path = Regex.Match(input, @"\AA       (.*)").Result("$1");
                result.status = SVNStatus.Add;
                result.absolutePath = path;
                result.path = path.Replace(rootDirectory, "");
            }
            else if (Regex.IsMatch(input, @"\A\!       .*"))
            {
                var path = Regex.Match(input, @"\A\!       (.*)").Result("$1");
                result.status = SVNStatus.Delete;
                result.absolutePath = path;
                result.path = path.Replace(rootDirectory, "");
            }
            else if (Regex.IsMatch(input, @"\AM       .*"))
            {
                var path = Regex.Match(input, @"\AM       (.*)").Result("$1");
                result.status = SVNStatus.Modify;
                result.absolutePath = path;
                result.path = path.Replace(rootDirectory, "");
            }
            else if (Regex.IsMatch(input, @"\A\?       .*"))
            {
                var path = Regex.Match(input, @"\A\?       (.*)").Result("$1");
                result.status = SVNStatus.OutOfControl;
                result.absolutePath = path;
                result.path = path.Replace(rootDirectory, "");
            }
            else if (Regex.IsMatch(input, @"\AC       .*"))
            {
                var path = Regex.Match(input, @"\AC       (.*)").Result("$1");
                result.status = SVNStatus.Conflict;
                result.absolutePath = path;
                result.path = path.Replace(rootDirectory, "");
            }
            else if (Regex.IsMatch(input, @"\AK       .*"))
            {
                var path = Regex.Match(input, @"\AK       (.*)").Result("$1");
                result.status = SVNStatus.Lock;
                result.absolutePath = path;
                result.path = path.Replace(rootDirectory, "");
            }
            else
            {
                result.status = SVNStatus.Unknown;
            }
            return result;
        }

        #endregion

        private static IEnumerator waitForMasterThreadCallback() // 文件写出只能在Unity主进程里
        {
            while (true)
            {
                if (svnCommandComplete)
                {
                    SVNCommandCompleted?.Invoke();
                    yield break;
                }
                yield return new WaitForSeconds(0.1f);
            }
        }

        private static bool IsIgnore( string checkPath , List<SVNIgnoreCondition> ignoreConditions )
        {
            foreach (var ignoreCondition in ignoreConditions)
            {
                if (!ignoreCondition.enable)
                    continue;
                bool ignore = ignoreCondition.IsIgnored(checkPath);
                if (ignore)
                    return true;
            }
            return false;
        }

        public static void GenPatch(List<SVNUpgradeInfo> statusInfos, string exportPath , string patchName = "patch" , List<SVNIgnoreCondition> ignoreConditions = null)
        {
            
            var directory = $"C:\\EXPORT_FOLDER\\patch package";
            var path = $"{directory}\\upgrade.json";

            List<string> patchLog = new List<string>();
            try
            {
                int index = 0;
                foreach (var statusInfo in statusInfos)
                {
                    float progress= (index++ * 1.0f) / statusInfos.Count;
                    progressCallback?.Invoke(progress);

                    bool ignore = IsIgnore(statusInfo.absolutePath, ignoreConditions);
                    if (ignore)
                        continue;
                    
                    patchLog.Add(statusInfo.ToString());
                    // 把这文件拷贝到patch补丁包中
                    // 应该支持设置一些忽略文件夹和忽略具体文件的办法
                    if (statusInfo.status == SVNStatus.Delete)
                        continue;

                    var isFile = File.Exists(statusInfo.absolutePath);
                    if (isFile) // 执行文件拷贝
                    {
                        string dstPath = $"{directory}\\{statusInfo.path}";
                        FileInfo fi = new FileInfo(dstPath);
                        if (!Directory.Exists(fi.Directory.FullName))
                            Directory.CreateDirectory(fi.Directory.FullName);
                        File.Copy(statusInfo.absolutePath, dstPath, true);
                    }
                    else // 执行文件夹拷贝
                    {
                        string dstDir = $"{directory}\\{statusInfo.path}";
                        IOUtil.DirectoryCopy(statusInfo.absolutePath, dstDir, true);
                    }
                }

                //// 强制将build_log.json加入到status info
                //SVNUpgradeInfo buildInfo = new SVNUpgradeInfo();
                //buildInfo.status = SVNStatus.Modify;
                //buildInfo.path = "\\IBV-Client\\uinnova_Data\\StreamingAssets\\IBV\\Core\\build_log.json";
                //patchLog.Add(buildInfo.ToString());
                //string src = $"{Application.streamingAssetsPath}/IBV/Core/build_log.json";
                //string dst = $"{directory}{buildInfo.path}";
                //IOUtil.FileCopy(src, dst, true);
                //Debug.LogError($"copy from {src} \n to {dst}");
            }
            catch (Exception e)
            {
                if (Directory.Exists(directory))
                    Directory.Delete(directory, true);
                throw e;
            }

            try
            {
                if (!System.IO.File.Exists(path))
                    System.IO.Directory.CreateDirectory(directory);
                var log = JsonHelper.ToJsonStr(patchLog);
                System.IO.File.WriteAllText(path, log);
            }
            catch (Exception e)
            {
                Debug.LogError($"export upgrade info failed. error : {e.Message}");
            }

            try
            {
                string zipDst = $"{exportPath}\\patch\\{patchName}.zip";
                FileInfo fi = new FileInfo(zipDst);
                if (!Directory.Exists(fi.Directory.FullName))
                    Directory.CreateDirectory(fi.Directory.FullName);

                ZipFile zip = new ZipFile(System.Text.Encoding.UTF8);
                zip.AddDirectory($"{directory}\\CampusBuilder2020x64_cn", "CampusBuilder2020x64_cn");
                zip.AddFile(path, "");
                zip.Save(zipDst);

                if (Directory.Exists(directory))
                    Directory.Delete(directory, true);
                compressCompletedCallback?.Invoke();
                Debug.Log($"<color=green>成功打包补丁包!</color>");
            }
            catch (Exception e)
            {
                if (Directory.Exists(directory))
                    Directory.Delete(directory, true);
                throw e;
            }
        }

        private static bool TryMatch( string input, string pattern , out string @out)
        {
            if (Regex.IsMatch(input, pattern))
            {
                @out = Regex.Match(input, pattern).Result("$1");
                return true;
            }
            @out = string.Empty;
            return false;
        }
    }
}