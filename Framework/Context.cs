using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Spectrum.Framework
{
    public class Context<T> : Context
    {
        private Context() { }
        private T Previous;
        public override void Dispose()
        {
            Current = Previous;
            base.Dispose();
        }
        private static AsyncLocal<T> current = new AsyncLocal<T>();
        public static T Current { get => current.Value; private set => current.Value = value; }
        public static Context<T> Create(T value)
        {
            var result = new Context<T>() { Previous = Current };
            Current = value;
            return result;
        }
    }
    public class Context : IDisposable
    {
        private IDisposable next;
        public virtual void Dispose()
        {
            next?.Dispose();
        }
        public static Context<U> Create<U>(U value) => Context<U>.Create(value);
        public Context<U> Inject<U>(U value)
        {
            var result = Create(value);
            result.next = this;
            return result;
        }
        public IDisposable Include(IDisposable disposable)
        {
            if (next != null) throw new InvalidOperationException("Next is already set");
            next = disposable;
            return this;
        }
    }
}
