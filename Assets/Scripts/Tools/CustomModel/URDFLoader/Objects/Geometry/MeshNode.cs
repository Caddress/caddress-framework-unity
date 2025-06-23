using System;
using System.Xml;
using UnityEngine;

namespace URDFLoader
{
    public class MeshNode : GeometryItemNode
    {
        public string filename;

        public override void Load(XmlNode xmlNode)
        {
            if (xmlNode != null && xmlNode.Attributes != null)
            {
                filename = ParseString(xmlNode, "filename");
            }
        }
    }
}