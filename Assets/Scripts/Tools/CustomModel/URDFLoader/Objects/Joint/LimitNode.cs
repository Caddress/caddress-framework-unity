/*
 * 在URDF（Unified Robot Description Format）中，
 * <limit> 标签用于定义关节（joint）的运动约束，包括位置范围、速度限制和力矩限制。
 * 它通常出现在 ​旋转关节（revolute）​、棱柱关节（prismatic）​​ 和 ​连续关节（continuous）​​ 的定义中，
 * 是机器人建模时确保物理合理性的关键参数。
 */
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace URDFLoader
{
    public class LimitNode : URDFNode
    {
        /*
         * 描述：关节的最小位置​（旋转关节为弧度，棱柱关节为米）
         * 单位：弧度（rad）或米（m）
         * ​适用关节类型：revolute, prismatic
         */
        public float lower;
        /*
        * 描述：关节的最大位置​（旋转关节为弧度，棱柱关节为米）
        * 单位：弧度（rad）或米（m）
        * ​适用关节类型：revolute, prismatic
        */
        public float upper;
        /*
        * 描述：关节的最大力矩或力​（绝对值，取决于关节类型：旋转关节为力矩，棱柱关节为力）
        * 单位：牛·米（Nm）或牛（N）
        * ​适用关节类型：	revolute, prismatic, continuous
        */
        public float effort;
        /*
        * 描述：关节的最大速度​（绝对值，正向或反向均受此限制）
        * 单位：弧度/秒 或 米/秒
        * ​适用关节类型：revolute, prismatic, continuous
        */
        public float velocity;

        public override void Load(XmlNode xmlNode)
        {
            if (xmlNode != null && xmlNode.Attributes != null)
            {
                lower = ParseFloat(xmlNode, "lower");
                upper = ParseFloat(xmlNode, "upper");
                effort = ParseFloat(xmlNode, "effort");
                velocity = ParseFloat(xmlNode, "velocity");
            }
        }
    }
}
