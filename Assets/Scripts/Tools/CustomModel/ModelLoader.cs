using System.Collections;
using System.Collections.Generic;
using TriLib;
using UnityEngine;

namespace Caddress.Tools {
    public class InfoCache {
        public AnimationData[] AnimationData;
        public Dictionary<string, string> NodesPath;
        public GameObject GameObject;
    }

    public abstract class ModelLoader : MonoBehaviour {
        public abstract void ModelLoading();

        public abstract GameObject GetModel();

        public virtual InfoCache Cache() {
            return null;
        }
    }
}
