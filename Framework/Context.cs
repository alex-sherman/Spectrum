using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Spectrum.Framework
{
    public class Context<T> : IDisposable
    {
        private Context() { }
        private T Previous;
        private IDisposable next;
        public void Dispose()
        {
            Current = Previous;
            next?.Dispose();
        }
        public static T Current { get; private set; }
        public Context<U> Inject<U>(U value)
        {
            var result = Context.Create(value);
            result.next = this;
            return result;
        }
        public IDisposable Include(IDisposable disposable)
        {
            if (next != null) throw new InvalidOperationException("Next is already set");
            next = disposable;
            return this;
        }
        public static Context<T> Create(T value)
        {
            var result = new Context<T>() { Previous = Current };
            Current = value;
            return result;
        }
    }
    public static class Context
    {
        public static Context<U> Create<U>(U value) => Context<U>.Create(value);
    }
}
