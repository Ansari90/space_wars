using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SharedCode;
using Microsoft.DirectX;

namespace GameLibrary
{
    class Ship : GameObject
    {
        public string PlayerID { get; set; }

        public Ship(Vector3 position, Direction direction, string playerID)
        {
            Position = position;
            _Direction = direction;
            Type = SpriteType.Ship;
            PlayerID = playerID;

            movement = 2;
            life = 1;
            Width = 5;
            Height = 5;
        }
    }
}
