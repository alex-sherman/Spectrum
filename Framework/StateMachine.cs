using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Spectrum.Framework
{
    public interface StateMachine
    {
        public abstract void ProcessEvent<TEvent>(TEvent e);
    }
    public class StateMachine<TState> : StateMachine
    {
        public delegate TState Handler(TState current, object e);
        public TState State { get; private set; }

        private Dictionary<Type, Handler> Defaults = new Dictionary<Type, Handler>();
        private Dictionary<(TState, Type), Handler> Handlers 
            = new Dictionary<(TState, Type), Handler>();

        public StateMachine(TState initial)
        {
            State = initial;
        }
        public void Add<TEvent>(Func<TState, TEvent, TState> handler)
        {
            Defaults[typeof(TEvent)] = (s, e) => handler(s, (TEvent)e);
        }
        public void Add<TEvent>(TState state, Func<TEvent, TState> handler)
        {
            Handlers[(state, typeof(TEvent))] = (s, e) => handler((TEvent)e);
        }
        public TState ProcessEvent<TEvent>(TEvent e)
        {
            if (Handlers.TryGetValue((State, typeof(TEvent)), out var handler)
                || Defaults.TryGetValue(typeof(TEvent), out handler))
                return State = handler(State, e);
            return State;
        }

        void StateMachine.ProcessEvent<TEvent>(TEvent e)
        {
            ProcessEvent(e);
        }
    }
}
