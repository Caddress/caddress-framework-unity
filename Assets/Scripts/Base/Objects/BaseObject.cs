using Caddress.Tools;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Caddress {

    public class BaseObject : MonoBehaviour {

        public string ID { get; set; }

        public bool selected = false;
        public bool IsSelected {
            get { return selected; }
        }

        protected bool candidate = false;
        public bool IsCandidated {
            set { candidate = value; }
            get { return candidate; }
        }

        protected bool isPickEnabled = true;
        public bool PickEnabled {
            get { return isPickEnabled; }
            set { SetPickEnabled(value); }
        }

        public Collider[] cachedColliders;
        protected SelectEffect selEffect = null;
        protected Color highColor = Color.cyan;

        public BaseObject() {
            selEffect = SelectEffectManager.Instance.Create();
        }

        private void Awake() {
            ID = string.Empty;
        }

        public virtual void OnSelect() {
            selected = true;
            selEffect.Select(gameObject, highColor);
        }

        public virtual void OnDeselect() {
            selected = false;
            selEffect.Deselect(gameObject);
        }

        public virtual void SetPickEnabled(bool bEnable) {
            isPickEnabled = bEnable;
        }
    }
}
