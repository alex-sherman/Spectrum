using ProtoBuf;
using Replicate;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Spectrum.Framework.Network
{
    [ReplicateType]
    public struct NetID : IComparable
    {
        public ulong? SteamID;
        public Guid? Guid;

        public static bool operator >(NetID lhs, NetID rhs)
        {
            return lhs.CompareTo(rhs) > 0;
        }
        public static bool operator <(NetID lhs, NetID rhs)
        {
            return lhs.CompareTo(rhs) < 0;
        }

        public static bool operator ==(NetID lhs, NetID rhs)
        {
            return lhs.Equals(rhs);
        }

        public static bool operator !=(NetID lhs, NetID rhs)
        {
            return !lhs.Equals(rhs);
        }

        public NetID(Guid guid)
        {
            this.Guid = guid;
            SteamID = null;
        }

        public NetID(ulong steamID)
        {
            SteamID = steamID;
            Guid = null;
        }

        public override bool Equals(object obj)
        {
            if(obj is NetID)
            {
                NetID other = (NetID)obj;
                return other.Guid == Guid && other.SteamID == SteamID;
            }
            return false;
        }

        public override int GetHashCode() => Guid?.GetHashCode() ?? SteamID.GetHashCode();

        public int CompareTo(object obj)
        {
            if(obj is NetID)
            {
                NetID otherNetID = (NetID)obj;
                if (SteamID != null)
                {
                    if (otherNetID.SteamID == null)
                        return 1;
                    return SteamID.Value.CompareTo(otherNetID.SteamID.Value);
                }
                else if(Guid != null)
                {
                    if (otherNetID.Guid == null)
                        return 1;
                    return Guid.Value.CompareTo(otherNetID.Guid.Value);
                }
                return 0;
            }
            throw new ArgumentException("Cannot compare to anything but NetID");
        }
        public override string ToString()
        {
            if (Guid.HasValue) return Guid.Value.ToString();
            else if (SteamID.HasValue) return $"Steam: {SteamID.Value}";
            return "null";
        }
    }
}
