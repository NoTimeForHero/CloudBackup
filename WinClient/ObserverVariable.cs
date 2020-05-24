using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WinClient
{
    class ObserverVariable<T>
    {
        protected T value;

        public ObserverVariable() {}

        public event Action<T> Changed;

        public T Value
        {
            get => value;
            set {
                this.value = value;
                Changed(value);
            }
        }

    }
}
