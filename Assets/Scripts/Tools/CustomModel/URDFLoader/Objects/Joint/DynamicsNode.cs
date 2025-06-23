/*
 * 定义关节的动态特性​（如摩擦、阻尼），主要用于仿真（如Gazebo）
 */
using System;
using System.Xml;
using UnityEngine;

namespace URDFLoader
{
    public class DynamicsNode : URDFNode
    {
        /*
         * 阻尼系数（抵抗运动的黏滞阻力）。
         * 棱柱关节以牛顿秒/米 [N∙s/m] 为单位，旋转关节以牛顿米秒/弧度 [N∙m∙s/rad] 为单位
         */
        public float damping;
        /*
        * 静摩擦力矩或力。
        * 棱柱关节以牛顿 [N] 为单位，旋转关节以牛顿米 [N∙m] 为单位
        */
        public float friction;

        public override void Load(XmlNode xmlNode)
        {
            if (xmlNode != null && xmlNode.Attributes != null)
            {
                damping = ParseFloat(xmlNode, "damping");
                friction = ParseFloat(xmlNode, "friction");
            }
        }
    }
}
