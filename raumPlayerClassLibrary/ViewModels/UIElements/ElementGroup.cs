using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace roomZone.Models
{
    public class ElementGroup : IGrouping<string, Element>
    {
        private ObservableCollection<Element> elements;

        public ElementGroup(string key, IEnumerable<Element> items)
        {
            Key = key;
            elements = new ObservableCollection<Element>(items);
        }

        public string Key { get; }
        public ObservableCollection<Element> Elements { get { return elements; } }

        public IEnumerator<Element> GetEnumerator() => elements.GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => elements.GetEnumerator();
    }
}
