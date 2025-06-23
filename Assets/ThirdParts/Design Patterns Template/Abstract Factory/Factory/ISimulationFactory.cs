using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Caddress.Template.AbstractFactory {
    public interface ISimulationFactory {
        SimulationModel CreateModel();
        SimulationView CreateView();
        SimulationController CreateController();
    }
}

