using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Caddress.Template.Builder {
    public abstract class GameObjectBuilder {
        protected GameObject gameObject;

        public void CreateNew(string name) {
            gameObject = new GameObject(name);
        }

        public abstract void AddMesh(Mesh mesh);
        public abstract void AddMaterial(Material material);
        public abstract void AddCollider();

        public BuiltObject GetResult() {
            return new BuiltObject(gameObject);
        }
    }
}