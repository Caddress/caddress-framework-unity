using System;
using System.Xml;
using UnityEngine;
using Caddress;

namespace URDFLoader
{
    public class URDFNode
    {
        public string name;
        public virtual void Load(XmlNode xmlNode) 
        {
            if (xmlNode != null && xmlNode.Attributes != null)
            {
                var nameAttr = xmlNode.Attributes.GetNamedItem("name");
                if (nameAttr != null)
                {
                    name = nameAttr.Value;
                }
                else
                {
                    name = this.GetType().Name;
                }
            }
            else
            {
                name = this.GetType().Name;
            }
        }

        public virtual GameObject BuildForCollision(GameObject parent) 
        {
            var go = new GameObject(name);
            go.transform.SetParent(parent.transform);
            go.transform.localPosition = Vector3.zero;
            go.transform.localEulerAngles = Vector3.zero;
            return go;
        }

        protected float ParseFloat(XmlNode xmlNode, string name) 
        {
            var fAttr = xmlNode.Attributes.GetNamedItem(name);
            if (fAttr != null && float.TryParse(fAttr.Value, out var o))
            {
                return o;
            }
            return 0f;
        }

        protected Color ParseColor(XmlNode xmlNode, string name)
        {
            var v3Attr = xmlNode.Attributes.GetNamedItem(name);
            if (v3Attr == null)
            {
                return Color.white;
            }
            var temp = StringConverter.ParseColor(v3Attr.Value);
            return temp;
        }

        protected Vector3 ParseVector3(XmlNode xmlNode, string name)
        {
            var v3Attr = xmlNode.Attributes.GetNamedItem(name);
            if (v3Attr == null)
            {
                return Vector3.zero;
            }
            var temp = StringConverter.ParseVector3(v3Attr.Value);
            return temp;
        }

        protected string ParseString(XmlNode xmlNode, string name)
        {
            var strAttr = xmlNode.Attributes.GetNamedItem(name);
            if (strAttr == null)
            {
                return null;
            }
            return strAttr.Value;
        }

        protected T ParseEnum<T>(XmlNode xmlNode, string name) where T : struct
        {
            var strAttr = xmlNode.Attributes.GetNamedItem(name);
            if (strAttr == null)
            {
                return default(T);
            }
            return Enum.Parse<T>(strAttr.Value, true);
        }
    }
}
