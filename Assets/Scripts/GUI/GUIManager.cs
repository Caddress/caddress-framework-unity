using UnityEngine;
using System.Collections.Generic;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Caddress.UI {

    /// <summary>
    /// GUI manager.
    /// </summary>
    public class GUIManager : MonoBehaviour {

        GameObject uiRoot;
        public GameObject UIRoot {
            get { return uiRoot; }
        }

        [SerializeField]
        List<GameObject> uiList = new List<GameObject>();
        public List<GameObject> UIList {
            get { return uiList; }
        }

        [SerializeField]
        GameObject currentOverUI = null;
        public GameObject CurrentOverUI {
            get { return currentOverUI; }
            set { currentOverUI = value; }
        }

        [SerializeField]
        bool currentMouseOver = false;

        public delegate void ScreenChangedDelegate(int x, int y);
        public event ScreenChangedDelegate OnScreenChanged;
        Camera cam;

        public static GUIManager Instance { get; private set; }

        void Awake() {
            Instance = this;

            uiRoot = new GameObject("UIRoot");
            uiRoot.transform.parent = this.transform;
        }

        void Start() {
            cam = Camera.main;
        }

        public GameObject AddUI(string resPath) {
            GameObject prefab = Resources.Load<GameObject>(resPath);
            if (prefab == null)
                Debug.Log("prefab is null: " + resPath);

            GameObject uiObj = Instantiate(prefab);
            uiObj.transform.SetParent(uiRoot.transform);
            uiList.Add(uiObj);

            return uiObj;
        }

        void Update() {
            currentMouseOver = IsPointOverAny(Input.mousePosition);
        }

        public bool CheckOnUI(Vector2 point) {
            bool onUI = false;
            currentOverUI = null;

            List<RaycastResult> eventRaycastResults = new List<RaycastResult>();

            PointerEventData pointerEventData = new PointerEventData(EventSystem.current);
            pointerEventData.position = point;
            if (EventSystem.current != null) {
                EventSystem.current.RaycastAll(pointerEventData, eventRaycastResults);
            }
            if (eventRaycastResults.Count == 0)
                return onUI;

            for (int i = 0; i < eventRaycastResults.Count; ++i) {
                GameObject uiObj = eventRaycastResults[i].gameObject;
                if (uiObj == null)
                    continue;
                if (uiObj.layer == LayerMask.NameToLayer("UI")) {
                    onUI = true;
                    currentOverUI = uiObj;
                    break;
                }
            }

            return onUI;
        }

        public bool IsMouseOverAny() {
            return currentMouseOver;
        }

        bool IsPointOverAny(Vector2 point) {
            bool rt = false;
            if (!rt)
                rt = CheckOnUI(point);

            return rt;
        }

        public bool IsInputingText() {
            if (EventSystem.current == null)
                return false;
            GameObject obj = EventSystem.current.currentSelectedGameObject;
            if (obj == null)
                return false;

            InputField text = obj.GetComponent<InputField>();
            if (text == null)
                return false;

            return true;
        }
    }

}