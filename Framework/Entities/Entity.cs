﻿using System;
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
    public class Entity : IDisposable, IReplicatable
    {
        #region Replication
        const int StateReplicationMessage = 0;
        const int FunctionReplicationMessage = 1;
        
        private float replicateCounter = 0;
        #endregion

        public Guid ID;
        public TypeData TypeData { get; private set; }
        /// <summary>
        /// Gets automatically set when constructing with InitData
        /// </summary>
        public string TypeName { get; set; }
        public bool AllowReplicate { get; set; }
        public bool AutoReplicate { get; set; }
        public bool IsLocal { get { return OwnerGuid == SpectrumGame.Game.MP.ID; } }
        public bool CanReplicate { get { return AllowReplicate && IsLocal; } }
        public EntityMessageHandler SendMessageCallback;
        public NetID OwnerGuid;
        public ReplicationData ReplicationData { get; set; }
        public EntityManager Manager;
        private bool replicateNextUpdate = false;

        public int UpdateOrder { get; protected set; }
        public int DrawOrder { get; protected set; }
        public bool Enabled { get; protected set; }
        public bool DrawEnabled { get; protected set; }
        public bool Disposing { get; private set; }

        public Entity()
        {
            Enabled = true;
            DrawEnabled = true;
            AllowReplicate = true;
            TypeData = TypeHelper.Types.GetData(this.GetType().Name);
        }

        public virtual void Initialize() { }

        [Replicate]
        public virtual void Dispose()
        {
            RPC("Dispose");
            Enabled = false;
            Disposing = true;
        }

        public void RPC(string method, params object[] args)
        {
            if (CanReplicate)
            {
                Manager?.SendFunctionReplication(this, method, args);
            }
        }

        public virtual void Update(GameTime gameTime)
        {
            ReplicationData.Interpolate(gameTime.DT());
            if (CanReplicate)
            {
                if (replicateCounter > 0)
                    replicateCounter -= gameTime.DT();

                if (replicateCounter <= 0 && (replicateNextUpdate || AutoReplicate))
                {
                    replicateCounter = ReplicationData.DefaultReplicationPeriod;
                    Manager.SendEntityReplication(this, default(NetID));
                }
            }
        }
        public virtual void DisabledUpdate(GameTime time) { }
        public virtual void Draw(GameTime gameTime, SpriteBatch spriteBatch) { }
        public virtual void TickTenth() { }
        public virtual void TickOne() { }
    }
}
