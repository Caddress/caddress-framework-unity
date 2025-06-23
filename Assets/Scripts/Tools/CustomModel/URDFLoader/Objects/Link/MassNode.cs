using System;
using System.Xml;
using UnityEngine;
using Unity.Robotics.UrdfImporter;

namespace URDFLoader
{
    //定义刚体的质量（单位：​千克）。
    public class MassNode : URDFNode
    {
        /*
         * 质量值，必须为正数
         */
        public float value;

        public override void Load(XmlNode xmlNode)
        {
            if (xmlNode != null && xmlNode.Attributes != null)
            {
                value = ParseFloat(xmlNode, "value");
            }
        }
    }
}
