using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Caddress.Template.Decorator {
    public class RawValueFormatter : IDataFormatter {
        public string Format(double value) {
            return value.ToString("F1");
        }
    }
}