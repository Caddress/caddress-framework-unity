
using UnityEngine;
using System.Collections.Generic;
using LitJson;
using System;
using Caddress.UI;

namespace Caddress.Common {

    public partial class Selector : MonoBehaviour {

        // 选择集
        [SerializeField]
        public List<BaseObject> selectedList = new List<BaseObject>();
        public List<BaseObject> SelectedList {
            get { return selectedList; }
            set { selectedList = value; }
        }

        Dictionary<string, BaseObject> selectedDict = new Dictionary<string, BaseObject>();
        public Dictionary<string, BaseObject> SelectedDict {
            get { return selectedDict; }
            set { selectedDict = value; }
        }

        // 候选集
        [SerializeField]
        List<BaseObject> candidateList = new List<BaseObject>();
        public List<BaseObject> CandidateList {
            get { return candidateList; }
        }

        // 进入选择框，但未能确认一定进入选择集合的
        List<BaseObject> tempInsideList = new List<BaseObject>();
        List<BaseObject> tempOutsideList = new List<BaseObject>();

        // 快照
        [SerializeField]
        List<BaseObject> snapSelection = new List<BaseObject>();
        public List<BaseObject> SnapSelection {
            get { return snapSelection; }
            set { snapSelection = value; }
        }

        Dictionary<string, BaseObject> snapSelectionDict = new Dictionary<string, BaseObject>();
        public Dictionary<string, BaseObject> SnapSelectionDict {
            get { return snapSelectionDict; }
            set { snapSelectionDict = value; }
        }

        Vector3 mouseDragDown = Vector3.zero;
        Vector3 mouseDown = Vector3.zero;
        bool moved = false;

        GameObject pickedObj = null;
        Vector3 pickedPos = Vector3.zero;

        List<GameObject> pickedObjList = new List<GameObject>();
        List<GameObject> lastPickedObjList = new List<GameObject>();
        List<Vector3> pickedPosList = new List<Vector3>();
        List<BaseObject> waitAddEventList = new List<BaseObject>();

        int pickedIdx = 0;

        public bool drawRectangle = false;
        bool enableRectangle = true;
        public bool EnableRectangle {
            set { enableRectangle = value; }
            get { return enableRectangle; }
        }

        bool enalbeCheckRectangleUpdate = true;
        public bool EnalbeCheckRectangleUpdate {
            set { enalbeCheckRectangleUpdate = value; }
        }

        float farPlane = 100;

        bool guiHotCtrl = false;
        bool clickInScene = false;

        Camera cam = null;

        GUIStyle boxStyle;

        Color selectColor = new Color(0, 81.0f, 0);
        public Color SelectColor {
            get { return selectColor; }
        }

        public float delayUpdateTime = 0.0f;

        public int limitRectangleSelectNum = int.MaxValue;

        protected Bounds selBounds;
        protected Vector3 selCenter;
        protected bool hasDirty = false;
        public bool HasDirty { set { hasDirty = value; } }
        public delegate void ChangeSelectionDelegate();
        public event ChangeSelectionDelegate OnChangeSelection;
        public event ChangeSelectionDelegate OnSelectedComplete;

        public event ChangeSelectionDelegate DrawSelectRegionStart;
        public event ChangeSelectionDelegate DrawSelectRegionEnd;


        public delegate void SingleClickObjectDelegate(BaseObject obj);
        public event SingleClickObjectDelegate OnObjectSingleClick;

        public static string OnRemoveLastFromSelection = "RemoveLastFromSelection";

        public static Selector Instance { get; set; }

        bool isEditing = true;

        bool multiSelect = false;
        public bool MultiSelect {
            get { return multiSelect; }
            set { multiSelect = value; }
        }

        void Awake() {
            Instance = this;
            cam = Camera.main;

            boxStyle = new GUIStyle();
            boxStyle.normal.background = Resources.Load<Texture2D>("GUI/Selector/SelectBoder");
            boxStyle.border = new RectOffset(2, 2, 2, 2);
            boxStyle.overflow.right = 1;

        }

        void Start() {
            farPlane = cam.farClipPlane;
        }


        void OnDisable() {
            moved = false;
            mouseDragDown = Vector3.zero;
            drawRectangle = false;
            clickInScene = false;
        }

        void OnDestroyObject(BaseObject bo) {
            this.RemoveCandidate(bo);
            this.Unselect(bo);
            if (snapSelectionDict.ContainsKey(bo.ID)) {
                snapSelection.Remove(bo);
                snapSelectionDict.Remove(bo.ID);
            }
        }

        public bool IsSelected(BaseObject bo) {
            return bo.IsSelected;
        }

        public void Select(BaseObject bo) {
            if (bo == null || selectedDict.ContainsKey(bo.ID))
                return;

            if (selectedList.Count >= limitRectangleSelectNum)
                return;

            selectedList.Add(bo);
            selectedDict.Add(bo.ID, bo);

            bo.OnSelect();

            EventManager.TriggerEvent(EventContent.OnObjectSelected, bo);

            hasDirty = true;
        }

        public void Unselect(BaseObject bo) {
            if (bo == null || !selectedDict.ContainsKey(bo.ID))
                return;

            selectedList.Remove(bo);
            selectedDict.Remove(bo.ID);

            if (!bo.CompareTag("Finish")) {
                EventManager.TriggerEvent(EventContent.OnObjectDeselected, bo);
                bo.OnDeselect();
            }

            if (selectedList.Count == 0) {
                EventManager.TriggerEvent(OnRemoveLastFromSelection, bo);
            }

            hasDirty = true;
        }

        public void SelectList(List<BaseObject> boList) {
            bool changed = false;
            for (int i = 0; i < boList.Count; i++) {
                BaseObject bo = boList[i];
                if (bo == null || selectedDict.ContainsKey(bo.ID))
                    continue;
                selectedList.Add(bo);
                selectedDict.Add(bo.ID, bo);

                bo.OnSelect();
                changed = true;
            }

            if (changed)
                hasDirty = true;
        }

        public void Reselect(Transform tr) {
            BaseObject bo = tr.GetComponent<BaseObject>();
            if (bo == null)
                return;
            Reselect(bo);
        }

        public void Reselect(BaseObject bo) {
            if (bo == null)
                return;

            if (selectedList.Count == 1 && selectedDict.ContainsKey(bo.ID))
                return;

            List<BaseObject> tempSelectedList = selectedList;
            selectedList = new List<BaseObject>();
            selectedDict.Clear();

            for (int i = 0; i < tempSelectedList.Count; i++) {
                BaseObject remObj = tempSelectedList[i];
                remObj.OnDeselect();

                EventManager.TriggerEvent(EventContent.OnObjectDeselected, remObj);
            }
            tempSelectedList.Clear();

            selectedList.Add(bo);
            selectedDict.Add(bo.ID, bo);

            bo.OnSelect();

            EventManager.TriggerEvent(EventContent.OnObjectSelected, bo);

            hasDirty = true;
        }

        public bool IsSingleSelected() {
            if (selectedList.Count == 1)
                return true;
            return false;
        }

        public void ClearSelection() {
            bool cleared = false;
            BaseObject baseObj = null;

            List<BaseObject> tempSelectedList = selectedList;
            selectedList = new List<BaseObject>();
            selectedDict.Clear();

            for (int i = 0; i < tempSelectedList.Count; i++) {
                BaseObject bo = tempSelectedList[i];
                if (bo == null || bo.CompareTag("Finish"))
                    continue;
                baseObj = bo;
                bo.OnDeselect();

                EventManager.TriggerEvent(EventContent.OnObjectDeselected, bo);
                cleared = true;
            }
            tempSelectedList.Clear();

            if (cleared && baseObj != null) {
                EventManager.TriggerEvent(OnRemoveLastFromSelection, baseObj);
            }

            if (cleared) {
                hasDirty = true;
                ProcessDirtyData();
            }
        }

        public bool CompareList(List<BaseObject> boList) {
            if (SelectedList.Count != boList.Count)
                return true;

            bool selChanged = false;
            for (int i = 0; i < boList.Count; i++) {
                BaseObject bo = boList[i];

                if (!selectedDict.ContainsKey(bo.ID)) {
                    selChanged = true;
                    break;
                }
            }
            return selChanged;
        }

        public void CloneSelection(List<BaseObject> goList) {
            goList.Clear();
            for (int i = 0; i < selectedList.Count; i++)
                goList.Add(selectedList[i]);
        }

        public void Snapshot() {
            snapSelection.Clear();
            snapSelectionDict.Clear();
            for (int i = 0; i < selectedList.Count; i++) {
                snapSelection.Add(selectedList[i]);
                BaseObject bo = selectedList[i].GetComponent<BaseObject>();
                snapSelectionDict.Add(bo.ID, bo);
            }
        }

        public bool CompareSnapshot() {
            return CompareList(snapSelection);
        }

        public void AddCandidate(BaseObject bo) {
            if (bo == null || bo.IsCandidated)
                return;

            bo.IsCandidated = true;
            candidateList.Add(bo);
        }

        public void AddCandidate(Transform tr) {
            if (tr == null)
                return;

            BaseObject bo = tr.GetComponent<BaseObject>();
            this.AddCandidate(bo);
        }

        public void AddCandidateList(List<BaseObject> objList) {
            for (int i = 0; i < objList.Count; i++)
                AddCandidate(objList[i]);
        }

        public void RemoveCandidate(BaseObject bo) {
            if (bo == null || !bo.IsCandidated)
                return;

            bo.IsCandidated = false;
            candidateList.Remove(bo);
        }

        public void SetCandidateList<T>(List<T> list) where T : BaseObject {
            candidateList.Clear();
            for (int i = 0; i < list.Count; i++)
                AddCandidate(list[i]);
        }

        public void ClearCandidate() {
            for (int i = 0; i < candidateList.Count; i++) {
                BaseObject bo = candidateList[i];
                bo.IsCandidated = false;
            }
            candidateList.Clear();
        }

        public bool IsCandidate(BaseObject bo) {
            return bo.IsCandidated;
        }

        public bool IsCandidate(Transform tr) {
            bool found = false;
            for (int i = 0; i < candidateList.Count; i++) {
                if (candidateList[i] == null) {
                    Debug.LogError("candidateList[i] is null!");
                    continue;
                }
                if (candidateList[i].transform == tr) {
                    found = true;
                    break;
                }
            }
            return found;
        }

        void Update() {

            if (delayUpdateTime > 0) {
                delayUpdateTime -= Time.deltaTime;
                return;
            }

            // 是否按下ctrl
            bool ctrlPressed = Input.GetKey(KeyCode.LeftControl) || Input.GetKey(KeyCode.RightControl);//加/减选
            bool shiftPressed = Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift);//减选
            bool altPressed = Input.GetKey(KeyCode.LeftAlt) || Input.GetKey(KeyCode.RightAlt);//同类选择

            // 鼠标在界面上
            bool over = GUIManager.Instance.IsMouseOverAny();

            if (Input.GetMouseButtonUp(0)) {
                if (!over && !drawRectangle && !moved && clickInScene && !guiHotCtrl) {
                    SingleSelect(ctrlPressed, altPressed);
                }
                else {
                    if (!guiHotCtrl && enableRectangle && drawRectangle) {
                        CheckRectangle(ctrlPressed);
                        this.waitAddEventList.Clear();
                        if (DrawSelectRegionEnd != null)
                            DrawSelectRegionEnd();
                    }
                }

                mouseDragDown = Vector3.zero;
                moved = false;

                clickInScene = false;

                if (drawRectangle && enableRectangle) {
                    drawRectangle = false;
                    hasDirty = true;
                }
            }

            if (!over && !drawRectangle && !moved && clickInScene && !guiHotCtrl) {
                if (Input.GetMouseButtonUp(1)) {
                    ClearSelection();
                    pickedIdx = 0;
                    lastPickedObjList.Clear();
                }
            }

            if (!over && !guiHotCtrl) {

                if (Input.GetMouseButtonDown(0)) {
                    mouseDragDown = Input.mousePosition;
                    mouseDown = Input.mousePosition;
                    moved = false;

                    clickInScene = true;
                }

                if (Input.GetMouseButton(0) && !moved && multiSelect) {
                    bool nomove = (Input.GetAxis("Mouse X") == 0 && Input.GetAxis("Mouse Y") == 0) ||
                            mouseDragDown == Vector3.zero ||
                            Vector3.Distance(mouseDown, Input.mousePosition) < 4;

                    if (!nomove) {
                        if (DrawSelectRegionStart != null)
                            DrawSelectRegionStart();
                        moved = true;

                        if (enableRectangle) {
                            if (!drawRectangle && !ctrlPressed)
                                ClearSelection();
                            drawRectangle = true;

                            if (!ctrlPressed)
                                ClearSelection();
                        }
                    }
                }

                if (drawRectangle && enableRectangle && enalbeCheckRectangleUpdate) {
                    CheckRectangle(ctrlPressed);
                }
            }

        }

        void ProcessDirtyData() {
            if (hasDirty) {
                selBounds = _CalculateSelectedBounds();
                selCenter = _CalculateSelectedCenter();
                if (OnChangeSelection != null)
                    OnChangeSelection();

                if (OnSelectedComplete != null)
                    OnSelectedComplete();

                hasDirty = false;
            }
        }
        void LateUpdate() {
            ProcessDirtyData();
        }

        void SingleSelect(bool ctrlPressed, bool altPressed) {
            if (Vector3.Distance(mouseDown, Input.mousePosition) > 4)
                return;
            Pick();
            if (pickedObj == null) {
                ClearSelection();
                return;
            }

            if (pickedObj && Vector3.Distance(mouseDown, Input.mousePosition) < 100) {
                BaseObject pickedBaseObj = pickedObj.GetComponent<BaseObject>();
                if (OnObjectSingleClick != null) {
                    OnObjectSingleClick(pickedBaseObj);
                }

                if (IsSelected(pickedBaseObj)) {
                    if (ctrlPressed) {
                        Unselect(pickedBaseObj);
                    }
                    else if (altPressed) {
                        SelectSameType(0, pickedBaseObj);
                    }
                    else {
                        Reselect(pickedBaseObj);
                    }
                }
                else {
                    if (IsCandidate(pickedBaseObj)) {
                        if (ctrlPressed) {
                            if (SelectedList.Count == 0) {
                                Select(pickedBaseObj);
                            }
                            else {
                                if (!SelectedList[0].CompareTag(pickedObj.tag)) {
                                    Reselect(pickedBaseObj);
                                }
                                else if (pickedBaseObj is BaseObject) {
                                    Select(pickedBaseObj);
                                }
                                else {
                                    Reselect(pickedBaseObj);
                                }
                            }
                        }
                        else if (altPressed) {
                            SelectSameType(0, pickedBaseObj);
                        }
                        else {
                            Reselect(pickedBaseObj);
                        }
                    }
                    else {
                        ClearSelection();
                    }
                }
            }
            else {
                if (!ctrlPressed) {
                    ClearSelection();
                }
            }
        }

        void Pick() {
            pickedObj = null;
            pickedObjList.Clear();
            pickedPosList.Clear();

            RaycastHit[] hits = PickManager.Instance.RaycastResult;

            for (int i = 0; i < hits.Length; ++i) {
                RaycastHit hit = hits[i];

                BaseObject foundObj = hit.transform.GetComponent<BaseObject>();
                if (foundObj == null)
                    continue;

                pickedObjList.Add(foundObj.gameObject);
                pickedPosList.Add(hit.point);
            }

            if (pickedObjList.Count > 0) {
                if (Util.CompareGameObjectList(pickedObjList, lastPickedObjList))
                    pickedIdx++;
                else
                    pickedIdx = 0;
                if (pickedIdx >= pickedObjList.Count)
                    pickedIdx = 0;
                if (!isEditing)
                    pickedIdx = 0;
                pickedObj = pickedObjList[pickedIdx];
                pickedPos = pickedPosList[pickedIdx];
            }

            lastPickedObjList.Clear();
            for (int j = 0; j < pickedObjList.Count; j++) {
                lastPickedObjList.Add(pickedObjList[j]);
            }
        }

        void CheckRectangle(bool ctrlPressed) {
            Vector3 start = mouseDragDown;
            Vector3 end = Input.mousePosition;

            Vector3 p1 = Vector3.zero;
            Vector3 p2 = Vector3.zero;
            if (start.x > end.x) {
                p1.x = end.x;
                p2.x = start.x;
            }
            else {
                p1.x = start.x;
                p2.x = end.x;
            }

            if (start.y > end.y) {
                p1.y = end.y;
                p2.y = start.y;
            }
            else {
                p1.y = start.y;
                p2.y = end.y;
            }

            tempInsideList.Clear();
            tempOutsideList.Clear();

            for (int i = 0; i < candidateList.Count; i++) {
                BaseObject bo = candidateList[i];

                if (!bo.PickEnabled) continue;

                if (!CanRectangled(bo))  continue;

                bo = TraceGroupObject(bo);

                if (!IsCandidate(bo)) continue;

                Vector3 pos = GetObjectCenter(bo);

                Vector3 point = cam.WorldToScreenPoint(pos);
                if (point.x < p1.x || point.x > p2.x || point.y < p1.y || point.y > p2.y || point.z < 0 || point.z > farPlane) {
                    if (!tempOutsideList.Contains(bo))
                        tempOutsideList.Add(bo);
                }
                else {
                    if (!tempInsideList.Contains(bo))
                        tempInsideList.Add(bo);
                }
            }

            if (!ctrlPressed) {
                for (int i = 0; i < tempInsideList.Count; i++) {
                    BaseObject bo = tempInsideList[i];
                    Select(bo);
                }
                for (int i = 0; i < tempOutsideList.Count; i++) {
                    BaseObject bo = tempOutsideList[i];
                    Unselect(bo);
                }

            }
            else {
                if (ctrlPressed) {
                    for (int i = 0; i < tempInsideList.Count; i++) {
                        BaseObject bo = tempInsideList[i];
                        Select(bo);
                    }
                }
            }
        }

        BaseObject TraceGroupObject(BaseObject bo) {
            Transform trans = bo.transform;
            while (trans != null) {
                if (trans.CompareTag("Placement") || trans.CompareTag("PlacementGroup") || trans.CompareTag("Facade")) {
                    if (!trans.parent.CompareTag("Untagged")) {
                        trans = trans.parent;
                        continue;
                    }
                }

                if (!trans.CompareTag("Untagged")) {
                    if (IsCandidate(trans))
                        break;
                }
            }

            BaseObject retBo = trans.GetComponent<BaseObject>();
            return retBo;
        }

        bool CanRectangled(BaseObject bo) {
            bool isBaseObject = bo is BaseObject ;
            if (isBaseObject) return true;

            return false;
        }

        void DrawSelectRegion() {
            Vector3 startPos = mouseDragDown;
            Vector3 endPos = Input.mousePosition;
            startPos.y = Screen.height - startPos.y;
            endPos.y = Screen.height - endPos.y;

            float left = Mathf.Min(startPos.x, endPos.x);
            float right = Mathf.Max(startPos.x, endPos.x);
            float top = Mathf.Min(startPos.y, endPos.y);
            float bottom = Mathf.Max(startPos.y, endPos.y);

            Rect rc = cam.pixelRect;
            float xMin = rc.xMin + 2;
            float xMax = rc.xMax - 2;
            float yMin = Screen.height - rc.yMax + 2;
            float yMax = Screen.height - rc.yMin - 2;
            left = Mathf.Clamp(left, xMin, xMax);
            right = Mathf.Clamp(right, xMin, xMax);
            top = Mathf.Clamp(top, yMin, yMax);
            bottom = Mathf.Clamp(bottom, yMin, yMax);

            GUI.Box(Rect.MinMaxRect(left, top, right, bottom), "", boxStyle);
        }

        void OnGUI() {
            guiHotCtrl = GUIUtility.hotControl != 0;
            if (drawRectangle && enableRectangle) {
                DrawSelectRegion();
            }
        }

        void UpdateBounds() {
            selCenter = _CalculateSelectedCenter();
            selBounds = _CalculateSelectedBounds();
        }
        public Vector3 CalculateSelectedCenter() {
            return selCenter;
        }
        protected Vector3 _CalculateSelectedCenter() {
            Vector3 pos = Vector3.zero;
            bool first = true;
            for (int i = 0; i < selectedList.Count; i++) {
                BaseObject obj = selectedList[i];
                if (first)
                    pos = obj.transform.position;
                else
                    pos += obj.transform.position;
                first = false;
            }
            Vector3 center = pos / selectedList.Count;
            return center;
        }
        public Bounds CalculateSelectedBounds() {
            UpdateBounds();
            return selBounds;
        }
        public Bounds _CalculateSelectedBounds() {
            Bounds bounds = new Bounds();
            if (selectedList.Count == 0)
                return bounds;

            bool firstBound = true;
            for (int i = 0; i < selectedList.Count; i++) {
                BaseObject bo = selectedList[i];
                Renderer[] renderers = bo.GetComponentsInChildren<Renderer>();
                foreach (Renderer rend in renderers) {
                    if (firstBound) {
                        bounds = rend.bounds;
                        firstBound = false;
                    }
                    else {
                        bounds.Encapsulate(rend.bounds);
                    }
                }
            };
            return bounds;
        }

        public Vector3 CalculateSelectedBoundsCenter() {
            return selBounds.center;
        }

        public Vector3 GetObjectCenter(BaseObject obj) {
            Vector3 pos = obj.transform.position;
            MeshFilter mf = obj.GetComponent<MeshFilter>();
            if (!mf)
                return pos;

            Bounds bounds = mf.mesh.bounds;
            Vector3 selfPos = obj.transform.InverseTransformPoint(bounds.center + pos);
            Vector3 worldPos = obj.transform.TransformPoint(selfPos);
            return worldPos;
        }

        void SelectSameType(int btn, BaseObject bo) {
            if (btn != 0)
                return;

            ClearSelection();

            List<BaseObject> bos = new List<BaseObject>();
            if (bo is BaseObject) {
                //添加相同属性的BaseObject
            }
            if (bos.Count > 0) {
                SelectList(bos);
            }
        }
    }

}