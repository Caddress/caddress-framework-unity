using System.Xml;
using UnityEngine;
using Caddress;

namespace URDFLoader
{
    public class LinkNode : ObjectNode
    {
        public InertialNode inertial;
        public CollisionNode collision;
        public VisualNode visual;

        public override void Load(XmlNode xmlNode)
        {
            base.Load(xmlNode);

            XmlNode inertialXML = xmlNode.SelectSingleNode("inertial");
            if (inertialXML != null)
            {
                inertial = new InertialNode();
                inertial.Load(inertialXML);
            }

            XmlNode collisionXML = xmlNode.SelectSingleNode("collision");
            if (collisionXML != null)
            {
                collision = new CollisionNode();
                collision.Load(collisionXML);
            }

            XmlNode visualXML = xmlNode.SelectSingleNode("visual");
            if (visualXML != null)
            {
                visual = new VisualNode();
                visual.Load(visualXML);
            }
        }

        public override GameObject BuildForCollision(GameObject parent)
        {
            var go = base.BuildForCollision(parent);
            collision?.BuildForCollision(go);
            return go;
        }

        public Transform FindVisualTransform(Transform parent, bool caseSensitive = true)
        {
            return parent.FindChildRecursive(this.name, caseSensitive);
        }
    }
}
