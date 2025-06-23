using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Caddress.Template.FactoryMethod {
    public class SimulationDataRunner{
        private readonly SimulationDataFactory _factory;

        public SimulationDataRunner(SimulationDataFactory factory) {
            _factory = factory;
        }

        public void SetData() {
            SimulationData data = _factory.CreateData();
            data.SetData();
        }
    }
}
