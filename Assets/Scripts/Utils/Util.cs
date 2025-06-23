using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace Caddress {
    public class Util{
        public static bool CompareGameObjectList(List<GameObject> list1, List<GameObject> list2) {
            if (list1.Count != list2.Count) return false;
            for (int k = 0; k < list1.Count; k++) {
                GameObject go = list1[k];
                if (!list2.Contains(go)) return false;
            }
            return true;
        }

        public static string GetNewModelID() {
            return System.Guid.NewGuid().ToString("N");
        }

        public static float GetModelFileSize(string path) {
            FileInfo file = new FileInfo(path);
            float fileSize = (float)file.Length / 1000 / 1024;

            return fileSize;
        }

        public static Transform ResetModelCenter(Transform t) {
            if (t == null)
                return null;

            MeshRenderer[] renders = t.GetComponentsInChildren<MeshRenderer>();
            if (renders == null || renders.Length == 0) {
                return null;
            }

            Vector3 postion = t.position;
            Quaternion rotation = t.rotation;
            Vector3 scale = t.localScale;
            t.position = Vector3.zero;
            t.rotation = Quaternion.Euler(Vector3.zero);
            t.localScale = Vector3.one;

            Bounds bounds = renders[0].bounds;
            foreach (MeshRenderer child in renders) {
                bounds.Encapsulate(child.bounds);
            }

            t.position = postion;
            t.rotation = rotation;
            t.localScale = scale;

            foreach (Transform item in t) {
                item.position = item.position - bounds.center + Vector3.up * bounds.size.y / 2;
            }

            BoxCollider collider = t.gameObject.GetComponent<BoxCollider>();
            if (collider == null)
                collider = t.gameObject.AddComponent<BoxCollider>();
            collider.center = Vector3.up * bounds.size.y / 2;
            collider.size = bounds.size;
            return t.transform;
        }

        public static void CopyFolder(string sourceFolder, string destinationFolder) {
            if (!Directory.Exists(destinationFolder)) {
                Directory.CreateDirectory(destinationFolder);
            }
            foreach (string file in Directory.GetFiles(sourceFolder)) {
                string destinationFile = Path.Combine(destinationFolder, Path.GetFileName(file));
                using (FileStream srcStream = File.OpenRead(file)) {
                    using (FileStream destStream = File.Create(destinationFile)) {
                        srcStream.CopyTo(destStream);
                    }
                }
            }
            foreach (string folder in Directory.GetDirectories(sourceFolder)) {
                string destinationSubFolder = Path.Combine(destinationFolder, Path.GetFileName(folder));
                CopyFolder(folder, destinationSubFolder);
            }
        }
    }
}

