using System.Collections.Generic;
using System.Xml;

namespace URDFLoader
{
    public class RobotNode : URDFNode
    {
        public Dictionary<string,LinkNode> linkNodes;
        public Dictionary<string, JointNode> jointNodes;
        public List<ObjectNode> allNodes;

        public Dictionary<string, JointNode> NotFixedJointNodes {
            get {
                Dictionary<string, JointNode> result = new Dictionary<string, JointNode>();
                foreach(var key in jointNodes.Keys) {
                    var node = jointNodes[key];
                    if(node.type != JointType.Fixed) {
                        result[key] = node;
                    }
                }
                return result;
            }
        }

        public override void Load(XmlNode xmlNode)
        {
            base.Load(xmlNode);
            linkNodes = new Dictionary<string, LinkNode>();
            jointNodes = new Dictionary<string, JointNode>();
            allNodes = new List<ObjectNode>();
            if (xmlNode.HasChildNodes)
            {
                foreach (XmlNode linkNode in xmlNode.SelectNodes("link"))
                {
                    var link = new LinkNode();
                    link.Load(linkNode);
                    linkNodes.Add(link.name, link);
                }
                foreach (XmlNode jointNode in xmlNode.SelectNodes("joint"))
                {
                    var typeAttr = jointNode.Attributes.GetNamedItem("type");
                    if (typeAttr != null /*&& typeAttr.Value != "fixed"*/)
                    {
                        var joint = new JointNode();
                        joint.Load(jointNode);
                        joint.FindRelative(linkNodes);
                        jointNodes.Add(joint.name, joint);
                    }
                }
            }
            allNodes.AddRange(linkNodes.Values);
            allNodes.AddRange(jointNodes.Values);
        }
    }
}
