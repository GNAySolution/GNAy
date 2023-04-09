using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace GNAy.Tools.NET48.Models
{
    [Serializable]
    public class NotifyPropertyChanged : INotifyPropertyChanged
    {
        /// <summary>
        /// <para>https://stackoverflow.com/questions/8879426/serializationexception-when-serializing-instance-of-a-class-which-implements-ino</para>
        /// <para>https://docs.microsoft.com/en-us/dotnet/api/system.nonserializedattribute?redirectedfrom=MSDN&view=net-6.0</para>
        /// </summary>
        [field: NonSerialized()]
        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged(in PropertyChangedEventArgs e)
        {
            PropertyChanged?.Invoke(this, e);
        }

        protected virtual void OnPropertyChanged([CallerMemberName] in string propertyName = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        /// <summary>
        /// https://stackoverflow.com/questions/1315621/implementing-inotifypropertychanged-does-a-better-way-exist
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="field"></param>
        /// <param name="value"></param>
        /// <param name="propertyName"></param>
        /// <returns></returns>
        protected virtual bool OnPropertyChanged<T>(ref T field, in T value, [CallerMemberName] in string propertyName = "")
        {
            if (EqualityComparer<T>.Default.Equals(field, value))
            {
                return false;
            }

            field = value;
            OnPropertyChanged(propertyName);

            return true;
        }

        protected virtual bool OnPropertiesChanged<T>(ref T field, in T value, in string propertyName, params string[] otherNames)
        {
            if (EqualityComparer<T>.Default.Equals(field, value))
            {
                return false;
            }

            field = value;
            OnPropertyChanged(propertyName);

            foreach (string name in otherNames)
            {
                OnPropertyChanged(name);
            }

            return true;
        }
    }
}
