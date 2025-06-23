using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Caddress.Template.Decorator {
    public abstract class DataFormatterDecorator : IDataFormatter {
        protected IDataFormatter _innerFormatter;

        public DataFormatterDecorator(IDataFormatter innerFormatter) {
            _innerFormatter = innerFormatter;
        }

        public abstract string Format(double value);
    }
}