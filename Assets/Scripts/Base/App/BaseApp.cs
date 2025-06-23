using Caddress.Common;
using Caddress.Tools;
using Caddress.UI;
using System.Collections;
using UnityEngine;

namespace Caddress {
    public class BaseApp : MonoBehaviour {

        public static BaseApp Instance { get; private set; }

        void Awake() {
            Instance = this;
        }

        private void Start() {
            StartCoroutine(Setup());
        }

        IEnumerator Setup() {

            GameObject guiManagerObj = GameObject.Find("GUIManager");
            if (guiManagerObj == null)
                guiManagerObj = new GameObject("GUIManager");
            guiManagerObj.AddComponent<GUIManager>();
            SetupSingleton();

            yield return new WaitForSeconds(1f);

            var testObj = GameObject.Find("TestObject");
            if (testObj != null) {
                Selector.Instance.AddCandidate(testObj.AddComponent<BaseObject>());
            }

            yield break;
        }

        protected virtual void SetupSingleton() {
            GameObject camObj = GameObject.Find("Main Camera");
            if (camObj == null)
                Debug.Log("Main Camera Not found");
            OrbitCamera orbit = camObj.AddComponent<OrbitCamera>();
            GameObject camTargetObj = GameObject.Find("Main Camera Target");
            if (camTargetObj == null)
                camTargetObj = new GameObject("Main Camera Target");
            orbit.target = camTargetObj.transform;

            GameObject root = GameObject.Find("Singleton");
            GameObject singleton = null;

            SelectEffectManager.Instance = new SelectEffectManager();
            SelectEffectManager.Setting.SetDefualtEffectManager(new HighlightSelectEffectManager());

            singleton = new GameObject("PickManager");
            singleton.transform.parent = root.transform;
            singleton.AddComponent<PickManager>();

            singleton = new GameObject("Selector");
            singleton.transform.parent = root.transform;
            singleton.AddComponent<Selector>();
        }
    }
}

