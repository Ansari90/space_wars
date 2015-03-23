using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;
using System.Windows.Forms;
using Microsoft.DirectX;
using Microsoft.DirectX.Direct3D;
using SharedCode;

namespace ClientGraphics
{
    //GRHandler --> Graphic Resource Handler
    //This class performs all drawing for the client
    public class GraphicsHandler
    {
        //Specifying this makes it easier to program the logic on the server side
        private const int ScreenWidth = 800;
        private const int ScreenHeight = 600;

        //these will hold rendering data
        private Texture backGroundTexture, playerBulletTexture, enemyBulletTexture, shipTexture, minionTexture, bossTexture, victoryTexture, defeatTexture;
        private Rectangle backGroundSize, playerBulletSize, enemyBulletSize, shipSize, minionSize, bossSize, victorySize, defeatSize;
        private Vector3 nullVector = new Vector3(0, 0, 0);

        //System specific display objects
        private Device device;
        private Sprite sprite;

        //On creation, a GraphicsHandler class sets itself up as per the system's graphics specifications
        public GraphicsHandler(System.Windows.Forms.Control theWindow)
        {
            // Set our presentation parameters
            PresentParameters presentParams = new PresentParameters();
            presentParams.SwapEffect = SwapEffect.Discard;

            // Start up full screen
            Format current = Manager.Adapters[0].CurrentDisplayMode.Format;

            if (Manager.CheckDeviceType(0, DeviceType.Hardware, current, current, false))
            {
                // Perfect, this is valid
                presentParams.Windowed = true;               //we are running in full screen
                presentParams.BackBufferFormat = current;     //the BackBuffer holds the screen info
                presentParams.BackBufferCount = 1;
                presentParams.BackBufferWidth = ScreenWidth;
                presentParams.BackBufferHeight = ScreenHeight;
            }
            else
            {
                presentParams.Windowed = true;                //use window if the above fails
            }

            //Setup all display objects
            device = new Device(0, DeviceType.Hardware, theWindow, CreateFlags.SoftwareVertexProcessing, presentParams);
            sprite = new Sprite(device);

            //Initialize all textures
            backGroundTexture = TextureLoader.FromFile(device, @"..\..\space.jpg");
            playerBulletTexture = TextureLoader.FromFile(device, @"..\..\playerBullet.bmp");
            enemyBulletTexture = TextureLoader.FromFile(device, @"..\..\enemyBullet.bmp");
            shipTexture = TextureLoader.FromFile(device, @"..\..\ship.bmp");
            minionTexture = TextureLoader.FromFile(device, @"..\..\minion.bmp");
            bossTexture = TextureLoader.FromFile(device, @"..\..\boss.bmp");
            victoryTexture = TextureLoader.FromFile(device, @"..\..\victory.bmp");
            defeatTexture = TextureLoader.FromFile(device, @"..\..\defeat.bmp");

            using (Surface s = backGroundTexture.GetSurfaceLevel(0))
            {
                SurfaceDescription desc = s.Description;
                backGroundSize = new Rectangle(0, 0, desc.Width, desc.Height);
            }
            using (Surface s = playerBulletTexture.GetSurfaceLevel(0))
            {
                SurfaceDescription desc = s.Description;
                playerBulletSize = new Rectangle(0, 0, desc.Width, desc.Height);
            }
            using (Surface s = enemyBulletTexture.GetSurfaceLevel(0))
            {
                SurfaceDescription desc = s.Description;
                enemyBulletSize = new Rectangle(0, 0, desc.Width, desc.Height);
            }
            using (Surface s = shipTexture.GetSurfaceLevel(0))
            {
                SurfaceDescription desc = s.Description;
                shipSize = new Rectangle(0, 0, desc.Width, desc.Height);
            }
            using (Surface s = minionTexture.GetSurfaceLevel(0))
            {
                SurfaceDescription desc = s.Description;
                minionSize = new Rectangle(0, 0, desc.Width, desc.Height);
            }
            using (Surface s = bossTexture.GetSurfaceLevel(0))
            {
                SurfaceDescription desc = s.Description;
                bossSize = new Rectangle(0, 0, desc.Width, desc.Height);
            }
            using (Surface s = defeatTexture.GetSurfaceLevel(0))
            {
                SurfaceDescription desc = s.Description;
                defeatSize = new Rectangle(0, 0, desc.Width, desc.Height);
            }
            using (Surface s = victoryTexture.GetSurfaceLevel(0))
            {
                SurfaceDescription desc = s.Description;
                victorySize = new Rectangle(0, 0, desc.Width, desc.Height);
            }
        }
        
        public void DidWeWin(bool torf)
        {
            device.BeginScene();
            device.Clear(ClearFlags.Target, Color.White, 1.0f, 0);
            sprite.Begin(SpriteFlags.AlphaBlend);
            if(torf == true)
                sprite.Draw(victoryTexture, victorySize, nullVector, nullVector, Color.White.ToArgb());
            else
                sprite.Draw(defeatTexture, defeatSize, nullVector, nullVector, Color.White.ToArgb());
            sprite.End();
            device.EndScene();
            device.Present();
        }

        //Function to send all drawing data to
        public void Render(List<DataPacket> displayList)
        {
            device.BeginScene();
            device.Clear(ClearFlags.Target, Color.White, 1.0f, 0);
            sprite.Begin(SpriteFlags.AlphaBlend);

            //Draw the background
            sprite.Draw(
            backGroundTexture,           //A Texture that represents the sprite texture
            backGroundSize,              //A Rectangle object that indicates the portion of the source
                                         //texture to use for the sprite. Specify Empty to use the
                                         //entire source image for the sprite
            nullVector,                  //A Vector3 structure that identifies the center of the
                                         //sprite. A value of (0,0,0) indicates the upper-left corner
            nullVector,                  //A Vector3 structure that identifies the position of
                                         //the sprite. A value of (0,0,0) indicates the upper-left corner
            Color.White.ToArgb());       //A Color structure. The color and alpha channels are
                                         //modulated by this value. The Transparent value maintains
                                         //the original source color and alpha data

            //Draw all the objects on screen
            foreach (DataPacket thePacket in displayList)
            {
                switch (thePacket.Type)
                {
                    case SpriteType.Ship:
                        sprite.Draw(shipTexture, shipSize, nullVector, thePacket.Position, Color.White);
                        break;
                    case SpriteType.Minion:
                        sprite.Draw(minionTexture, minionSize, nullVector, thePacket.Position, Color.White);
                        break;
                    case SpriteType.Boss:
                        sprite.Draw(bossTexture, bossSize, nullVector, thePacket.Position, Color.White);
                        break;
                    case SpriteType.EnemyBullet:
                        sprite.Draw(enemyBulletTexture, enemyBulletSize, nullVector, thePacket.Position, Color.White);
                        break;
                    case SpriteType.PlayerBullet:
                        sprite.Draw(playerBulletTexture, playerBulletSize, nullVector, thePacket.Position, Color.White);
                        break;
                }
            }

            sprite.End();
            device.EndScene();
            device.Present();
        }
    }
}
