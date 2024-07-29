using System.Xml.Serialization;

namespace SAGPoliceUtilities.Models
{
    public class Jail
    {
        [XmlAttribute]
        public string Name { get; set; }

        [XmlAttribute]
        public float X { get; set; }

        [XmlAttribute]
        public float Y { get; set; }

        [XmlAttribute]
        public float Z { get; set; }
    }
}