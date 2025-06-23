using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Caddress.Template.Bridge {
    public class EquipmentDecorateSetter : DecorateSetter {

        public EquipmentDecorateSetter(IDecorate obj) : base(obj) { }

        public void OnPassing() {
            _decorateObj.HighLight(Color.green);
        }

        public void OnWarning() {
            _decorateObj.HighLight(Color.yellow);
        }

        public void OnError() {
            _decorateObj.HighLight(Color.red);
        }
    }
}