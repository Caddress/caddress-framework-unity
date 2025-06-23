using System.Collections.Generic;
using Caddress;

namespace URDFLoader
{
    public class ObjectNode : URDFNode
    {
        public BaseObject parentPlacement;
        public ObjectNode parentObjectNode;
        public List<ObjectNode> childrenObjectNode = new List<ObjectNode>();
    }
}
