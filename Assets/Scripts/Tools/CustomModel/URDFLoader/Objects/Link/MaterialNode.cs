using System;
using System.Xml;
using UnityEngine;
using Unity.Robotics.UrdfImporter;

namespace URDFLoader
{
    public class MaterialNode : URDFNode
    {
        //属性：name（引用全局材质名称，需在 <robot> 顶层定义）。
        // name 在基类
        public ColorNode colorNode;

        public override void Load(XmlNode xmlNode)
        {
            base.Load(xmlNode);

            XmlNode colorXML = xmlNode.SelectSingleNode("color");
            if (colorXML != null)
            {
                colorNode = new ColorNode();
                colorNode.Load(colorXML);
            }
        }

    }
}
