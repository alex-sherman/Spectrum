using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace Spectrum.Framework
{
    public static class FunctionalExtensions
    {
        public static object GetConstantValue(this Expression exp)
        {
            return Expression.Lambda<Func<object>>(Expression.Convert(exp, typeof(object))).Compile()();
        }
    }
}
