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


namespace Spectrum.Framework.Entities
{
    /// <summary>
    /// Entities are just collections of components.
    /// They have more specific definitions as game objects which are specific sets of components.
    /// </summary>
    public class Entity : IDisposable
    {
        #region Replication
        private int StateReplicationMessage;
        private int FunctionReplicationMessage;


        private int messageTypeTracker = 0;

        private Dictionary<string, MethodInfo> replicatedMethods = new Dictionary<string, MethodInfo>();
        private List<PropertyInfo> replicatedProperties = new List<PropertyInfo>();

        //Replication period in ms
        protected int minReplicationPeriod = 200;
        private int replicateCounter = 0;
        protected int _numReplicatedFunctions;
        #endregion

        public Guid ID;
        public bool AllowReplicate { get; protected set; }
        public bool IsLocal { get { return OwnerGuid == SpectrumGame.Game.ID; } }
        public bool CanReplicate { get { return AllowReplicate && IsLocal; } }
        public EntityMessageHandler SendMessageCallback;
        public Guid OwnerGuid;
        public object[] creationArgs = new object[0];
        public EntityManager Manager;
        public Entity()
        {
            Enabled = true;
            AllowReplicate = true;
            StateReplicationMessage = AllocateMessageType();
            FunctionReplicationMessage = AllocateMessageType();
            foreach (PropertyInfo property in this.GetType().GetProperties())
            {
                if (property.GetCustomAttributes(true).ToList().ConvertAll(x => x is ReplicateAttribute).Any(x => x))
                {
                    replicatedProperties.Add(property);
                }
            }
            foreach (MethodInfo method in this.GetType().GetMethods())
            {
                if (method.GetCustomAttributes(true).ToList().ConvertAll(x => x is ReplicateAttribute).Any(x => x))
                {
                    replicatedMethods[method.Name] = method;
                }
            }
        }

        public virtual void Initialize() { }

        public virtual bool Enabled { get; private set; }
        public bool Disposing { get; private set; }

        [Replicate]
        public virtual void Dispose()
        {
            RPC("Dispose");
            Enabled = false;
            Disposing = true;
        }

        protected virtual void getData(NetMessage output)
        {
            object[] fields = replicatedProperties.ConvertAll(x => x.GetValue(this, null)).ToArray();
            output.WritePrimitiveArray(fields);
        }
        protected virtual void setData(NetMessage input)
        {
            object[] fields = input.ReadPrimitiveArray();
            for (int i = 0; i < fields.Count(); i++)
            {
                replicatedProperties[i].SetValue(this, fields[i], null);
            }
        }

        public int AllocateMessageType()
        {
            return messageTypeTracker++;
        }

        public void SendMessage(Guid peer, int type, NetMessage message)
        {
            if (SendMessageCallback != null)
            {
                NetMessage toSend = new NetMessage();
                toSend.Write(type);
                toSend.Write(message);
                SendMessageCallback(peer, this, toSend);
            }
        }
        public virtual void HandleMessage(Guid peer, int type, NetMessage message)
        {
            if (type == StateReplicationMessage)
            {
                setData(message);
            }
            else if(type == FunctionReplicationMessage)
            {
                string method = message.ReadString();
                object[] args = message.ReadPrimitiveArray();
                replicatedMethods[method].Invoke(this, args);
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
                replicationMessage.WritePrimitiveArray(args);
                SendMessage(default(Guid), FunctionReplicationMessage, replicationMessage);
            }
        }
        public void Replicate()
        {
            if (replicateCounter <= 0 && CanReplicate)
            {
                replicateCounter = minReplicationPeriod;
                NetMessage replicationMessage = new NetMessage();
                getData(replicationMessage);
                SendMessage(default(Guid), StateReplicationMessage, replicationMessage);
            }
        }

        public int UpdateOrder { get; protected set; }
        public virtual void Update(GameTime gameTime)
        {
            if (replicateCounter > 0) { replicateCounter -= gameTime.ElapsedGameTime.Milliseconds; }
        }
        public virtual void Draw(GameTime gameTime, SpriteBatch spriteBatch, bool updatingWater) { }
        public virtual void TickTenth() { }
        public virtual void TickOne() { }
    }
}
