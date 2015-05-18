using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;

namespace Spectrum.Framework.Network.Directory
{
    public class DirectoryUser
    {

        public int ID { get; private set; }
        public string Name { get; private set; }
        public Guid Guid { get; private set; }
        public IPAddress ExternalIP { get; private set; }
        public int Port { get; private set; }
        public DirectoryUser(JObject user)
        {
            setValues(user);
        }
        public void Update(JObject user)
        {
            if ((int)user["id"] != ID)
            {
                throw new ArgumentOutOfRangeException("You must provide the jobject for the same user to this method");
            }
            setValues(user);
        }
        private void setValues(JObject user)
        {
            ID = (int)user["id"];
            Name = (string)user["name"];
            Guid = Guid.Parse((string)user["guid"]);
            Port = (int)user["port"];
            IPAddress testIP;
            if (IPAddress.TryParse((string)user["ip"], out testIP))
            {
                ExternalIP = testIP;
            }
        }
        public void UpdateIP(IPAddress ip, int port)
        {
            ExternalIP = ip;
            this.Port = port;
        }
    }
}
