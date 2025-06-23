using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Caddress.Template.AbstractFactory {
    public class SinkFactory : ISimulationFactory {

        static SinkFactory() {
            FactoryRegistry.Register(SimulationType.Sink, new SinkFactory());
        }

        public SimulationModel CreateModel() {
            return new SinkModel();
        }

        public SimulationController CreateController() {
            return new SinkController();
        }

        public SimulationView CreateView() {
            return new SinkView();
        }
    }
}