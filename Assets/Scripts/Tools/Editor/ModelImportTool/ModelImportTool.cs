using System.Collections;
using System.Collections.Generic;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;

namespace Caddress.Tools {
    public class ModelImportTool : Editor {
        [MenuItem("Caddress/Model Import Tool")]
        static void Build() {
            ModelImportToolWindow.OpenBuildWindow();
        }
    }

    public class ModelImportToolWindow : EditorWindow {

        Dictionary<string, string> modelFormats = new Dictionary<string, string>{
            { "FBX ( .fbx )", ".fbx" },
            { "GLTF ( .zip )", ".zip" },
            { "3DS ( .3ds )", ".3ds" },
            { "OBJ ( .obj )", ".obj" },
            { "DAE ( .dae )", ".dae" },
            { "STL ( .stl )", ".stl" },
            { "URDF ( .urdf )", ".urdf" },
            { "PLY ( .ply )", ".ply" },
            { "Other ( .zip )", ".zip" }
        };

        public static void OpenBuildWindow() {
            var window = GetWindow<ModelImportToolWindow>(true, "Model Import Tool", true);
            window.Show();
        }

        private void OnGUI() {
            GUI.color = Color.white;
            EditorGUILayout.LabelField("导入模型：");
            GUI.color = Color.cyan;
            foreach (var kvp in modelFormats) {
                if (GUILayout.Button(kvp.Key, GUILayout.Height(20))) {
                    CustomModelLoader.AddModelFileByFile(kvp.Value);
                }
                GUILayout.Space(5);
            }
        }
    }
}

#endif