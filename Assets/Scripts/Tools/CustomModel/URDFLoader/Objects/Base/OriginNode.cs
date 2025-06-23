/*
 * 在URDF（Unified Robot Description Format）中，
 * <origin> 标签用于定义坐标系之间的位置和姿态变换。
 * 它是描述机器人部件（如连杆、关节、几何形状等）​相对位置和朝向的核心参数。
 * ​功能：指定当前坐标系相对于其父坐标系的平移（translation）​和旋转（rotation）​。
 */
using System;
using System.Xml;
using UnityEngine;
using Unity.Robotics.UrdfImporter;

namespace URDFLoader
{
    public class OriginNode : URDFNode
    {
        /*
         * 描述：沿父坐标系的 ​X, Y, Z轴平移量，格式为 "x y z"
         * 单位：米（m）
         * ​​默认值​："0 0 0"（无平移）
         */
        public Vector3 xyz;
        /*
        * 描述：绕父坐标系的 ​X→Y→Z轴​ 旋转的欧拉角（Roll, Pitch, Yaw）
        * 单位：弧度（rad）
        * ​默认值："0 0 0"（无旋转）
        */
        public Vector3 rpy;

        public override void Load(XmlNode xmlNode)
        {
            if (xmlNode != null && xmlNode.Attributes != null)
            {
                xyz = ParseVector3(xmlNode, "xyz").Ros2Unity();
                rpy = ParseVector3(xmlNode, "rpy").GetRotationFromUrdfRpy();
            }
        }
    }
}
