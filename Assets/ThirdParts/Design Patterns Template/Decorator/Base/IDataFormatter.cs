using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Caddress.Template.Decorator {
    public interface IDataFormatter {

        string Format(double value);
    }
}