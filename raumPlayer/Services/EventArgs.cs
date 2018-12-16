using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace raumPlayer.Services
{
    public class EventArgs<T> : EventArgs
    {
        public bool? Canceled { get; set; }
        public bool? Handled { get; set; }

        public EventArgs(T value)
        {
            Value = value;
            Canceled = null;
            Handled = null;
        }
        public T Value { get; private set; }
    }
}
