using System;
using System.Xml;
using UnityEngine;
using Unity.Robotics.UrdfImporter;

namespace URDFLoader
{
    public class AxisNode : URDFNode
    {
        /*
         * 定义关节的运动轴方向​（仅对 revolute、prismatic、continuous 有效）
         */
        public Vector3 xyz;

        public override void Load(XmlNode xmlNode)
        {
            if (xmlNode != null && xmlNode.Attributes != null)
            {
                xyz = ParseVector3(xmlNode, "xyz").GetRotationFromUrdfRpy();
            }
        }
    }
}
