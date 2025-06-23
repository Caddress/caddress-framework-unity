using System.Xml;

namespace URDFLoader
{
    public class URDFElement
    {
        public RobotNode robotNode;

        public void Load(string path) 
        {
            XmlDocument urdfDoc = new XmlDocument();
            urdfDoc.Load(path);
            var list = urdfDoc.GetElementsByTagName("robot");
            robotNode = new RobotNode();
            robotNode.Load(list[0]);
        }
    }
}
