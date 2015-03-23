using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SharedCode;
using System.Runtime.Serialization.Formatters.Binary;
using System.IO;
using Microsoft.DirectX.DirectPlay;
using System.Windows.Forms;

namespace Server
{
    public class NetworkingHandler
    {
        private Form1 formReference;
        private Microsoft.DirectX.DirectPlay.Server PrivateServer = null;   //full qualification required, because the namespace is called Server too
        private List<string> NewPlayerList = null;
        private List<DataPacket> NewDataList = null;
        private List<string> DcdPlayerList = null;

        private BinaryFormatter bf = new BinaryFormatter();

        public NetworkingHandler(Form1 app)
        {
            formReference = app;
        }

        public void Dispose()
        {
            PrivateServer.Dispose();
            PrivateServer = null;
        }

        //Interface calls these public methods to obtain information on client actions
        //Think about merging the following 3 functions
        public List<string> GetNewPlayers()
        {
            List<string> tmpList = NewPlayerList;
            NewPlayerList = new List<string>();

            return tmpList;
        }
        public List<DataPacket> GetNewData()
        {
            List<DataPacket> tmpList = NewDataList;
            NewDataList = new List<DataPacket>();

            return tmpList;
        }
        
        public List<string> GetDcdPlayers()
        {
            List<string> tmpList = DcdPlayerList;
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

            PrivateServer = new Microsoft.DirectX.DirectPlay.Server();
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
            PrivateServer.PlayerDestroyed += new PlayerDestroyedEventHandler(OnPlayerDestroyed);
            PrivateServer.Receive += new ReceiveEventHandler(OnDataReceive);

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
                string playerName = ((Microsoft.DirectX.DirectPlay.Server)sender).GetClientInformation(e.Message.PlayerID).Name;
                NewPlayerList.Add(playerName);
            }
            catch { /* Ignore this, probably the server */ }
            NextStep(1);
        }

        private void OnPlayerDestroyed(object sender, PlayerDestroyedEventArgs e)
        {
            DcdPlayerList.Add(((Microsoft.DirectX.DirectPlay.Server)sender).GetClientInformation(e.Message.PlayerID).Name);
            NextStep(2);         
        }

        private void OnDataReceive(object sender, ReceiveEventArgs e)
        {
            List<DataPacket> data;
            using (MemoryStream ms = new MemoryStream())
            {
                data = (List<DataPacket>)bf.Deserialize(new MemoryStream(e.Message.ReceiveData.GetData()));
            }

            foreach (DataPacket packet in data)
            {
                packet.PlayerID = ((Microsoft.DirectX.DirectPlay.Server)sender).GetClientInformation(e.Message.SenderID).Name;
                NewDataList.Add(packet);
            }
            NextStep(3);
        }

        private void NextStep(int whichOne)
        {
            formReference.ProcessInputs(whichOne);
            formReference.Next(); 
        }
    }
}
