using LitJson;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Caddress.Template.Composite {
    public class RoomLevel : CompositeLevel<PlacementLevel> {

        protected string _roomType;
        public string RoomType {
            get {
                return _roomType;
            }
            set {
                _roomType = value;
            }
        }

        public override void SaveJson(JsonWriter jsonWriter) {
            base.SaveJson(jsonWriter);
            jsonWriter.WritePropertyName("roomType");
            jsonWriter.Write(_roomType);
        }

        public override void LoadJson(JsonData jsonData) {
            base.LoadJson(jsonData);
            if (jsonData.Inst_Object.ContainsKey("roomType") && jsonData.Inst_Object["roomType"].IsString) {
                _roomType = jsonData.Inst_Object["roomType"].ToString();
            }
        }

        protected override PlacementLevel NewChild(LevelType type) {
            return new PlacementLevel();
        }

    }
}