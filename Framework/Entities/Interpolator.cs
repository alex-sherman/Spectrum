using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Spectrum.Framework.Entities
{
    class Interpolator
    {
        private float period;
        private float periodRemaining;
        private object target;
        Func<float, object, object, object> interpolator;
        public Interpolator(Func<float, object, object, object> interpolator)
        {
            this.interpolator = interpolator;
        }
        public void BeginInterpolate(float period, object target)
        {
            this.target = target;
            this.period = period;
            periodRemaining = period;
        }
        public object Update(float elapsed, object currentValue)
        {
            if (periodRemaining > 0)
            {
                periodRemaining -= elapsed;
                return interpolator(1 - periodRemaining / period, currentValue, target);
            }
            return null;
        }
    }
}
