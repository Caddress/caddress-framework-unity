#if UNITY_EDITOR
using System.IO;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace Unity.Robotics.UrdfImporter.Editor
{
    public class AddDhParameterWindow : EditorWindow
    {
        public float alpha;
        public float a;
        public float d;
        public float theta;
        public Vector2 scrollpos;
        public string dhDisplay = "";
        public int count = 0;


        public void OnGUI()
        {
            
            GUILayout.Space(10);

            GUILayout.Space(10);
            alpha = EditorGUILayout.FloatField("Alpha", alpha);
            a = EditorGUILayout.FloatField("a", a);
            theta = EditorGUILayout.FloatField("theta", theta);
            d = EditorGUILayout.FloatField("d", d);

            scrollpos = EditorGUILayout.BeginScrollView(scrollpos,false,true ,GUILayout.Width(400), GUILayout.Height(300));
            EditorGUILayout.TextArea(dhDisplay);
            EditorGUILayout.EndScrollView();
        }

        private void OnDestroy()
        {
            SetEditorPrefs();
        }

        private void SetEditorPrefs()
        {
        }

        public void GetEditorPrefs()
        {
        }
    }
}
#endif