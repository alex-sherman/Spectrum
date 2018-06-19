using Spectrum.Framework.Entities;
using Spectrum.Framework.Network.Surrogates;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Spectrum.Framework.Network
{
    public interface IReplicatable
    {
        InitData InitData { get; set; }
        ReplicationData ReplicationData { get; set; }
    }
    public class ReplicationData
    {
        public const float DefaultReplicationPeriod = .2f;
        public TypeData TypeData { get; private set; }
        public IReplicatable Replicated { get; private set; }
        public ReplicationData(TypeData typeData, IReplicatable replicated)
        {
            TypeData = typeData;
            Replicated = replicated;
        }
        private Dictionary<string, Interpolator> interpolators = new Dictionary<string, Interpolator>();

        public void SetInterpolator(string attributeName, Func<float, object, object, object> interpolator)
        {
            interpolators[attributeName] = new Interpolator(interpolator);
        }

        public void HandleRPC(string name, object[] args)
        {
            if (TypeData.ReplicatedMethods.Contains(name))
                TypeData.Call(Replicated, name, args);
        }

        public virtual NetMessage WriteReplicationData(NetMessage output)
        {
            Primitive[] fields = TypeData.ReplicatedMemebers.ToList().ConvertAll(x => new Primitive(TypeData.Get(Replicated, x))).ToArray();
            output.Write(fields);
            return output;
        }
        public virtual void ReadReplicationData(NetMessage input)
        {
            Primitive[] fields = input.Read<Primitive[]>();
            var properties = TypeData.ReplicatedMemebers.ToList();
            for (int i = 0; i < fields.Count(); i++)
            {
                var replicate = properties[i];
                if (interpolators.ContainsKey(replicate))
                    interpolators[replicate].BeginInterpolate(DefaultReplicationPeriod * 2, fields[i].Object);
                else
                    TypeData.Set(Replicated, replicate, fields[i].Object);
            }
        }
        public void Interpolate(float dt)
        {
            foreach (var interpolator in interpolators)
            {
                object value = interpolator.Value.Update(dt, TypeData.Get(Replicated, interpolator.Key));
                if (value != null)
                    TypeData.Set(Replicated, interpolator.Key, value);
            }
        }
    }
}
