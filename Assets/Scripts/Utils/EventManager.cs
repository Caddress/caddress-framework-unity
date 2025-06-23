using UnityEngine;
using System.Collections.Generic;
using System;

namespace Caddress.Common {
    public class EventContent {
        public static string OnObjectSelected = "OnObjectSelected";
        public static string OnObjectDeselected = "OnObjectDeselected";
        public static string OnMouseLeaveObject = "OnMouseLeaveObject";
        public static string OnMouseEnterObject = "OnMouseEnterObject";

        public static string OnClickObject = "OnClickObject";
        public static string OnLeftClickObject = "OnLeftClickObject";
        public static string OnRightClickObject = "OnRightClickObject";
        public static string OnLeftClick = "OnLeftClick";
        public static string OnRightClick = "OnRightClick";

        public static string OnDbClickObject = "OnDbClickObject";
        public static string OnLeftDbClickObject = "OnLeftDbClickObject";
        public static string OnRightDbClickObject = "OnRightDbClickObject";
        public static string OnLeftDbClick = "OnLeftDbClick";
        public static string OnRightDbClick = "OnRightDbClick";

        public static string OnDelayClickObject = "OnDelayClickObject";
        public static string OnLeftDelayClickObject = "OnLeftDelayClickObject";
        public static string OnRightDelayClickObject = "OnRightDelayClickObject";
        public static string OnLeftDelayClick = "OnLeftDelayClick";
        public static string OnRightDelayClick = "OnRightDelayClick";
    }


    public class EventManager {
        private static Dictionary<string, Action<object>> m_dict_function_event = new Dictionary<string, Action<object>>();
        private static Dictionary<string, Action<object>> dictFunctionEvents {
            get {
                return m_dict_function_event;
            }

            set {
                m_dict_function_event = value;
            }
        }

        public static void StartListening(string strEventName, Action<object> unityActionListener) {
            if (string.IsNullOrEmpty(strEventName) || unityActionListener == null) {
                Debug.LogError("[EventManager] : [StartListening] : check your input params;");
                return;
            }

            if (dictFunctionEvents.ContainsKey(strEventName)) {
                dictFunctionEvents[strEventName] += unityActionListener;
            }
            else {
                dictFunctionEvents.Add(strEventName, unityActionListener);
            }
        }

        public static void StopListening(string strEventName, Action<object> unityActionListener) {
            if (string.IsNullOrEmpty(strEventName) || unityActionListener == null) {
                Debug.LogError("[EventManager] : [StopListening] : check your input params;");
                return;
            }

            if (dictFunctionEvents.ContainsKey(strEventName)) {
                dictFunctionEvents[strEventName] -= unityActionListener;
                if (dictFunctionEvents[strEventName] == null)
                    dictFunctionEvents.Remove(strEventName);
            }
        }

        public static void TriggerEvent(string strEventName, object data = null) {
            if (string.IsNullOrEmpty(strEventName)) {
                Debug.LogError("[EventManager] : [TriggerEvent] : check your input params;");
                return;
            }
            if (dictFunctionEvents.ContainsKey(strEventName)) {
                makeActionTargetSafe(strEventName);
                dictFunctionEvents[strEventName].Invoke(data);
            }
        }

        private static void makeActionTargetSafe(string strEventName) {
            Delegate[] delegetaArray = dictFunctionEvents[strEventName].GetInvocationList();
            for (int i = 0; i < delegetaArray.Length; i++) {
                Delegate de = delegetaArray[i];
                if (de.GetHashCode() < 0 && de.Target.ToString().IndexOf("null") >= 0) {
                    Action<object> act = (Action<object>)de;
                    dictFunctionEvents[strEventName] -= act;
                }
            }
        }
    }
}