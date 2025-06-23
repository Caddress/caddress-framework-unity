using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Caddress.Template.Prototype {
    public class Placement : BaseObject {
        public override BaseObject Clone() {

            var result = new Placement();
            result.id = this.id;
            result.type = this.type;
            result.objectName = this.objectName;

            return result;
        }
    }
}