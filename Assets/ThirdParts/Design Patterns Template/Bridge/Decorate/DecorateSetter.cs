using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Caddress.Template.Bridge {
    public class DecorateSetter {

        protected IDecorate _decorateObj;

        public DecorateSetter(IDecorate obj) {
            _decorateObj = obj;
        }

        public virtual void Show(bool bShow) {
            if (_decorateObj.IsShow()) {
                _decorateObj.Hide();
            }
            else {
                _decorateObj.Show();
            }
        }

        public virtual void OnSelect() {
            _decorateObj.HighLight(Color.cyan);
        }

        public virtual void OnNormal() {
            _decorateObj.HighLight(Color.clear);
        }

        public virtual void Alert() {
            _decorateObj.Flash(Color.red);
        }
    }
}