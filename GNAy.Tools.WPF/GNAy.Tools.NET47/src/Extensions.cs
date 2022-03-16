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

        public static string ToROCYear(this string obj, DateTime date)
        {
            string yyyy = date.ToString("yyyy");
            string yyy = date.Year.ToROCYear().ToString();
            string yy = date.ToString("yy");

            return obj.Replace("{yyyy}", yyyy).Replace("{yyy}", yyy).Replace("{yy}", yy);
        }

        public static DateTime ToROCYear(this DateTime obj)
        {
            return new DateTime(obj.Year.ToROCYear(), obj.Month, obj.Day, obj.Hour, obj.Minute, obj.Second, obj.Millisecond);
        }

        public static void LoadHolidays(this SortedDictionary<DateTime, string> obj, string path, Encoding encoding, int yyyy, List<string> keywords1, List<string> keywords2)
        {
            string[] separators1 = new string[] { "\"", "," };
            string[] separators2 = new string[] { keywords1[0], keywords1[1] };

            foreach (string line in File.ReadAllLines(path, encoding))
            {
                string[] cells = line.Split(separators1, StringSplitOptions.RemoveEmptyEntries);

                if (cells.Length < 4)
                {
                    continue;
                }

                bool found = false;
                foreach (string keyword in keywords2)
                {
                    if (cells[3].LastIndexOf(keyword) >= 0)
                    {
                        found = true;
                        break;
                    }
                }
                if (!found)
                {
                    continue;
                }

                foreach (string cell in cells)
                {
                    if (cell.Contains(keywords1[0]) && cell.EndsWith(keywords1[1]))
                    {
                        string[] _MMdd = cell.Split(separators2, StringSplitOptions.RemoveEmptyEntries);
                        DateTime holiday = new DateTime(yyyy, int.Parse(_MMdd[0]), int.Parse(_MMdd[1]));
                        obj[holiday] = $"{holiday.DayOfWeek}, {string.Join(", ", cells)}";
                    }
                }
            }
        }
    }
}
