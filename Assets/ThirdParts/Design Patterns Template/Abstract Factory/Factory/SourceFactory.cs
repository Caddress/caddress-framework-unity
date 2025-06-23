using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Caddress.Template.AbstractFactory {
    public class SourceFactory : ISimulationFactory{

        static SourceFactory() {
            FactoryRegistry.Register(SimulationType.Source, new SourceFactory());
        }

        public SimulationModel CreateModel() {
            return new SourceModel();
        }

        public SimulationController CreateController() {
            return new SourceController();
        }

        public SimulationView CreateView() {
            return new SourceView();
        }
    }
}