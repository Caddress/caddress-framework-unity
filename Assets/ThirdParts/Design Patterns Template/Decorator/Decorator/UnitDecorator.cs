using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Caddress.Template.Decorator {
    public class UnitDecorator : DataFormatterDecorator {
        private string _unit;

        public UnitDecorator(IDataFormatter inner, string unit) : base(inner) {
            _unit = unit;
        }

        public override string Format(double value) {
            return _innerFormatter.Format(value) + _unit;
        }
    }
}