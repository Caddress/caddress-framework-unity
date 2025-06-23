using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Caddress.Template.Prototype {
    public class BaseObject{

        public string id { get; set; }
        public string type { get; set; }
        public string objectName { get; set; }

        public virtual BaseObject Clone() {
            return null;
        }
    }
}