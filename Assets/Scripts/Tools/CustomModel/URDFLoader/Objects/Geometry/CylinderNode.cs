using System;
using System.Xml;
using UnityEngine;

namespace URDFLoader
{
    public class CylinderNode : GeometryItemNode
    {
        /*
         * 圆柱底面半径（单位：米）。
         */
        public float radius;

        /*
         * 圆柱的高度（沿Z轴方向，单位：米）。
         */
        public float length;

        public override void Load(XmlNode xmlNode)
        {
            if (xmlNode != null && xmlNode.Attributes != null)
            {
                radius = ParseFloat(xmlNode, "radius");
                length = ParseFloat(xmlNode, "length");
            }
        }

        public override GameObject BuildForCollision(GameObject parent)
        {
            var go = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            go.name = GeometryType.Cylinder.ToString();
            go.transform.SetParent(parent.transform);
            go.transform.localPosition = Vector3.zero;
            go.transform.localEulerAngles = Vector3.zero;
            go.transform.localScale = new Vector3(radius * 2f, length * 0.5f, radius * 2f);
            go.GetComponent<MeshRenderer>().material = Resources.Load<Material>("Materials/URDFCollision");
            return go;
        }
    }
}
