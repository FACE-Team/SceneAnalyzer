using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using System.Xml;
using System.Xml.Serialization;
using System.IO;

using Newtonsoft.Json;
using Newtonsoft.Json.Schema;


namespace ComUtils
{
    public class XmlUtils
    {
        public static string Serialize<T>(T value)
        {
            if (value == null)
            {
                return null;
            }


            XmlSerializer serializer = new XmlSerializer(typeof(T));
            XmlWriterSettings settings = new XmlWriterSettings();
            settings.Indent = false;
            settings.OmitXmlDeclaration = false;

            //using (StringWriter textWriter = new StringWriter())
            //{
            //    using (XmlWriter xmlWriter = XmlWriter.Create(textWriter, settings))
            //    {
            //        serializer.Serialize(xmlWriter, value);
            //    }
            //    return textWriter.ToString();
            //}

            using (MemoryStream memS = new MemoryStream())
            {
                using (XmlWriter writer = XmlTextWriter.Create(memS, settings))
                {
                    serializer.Serialize(writer, value);
                    writer.Close();
                }
                return Encoding.UTF8.GetString(memS.ToArray());
            }
        }


        public static T Deserialize<T>(string xml)
        {
            if (string.IsNullOrEmpty(xml))
            {
                return default(T);
            }

            xml = xml.Replace(@"\", "");
            xml = xml.Substring(1, xml.Length - 2);
            //xml = xml.Replace(@"?", "");
            if(xml.Substring(0,1)=="?")
                xml = xml.Remove(0, 1);
            //System.Diagnostics.Debug.WriteLine("COMUILS: " + xml);

            System.Globalization.CultureInfo cultureInfo = (System.Globalization.CultureInfo)System.Threading.Thread.CurrentThread.CurrentCulture.Clone();
            cultureInfo.NumberFormat.NumberDecimalSeparator = ".";
            System.Threading.Thread.CurrentThread.CurrentCulture = cultureInfo;

        

            XmlSerializer serializer = new XmlSerializer(typeof(T));
            XmlReaderSettings settings = new XmlReaderSettings();


            using (StringReader textReader = new StringReader(xml))
            {
                using (XmlReader xmlReader = XmlReader.Create(textReader, settings))
                {
                    return (T)serializer.Deserialize(xmlReader);
                }
            }
        }
    }
}

   
