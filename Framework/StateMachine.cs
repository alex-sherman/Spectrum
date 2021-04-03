using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Spectrum.Framework
{
    public class StateMachine
    {
        public delegate int Handler(int current, object e);
        public int State { get; private set; }

        private Dictionary<Type, Handler> Defaults = new Dictionary<Type, Handler>();
        private Dictionary<(int, Type), Handler> Handlers
            = new Dictionary<(int, Type), Handler>();

        public StateMachine(int initial)
        {
            State = initial;
        }
        public void Add<TEvent>(Func<int, TEvent, int> handler)
        {
            Defaults[typeof(TEvent)] = (s, e) => handler(s, (TEvent)e);
        }
        public void Add<TEvent>(int state, Func<TEvent, int> handler)
        {
            Handlers[(state, typeof(TEvent))] = (s, e) => handler((TEvent)e);
        }
        public int ProcessEvent<TEvent>(TEvent e)
        {
            if (Handlers.TryGetValue((State, typeof(TEvent)), out var handler)
                || Defaults.TryGetValue(typeof(TEvent), out handler))
                return State = handler(State, e);
            return State;
        }
    }
}
