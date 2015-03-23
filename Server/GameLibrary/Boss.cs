using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SharedCode;
using Microsoft.DirectX;

namespace GameLibrary
{
    class Boss : GameObject
    {
        private int moveCounter = 0;

        public Boss(Vector3 position, Direction direction)
        {
            Position = position;
            _Direction = direction;
            Type = SpriteType.Boss;

            movement = 0.5f;
            life = 50;
            Width = 50;
            Height = 50;
        }

        public override void Move()
        {
            moveCounter++;

            if (moveCounter > 100)
            {
                Random randomGen = new Random();
                switch (randomGen.Next(4))
                {
                    case 0:
                        _Direction = Direction.Up;
                        break;
                    case 1:
                        _Direction = Direction.Down;
                        break;
                    case 2:
                        _Direction = Direction.Left;
                        break;
                    case 3:
                        _Direction = Direction.Right;
                        break;
                }
                moveCounter = 0;
            }

            base.Move();
        }
    }
}
