using System;
using System.Xml;
using UnityEngine;

namespace URDFLoader
{
    public class CollisionNode : URDFNode
    {
        public OriginNode origin;
        public GeometryNode geometry;

        public override void Load(XmlNode xmlNode)
        {
            base.Load(xmlNode);

            XmlNode originXML = xmlNode.SelectSingleNode("origin");
            origin = new OriginNode();
            origin.Load(originXML);

            XmlNode geometryXML = xmlNode.SelectSingleNode("geometry");
            geometry = new GeometryNode();
            geometry.Load(geometryXML);
        }

        public override GameObject BuildForCollision(GameObject parent)
        {
            var go = base.BuildForCollision(parent);
            go.transform.localPosition = origin.xyz;
            go.transform.localEulerAngles = origin.rpy;
            geometry.BuildForCollision(go);
            return go;
        }
    }
}
