using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Org.BouncyCastle.Crypto;
using Org.BouncyCastle.Crypto.Parameters;
using Org.BouncyCastle.OpenSsl;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.IO;
using System.Linq;
using System.Net;
using System.Security.Cryptography;
using System.Text;

namespace Spectrum.Framework.Network.Directory
{
    public class DirectoryHelper
    {
        private AsymmetricCipherKeyPair key;
        public DirectoryUser LocalUser = null;
        StringPasswordFinder pass;
        private Uri serverURL;
        public bool Authenticated { get; private set; }
        public int ListenPort = 27015;
        public bool Authenticate(Uri serverURL, string username, string password)
        {
            this.serverURL = serverURL;
            Authenticated = false;
            pass = new StringPasswordFinder(password);
            JObject jobject;
            if (!GetUser(username, out jobject))
            {
                return false;
            }
            TextReader textReader = new StringReader((string)jobject["safe_ppk"]);
            Authenticated = Validate(textReader);
            return Authenticated;
        }
        private bool GetUser(string name, out JObject jobject)
        {
            jobject = null;
            try
            {
                WebClient client = new WebClient();
                string result = client.DownloadString(new Uri(serverURL + "player/" + name));
                jobject = JObject.Parse(result);
                LocalUser = new DirectoryUser(jobject);
            }
            catch (Exception)
            {
                return false;
            }
            return true;
        }
        private bool Validate(TextReader tr)
        {
            PemReader pem = new PemReader(tr, pass);
            try
            {
                key = (AsymmetricCipherKeyPair)pem.ReadObject();
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }
        private void VerifyAuthenticated()
        {
            if (!Authenticated || LocalUser == null) { throw new Exception("AuthManager must be athenticated first to perform this operation"); }
        }
        public byte[] SignData(byte[] bytes)
        {
            VerifyAuthenticated();
            Org.BouncyCastle.Crypto.ISigner sigBouncyCastle1 = Org.BouncyCastle.Security.SignerUtilities.GetSigner("SHA256WithRSA");
            sigBouncyCastle1.Init(true, key.Private);
            sigBouncyCastle1.BlockUpdate(bytes, 0, bytes.Length);
            return sigBouncyCastle1.GenerateSignature();
        }
        public void SetData(string key, string value)
        {
            VerifyAuthenticated();
            using (WebClient wb = new WebClient())
            {
                byte[] bytes = Encoding.ASCII.GetBytes(value);
                NameValueCollection data = new NameValueCollection();
                data[key] = Convert.ToBase64String(bytes);
                byte[] signbytes = SignData(bytes);
                data["signature"] = Convert.ToBase64String(signbytes);

                byte[] response = wb.UploadValues(new Uri(serverURL, "player/" + LocalUser.Name + "/data"), "POST", data);
                string sresponse = Encoding.ASCII.GetString(response);
                if (sresponse != "") { throw new Exception(sresponse); }
            }
        }
        public bool SetIP()
        {
            VerifyAuthenticated();
            using (WebClient wb = new WebClient())
            {
                byte[] bytes = Encoding.ASCII.GetBytes(ListenPort.ToString().ToArray());
                NameValueCollection data = new NameValueCollection();
                data["port"] = Convert.ToBase64String(bytes);
                data["host"] = MultiplayerService.GetLocalIP()[0].ToString();
                byte[] signbytes = SignData(bytes);
                data["signature"] = Convert.ToBase64String(signbytes);
                try
                {
                    byte[] response = wb.UploadValues(new Uri(serverURL, "player/" + LocalUser.Name + "/ip"), "POST", data);
                    string sresponse = Encoding.ASCII.GetString(response);
                    JToken jobj = JToken.Parse(sresponse);
                    LocalUser.UpdateIP(IPAddress.Parse((string)jobj), ListenPort);
                    return true;
                }
                catch(Exception e)
                {
                    DebugPrinter.print("Auth directory couldn't reverse connect: " + e.Message);
                    return false;
                }
            }
        }
        public List<DirectoryUser> GetNearby()
        {
            VerifyAuthenticated();
            List<DirectoryUser> output = new List<DirectoryUser>();
            WebClient client = new WebClient();
            string result = client.DownloadString(new Uri(serverURL, "player/" + LocalUser.Name + "/nearby"));
            JArray data = JArray.Parse(result);
            foreach (JObject jobj in data)
            {
                output.Add(new DirectoryUser(jobj));
            }
            return output;
        }
        public void SaveToFile()
        {
            VerifyAuthenticated();
            NetMessage authMessage = new NetMessage();
            authMessage.Write(serverURL.OriginalString);
            authMessage.Write(LocalUser.Name);
            NetMessage keyMessage = new NetMessage();
            TextWriter tw = new StreamWriter(keyMessage.stream);

            PemWriter pemWriter = new PemWriter(tw);
            pemWriter.WriteObject(key.Private);
            tw.Flush();
            authMessage.Write(keyMessage);
            authMessage.Write(ListenPort);
            FileStream fs = new FileStream("auth.sav", FileMode.Create);
            authMessage.WriteTo(fs);
            fs.Close();
        }
        public bool LoadFile()
        {
            Authenticated = false;
            byte[] guidBytes = new byte[16];
            byte[] ppkBytes;
            string name;
            try
            {
                using (FileStream fs = new FileStream("auth.sav", FileMode.Open))
                {
                    NetMessage fileMessage = new NetMessage(fs);
                    serverURL = new Uri(fileMessage.ReadString());
                    name = fileMessage.ReadString();
                    ppkBytes = fileMessage.ReadMessage().stream.ToArray();
                    ListenPort = fileMessage.ReadInt();
                }
            }
            catch (Exception)
            {
                return false;
            }
            string ppkString = Encoding.ASCII.GetString(ppkBytes);

            Authenticated = Validate(new StringReader(ppkString));
            if (!Authenticated) { return false; }
            JObject jobject;
            Authenticated = GetUser(name, out jobject);
            return Authenticated;
        }
    }
}
