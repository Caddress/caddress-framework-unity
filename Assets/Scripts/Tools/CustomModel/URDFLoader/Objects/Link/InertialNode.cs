using System;
using System.Xml;
using UnityEngine;

namespace URDFLoader
{
    public class InertialNode : URDFNode
    {
        public OriginNode origin;
        public MassNode mass;
        public InertiaNode inertia;

        public override void Load(XmlNode xmlNode)
        {
            base.Load(xmlNode);

            XmlNode originXML = xmlNode.SelectSingleNode("origin");
            if (originXML != null)
            {
                origin = new OriginNode();
                origin.Load(originXML);
            }

            XmlNode massXML = xmlNode.SelectSingleNode("mass");
            if (massXML != null)
            {
                mass = new MassNode();
                mass.Load(massXML);
            }

            XmlNode inertiaXML = xmlNode.SelectSingleNode("inertia");
            if (inertiaXML != null)
            {
                inertia = new InertiaNode();
                inertia.Load(inertiaXML);
            }
        }
    }
}
