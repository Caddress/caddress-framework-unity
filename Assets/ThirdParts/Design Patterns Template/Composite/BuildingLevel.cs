using LitJson;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Caddress.Template.Composite {
    public class BuildingLevel : CompositeLevel<FloorLevel> {

        protected string _buildingType;
        public string BuildingType {
            get {
                return _buildingType;
            }
            set {
                _buildingType = value;
            }
        }

        public override void SaveJson(JsonWriter jsonWriter) {
            base.SaveJson(jsonWriter);
            jsonWriter.WritePropertyName("buildingType");
            jsonWriter.Write(_buildingType);
        }

        public override void LoadJson(JsonData jsonData) {
            base.LoadJson(jsonData);
            if (jsonData.Inst_Object.ContainsKey("buildingType") && jsonData.Inst_Object["buildingType"].IsString) {
                _buildingType = jsonData.Inst_Object["buildingType"].ToString();
            }
        }

        protected override FloorLevel NewChild(LevelType type) {
            return new FloorLevel();
        }

    }
}