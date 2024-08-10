using System.Xml;
using System.Collections.Generic;
using System.Xml.Serialization;

public class DialogData
{ 
	[XmlAttribute("name")]
	public string Name;

	[XmlArray("lines")]
	[XmlArrayItem("l")]
	public List<LineData> lines = new List<LineData>();

}

public class LineData
{ 
	[XmlAttribute("pic")]
	public string SpeakerName = "";

    [XmlAttribute("c")]
    public string Color = "";

	// Advances automatically
	[XmlAttribute("auto")]
	public bool auto = false;


    [XmlAttribute("fadescreen")]
    public bool fadescreen = false;

	[XmlText]
	public string Dialog;

}