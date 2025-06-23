using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Caddress.Template.AbstractFactory {
    public class ProcessFactory : ISimulationFactory {

        static ProcessFactory() {
            FactoryRegistry.Register(SimulationType.Process, new ProcessFactory());
        }

        public SimulationModel CreateModel() {
            return new ProcessModel();
        }

        public SimulationController CreateController() {
            return new ProcessController();
        }

        public SimulationView CreateView() {
            return new ProcessView();
        }
    }
}