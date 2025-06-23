using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Caddress.Template.Decorator {
    public class ThresholdColorDecorator : DataFormatterDecorator {
        private double _low;
        private double _high;

        public ThresholdColorDecorator(IDataFormatter inner, double low, double high) : base(inner) {
            _low = low;
            _high = high;
        }

        public override string Format(double value) {
            string formatted = _innerFormatter.Format(value);
            string color = "green";

            if (value < _low) color = "blue";
            else if (value > _high) color = "red";

            return $"<color={color}>{formatted}</color>";
        }
    }
}