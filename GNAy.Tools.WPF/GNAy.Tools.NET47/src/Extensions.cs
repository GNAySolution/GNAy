using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Threading.Tasks;

namespace GNAy.Tools.NET47
{
    public static class Extensions
    {
        /// <summary>
        /// https://stackoverflow.com/questions/18912697/system-componentmodel-descriptionattribute-in-portable-class-library
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        public static Dictionary<string, string> GetPropertyDescriptionMap(this Type obj)
        {
            Dictionary<string, string> result = new Dictionary<string, string>();

            PropertyInfo[] piArr = obj.GetProperties(BindingFlags.Instance | BindingFlags.Public | BindingFlags.GetProperty | BindingFlags.SetProperty);
            foreach (PropertyInfo pi in piArr)
            {
                Attribute att = Attribute.GetCustomAttribute(pi, typeof(DescriptionAttribute), false);
                if (att is DescriptionAttribute dc)
                {
                    result[pi.Name] = dc.Description;
                }
            }

            return result;
        }

        /// <summary>
        /// https://stackoverflow.com/questions/78536/deep-cloning-objects
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="obj"></param>
        /// <returns></returns>
        public static T DeepClone<T>(this T obj)
        {
            using (MemoryStream ms = new MemoryStream())
            {
                BinaryFormatter formatter = new BinaryFormatter();
                formatter.Serialize(ms, obj);
                ms.Seek(0, SeekOrigin.Begin);
                return (T)formatter.Deserialize(ms);
            }
        }

        public static int ToROCYear(this int obj)
        {
            return obj > 1911 ? obj - 1911 : obj;
        }

        public static DateTime ToROCYear(this DateTime obj)
        {
            return new DateTime(obj.Year.ToROCYear(), obj.Month, obj.Day, obj.Hour, obj.Minute, obj.Second, obj.Millisecond);
        }
    }
}
