#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using System.IO;
using System.Collections.Generic;

namespace Caddress.Tools {
    public static class PrefabExporter {
        private static string _baseDir;
        private static Dictionary<Mesh, string> _meshMap;
        private static Dictionary<Material, string> _matMap;
        private static Dictionary<Texture2D, string> _texMap;

        public static void SaveModelAsPrefab(GameObject root, string fileTitle) {
            if (root == null) return;

            _baseDir = "Assets/Resources/Prefabs/" + fileTitle;
            Directory.CreateDirectory(_baseDir);

            _meshMap = new();
            _matMap = new();
            _texMap = new();

            foreach (Transform child in root.GetComponentsInChildren<Transform>(true)) {
                ProcessMesh(child);
                ProcessMaterials(child);
            }

            AssetDatabase.SaveAssets();
            string prefabPath = $"{_baseDir}/model.prefab";
            PrefabUtility.SaveAsPrefabAssetAndConnect(root, prefabPath, InteractionMode.UserAction);
        }

        private static void ProcessMesh(Transform child) {
            Mesh mesh = null;
            var mf = child.GetComponent<MeshFilter>();
            var smr = child.GetComponent<SkinnedMeshRenderer>();

            if (mf != null && mf.sharedMesh != null) mesh = mf.sharedMesh;
            if (smr != null && smr.sharedMesh != null) mesh = smr.sharedMesh;

            if (mesh == null) return;

            if (!_meshMap.ContainsKey(mesh)) {
                Mesh meshCopy = Object.Instantiate(mesh);
                string meshPath = $"{_baseDir}/{child.name}_Mesh.asset";
                AssetDatabase.CreateAsset(meshCopy, meshPath);
                _meshMap[mesh] = meshPath;
            }

            var savedMesh = AssetDatabase.LoadAssetAtPath<Mesh>(_meshMap[mesh]);
            if (mf != null) mf.sharedMesh = savedMesh;
            if (smr != null) smr.sharedMesh = savedMesh;
        }

        private static void ProcessMaterials(Transform child) {
            var renderer = child.GetComponent<Renderer>();
            if (renderer == null || renderer.sharedMaterials == null) return;

            var newMats = new Material[renderer.sharedMaterials.Length];

            for (int i = 0; i < renderer.sharedMaterials.Length; i++) {
                var mat = renderer.sharedMaterials[i];
                if (mat == null) continue;

                if (!_matMap.ContainsKey(mat)) {
                    Material matCopy = new Material(mat);
                    string matPath = $"{_baseDir}/{child.name}_Mat_{i}.mat";

                    if (mat.HasProperty("_MainTex")) {
                        if (mat.mainTexture is Texture2D tex) {
                            if (!_texMap.ContainsKey(tex)) {
                                string texPath = $"{_baseDir}/{child.name}_Tex_{i}.asset";
                                Texture2D texCopy = Object.Instantiate(tex);
                                AssetDatabase.CreateAsset(texCopy, texPath);
                                _texMap[tex] = texPath;
                                matCopy.mainTexture = texCopy;
                            }
                            else {
                                matCopy.mainTexture = AssetDatabase.LoadAssetAtPath<Texture2D>(_texMap[tex]);
                            }
                        }
                    }

                    AssetDatabase.CreateAsset(matCopy, matPath);
                    _matMap[mat] = matPath;
                    newMats[i] = matCopy;
                }
                else {
                    newMats[i] = AssetDatabase.LoadAssetAtPath<Material>(_matMap[mat]);
                }
            }

            renderer.sharedMaterials = newMats;
        }
    }
}
#endif
