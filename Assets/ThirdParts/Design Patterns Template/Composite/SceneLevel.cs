using LitJson;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Caddress.Template.Composite {
    public class SceneLevel : CompositeLevel<BaseLevel> {

        protected string _sceneType;
        public string SceneType {
            get {
                return _sceneType;
            }
            set {
                _sceneType = value;
            }
        }

        public override void SaveJson(JsonWriter jsonWriter) {
            base.SaveJson(jsonWriter);
            jsonWriter.WritePropertyName("sceneType");
            jsonWriter.Write(_sceneType);
        }

        public override void LoadJson(JsonData jsonData) {
            base.LoadJson(jsonData);
            if (jsonData.Inst_Object.ContainsKey("sceneType") && jsonData.Inst_Object["sceneType"].IsString) {
                _sceneType = jsonData.Inst_Object["sceneType"].ToString();
            }
        }

        protected override BaseLevel NewChild(LevelType type) {
            return type switch {
                LevelType.Building => new BuildingLevel(),
                LevelType.Placement => new PlacementLevel(),
                _ => new PlacementLevel()
            };
        }
    }
}