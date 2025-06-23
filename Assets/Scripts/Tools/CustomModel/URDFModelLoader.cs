using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Robotics.UrdfImporter;
using UnityEngine;


namespace Caddress.Tools
{
    public class URDFModelLoader : ModelLoader
    {
        private readonly string _file = null;
        private GameObject _model = null;
        public InfoCache cache = null;

        public URDFModelLoader(string file)
        {
            this._file = file;
        }

        public override void ModelLoading()
        {
            GameObject realObject = new GameObject("RealObject");
            GameObject body = new GameObject("Body");
            GameObject rootNode = new GameObject("RootNode");
            body.transform.SetParent(realObject.transform);
            var model = UrdfRobotExtensions.CreateRuntime(_file, new ImportSettings { chosenAxis = ImportSettings.axisType.yAxis, convexMethod = ImportSettings.convexDecomposer.vHACD });
            model.transform.SetParent(rootNode.transform);
            rootNode.transform.SetParent(body.transform);

            model.transform.SetParent(rootNode.transform);
            rootNode.transform.SetParent(body.transform);
            AutoAddBoxCollider(realObject.transform);
            this._model = realObject;
        }
        public override InfoCache Cache()
        {
            return cache;
        }

        /// <summary>
        /// 获取模型
        /// </summary>
        /// <returns></returns>
        public override GameObject GetModel()
        {
            return _model;
        }

        void AutoAddBoxCollider(Transform parent)
        {
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
            foreach (Renderer child in renders)
            {
                center += child.bounds.center;
            }
            center /= renders.Length;
            Bounds bounds = new Bounds(center, Vector3.zero);
            foreach (Renderer child in renders)
            {
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
