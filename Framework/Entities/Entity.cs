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
    public class Entity : IDisposable, IReplicatable
    {
        #region Replication
        const int StateReplicationMessage = 0;
        const int FunctionReplicationMessage = 1;
        
        private float replicateCounter = 0;
        #endregion

        public Guid ID;
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
        public bool Enabled { get; set; }
        public bool Disposing { get; private set; }

        public Entity()
        {
            Enabled = true;
            AllowReplicate = true;
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

        public void Replicate()
        {
            replicateNextUpdate = true;
        }

        public virtual void Update(GameTime gameTime)
        {
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
        public virtual void Draw(GameTime gameTime, SpriteBatch spriteBatch) { }
        public virtual List<RenderTask> GetRenderTasks(RenderPhaseInfo phase) { return null; }
        public virtual void TickTenth() { }
        public virtual void TickOne() { }
    }
}
