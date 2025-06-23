using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Caddress.Template.Builder {
    public class BuilderExample : MonoBehaviour {
        public Material boxMaterial;

        void Start() {
            Mesh cubeMesh = CreateCubeMesh();

            GameObjectBuilder builder = new BoxObjectBuilder();
            ObjectDirector director = new ObjectDirector(builder);
            BuiltObject builtBox = director.Construct("BoxObject", cubeMesh, boxMaterial);

            builtBox.GameObject.transform.position = new Vector3(0, 1, 0);
        }

        Mesh CreateCubeMesh() {
            GameObject temp = GameObject.CreatePrimitive(PrimitiveType.Cube);
            Mesh mesh = temp.GetComponent<MeshFilter>().mesh;
            GameObject.Destroy(temp);
            return mesh;
        }
    }
}