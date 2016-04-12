using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using Spectrum.Framework.Physics;
using Microsoft.Xna.Framework;
using Spectrum.Framework.Network;
using Microsoft.Xna.Framework.Graphics;
using System.Reflection;
using Spectrum.Framework.Network.Surrogates;
using ProtoBuf;

namespace Spectrum.Framework.Entities
{
    /// <summary>
    /// Entities are just collections of components.
    /// They have more specific definitions as game objects which are specific sets of components.
    /// </summary>
    public class Entity : IDisposable
    {
        #region Replication
        const int StateReplicationMessage = 0;
        const int FunctionReplicationMessage = 1;
        /// <summary>
        /// The default replication period of entities in miliseconds
        /// </summary>
        public const int DefaultReplicationPeriod = 200;

        private Dictionary<string, MethodInfo> replicatedMethods = new Dictionary<string, MethodInfo>();
        private Dictionary<string, Interpolator> interpolators = new Dictionary<string, Interpolator>();

        protected int ReplicationPeriod = DefaultReplicationPeriod;
        private int replicateCounter = 0;
        #endregion

        public Guid ID;
        public TypeData TypeData { get; private set; }
        public bool AllowReplicate { get; set; }
        public bool AutoReplicate { get; set; }
        public bool IsLocal { get { return OwnerGuid == SpectrumGame.Game.MP.ID; } }
        public bool CanReplicate { get { return AllowReplicate && IsLocal; } }
        public EntityMessageHandler SendMessageCallback;
        public NetID OwnerGuid;
        public EntityData CreationData;
        public EntityManager Manager;
        private bool replicateNextUpdate = false;

        public bool Enabled { get; protected set; }
        public bool DrawEnabled { get; protected set; }
        public bool Disposing { get; private set; }

        public Entity()
        {
            Enabled = true;
            DrawEnabled = true;
            AllowReplicate = true;
            TypeData = TypeHelper.Types.GetData(this.GetType().Name);
            foreach (MethodInfo method in this.GetType().GetMethods())
            {
                if (method.GetCustomAttributes(true).ToList().Any(x => x is ReplicateAttribute))
                {
                    replicatedMethods[method.Name] = method;
                }
            }
        }

        public virtual void Initialize() { }

        [Replicate]
        public virtual void Dispose()
        {
            RPC("Dispose");
            Enabled = false;
            Disposing = true;
        }

        public void SetInterpolator(string attributeName, Func<float, object, object, object> interpolator)
        {
            interpolators[attributeName] = new Interpolator(interpolator);
        }

        protected virtual void getData(NetMessage output)
        {
            Primitive[] fields = TypeData.ReplicatedMemebers.ToList().ConvertAll(x => new Primitive(TypeData.Get(this, x))).ToArray();
            output.Write(fields);
        }
        protected virtual void setData(NetMessage input)
        {
            Primitive[] fields = input.Read<Primitive[]>();
            var properties = TypeData.ReplicatedMemebers.ToList();
            for (int i = 0; i < fields.Count(); i++)
            {
                var replicate = properties[i];
                if (interpolators.ContainsKey(replicate))
                    interpolators[replicate].BeginInterpolate(ReplicationPeriod * 2, fields[i].Object);
                else
                    TypeData.Set(this, replicate, fields[i].Object);
            }
        }

        public void SendMessage(NetID peer, int type, NetMessage message)
        {
            if (SendMessageCallback != null)
            {
                NetMessage toSend = new NetMessage();
                toSend.Write(type);
                toSend.Write(message);
                SendMessageCallback(peer, this, toSend);
            }
        }
        public virtual void HandleMessage(NetID peer, int type, NetMessage message)
        {
            if (type == StateReplicationMessage)
            {
                setData(message);
            }
            else if (type == FunctionReplicationMessage)
            {
                string method = message.Read<string>();
                Primitive[] args = message.Read<Primitive[]>();
                replicatedMethods[method].Invoke(this, args.Select(prim => prim.Object).ToArray());
            }
            else
            {
                DebugPrinter.print("Received invalid message type " + type + " for entity type " + this.GetType().Name);
            }
        }

        public void RPC(string method, params object[] args)
        {
            if (CanReplicate)
            {
                NetMessage replicationMessage = new NetMessage();
                replicationMessage.Write(method);
                replicationMessage.Write(args.Select(obj => new Primitive(obj)).ToArray());
                SendMessage(default(NetID), FunctionReplicationMessage, replicationMessage);
            }
        }
        private void _replicate()
        {
            replicateCounter = ReplicationPeriod;
            NetMessage replicationMessage = new NetMessage();
            getData(replicationMessage);
            SendMessage(default(NetID), StateReplicationMessage, replicationMessage);
        }
        public void Replicate(bool force = false)
        {
            if ((force || replicateCounter <= 0))
            {
                replicateNextUpdate = true;
            }
        }

        public int UpdateOrder { get; protected set; }
        public virtual void Update(GameTime gameTime)
        {
            foreach (var interpolator in interpolators)
            {
                object value = interpolator.Value.Update(gameTime.ElapsedGameTime.Milliseconds, TypeData.Get(this, interpolator.Key));
                if (value != null)
                    TypeData.Set(this, interpolator.Key, value);
            }
            if (replicateCounter > 0)
                replicateCounter -= gameTime.ElapsedGameTime.Milliseconds;

            if (replicateCounter <= 0 && (replicateNextUpdate || AutoReplicate))
            {
                replicateCounter = ReplicationPeriod;
                _replicate();
            }
        }
        public virtual void DisabledUpdate(GameTime time) { }
        public virtual void Draw(GameTime gameTime, SpriteBatch spriteBatch) { }
        public virtual void TickTenth() { }
        public virtual void TickOne() { }
    }
}
