using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using ClientGraphics;
using SharedCode;

namespace Client
{
    public partial class Form1 : Form
    {
        private List<DataPacket> objectList;
        private GraphicsHandler Graphics;
        private NetworkingHandler Messenger;
        private List<DataPacket> inputList;

        private delegate void PrintBossHealth(int health);

        public bool BeginGame = false;

        public Form1()
        {
            InitializeComponent();

            this.KeyDown += new KeyEventHandler(keyPressed);
            BeginGame = true;

            Graphics = new GraphicsHandler(this);
            Messenger = new NetworkingHandler(this);
            inputList = new List<DataPacket>();

            Random randomGen = new Random();
            Messenger.InitializeClient("" + System.DateTime.Now + randomGen.Next(1000000));
        }

        public void printBossHealth(int health)
        {
            this.Text = "Boss Health: " + health + "hp Remaining!";
        }

        public void keyPressed(Object sender, EventArgs e)
        {
            KeyEventArgs keyEvent = (KeyEventArgs)e;
            DataPacket thePacket = new DataPacket(Direction.Fire, StatusMessage.MovementUpdate);
            bool validInput = true;

            switch (keyEvent.KeyCode)
            {
                case Keys.W:
                    thePacket.Facing = Direction.Up;
                    break;
                case Keys.S:
                    thePacket.Facing = Direction.Down;
                    break;
                case Keys.A:
                    thePacket.Facing = Direction.Left;
                    break;
                case Keys.D:
                    thePacket.Facing = Direction.Right;
                    break;
                case Keys.Q:
                    thePacket.Facing = Direction.Fire;
                    break;
                case Keys.Escape:
                    this.Close();
                    break;
                default:
                    validInput = false;
                    break;
            }

            if (validInput == true)
            {
                inputList = new List<DataPacket>();
                inputList.Add(thePacket);
                Messenger.SendData(inputList);
            }
        }

        public void Play()  //It is a game, after all
        {
            objectList = Messenger.GetNewData();
            if (objectList[0].TheStatus == StatusMessage.GraphicsUpdate)
            {
                this.BeginInvoke(new PrintBossHealth(printBossHealth), new object[] { objectList[0].BossHealth });
                Render();
                inputList = new List<DataPacket>();
                inputList.Add(new DataPacket(Direction.Undefined, StatusMessage.RenderFinished));
                Messenger.SendData(inputList);
            }
            else
            {
                switch (objectList[0].TheStatus)
                {
                    case StatusMessage.Defeat:
                        Graphics.DidWeWin(false);
                        Messenger.Dispose();
                        break;
                    case StatusMessage.Victory:
                        Graphics.DidWeWin(true);
                        Messenger.Dispose();
                        break;
                }                
            }            
        }

        public void Render()
        {
            Graphics.Render(objectList);
        }
    }
}
