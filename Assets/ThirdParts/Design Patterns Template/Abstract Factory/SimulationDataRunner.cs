using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Caddress.Template.AbstractFactory {
    public class SimulationDataRunner{
        private readonly ISimulationFactory _factory;

        public SimulationDataRunner(ISimulationFactory factory) {
            _factory = factory;
        }

        public void Run() {
            SimulationModel m = _factory.CreateModel();
            SimulationView v = _factory.CreateView();
            SimulationController c = _factory.CreateController();
            c.Bind(m, v);
        }
    }
}
