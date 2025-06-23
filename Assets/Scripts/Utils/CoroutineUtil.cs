using System;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public enum CoroutineFlag
{
    None = 0B0000,
    // coroutine instance start when flag has managed can be stop via stop coroutine interface.
    Managed = 0b0001
}

namespace Caddress {

    /// <summary>
    /// 协程工具类，用于启动协程程序
    /// </summary>
    /// 1. 协程程序：将单帧运行的压力释放到后续多帧执行
    /// 2. 协程并非多线程，它与主线程共享单核计算能力
    public class CoroutineUtil : MonoBehaviour {
        private static CoroutineUtil m_instance;
        public static CoroutineUtil Instance {
            get {
                if (null == m_instance) {
                    var coroutine = new GameObject("__COROUTINE_UTIL__");
                    coroutine.hideFlags = HideFlags.HideInHierarchy;
                    m_instance = coroutine.AddComponent<CoroutineUtil>();
                }
                return m_instance;
            }
        }

        private CoroutineFlag flags = CoroutineFlag.None;
        private List<Coroutine> m_managedCoroutines = new List<Coroutine>();

        private Dictionary<float, WaitForSeconds> m_waitForSeconds = new Dictionary<float, WaitForSeconds>();
        private WaitForSeconds this[float second] {
            get {
                if (!m_waitForSeconds.ContainsKey(second))
                    m_waitForSeconds.Add(second, new WaitForSeconds(second));
                return m_waitForSeconds[second];
            }
        }

        private WaitForEndOfFrame m_waitForEndOfFrame = null;
        private WaitForEndOfFrame waitForEndOfFrame {
            get {
                if (null == m_waitForEndOfFrame)
                    m_waitForEndOfFrame = new WaitForEndOfFrame();
                return m_waitForEndOfFrame;
            }
        }

        private WaitForFixedUpdate m_waitForFixedUpdate = null;
        private WaitForFixedUpdate waitForFixedUpdate {
            get {
                if (null == m_waitForFixedUpdate)
                    m_waitForFixedUpdate = new WaitForFixedUpdate();
                return m_waitForFixedUpdate;
            }
        }

        public void RunStart(System.Object obj, Action callback) {
            var coroutine = StartCoroutine(this.IE_StartCoroutine(obj, callback));
            if (this.flags.HasFlag(CoroutineFlag.Managed))
                m_managedCoroutines.Add(coroutine);
        }

        /// <summary> 启动协同程序 </summary>
        /// <param name="second">等待描述(s)</param>
        /// <param name="callback">回调函数</param>
        public void StartWaitForSeconds(float second, Action callback) {
            var coroutine = StartCoroutine(this.IE_StartCoroutine(this[second], callback));
            if (this.flags.HasFlag(CoroutineFlag.Managed))
                m_managedCoroutines.Add(coroutine);
        }

        /// <summary> 启动WaitForEndOfFrame协同程序 </summary>
        /// <param name="callback">回调函数</param>
        public void StartWaitForEndOfFrame(Action callback) {
            var coroutine = StartCoroutine(this.IE_StartCoroutine(this.waitForEndOfFrame, callback));
            if (this.flags.HasFlag(CoroutineFlag.Managed))
                m_managedCoroutines.Add(coroutine);
        }

        /// <summary>
        /// 启动WaitForFixedUpdate协同程序
        /// </summary>
        /// <param name="callback">回调函数</param>
        public void StartWaitForFixedUpdate(Action callback) {
            var coroutine = StartCoroutine(this.IE_StartCoroutine(this.waitForFixedUpdate, callback));
            if (this.flags.HasFlag(CoroutineFlag.Managed))
                m_managedCoroutines.Add(coroutine);
        }

        public void StartWaitUntil(Func<bool> condition, Action callback, float timeOut = 5f) {
            var coroutine = StartCoroutine(this.IE_StartWaitUntilCoroutine(condition, callback, timeOut));
            if (this.flags.HasFlag(CoroutineFlag.Managed))
                m_managedCoroutines.Add(coroutine);
        }

        private IEnumerator IE_StartCoroutine(System.Object obj, Action callback) {
            yield return obj;
            callback?.Invoke();
        }

        private IEnumerator IE_StartWaitUntilCoroutine(Func<bool> condition, Action callback, float timeOut = 5f) {
            var timeElapsed = 0f;

            while (!condition()) {
                timeElapsed += Time.deltaTime;
                if (timeElapsed > timeOut) {
                    yield break;
                }
                yield return null;
            }
            callback?.Invoke();
        }

        /// <summary> 启用协程托管，管理器会记录已开启的协程程序，允许通过stopManagedCoroutine来释放正在执行的协程程序 </summary>
        public void ManageCoroutine() {
            flags |= CoroutineFlag.Managed;
        }

        /// <summary> 关闭协程托管功能，可使用manageCoroutine来开启 </summary>
        public void UnManageCoroutine() {
            flags ^= CoroutineFlag.Managed;
        }

        /// <summary> 停止当前的托管协程 </summary>
        public void StopManagedCoroutine() {
            if (null == m_managedCoroutines || m_managedCoroutines.Count <= 0)
                return;
            foreach (var managedCoroutine in m_managedCoroutines) {
                if (!ReferenceEquals(managedCoroutine, null))
                    StopCoroutine(managedCoroutine);
            }
            m_managedCoroutines.Clear();
        }
    }
}