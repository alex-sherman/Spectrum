/* Copyright (C) <2009-2011> <Thorben Linneweber, Jitter Physics>
* 
*  This software is provided 'as-is', without any express or implied
*  warranty.  In no event will the authors be held liable for any damages
*  arising from the use of this software.
*
*  Permission is granted to anyone to use this software for any purpose,
*  including commercial applications, and to alter it and redistribute it
*  freely, subject to the following restrictions:
*
*  1. The origin of this software must not be misrepresented; you must not
*      claim that you wrote the original software. If you use this software
*      in a product, an acknowledgment in the product documentation would be
*      appreciated but is not required.
*  2. Altered source versions must be plainly marked as such, and must not be
*      misrepresented as being the original software.
*  3. This notice may not be removed or altered from any source distribution. 
*/

#region Using Statements
using System;
using System.Collections.Generic;
using System.Threading;

using Spectrum.Framework.Physics.Dynamics;
using Spectrum.Framework.Physics.LinearMath;
using Spectrum.Framework.Physics.Collision.Shapes;
using Spectrum.Framework.Entities;
#endregion

namespace Spectrum.Framework.Physics.Dynamics.Constraints
{

    public interface IConstraint
    {
        void PrepareForIteration(float timestep);
        void Iterate();

        GameObject Body1 { get; }

        /// <summary>
        /// Gets the second body. Can be null.
        /// </summary>
        GameObject Body2 { get; }
    }

    /// <summary>
    /// A constraints forces a body to behave in a specific way.
    /// </summary>
    public abstract class Constraint : IConstraint, IComparable<Constraint>
    {
        internal GameObject body1;
        internal GameObject body2;

        /// <summary>
        /// Gets the first body. Can be null.
        /// </summary>
        public GameObject Body1 { get { return body1; } }

        /// <summary>
        /// Gets the second body. Can be null.
        /// </summary>
        public GameObject Body2 { get { return body2; } }

        private static int instanceCount = 0;
        private int instance;

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="body1">The first body which should get constrained. Can be null.</param>
        /// <param name="body2">The second body which should get constrained. Can be null.</param>
        public Constraint(GameObject body1, GameObject body2)
        {
            this.body1 = body1;
            this.body2 = body2;

            instance = Interlocked.Increment(ref instanceCount);
        }

        /// <summary>
        /// Called once before iteration starts.
        /// </summary>
        /// <param name="timestep">The simulation timestep</param>
        public abstract void PrepareForIteration(float timestep);

        /// <summary>
        /// Iteratively solve this constraint.
        /// </summary>
        public abstract void Iterate();


        public int CompareTo(Constraint other)
        {
            if (other.instance < this.instance) return -1;
            else if (other.instance > this.instance) return 1;
            else return 0;
        }
    }
}
