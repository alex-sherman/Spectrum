using Replicate.MetaData;
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
        public TypeAccessor TypeData { get; private set; }
        public IReplicatable Replicated { get; private set; }
        public ReplicationData(TypeAccessor typeData, IReplicatable replicated)
        {
            TypeData = typeData;
            Replicated = replicated;
        }
        private Dictionary<string, Interpolator> interpolators = new Dictionary<string, Interpolator>();

        public void SetInterpolator<T>(string attributeName, Func<float, T, T, T> interpolator)
        {
            interpolators[attributeName] = new Interpolator<T>(interpolator);
        }

        public void HandleRPC(string name, object[] args)
        {
            if (TypeData.Methods.ContainsKey(name))
                TypeData.Methods[name].Invoke(Replicated, args);
        }

        public virtual NetMessage WriteReplicationData(NetMessage output)
        {
            Primitive[] fields = TypeData.Members.Values.ToList().ConvertAll(x => new Primitive(x.GetValue(Replicated))).ToArray();
            output.Write(fields);
            return output;
        }
        public virtual void ReadReplicationData(NetMessage input)
        {
            Primitive[] fields = input.Read<Primitive[]>();
            var properties = TypeData.Members.Values.ToList();
            for (int i = 0; i < fields.Count(); i++)
            {
                var replicate = properties[i];
                if (interpolators.ContainsKey(replicate.Info.Name))
                    interpolators[replicate.Info.Name].BeginInterpolate(DefaultReplicationPeriod * 2, fields[i].Object);
                else
                    replicate.SetValue(Replicated, fields[i].Object);
            }
        }
        public void Interpolate(float dt)
        {
            foreach (var interpolator in interpolators)
            {
                if (!interpolator.Value.NeedsUpdate)
                    continue;
                var member = TypeData[interpolator.Key];
                object value = interpolator.Value.Update(dt, member.GetValue(Replicated));
                if (value != null)
                    member.SetValue(Replicated, value);
            }
        }
    }
}
