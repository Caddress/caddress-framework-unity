using Caddress.UI;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Caddress.Template.Singleton {
    /// <summary>
    /// 单例模式
    /// 管理类
    /// </summary>
    public class Singleton : MonoBehaviour {

        public static Singleton Instance { get; private set; }

        void Awake() {
            Instance = this;
        }
    }
}
