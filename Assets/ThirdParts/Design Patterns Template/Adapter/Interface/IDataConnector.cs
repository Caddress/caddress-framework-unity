using System;
using System.Collections;
using System.Collections.Generic;

namespace Caddress.Template.Adapter {
    public interface IDataConnector {
        void Connect();
        void Close();
    }
}