using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Globalization;
using System;

namespace Caddress {

	public class StringConverter {
		public static string ToString(Rect rc) {
			string result = ToString(rc, "F3");
			return result;
		}

		public static string ToString(Rect rc, string format) {
			string result = rc.x.ToString(format) + " " + rc.y.ToString(format) + " " + rc.width.ToString(format) + " " + rc.height.ToString(format);
			return result;
		}

		public static string ToString(Vector3 vec) {
			string result = ToString(vec, "F3");
			return result;
		}

		public static string ToString(Vector4 vec) {
			string result = ToString(vec, "F3");
			return result;
		}

		public static string ToString(Vector2 vec) {
			string result = ToString(vec, "F3");
			return result;
		}

		public static string ToString(RangeInt rangeInt) {
			string result = rangeInt.start + " " + rangeInt.end;
			return result;
		}

		public static string ToString(Vector3 vec, string format) {
			string result = vec.x.ToString(format) + " " + vec.y.ToString(format) + " " + vec.z.ToString(format);
			return result;
		}

		public static string ToString(Vector4 vec, string format) {
			string result = vec.x.ToString(format) + " " + vec.y.ToString(format) + " " + vec.z.ToString(format) + " " + vec.w.ToString(format);
			return result;
		}

		public static string ToString(Vector2 vec, string format) {
			string result = vec.x.ToString(format) + " " + vec.y.ToString(format);
			return result;
		}

		public static string ToString(Quaternion q) {
			string result = ToString(q, "F3");
			return result;
		}

		public static string ToString(Quaternion q, string format) {
			string result = q.x.ToString(format) + " " + q.y.ToString(format) + " " + q.z.ToString(format) + " " + q.w.ToString(format);
			return result;
		}

		public static string ToString(Color clr, string format) {
			string result = clr.r.ToString(format) + " " + clr.g.ToString(format) + " " + clr.b.ToString(format) + " " + clr.a.ToString(format);
			return result;
		}

		public static string ToString(Color clr) {
			string result = clr.r.ToString() + " " + clr.g.ToString() + " " + clr.b.ToString() + " " + clr.a.ToString();
			return result;
		}

		public static string ToString(bool[] arr) {
			string str = "";
			for (int i = 0; i < arr.Length; i++) {
				if (arr[i])
					str += "1";
				else
					str += "0";
				if (i != arr.Length - 1)
					str += " ";
			}
			return str;
		}

		public static string ToString(List<string> list) {
			string str = "";
			for (int i = 0; i < list.Count; i++) {
				str += list[i];
				if (i != list.Count - 1)
					str += ",";
			}
			return str;
		}

		public static RangeInt ParseRangeInt(string param) {
			string[] temp = param.Split(' ');
			if (temp.Length == 1)
				temp = param.Split(',');
			int start;
			int end;
			if (!int.TryParse(temp[0], out start)) {
				start = (int)float.Parse(temp[0]);
			}
			if (!int.TryParse(temp[1], out end)) {
				end = (int)float.Parse(temp[1]);
			}
			RangeInt result = new RangeInt(start, end - start);
			return result;
		}

		public static Vector2 ParseVector2(string param) {
			string[] temp = param.Split(' ');
			if (temp.Length == 1)
				temp = param.Split(',');
			float x = float.Parse(temp[0]);
			float y = float.Parse(temp[1]);
			Vector2 result = new Vector2(x, y);
			return result;
		}

		public static Vector3 ParseVector3(string param) {
			string[] parts;
			Vector3 vec = new Vector3();
			// (10.0, 0.0, 0.0)
			if (param.StartsWith("(") && param.EndsWith(")")) {
				param = param.Substring(1, param.Length - 2);
				parts = param.Split(',');
			}
			// 10.0 0.0 0.0
			else {
				parts = param.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
			}
			if (parts.Length == 3) {
				try {
					float x = float.Parse(parts[0], CultureInfo.InvariantCulture);
					float y = float.Parse(parts[1], CultureInfo.InvariantCulture);
					float z = float.Parse(parts[2], CultureInfo.InvariantCulture);
					if (float.IsNaN(x)) {
						x = 0;
					}
					if (float.IsNaN(y)) {
						y = 0;
					}
					if (float.IsNaN(z)) {
						z = 0;
					}
					vec.x = x;
					vec.y = y;
					vec.z = z;
				}
				catch (FormatException) {
					Debug.LogError("无法将字符串转换为浮点数！");
				}
			}
			else {
				Debug.LogError("字符串分割后长度不等于3，无法转为 Vector3！");
			}

			return vec;
		}

		public static Vector3 ParseVector3(string param, char s) {
			string[] temp = param.Split(s);
			if (temp.Length == 1)
				temp = param.Split(',');
			float x = float.Parse(temp[0]);
			float y = float.Parse(temp[1]);
			float z = float.Parse(temp[2]);
			Vector3 result = new Vector3(x, y, z);
			return result;
		}

		public static Vector3[] ParseVector3Array(string param) {
			string[] temp = param.Split(' ');
			if (temp.Length == 1)
				temp = param.Split(',');
			int j = 0;
			Vector3[] result = new Vector3[temp.Length / 3];
			for (int i = 0; i < temp.Length; i += 3) {
				float x = float.Parse(temp[i + 0]);
				float y = float.Parse(temp[i + 1]);
				float z = float.Parse(temp[i + 2]);
				Vector3 v = new Vector3(x, y, z);
				result[j++] = v;
			}
			return result;
		}

		public static Vector4 ParseVector4(string param) {
			string[] temp = param.Split(' ');
			if (temp.Length == 1)
				temp = param.Split(',');
			float x = float.Parse(temp[0]);
			float y = float.Parse(temp[1]);
			float z = float.Parse(temp[2]);
			float w = float.Parse(temp[3]);
			Vector4 result = new Vector4(x, y, z, w);
			return result;
		}

		public static string[] ParseStringArray(string param) {
			string[] result = param.Split(' ');
			if (result.Length == 1)
				result = param.Split(',');
			int j = 0;
			return result;
		}

		public static List<string> ParseStringList(string param) {
			if (string.IsNullOrEmpty(param)) {
				return null;
			}
			List<string> result = param.Split(' ').ToList();
			if (result.Count == 1)
				result = param.Split(',').ToList();
			return result;
		}

		public static Quaternion ParseQuaternion(string param) {
			string[] temp = param.Split(' ');
			if (temp.Length == 1)
				temp = param.Split(',');
			float x = float.Parse(temp[0]);
			float y = float.Parse(temp[1]);
			float z = float.Parse(temp[2]);
			float w = float.Parse(temp[3]);
			Quaternion result = new Quaternion(x, y, z, w);
			return result;
		}

		public static Rect ParseRect(string param) {
			string[] temp = param.Split(' ');
			if (temp.Length == 1)
				temp = param.Split(',');
			float x = float.Parse(temp[0]);
			float y = float.Parse(temp[1]);
			float w = float.Parse(temp[2]);
			float h = float.Parse(temp[3]);
			Rect result = new Rect(x, y, w, h);
			return result;
		}

		public static Color ParseColor(string param) {
			string[] temp;
			if (param.StartsWith("(") && param.EndsWith(")")) {
				param = param.Substring(1, param.Length - 2);
				temp = param.Split(',');
			}
			else {
				temp = param.Split(' ');
			}

			float r = float.Parse(temp[0]);
			float g = float.Parse(temp[1]);
			float b = float.Parse(temp[2]);
			float a = float.Parse(temp[3]);
			Color result = new Color(r, g, b, a);
			return result;
		}



		public static bool[] ParseBoolArray(string param) {
			string[] temp = param.Split(' ');
			bool[] result = new bool[temp.Length];
			for (int i = 0; i < temp.Length; i++) {
				if (temp[i].CompareTo("1") == 0 || temp[i].CompareTo("True") == 0)
					result[i] = true;
				else
					result[i] = false;
			}
			return result;
		}

		public static string ToJsonArrayString(Vector3 vec) {

			return ToJsonArrayString(vec, "F3");
		}

		public static string ToJsonArrayString(Vector3 vec, string format) {
			string result = "[" + vec.x.ToString(format) + "," + vec.y.ToString(format) + "," + vec.z.ToString(format) + "]";
			return result;
		}

		public static string ToJsonArrayString(Quaternion quat) {
			return ToJsonArrayString(quat, "F3");
		}

		public static string ToJsonArrayString(Quaternion quat, string format) {
			string result = "[" + quat.x.ToString(format) + "," + quat.y.ToString(format) + "," + quat.z.ToString(format) + "," + quat.w.ToString(format) + "]";
			return result;
		}
	}
}