using System.Xml;
using System.Xml.Serialization;

public class LevelData
{ 
	[XmlAttribute("SceneName")]
	public string SceneName;

	[XmlElement("Name")]
	public string Name;

	[XmlElement("ExitName")]
	public string ExitName;
}