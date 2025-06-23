using System;
using System.Collections;
using System.Collections.Generic;

namespace Caddress.Template.Adapter {
    public interface IMessageBase : IDataConnector {
        void Send(string message);
    }
}
