using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Caddress.Template.Builder {
    public class BuiltObject {
        public GameObject GameObject { get; private set; }

        public BuiltObject(GameObject gameObject) {
            GameObject = gameObject;
        }
    }
}