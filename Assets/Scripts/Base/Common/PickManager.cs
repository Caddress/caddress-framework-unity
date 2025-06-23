using UnityEngine;
using System.Collections.Generic;
using Caddress.UI;

namespace Caddress.Common {
    public class RaycastObjectHit {
        public bool picked = false;
        public BaseObject obj = null;
        public float distance = 0;
        public Vector3 point = Vector3.zero;
        public Transform hitTrans = null;

        public void Reset() {
            picked = false;
            obj = null;
            distance = 0;
            point = Vector3.zero;
            hitTrans = null;

        }
    }

    public partial class PickManager : MonoBehaviour {

        Camera cam;

        public static PickManager Instance { get; private set; }

        public delegate void SingleClickDelegate(int btn, BaseObject obj);
        public delegate void DoubleClickDelegate(int btn, BaseObject obj);
        public delegate void MouseMoveDelegate();
        public event SingleClickDelegate OnSingleClick;
        public event DoubleClickDelegate OnDoubleClick;
        public event MouseMoveDelegate OnMouseMoveEvent;

        [SerializeField]
        protected GameObject pickedObj = null;
        public GameObject PickedObj {
            get { return pickedObj; }
        }
        protected BaseObject pickedBaseObj = null;
        [SerializeField]
        protected GameObject pickedCollider = null;
        public GameObject PickedCollider {
            get { return pickedCollider; }
        }

        [SerializeField]
        GameObject lastPickedObj = null;

        [SerializeField]
        GameObject lastPickedCollider = null;

        [SerializeField]
        protected Vector3 pickedPos = Vector3.zero;
        public Vector3 PickedPos {
            get { return pickedPos; }
        }

        public bool picked = false;
        public bool Picked {
            get { return picked; }
        }

        float delayClick = -1.0f;
        float delayClickMax = 0.3f;

        int raycastMask = Physics.DefaultRaycastLayers;

        RaycastHit[] raycastResult;
        public RaycastHit[] RaycastResult {
            get { return raycastResult; }
        }
        RaycastObjectHit raycastObjResult = new RaycastObjectHit();
        public RaycastObjectHit RaycastObjResult {
            get { return raycastObjResult; }
        }


        List<BaseObject> tmpRayCastPlacementList = new List<BaseObject>();

        public BaseObject GetPickedObject() {
            return pickedBaseObj;
        }

        public Vector3 GetPickedPos() {
            return pickedPos;
        }

        void Awake() {
            Instance = this;
            cam = Camera.main;
        }

        void Update() {
            Pick();
            UpdateDelayClickEvent();
        }

        void Pick() {
            raycastResult = null;
            raycastObjResult.Reset();

            pickedObj = null;
            pickedBaseObj = null;
            pickedCollider = null;
            picked = false;

            if (GUIManager.Instance.IsMouseOverAny()) {
                pickedObj = null;
                pickedBaseObj = null;
                pickedCollider = null;
                picked = true;
                goto END_PICK;
            }
            else {
                if (GUIUtility.hotControl != 0)
                    goto END_PICK;
            }

            Vector3 pos = Input.mousePosition; pos.z = 0;
            Ray ray = cam.ScreenPointToRay(pos);
            raycastResult = Physics.RaycastAll(ray, Mathf.Infinity, raycastMask);

            System.Array.Sort(raycastResult, (left, right) => {
                return left.distance.CompareTo(right.distance);
            });

            for (int i = 0; i < raycastResult.Length; i++) {
                RaycastHit hit = raycastResult[i];
                BaseObject foundObj = hit.transform.GetComponent<BaseObject>();

                if (foundObj == null || !foundObj.PickEnabled)
                    continue;

                pickedObj = foundObj.gameObject;
                pickedBaseObj = foundObj;
                pickedCollider = hit.transform.gameObject;
                pickedPos = hit.point;
                picked = true;

                raycastObjResult.picked = true;
                raycastObjResult.obj = foundObj;
                raycastObjResult.point = pickedPos;
                raycastObjResult.distance = hit.distance;

                break;
            }

        END_PICK:
            SendMouseEnterLeaveEvent();

            lastPickedObj = pickedObj;
            lastPickedCollider = pickedCollider;
        }

        public RaycastObjectHit RaycastObjectList(List<BaseObject> objList) {
            Vector3 pos = Input.mousePosition; pos.z = 0;
            Ray ray = cam.ScreenPointToRay(pos);

            RaycastHit info = new RaycastHit();
            float maxDist = Mathf.Infinity;
            BaseObject resultObj = null;
            RaycastHit resultHit = new RaycastHit();

            for (int i = 0; i < objList.Count; i++) {
                var obj = objList[i];
                for (int c = 0; c < obj.cachedColliders.Length; c++) {
                    Collider coll = obj.cachedColliders[c];
                    if (coll.Raycast(ray, out info, Mathf.Infinity)) {
                        if (info.distance < maxDist) {
                            resultObj = obj;
                            resultHit = info;
                            maxDist = info.distance;
                        }
                    }
                }
            }

            RaycastObjectHit result = new RaycastObjectHit();
            if (resultObj != null) {
                result.picked = true;
                result.obj = resultObj;
                result.distance = resultHit.distance;
                result.point = resultHit.point;
                result.hitTrans = resultHit.transform;
            }
            return result;
        }

        public RaycastObjectHit RaycastPlane(Plane plane) {
            Vector3 pos = Input.mousePosition; pos.z = 0;
            Ray ray = cam.ScreenPointToRay(pos);

            float distance = Mathf.Infinity;
            bool picked = plane.Raycast(ray, out distance);

            RaycastObjectHit result = new RaycastObjectHit();
            if (picked) {
                result.picked = true;
                result.obj = null;
                result.distance = Mathf.Infinity;
                result.point = ray.origin + ray.direction * distance;

            }
            return result;
        }

        public RaycastObjectHit Raycast() {
            RaycastObjectHit result = new RaycastObjectHit();

            if (GUIManager.Instance.IsMouseOverAny()) return result;
            if (GUIUtility.hotControl != 0) return result;

            BaseObject resultObj = null;
            RaycastHit resultHit = new RaycastHit();

            Vector3 pos = Input.mousePosition; pos.z = 0;
            Ray ray = cam.ScreenPointToRay(pos);
            raycastResult = Physics.RaycastAll(ray, Mathf.Infinity, raycastMask);

            // ÅÅÐòraycast½á¹û
            System.Array.Sort(raycastResult, (left, right) => {
                return left.distance.CompareTo(right.distance);
            });

            for (int i = 0; i < raycastResult.Length; i++) {
                RaycastHit hit = raycastResult[i];
                BaseObject foundObj = hit.transform.GetComponent<BaseObject>();
                if (foundObj == null) continue;

                if (!foundObj.PickEnabled) continue;

                resultObj = foundObj;
                resultHit = hit;
                break;
            }

            if (resultObj == null)
                return result;

            result = new RaycastObjectHit();
            if (resultObj != null) {
                result.picked = true;
                result.obj = resultObj;
                result.distance = resultHit.distance;
                result.point = resultHit.point;
                result.hitTrans = resultHit.transform;
            }
            return result;
        }

        public BaseObject GetBaseObject(Transform trans) {
            BaseObject foundObj = null;

            while (trans != null) {

                BaseObject bo = trans.GetComponent<BaseObject>();
                if (bo == null || !bo.PickEnabled) {
                    trans = trans.parent;
                    continue;
                }

                if (bo != null) {
                    foundObj = bo;
                    break;
                }
                trans = trans.parent;
            }

            return foundObj;
        }

        public List<RaycastObjectHit> RaycastAllObjects() {
            RaycastObjectHit result = new RaycastObjectHit();
            List<RaycastObjectHit> list = new List<RaycastObjectHit>();
            if (GUIManager.Instance.IsMouseOverAny()) return list;
            if (GUIUtility.hotControl != 0) return list;

            BaseObject resultObj = null;
            RaycastHit resultHit = new RaycastHit();

            Vector3 pos = Input.mousePosition; pos.z = 0;
            Ray ray = cam.ScreenPointToRay(pos);
            raycastResult = Physics.RaycastAll(ray, Mathf.Infinity, raycastMask);

            System.Array.Sort(raycastResult, (left, right) => {
                return left.distance.CompareTo(right.distance);
            });

            for (int i = 0; i < raycastResult.Length; i++) {
                RaycastHit hit = raycastResult[i];
                string layer = LayerMask.LayerToName(hit.transform.gameObject.layer);
                BaseObject foundObj = GetBaseObject(hit.transform);
                if (foundObj == null) continue;

                if (!foundObj.PickEnabled) continue;
                resultObj = foundObj;
                resultHit = hit;

                if (resultObj != null) {
                    result = new RaycastObjectHit();
                    result.picked = true;
                    result.obj = resultObj;
                    result.distance = resultHit.distance;
                    result.point = resultHit.point;
                    result.hitTrans = resultHit.transform;
                    list.Add(result);
                }
            }
            return list;
        }

        public List<RaycastObjectHit> RaycastAllObjectsByRay(Ray ray) {
            Debug.DrawRay(ray.origin, ray.direction, Color.red, 10000);
            RaycastObjectHit result = new RaycastObjectHit();
            List<RaycastObjectHit> list = new List<RaycastObjectHit>();
            if (GUIManager.Instance.IsMouseOverAny()) return list;
            if (GUIUtility.hotControl != 0) return list;

            BaseObject resultObj = null;
            RaycastHit resultHit = new RaycastHit();
            raycastResult = Physics.RaycastAll(ray, Mathf.Infinity, raycastMask);

            System.Array.Sort(raycastResult, (left, right) => {
                return left.distance.CompareTo(right.distance);
            });

            for (int i = 0; i < raycastResult.Length; i++) {
                RaycastHit hit = raycastResult[i];
                string layer = LayerMask.LayerToName(hit.transform.gameObject.layer);
                BaseObject foundObj = GetBaseObject(hit.transform);
                if (foundObj == null)
                    continue;

                if (!foundObj.PickEnabled)
                    continue;
                resultObj = foundObj;
                resultHit = hit;

                if (resultObj != null) {
                    result = new RaycastObjectHit();
                    result.picked = true;
                    result.obj = resultObj;
                    result.distance = resultHit.distance;
                    result.point = resultHit.point;
                    result.hitTrans = resultHit.transform;
                    list.Add(result);
                }
            }

            return list;
        }

        void SendMouseEnterLeaveEvent() {
            if ((lastPickedObj && lastPickedObj != pickedObj) || (lastPickedCollider && lastPickedCollider != pickedCollider)) {
                BaseObject bo = lastPickedObj.GetComponent<BaseObject>();
                if (bo != null && !Selector.Instance.drawRectangle) {
                    EventManager.TriggerEvent(EventContent.OnMouseLeaveObject, bo);
                }
            }

            if ((pickedObj && lastPickedObj != pickedObj) || (pickedCollider && lastPickedCollider != pickedCollider)) {
                BaseObject bo = pickedObj.GetComponent<BaseObject>();
                if (bo != null && !Selector.Instance.drawRectangle) {
                    EventManager.TriggerEvent(EventContent.OnMouseEnterObject, bo);
                }
            }
        }

        void SendClickEvent(int btn) {
            BaseObject bo = null;

            if (pickedObj != null) {
                bo = pickedObj.GetComponent<BaseObject>();
                if (bo != null) {
                    EventManager.TriggerEvent(EventContent.OnClickObject, (btn, pickedPos));

                    if (btn == 0) {
                        EventManager.TriggerEvent(EventContent.OnLeftClickObject, (btn, pickedPos));
                    }
                    else if (btn == 1) {
                        EventManager.TriggerEvent(EventContent.OnRightClickObject, (btn, pickedPos));
                    }
                }
            }

            if (btn == 0) {
                EventManager.TriggerEvent(EventContent.OnLeftClick, (btn, pickedPos));
            }
            else if (btn == 1) {
                EventManager.TriggerEvent(EventContent.OnRightClick, (btn, pickedPos));
            }

            if (OnSingleClick != null)
                OnSingleClick(btn, bo);
        }

        void SendDoubleClickEvent(int btn) {
            BaseObject bo = null;

            if (pickedObj != null) {
                bo = pickedObj.GetComponent<BaseObject>();
                if (bo != null) {
                    EventManager.TriggerEvent(EventContent.OnDbClickObject, (btn, pickedPos));

                    if (btn == 0) {
                        EventManager.TriggerEvent(EventContent.OnLeftDbClickObject, (btn, pickedPos));
                    }
                    else if (btn == 1) {
                        EventManager.TriggerEvent(EventContent.OnRightDbClickObject, (btn, pickedPos));
                    }
                }
            }

            if (btn == 0) {
                EventManager.TriggerEvent(EventContent.OnLeftDbClick, (btn, pickedPos));
            }
            else if (btn == 1) {
                EventManager.TriggerEvent(EventContent.OnRightDbClick, (btn, pickedPos));
            }

            if (OnDoubleClick != null)
                OnDoubleClick(btn, bo);
        }

        void UpdateDelayClickEvent() {
            if (delayClick >= 0) {
                delayClick += Time.deltaTime;

                if (delayClick > delayClickMax) {
                    SendDelayClickEvent(0);
                    delayClick = -1.0f;
                }
            }
        }

        void SendDelayClickEvent(int btn) {
            BaseObject bo = null;

            if (pickedObj != null) {
                bo = pickedObj.GetComponent<BaseObject>();
                if (bo != null) {
                    EventManager.TriggerEvent(EventContent.OnDelayClickObject, (btn, pickedPos));

                    if (btn == 0)
                        EventManager.TriggerEvent(EventContent.OnLeftDelayClickObject, (btn, pickedPos));
                    else if (btn == 1)
                        EventManager.TriggerEvent(EventContent.OnRightDelayClickObject, (btn, pickedPos));
                }
            }

            if (btn == 0) {
                EventManager.TriggerEvent(EventContent.OnLeftDelayClick, (btn, pickedPos));
            }
            else if (btn == 1) {
                EventManager.TriggerEvent(EventContent.OnRightDelayClick, (btn, pickedPos));
            }
        }

        void OnGUI() {
            bool over = GUIManager.Instance.IsMouseOverAny();
            if (over) return;

            if (GUIUtility.hotControl != 0) return;

            Vector2 cursorPos = new Vector2(Input.mousePosition.x, Input.mousePosition.y);
            if (cursorPos.x < 0 || cursorPos.x > cam.rect.width * Screen.width || cursorPos.y < 0 || cursorPos.y > cam.rect.height * Screen.height)  return;

            var e = Event.current;
            if (e.isMouse) {
                if (e.clickCount == 1 && e.type == UnityEngine.EventType.MouseUp) {
                    SendClickEvent(e.button);
                    delayClick = 0;
                }

                if (e.clickCount == 2 && e.type == UnityEngine.EventType.MouseDown) {
                    SendDoubleClickEvent(e.button);
                    delayClick = -1.0f;
                }
            }
        }
    }
}