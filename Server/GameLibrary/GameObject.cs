using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;
using SharedCode;
using Microsoft.DirectX;

namespace GameLibrary
{
    //Everything is an object!
    public abstract class GameObject
    {
        //These variables are to be setup in the constructor for their respective child classes
        protected float movement;
        public int life { get; set; }
        public int Width { get; set; }
        public int Height { get; set; }
        public Vector3 Position { get; set; }
        public SpriteType Type { get; set; }
        public Direction _Direction { get; set; }

        public void Hit() { life--; }
        public bool Alive() { return (life > 0 ? true : false); }

        public bool Collission(GameObject theObject)
        {
            bool collided = false;

            Rectangle rect1 = new Rectangle((int)Position.X, (int)Position.Y, Width, Height);
            Rectangle rect2 = new Rectangle((int)theObject.Position.X, (int)theObject.Position.Y, theObject.Width, theObject.Height);

            if (rect1.IntersectsWith(rect2) == true)
                collided = true;

            return collided;

            /* More complicated way, without System.Drawing's Rectangle Class
            //Collission Check Algorithm
            //We check if corners of either object lie within the rectangles created by
            //either one of them. 
            
            GameObject obj1 = this, obj2 = theObject;

            double top, top2, bottom, bottom2, left, left2, right, right2;

            for (int i = 0; i < 2; i++, obj1 = theObject, obj2 = this)
            {                
                //declaring it like this simply makes it easier
                left = obj1.Position.X;
                right = left + width;
                top = obj1.Position.Y;
                bottom = top + height;

                left2 = obj2.Position.X;
                right2 = left2 + obj2.width;
                top2 = obj2.Position.Y;
                bottom2 = top2 + obj2.height;

                //4 corners, 4 checks -- Can be accomplished in significantly less code through an array, I guess
                //top left corner
                if (left2 > left && left2 < right)
                {
                    if(top2 > top && top2 < bottom)
                    {
                        collided = true;
                    }
                }
                //top right corner
                if (right2 > left && right2 < right)
                {
                    if (top2 > top && top2 < bottom)
                    {
                        collided = true;
                    }
                }
                //bottom left corner
                if (left2 > left && left2 < right)
                {
                    if (bottom2 > top && bottom2 < bottom)
                    {
                        collided = true;
                    }
                }
                //bottom right corner
                if (right2 > left && right2 < right)
                {
                    if (bottom2 > top && bottom2 < bottom)
                    {
                        collided = true;
                    }
                }
            }            
            */
        }

        virtual public void Move()
        {
            Vector3 position = Position, position2 = Position;
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

            Position = position;

            //check if we went out of bounds, if yes, revert to original position with opposite direction
            if ((position.X + Width) > Space.ScrWidth || position.X < 0)
            {
                if (_Direction == Direction.Left)
                    _Direction = Direction.Right;
                else
                    _Direction = Direction.Left;

                Position = position2;
            }

            if (position.Y < 0 || (position.Y + Height) > Space.ScrHeight)
            {
                if (_Direction == Direction.Up)
                    _Direction = Direction.Down;
                else
                    _Direction = Direction.Up;

                Position = position2;
            }
        }
    }
}
