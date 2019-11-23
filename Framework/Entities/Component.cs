using Spectrum.Framework.Input;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Spectrum.Framework.Entities
{
    public interface IUpdate
    {
        void Update(float dt);
    }
    public interface IFullUpdate : IUpdate
    {
        void PreStep(float step);
        void PostStep(float step);
    }
    public class Component
    {
        public Entity E { get; set; }
        public virtual void Initialize(Entity e) { E = e; }
    }
    public class Component<T> : Component
    {
        public T P { get; set; }
        public override void Initialize(Entity e)
        {
            base.Initialize(e);
            if (!(e is T p)) throw new InvalidCastException($"Cannot convert from {e.GetType()} to {typeof(T)}");
            P = p;
        }
    }
}
