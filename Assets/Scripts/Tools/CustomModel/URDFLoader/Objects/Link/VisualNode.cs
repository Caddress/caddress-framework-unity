/*
 * <visual> 标签**用于定义机器人部件（<link>）的可视化属性（形状、颜色、材质等），
 * 仅用于仿真或可视化显示（如 RViz）。
 */
using System;
using System.Xml;
using UnityEngine;

namespace URDFLoader
{
    public class VisualNode : URDFNode
    {
        public OriginNode origin;
        public GeometryNode geometry;
        public MaterialNode material;

        public override void Load(XmlNode xmlNode)
        {
            base.Load(xmlNode);
            XmlNode originXML = xmlNode.SelectSingleNode("origin");
            if (originXML != null)
            {
                origin = new OriginNode();
                origin.Load(originXML);
            }
            XmlNode geometryXML = xmlNode.SelectSingleNode("geometry");
            if (geometryXML != null)
            {
                geometry = new GeometryNode();
                geometry.Load(geometryXML);
            }
            XmlNode materialXML = xmlNode.SelectSingleNode("material");
            if (materialXML != null)
            {
                material = new MaterialNode();
                material.Load(materialXML);
            }
        }
    }
}
