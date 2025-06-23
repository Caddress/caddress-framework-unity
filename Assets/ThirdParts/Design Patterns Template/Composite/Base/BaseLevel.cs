using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using LitJson;
using TriLib.Samples;

namespace Caddress.Template.Composite {

    public abstract class BaseLevel {

        protected string _name;
        public string Name {
            get {
                return _name;
            }
            set {
                _name = value;
            }
        }

        protected LevelType _type;
        public LevelType Type {
            get {
                return _type;
            }
            set {
                _type = value;
            }
        }

        protected Vector3 _position;
        public Vector3 Position {
            get {
                return _position;
            }
            set {
                _position = value;
            }
        }

        protected float _size;
        public float Size {
            get {
                return _size;
            }
            set {
                _size = value;
            }
        }

        protected Color _color;
        public Color Color {
            get {
                return _color;
            }
            set {
                _color = value;
            }
        }

        protected bool _enablePicked;
        public bool EnablePicked {
            get {
                return _enablePicked;
            }
            set {
                _enablePicked = value;
            }
        }

        protected List<string> _stringList = new List<string>();
        public List<string> StringList {
            get {
                return _stringList;
            }
        }

        public BaseLevel() {
            
        }

        public virtual void SaveJson(JsonWriter jsonWriter) {
            jsonWriter.WritePropertyName("name");
            jsonWriter.Write(_name);
            jsonWriter.WritePropertyName("type");
            jsonWriter.Write((int)_type);
            jsonWriter.WritePropertyName("position");
            jsonWriter.Write(StringConverter.ToString(_position));
            jsonWriter.WritePropertyName("size");
            jsonWriter.Write(_size);
            jsonWriter.WritePropertyName("color");
            jsonWriter.Write(StringConverter.ToString(_color));
            jsonWriter.WritePropertyName("enablePicked;");
            jsonWriter.Write(_enablePicked);
            if (_stringList != null && _stringList.Count > 0) {
                jsonWriter.WritePropertyName("stringList");
                jsonWriter.WriteArrayStart();
                foreach (var str in _stringList) {
                    jsonWriter.Write(str);
                }
                jsonWriter.WriteArrayEnd();
            }
        }


        public virtual void LoadJson(JsonData jsonData) {
            if (jsonData.Inst_Object.ContainsKey("name") && jsonData.Inst_Object["name"].IsString) {
                _name = jsonData.Inst_Object["name"].ToString();
            }
            if (jsonData.Inst_Object.ContainsKey("type") && jsonData.Inst_Object["type"].IsInt) {
                _type = (LevelType)(int)jsonData.Inst_Object["type"];
            }
            if (jsonData.Inst_Object.ContainsKey("position") && jsonData.Inst_Object["position"].IsString) {
                _position = StringConverter.ParseVector3(jsonData.Inst_Object["position"].ToString());
            }
            if (jsonData.Inst_Object.ContainsKey("size") && jsonData.Inst_Object["size"].IsDouble) {
                _size = float.Parse(jsonData.Inst_Object["size"].ToString());
            }
            if (jsonData.Inst_Object.ContainsKey("color") && jsonData.Inst_Object["color"].IsString) {
                _color = StringConverter.ParseColor(jsonData.Inst_Object["color"].ToString());
            }
            if (jsonData.Inst_Object.ContainsKey("enablePicked") && jsonData.Inst_Object["enablePicked"].IsBoolean) {
                _enablePicked = (bool)jsonData.Inst_Object["enablePicked"];
            }
            if (jsonData.Inst_Object.ContainsKey("stringList")) {
                var stringArray = jsonData.Inst_Object["stringList"];
                if (_stringList == null) {
                    _stringList = new List<string>();
                }
                else {
                    _stringList.Clear();
                }
                if (stringArray.IsArray) {
                    for (int i = 0; i < stringArray.Count; i++) {
                        _stringList.Add(stringArray[i].ToString());
                    }
                }
            }
        }
    }
}