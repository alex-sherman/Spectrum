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
using Spectrum.Framework.Graphics;
using Replicate;

namespace Spectrum.Framework.Entities
{
    [LoadableType]
    public class Entity : IReplicatable
    {
        #region Replication
        private float replicateCounter = 0;
        public InitData InitData { get; set; }
        public ReplicationData ReplicationData { get; set; }
        public bool AllowReplicate;
        public bool AutoReplicate;
        public bool IsLocal;
        public bool IsInitialized;
        public bool CanReplicate { get { return AllowReplicate && IsLocal; } }
        #endregion
        public List<Component> Components = new List<Component>();
        public void AddComponent(Component component) => Components.Add(component);
        public T Data<T>(string key)
        {
            if (InitData.Data.TryGetValue(key, out Primitive output) && output.Object is T tOutput)
                return tOutput;
            return default(T);
        }
        public T Data<T>(string key, T ifMissing)
        {
            if(!InitData.Data.ContainsKey(key) || !(InitData.Data[key].Object is T))
                InitData.Data[key] = new Primitive(ifMissing);
            return (T)InitData.Data[key].Object;
        }
        public Guid ID;
        /// <summary>
        /// Gets automatically set when constructing with InitData
        /// </summary>
        public string TypeName { get; set; }
        public EntityMessageHandler SendMessageCallback;
        public NetID OwnerGuid;
        public EntityManager Manager;
        private bool replicateNextUpdate = false;

        public int UpdateOrder { get; protected set; }
        public int DrawOrder { get; protected set; }
        public bool Enabled { get; set; }
        public virtual bool DrawEnabled { get; set; }
        public bool Destroying { get; private set; }
        public event Action OnDestroy;

        public Entity()
        {
            Enabled = true;
            DrawEnabled = true;
            AllowReplicate = true;
        }

        public virtual void Initialize()
        {
            IsLocal = true;
            // TODO: Fix
            //IsLocal = OwnerGuid == SpectrumGame.Game.MP.ID;
            foreach (var comp in Components)
                comp.Initialize(this);
            IsInitialized = true;
        }

        public virtual void Reload()
        {
            InitData.Apply(this);
        }

        [ReplicateRPC]
        public virtual void Destroy()
        {
            RPC("Destroy");
            Enabled = false;
            Destroying = true;
        }

        internal void CallOnDestroy() => OnDestroy?.Invoke();

        public void RPC(string method, params object[] args)
        {
            if (CanReplicate)
            {
                Manager?.SendFunctionReplication(this, method, args);
            }
        }

        public void Replicate()
        {
            replicateNextUpdate = true;
        }

        public virtual void Update(float dt)
        {
            foreach (var comp in Components)
            {
                if (comp is IUpdate compUpdateable)
                {
                    compUpdateable.Update(dt);
                }
            }
            if (AllowReplicate)
                ReplicationData?.Interpolate(dt);
            if (CanReplicate)
            {
                if (replicateCounter > 0)
                    replicateCounter -= dt;

                if (replicateNextUpdate || (replicateCounter <= 0 && AutoReplicate))
                {
                    replicateNextUpdate = false;
                    replicateCounter = ReplicationData.DefaultReplicationPeriod;
                    Manager.SendEntityReplication(this, default(NetID));
                }
            }
        }
        public virtual void DisabledUpdate(float time) { }
        public virtual void Draw(float gameTime) { }
        public virtual void TickTenth() { }
        public virtual void TickOne() { }
    }
}
