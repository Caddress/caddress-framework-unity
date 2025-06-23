/*
 * 在URDF（Unified Robot Description Format）中，
 * <joint> 标签用于定义两个连杆（<link>）之间的连接方式和运动特性。
 * 它是机器人模型中描述机械结构（如旋转关节、平移关节等）的核心标签。
 */
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using UnityEngine;

namespace URDFLoader
{
    public enum JointType 
    {
        Revolute,//绕单轴旋转（角度有限制，如舵机）。
        Prismatic,//沿单轴平移（线性运动，如滑块）。	
        Continuous,//绕单轴无限旋转（如车轮）。
        Fixed,//完全固定（无运动）。	
        Floating,//6自由度自由运动（平移和旋转）。	
        Planar //在平面内运动（2D平移 + 1D旋转）。	
    }

    public class JointNode : ObjectNode
    {
        public JointType type;//关节的运动类型

        public ParentNode parent;//指定关节连接的父连杆，父连杆是参考坐标系的基础。

        public ChildNode child;//指定关节连接的子连杆，子连杆的位置和姿态相对于父连杆定义。

        public OriginNode origin;//定义子连杆坐标系相对于父连杆坐标系的平移和旋转偏移。

        public AxisNode axis;//定义关节的运动轴方向​（仅对 revolute、prismatic、continuous 有效）。
        
        public LimitNode limit;//定义关节的运动范围、速度限制和力矩/力限制。

        public DynamicsNode dynamics;//定义关节的动态特性​（如摩擦、阻尼），主要用于仿真（如Gazebo）。

        public override void Load(XmlNode xmlNode)
        {
            base.Load(xmlNode);
            if (xmlNode != null && xmlNode.Attributes != null)
            {
                type = ParseEnum<JointType>(xmlNode, "type");
            }
            var parentXmlNode = xmlNode.SelectSingleNode("parent");
            if (parentXmlNode != null)
            {
                parent = new ParentNode();
                parent.Load(parentXmlNode);
            }
            var childXmlNode = xmlNode.SelectSingleNode("child");
            if (childXmlNode != null)
            {
                child = new ChildNode();
                child.Load(childXmlNode);
            }
            var originXmlNode = xmlNode.SelectSingleNode("origin");
            if (originXmlNode != null)
            {
                origin = new OriginNode();
                origin.Load(originXmlNode);
            }
            var axisXmlNode = xmlNode.SelectSingleNode("axis");
            if (axisXmlNode != null)
            {
                axis = new AxisNode();
                axis.Load(axisXmlNode);
            }
            var limitXmlNode = xmlNode.SelectSingleNode("limit");
            if (limitXmlNode != null)
            {
                limit = new LimitNode();
                limit.Load(limitXmlNode);
            }
            var dynamicsXmlNode = xmlNode.SelectSingleNode("child");
            if (dynamicsXmlNode != null)
            {
                dynamics = new DynamicsNode();
                dynamics.Load(dynamicsXmlNode);
            }
        }

        public override GameObject BuildForCollision(GameObject parent)
        {
            var go = base.BuildForCollision(parent);
            go.transform.localPosition = origin == null ? Vector3.zero : origin.xyz;
            go.transform.localEulerAngles = origin == null ? Vector3.zero : origin.rpy;
            return go;
        }

        public void FindRelative(Dictionary<string, LinkNode> links) 
        {
            if (parent != null)
            {
                parent.FindRelative(links);
                if (parent.linkNode != null)
                {
                    this.parentObjectNode = parent.linkNode;
                    parent.linkNode.childrenObjectNode.Add(this);
                }
            }
            if (child != null)
            {
                child.FindRelative(links);
                if (child.linkNode != null)
                {
                    this.childrenObjectNode.Add(child.linkNode);
                    child.linkNode.parentObjectNode = this;
                }
            }
        }
    }
}
