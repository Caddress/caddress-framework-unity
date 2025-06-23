using LitJson;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Caddress.Template.Composite {
    public class PlacementLevel : BaseLevel {

        protected string _modelID;
        public string ModelID {
            get {
                return _modelID;
            }
            set {
                _modelID = value;
            }
        }

        public override void SaveJson(JsonWriter jsonWriter) {
            base.SaveJson(jsonWriter);
            jsonWriter.WritePropertyName("modelID");
            jsonWriter.Write(_modelID);
        }

        public override void LoadJson(JsonData jsonData) {
            base.LoadJson(jsonData);
            if (jsonData.Inst_Object.ContainsKey("modelID") && jsonData.Inst_Object["modelID"].IsString) {
                _modelID = jsonData.Inst_Object["modelID"].ToString();
            }
        }
    }
}