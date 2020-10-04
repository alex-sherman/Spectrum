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
    [ReplicateType]
    public class Entity : IReplicatable
    {
        #region Replication
        private float replicateCounter = 0;
        [ReplicateIgnore]
        public InitData InitData { get; set; }
        [ReplicateIgnore]
        public ReplicationData ReplicationData { get; set; }
        [ReplicateIgnore]
        public bool AllowReplicate;
        [ReplicateIgnore]
        public bool AutoReplicate;
        [ReplicateIgnore]
        public bool IsLocal;
        [ReplicateIgnore]
        public bool IsInitialized;
        [ReplicateIgnore]
        public bool CanReplicate { get { return AllowReplicate && IsLocal; } }
        #endregion
        [ReplicateIgnore]
        public List<Component> Components = new List<Component>();
        public void AddComponent(Component component)
        {
            if (component != null) Components.Add(component);
        }
        public T GetComponent<T>()
        {
            return Components.Where(c => c is T).Cast<T>().FirstOrDefault();
        }
        public T Data<T>(string key)
        {
            if (InitData.Data.TryGetValue(key, out Primitive output) && output.Object is T tOutput)
                return tOutput;
            return default(T);
        }
        public T Data<T>(string key, T ifMissing)
        {
            if (!InitData.Data.ContainsKey(key) || !(InitData.Data[key].Object is T))
                InitData.Data[key] = new Primitive(ifMissing);
            return (T)InitData.Data[key].Object;
        }
        public Guid ID;
        /// <summary>
        /// Gets automatically set when constructing with InitData
        /// </summary>
        public string TypeName { get; set; }
        [ReplicateIgnore]
        public EntityMessageHandler SendMessageCallback;
        public NetID OwnerGuid;
        [ReplicateIgnore]
        public EntityManager Manager;
        private bool replicateNextUpdate = false;

        [ReplicateIgnore]
        public int UpdateOrder { get; protected set; }
        [ReplicateIgnore]
        public int DrawOrder { get; protected set; }
        public bool Enabled { get; set; }
        public virtual bool DrawEnabled { get; set; }
        [ReplicateIgnore]
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
                //Manager?.SendFunctionReplication(this, method, args);
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
