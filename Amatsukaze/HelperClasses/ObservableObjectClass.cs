using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel;
using System.Diagnostics;

namespace Amatsukaze.HelperClasses
{
    //This is a primarily a helper class that is to be inherited by all viewmodel classes to save the trouble of having to implement INotifyPropertyChanged EVERY SINGLE TIME....

    public abstract class ObservableObjectClass : INotifyPropertyChanged
    {
        //Event raised whenever a property is changed.
        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged(string propertyName)
        {
            this.VerifyPropertyName(propertyName);

            if (this.PropertyChanged != null)
            {
                var e = new PropertyChangedEventArgs(propertyName);
                PropertyChanged(this, e);
            }
        }

        [Conditional("DEBUG")]
        public virtual void VerifyPropertyName(string propertyName)
        {
            //Debug tool to check if the property name is actually valid

            if (TypeDescriptor.GetProperties(this)[propertyName] == null)
            {
                string msg = "Your property name: " + propertyName + " is SHIT!";
                Debug.Fail(msg);                
            }
        }               
    }
}
