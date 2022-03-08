using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
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

            PropertyInfo[] piArr = obj.GetProperties(BindingFlags.DeclaredOnly | BindingFlags.Instance | BindingFlags.Public | BindingFlags.GetProperty | BindingFlags.SetProperty);
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
    }
}
