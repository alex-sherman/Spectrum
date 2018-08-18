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

namespace Spectrum.Framework.Entities
{
    [LoadableType]
    public class Entity : IReplicatable
    {
        #region Replication
        const int StateReplicationMessage = 0;
        const int FunctionReplicationMessage = 1;
        private float replicateCounter = 0;
        public InitData InitData { get; set; }
        public ReplicationData ReplicationData { get; set; }
        public bool AllowReplicate;
        public bool AutoReplicate;
        public bool IsLocal;
        public bool IsInitialized;
        public bool CanReplicate { get { return AllowReplicate && IsLocal; } }
        #endregion
        public DefaultDict<string, object> Data = new DefaultDict<string, object>();
        public T Get<T>(string key)
        {
            if (Data.TryGetValue(key, out object output) && output is T tOutput)
                return tOutput;
            return default(T);
        }
        public T Get<T>(string key, T ifMissing)
        {
            if(!Data.ContainsKey(key) || !(Data[key] is T))
                Data[key] = ifMissing;
            return (T)Data[key];
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
        public bool DrawEnabled { get; set; }
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
            IsLocal = OwnerGuid == SpectrumGame.Game.MP.ID;
            IsInitialized = true;
        }

        [Replicate]
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

        public virtual void Update(GameTime gameTime)
        {
            if (AllowReplicate)
                ReplicationData?.Interpolate(gameTime.DT());
            if (CanReplicate)
            {
                if (replicateCounter > 0)
                    replicateCounter -= gameTime.DT();

                if (replicateNextUpdate || (replicateCounter <= 0 && AutoReplicate))
                {
                    replicateNextUpdate = false;
                    replicateCounter = ReplicationData.DefaultReplicationPeriod;
                    Manager.SendEntityReplication(this, default(NetID));
                }
            }
        }
        public virtual void DisabledUpdate(GameTime time) { }
        [Obsolete]
        public virtual void Draw(GameTime gameTime, SpriteBatch spriteBatch) { }
        public virtual void Draw(float gameTime) { }
        public virtual void TickTenth() { }
        public virtual void TickOne() { }
    }
}
