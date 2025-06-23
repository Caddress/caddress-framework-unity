using System;
using System.Xml;
using UnityEngine;

namespace URDFLoader
{
    public class SphereNode : GeometryItemNode
    {
        /*
         * 球体的半径（单位：米）。
         */
        public float radius;

        public override void Load(XmlNode xmlNode)
        {
            if (xmlNode != null && xmlNode.Attributes != null)
            {
                radius = ParseFloat(xmlNode, "radius");
            }
        }

        public override GameObject BuildForCollision(GameObject parent)
        {
            var go = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            go.name = GeometryType.Sphere.ToString();
            go.transform.SetParent(parent.transform);
            go.transform.localPosition = Vector3.zero;
            go.transform.localEulerAngles = Vector3.zero;
            go.transform.localScale = Vector3.one * radius * 2f;
            go.GetComponent<MeshRenderer>().material = Resources.Load<Material>("Materials/URDFCollision");
            return go;
        }
    }
}
