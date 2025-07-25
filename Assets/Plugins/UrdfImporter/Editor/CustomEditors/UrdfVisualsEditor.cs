﻿/*
© Siemens AG, 2018
Author: Suzannah Smith (suzannah.smith@siemens.com)
Licensed under the Apache License, Version 2.0 (the "License");
you may not use this file except in compliance with the License.
You may obtain a copy of the License at
<http://www.apache.org/licenses/LICENSE-2.0>.
Unless required by applicable law or agreed to in writing, software
distributed under the License is distributed on an "AS IS" BASIS,
WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
See the License for the specific language governing permissions and
limitations under the License.
*/
#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

namespace Unity.Robotics.UrdfImporter.Editor
{
    [CustomEditor(typeof(UrdfVisuals))]
    class UrdfVisualsEditor : UnityEditor.Editor
    {
        private UrdfVisuals urdfVisuals;
        private GeometryTypes geometryType = GeometryTypes.Box;

        public override void OnInspectorGUI()
        {
            urdfVisuals = (UrdfVisuals)target;

            GUILayout.Space(10);
            geometryType = (GeometryTypes)EditorGUILayout.EnumPopup("Type of visual", geometryType);
            
            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button("Add visual"))
                UrdfVisualExtensions.Create(urdfVisuals.transform, geometryType);
            EditorGUILayout.EndHorizontal();
        }
    }
}
#endif