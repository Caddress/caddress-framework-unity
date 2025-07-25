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

using UnityEngine;

namespace Unity.Robotics.UrdfImporter
{
    public static class UrdfVisualExtensions
    {
        public static bool SetUrdfVisualMaterial = true;
        public static void Create(Transform parent, GeometryTypes type)
        {
            GameObject visualObject = new GameObject("unnamed");
            visualObject.transform.SetParentAndAlign(parent);
            UrdfVisual urdfVisual = visualObject.AddComponent<UrdfVisual>();

            urdfVisual.geometryType = type;
            UrdfGeometryVisual.Create(visualObject.transform, type);
#if UNITY_EDITOR
            UnityEditor.EditorGUIUtility.PingObject(visualObject);
#endif
        }

        public static void Create(Transform parent, Link.Visual visual)
        {
            GameObject visualObject = new GameObject(visual.name ?? "unnamed");
            visualObject.transform.SetParentAndAlign(parent);
            UrdfVisual urdfVisual = visualObject.AddComponent<UrdfVisual>();

            urdfVisual.geometryType = UrdfGeometry.GetGeometryType(visual.geometry);
            UrdfGeometryVisual.Create(visualObject.transform, urdfVisual.geometryType, visual.geometry);

            if (SetUrdfVisualMaterial)
            {
                UrdfMaterial.SetUrdfMaterial(visualObject, visual.material);
            }
            if (visualObject != null)
            {
                var mrs = visualObject.GetComponentsInChildren<MeshRenderer>();
                if (mrs != null && mrs.Length > 0)
                {
                    foreach (var mr in mrs)
                    {
                        mr.reflectionProbeUsage = UnityEngine.Rendering.ReflectionProbeUsage.Off;
                    }
                }
            }
            UrdfOrigin.ImportOriginData(visualObject.transform, visual.origin);
        }

        public static void AddCorrespondingCollision(this UrdfVisual urdfVisual)
        {
            UrdfCollisions collisions = urdfVisual.GetComponentInParent<UrdfLink>().GetComponentInChildren<UrdfCollisions>();
            UrdfCollisionExtensions.Create(collisions.transform, urdfVisual.geometryType, urdfVisual.transform);
        }

        public static Link.Visual ExportVisualData(this UrdfVisual urdfVisual)
        {
            UrdfGeometry.CheckForUrdfCompatibility(urdfVisual.transform, urdfVisual.geometryType);

            Link.Geometry geometry = UrdfGeometry.ExportGeometryData(urdfVisual.geometryType, urdfVisual.transform);

            Link.Visual.Material material = null;
            if ((geometry.mesh != null )) 
            {
                material = UrdfMaterial.ExportMaterialData(urdfVisual.GetComponentInChildren<MeshRenderer>().sharedMaterial);
            }
            string visualName = urdfVisual.name == "unnamed" ? null : urdfVisual.name;

            return new Link.Visual(geometry, visualName, UrdfOrigin.ExportOriginData(urdfVisual.transform), material);
        }
    }
}