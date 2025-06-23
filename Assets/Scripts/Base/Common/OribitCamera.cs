using UnityEngine;
using System.Collections.Generic;
using LitJson;
using Caddress.UI;

namespace Caddress.Common {
    [RequireComponent(typeof(Camera))]
    public partial class OrbitCamera : MonoBehaviour {

        public Transform target = null;

        [SerializeField]
        // 在三角函数里，-45度和315是相等的，所以如果是315度，应该先变成-45度，然后用pitchLimit [0]去限制
        public Vector2 pitchLimit = new Vector2(5, 90);

        [HideInInspector]
        public Vector2 heightLimit = new Vector2(0, 12000);

        [SerializeField]
        // Punch问题，往往是速度或阻尼设置不合适，而且要注意检查编辑器上的值和这里可能不一样
        protected float zoomSpeed = 50; 
        protected float zoomDamping = 0.2f;
        [SerializeField]
        public Vector2 zoomLimit = new Vector2(0.1f, 12000.0f);

        [SerializeField]
        protected float panSpeed = 50;
        protected float panDamping = 8;

        public Vector2 rotSpeed = new Vector2(1000, 600);
        public float rotDamping = 10;

        public Vector3 movSpeed = new Vector3(2, 2, 2);
        public bool smooth = true;

        public float farClipPlane = 5000f;
        public float defaultFarClipPlane = 5000f;

        public float panSlope = 0.35f;
        public float minPanSpeed = 0.001f;
        public float maxPanSpeed = 2.0f;

        [HideInInspector]
        public float defaultRadius;

        public bool isControlling = false;

        [HideInInspector]
        public bool enablePan = true;
        [HideInInspector]
        public bool enableRot = true;
        [HideInInspector]
        public bool enableMove = true;
        [HideInInspector]
        public bool enableZoom = true;
        [HideInInspector]
        public bool enableZoomInput = true;
        [HideInInspector]
        public bool enablePanInput = true;
        [HideInInspector]
        public bool enableHightLimit = true;
        [HideInInspector]
        public bool enableMoveInput = true;

        public bool enableLBInput = true;

        private bool enableLeftRotate = false;
        public bool EnableLeftRotate {
            get { return this.enableLeftRotate; }
            set { this.enableLeftRotate = value; }
        }

        //左键拾取 中间平移 右键旋转
        [HideInInspector]
        public bool enableLPMMRR = false;
        public Camera cam = null;
        protected float x = 0.0f;
        protected float y = 0.0f;
        protected float wantedX = 0.0f;
        protected float wantedY = 0.0f;
        //[HideInInspector]
        public float distance = 10.0f;
        [HideInInspector]
        public float wantedDistance = 10.0f;
        protected float panX = 0;
        protected float panY = 0;
        protected float wantedPanX = 0;
        protected float wantedPanY = 0;
        protected bool dragRot = false;
        protected bool dragPan = false;

        // 检测是否要更新中心位置
        // 0不需要，1仅当pick距离（按中键松开时候，检查距离）结果不是很远时候才需要，2肯定需要
        protected int needUpdateCenter = 0;

        protected Vector2 rotValue = new Vector2(0.0f, 0.0f);
        protected Vector2 panValue = new Vector2(0.0f, 0.0f);
        protected float zoomValue = 0;
        protected Vector3 moveOffset = new Vector3(1, 1, 1);

        public bool switchSecondaryRotButton = false;
        protected Plane refPlane = new Plane(Vector3.up, 0);
        public Plane ReferencePlane {
            get { return refPlane; }
            set { refPlane = value; }
        }

        protected string eyeFlyName;
        protected string targetFlyName;

        public delegate void FlyEndDelegate();
        public FlyEndDelegate FlyEndCallback = null;

        bool isFlying = false;
        public bool Flying {
            set { isFlying = value; }
            get { return isFlying; }
        }
        bool moveFlying = false;

        public delegate void Change3DDelegate(bool d);
        public event Change3DDelegate OnChange3D;

        protected float waitTime = 0.0f;
        public float WaitTime {
            get { return waitTime; }
            set { waitTime = value; }
        }

        public bool EnableCollideObject {
            get { return enableCollideObject; }
            set { enableCollideObject = value; }
        }
        protected bool enableCollideObject = false;
        public bool EnableLimitMouseRange = false;

        public bool isOrthograph = false;
        public bool isSideView = false;

        private Vector3 lastMousePos = Vector3.zero;

        public static OrbitCamera Instance { get; set; }

        void Awake() {
            Instance = this;
            cam = GetComponent<Camera>();
            eyeFlyName = "OrbitCamera#" + gameObject.GetInstanceID();
            targetFlyName = "OrbitCameraTarget#" + gameObject.GetInstanceID();
        }

        void Start() {
            if (!target) {
                print("CameraOrbit need target");
                return;
            }
            ResetByTarget();
            ResetByDistance();
        }

        virtual public bool IsOrtho() {
            return cam.orthographic;
        }

        virtual public void Change3D(bool b3D) {
            cam.orthographic = !b3D;
            if (OnChange3D != null)
                OnChange3D(b3D);
        }

        virtual public void ResetByTarget() {
            dragRot = false;
            dragPan = false;
            distance = wantedDistance = Vector3.Distance(transform.position, target.position);
            Vector3 angles = transform.eulerAngles;

            x = wantedX = angles.y < 180 ? angles.y : angles.y - 360;
            y = wantedY = angles.x < 180 ? angles.x : angles.x - 360;

            panX = panY = 0;
            wantedPanX = wantedPanY = 0;

            if (enableRot && !cam.orthographic) {
                UpdateRotate(0, 0, 0);
            }
        }

        void LateUpdate() {
            if (!target)
                return;

            if (moveFlying)
                return;

            if (Flying) {
                gameObject.transform.LookAt(target.position, Vector3.up);
                return;
            }

            if (GUIUtility.hotControl != 0)
                return;

            float frameTime = Time.deltaTime;
            if (frameTime > 0.05f) {
                frameTime = 0.05f;
            }
            if (waitTime > 0.0f) {
                waitTime -= frameTime;
                return;
            }
            if (EnableLimitMouseRange) {
                Vector2 mousePos = Input.mousePosition;
                if (mousePos.x <= 0 || mousePos.x >= Screen.width ||
                    mousePos.y <= 0 || mousePos.y >= Screen.height) {
                    return;
                }
            }
            UpdateInput(frameTime);

            if (needUpdateCenter != 0) {
                if (!cam.orthographic) {
                    SetCenterByPick();
                }
                else {
                    ResetByDistance();
                }
            }

            Vector3 oldPos = transform.position;

            if (enableRot) {
                UpdateRotate(rotValue.x, rotValue.y, frameTime);
            }

            if (enablePan) {
                UpdatePan(panValue.x, panValue.y, frameTime);
            }
            if (enableZoom) {
                UpdateZoom();
            }
                
            if (EnableCollideObject) {
                Vector3 newPos = transform.position;
                Vector3 collidePos = newPos;
                if (!cam.orthographic && CheckCollideObject(oldPos, newPos, out collidePos))
                    transform.position = collidePos;
            }
            bool textFocus = false;
            if (GUIManager.Instance != null) {
                textFocus = GUIManager.Instance.IsInputingText();
            }
                
            if (enableMove && !textFocus) {
                UpdateMove(moveOffset.x, moveOffset.y, moveOffset.z, frameTime);
            }

            if (enableHightLimit && !isOrthograph && !isSideView) {
                LimitHeight();
            }
                
            float h = cam.transform.position.y;
            float nearClipPlane = 0.02f * h;
            if (distance * 0.1f < nearClipPlane) {
                nearClipPlane = distance * 0.1f;
            }

            float farClipPlaneValue = farClipPlane * h / 10f;

            cam.farClipPlane = Mathf.Clamp(farClipPlaneValue, farClipPlane, farClipPlane);

            if (isSideView || isOrthograph) return;

            cam.nearClipPlane = Mathf.Clamp(nearClipPlane, 0.01f, 2f);
        }

        virtual protected void LimitHeight() {
            Vector3 camPos = cam.transform.position;
            if (camPos.y < heightLimit.x) {
                camPos.y = heightLimit.x + cam.nearClipPlane * 1.1f;
                transform.position = camPos;
            }
        }

        virtual protected void UpdateInput(float frameTime) {
            float ix = Input.GetAxis("Mouse X");
            float iy = Input.GetAxis("Mouse Y");
            float iz = Input.GetAxis("Mouse ScrollWheel");

            var inputOffest = Input.mousePosition - lastMousePos;
            if (ix.Equals(0.0f)) {
                ix = inputOffest.x / Screen.width * 50f;
            }
                
            if (iy.Equals(0.0f)) {
                iy = inputOffest.y / Screen.height * 50f;
            }
                
            lastMousePos = Input.mousePosition;
            Vector2 pt = new Vector2(Input.mousePosition.x / Screen.width, Input.mousePosition.y / Screen.height);
            bool inViewport = cam.rect.Contains(pt) && !GUIManager.Instance.IsMouseOverAny();

            int rotButton = 0;
            int panButton = 1;
            int rotButtonSecondary = 2;
            if (switchSecondaryRotButton) {
                panButton = 2;
                rotButtonSecondary = 1;
            }

            if (enableLeftRotate) {
                rotButton = 1;
                panButton = 0;
            }

            bool needRot = false;

            if (enableLPMMRR) {
                panButton = 2;
                rotButtonSecondary = 1;
                if (Input.GetMouseButtonDown(1) && inViewport) {
                    dragRot = true;
                }
                    
                if (Input.GetMouseButtonUp(1)) {
                    dragRot = false;
                }
            }
            else {
                if (enableLBInput) {
                    if (Input.GetMouseButtonDown(rotButton) && inViewport) {
                        dragRot = true;
                    }
                    if (Input.GetMouseButtonUp(rotButton)) {
                        dragRot = false;
                    }
                    if (Input.GetMouseButton(rotButton) && dragRot) {
                        needRot = true;
                    }
                }
                else {
                    dragRot = false;
                }
            }

            if ((Input.GetMouseButtonDown(panButton)) && inViewport) {
                dragPan = true;
            }
            needUpdateCenter = 0;
            if (Input.GetMouseButtonUp(panButton)) {
                dragPan = false;
                needUpdateCenter = 1;
            }

            bool needPan = false;
            if (Input.GetMouseButton(panButton) && dragPan) {
                needPan = true;
            }
            if (cam.orthographic) {
                if ((Input.GetMouseButtonDown(rotButtonSecondary)) && inViewport) {
                    dragPan = true;
                }

                if (Input.GetMouseButtonUp(rotButtonSecondary)) {
                    dragPan = false;
                    needUpdateCenter = 1;
                }

                if (Input.GetMouseButton(rotButtonSecondary) && dragPan) {
                    needPan = true;
                }
            }
            else {
                if (Input.GetMouseButtonDown(rotButtonSecondary) && inViewport) {
                    dragRot = true;
                }
                if (Input.GetMouseButtonUp(rotButtonSecondary)) {
                    dragRot = false;
                }
                if (Input.GetMouseButton(rotButtonSecondary) && dragRot) {
                    needRot = true;
                }
            }
            // 当抬起鼠标中键，或滚轮时候，会根据拾取到的物体来更新
            if (!iz.Equals(0.0f)) {
                needUpdateCenter = 2;
            }
            if (dragPan || dragRot) {
                if (!isControlling && (!ix.Equals(0.0f) || !iy.Equals(0.0f))) {
                    if (PickManager.Instance != null && PickManager.Instance.PickedObj != null && PickManager.Instance.PickedObj.GetComponent<BaseObject>().IsSelected) {
                        isControlling = false;
                    }
                    else {
                        isControlling = true;
                    }
                }
            }
            else {
                isControlling = false;
            }

            moveOffset.Set(0, 0, 0);
            if (inViewport && enableMoveInput) {
                float moveSpeedTime = 1;
                if (Input.GetKey(KeyCode.RightShift) || Input.GetKey(KeyCode.LeftShift)) {
                    moveSpeedTime = 2;
                }
                if (Input.GetKey(KeyCode.W)) {
                    moveOffset.z += movSpeed.z * moveSpeedTime * frameTime;
                }
                if (Input.GetKey(KeyCode.S)) {
                    moveOffset.z -= movSpeed.z * moveSpeedTime * frameTime;
                }
                if (Input.GetKey(KeyCode.A)) {
                    moveOffset.x -= movSpeed.x * moveSpeedTime * frameTime;
                }
                if (Input.GetKey(KeyCode.D)) {
                    moveOffset.x += movSpeed.x * moveSpeedTime * frameTime;
                }
                if (Input.GetKey(KeyCode.Q)) {
                    moveOffset.y += movSpeed.y * moveSpeedTime * frameTime;
                }
                if (Input.GetKey(KeyCode.Z)) {
                    moveOffset.y -= movSpeed.y * moveSpeedTime * frameTime;
                }
            }

            bool overGUI = false;
            if (GUIManager.Instance != null) {
                overGUI = GUIManager.Instance.IsMouseOverAny();
            }

            if (inViewport && enableZoomInput && !overGUI) {
                if (cam.orthographic) {
                    iz = Mathf.Clamp(iz, -0.1f, 0.1f);
                }   
                zoomValue = -iz * zoomSpeed;
            }

            if (Input.GetKey(KeyCode.LeftShift) && Input.GetMouseButton(1)) {
                zoomValue = Input.mousePosition.y >= Screen.height / 2 ? frameTime * zoomSpeed : -frameTime * zoomSpeed;
                dragRot = false;
                dragPan = false;
            }

            if (overGUI) {
                zoomValue = 0;// 鼠标滚轮zoom的同时，鼠标移动到界面上，会造成持续滚动的Bug
            }
            if (needRot) {
                rotValue.x = ix * rotSpeed[0] * frameTime;
                rotValue.y = -iy * rotSpeed[1] * frameTime;
            }
            else {
                rotValue.Set(0.0f, 0.0f);
            }

            if (needPan && enablePanInput) {
                panValue.x = -ix * panSpeed;
                panValue.y = -iy * panSpeed;
            }
            else {
                panValue.Set(0.0f, 0.0f);
            }
        }

        // 这个方法任务是切换旋转中心，用屏幕中心发出射线拾取，得到距离，如果与之前距离相差不大
        virtual protected void SetCenterByPick() {
            bool picked = false;
            Vector3 pickedPos = Vector3.one;

            float newDistance = 0.0f;
            Ray ray = cam.ScreenPointToRay(cam.pixelRect.center);
            RaycastHit hit;
            if (Physics.Raycast(ray, out hit, Mathf.Infinity)) {
                newDistance = Vector3.Distance(hit.point, transform.position);
                pickedPos = hit.point;
                picked = true;
            }
            else {
                if (refPlane.Raycast(ray, out newDistance)) {
                    pickedPos = transform.position + ray.direction * newDistance;
                    picked = true;
                }
            }
            if (!picked)
                return;

            // 如果要求检测距离变化是不是太大了，那么久检测
            if (needUpdateCenter == 1) {
                float refDisScale = 1.618f;
                bool changeTooBig = Mathf.Abs(newDistance - distance) / distance > refDisScale;
                if (changeTooBig)
                    return;
            }

            // 切换中心点
            target.transform.position = pickedPos;
            wantedDistance = distance = newDistance;

            ResetByDistance();
        }

        virtual public void ResetByDistance() {
            float zoomSlope = 1.5f; // 随着距离越来越远，加速越来越大，是距离的slope倍变化
            float minZoomSpeed = 0.25f;
            float maxZoomSpeed = 12000.0f;
            zoomSpeed = Mathf.Clamp(distance * zoomSlope, minZoomSpeed, maxZoomSpeed);
            panSpeed = Mathf.Clamp(distance * panSlope * 0.003f, minPanSpeed, maxPanSpeed);
        }

        virtual public void UpdateMove(float x, float y, float z, float frameTime) {
            Vector3 offset = new Vector3(x, y, z);
            Vector3 offsetWorld = transform.rotation * offset;
            transform.position += offsetWorld;
            target.position += offsetWorld;
        }

        // 仍然保留参数传递，是因为有时候并不使用成员变量做为参数，比如UpdateRotate(0,0,0)
        virtual protected void UpdateRotate(float angleOfYaw, float angleOfPitch, float frameTime) {
            wantedX += angleOfYaw;
            wantedY += angleOfPitch;

            // 2016.2.28 这个限制要小心，要确保wantedY是0到180度，而计算结果有些情况是大于180度的，
            // 因为在三角函数里，-45度和315是相等的，所以如果是315度，应该先变成-45度，然后用pitchLimit [0]去限制
            wantedY = ClampAngle(wantedY, pitchLimit[0], pitchLimit[1]);

            if (smooth) {
                // 太小就直接赋值，否则最后会颤抖
                if (Mathf.Abs(x - wantedX) < 0.15f) {
                    x = wantedX;
                }
                else {
                    x = Mathf.Lerp(x, wantedX, rotDamping * frameTime);
                }
                if (Mathf.Abs(y - wantedY) < 0.15f) {
                    y = wantedY;
                }
                else {
                    y = Mathf.Lerp(y, wantedY, rotDamping * frameTime);
                }
            }
            else {
                x = wantedX;
                y = wantedY;
            }

            Quaternion rotation = Quaternion.Euler(y, x, 0);
            Vector3 newPosition = target.position + rotation * Vector3.back * distance;
            transform.position = newPosition;
            transform.rotation = rotation;
        }

        virtual protected void UpdatePan(float offsetX, float offsetY, float frameTime) {
            wantedPanX += offsetX;
            wantedPanY += offsetY;
            if (smooth) {
                panX = Mathf.Lerp(panX, wantedPanX, panDamping * frameTime);
                panY = Mathf.Lerp(panY, wantedPanY, panDamping * frameTime);
            }
            else {
                panX = wantedPanX;
                panY = wantedPanY;
            }

            Vector3 vec = Vector3.zero;
            if (isSideView) {
                vec = smooth ? transform.forward * (wantedPanX - panX) : transform.forward * offsetX;
            }
            else {
                vec = smooth ? transform.right * (wantedPanX - panX) : transform.right * offsetX;
            }
            // 注意这里其实不是正交，而是顶视图的时候Pan算法不同，不过我这里简化一下，只判断正交了
            if (!cam.orthographic)
                vec += smooth ? transform.up * (wantedPanY - panY) : transform.up * offsetY;
            else {
                if (isOrthograph || isSideView) {
                    vec += smooth ? transform.up * (wantedPanY - panY) : transform.up * offsetY;
                }
                else {
                    vec -= smooth ? transform.forward * (wantedPanY - panY) : transform.forward * offsetY;
                }
            }

            Vector3 newPosition = newPosition = transform.position + vec;
            transform.Translate(vec);
            target.transform.Translate(vec);
        }
        float lastZoomValue = 0;
        public bool isZoomComplete = false;
        virtual protected void UpdateZoom() {
            if (cam.orthographic && Input.GetKey(KeyCode.LeftControl)) {
                if (isOrthograph || isSideView) {
                    float nearClip = cam.nearClipPlane;
                    nearClip -= zoomValue;
                    cam.nearClipPlane = Mathf.Clamp(nearClip, -50, 12000);
                }
            }
            else {
                wantedDistance += zoomValue;
                wantedDistance = Mathf.Clamp(wantedDistance, zoomLimit[0], zoomLimit[1]);

                if (isSideView || isOrthograph) {
                    wantedDistance = Mathf.Clamp(wantedDistance, 0, 200);
                }

                if (smooth) {
                    // 太小就直接赋值，否则最后会颤抖
                    if (Mathf.Abs(distance - wantedDistance) < 0.1f) {
                        distance = wantedDistance;
                    }
                    else {
                        distance = Mathf.Lerp(distance, wantedDistance, zoomDamping);
                    }
                }
                else {
                    distance = wantedDistance;
                }

                if (!cam.orthographic) {
                    Vector3 newPos = target.position + transform.rotation * Vector3.back * distance;
                    transform.position = newPos;				distance = wantedDistance;
                }
                else {
                    cam.orthographicSize = distance;
                }
                if (lastZoomValue == distance) {
                    if (isZoomComplete) {
                        isZoomComplete = false;
                    }
                }
                else {
                    lastZoomValue = distance;
                    if (distance != 10.0f) {
                        isZoomComplete = true;
                    }
                }
            }
        }

        protected bool CheckCollideObject(Vector3 oldPosition, Vector3 newPosition, out Vector3 collidePos) {
            RaycastHit hitInfo;
            Vector3 dir = newPosition - oldPosition;
            //如果新旧方向，是朝上移动的，就不管了，只管往下的
            if (Vector3.Angle(dir, Vector3.down) > 90) {
                collidePos = Vector3.zero;
                return false;
            }

            Collider[] colliders = Physics.OverlapSphere(oldPosition, 2);
            if (colliders.Length > 0) {
                Ray ray = new Ray(oldPosition + new Vector3(0, 0.5f, 0), Vector3.down);
                if (Physics.Raycast(ray, out hitInfo, 2)) {
                    if (hitInfo.distance < 2) {
                        dir = Vector3.ProjectOnPlane(dir, Vector3.up);
                        collidePos = hitInfo.point + new Vector3(0, 1, 0) + dir;
                        return true;
                    }
                }
            }
            Vector3 newPos = newPosition;
            if (Physics.Linecast(oldPosition, newPos, out hitInfo)) {
                dir = Vector3.ProjectOnPlane((newPosition - oldPosition), Vector3.up);
                collidePos = hitInfo.point + new Vector3(0, 0, 0) + dir;
                return true;
            }
            else {
                collidePos = Vector3.zero;
                return false;
            }

        }
        virtual public void Pan(float offsetX, float offsetY) {
            UpdateMove(offsetX, offsetY, 0, Time.deltaTime);
        }

        virtual public void Zoom(float zvalue) {
            UpdateZoom();
        }

        virtual public void FitBestView(GameObject root) {
            FitBestView(root.transform);
        }

        virtual public void FitBestView(GameObject root, Vector3 offsetFactor) {
            FitBestView(root.transform, offsetFactor);
        }

        virtual public void FitBestView(Transform root) {
            Vector3 givenOffset = new Vector3(-0.6f, 1.2f, 0.8f);
            FitBestView(root, givenOffset);
        }

        virtual public void FitBestView(Transform root, Vector3 offsetFactor) {
            Bounds bounds = ObjectUtil.CalculateBounds(root);
            FitBestView(bounds, offsetFactor);
        }

        virtual public void FitBestView(Bounds bounds) {
            Vector3 givenOffset = new Vector3(-0.6f, 1.2f, 0.8f);
            FitBestView(bounds, givenOffset);
        }

        virtual public void FitBestView(Bounds bounds, Vector3 offsetFactor) {
            FitBestView(bounds.center, bounds.extents.magnitude, offsetFactor);
        }

        virtual public void FitBestView(Vector3 c, float r, Vector3 offsetFactor) {
            Vector3 center = c;
            float radius = r;

            if (radius == 0)
                radius = defaultRadius;

            if (cam.orthographic) {
                radius = Mathf.Clamp(radius, 4, 12000);
            }

            Vector3 eyePos = new Vector3(
                center.x + radius * offsetFactor.x,
                center.y + radius * offsetFactor.y,
                center.z + radius * offsetFactor.z
                );

            if (cam.orthographic) {
                eyePos.y = Mathf.Clamp(eyePos.y, 10, 12000);
            }

            transform.position = eyePos;
            transform.LookAt(center);
            target.position = center;

            ResetByTarget();
            ResetByDistance();
        }

        virtual public void PunchDistance(float fac) {
            distance = wantedDistance * fac;
        }

        static float ClampAngle(float angle, float min, float max) {
            if (angle < -360)
                angle += 360;
            if (angle > 360)
                angle -= 360;
            return Mathf.Clamp(angle, min, max);
        }

        virtual public void FlyTo(Transform destTransform, Vector3 offset) {
            FlyTo(destTransform, offset, 0.5f, null);
        }

        virtual public void FlyTo(BaseObject bo, Vector3 offset, float costTime, FlyEndDelegate callback) {
            FlyTo(bo.transform, offset, costTime, callback);
        }

        virtual public void FlyTo(Transform destTransform, Vector3 offset, float costTime, FlyEndDelegate callback) {
            Vector3 destPos = destTransform.TransformPoint(offset);
            Vector3 destCenter = destTransform.position;
            FlyTo(destPos, destCenter, costTime, callback);
        }

        virtual public void FlyTo(Vector3[] camInfo) {
            if (camInfo == null || camInfo.Length < 2)
                return;

            FlyTo(camInfo[0], camInfo[1], 1.0f, null);
        }

        virtual public void FlyTo(Vector3[] camInfo, FlyEndDelegate callback) {
            if (camInfo == null || camInfo.Length < 2)
                return;

            FlyTo(camInfo[0], camInfo[1], 1.0f, callback);
        }

        virtual public void FlyTo(Vector3[] camInfo, float costTime) {
            if (camInfo == null || camInfo.Length < 2)
                return;

            FlyTo(camInfo[0], camInfo[1], costTime, null);
        }

        virtual public void FlyTo(Vector3 destPos, Vector3 destCenter) {
            FlyTo(destPos, destCenter, 0.5f, null);
        }

        virtual public void FlyTo(Vector3 destPos, Vector3 destCenter, float costTime, FlyEndDelegate callback) {
            //delayTime 比看点慢10%，同时也可以确保看点MoveTo先到达
            FlyTo(destPos, destCenter, costTime, costTime * 0.1f, callback);
        }
        virtual public void FlyTo(Vector3 destPos, Vector3 destCenter, float costTime, float delayTime, FlyEndDelegate callback) {
            if (Flying) {
                iTween.StopByName(gameObject, eyeFlyName);
                iTween.StopByName(target.gameObject, targetFlyName);
            }
            Flying = true;

            iTween.MoveTo(gameObject, iTween.Hash(
                "name", eyeFlyName,
                "position", destPos,
                "time", costTime,
                "delay", delayTime,
                "easetype", iTween.EaseType.easeInOutQuad,
                "oncomplete", "OnFlyEnd",
                "oncompletetarget", gameObject
            ));

            iTween.MoveTo(target.gameObject, iTween.Hash(
                "name", targetFlyName,
                "position", destCenter,
                "time", costTime,
                "delay", 0,
                "easetype", iTween.EaseType.easeInOutQuad
            ));
            if (FlyEndCallback != null)
                FlyEndCallback();
            FlyEndCallback = callback;
        }
        virtual public void FlyTo(Transform root, Vector3 currentPos, Vector3 targetPos) {
            FlyTo(root, currentPos, 0.4f, 0f, targetPos);
        }
        virtual public void FlyTo(Transform root, Vector3 currentPos, float costTime, float delayTime, Vector3 targetPos) {
            Vector3 offsetV = targetPos - currentPos;
            Vector3 camPos = target.position + offsetV;

            if (IsOrtho()) {
                Flying = true;
                camPos.y = target.position.y;
            }
            iTween.MoveTo(target.gameObject, iTween.Hash(
                "position", camPos,
                "time", costTime,
                "delay", delayTime,
                "easetype", iTween.EaseType.easeInOutQuad,
                "oncomplete", "MoveEnd",
                "oncompletetarget", gameObject
            ));
        }

        void MoveEnd() {
            Flying = false;
        }

        virtual public void StopFlying() {
            if (Flying) {
                iTween.StopByName(gameObject, eyeFlyName);
                iTween.StopByName(target.gameObject, targetFlyName);
                Flying = false;
            }
        }

        virtual public void FlyToBest(GameObject obj) {
            Bounds bounds = ObjectUtil.CalculateBounds(obj);
            FlyToBest(bounds, new Vector3(-1.0f, 0.8f, -1.0f), true);
        }

        virtual public void FlyToBest(GameObject obj, Vector3 offset) {
            Bounds bounds = ObjectUtil.CalculateBounds(obj);
            FlyToBest(bounds, offset, true);
        }

        public void FlyToBest<T>(List<T> objList, Vector3 offset) where T : BaseObject {
            Bounds bounds = new Bounds();
            for (int i = 0; i < objList.Count; i++) {
                GameObject obj = objList[i].gameObject;
                Bounds b = ObjectUtil.CalculateBounds(obj);
                bounds.Encapsulate(b);
            }

            FlyToBest(bounds, offset, true);
        }

        virtual public void FlyToBest(Bounds bounds) {
            FlyToBest(bounds, new Vector3(-1.0f, 0.8f, -1.0f), true);
        }

        virtual public void FlyToBest(Bounds bounds, Vector3 offset, bool near) {
            float radius = bounds.size.x > bounds.size.z ? bounds.size.x : bounds.size.z;
            radius *= 0.5f;
            Vector3 center = bounds.center;

            Vector3 offsetFactor1 = offset;
            offsetFactor1.z *= -1;
            Vector3 eyePos1 = new Vector3(
                center.x + radius * offsetFactor1.x,
                center.y + radius * offsetFactor1.y,
                center.z + radius * offsetFactor1.z
                );

            Vector3 offsetFactor2 = offset;
            Vector3 eyePos2 = new Vector3(
                center.x + radius * offsetFactor2.x,
                center.y + radius * offsetFactor2.y,
                center.z + radius * offsetFactor2.z
                );

            // 去距离近的那个
            Vector3 eyePos;
            if (near) {
                eyePos = Vector3.Distance(cam.transform.position, eyePos1) < Vector3.Distance(cam.transform.position, eyePos2) ? eyePos1 : eyePos2;
            }
            else {
                eyePos = eyePos2;
            }
            this.FlyTo(eyePos, center);
        }

        virtual protected void OnFlyEnd() {
            transform.LookAt(target.position, Vector3.up); // confirm
            ResetByTarget();
            ResetByDistance();

            if (FlyEndCallback != null) {
                FlyEndCallback();
                FlyEndCallback = null;
            }

            Flying = false;
        }

        protected string recoverState;
        virtual public JsonData Snapshot() {
            string data = "" + cam.transform.position.x.ToString("F2") + "," + cam.transform.position.y.ToString("F2") + "," + cam.transform.position.z.ToString("F2");
            data += "," + target.transform.position.x.ToString("F2") + "," + target.transform.position.y.ToString("F2") + "," + target.transform.position.z.ToString("F2");

            return new JsonData(data);
        }

        virtual public void Recover(JsonData data) {
            this.recoverState = "recovering";

            JsonData datas = JsonMapper.ToObject("[" + (string)data + "]");

            float time = 1f;
            if (datas.Count > 6) {
                time = float.Parse(datas[6].ToString());
            }
            OrbitCamera that = this;
            this.FlyTo(
                new Vector3(float.Parse(datas[0].ToString()), float.Parse(datas[1].ToString()), float.Parse(datas[2].ToString())),
                new Vector3(float.Parse(datas[3].ToString()), float.Parse(datas[4].ToString()), float.Parse(datas[5].ToString())),
                time,
                delegate {
                    that.recoverState = "ok";
                }
            );
        }

        virtual public bool IsRecoverOk() {
            return (this.recoverState == "ok");
        }
    }

}