using UnityEngine;
using System.Collections.Generic;

namespace Caddress.Tools {

    public class SelectEffect {

        virtual public void Select(GameObject go, Color selColor) {
        }

        virtual public void Deselect(GameObject go) {
        }

        virtual public void ResestMaterial() {
        }
    }

    public class SelectEffectManager {

        public enum SelectEffectType {
            Hightlingh,
            BoxWire,
        }

        static SelectEffectManager _instance = null;

        public static SelectSetting Setting = new SelectSetting();

        public static SelectEffectManager Instance {
            get {
                if (_instance == null)
                    _instance = new SelectEffectManager();

                return _instance;
            }

            set {
                _instance = value;
            }
        }

        public SelectEffect Create() {
            return Setting.GetEffectManager().CreateEffect();
        }

        virtual protected SelectEffect CreateEffect() {
            return null;
        }
    }

    public class SelectSetting {

        SelectEffectManager defaultFx = null;

        public void SetDefualtEffectManager(SelectEffectManager mgr) {
            defaultFx = mgr;
        }

        public SelectEffectManager GetEffectManager() {
            return defaultFx;
        }

    }

}