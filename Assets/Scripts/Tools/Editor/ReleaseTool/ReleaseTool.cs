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
            /// ��Դ���п���
            /// </summary>
            Source,
            /// <summary>
            /// �ӷ�����Դ�п���
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
            EditorGUILayout.LabelField("��ǰ�汾��Ϣ��");
            GUI.color = Color.white;
            EditorGUILayout.LabelField("��ǰ�汾��", lastBuildInfo.latestVersion);
            EditorGUILayout.LabelField("�汾����ʱ�䣺", lastBuildInfo.buildDatetime);
            EditorGUILayout.LabelField("�����ʱ(��)��", $"{lastBuildInfo.buildDuration * 0.001f}��");

            EditorGUILayout.Space();
            GUI.color = Color.yellow;
            EditorGUILayout.LabelField("���η�����Ϣ��");
            GUI.color = Color.white;

            GUI.color = Color.gray;
            int largeVersion = 0, smallVersion = 0, patchVersion = 0;
            try {
                // �����汾����Ϣ
                var vStrs = lastBuildInfo.latestVersion.Replace(PRODUCT_NAME + "V", "").Split('.');
                largeVersion = int.Parse(vStrs[0]);
                smallVersion = lV > 0 ? 0 : int.Parse(vStrs[1]);
                patchVersion = (lV > 0 || sV > 0) ? 0 : int.Parse(vStrs[2]);
            }
            catch (System.Exception e) {
                Debug.LogError($"�������а汾�Ŵ���");
                throw e;
            }

            GUILayout.BeginHorizontal();

            // ����һ��������
            GUI.color = Color.green;
            if (GUILayout.Button("����һ�������汾��")) {
                pV += 1;
            }
            // ����һ��С�汾��
            GUI.color = Color.yellow;
            if (GUILayout.Button("����һ��С�汾��")) {
                pV = 0;
                sV += 1;
            }
            // ����һ����汾��
            GUI.color = Color.red;
            if (GUILayout.Button("����һ����汾��")) {
                lV += 1;
                pV = sV = 0;
            }
            targetVersion = $"{PRODUCT_NAME}V{largeVersion + lV}.{(smallVersion + sV).ToString("D2")}.{(patchVersion + pV).ToString("D3")}";

            GUI.color = Color.white;
            GUILayout.EndHorizontal();
            EditorGUILayout.LabelField("����/�޸� ��־");
            if (string.IsNullOrEmpty(log)) {
                log = $"{targetVersion}({System.DateTime.Now.ToString("yyyy.MM.dd")}) \n" +
                   "Add ... \n" +
                   "Fix ... \n";
            }
            log = GUILayout.TextArea(log);
            EditorGUILayout.Space();

            EditorGUILayout.LabelField($"ѡ������ : {buildToDirectory}");
            if (string.IsNullOrEmpty(buildToDirectory))
                EditorGUILayout.HelpBox("����ѡ����·��", MessageType.Error);
            if (GUILayout.Button("ѡ����·��")) {
                buildToDirectory = EditorUtility.SaveFolderPanel("ѡ����·��", "", "");
                PlayerPrefs.SetString(BUILD_TO_DIRECTORY, buildToDirectory);
                SVNTool.GetSVNRepositoryInfo(buildToDirectory, repoInfo => { outputRepositoryInfo = repoInfo; });
            }
            if (null == outputRepositoryInfo && !string.IsNullOrEmpty(buildToDirectory)) {
                SVNTool.GetSVNRepositoryInfo(buildToDirectory, repoInfo => { outputRepositoryInfo = repoInfo; });
            }
            if (null != outputRepositoryInfo) {
                EditorGUILayout.LabelField($"��ǰ������Url : {outputRepositoryInfo.repoUrl}");
                EditorGUILayout.LabelField($"��ǰ������Revision : {outputRepositoryInfo.localRevision}");
            }

            GUILayout.BeginHorizontal();
            EditorGUILayout.LabelField($"������汾�ţ�{targetVersion}");
            PlayerSettings.productName = targetVersion;
            GUILayout.EndHorizontal();

            bool valid = lastBuildInfo.latestVersion != targetVersion;

            if (!valid) {
                EditorGUILayout.HelpBox("�汾�ű��������仯!", MessageType.Error);
            }
            valid &= !string.IsNullOrEmpty(buildToDirectory);

            GUI.color = valid ? Color.green : Color.red;
            if (GUILayout.Button("ȷ�ϲ�����") && valid) {
                GUI.color = Color.white;
                currentBuildInfo.lastVersion = currentBuildInfo.latestVersion;
                currentBuildInfo.latestVersion = targetVersion;
                currentBuildInfo.upgradeLog = this.log;

                Build();
            }

            if (GUILayout.Button("���ɲ�����")) {
                try {
                    Debug.Log($"��ʼ���������!");
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

            // ����������Ϣ
            string srcPath = this.buildLog.SrcUrl();
            string dstPath = buildToDirectory + $"/{executableApplicationName}_Data/StreamingAssets/Logs/build_log.json";
            IOUtil.FileCopy(srcPath, dstPath, true);

            if (this.carryResourcesToBuildDirectory) {
                Debug.LogError("������Ϣ�ѿ��������Ŀ¼!");
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
                        Debug.Log($"<color=green>�Զ�����{carryFile}�����Ŀ¼:{dst}</color>");
                    }
                }
            }

            // ����exe
            string dracoxSrcPath = $"{Application.dataPath}/Plugins/plugin.exe";
            string dracoxDstPath = $"{buildToDirectory}/{executableApplicationName}_Data/Plugins/plugin.exe";
            if (IOUtil.ExistFile(dracoxSrcPath)) {
                Debug.Log($"<color=green>�ɹ�����plugin.exe</color>");
                IOUtil.FileCopy(dracoxSrcPath, dracoxDstPath, true);
            }

            //�滻StreamingAssets
            string streamingSourcePath = $"{Application.dataPath.Substring(0, Application.dataPath.LastIndexOf("/"))}/ReleaseStreamingAssets";
            string streamingDstPath = $"{buildToDirectory}/{executableApplicationName}_Data/StreamingAssets";
            if (IOUtil.ExistDirectory(streamingDstPath)) {
                IOUtil.DeleteDirectory(streamingDstPath);
            }
            if (IOUtil.ExistDirectory(streamingSourcePath)) {
                Debug.Log($"<color=green>�ɹ�������StreamingAssets�ļ���</color>");
                IOUtil.DirectoryCopy(streamingSourcePath, streamingDstPath, true);
            }

            this.DrawBuildInfo();
        }

        private void DrawBuildInfo() {
            if (!buildCompleted)
                return;
            GUI.color = Color.green;
            EditorGUILayout.LabelField($"������ʱ��{this.currentBuildInfo.buildDuration * 0.001f}��");
        }
    }
}

#endif