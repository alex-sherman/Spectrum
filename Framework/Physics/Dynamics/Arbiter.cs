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

using Spectrum.Framework.Physics.Dynamics;
using Spectrum.Framework.Physics.LinearMath;
using Spectrum.Framework.Physics.Collision.Shapes;
using System.Diagnostics;
using Microsoft.Xna.Framework;
using Spectrum.Framework.Physics.Collision;
using Spectrum.Framework.Entities;
using System.Linq;
#endregion

namespace Spectrum.Framework.Physics.Dynamics
{




    /// <summary>
    /// Represents a list of contacts. Every ContactList 
    /// has a maximum of four contacts.
    /// </summary>
    public class ContactList : List<Contact>
    {

        public ContactList() : base(4) { }


        #region TODO: Write an implementation which only has 4 elements.

        //Contact[] contacts = new Contact[4];
        //int count = 0;

        //public void Add(Contact contact)
        //{
        //    contacts[count] = contact;
        //    count++;
        //}

        //public int Count { get { return count; } }

        //public Contact this[int index]
        //{
        //    get
        //    {
        //        return contacts[index];
        //    }
        //}

        //public void RemoveAt(int index)
        //{
        //    if (index == 2)
        //    {
        //        contacts[2] = contacts[3];
        //    }
        //    else if (index == 1)
        //    {
        //        contacts[1] = contacts[2];
        //        contacts[2] = contacts[3];
        //    }
        //    else if (index == 0)
        //    {
        //        contacts[0] = contacts[1];
        //        contacts[1] = contacts[2];
        //        contacts[2] = contacts[3];
        //    }

        //    count--;
        //}

        //public void Clear()
        //{
        //    count = 0;
        //}
        #endregion
    }

    /// <summary>
    /// An arbiter holds all contact information of two bodies.
    /// The contacts are stored in the ContactList. There is a maximum
    /// of four contacts which can be added to an arbiter. The arbiter
    /// only keeps the best four contacts based on the area spanned by
    /// the contact points.
    /// </summary>
    public class Arbiter
    {
        public CollisionSystem system;
        /// <summary>
        /// The first body.
        /// </summary>
        public GameObject Body1;

        /// <summary>
        /// The second body.
        /// </summary>
        public GameObject Body2;

        /// <summary>
        /// The contact list containing all contacts of both bodies.
        /// </summary>
        public List<Contact> ContactList = new List<Contact>();

        /// <summary>
        /// </summary>
        public static ResourcePool<Arbiter> Pool = new ResourcePool<Arbiter>();

        /// <summary>
        /// Adds a contact to the arbiter (threadsafe). No more than four contacts 
        /// are stored in the contactList. When adding a new contact
        /// to the arbiter the existing are checked and the best are kept.
        /// </summary>
        /// <param name="point1">Point on body1. In world space.</param>
        /// <param name="point2">Point on body2. In world space.</param>
        /// <param name="normal">The normal pointing to body2.</param>
        /// <param name="penetration">The estimated penetration depth.</param>
        public Contact AddContact(Vector3 point, Vector3 normal, float penetration, 
            ContactSettings contactSettings)
        {
            Vector3 relPos1;
            Vector3.Subtract(ref point, ref Body1.position, out relPos1);

            Contact remove;

            lock (ContactList)
            {
                Contact contact = Contact.Pool.GetNew();
                contact.Initialize(system, Body1, Body2, ref point, ref normal, penetration, true, contactSettings);
                ContactList.Add(contact);
                if (this.ContactList.Count == 5)
                {
                    remove = FindWorstContact();
                    ContactList.Remove(remove);
                    if (remove == contact)
                        return null;
                    return contact;
                }
                return contact;
            }
        }

        private float ContactScore(Contact test)
        {
            return test.Penetration - (float)Math.Pow(test.slip, 0.5);
            //return contactList.Sum((other) => other == test ? 0 : (test.Position1 - other.Position1).Length());
        }

        private Contact FindWorstContact()
        {
            var testList = ContactList
                .OrderByDescending((test) => test.Penetration)
                .Skip(1)
                .OrderBy(ContactScore).ToList();
            return testList.First();
            //return contactList.OrderBy(ContactScore).First();
        }

        private void ReplaceContact(ref Vector3 point, ref Vector3 n, float p, int index,
            ContactSettings contactSettings)
        {
            Contact contact = ContactList[index];

            Debug.Assert(Body1 == contact.body1, "Body1 and Body2 not consistent.");

            contact.Initialize(system, Body1, Body2, ref point, ref n, p, false, contactSettings);

        }

        private int GetCacheEntry(ref Vector3 realRelPos1, float contactBreakThreshold)
        {
            float shortestDist = contactBreakThreshold * contactBreakThreshold;
            int size = ContactList.Count;
            int nearestPoint = -1;
            for (int i = 0; i < size; i++)
            {
                Vector3 diffA; Vector3.Subtract(ref ContactList[i].relativePos1,ref realRelPos1,out diffA);
                float distToManiPoint = diffA.LengthSquared();
                if (distToManiPoint < shortestDist)
                {
                    shortestDist = distToManiPoint;
                    nearestPoint = i;
                }
            }
            return nearestPoint;
        }
    }
}
