using TriLib;
using UnityEngine;


namespace Caddress.Tools {
    public class ModelLoadByTriLib : ModelLoader {
        private readonly string _file = null;
        private GameObject _model = null;
        public InfoCache cache = null;

        public ModelLoadByTriLib(string file) {
            this._file = file;
        }

        public override void ModelLoading() {
            using (var assetLoader = new AssetLoader()) {
                GameObject realObject = new GameObject("RealObject");
                GameObject body = new GameObject("Body");
                GameObject rootNode = new GameObject("RootNode");
                body.transform.SetParent(realObject.transform);
                string file = _file.Replace("\\", "/");

                var assetLoaderOptions = AssetLoaderOptions.CreateInstance();
                assetLoaderOptions.AutoPlayAnimations = false;
                assetLoaderOptions.UseOriginalPositionRotationAndScale = true;
                assetLoaderOptions.LoadRawMaterialProperties = true;
                var model = assetLoader.LoadFromFileWithTextures(file, assetLoaderOptions);

                cache = new InfoCache();
                cache.AnimationData = assetLoader.AnimationData;
                cache.NodesPath = assetLoader.NodesPath;
                cache.GameObject = assetLoader.RootNodeData.GameObject;

                model.transform.SetParent(rootNode.transform);
                rootNode.transform.SetParent(body.transform);
                RemoveCamera(body);

                if (CheckBIPOrBone(body)) {
                    RemoveAnim(body);
                    Debug.Log("该模型可能含有骨骼动画，此上传暂不支持，请使用max上传。");
                }
                ReflectionsControl(realObject);
                AutoAddBoxCollider(realObject.transform);

                CreateTexutureToLoacal(realObject);
                this._model = realObject;
                Debug.Log("模型加载成功");
            }
        }
        public override InfoCache Cache() {
            return cache;
        }

        public override GameObject GetModel() {
            return _model;
        }

        bool CheckBIPOrBone(GameObject gb) {
            int bipCount = 0;
            int boneCount = 0;
            foreach (var item in gb.GetComponentsInChildren<Transform>()) {
                if (item.name.ToLower().Contains("bip"))
                    bipCount++;
                if (item.name.ToLower().Contains("bone"))
                    boneCount++;
            }
            if (bipCount > 10 || boneCount > 3)
                return true;
            else
                return false;
        }
        void RemoveAnim(GameObject go) {
            foreach (Transform tf in go.GetComponentsInChildren<Transform>()) {
                if (tf.GetComponent<Animation>() != null) {
                    tf.GetComponent<Animation>().enabled = false;
                    Destroy(tf.GetComponent<Animation>());
                }
            }
        }
        void RemoveCamera(GameObject go) {
            foreach (Transform tf in go.GetComponentsInChildren<Transform>()) {
                if (tf.GetComponent<Camera>() != null) {
                    Debug.Log(tf);
                    Destroy(tf.GetComponent<Camera>());
                }
            }
        }
        void ReflectionsControl(GameObject gb) {
            foreach (Transform tm in gb.GetComponentsInChildren<Transform>()) {
                Renderer rd = tm.GetComponent<Renderer>();
                if (null != rd) {
                    var reflection = UnityEngine.Rendering.ReflectionProbeUsage.Off;
                    Material mat = rd.sharedMaterial;
                    if (mat.GetTexture("_MetallicGlossMap") != null)
                        reflection = UnityEngine.Rendering.ReflectionProbeUsage.BlendProbes;
                    if (mat.HasProperty("_SpecColor")) {
                        Color specC = mat.GetColor("_SpecColor");
                        if (specC != Color.black)
                            reflection = UnityEngine.Rendering.ReflectionProbeUsage.BlendProbes;
                    }
                    rd.reflectionProbeUsage = reflection;
                    //rd.reflectionProbeUsage = UnityEngine.Rendering.ReflectionProbeUsage.Off;
                }
            }
        }

        void CreateTexutureToLoacal(GameObject go) {
            foreach (Transform tm in go.GetComponentsInChildren<Transform>()) {
                Renderer rd = tm.GetComponent<Renderer>();
                if (null != rd) {
                    int index = 0;
                    foreach (var mat in rd.sharedMaterials) {
                        index++;
                        if (mat.GetTexture("_MetallicGlossMap") != null) {
                            mat.GetTexture("_MetallicGlossMap").name = $"{mat.name}_Metallic_{index}";
                            mat.SetFloat("_Glossiness", 1);
                        }
                        if (mat.GetTexture("_BumpMap") != null) {
                            mat.GetTexture("_BumpMap").name = $"{mat.name}_BumpMap_{index}";
                            mat.SetFloat("_Glossiness", 1);
                        }
                    }
                }
            }
        }

        void AutoAddBoxCollider(Transform parent) {
            BoxCollider oldCollider = parent.gameObject.GetComponent<BoxCollider>();
            if (oldCollider != null)
                Destroy(oldCollider);
            Vector3 postion = parent.position;
            Quaternion rotation = parent.rotation;
            Vector3 scale = parent.localScale;
            parent.position = Vector3.zero;
            parent.rotation = Quaternion.Euler(Vector3.zero);
            parent.localScale = Vector3.one;

            Vector3 center = Vector3.zero;
            Renderer[] renders = parent.GetComponentsInChildren<Renderer>();
            foreach (Renderer child in renders) {
                center += child.bounds.center;
            }
            center /= renders.Length;
            Bounds bounds = new Bounds(center, Vector3.zero);
            foreach (Renderer child in renders) {
                bounds.Encapsulate(child.bounds);
            }
            BoxCollider boxCollider = parent.gameObject.AddComponent<BoxCollider>();
            boxCollider.center = bounds.center - parent.position;
            boxCollider.size = bounds.size;

            parent.position = postion;
            parent.rotation = rotation;
            parent.localScale = scale;
        }
    }
}
