using System;
using System.Runtime.Serialization;
using System.Xml.Serialization;

namespace Client.Model
{
	[Serializable] //XML
	[DataContract] //Json
	public class Point3
	{
		[XmlAttribute("X")]
		[DataMember] //Json
		public float X { get; set; }

		[XmlAttribute("Y")]
		[DataMember] //Json
		public float Y { get; set; }

		[XmlAttribute("Z")]
		[DataMember] //Json
		public float Z { get; set; }
	}
}
