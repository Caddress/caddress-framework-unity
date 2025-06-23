using LitJson;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Caddress.Template.Composite {
    public class FloorLevel : CompositeLevel<RoomLevel> {

        protected string _floorType;
        public string FloorType {
            get {
                return _floorType;
            }
            set {
                _floorType = value;
            }
        }

        public override void SaveJson(JsonWriter jsonWriter) {
            base.SaveJson(jsonWriter);
            jsonWriter.WritePropertyName("floorType");
            jsonWriter.Write(_floorType);
        }

        public override void LoadJson(JsonData jsonData) {
            base.LoadJson(jsonData);
            if (jsonData.Inst_Object.ContainsKey("floorType") && jsonData.Inst_Object["floorType"].IsString) {
                _floorType = jsonData.Inst_Object["floorType"].ToString();
            }
        }

        protected override RoomLevel NewChild(LevelType type) {
            return new RoomLevel();
        }

    }
}