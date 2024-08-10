using System.Collections.Generic;
using System.Xml.Serialization;
using System.Xml;
using System.IO;
using UnityEngine;

[XmlRoot("LevelDataRoot")]
public class LevelDataContainer
{
	[XmlArray("LevelDatas")]
	[XmlArrayItem("LevelData")]
	public List<LevelData> LevelDatas = new List<LevelData>();

	public static LevelDataContainer Load(string path)
	{
		
		var serializer = new XmlSerializer(typeof(LevelDataContainer));
		TextAsset ta = Resources.Load(path) as TextAsset;



	//	using(var stream = new FileStream(path, FileMode.Open))
		using(var stream = new System.IO.StringReader(ta.text))
		{
			return serializer.Deserialize(stream) as LevelDataContainer;
		}
	}
}