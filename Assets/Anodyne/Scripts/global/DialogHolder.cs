using System.Collections.Generic;
using System.Xml.Serialization;
using System.Xml;
using System.IO;
using UnityEngine;

[XmlRoot("Root")]
public class DialogHolder
{
	[XmlArray("DialogHolder")]
	[XmlArrayItem("d")]
	public List<DialogData> DialogDatas = new List<DialogData>();

	public static DialogHolder Load(string path)
	{
        if (SaveManager.language == "jp") path += "_jp";
        if (SaveManager.language == "ru") path += "_ru";
        if (SaveManager.language == "es") path += "_es";
        if (SaveManager.language == "fr") path += "_fr";
        if (SaveManager.language == "de") path += "_de";
        if (SaveManager.language == "pt-br") path += "_pt-br";
        if (SaveManager.language == "zh-simp") path += "_zh-simp";
        if (SaveManager.language == "zh-trad") path += "_zh-trad";

        var serializer = new XmlSerializer(typeof(DialogHolder));
		TextAsset ta = Resources.Load(path) as TextAsset;
		using(var stream = new System.IO.StringReader(ta.text))
		{
			return serializer.Deserialize(stream) as DialogHolder;
		}
	}

}