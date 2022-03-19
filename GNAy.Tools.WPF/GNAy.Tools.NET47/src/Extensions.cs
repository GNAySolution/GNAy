using System;
using System.Collections.Generic;
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
        /// <param name="flags"></param>
        /// <returns></returns>
        public static Dictionary<string, (ColumnAttribute, PropertyInfo)> GetColumnAttrMapByProperty(this Type obj, BindingFlags flags)
        {
            Dictionary<string, (ColumnAttribute, PropertyInfo)> result = new Dictionary<string, (ColumnAttribute, PropertyInfo)>();
            PropertyInfo[] piArr = obj.GetProperties(flags);

            foreach (PropertyInfo pi in piArr)
            {
                Attribute attr = Attribute.GetCustomAttribute(pi, typeof(ColumnAttribute), false);
                if (attr is ColumnAttribute csv)
                {
                    result.Add(pi.Name, (csv, pi));
                }
            }

            return result;
        }

        public static SortedDictionary<int, (ColumnAttribute, PropertyInfo)> GetColumnAttrMapByIndex(this Type obj, BindingFlags flags)
        {
            SortedDictionary<int, (ColumnAttribute, PropertyInfo)> result = new SortedDictionary<int, (ColumnAttribute, PropertyInfo)>();
            PropertyInfo[] piArr = obj.GetProperties(flags);

            foreach (PropertyInfo pi in piArr)
            {
                Attribute attr = Attribute.GetCustomAttribute(pi, typeof(ColumnAttribute), false);
                if (attr is ColumnAttribute csv && csv.Index >= 0)
                {
                    result.Add(csv.Index, (csv, pi));
                }
            }

            return result;
        }


        public static string PropertyValueToString(this PropertyInfo obj, object instance, string format)
        {
            object value = obj.GetValue(instance);

            if (!(value is object))
            {
                return null;
            }
            else if (string.IsNullOrWhiteSpace(format))
            {
                return value.ToString();
            }

            Type declaring = obj.PropertyType;

            if (declaring == typeof(string) || declaring == typeof(StringBuilder))
            {
                return value.ToString();
            }
            else if (declaring == typeof(DateTime))
            {
                return ((DateTime)value).ToString(format);
            }
            else if (declaring == typeof(decimal))
            {
                return ((decimal)value).ToString(format);
            }
            else if (declaring == typeof(double))
            {
                return ((double)value).ToString(format);
            }
            else if (declaring == typeof(float))
            {
                return ((float)value).ToString(format);
            }
            else if (declaring == typeof(long))
            {
                return ((long)value).ToString(format);
            }
            else if (declaring == typeof(int))
            {
                return ((int)value).ToString(format);
            }
            else if (declaring == typeof(short))
            {
                return ((short)value).ToString(format);
            }
            else if (declaring == typeof(byte))
            {
                return ((byte)value).ToString(format);
            }
            
            throw new NotSupportedException($"DeclaringType ({declaring.FullName}) is not supported.");
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

        /// <summary>
        /// https://www.twse.com.tw/zh/holidaySchedule/holidaySchedule
        /// </summary>
        /// <param name="obj"></param>
        /// <param name="path"></param>
        /// <param name="encoding"></param>
        /// <param name="yyyy"></param>
        /// <param name="keywords1"></param>
        /// <param name="keywords2"></param>
        public static void LoadHolidays(this IDictionary<DateTime, string> obj, string path, Encoding encoding, int yyyy, IList<string> keywords1, IList<string> keywords2)
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
