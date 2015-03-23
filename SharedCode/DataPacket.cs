using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.DirectX;

namespace SharedCode
{
    public enum SpriteType
    {
        Ship,
        Minion,
        Boss,
        PlayerBullet,
        EnemyBullet
    }

    public enum StatusMessage
    {
        Victory,
        Defeat,
        GraphicsUpdate,
        MovementUpdate,
        RenderFinished      //Rendering has finished on this particular client
    }

    public enum Direction   //WASD & Q
    {
        Up,
        Down,
        Left,
        Right,
        Fire,
        Undefined
    }

    public class ServerInfo
    {
        public static readonly Guid ApplicationGuid = new Guid
          (15741039, 1702, 1503, 178, 101, 32, 13, 121, 230, 109, 59);

        public const int DataPort = 9800;
    }

    //Contains information on what to draw, and where to draw it
    [Serializable]
    public class DataPacket
    {
        public Vector3 Position { get; set; }
        public SpriteType Type { get; set; }
        public StatusMessage TheStatus { get; set; }
        public Direction Facing { get; set; }
        public string PlayerID { get; set; }
        public int BossHealth { get; set; }

        public DataPacket(Vector3 position, SpriteType type, StatusMessage message) //Used by server to send data to a client
        {
            Position = position;
            Type = type;
            TheStatus = message;
        }

        public DataPacket(Direction facing, StatusMessage message)  //used by client to send data to the server
        {
            Facing = facing;
            TheStatus = message;
        }
    }
}
