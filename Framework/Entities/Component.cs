using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Spectrum.Framework.Entities
{
    public interface IUpdateable
    {
        void Update(float dt);
    }
    public interface IFullUpdateable
    {
        void PreStep(float step);
        void PostStep(float step);
    }
    public class Component
    {
        public Entity Entity { get; set; }
    }
}
