
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using LitJson;
using System.Reflection;
using System;
using System.Linq;

namespace Caddress {

    public static class JsonHelper {
        public static void RegistCustomTypes() {
            JsonMapper.RegisterImporter<string, Vector3>(Vector3ImporterFunc);
            JsonMapper.RegisterExporter<Vector3>(Vector3ExporterFunc);

            JsonMapper.RegisterImporter<string, Quaternion>(Quaternion3ImporterFunc);
            JsonMapper.RegisterExporter<Quaternion>(QuaternionExporterFunc);

            JsonMapper.RegisterImporter<string, Vector2>(Vector2ImporterFunc);
            JsonMapper.RegisterExporter<Vector2>(Vector2ExporterFunc);

            JsonMapper.RegisterImporter<string, Vector4>(Vector4ImporterFunc);
            JsonMapper.RegisterExporter<Vector4>(Vector4ExporterFunc);

            JsonMapper.RegisterImporter<string, Color>(ColorImporterFunc);
            JsonMapper.RegisterExporter<Color>(ColorExporterFunc);

            JsonMapper.RegisterImporter<string, Rect>(RectImporterFunc);
            JsonMapper.RegisterExporter<Rect>(RectExporterFunc);
        }

        public static string ToJsonStr(JsonData jsonData) {
            string result = null;
            try {
                result = jsonData.ToJson(true);
            }
            catch {
                CheckJsonData(jsonData);
            }
            return result;
        }

        public static string ToJsonStr(KeyValuePair<string, JsonData> dicData) {
            string result = null;
            try {
                result = $"{{\"{dicData.Key}\":{(dicData.Value).ToJson()}}}";
            }
            catch {
                CheckJsonData(dicData.Value);
            }
            return result;
        }


        public static void CheckJsonData(JsonData jd) {
            if (jd.IsArray) {
                var index = 0;
                foreach (var item in jd) {
                    Debug.LogError(index + ":" + item);
                    index++;
                }
                return;
            }
            var dict = jd.Inst_Object;
            foreach (var item in dict) {
                try {
                    var str = JsonMapper.ToJson(item.Value);
                }
                catch {
                    Debug.LogError(item.Key.ToString() + ":" + item.Value);
                    if (item.Value != null)
                        CheckJsonData(item.Value);
                }
            }
        }

        public static string ToJsonStr(object obj) {
            return JsonMapper.ToJson(obj);
        }

        public static JsonData ToJsonData(FieldInfo fieldInfo, object target) {
            var fieldType = fieldInfo.FieldType;
            var value = fieldInfo.GetValue(target);
            if (null == value)
                value = Activator.CreateInstance(fieldInfo.FieldType);
            if (fieldType == typeof(int)) {
                return new JsonData(value);
            }
            else if (fieldType == typeof(float)) {
                return new JsonData(value);
            }
            else if (fieldType == typeof(bool)) {
                return new JsonData(value);
            }
            else if (fieldType == typeof(Vector2)) {
                var jsonValue = Vector2ToString((Vector2)value);
                return new JsonData(jsonValue);
            }
            else if (fieldType == typeof(Vector3)) {
                var jsonValue = Vector3ToString((Vector3)value);
                return new JsonData(jsonValue);
            }
            else if (fieldType == typeof(Color)) {
                var jsonValue = ColorToString((Color)value);
                return new JsonData(jsonValue);
            }
            else if (fieldType == typeof(string)) {
                return new JsonData(value);
            }
            else if (fieldType.IsEnum) {
                return new JsonData(value.ToString());
            }
            else {
                return ToJsonData(value);
            }
        }

        public static JsonData ToJsonData(PropertyInfo propertyInfo, object target) {
            var propertyType = propertyInfo.PropertyType;
            var value = propertyInfo.GetValue(target, null);
            if (null == value)
                value = Activator.CreateInstance(propertyInfo.PropertyType);
            if (propertyType == typeof(int)) {
                return new JsonData(value);
            }
            else if (propertyType == typeof(float)) {
                return new JsonData(value);
            }
            else if (propertyType == typeof(bool)) {
                return new JsonData(value);
            }
            else if (propertyType == typeof(Vector2)) {
                var jsonValue = Vector2ToString((Vector2)value);
                return new JsonData(jsonValue);
            }
            else if (propertyType == typeof(Vector3)) {
                var jsonValue = Vector3ToString((Vector3)value);
                return new JsonData(jsonValue);
            }
            else if (propertyType == typeof(Color)) {
                var jsonValue = ColorToString((Color)value);
                return new JsonData(jsonValue);
            }
            else if (propertyType == typeof(string)) {
                return new JsonData(value);
            }
            else if (propertyType == typeof(Enum)) {
                return new JsonData(value.ToString());
            }
            else {
                return ToJsonData(value);
            }
        }

        public static string ToJsonStr<T>(T obj) {
            return JsonMapper.ToJson(obj);
        }

        public static JsonData ToJsonData(string jsonStr) {
            return JsonMapper.ToObject(jsonStr);
        }

        public static JsonData ToJsonData<T>(T obj) {
            return ToJsonData(JsonMapper.ToJson(obj));
        }

        public static T ToObject<T>(string jsonStr) {
            return JsonMapper.ToObject<T>(jsonStr);
        }

        public static T ToObject<T>(JsonData json) {
            return JsonMapper.ToObject<T>(json.ToJson());
        }

        public static T JsonToClass<T>(JsonData jsonData) where T : class, new() {
            return JsonToObject(typeof(T), jsonData) as T;
        }

        public static object JsonToObject(Type type, JsonData jd) {
            if (type == typeof(bool))
                return bool.Parse(jd.ToString());
            else if (type == typeof(int)) {
                int value = int.Parse(jd.ToString());
                return value;
            }
            else if (type == typeof(float)) {
                float value = float.Parse(jd.ToString());
                return value;
            }
            else if (type == typeof(string)) {
                return jd.ToString();
            }
            else if (type.IsEnum) {
                object value = Enum.Parse(type, jd.ToString());
                return value;
            }
            else if (type == typeof(UnityEngine.Vector3)) {
                string str = jd.ToString();
                string[] array;
                if (str.Contains(","))
                    array = jd.ToString().Split(',');
                else
                    array = jd.ToString().Split(' ');
                Vector3 value = new Vector3();
                value.x = float.Parse(array[0]);
                value.y = float.Parse(array[1]);
                value.z = float.Parse(array[2]);
                return value;
            }
            else if (type == typeof(UnityEngine.Vector4)) {
                return Vector4ImporterFunc(jd.ToString());
            }
            else if (type == typeof(UnityEngine.Vector2)) {
                return Vector2ImporterFunc(jd.ToString());
            }
            else if (type == typeof(UnityEngine.Color)) {
                return ColorImporterFunc(jd.ToString());
            }
            else if (type == typeof(UnityEngine.Quaternion)) {
                return Quaternion3ImporterFunc(jd.ToString());
            }
            else if (type == typeof(UnityEngine.AnimationCurve)) {
                return StringToAnimationCurve(JsonHelper.ToJsonStr(jd));
            }
            else if (type.IsArray) {
                if (jd.IsArray) {
                    Type eleType = type.GetElementType();
                    var value = Array.CreateInstance(eleType, jd.Count);
                    for (int i = 0; i < jd.Count; i++) {
                        object obj = JsonToObject(eleType, jd[i]);
                        value.SetValue(obj, i);
                    }
                    return value;
                }
            }
            else if (type.IsClass) {
                object value = Activator.CreateInstance(type);
                FieldInfo[] fields = type.GetFields();
                foreach (FieldInfo field in fields) {
                    if (jd.ContainsKey(field.Name)) {
                        object fieldValue = JsonToObject(field.FieldType, jd[field.Name]);
                        field.SetValue(value, fieldValue);
                    }
                }
                PropertyInfo[] props = type.GetProperties();
                foreach (PropertyInfo prop in props) {
                    if (jd.ContainsKey(prop.Name)) {
                        object propValue = JsonToObject(prop.PropertyType, jd[prop.Name]);
                        prop.SetValue(value, propValue, null);
                    }
                }
                return value;
            }
            else if (type == typeof(double)) {
                double value = double.Parse(jd.ToString());
                return value;
            }
            return null;
        }

        public static bool IsJson(this string jsonStr) {
            try {
                JsonData jd = JsonMapper.ToObject(jsonStr);
                return jd != null;
            }
            catch (Exception e) {
                return false;
            }
        }

        public static JsonData StrToJson(JsonType type, string str) {
            if (type == JsonType.Array || type == JsonType.Object)
                return ToJsonData(str);
            else if (type == JsonType.Boolean) {
                bool value = false;
                if (bool.TryParse(str, out value))
                    return new JsonData(value);
                else return null;
            }
            else if (type == JsonType.Int) {
                int value = 0;
                if (int.TryParse(str, out value))
                    return new JsonData(value);
                else return null;
            }
            else if (type == JsonType.String)
                return new JsonData(str);
            else if (type == JsonType.Long) {
                float value = 0f;
                if (float.TryParse(str, out value))
                    return new JsonData(value);
                else return null;
            }
            else if (type == JsonType.Double) {
                double value = 0f;
                if (double.TryParse(str, out value))
                    return new JsonData(value);
                else return null;
            }
            else
                return null;
        }

        #region JsonMapper Extension  

        static Vector3 Vector3ImporterFunc(string input) {
            string[] array = input.Split(',');
            Vector3 value = new Vector3();
            value.x = float.Parse(array[0]);
            value.y = float.Parse(array[1]);
            value.z = float.Parse(array[2]);
            return value;
        }

        static void Vector3ExporterFunc(Vector3 obj, JsonWriter writer) {
            writer.Write(obj.x.ToString() + "," + obj.y.ToString() + "," + obj.z.ToString());
        }

        static Vector4 Vector4ImporterFunc(string input) {
            string[] array = input.Split(',');
            Vector4 value = new Vector4();
            value.x = float.Parse(array[0]);
            value.y = float.Parse(array[1]);
            value.z = float.Parse(array[2]);
            value.w = float.Parse(array[3]);
            return value;
        }

        static void Vector4ExporterFunc(Vector4 obj, JsonWriter writer) {
            writer.Write(obj.x.ToString() + "," + obj.y.ToString() + "," + obj.z.ToString() + "," + obj.w.ToString());
        }

        static Vector2 Vector2ImporterFunc(string input) {
            string[] array = input.Split(',');
            Vector2 value = new Vector2();
            value.x = float.Parse(array[0]);
            value.y = float.Parse(array[1]);
            return value;
        }

        static void Vector2ExporterFunc(Vector2 obj, JsonWriter writer) {
            writer.Write(obj.x.ToString() + "," + obj.y.ToString());
        }

        static Color ColorImporterFunc(string input) {
            string hex = input;
            if (hex.Length == 9) {
                hex = hex.TrimStart(new char[] { '#' });
            }

            byte br = byte.Parse(hex.Substring(0, 2), System.Globalization.NumberStyles.HexNumber);
            byte bg = byte.Parse(hex.Substring(2, 2), System.Globalization.NumberStyles.HexNumber);
            byte bb = byte.Parse(hex.Substring(4, 2), System.Globalization.NumberStyles.HexNumber);
            float a = 1;
            if (hex.Length == 8) {
                byte cc = byte.Parse(hex.Substring(6, 2), System.Globalization.NumberStyles.HexNumber);
                a = cc / 255f;
            }

            float r = br / 255f;
            float g = bg / 255f;
            float b = bb / 255f;
            return new Color(r, g, b, a);
        }

        static void ColorExporterFunc(Color color, JsonWriter writer) {
            string jsonStr = null;
            jsonStr = ColorToHex(color);
            writer.Write(jsonStr);
        }

        static Quaternion Quaternion3ImporterFunc(string input) {
            string[] array = input.Split(',');
            Quaternion value = new Quaternion();
            value.x = float.Parse(array[0]);
            value.y = float.Parse(array[1]);
            value.z = float.Parse(array[2]);
            value.w = float.Parse(array[3]);
            return value;
        }

        static void QuaternionExporterFunc(Quaternion obj, JsonWriter writer) {
            writer.Write(obj.x.ToString() + "," + obj.y.ToString() + "," + obj.z.ToString() + "," + obj.w.ToString());
        }

        static Rect RectImporterFunc(string input) {
            string[] rectArr = input.Split(',');
            Rect rect = new Rect(float.Parse(rectArr[0]), float.Parse(rectArr[1]), float.Parse(rectArr[2]), float.Parse(rectArr[3]));
            return rect;
        }

        static void RectExporterFunc(Rect obj, JsonWriter writer) {
            writer.Write(string.Format("{0},{1},{2},{3}", obj.position.x, obj.position.y, obj.size.x, obj.size.y));
        }

        public static string Vector2ToString(Vector2 vec2) {
            string str = string.Empty;
            try {
                str = string.Format("{0},{1}", vec2.x.ToString("F2"), vec2.y.ToString("F2"));
            }
            catch (FormatException exception) {
                throw exception;
            }
            return str;
        }

        public static string Vector3ToString(Vector3 vec3, string split = " ") {
            string str = string.Empty;
            try {
                //str = string.Format("{0},{1},{2}",vec3.x.ToString("F2"),vec3.y.ToString("F2"),vec3.z.ToString("F2"));
                str = $"{vec3.x.ToString("F3")}{split}{vec3.y.ToString("F3")}{split}{vec3.z.ToString("F3")}";
            }
            catch (FormatException exception) {
                throw exception;
            }
            return str;
        }


        public static string RotationToString(Quaternion rot) {
            string str = string.Empty;
            try {
                str = $"{rot.x.ToString("F3")} {rot.y.ToString("F3")} {rot.z.ToString("F3")} {rot.w.ToString("F3")}";
            }
            catch (FormatException exception) {
                throw exception;
            }
            return str;
        }

        public static string ColorToString(Color color) {
            return ColorToHex(color);
        }

        public static Vector2 StringToVector2(string vecString) {
            Vector2 vec2 = Vector2.zero;
            try {
                string[] xy = vecString.Split(',');
                vec2.x = float.Parse(xy[0]);
                vec2.y = float.Parse(xy[1]);
            }
            catch (FormatException excetion) {
                throw excetion;
            }
            return vec2;
        }

        public static Vector3 StringToVector3(string vecString, char split = ',') {
            Vector3 vec3 = Vector3.zero;
            try {
                string[] xyz = vecString.Split(split);
                vec3.x = float.Parse(xyz[0]);
                vec3.y = float.Parse(xyz[1]);
                vec3.z = float.Parse(xyz[2]);
            }
            catch (FormatException excetion) {
                throw excetion;
            }
            return vec3;
        }

        public static Rect? StringToRect(string vecString) {
            return StringToRect(vecString);
        }

        public static Rect? StringToRect(string vecString, char split = ',') {
            Rect? rect = null;
            try {
                vecString = vecString.Replace("(", "");
                vecString = vecString.Replace(")", "");
                string[] xyzw = vecString.Split(split);
                Rect tmp = new Rect();
                Vector2 pos = new Vector2(float.Parse(xyzw[0]), float.Parse(xyzw[1]));
                tmp.size = new Vector2(float.Parse(xyzw[2]), float.Parse(xyzw[3]));
                tmp.position = pos - tmp.size / 2;
                rect = tmp;
            }
            catch (FormatException excetion) {
                throw excetion;
            }
            return rect;
        }

        public static Quaternion StringToQuaternion(string quaString, char split = ',') {
            Quaternion rot = Quaternion.identity;
            try {
                string[] xyz = quaString.Split(split);
                rot.x = float.Parse(xyz[0]);
                rot.y = float.Parse(xyz[1]);
                rot.z = float.Parse(xyz[2]);
                rot.w = float.Parse(xyz[3]);
            }
            catch (FormatException excetion) {
                throw excetion;
            }
            return rot;
        }

        public static Color StringToColor(string colorString) {
            return HexToColor(colorString);
        }

        public static AnimationCurve StringToAnimationCurve(string curveString) {
            if (string.IsNullOrEmpty(curveString))
                return null;
            var jsonData = JsonHelper.ToJsonData(curveString);
            if (!jsonData.IsArray)
                return null;
            AnimationCurve curve = new AnimationCurve();
            foreach (var keyframeObj in jsonData) {
                LitJson.JsonData kfData = keyframeObj as LitJson.JsonData;
                float time = float.Parse(kfData["time"].ToString());
                float value = float.Parse(kfData["value"].ToString());
                float inTangent = float.Parse(kfData["inTangent"].ToString());
                float outTangent = float.Parse(kfData["outTangent"].ToString());
                Keyframe keyframe = new Keyframe();
                keyframe.time = time;
                keyframe.value = value;
                keyframe.inTangent = inTangent;
                keyframe.outTangent = outTangent;
                curve.AddKey(keyframe);
            }
            return curve;
        }

        /// <summary>
        /// hex转换到color
        /// </summary>
        /// <param name="hexCol"></param>
        /// <returns></returns>
        public static Color HexToColor(string hexCol) {
            string hex = hexCol;
            if (hex.Length == 9) {
                hex = hex.TrimStart(new char[] { '#' });
            }

            byte br = byte.Parse(hex.Substring(0, 2), System.Globalization.NumberStyles.HexNumber);
            byte bg = byte.Parse(hex.Substring(2, 2), System.Globalization.NumberStyles.HexNumber);
            byte bb = byte.Parse(hex.Substring(4, 2), System.Globalization.NumberStyles.HexNumber);
            float a = 1;
            if (hex.Length == 8) {
                byte cc = byte.Parse(hex.Substring(6, 2), System.Globalization.NumberStyles.HexNumber);
                a = cc / 255f;
            }

            float r = br / 255f;
            float g = bg / 255f;
            float b = bb / 255f;
            return new Color(r, g, b, a);
        }

        /// <summary>
        /// color转换到hex
        /// </summary>
        /// <param name="color"></param>
        /// <returns></returns>
        public static string ColorToHex(Color color) {
            if (color == null)
                return "#000000";
            Color32 color32 = color;
            string R = Convert.ToString(color32.r, 16);
            if (R.Length == 1)
                R = "0" + R;
            string G = Convert.ToString(color32.g, 16);
            if (G.Length == 1)
                G = "0" + G;
            string B = Convert.ToString(color32.b, 16);
            if (B.Length == 1)
                B = "0" + B;
            string A = Convert.ToString(color32.a, 16);
            if (A.Length == 1)
                A = "0" + A;
            string HexColor = "#" + R + G + B + A;
            return HexColor.ToUpper();
        }

        public static void ReplaceKey(this JsonData target, string key, string replace) {
            if (target.ContainsKey(replace) || key == replace)
                return;
            string tempKey = replace.Trim();
            string tempReplace = replace.Trim();
            if (string.IsNullOrEmpty(tempKey) || string.IsNullOrEmpty(tempReplace)) return;
            var ins = target.Inst_Object;
            var keys = ins.Keys.ToArray();
            for (int i = 0; i < keys.Length; i++) {
                var _key = keys[i];
                if (_key == key) {
                    var value = target[_key];
                    target.Remove(key);
                    target[replace] = value;
                }
                else {
                    var value = target[_key];
                    target.Remove(_key);
                    target[_key] = value;
                }
            }
        }

        public static object First(this JsonData target) {
            if (target.IsObject && target.Inst_Object != null) {
                foreach (var item in target.Inst_Object) {
                    return item;
                }
                return target;
            }
            else if (target.IsArray)
                return target[0];
            else
                return target;
        }

        public static object IndexOf(this JsonData target, int index) {
            if (target.IsObject && target.Inst_Object != null) {
                var ids = 0;
                foreach (var item in target.Inst_Object) {
                    if (ids == index) {
                        return item;
                    }
                    ids++;
                }
                return target;
            }
            else
                return null;
        }

        #endregion
    }
}