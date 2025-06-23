using System;
using System.Xml;
using UnityEngine;
using Unity.Robotics.UrdfImporter;

namespace URDFLoader
{
    public class ColorNode : URDFNode
    {
        public Color color;
        public override void Load(XmlNode xmlNode)
        {
            if (xmlNode != null && xmlNode.Attributes != null)
            {
                color = ParseColor(xmlNode, "rgba");
            }
        }
    }
}
