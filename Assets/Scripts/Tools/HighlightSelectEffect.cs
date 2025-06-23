using UnityEngine;
using HighlightingSystem;

namespace Caddress.Tools {
    public class HighlightSelectEffect : SelectEffect {

        protected Highlighter highLighter = null;

        override public void Select(GameObject go, Color selColor) {
            highLighter = go.GetComponent<Highlighter>();
            if (highLighter == null)
                highLighter = go.AddComponent<Highlighter>();
            highLighter.ReinitMaterials();
            highLighter.ConstantOn(selColor);
            highLighter.ConstantOnImmediate();
        }

        override public void Deselect(GameObject go) {
            highLighter = go.GetComponent<Highlighter>();
            if (highLighter == null)
                highLighter = go.AddComponent<Highlighter>();

            highLighter.ConstantOffImmediate();
        }

        override public void ResestMaterial() {
            if (highLighter != null)
                highLighter.ReinitMaterials();
        }
    }

    public class HighlightSelectEffectManager : SelectEffectManager {

        override protected SelectEffect CreateEffect() {
            return new HighlightSelectEffect();
        }
    }

}