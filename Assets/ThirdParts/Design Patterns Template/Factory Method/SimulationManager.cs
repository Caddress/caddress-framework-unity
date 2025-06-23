using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Caddress.Template.FactoryMethod {

    public class SimulationManager : MonoBehaviour {

        public static void SetSimulationData(SimulationType type) {
            SimulationDataFactory factory = FactoryRegistry.GetFactory(type);
            var controller = new SimulationDataRunner(factory);
            controller.SetData();
        }
    }
}