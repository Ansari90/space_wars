using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SharedCode;
using System.Runtime.Serialization.Formatters.Binary;
using System.IO;
using Microsoft.DirectX.DirectPlay;

namespace ServerNetworking
{
    public class NetworkingHandler
    {
        private Server PrivateServer = null;
        private List<string> NewPlayerList = null;
        private List<DataPacket> NewDataList = null;
        private List<string> DcdPlayerList = null;

        private BinaryFormatter bf = new BinaryFormatter();

        //Interface calls these public methods to obtain information on client actions
        //Think about merging the following 3 functions
        public List<string> GetNewPlayers()
        {
            List<string> tmpList = new List<string>();

            foreach (string s in NewPlayerList)
            {
                tmpList.Add(s);
            }

            NewPlayerList = new List<string>();

            return tmpList;
        }
        public List<DataPacket> GetNewData()
        {
            List<DataPacket> tmpList = new List<DataPacket>();

            foreach (DataPacket s in NewDataList)
            {
                tmpList.Add(s);
            }

            NewDataList = new List<DataPacket>();

            return tmpList;
        }
        public List<string> GetDcdPlayers()
        {
            List<string> tmpList = new List<string>();

            foreach (string s in DcdPlayerList)
            {
                tmpList.Add(s);
            }

            DcdPlayerList = new List<string>();

            return tmpList;
        }

        //Send data to all clients!
        public void SendData(List<DataPacket> allSpaceData)
        {
            NetworkPacket returnedPacket = new NetworkPacket();
            
            //Convert data to binary form
            using (MemoryStream ms = new MemoryStream())
            {
                bf.Serialize(ms, allSpaceData);
                returnedPacket.Write(ms.ToArray());
            }

            PrivateServer.SendTo((int)PlayerID.AllPlayers, returnedPacket, 0, SendFlags.Guaranteed | SendFlags.NoLoopback);
        }

        //A true return signals that the server is up and running
        public bool InitializeServer()
        {
            NewPlayerList = new List<string>();
            NewDataList = new List<DataPacket>();
            DcdPlayerList = new List<string>();

            PrivateServer = new Server();
            bool setupSuccess = true;

            // Check to see if we can create a TCP/IP connection
            if (!IsServiceProviderValid(Address.ServiceProviderTcpIp))
            {
                setupSuccess = false;
            }
            else
            {

                // Create a new address for our local machine
                Address deviceAddress = new Address();
                deviceAddress.ServiceProvider = Address.ServiceProviderTcpIp;
                deviceAddress.AddComponent(Address.KeyPort, ServerInfo.DataPort);

                // Set up an application description
                ApplicationDescription desc = new ApplicationDescription();
                desc.SessionName = "MDX Book Server Session";
                desc.GuidApplication = ServerInfo.ApplicationGuid;
                desc.Flags = SessionFlags.ClientServer | SessionFlags.NoDpnServer;

                try
                {
                    // Host a new session on the Server
                    PrivateServer.Host(desc, deviceAddress);
                }
                catch
                {
                    setupSuccess = false;
                }
            }

            //Attach all handlers
            PrivateServer.PlayerCreated += new PlayerCreatedEventHandler(OnPlayerCreated);
            PrivateServer.Receive += new ReceiveEventHandler(OnDataReceive);
            PrivateServer.PlayerDestroyed += new PlayerDestroyedEventHandler(OnPlayerDestroyed);

            return setupSuccess;
        }

        private bool IsServiceProviderValid(Guid provider)
        {
            ServiceProviderInformation[] providers = PrivateServer.GetServiceProviders(false);

            foreach (ServiceProviderInformation info in providers)
            {
                if (info.Guid == provider)
                    return true;
            }

            return false;
        }

        private void OnPlayerCreated(object sender, PlayerCreatedEventArgs e)
        {
            try
            {
                string playerName = ((Server)sender).GetClientInformation(e.Message.PlayerID).Name;
                NewPlayerList.Add(playerName);
            }   
            catch { /* Ignore this, probably the server */ }
        }

        private void OnDataReceive(object sender, ReceiveEventArgs e)
        {
            DataPacket data;
            using (MemoryStream ms = new MemoryStream())
            {
                data = (DataPacket)bf.Deserialize(new MemoryStream(e.Message.ReceiveData.GetData()));
            }

            data.PlayerID = ((Server)sender).GetClientInformation(e.Message.SenderID).Name;
            NewDataList.Add(data);
        }

        private void OnPlayerDestroyed(object sender, PlayerDestroyedEventArgs e)
        {
            DcdPlayerList.Add(((Server)sender).GetClientInformation(e.Message.PlayerID).Name);
        }
    }
}
