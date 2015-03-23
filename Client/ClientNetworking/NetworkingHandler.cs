using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SharedCode;
using System.Runtime.Serialization.Formatters.Binary;
using System.IO;
using Microsoft.DirectX.DirectPlay;

namespace ClientNetworking
{
    public class NetworkingHandler
    {
        private Client ThinClient = null;
        private bool connected = false;
        private List<DataPacket> NewDataList;
        private BinaryFormatter bf = new BinaryFormatter();

        //Interface calls upon these public methods to obtain information sent from client
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

        public void SendData(List<DataPacket> dataList)
        {
            NetworkPacket packet = new NetworkPacket();

            //Convert data to binary form
            using (MemoryStream ms = new MemoryStream())
            {
                bf.Serialize(ms, dataList);
                packet.Write(ms.ToArray());
            }

            ThinClient.Send(packet, 0, SendFlags.Guaranteed);
        }

        public bool InitializeClient(string name)
        {
            // Create our client object
            ThinClient = new Client();
            NewDataList = new List<DataPacket>();
            bool connectionSuccess = true;

            // Check to see if we can create a TCP/IP connection
            if (!IsServiceProviderValid(Address.ServiceProviderTcpIp))
            {
                connectionSuccess = false;
            }

            // Create a new address for our local machine
            Address deviceAddress = new Address();
            deviceAddress.ServiceProvider = Address.ServiceProviderTcpIp;
            // Create a new address for our host machine
            Address hostaddress = new Address();
            hostaddress.ServiceProvider = Address.ServiceProviderTcpIp;
            hostaddress.AddComponent(Address.KeyPort, ServerInfo.DataPort);
            // Find out about the newer standard!!
            // Set our name
            PlayerInformation info = new PlayerInformation();
            info.Name = name;
            ThinClient.SetClientInformation(info, SyncFlags.ClientInformation);

            // Set up an application description
            ApplicationDescription desc = new ApplicationDescription();
            desc.GuidApplication = ServerInfo.ApplicationGuid;

            //Attach Event handlers
            ThinClient.ConnectComplete += new ConnectCompleteEventHandler(OnConnectComplete);
            ThinClient.FindHostResponse += new FindHostResponseEventHandler(OnFindHost);
            ThinClient.Receive += new ReceiveEventHandler(OnDataReceive);
            ThinClient.SessionTerminated += new SessionTerminatedEventHandler(OnSessionTerminate);

            try
            {
                // Search for a server
                //This will trigger the FindHostResponse event if a server is up and running
                ThinClient.FindHosts(desc, hostaddress, deviceAddress, null, 0, 0, 0, FindHostsFlags.None);
            }
            catch
            {
                connectionSuccess = false;
            }

            return connectionSuccess;
        }

        private bool IsServiceProviderValid(Guid provider)
        {
            //ask directPlay for the service provider list
            ServiceProviderInformation[] providers = ThinClient.GetServiceProviders(false);

            foreach (ServiceProviderInformation info in providers)
            {
                if (info.Guid == provider)
                    return true;
            }

            //not found
            return false;
        }

        private void OnFindHost(object sender, FindHostResponseEventArgs e)
        {
            lock (this)
            {
                // Do nothing if we're connected already
                if (connected) { }
                else
                {
                    connected = true;

                    // Connect to the first one
                    ((Client)sender).Connect(e.Message.ApplicationDescription,
                          e.Message.AddressSender,
                        e.Message.AddressDevice, null, ConnectFlags.OkToQueryForAddressing);
                }
            }
        }

        private void OnConnectComplete(object sender, ConnectCompleteEventArgs e)
        {
            // Check to see if we connected properly
            if (e.Message.ResultCode == ResultCode.Success)
            {
                connected = true;
            }
            else
            {
                connected = false;
            }
        }

        private void OnSessionTerminate(object sender, SessionTerminatedEventArgs e)
        {
            connected = false;

            // Dispose of our connection, and set it to null
            ThinClient.Dispose();
            ThinClient = null;
        }

        private void OnDataReceive(object sender, ReceiveEventArgs e)
        {
            DataPacket data;
            using (MemoryStream ms = new MemoryStream())
            {
                data = (DataPacket)bf.Deserialize(new MemoryStream(e.Message.ReceiveData.GetData()));
            }

            NewDataList.Add(data);
        }
    }
}
