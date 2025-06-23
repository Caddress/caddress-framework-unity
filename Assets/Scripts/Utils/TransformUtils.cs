using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Text.RegularExpressions;

namespace Caddress {
    public static class TransformUtils {
        public static Transform TryFind(this Transform target, string nodeName, bool recursive, bool depthFirstTraveral = true) {
            if (!recursive)
                return target.TryFind(nodeName);
            Transform result = null;
            ExistNode(target, nodeName, ref result, depthFirstTraveral);
            return result;
        }

        private static bool ExistNode(Transform parent, string nodeName, ref Transform result, bool depthFirstTraveral) {
            for (int i = 0; i < parent.childCount; i++) {
                var child = parent.GetChild(i);
                if (child.name == nodeName) {
                    result = child;
                    return true;
                }
                if (depthFirstTraveral) {
                    bool exist = ExistNode(child, nodeName, ref result, depthFirstTraveral);
                    if (exist)
                        return true;
                }
            }
            if (!depthFirstTraveral) {
                for (int i = 0; i < parent.childCount; i++) {
                    var child = parent.GetChild(i);
                    bool exist = ExistNode(child, nodeName, ref result, depthFirstTraveral);
                    if (exist)
                        return true;
                }
            }
            return false;
        }

        public static Transform FindByPath(this Transform target, string name) {
            string[] names = name.Split('/');
            Transform cur = target;
            for (int i = 0; i < names.Length; i++) {
                string curname = names[i];
                Transform matched_child = null;
                for (int j = 0; j < cur.childCount; j++) {
                    Transform child = cur.GetChild(j);
                    if (child.name.CompareTo(curname) == 0) {
                        matched_child = child;
                        break;
                    }
                }
                if (!matched_child)
                    return null;
                cur = matched_child;
            }

            return null;
        }

        public static Transform RegFindFirst(this Transform target, string regString) {
            string str = regString;
            Transform parent = target;

            int pos = str.IndexOf("#");
            if (-1 == pos) {
                Transform tr = parent.Find(str);
                return tr;
            }

            if (0 == pos) {
                Regex regex = new Regex(str.Substring(1));
                for (int i = 0; i < parent.childCount; i++) {
                    Transform tr = parent.GetChild(i);
                    if (regex.IsMatch(tr.name)) {
                        return tr;
                    }
                }
                return null;
            }

            string[] strs = str.Split('#');
            if (strs.Length != 2) {
                Debug.LogError("");
                return null;
            }

            string prename = strs[0].TrimEnd('/');
            string regname = strs[1];


            return RegFindFirst(parent.Find(prename), "#" + regname);
        }

        public static void Reset(this Transform target) {
            target.localPosition = Vector3.zero;
            target.localRotation = Quaternion.identity;
            target.localScale = Vector3.one;
        }

        public static string Path(this Transform target) {
            string path = "";
            Transform tr = target;
            while (tr != null) {
                path = tr.name + "/" + path;
                tr = tr.parent;
            }
            path = path.TrimEnd('/');
            return path;
        }

        public static Transform TryFind(this Transform target, string nodeName) {
            Transform child = target.Find(nodeName);
            if (null == child) {
                child = new GameObject(nodeName).transform;
                child.SetParent(target);
                child.localPosition = Vector3.zero;
                child.localScale = Vector3.one;
                child.localRotation = Quaternion.Euler(0, 0, 0);
            }
            return child;
        }

        public static T GetComponentOrAdd<T>(this Transform target) where T : UnityEngine.Component {
            T t = target.GetComponent<T>();
            if (t == null) {
                t = target.gameObject.AddComponent<T>();
            }
            return t;
        }

        public static T GetComponentOrAdd<T>(this GameObject target) where T : UnityEngine.Component {
            return target.transform.GetComponentOrAdd<T>();
        }

        public static T GetComponentOrAdd<T>(this MonoBehaviour target) where T : UnityEngine.Component {
            return target.gameObject.GetComponentOrAdd<T>();
        }

        public static Bounds GetBoundsWithChildren(this GameObject target, bool includeInActive = false) {
            return target.transform.GetBoundsWithChildren(includeInActive);
        }
        public static Bounds GetBoundsWithChildren(this Transform target, bool includeInActive = false) {
            Bounds bounds = new Bounds(target.transform.position, Vector3.zero);
            if (target == null)
                return bounds;
            //todo: 先逐个处理，后优化代码统一处理
            MeshFilter[] meshFilters = target.GetComponentsInChildren<MeshFilter>(includeInActive);
            LineRenderer[] lines = target.GetComponentsInChildren<LineRenderer>(includeInActive);
            ParticleSystemRenderer[] particles = target.GetComponentsInChildren<ParticleSystemRenderer>(includeInActive);
            Dictionary<Transform, Mesh> meshes = new Dictionary<Transform, Mesh>();
            for (int i = 0; i < meshFilters.Length; i++) {
                var mesh = meshFilters[i].mesh;
                if (mesh == null || (mesh.bounds.extents.Equals(Vector3.zero) && mesh.bounds.size.Equals(Vector3.zero)))
                    continue;
                meshes.Add(meshFilters[i].transform, mesh);
            }
            for (int i = 0; i < lines.Length; i++) {
                if (!IsValidLine(lines[i])) continue;
                Mesh mesh = new Mesh();
                lines[i].BakeMesh(mesh, lines[i].alignment == LineAlignment.TransformZ);
                meshes.Add(lines[i].transform, mesh);
            }
            for (int i = 0; i < particles.Length; i++) {
                if (Camera.main == null) {
                    Debug.LogError("在获取粒子系统的碰撞盒的时候，MainCamera不能为空！");
                    continue;
                }
                Mesh mesh = new Mesh();
                particles[i].BakeMesh(mesh);
                meshes.Add(particles[i].transform, mesh);
            }
            var index = 0;
            foreach (var item in meshes) {
                Mesh mesh = item.Value;
                if (index == 0) {
                    bounds = mesh.bounds;
                    bounds.center = item.Key.TransformPoint(bounds.center);
                    bounds.size = item.Key.transform.TransformVector(bounds.size);
                }
                var newBounds = mesh.bounds;
                newBounds.center = item.Key.transform.TransformPoint(newBounds.center);
                newBounds.size = item.Key.transform.TransformVector(newBounds.size);
                bounds.Encapsulate(newBounds);
                index++;
            }
            bounds.center = target.InverseTransformPoint(bounds.center);
            bounds.size = target.InverseTransformVector(bounds.size);
            //TODO
            //目前粒子系统还没有网格，临时给添加；后续取到粒子系统的网格
            if (bounds.size.Equals(Vector3.zero)) {
                bounds.size = Vector3.one * 2;
                bounds.center = Vector3.zero;
            }
            bounds.size = new Vector3(Mathf.Abs(bounds.size.x), Mathf.Abs(bounds.size.y), Mathf.Abs(bounds.size.z));
            return bounds;
        }

        private static bool IsValidLine(LineRenderer line) {
            if (line.positionCount < 2) return false;
            for (int i = 0; i < line.positionCount - 1; i++) {
                if (!line.GetPosition(i).Equals(line.GetPosition(i + 1)))
                    return true;
            }
            return false;
        }

        static public void Fade(this GameObject target, float fadeValue) {
            ObjectUtil.FadeObject(target, fadeValue);
        }
    }
}