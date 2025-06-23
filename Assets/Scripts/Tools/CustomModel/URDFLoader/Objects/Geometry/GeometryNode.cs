using System;
using System.Xml;
using UnityEngine;

namespace URDFLoader
{
    public enum GeometryType 
    {
        Box,
        Cylinder,
        Sphere,
        Mesh,
        Plane
    }

    public class GeometryNode : URDFNode
    {
        public GeometryItemNode item;

        public override GameObject BuildForCollision(GameObject parent)
        {
            var go = item?.BuildForCollision(parent);
            return go;
        }

        public override void Load(XmlNode xmlNode)
        {
            XmlNode child = xmlNode.SelectSingleNode(GeometryType.Box.ToString().ToLower());
            if (child != null)
            {
                item = new BoxNode();
                item.Load(child);
                item.type = GeometryType.Box;
                return;
            }
            child = xmlNode.SelectSingleNode(GeometryType.Cylinder.ToString().ToLower());
            if (child != null)
            {
                item = new CylinderNode();
                item.Load(child);
                item.type = GeometryType.Cylinder;
                return;
            }
            child = xmlNode.SelectSingleNode(GeometryType.Sphere.ToString().ToLower());
            if (child != null)
            {
                item = new SphereNode();
                item.Load(child);
                item.type = GeometryType.Sphere;
                return;
            }
            child = xmlNode.SelectSingleNode(GeometryType.Mesh.ToString().ToLower());
            if (child != null)
            {
                item = new MeshNode();
                item.Load(child);
                item.type = GeometryType.Mesh;
                return;
            }
        }
    }

    public class GeometryItemNode : URDFNode
    {
        public GeometryType type;
    }
}
