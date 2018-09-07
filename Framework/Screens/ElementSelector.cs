using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Spectrum.Framework.Screens
{
    public class Selector
    {
        // TODO: Implement properly?
        public static Selector Parse(string selector)
        {
            return new Selector(e => e?.HasTag(selector) ?? false);
        }
        private Func<Element, bool> _selector;
        public Selector(Func<Element, bool> selector) { _selector = selector; }
        public static implicit operator Selector(string selector) => Parse(selector);
        public static Selector operator &(Selector a, Selector b) => new Selector((e) => a.Matches(e) && b.Matches(e));
        public static Selector operator |(Selector a, Selector b) => new Selector((e) => a.Matches(e) || b.Matches(e));
        public static Selector Parent(Selector selector, bool recursive = false)
        {
            bool parent(Element e) => selector.Matches(e.Parent) || (recursive && (e.Parent != null && parent(e.Parent)));
            return new Selector(parent);
        }
        public virtual bool Matches(Element element)
        {
            return _selector(element);
        }
    }
}
