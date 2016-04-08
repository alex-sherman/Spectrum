using ProtoBuf;
using Spectrum.Framework.Entities;
using Spectrum.Framework.Network.Surrogates;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Spectrum.Framework.Network.Surrogates
{
    [ProtoContract]
    public class EntitySurrogate<T> where T : Entity
    {
        [ProtoMember(1)]
        Guid id;
        public static implicit operator EntitySurrogate<T>(T entity)
        {
            if (entity == null) return null;
            return new EntitySurrogate<T>() { id = entity.ID };
        }
        public static implicit operator T(EntitySurrogate<T> entity)
        {
            if (entity == null) return null;
            return SpectrumGame.Game.EntityManager.Find(entity.id) as T;
        }
    }
}
