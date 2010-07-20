using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Serialization;

namespace SimpleQuery.Impl {
    [Serializable]
    [XmlRoot("queries")]
    public class QueryInfoCollection {
        [XmlElement("query")]
        public QueryInfo[] QueryInfo { get; set; }
    }

    [Serializable]
    public class QueryInfo {
        [XmlAttribute("name")]
        public string Name { get; set; }
        [XmlAttribute("defaultsort")]
        public string DefaultSort { get; set; }
        [XmlAttribute("basequery")]
        public string BaseQuery { get; set; }
        [XmlElement("count")]
        public string CountQuery { get; set; }
        [XmlElement("select")]
        public string SelectQuery { get; set; }
        [XmlElement("parameters")]
        public ParameterInfoCollection Parameters { get; set; }
    }

    [Serializable]
    public class ParameterInfoCollection {
        [XmlElement("parameter")]
        public ParameterInfo[] ParameterInfo { get; set; }
    }

    [Serializable]
    public class ParameterInfo {
        [XmlAttribute("name")]
        public string Name { get; set; }
        [XmlAttribute("size")]
        public int Size { get; set; }
    }
}
