using SkyTrack.Annotations;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace SkyTrack.LaborSetup.ViewModels.AppState
{
    class ObservableState : INotifyPropertyChanged, ICloneable
    {
        private bool _isLoading;
        public Dictionary<string, Action<object>> EventHandlers;

        public ObservableState(Dictionary<string, Action<object>> eventHandlers, Dictionary<string, Action> commandHandlers=null)
        {
            EventHandlers = eventHandlers;
            _isLoading = false;
        }

        public bool IsLoading
        {
            get { return _isLoading; }
            set
            {
                if (_isLoading == value)
                    return;
                _isLoading = value;
                OnPropertyChanged();
            }
        }

        public void Copy()
        {
            IsLoading = true;

            IEnumerable<PropertyInfo> properties =
                from property in this.GetType().GetProperties()
                let orderAttribute = property.GetCustomAttributes(typeof(OrderAttribute), false).SingleOrDefault() as OrderAttribute
                orderby orderAttribute.Order
                select property;

            foreach (PropertyInfo prop in properties)
            {
                PropertyInfo currentProp = this.GetType().GetProperty(prop.Name);
                if (currentProp.PropertyType.Name != "ObservableCollection`1")
                {
                    currentProp.SetValue(this, prop.GetValue(this), null);
                }
                else 
                {
                    currentProp.SetValue(this, new ObservableCollection<object>((ObservableCollection<object>)prop.GetValue(this)), null);
                }
            }

            IsLoading = false;
        }

        public event PropertyChangedEventHandler PropertyChanged;

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChangedEventHandler handler = PropertyChanged;
            if (handler != null) handler(this, new PropertyChangedEventArgs(propertyName));

            if (EventHandlers.ContainsKey(propertyName))
            {
                EventHandlers[propertyName](propertyName);
            }
        }

        public object Clone()
        {
            return MemberwiseClone();
        }
    }

    [AttributeUsage(AttributeTargets.Property, Inherited = false, AllowMultiple = false)]
    public sealed class OrderAttribute : Attribute
    {
        private readonly int order_;
        public OrderAttribute([CallerLineNumber]int order = 0)
        {
            order_ = order;
        }

        public int Order { get { return order_; } }
    }
}
