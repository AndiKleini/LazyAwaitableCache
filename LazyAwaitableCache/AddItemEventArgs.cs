using System;
using System.Collections.Generic;
using System.Text;

namespace LazyAwaitableCache
{
    public class AddItemEventArgs : EventArgs
    {
        public AddItemEventArgs(string key, TimeSpan expires)
        {
            this.Key = key;
            this.Expires = expires;
        }
        public string Key { get; set; }
        public TimeSpan Expires { get; set; }
    }
}
