using System;
using System.Xml;
using UnityEngine;
using Unity.Robotics.UrdfImporter;

namespace URDFLoader
{
    //定义惯性矩阵（单位：​kg·m²），需满足对称性和正定性。
    public class InertiaNode : URDFNode
    {
        /*
         * 绕x、y、z轴的转动惯量。
         */
        public float ixx;
        public float iyy;
        public float izz;
        /*
         * 惯性积（对称性要求：ixy = iyx, ixz = izx, iyz = izy）。
         */
        public float ixy;
        public float ixz;
        public float iyz;

        public override void Load(XmlNode xmlNode)
        {
            if (xmlNode != null && xmlNode.Attributes != null)
            {
                ixx = ParseFloat(xmlNode, "ixx");
                iyy = ParseFloat(xmlNode, "iyy");
                izz = ParseFloat(xmlNode, "izz");
                ixy = ParseFloat(xmlNode, "ixy");
                ixz = ParseFloat(xmlNode, "ixz");
                iyz = ParseFloat(xmlNode, "iyz");
            }
        }
    }
}
