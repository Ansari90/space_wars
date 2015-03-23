using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using GameLibrary;
using SharedCode;

namespace Server
{
    public partial class Form1 : Form
    {
        //Don't want them anywhere near my private space!
        private Space ServerSpace;
        private NetworkingHandler Messenger;
        //Holds all playerIDs
        //Used to make sure all clients are displaying the same image
        private int howManyToRenderOn = 0;
        private int renderFinishedOn = 0;

        public Form1()
        {
            InitializeComponent();
            Messenger = new NetworkingHandler(this);
        }

        //Brings all server functions together
        public void HostGame()
        {
            if (!Messenger.InitializeServer())
            {
                MessageBox.Text = "Could not create a TCP/IP service provider! Exiting";
                this.Close();
            }

            if (ServerSpace == null)
            {
                ServerSpace = new Space();
            }         
        }

        public void Next()
        {
            if (!(renderFinishedOn < howManyToRenderOn))
            {
                renderFinishedOn = 0;
                NextMoment();
                Messenger.SendData(ServerSpace.GetSpaceData());
            }
            WinOrLoss();
        }

        public void ProcessInputs(int whichOne)
        {
            List<string> tmpPlayerList;
            switch (whichOne)
            {
                case 1:
                    tmpPlayerList = Messenger.GetNewPlayers();
                    foreach (string s in tmpPlayerList)
                    {
                        ServerSpace.AddPlayer(s);
                        howManyToRenderOn++;
                        renderFinishedOn++;
                    }            
                    break;
                case 2:
                    tmpPlayerList = Messenger.GetDcdPlayers();
                    foreach (string s in tmpPlayerList)
                    {
                        howManyToRenderOn--;
                        ServerSpace.RemovePlayer(s);
                    }
            
                    break;
                case 3:
                    List<DataPacket> newData = Messenger.GetNewData();
                    foreach (DataPacket data in newData)
                    {
                        switch (data.TheStatus)
                        {
                            case StatusMessage.MovementUpdate:
                                if (data.Facing == Direction.Fire)
                                {
                                    ServerSpace.Fire(data.PlayerID);
                                }
                                else
                                {
                                    ServerSpace.SendNewDirection(data.PlayerID, data.Facing);
                                }
                                break;
                            case StatusMessage.RenderFinished:
                                renderFinishedOn++;
                                break;
                        }
                    }            
                    break;
            }            
        }

        public void WinOrLoss()
        {
            List<DataPacket> listOfOne = new List<DataPacket>();
            if (ServerSpace.AnyPlayer() != true)
            {
                listOfOne.Add(new DataPacket(Direction.Undefined, StatusMessage.Defeat));
                Messenger.SendData(listOfOne);
                Messenger.Dispose();
            }
            else
            {
                if (ServerSpace.BossAlive() != true)
                {
                    listOfOne.Add(new DataPacket(Direction.Undefined, StatusMessage.Victory));
                    Messenger.SendData(listOfOne);
                    Messenger.Dispose();
                }
            }
        }

        //Run through a list of activities which defines the next moment in Space
        public void NextMoment()
        {
            ServerSpace.UpdateEverything();
            ServerSpace.HostileAction();
        }
    }
}
