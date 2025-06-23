using System;
using System.Collections.Generic;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;

namespace Caddress.Tools {
    public class ReleaseTool : Editor {
        [MenuItem("Caddress/Build Tool")]
        static void Build() {
            ReleaseToolWindow.OpenBuildWindow();
        }
    }

    public class CarryResourcePattern {
        public enum CarryMode {
            /// <summary>
            /// 从源码中拷贝
            /// </summary>
            Source,
            /// <summary>
            /// 从发布资源中拷贝
            /// </summary>
            Build
        }

        public bool enable;
        public string pattern;
        public CarryMode mode;

        public CarryResourcePattern(string pattern, bool enable, CarryMode carryMode = CarryMode.Source) {
            this.mode = carryMode;
            this.pattern = pattern;
            this.enable = enable;
        }
    }

    public class ReleaseToolWindow : EditorWindow {

        private const string BUILD_TO_DIRECTORY = "CADDRESS_DIRECTORY";
        private const string PRODUCT_NAME = "Caddress System 2025 ";

        private bool buildCompleted = false;
        private bool carryResourcesToBuildDirectory = false;
        private int lV, sV, pV;
        private string log = "";
        private string targetVersion = "";
        private string buildToDirectory = string.Empty;
        private string executableApplicationName = "Caddress";
        private SVNRepositoryInfo outputRepositoryInfo = null;
        private BuildInfo lastBuildInfo;
        public BuildInfo LastBuildInfo {
            get {
                return lastBuildInfo;
            }
        }
        private BuildInfo currentBuildInfo = new BuildInfo();
        private BuildLog buildLog;
        private System.Diagnostics.Stopwatch watcher = new System.Diagnostics.Stopwatch();

        private List<CarryResourcePattern> resPatterns = new List<CarryResourcePattern>()
        {
        new CarryResourcePattern("*.exe",true,CarryResourcePattern.CarryMode.Source),
    };

        private List<SVNIgnoreCondition> ignoredConditions = new List<SVNIgnoreCondition>()
        {
        new SVNIgnoreCondition(@"Caddress\logs"),
    };

        public static void OpenBuildWindow() {
            var window = GetWindow<ReleaseToolWindow>(true, "Release Tool", true);
            window.Show();
        }

        private void OnGUI() {

            if (null == this.buildLog) {
                this.buildLog = new BuildLog();
                this.buildLog.Load();
            }
            this.lastBuildInfo = this.buildLog.Latest();

            if (PlayerPrefs.HasKey(BUILD_TO_DIRECTORY)) {
                buildToDirectory = PlayerPrefs.GetString(BUILD_TO_DIRECTORY);
            }

            GUI.color = Color.yellow;
            EditorGUILayout.LabelField("当前版本信息：");
            GUI.color = Color.white;
            EditorGUILayout.LabelField("当前版本：", lastBuildInfo.latestVersion);
            EditorGUILayout.LabelField("版本发布时间：", lastBuildInfo.buildDatetime);
            EditorGUILayout.LabelField("打包耗时(秒)：", $"{lastBuildInfo.buildDuration * 0.001f}秒");

            EditorGUILayout.Space();
            GUI.color = Color.yellow;
            EditorGUILayout.LabelField("本次发布信息：");
            GUI.color = Color.white;

            GUI.color = Color.gray;
            int largeVersion = 0, smallVersion = 0, patchVersion = 0;
            try {
                // 解析版本号信息
                var vStrs = lastBuildInfo.latestVersion.Replace(PRODUCT_NAME + "V", "").Split('.');
                largeVersion = int.Parse(vStrs[0]);
                smallVersion = lV > 0 ? 0 : int.Parse(vStrs[1]);
                patchVersion = (lV > 0 || sV > 0) ? 0 : int.Parse(vStrs[2]);
            }
            catch (System.Exception e) {
                Debug.LogError($"解析已有版本号错误");
                throw e;
            }

            GUILayout.BeginHorizontal();

            // 增加一个补丁号
            GUI.color = Color.green;
            if (GUILayout.Button("增加一个补丁版本号")) {
                pV += 1;
            }
            // 增加一个小版本号
            GUI.color = Color.yellow;
            if (GUILayout.Button("增加一个小版本号")) {
                pV = 0;
                sV += 1;
            }
            // 增加一个大版本号
            GUI.color = Color.red;
            if (GUILayout.Button("增加一个大版本号")) {
                lV += 1;
                pV = sV = 0;
            }
            targetVersion = $"{PRODUCT_NAME}V{largeVersion + lV}.{(smallVersion + sV).ToString("D2")}.{(patchVersion + pV).ToString("D3")}";

            GUI.color = Color.white;
            GUILayout.EndHorizontal();
            EditorGUILayout.LabelField("新增/修复 日志");
            if (string.IsNullOrEmpty(log)) {
                log = $"{targetVersion}({System.DateTime.Now.ToString("yyyy.MM.dd")}) \n" +
                   "Add ... \n" +
                   "Fix ... \n";
            }
            log = GUILayout.TextArea(log);
            EditorGUILayout.Space();

            EditorGUILayout.LabelField($"选择打包至 : {buildToDirectory}");
            if (string.IsNullOrEmpty(buildToDirectory))
                EditorGUILayout.HelpBox("必须选择打包路径", MessageType.Error);
            if (GUILayout.Button("选择打包路径")) {
                buildToDirectory = EditorUtility.SaveFolderPanel("选择打包路径", "", "");
                PlayerPrefs.SetString(BUILD_TO_DIRECTORY, buildToDirectory);
                SVNTool.GetSVNRepositoryInfo(buildToDirectory, repoInfo => { outputRepositoryInfo = repoInfo; });
            }
            if (null == outputRepositoryInfo && !string.IsNullOrEmpty(buildToDirectory)) {
                SVNTool.GetSVNRepositoryInfo(buildToDirectory, repoInfo => { outputRepositoryInfo = repoInfo; });
            }
            if (null != outputRepositoryInfo) {
                EditorGUILayout.LabelField($"当前发布版Url : {outputRepositoryInfo.repoUrl}");
                EditorGUILayout.LabelField($"当前发布版Revision : {outputRepositoryInfo.localRevision}");
            }

            GUILayout.BeginHorizontal();
            EditorGUILayout.LabelField($"发布后版本号：{targetVersion}");
            PlayerSettings.productName = targetVersion;
            GUILayout.EndHorizontal();

            bool valid = lastBuildInfo.latestVersion != targetVersion;

            if (!valid) {
                EditorGUILayout.HelpBox("版本号必须有所变化!", MessageType.Error);
            }
            valid &= !string.IsNullOrEmpty(buildToDirectory);

            GUI.color = valid ? Color.green : Color.red;
            if (GUILayout.Button("确认并发布") && valid) {
                GUI.color = Color.white;
                currentBuildInfo.lastVersion = currentBuildInfo.latestVersion;
                currentBuildInfo.latestVersion = targetVersion;
                currentBuildInfo.upgradeLog = this.log;

                Build();
            }

            if (GUILayout.Button("生成补丁包")) {
                try {
                    Debug.Log($"开始打包补丁包!");
                    var buildRoot = buildToDirectory.Substring(0, buildToDirectory.LastIndexOf('/'));
                    buildRoot = buildRoot.Substring(0, buildRoot.LastIndexOf('/'));
                    buildRoot = buildRoot.Replace("/", "\\");
                    var patchName = "Patch";
                    SVNTool.GetStatus(buildRoot, status => {

                    }, true, patchName, ignoredConditions, () => {
                        EditorUtility.ClearProgressBar();
                    }, progress => {
                        Debug.Log((progress * 100).ToString("0.##") + "%");
                    });
                }
                catch (Exception ex) {
                    Debug.LogError($"Failed to Patch: {ex.Message}");
                }
            }
        }

        private void Build() {
            watcher.Start();
            string[] levels = new string[]
            {
                "Assets/Scenes/Main.unity",
            };
            if (string.IsNullOrEmpty(buildToDirectory))
                return;

            // Build player.
            BuildPipeline.BuildPlayer(
                levels,
                buildToDirectory + $"/{executableApplicationName}.exe",
                BuildTarget.StandaloneWindows64,
                BuildOptions.None);
            this.buildCompleted = true;

            watcher.Stop();
            this.currentBuildInfo.buildDuration = (int)watcher.ElapsedMilliseconds;
            this.currentBuildInfo.buildDatetime = System.DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
            this.buildLog.Save(currentBuildInfo);

            // 拷贝构建信息
            string srcPath = this.buildLog.SrcUrl();
            string dstPath = buildToDirectory + $"/{executableApplicationName}_Data/StreamingAssets/Logs/build_log.json";
            IOUtil.FileCopy(srcPath, dstPath, true);

            if (this.carryResourcesToBuildDirectory) {
                Debug.LogError("构建信息已拷贝至打包目录!");
                var appRoot = Application.dataPath.Substring(0, Application.dataPath.LastIndexOf("/"));
                foreach (var resPattern in resPatterns) {
                    if (!resPattern.enable)
                        continue;

                    var carryFiles = System.IO.Directory.GetFiles(appRoot, resPattern.pattern);
                    foreach (var carryFile in carryFiles) {
                        var str = carryFile.Replace('\\', '/');
                        var fileName = str.Substring(str.LastIndexOf('/') + 1);
                        string dst = $"{buildToDirectory}/{fileName}";
                        IOUtil.FileCopy(carryFile, dst, true);
                        Debug.Log($"<color=green>自动拷贝{carryFile}到打包目录:{dst}</color>");
                    }
                }
            }

            // 拷贝exe
            string dracoxSrcPath = $"{Application.dataPath}/Plugins/plugin.exe";
            string dracoxDstPath = $"{buildToDirectory}/{executableApplicationName}_Data/Plugins/plugin.exe";
            if (IOUtil.ExistFile(dracoxSrcPath)) {
                Debug.Log($"<color=green>成功拷贝plugin.exe</color>");
                IOUtil.FileCopy(dracoxSrcPath, dracoxDstPath, true);
            }

            //替换StreamingAssets
            string streamingSourcePath = $"{Application.dataPath.Substring(0, Application.dataPath.LastIndexOf("/"))}/ReleaseStreamingAssets";
            string streamingDstPath = $"{buildToDirectory}/{executableApplicationName}_Data/StreamingAssets";
            if (IOUtil.ExistDirectory(streamingDstPath)) {
                IOUtil.DeleteDirectory(streamingDstPath);
            }
            if (IOUtil.ExistDirectory(streamingSourcePath)) {
                Debug.Log($"<color=green>成功复制新StreamingAssets文件夹</color>");
                IOUtil.DirectoryCopy(streamingSourcePath, streamingDstPath, true);
            }

            this.DrawBuildInfo();
        }

        private void DrawBuildInfo() {
            if (!buildCompleted)
                return;
            GUI.color = Color.green;
            EditorGUILayout.LabelField($"发布耗时：{this.currentBuildInfo.buildDuration * 0.001f}秒");
        }
    }
}

#endif