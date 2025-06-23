using Caddress.Template.Composite;
using LitJson;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Caddress.Template.Composite {
    public abstract class CompositeLevel<TChild> : BaseLevel where TChild : BaseLevel {
        protected List<TChild> _levelList = new List<TChild>();

        public override void SaveJson(JsonWriter jsonWriter) {
            base.SaveJson(jsonWriter);
            var levelList = _levelList;
            if (levelList != null && levelList.Count > 0) {
                jsonWriter.WritePropertyName("level");
                jsonWriter.WriteArrayStart();
                foreach (var level in levelList) {
                    jsonWriter.WriteObjectStart();
                    level.SaveJson(jsonWriter);
                    jsonWriter.WriteObjectEnd();
                }
                jsonWriter.WriteArrayEnd();
            }
        }

        public override void LoadJson(JsonData jsonData) {
            base.LoadJson(jsonData);
            if (jsonData.Inst_Object.ContainsKey("level")) {
                var levelArray = jsonData.Inst_Object["level"];
                if (_levelList == null) {
                    _levelList = new List<TChild>();
                }
                else {
                    _levelList.Clear();
                }
                if (levelArray.IsArray) {
                    for (int i = 0; i < levelArray.Count; i++) {
                        var levelData = levelArray[i];
                        var level = NewChild(_type);
                        level.LoadJson(levelData);
                        _levelList.Add(level);
                    }
                }
            }
        }

        protected abstract TChild NewChild(LevelType type);
    }
}