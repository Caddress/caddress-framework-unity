using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Caddress.Template.AbstractFactory {
    public enum SimulationType {
        None = 0,
        Source = 1,
        Sink = 2,
        Process = 3
    }

    public class FactoryRegistry{

        private static readonly Dictionary<SimulationType, ISimulationFactory> _factories = new();
        public static void Register(SimulationType type, ISimulationFactory factory) {
            _factories[type] = factory;
        }

        public static ISimulationFactory GetFactory(SimulationType type) {
            return _factories.TryGetValue(type, out var factory)
                ? factory
                : throw new NotSupportedException($"Unsupported type {type}");
        }
    }
}
