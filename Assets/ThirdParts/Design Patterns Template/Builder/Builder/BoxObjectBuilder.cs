using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Caddress.Template.Builder {
    public class BoxObjectBuilder : GameObjectBuilder {
        public override void AddMesh(Mesh mesh) {
            var meshFilter = gameObject.AddComponent<MeshFilter>();
            meshFilter.mesh = mesh;

            var meshRenderer = gameObject.AddComponent<MeshRenderer>();
        }

        public override void AddMaterial(Material material) {
            var renderer = gameObject.GetComponent<MeshRenderer>();
            if (renderer != null) {
                renderer.material = material;
            }
        }

        public override void AddCollider() {
            gameObject.AddComponent<BoxCollider>();
        }
    }
}