using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Caddress.Template.FactoryMethod {
    public abstract class SimulationDataFactory{
        public abstract SimulationData CreateData();
    }
}