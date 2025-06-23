/*
 * 亲属节点
 */
using System;
using System.Collections.Generic;
using System.Xml;
using UnityEngine;

namespace URDFLoader
{
    public class ParentNode : RelativeNode
    {
    }

    public class ChildNode : RelativeNode
    {
    }

    public class RelativeNode : URDFNode
    {
        public string relativeNodeName;
        public LinkNode linkNode;

        public override void Load(XmlNode xmlNode)
        {
            relativeNodeName = ParseString(xmlNode, "link");
        }

        public void FindRelative(Dictionary<string, LinkNode> links)
        {
            if (links.ContainsKey(relativeNodeName))
            {
                linkNode = links[relativeNodeName];
            }
        }
    }
}
