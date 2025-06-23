using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Caddress.Template.FactoryMethod {
    public class SinkFactory : SimulationDataFactory {
        static SinkFactory() {
            FactoryRegistry.Register(SimulationType.Sink, new SinkFactory());
        }

        public override SimulationData CreateData() {
            return new SinkData();
        }
    }
}