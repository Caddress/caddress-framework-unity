using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Caddress.Template.FactoryMethod {
    public class SourceFactory : SimulationDataFactory {

        static SourceFactory() {
            FactoryRegistry.Register(SimulationType.Source, new SourceFactory());
        }

        public override SimulationData CreateData() {
            return new SourceData();
        }
    }
}