using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Caddress.Template.Builder {
    public class ObjectDirector {
        private GameObjectBuilder builder;

        public ObjectDirector(GameObjectBuilder builder) {
            this.builder = builder;
        }

        public BuiltObject Construct(string name, Mesh mesh, Material material) {
            builder.CreateNew(name);
            builder.AddMesh(mesh);
            builder.AddMaterial(material);
            builder.AddCollider();
            return builder.GetResult();
        }
    }
}