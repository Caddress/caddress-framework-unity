using System;
using System.Runtime.CompilerServices;
using UnityEngine;

namespace Caddress.Template.Facade {
    public static class DebugUtil {
        public static void Log(string message, UnityEngine.Object context = null,
            [CallerMemberName] string methodName = "", [CallerFilePath] string filePath = "") {
            Print(Debug.Log, message, context, methodName, filePath);
        }
        public static void Log(object varValue, UnityEngine.Object context = null,
            [CallerMemberName] string methodName = "", [CallerFilePath] string filePath = "") {
            string className = GetClassName(filePath);
            string msg = $"[{className}] : [{methodName}] : {nameof(varValue)} : {varValue}";
            Debug.Log(msg, context);
        }

        public static void LogWarning(string message, UnityEngine.Object context = null,
            [CallerMemberName] string methodName = "", [CallerFilePath] string filePath = "") {
            Print(Debug.LogWarning, message, context, methodName, filePath);
        }
        public static void LogWarning(object varValue, UnityEngine.Object context = null,
            [CallerMemberName] string methodName = "", [CallerFilePath] string filePath = "") {
            string className = GetClassName(filePath);
            string msg = $"[{className}] : [{methodName}] : {nameof(varValue)} : {varValue}";
            Debug.LogWarning(msg, context);
        }

        public static void LogError(string message, UnityEngine.Object context = null,
            [CallerMemberName] string methodName = "", [CallerFilePath] string filePath = "") {
            Print(Debug.LogError, message, context, methodName, filePath);
        }
        public static void LogError(object varValue, UnityEngine.Object context = null,
            [CallerMemberName] string methodName = "", [CallerFilePath] string filePath = "") {
            string className = GetClassName(filePath);
            string msg = $"[{className}] : [{methodName}] : {nameof(varValue)} : {varValue}";
            Debug.LogError(msg, context);
        }


        private static void Print(Action<string> logMethod, string message, UnityEngine.Object context,
            string methodName, string filePath) {
            string className = GetClassName(filePath);
            string msg = $"[{className}] : [{methodName}] : {message}";
            if (context != null) {
                logMethod.Invoke($"{msg} (Context: {context.name})");
            }
            else {
                logMethod.Invoke(msg);
            }
        }

        private static string GetClassName(string filePath) {
            try {
                return System.IO.Path.GetFileNameWithoutExtension(filePath);
            }
            catch {
                return "UnknownClass";
            }
        }
    }
}
