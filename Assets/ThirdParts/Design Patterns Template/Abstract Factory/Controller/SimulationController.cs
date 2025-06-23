using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Caddress.Template.AbstractFactory {
    public abstract class SimulationController {
        public abstract void Bind(SimulationModel model, SimulationView view );
    }
}