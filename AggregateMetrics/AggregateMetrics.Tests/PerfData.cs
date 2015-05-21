namespace CounterCollection.Tests
{
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.IO;
    using System.Text;
    using System.Xml;
    using System.Xml.Serialization;

    /// <summary>
    /// ALT Perf System .perfdata compatible Xml class
    /// </summary>
    [XmlRoot(ElementName = "ScenarioResults")]
    public class PerfData
    {
        public ScenarioResult ScenarioResult { get; set; }

        [XmlIgnore]
        public string Name { get; set; }

        public string WriteXml(string dropPath)
        {
            string xmlPath = Path.Combine(dropPath, String.Format(CultureInfo.CurrentCulture, "PerfResults_{0}.perfdata", this.Name));

            using (XmlTextWriter xmlTextWriter = new XmlTextWriter(xmlPath, Encoding.UTF8))
            {
                xmlTextWriter.Formatting = Formatting.Indented;

                XmlSerializer serializer = new XmlSerializer(typeof(PerfData));

                serializer.Serialize(xmlTextWriter, this);
            }

            return xmlPath;
        }
    }

    [Serializable]
    public class ScenarioResult
    {
        [XmlAttribute]
        public string Name { get; set; }

        public List<CounterResult> CounterResults { get; set; }
    }

    [Serializable]
    public class CounterResult
    {
        [XmlAttribute]
        public string Name { get; set; }

        [XmlAttribute]
        public string Units { get; set; }

        [XmlAttribute]
        public bool Default { get; set; }

        [XmlAttribute]
        public bool Top { get; set; }

        [XmlText]
        public string Value { get; set; }
    }
}
