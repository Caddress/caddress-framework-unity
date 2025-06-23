using Pcx;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace Caddress.Tools {
    public class PlyModelLoader : ModelLoader {

        private readonly string _file = null;
        private GameObject _model = null;
        public InfoCache cache = null;

        public PlyModelLoader(string file) {
            this._file = file;
        }

        public override void ModelLoading() {
            GameObject realObject = new GameObject("RealObject");
            GameObject body = new GameObject("Body");
            GameObject rootNode = new GameObject("RootNode");
            body.transform.SetParent(realObject.transform);
            var model = LoadPlyAsPointCloud(_file);
            model.transform.SetParent(rootNode.transform);
            rootNode.transform.SetParent(body.transform);

            model.transform.SetParent(rootNode.transform);
            rootNode.transform.SetParent(body.transform);
            this._model = realObject;
        }

        public override InfoCache Cache() {
            return cache;
        }

        public override GameObject GetModel() {
            return _model;
        }

        private GameObject LoadPlyAsPointCloud(string path) {
            if (!File.Exists(path)) {
                Debug.LogError("PLY file not found at: " + path);
                return null;
            }

            Mesh mesh = PlyLoader.Load(path);
            GameObject go = new GameObject(Path.GetFileNameWithoutExtension(path));
            var mf = go.AddComponent<MeshFilter>();
            var mr = go.AddComponent<MeshRenderer>();
            mf.mesh = mesh;
            mr.material = AssetDatabase.LoadAssetAtPath<Material>("Assets/Plugins/Pcx/Editor/Default Point.mat");

            return go;
        }
    }
}

