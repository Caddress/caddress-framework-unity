using System;
using System.Xml;
using UnityEngine;
using Unity.Robotics.UrdfImporter;

namespace URDFLoader
{
    public class BoxNode : GeometryItemNode
    {
        /*
         * size="长(x) 宽(y) 高(z)"：定义立方体的尺寸（单位：米）。
         */
        public Vector3 size;

        public override void Load(XmlNode xmlNode)
        {
            if (xmlNode != null && xmlNode.Attributes != null)
            {
                size = ParseVector3(xmlNode, "size").Ros2UnityScale();
            }
        }

        public override GameObject BuildForCollision(GameObject parent)
        {
            var go = GameObject.CreatePrimitive(PrimitiveType.Cube);
            go.name = GeometryType.Box.ToString();
            go.transform.SetParent(parent.transform);
            go.transform.localPosition = Vector3.zero;
            go.transform.localEulerAngles = Vector3.zero;
            go.transform.localScale = size;
            go.GetComponent<MeshRenderer>().material = Resources.Load<Material>("Materials/URDFCollision");
            return go;
        }
    }
}
