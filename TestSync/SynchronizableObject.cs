using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TestSync
{
    /// <summary>
    /// Interface for object which can synchronized between process
    /// </summary>
    interface ISynchronizePropertyChanged
    {
        event PropertyChangedEventHandler SynchronizeProperty;
    }

    /// <summary>
    /// Interface for Synchronizable object
    /// </summary>
    public abstract class SynchronizableObject : INotifyPropertyChanged, ISynchronizePropertyChanged
    {
        #region ISynchronizePropertyChanged Members
        public event PropertyChangedEventHandler SynchronizeProperty;

        protected virtual void InvokeSynchronizeProperty(string propName)
        {
            var e = SynchronizeProperty;
            if (e != null)
                e(this, new PropertyChangedEventArgs(propName));
        }
        #endregion

        #region INotifyPropertyChanged Members

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void InvokePropertyChanged(string propName)
        {
            var e = PropertyChanged;
            if (e != null)
                e(this, new PropertyChangedEventArgs(propName));
        }

        #endregion

        #region Interfaces for sync objects
        public abstract string Serialize();
        public abstract void Synchronize(string xml);
        #endregion
    }
}
