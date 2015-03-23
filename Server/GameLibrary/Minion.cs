using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SharedCode;
using Microsoft.DirectX;

namespace GameLibrary
{
    class Minion : GameObject
    {
        private int moveCounter = 0;
        private int minionMoves = 800;

        public Minion(Vector3 position, Direction direction)
        {
            Position = position;
            _Direction = direction;
            Type = SpriteType.Minion;

            movement = 1;
            life = 5;
            Width = 10;
            Height = 10;
        }

        public override void Move()
        {
            moveCounter++;
            minionMoves--;

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

            if (minionMoves == 0)
                life = 0;

            base.Move();
        }
    }
}
