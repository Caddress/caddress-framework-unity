using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Caddress.Template.AbstractFactory {

    public class SimulationManager : MonoBehaviour {

        public static void SetSimulationData(SimulationType type) {
            ISimulationFactory factory = FactoryRegistry.GetFactory(type);
            var controller = new SimulationDataRunner(factory);
            controller.Run();
        }
    }
}