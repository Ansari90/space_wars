using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SharedCode;
using Microsoft.DirectX;

namespace GameLibrary
{
    //Holds all the objects, and performs a simulation of the spacefield to display
    public class Space
    {
        //Game Screen Constants - Related to objects by virtue of the bounds between which an object can exist
        public static int ScrWidth = 800;
        public static int ScrHeight = 600;
        
        //Following variables will be used exclusively by the HostileAction method
        private const int spawnBulletsAt = 20;
        private const int spawnMinionsAt = spawnBulletsAt * 10;
        private int spawnCounter = 0;
        private int spawnSafety = 50;
        private int minionSpawnDistance = 50;

        List<GameObject> Everything;
        Random randomGen;

        public Space()
        {
            Everything = new List<GameObject>();
            randomGen = new Random();
            Everything.Add(GenerateGameObject(SpriteType.Boss));
        }

        public int GetBossHealth()
        {
            int bossLife = 0;
            foreach (GameObject theObject in Everything)
            {
                if (theObject.Type == SpriteType.Boss)
                    bossLife = theObject.life;
            }

            return bossLife;
        }

        //Creates an object of specified type at a 'safe' location on the map
        public GameObject GenerateGameObject (SpriteType theType)
        {
            GameObject theObject = null;
            Vector3 tmpVector = new Vector3(0, 0, 0);

            switch (theType)
            {
                case SpriteType.Boss:
                    theObject = new Boss(tmpVector, Direction.Up);
                    break;
                case SpriteType.Minion:
                    theObject = new Minion(tmpVector, Direction.Up);
                    break;
                case SpriteType.Ship:
                    theObject = new Ship(tmpVector, Direction.Up, "");
                    break;
            }

            bool validArea = false, collision; 
            while (!validArea)
            {
                tmpVector.X = (float)randomGen.NextDouble() * ScrWidth;
                tmpVector.Y = (float)randomGen.NextDouble() * ScrHeight;

                if (tmpVector.X > (ScrWidth - theObject.Width) || tmpVector.Y > (ScrHeight - theObject.Height)) 
                {
                    //get new X-Y pair, current values will cause object to be out of screen bounds
                } 
                else
                {
                    //check for collisions, get a new X-Y pair for in the event of a single collision                 
                    collision = false; ;
                    foreach (GameObject gO in Everything)
                    {
                        if (gO.Collission(theObject) == true)
                        {
                            collision = true;
                        }
                    }

                    //check if the spaceship is too close to boss
                    //200 +/- units around the boss (50 +/- around everything else) is a no-spawn zone for players
                    if (theType != SpriteType.Boss && !collision)
                    {
                        int spawnCheck = spawnSafety;

                        foreach (GameObject gO in Everything)
                        {
                            if (gO.Type == SpriteType.Boss)
                                spawnCheck = spawnSafety * 4;
                            else
                                spawnCheck = spawnSafety;


                            if (Math.Abs(tmpVector.X - gO.Position.X) < spawnCheck ||
                            Math.Abs(tmpVector.Y - gO.Position.Y) < spawnCheck)
                            {
                                collision = true;
                            }
                        }
                        
                    }

                    if (!collision)
                    {
                        validArea = true;                        
                        theObject.Position = tmpVector;   
                    }
                }
            }


            return theObject;
        }        

        //Functions to be wired up to event handlers dealing with player input (joining, movement and firing)
        public void AddPlayer(String playerID)
        {
            //find an empty area of space capable of holding the ship
            Ship tempShip = GenerateGameObject(SpriteType.Ship) as Ship;
            tempShip.PlayerID = playerID;

            Everything.Add(tempShip);
        }
        public void RemovePlayer(String playerID)
        {
            int count = Everything.Count;
            for (int i = 0; i < count; i++)
            {
                if (Everything[i].Type == SpriteType.Ship)
                {
                    if (((Ship)Everything[i]).PlayerID == playerID)
                    {
                        Everything[i].Hit();    //Since players die on a single hit, one call to this is enough;
                    }
                }
            }
        }
        public void Fire(String playerId)
        {
            Ship theShip = null;
            foreach (GameObject theObject in Everything)
            {
                if (theObject.Type == SpriteType.Ship)
                {
                    theShip = theObject as Ship;
                    if (theShip.PlayerID == playerId)
                        break;
                }
            }

            Everything.Add(new Bullet(theShip.Position, theShip._Direction, SpriteType.PlayerBullet));
        }
        public void SendNewDirection(string playerId, Direction direction)
        {
            foreach (GameObject theObject in Everything)
            {
                if (theObject.Type == SpriteType.Ship)
                {
                    Ship player = theObject as Ship;
                    if (player.PlayerID == playerId)
                    {
                        player._Direction = direction;
                        break;
                    }
                }
            }
        }

        //Any player still alive? -- the defeat check
        public bool AnyPlayer()
        {
            int playerCount = 0;

            foreach (GameObject theObject in Everything)
            {
                if (theObject.Type == SpriteType.Ship)
                    playerCount++;
            }

            if (playerCount == 0)
                return false;
            else
                return true;
        }
        //Boss still alive? -- the victory check
        public bool BossAlive()
        {
            bool bossAlive = false;

            foreach (GameObject theObject in Everything)
            {
                if (theObject.Type == SpriteType.Boss)
                    bossAlive = true;
            }

            return bossAlive;
        }

        //Invokes Move() on all GameObjects, and then checks for collissions between objects.
        //Once collission checks are completed, Everything is updated (pun intended)
        public void UpdateEverything()
        {
            foreach (GameObject theObject in Everything)
            {
                theObject.Move();
            }

            //Deal Damage on Collission
            foreach (GameObject theObject in Everything)
            {
                if (theObject.Alive())
                {
                    switch (theObject.Type)
                    {
                        case SpriteType.EnemyBullet:
                            foreach (GameObject otherObject in Everything)
                            {
                                if (basicCheck(theObject, otherObject) && otherObject.Type == SpriteType.Ship)
                                {
                                    theObject.Hit();
                                    otherObject.Hit();
                                }
                            }
                            break;
                        case SpriteType.PlayerBullet:
                            foreach (GameObject otherObject in Everything)
                            {
                                if (basicCheck(theObject, otherObject) && (otherObject.Type == SpriteType.Boss || otherObject.Type == SpriteType.Minion))
                                {
                                    theObject.Hit();
                                    otherObject.Hit();
                                }
                            }
                            break;
                        //Ships collide and take damage from everything
                        case SpriteType.Ship:
                            foreach (GameObject otherObject in Everything)
                            {
                                if (basicCheck(theObject, otherObject) && (otherObject.Type == SpriteType.Boss || otherObject.Type == SpriteType.EnemyBullet 
                                    || otherObject.Type == SpriteType.Minion || otherObject.Type == SpriteType.Ship))
                                {
                                    theObject.Hit();
                                    otherObject.Hit();
                                }
                            }
                            break;
                        //EnemyBullets, Minions and the Boss take damage only from ships and player bullets
                        case SpriteType.Boss:
                        case SpriteType.Minion:
                            foreach (GameObject otherObject in Everything)
                            {
                                if ((otherObject.Type == SpriteType.PlayerBullet || otherObject.Type == SpriteType.Ship) && basicCheck(theObject, otherObject))
                                {
                                    theObject.Hit();
                                    otherObject.Hit();
                                }
                            }
                            break;
                    }
                }
            }

            //Remove dead objects
            List<int> deadIndexes = new List<int>();
            foreach (GameObject theObject in Everything)
            {
                if (theObject.Alive() == false)
                {
                    deadIndexes.Add(Everything.IndexOf(theObject));
                }
            }

            foreach (int i in deadIndexes)
            {
                Everything.RemoveAt(i);
            }
        }
        //Helper Function for UpdateEverything()
        private bool basicCheck(GameObject theObject, GameObject otherObject)
        {
            bool torf;

            if (otherObject != theObject && otherObject.Collission(theObject) && otherObject.Alive())
                torf = true;
            else
                torf = false;

            return torf;
        }

        //Invoking this method will cause enemy bullets to spawn around the Boss, and minions to spawn around the map/boss
        //(minion functionality not implemented)
        public void HostileAction()
        {
            if ((spawnCounter % spawnBulletsAt) == 0)
            {
                Boss BossObject = null;
                foreach (GameObject theObject in Everything)
                {
                    if (theObject.Type == SpriteType.Boss)
                    {
                        BossObject = (Boss)theObject;
                        break;
                    }
                }

                if (BossObject != null)
                {
                    switch (randomGen.Next(4))
                    {
                        case 0:
                            TopSpawn(BossObject);
                            break;
                        case 1:
                            BotSpawn(BossObject);
                            break;
                        case 2:
                            LeftSpawn(BossObject);
                            break;
                        case 3:
                            RightSpawn(BossObject);

                            break;
                    }

                    if ((spawnCounter % spawnMinionsAt) == 0)
                    {
                        spawnCounter = 0;
                        MinionSpawn(BossObject);
                    }
                }
            }

            spawnCounter++;
        }

        //Boss Bullet Spawn Functions
        //Use this if you want the minion to spawn around the boss, not randomly on the map
        private void MinionSpawn(Boss BossObject)
        {
            Vector3 position = BossObject.Position;
            int width = BossObject.Width;
            int height = BossObject.Height;
            int side = randomGen.Next(4);

            switch (randomGen.Next(4))
            {
                case 1:
                    Everything.Add(new Minion(new Vector3(position.X + width/2, position.Y - minionSpawnDistance, 0), Direction.Up));
                    break;
                case 2:
                    Everything.Add(new Minion(new Vector3(position.X + width/2, position.Y + height + minionSpawnDistance, 0), Direction.Down));
                    break;
                case 3:
                    Everything.Add(new Minion(new Vector3(position.X - minionSpawnDistance, position.Y + height/2,0), Direction.Left));
                    break;
                case 0:
                    Everything.Add(new Minion(new Vector3(position.X + width + minionSpawnDistance, position.Y + height/2,0), Direction.Right));
                    break;
            }
        }

        private void TopSpawn(Boss BossObject)
        {
            Vector3 position = BossObject.Position;
            int width = BossObject.Width;
            int height = BossObject.Height;
            int side =  randomGen.Next(4);

            switch (side)
            {
                case 0:
                    Everything.Add(new Bullet(new Vector3(position.X, position.Y, 0), Direction.Up, SpriteType.EnemyBullet));
                    break;
                case 1:
                    Everything.Add(new Bullet(new Vector3(position.X + width / 4, position.Y, 0), Direction.Up, SpriteType.EnemyBullet));
                    break;
                case 2:
                    Everything.Add(new Bullet(new Vector3(position.X + (width - width / 4), position.Y, 0), Direction.Up, SpriteType.EnemyBullet));
                    break;
                case 3:
                    Everything.Add(new Bullet(new Vector3(position.X + width, position.Y, 0), Direction.Up, SpriteType.EnemyBullet));
                    break;
            }
        }

        private void BotSpawn(Boss BossObject)
        {
            Vector3 position = BossObject.Position;
            int width = BossObject.Width;
            int height = BossObject.Height;
            int side = randomGen.Next(4);

            switch (side)
            {
                case 0:
                    Everything.Add(new Bullet(new Vector3(position.X, position.Y + height, 0), Direction.Down, SpriteType.EnemyBullet));
                    break;
                case 1:
                    Everything.Add(new Bullet(new Vector3(position.X + width, position.Y + height, 0), Direction.Down, SpriteType.EnemyBullet));
                    break;
                case 2:
                    Everything.Add(new Bullet(new Vector3(position.X + width / 4, position.Y + height, 0), Direction.Down, SpriteType.EnemyBullet));
                    break;
                case 3:
                    Everything.Add(new Bullet(new Vector3(position.X + (width - width / 4), position.Y + height, 0), Direction.Down, SpriteType.EnemyBullet));
                    break;
            }
        }

        private void LeftSpawn(Boss BossObject)
        {
            Vector3 position = BossObject.Position;
            int width = BossObject.Width;
            int height = BossObject.Height;
            int side = randomGen.Next(4);

            switch (side)
            {
                case 0:
                    Everything.Add(new Bullet(new Vector3(position.X, position.Y, 0), Direction.Left, SpriteType.EnemyBullet));
                    break;
                case 1:
                    Everything.Add(new Bullet(new Vector3(position.X, position.Y + height / 4, 0), Direction.Left, SpriteType.EnemyBullet));
                    break;
                case 2:
                    Everything.Add(new Bullet(new Vector3(position.X, position.Y + height, 0), Direction.Left, SpriteType.EnemyBullet));
                    break;
                case 3:
                    Everything.Add(new Bullet(new Vector3(position.X, position.Y + (height - height / 4), 0), Direction.Left, SpriteType.EnemyBullet));
                    break;
            }
        }

        private void RightSpawn(Boss BossObject)
        {
            Vector3 position = BossObject.Position;
            int width = BossObject.Width;
            int height = BossObject.Height;
            int side = randomGen.Next(4);

            switch (side)
            {
                case 0:
                    Everything.Add(new Bullet(new Vector3(position.X + width, position.Y, 0), Direction.Right, SpriteType.EnemyBullet));
                    break;
                case 1:
                    Everything.Add(new Bullet(new Vector3(position.X + width, position.Y + height / 4, 0), Direction.Right, SpriteType.EnemyBullet));
                    break;
                case 2:
                    Everything.Add(new Bullet(new Vector3(position.X + width, position.Y + (height - height / 4), 0), Direction.Right, SpriteType.EnemyBullet));
                    break;
                case 3:
                    Everything.Add(new Bullet(new Vector3(position.X + width, position.Y + height, 0), Direction.Right, SpriteType.EnemyBullet));
                    break;
            }
        }

        //Prepare a list of DataPackets to be sent to clients
        public List<DataPacket> GetSpaceData()
        {
            List<DataPacket> allSpaceData = new List<DataPacket>();
            DataPacket thePacket;

            foreach (GameObject theObject in Everything)
            {
                thePacket = new DataPacket(theObject.Position, theObject.Type, StatusMessage.GraphicsUpdate);
                thePacket.BossHealth = GetBossHealth();
                allSpaceData.Add(thePacket);
            }

            return allSpaceData;
        }
    }
}
