using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace RuntimeGC
{
    internal class DirectXmlToObject
    {
        public static object ObjectFromXml(Type type, XmlNode xmlRoot, bool doPostLoad = true)
        {
            object obj = Activator.CreateInstance(type);

            foreach (PropertyInfo property in type.GetProperties())
            {
                if (xmlRoot[property.Name] != null)
                {
                    if (property.PropertyType.IsPrimitive || property.PropertyType == typeof(string))
                    {
                        property.SetValue(obj, Convert.ChangeType(xmlRoot[property.Name].InnerText, property.PropertyType));
                    }
                    else
                    {
                        XmlDocument xmlDoc = new XmlDocument();
                        xmlDoc.LoadXml("<" + property.Name + ">" + xmlRoot[property.Name].InnerXml + "</" + property.Name + ">");
                        XmlNode childNode = xmlDoc.DocumentElement;
                        object childObject = ObjectFromXml(property.PropertyType, childNode);
                        property.SetValue(obj, childObject);
                    }
                }
            }

            if (doPostLoad)
            {
                // Call the doPostLoad method on the object if it exists
                MethodInfo doPostLoadMethod = type.GetMethod("DoPostLoad", BindingFlags.Instance | BindingFlags.NonPublic);
                if (doPostLoadMethod != null)
                {
                    doPostLoadMethod.Invoke(obj, null);
                }
            }

            return obj;
        }
    }
}
