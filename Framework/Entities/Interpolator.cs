using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Spectrum.Framework.Entities
{
    public class Interpolator<T> : Interpolator
    {
        Func<float, T, T, T> interpolator;
        public Interpolator(Func<float, T, T, T> interpolator) => this.interpolator = interpolator;
        public override object GetValue(float weight, object currentValue, object target)
        {
            return interpolator(weight, (T)currentValue, (T)target);
        }
    }
    public abstract class Interpolator
    {
        private float period;
        private float periodRemaining;
        private object target;
        public bool NeedsUpdate => periodRemaining > 0;
        public void BeginInterpolate(float period, object target)
        {
            this.target = target;
            this.period = period;
            periodRemaining = period;
        }
        public virtual object Update(float elapsed, object currentValue)
        {
            if (periodRemaining > 0)
            {
                periodRemaining -= elapsed;
                return GetValue(1 - periodRemaining / period, currentValue, target);
            }
            return null;
        }
        public abstract object GetValue(float weight, object currentValue, object target);
    }
}
