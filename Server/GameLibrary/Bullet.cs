using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SharedCode;
using Microsoft.DirectX;

namespace GameLibrary
{
    class Bullet : GameObject
    {
        private int bulletMoves = 800;

        public Bullet(Vector3 position, Direction direction, SpriteType theType)
        {
            Position = position;
            _Direction = direction;
            Type = theType;

            if (Type == SpriteType.PlayerBullet)
            {
                movement = 3;
            }
            else
            {
                movement = 1.5f;    //Enemy bullets move at half the speed of player bullets
            }

            life = 1;
            Width = 2;
            Height = 2;
        }

        public override void Move()
        {
            Vector3 position = Position;
            switch (_Direction)
            {
                case Direction.Up:
                    position.Y -= movement;
                    break;
                case Direction.Down:
                    position.Y += movement;
                    break;
                case Direction.Left:
                    position.X -= movement;
                    break;
                case Direction.Right:
                    position.X += movement;
                    break;
            }

            //If a bullet moves out of screen bounds, it 'dies'
            //if (position.X < 0 || position.Y < 0 || position.X > Space.ScrWidth || position.Y > Space.ScrHeight)
            //{
            //    life = 0;
            //}

            if (bulletMoves == 0)
            {
                life = 0;
            }

            Position = position;
            bulletMoves--;
        }
    }
}
