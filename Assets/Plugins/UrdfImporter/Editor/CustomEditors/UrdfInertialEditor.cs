﻿/*
© Siemens AG, 2017
Author: Dr. Martin Bischoff (martin.bischoff@siemens.com)
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
    [CustomEditor(typeof(UrdfInertial))]
    public class UrdfInertialEditor : UnityEditor.Editor
    {
        private Vector3 testVector;

        public override void OnInspectorGUI()
        {
            UrdfInertial urdfInertial = (UrdfInertial) target;

            GUILayout.Space(5);
            urdfInertial.displayInertiaGizmo =
                EditorGUILayout.ToggleLeft("Display Inertia Gizmo", urdfInertial.displayInertiaGizmo);
            GUILayout.Space(5);

            bool newValue = EditorGUILayout.BeginToggleGroup("Use URDF Data", urdfInertial.useUrdfData);
            EditorGUILayout.Vector3Field("URDF Center of Mass", urdfInertial.centerOfMass);
            EditorGUILayout.Vector3Field("URDF Inertia Tensor", urdfInertial.inertiaTensor);
            EditorGUILayout.Vector3Field("URDF Inertia Tensor Rotation",
                urdfInertial.inertiaTensorRotation.eulerAngles);
            EditorGUILayout.EndToggleGroup();

            if (newValue != urdfInertial.useUrdfData)
            {
                urdfInertial.useUrdfData = newValue;
                urdfInertial.UpdateLinkData();
            }
        }
    }
}
#endif