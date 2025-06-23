using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Caddress.Template.FactoryMethod {
    public class ProcessFactory : SimulationDataFactory {
        static ProcessFactory() {
            FactoryRegistry.Register(SimulationType.Process, new ProcessFactory());
        }

        public override SimulationData CreateData() {
            return new ProcessData();
        }
    }
}