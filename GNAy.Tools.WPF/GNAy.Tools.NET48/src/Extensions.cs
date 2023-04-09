using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Threading.Tasks;

namespace GNAy.Tools.NET48
{
    public static class Extensions
    {
        public static IEnumerable<string> SplitWithoutWhiteSpace(this string obj, params char[] separator)
        {
            if (string.IsNullOrWhiteSpace(obj))
            {
                yield break;
            }

            foreach (string cell in obj.Split(separator))
            {
                yield return cell;
            }
        }

        public static IEnumerable<string> ForeachSortedSet(this string obj, params char[] separator)
        {
            if (string.IsNullOrWhiteSpace(obj))
            {
                yield break;
            }

            foreach (string cell in new SortedSet<string>(obj.Split(separator)))
            {
                yield return cell;
            }
        }

        public static string JoinSortedSet(this string obj, in char joinSeparator, params char[] setSeparator)
        {
            return setSeparator.Length <= 0 ? string.Join(joinSeparator.ToString(), obj.ForeachSortedSet(joinSeparator)) : string.Join(joinSeparator.ToString(), obj.ForeachSortedSet(setSeparator));
        }

        /// <summary>
        /// https://codertw.com/%E5%89%8D%E7%AB%AF%E9%96%8B%E7%99%BC/220001/
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        public static string GetDescription(this Enum obj)
        {
            FieldInfo field = obj.GetType().GetField(obj.ToString());
            DescriptionAttribute attr = (DescriptionAttribute)Attribute.GetCustomAttribute(field, typeof(DescriptionAttribute), false);

            return attr.Description;
        }

        /// <summary>
        /// https://stackoverflow.com/questions/18912697/system-componentmodel-descriptionattribute-in-portable-class-library
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="obj"></param>
        /// <param name="flags"></param>
        /// <returns></returns>
        public static Dictionary<string, (T, PropertyInfo)> GetColumnAttrMapByProperty<T>(this Type obj, in BindingFlags flags) where T : ColumnAttribute
        {
            Dictionary<string, (T, PropertyInfo)> result = new Dictionary<string, (T, PropertyInfo)>();
            PropertyInfo[] piArr = obj.GetProperties(flags);

            foreach (PropertyInfo pi in piArr)
            {
                try
                {
                    Attribute attr = Attribute.GetCustomAttribute(pi, typeof(T), false);

                    if (attr is T column)
                    {
                        result.Add(pi.Name, (column, pi));
                    }
                }
                catch (ArgumentException ex)
                {
                    throw new ArgumentException($"{pi.Name}|{ex.Message}");
                }
                catch
                {
                    throw;
                }
            }

            return result;
        }

        public static SortedDictionary<int, (T, PropertyInfo)> GetColumnAttrMapByIndex<T>(this Type obj, in BindingFlags flags) where T : ColumnAttribute
        {
            SortedDictionary<int, (T, PropertyInfo)> result = new SortedDictionary<int, (T, PropertyInfo)>();
            PropertyInfo[] piArr = obj.GetProperties(flags);
            int count = -1;

            foreach (PropertyInfo pi in piArr)
            {
                Attribute attr = Attribute.GetCustomAttribute(pi, typeof(T), false);
                ++count;

                if (attr is T column && column.CSVIndex >= 0)
                {
                    try
                    {
                        if (flags.HasFlag(BindingFlags.GetProperty) && pi.CanRead)
                        {
                            result.Add(column.CSVIndex == 0 ? count : column.CSVIndex, (column, pi));
                        }
                        else if (flags.HasFlag(BindingFlags.SetProperty) && pi.CanWrite)
                        {
                            result.Add(column.CSVIndex == 0 ? count : column.CSVIndex, (column, pi));
                        }
                    }
                    catch (ArgumentException ex)
                    {
                        throw new ArgumentException($"{column.CSVName}|{column.CSVIndex}|{ex.Message}");
                    }
                    catch
                    {
                        throw;
                    }
                }
            }

            return result;
        }

        public static Dictionary<string, (T, PropertyInfo)> GetColumnAttrMapByName<T>(this Type obj, in BindingFlags flags) where T : ColumnAttribute
        {
            Dictionary<string, (T, PropertyInfo)> result = new Dictionary<string, (T, PropertyInfo)>();
            PropertyInfo[] piArr = obj.GetProperties(flags);

            foreach (PropertyInfo pi in piArr)
            {
                Attribute attr = Attribute.GetCustomAttribute(pi, typeof(T), false);

                if (attr is T column && column.CSVIndex >= 0)
                {
                    try
                    {
                        if (flags.HasFlag(BindingFlags.GetProperty) && pi.CanRead)
                        {
                            result.Add(column.CSVName, (column, pi));
                        }
                        else if (flags.HasFlag(BindingFlags.SetProperty) && pi.CanWrite)
                        {
                            result.Add(column.CSVName, (column, pi));
                        }
                    }
                    catch (ArgumentException ex)
                    {
                        throw new ArgumentException($"{column.CSVName}|{column.CSVIndex}|{ex.Message}");
                    }
                    catch
                    {
                        throw;
                    }
                }
            }

            return result;
        }

        public static string ValueToString(this PropertyInfo obj, in object instance, in string format)
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

            Type propertyType = obj.PropertyType;

            if (propertyType == typeof(string) || propertyType == typeof(StringBuilder))
            {
                return value.ToString();
            }
            else if (propertyType == typeof(DateTime))
            {
                return ((DateTime)value).ToString(format);
            }
            else if (propertyType == typeof(DateTime?))
            {
                return ((DateTime?)value).Value.ToString(format);
            }
            else if (propertyType == typeof(decimal))
            {
                return ((decimal)value).ToString(format);
            }
            else if (propertyType == typeof(decimal?))
            {
                return ((decimal?)value).Value.ToString(format);
            }
            else if (propertyType == typeof(double))
            {
                return ((double)value).ToString(format);
            }
            else if (propertyType == typeof(double?))
            {
                return ((double?)value).Value.ToString(format);
            }
            else if (propertyType == typeof(float))
            {
                return ((float)value).ToString(format);
            }
            else if (propertyType == typeof(float?))
            {
                return ((float?)value).Value.ToString(format);
            }
            else if (propertyType == typeof(long))
            {
                return ((long)value).ToString(format);
            }
            else if (propertyType == typeof(long?))
            {
                return ((long?)value).Value.ToString(format);
            }
            else if (propertyType == typeof(int))
            {
                return ((int)value).ToString(format);
            }
            else if (propertyType == typeof(int?))
            {
                return ((int?)value).Value.ToString(format);
            }
            else if (propertyType == typeof(short))
            {
                return ((short)value).ToString(format);
            }
            else if (propertyType == typeof(short?))
            {
                return ((short?)value).Value.ToString(format);
            }
            else if (propertyType == typeof(byte))
            {
                return ((byte)value).ToString(format);
            }
            else if (propertyType == typeof(byte?))
            {
                return ((byte?)value).Value.ToString(format);
            }

            throw new NotSupportedException($"PropertyType ({propertyType.FullName}) is not supported.");
        }

        public static void SetValueFromString(this PropertyInfo obj, in object instance, in string value, in string format)
        {
            Type propertyType = obj.PropertyType;

            if (propertyType == typeof(string))
            {
                obj.SetValue(instance, value);
            }
            else if (propertyType == typeof(StringBuilder))
            {
                obj.SetValue(instance, new StringBuilder(value));
            }
            else if (string.IsNullOrWhiteSpace(value))
            {
                return;
            }
            else if (propertyType == typeof(DateTime))
            {
                obj.SetValue(instance, DateTime.ParseExact(value, format, CultureInfo.InvariantCulture));
            }
            else if (propertyType == typeof(DateTime?))
            {
                obj.SetValue(instance, DateTime.ParseExact(value, format, CultureInfo.InvariantCulture));
            }
            else if (propertyType == typeof(decimal))
            {
                obj.SetValue(instance, decimal.Parse(value));
            }
            else if (propertyType == typeof(decimal?))
            {
                obj.SetValue(instance, decimal.Parse(value));
            }
            else if (propertyType == typeof(double))
            {
                obj.SetValue(instance, double.Parse(value));
            }
            else if (propertyType == typeof(double?))
            {
                obj.SetValue(instance, double.Parse(value));
            }
            else if (propertyType == typeof(float))
            {
                obj.SetValue(instance, float.Parse(value));
            }
            else if (propertyType == typeof(float?))
            {
                obj.SetValue(instance, float.Parse(value));
            }
            else if (propertyType == typeof(long))
            {
                obj.SetValue(instance, long.Parse(value));
            }
            else if (propertyType == typeof(long?))
            {
                obj.SetValue(instance, long.Parse(value));
            }
            else if (propertyType == typeof(int))
            {
                obj.SetValue(instance, int.Parse(value));
            }
            else if (propertyType == typeof(int?))
            {
                obj.SetValue(instance, int.Parse(value));
            }
            else if (propertyType == typeof(short))
            {
                obj.SetValue(instance, short.Parse(value));
            }
            else if (propertyType == typeof(short?))
            {
                obj.SetValue(instance, short.Parse(value));
            }
            else if (propertyType == typeof(byte))
            {
                obj.SetValue(instance, byte.Parse(value));
            }
            else if (propertyType == typeof(byte?))
            {
                obj.SetValue(instance, byte.Parse(value));
            }
            else if (propertyType == typeof(bool))
            {
                obj.SetValue(instance, bool.Parse(value));
            }
            else if (propertyType == typeof(bool?))
            {
                obj.SetValue(instance, bool.Parse(value));
            }
            else if (propertyType.BaseType != null && propertyType.BaseType == typeof(Enum))
            {
                obj.SetValue(instance, FlagOperator.ConvertToEnum(value, propertyType));
            }
            else
            {
                throw new NotSupportedException($"PropertyType ({propertyType.FullName}) is not supported.");
            }
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

        public static string ToStringWithROCYear(this DateTime obj, in string format = "yyy/MM/dd")
        {
            return string.IsNullOrWhiteSpace(format) ? obj.ToString(Local.TWCulture) : obj.ToString(format, Local.TWCulture);
        }

        public static DateTime ParseROCYear(this string obj)
        {
            return DateTime.Parse(obj, Local.TWCulture);
        }

        public static string ToROCYear(this string obj, in DateTime date)
        {
            string yyyy = date.ToString("yyyy");
            string yyy = Local.TWCalendar.GetYear(date).ToString();
            string yy = date.ToString("yy");

            return obj.Replace("{yyyy}", yyyy).Replace("{yyy}", yyy).Replace("{yy}", yy);
        }

        public static string[] SplitFromCSV(this string obj)
        {
            string[] cells = obj.Split(Separator.CSV, StringSplitOptions.None);

            if (cells.Length > 0 && cells[0].StartsWith("\""))
            {
                cells[0] = cells[0].Substring(1);

                string last = cells[cells.Length - 1];
                cells[cells.Length - 1] = last.Substring(0, last.Length - 1);
            }

            return cells;
        }

        /// <summary>
        /// https://www.twse.com.tw/zh/holidaySchedule/holidaySchedule
        /// </summary>
        /// <param name="obj"></param>
        /// <param name="lines"></param>
        /// <param name="yyyy"></param>
        /// <param name="keywords1"></param>
        /// <param name="keywords2"></param>
        public static void LoadHolidays(this IDictionary<DateTime, string> obj, in IList<string> lines, in int yyyy, in IList<string> keywords1, in IEnumerable<string> keywords2)
        {
            string[] separators1 = new string[] { keywords1[0], keywords1[1] };

            foreach (string line in lines)
            {
                string[] cells = line.SplitFromCSV();

                if (cells.Length < 4)
                {
                    continue;
                }
                else if (string.IsNullOrWhiteSpace(keywords2.FirstOrDefault(x => cells[3].LastIndexOf(x) >= 0)))
                {
                    continue;
                }

                foreach (string cell in cells)
                {
                    if (cell.Contains(keywords1[0]) && cell.EndsWith(keywords1[1]))
                    {
                        string[] _MMdd = cell.Split(separators1, StringSplitOptions.RemoveEmptyEntries);
                        DateTime holiday = new DateTime(yyyy, int.Parse(_MMdd[0]), int.Parse(_MMdd[1]));
                        obj[holiday] = $"{holiday.DayOfWeek}, {string.Join(", ", cells)}";
                    }
                }
            }
        }

        public static void LoadHolidays(this IDictionary<DateTime, string> obj, in string path, in Encoding encoding, in int yyyy, in IList<string> keywords1, in IEnumerable<string> keywords2)
        {
            obj.LoadHolidays(File.ReadAllLines(path, encoding), yyyy, keywords1, keywords2);
        }

        public static T LoadHolidays<T>(this string obj, in Encoding encoding, in int yyyy, in IList<string> keywords1, in IEnumerable<string> keywords2) where T : IDictionary<DateTime, string>, new()
        {
            T dic = new T();
            dic.LoadHolidays(obj, encoding, yyyy, keywords1, keywords2);

            return dic;
        }

        /// <summary>
        /// 取活動日
        /// </summary>
        /// <param name="obj">指定年月</param>
        /// <param name="dowCount">星期幾計數，大於0正著數，小於0反著數</param>
        /// <param name="dow">星期幾</param>
        /// <returns></returns>
        public static DateTime GetActivityDate(this DateTime obj, in int dowCount, in DayOfWeek dow)
        {
            if (dowCount > 0)
            {
                DateTime date = new DateTime(obj.Year, obj.Month, 1).AddDays(-1);
                int targetWK = dowCount;
                int cnt = 0;

                for (int i = 0; i < 31; ++i)
                {
                    date = date.AddDays(1);

                    if (date.DayOfWeek == dow)
                    {
                        ++cnt;

                        if (cnt == targetWK)
                        {
                            return date;
                        }
                    }
                }
            }
            else if (dowCount < 0)
            {
                DateTime date = new DateTime(obj.Year, obj.Month, 1).AddMonths(1);
                int targetWK = dowCount * -1;
                int cnt = 0;

                for (int i = 0; i < 31; ++i)
                {
                    date = date.AddDays(-1);

                    if (date.DayOfWeek == dow)
                    {
                        ++cnt;

                        if (cnt == targetWK)
                        {
                            return date;
                        }
                    }
                }
            }

            return DateTime.MaxValue;
        }
    }
}
