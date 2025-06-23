using UnityEngine;
using System.Collections.Generic;
using UnityEditor.Experimental.GraphView;


namespace Caddress {

    static public class ObjectUtil {

        static public Vector3 CalculateObjectsCenter(List<GameObject> objs) {
            if (objs == null || objs.Count == 0)
                return Vector3.zero;

            Vector3 pos = Vector3.zero;
            bool first = true;
            foreach (GameObject obj in objs) {
                if (first)
                    pos = obj.transform.position;
                else
                    pos += obj.transform.position;
                first = false;
            }
            Vector3 center = pos / objs.Count;
            return center;
        }
        static public Vector3 CalculateObjectsCenter(List<BaseObject> objs) {
            Vector3 pos = Vector3.zero;
            bool first = true;
            foreach (BaseObject obj in objs) {
                GameObject tmp = obj.gameObject;
                if (first)
                    pos = tmp.transform.position;
                else
                    pos += tmp.transform.position;
                first = false;
            }
            Vector3 center = pos / objs.Count;
            return center;
        }

        static public void MoveObjects(List<GameObject> objs, Vector3 offset) {
            for (int i = 0; i < objs.Count; i++) {
                GameObject obj = objs[i];
                obj.transform.position += offset;
            }
        }

        static public void MoveObjects(List<GameObject> objs, Vector3 offset, Vector3 normal) {
            for (int i = 0; i < objs.Count; i++) {
                GameObject obj = objs[i];
                obj.transform.position += offset;
                obj.transform.LookAt(obj.transform.position + normal);
            }
        }

        static public void MoveObjectsClampY(List<GameObject> objs, Vector3 offset, float low, float high) {
            for (int i = 0; i < objs.Count; i++) {
                GameObject obj = objs[i];
                Vector3 pos = obj.transform.position + offset;
                pos.y = Mathf.Clamp(pos.y, low, high);
            }
        }

        static public void SetObjectsPosition(List<GameObject> objs, Vector3 pos) {
            for (int i = 0; i < objs.Count; i++) {
                GameObject obj = objs[i];
                obj.transform.position = pos;
            }
        }


        static public Bounds CalculateBounds(List<GameObject> objs) {
            Bounds bounds = new Bounds();
            if (objs.Count == 0)
                return bounds;

            bounds = CalculateBounds(objs[0]);
            for (int i = 1; i < objs.Count; ++i)
                bounds.Encapsulate(CalculateBounds(objs[i]));

            return bounds;
        }

        static public Bounds CalculateBounds(GameObject rootObj) {
            return CalculateBounds(rootObj.transform);
        }

        static public Bounds CalculateBounds(Transform root) {
            Bounds bounds = new Bounds();
            if (root == null)
                return bounds;

            bool firstBound = true;
            Renderer[] renderers = root.GetComponentsInChildren<Renderer>(true);
            for (int i = 0; i < renderers.Length; i++) {
                Renderer rend = renderers[i];
                if (!rend.gameObject.activeInHierarchy) continue;
                if (rend.bounds.extents.Equals(Vector3.zero) && rend.bounds.size.Equals(Vector3.zero)) continue;
                if (firstBound) {
                    if (!float.IsNaN(rend.bounds.center.x)) {
                        bounds = rend.bounds;
                        bounds.center = rend.bounds.center;
                        firstBound = false;
                    }
                }
                else {
                    bounds.Encapsulate(rend.bounds);
                }
            }

            Renderer rootRend = root.GetComponent<Renderer>();
            if (rootRend != null)
                bounds.Encapsulate(rootRend.bounds);

            return bounds;
        }

        public static Bounds CalculateMeshBounds(GameObject rootObj) {
            return CalculateMeshBounds(rootObj.transform);
        }

        public static Bounds CalculateMeshBounds(Transform root) {
            Bounds bounds = new Bounds();
            if (root == null)
                return bounds;

            bool firstBound = true;
            MeshFilter[] meshFilters = root.GetComponentsInChildren<MeshFilter>();
            for (int i = 0; i < meshFilters.Length; i++) {
                Mesh mesh = meshFilters[i].mesh;

                if (firstBound) {
                    bounds = mesh.bounds;
                    firstBound = false;
                }
                else {
                    bounds.Encapsulate(mesh.bounds);
                }
            }

            return bounds;
        }

        static public Bounds CalculateColliderBounds(List<GameObject> objs) {
            Bounds bounds = new Bounds();
            if (objs.Count == 0)
                return bounds;

            bounds = CalculateColliderBounds(objs[0]);
            for (int i = 1; i < objs.Count; ++i)
                bounds.Encapsulate(CalculateColliderBounds(objs[i]));

            return bounds;
        }

        public static Bounds CalculateColliderBounds(GameObject rootObj) {
            return CalculateColliderBounds(rootObj.transform);
        }

        public static Bounds CalculateColliderBounds(Transform root) {
            Bounds bounds = new Bounds();
            if (root == null)
                return bounds;

            bool firstBound = true;
            Collider[] colliders = root.GetComponentsInChildren<Collider>();
            for (int i = 0; i < colliders.Length; i++) {
                if (firstBound) {
                    bounds = colliders[i].bounds;
                    firstBound = false;
                }
                else {
                    bounds.Encapsulate(colliders[i].bounds);
                }
            }

            return bounds;
        }

        static public GameObject CreateWirePlane() {
            Vector3[] vs = new Vector3[] {
                new Vector3(-0.5f, 0f, -0.5f),
                new Vector3(0.5f, 0f, -0.5f),
                new Vector3(0.5f, 0f, 0.5f),
                new Vector3(-0.5f, 0f, 0.5f),
            };

            int[] index = new int[] {
                0, 1, 2, 3, 0
            };

            Vector2[] uvs = new Vector2[] {
                new Vector2(0f, 0f),
                new Vector2(0f, 0f),
                new Vector2(0f, 0f),
                new Vector2(0f, 0f),
            };

            Vector3[] normals = new Vector3[] {
                new Vector3(0f, 1f, 0f),
                new Vector3(0f, 1f, 0f),
                new Vector3(0f, 1f, 0f),
                new Vector3(0f, 1f, 0f),
            };

            GameObject wirePlane = new GameObject("WirePlane");
            MeshFilter mf = wirePlane.AddComponent<MeshFilter>();
            mf.mesh = new Mesh();
            mf.mesh.vertices = vs;
            mf.mesh.uv = uvs;
            mf.mesh.normals = normals;
            mf.mesh.SetIndices(index, MeshTopology.LineStrip, 0);
            MeshRenderer mr = wirePlane.AddComponent<MeshRenderer>();
            mr.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
            return wirePlane;
        }

        static public void FadeObject(GameObject target, float fadeValue) {
            List<Material> materials = new List<Material>();
            var mrs = target.GetComponentsInChildren<MeshRenderer>();
            foreach (var mr in mrs) {
                materials.AddRange(mr.materials);
            }
            FadeObject(materials, fadeValue);
        }

        static public void FadeObject(List<Material> materials, float fadeValue) {
            var opaque = Mathf.Approximately(fadeValue, 1f);
            foreach (var material in materials) {
                var clr = material.color;
                clr.a = fadeValue;
                if (material.shader.name == "Custom/NoLighting" && !opaque) {
                    material.shader = Shader.Find("Custom/NoLightingFade");
                }
                else if (material.shader.name == "Custom/NoLightingFade" && opaque) {
                    material.shader = Shader.Find("Custom/NoLighting");
                }
                else if (material.shader.name == "Legacy Shaders/Diffuse" && !opaque) {
                    material.shader = Shader.Find("Legacy Shaders/Transparent/Diffuse");
                }
                else if (material.shader.name == "Legacy Shaders/Transparent/Diffuse" && opaque) {
                    material.shader = Shader.Find("Legacy Shaders/Diffuse");
                }
                material.color = clr;
            }
        }

        public static Transform FindChildRecursive(this Transform parent, string childName, bool caseSensitive) {
            if (parent == null || string.IsNullOrEmpty(childName)) return null;

            foreach (Transform child in parent) {
                bool isMatch = caseSensitive ?
                    (child.name == childName) :
                    (child.name.Equals(childName, System.StringComparison.OrdinalIgnoreCase));

                if (isMatch)
                    return child;

                Transform found = child.FindChildRecursive(childName, caseSensitive);
                if (found != null)
                    return found;
            }
            return null;
        }
    }

}