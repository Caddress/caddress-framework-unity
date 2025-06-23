using System.Collections.Generic;
using System.Linq;

namespace Caddress.Tools {

    public class BuildInfo {
        public string buildDatetime;
        public string lastVersion;
        public string latestVersion;
        public string upgradeLog;
        public int buildDuration = 0;

    }

    public class BuildLog {
        private SortedDictionary<string, BuildInfo> buildInfos = null;

        public BuildInfo Latest() {
            if (null == buildInfos)
                return new BuildInfo() {
                    latestVersion = "V1.0.0",
                    buildDatetime = System.DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")
                };
            else
                return buildInfos.Values.Last();
        }

        public void Load() {
            // 从本地加载发布日志
            string logPath = UnityEngine.Application.streamingAssetsPath + $"/Logs/build_log.json";
            if (!System.IO.File.Exists(logPath))
                return;
            var log = System.IO.File.ReadAllText(logPath);
            buildInfos = JsonHelper.ToObject<SortedDictionary<string, BuildInfo>>(log);
        }

        public void Save(BuildInfo newInfo) {
            if (null != this.buildInfos) {
                this.buildInfos[newInfo.latestVersion] = newInfo;
            }
            else {
                this.buildInfos = new SortedDictionary<string, BuildInfo>() { { newInfo.latestVersion, newInfo } };
            }
            // 保存发布日志
            string logPath = UnityEngine.Application.streamingAssetsPath + $"/Logs/build_log.json";
            var log = JsonHelper.ToJsonStr(this.buildInfos);
            System.IO.File.WriteAllText(logPath, log);
        }

        public string SrcUrl() {
            return UnityEngine.Application.streamingAssetsPath + $"/Logs/build_log.json";
        }
    }

}