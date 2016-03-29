using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using System.Diagnostics;

namespace StrategyGame
{
    public class Unit : Prop
    {
        public Tile tile;
        public Hex destinationHex;
        public Tile destinationTile;
        public Animation currentAnimation;
        public bool animated = false;
        public bool moving = false;
        int speed = 2;
        public Stack<Vector2> path;

        public int timeSinceLastFrame = 0;
        public int frameIndex = 0;
        public Vector2 animateAdjustVector;
        public Vector2 moveAdjustVector;
        Dictionary<string, Animation> animations = new Dictionary<string, Animation>();

        List<animate> movements = new List<animate>();
        animate currentMovement;

        int idleTimer = 0;
        int timeToNextTile = 0;

        public Random rand = new Random(Guid.NewGuid().GetHashCode());

        public Unit(Rectangle sprite, Hex hex, Tile tile, Vector2 scale, string tag, params Animation[] anims) : 
            base(hex, tile.coords * new Vector2(78,129) - new Vector2(sprite.Width * scale.X + 39, sprite.Height * scale.Y + 64) , 0f, sprite, scale,false,tag)
        {
            this.tile = tile;
            tile.occupy(this);

            movements.Add(new animate(this.moveLeft));
            movements.Add(new animate(this.moveRight));
            movements.Add(new animate(this.idle));

            foreach (Animation animation in anims)
            {
                animations.Add(animation.tag, animation);
            }
            
            
        }
        public void update(GameTime gameTime)
        {
            //Debug.Print("{0}", this.tile.coords + this.hex.coords * new Vector2(3, 2));
            if(moving)
            {
                this.timeToNextTile = MathHelper.Clamp(timeToNextTile + gameTime.ElapsedGameTime.Milliseconds,0,1020);
                this.moveAdjustVector = Vector2.Lerp(this.getAbsoluteCoords(), this.getDestinationCoords(), timeToNextTile/1020.0f) - this.getAbsoluteCoords();
                

                if (timeToNextTile == 1020)
                {
                    this.tile.deoccupy();
                    this.hex = destinationHex;
                    this.tile = destinationTile;
                    destinationHex = null;
                    destinationTile = null;
                    tile.occupy(this);
                    this.timeToNextTile = 0;
                    this.moving = false;
                    this.moveAdjustVector = Vector2.Zero;
                    if (this.path.Count > 0) 
                    {
                        moveNext();
                    }
                }
                if (!animated) { currentMovement(); }
            }
            if (animated)
            {
                currentMovement();
                timeSinceLastFrame += gameTime.ElapsedGameTime.Milliseconds;
                if (currentAnimation.getLocation(timeSinceLastFrame, frameIndex, this.flipped, this.sprite, out animateAdjustVector, out rotation))
                {
                    timeSinceLastFrame = 0;
                    frameIndex++;
                    if (frameIndex >= currentAnimation.Count() - 1)
                    {
                        animated = false;
                        frameIndex = 0;
                    }
                }
            }
        }

        public void setPath(Hex destHex, Tile destTile)
        {
            this.path = WorldMap.AstarSearch(hex.coords * new Vector2(3, 2) + tile.coords,
                destHex.coords * new Vector2(3,2) + destTile.coords);
            moveNext();
        }

        public void moveNext()
        {
            this.moving = true;
            /*!!!Conversion between cartesian grid and triangle grid is broken
             * make function to convert between cartesian coordinate and hex/tile coordinate
             * subtract 1 from the Y value for each odd X coordinate hex
             * Involve this in the path finding so negative Y values don't get chosen
             */
            Vector2 destination = path.Pop();
            Debug.Print("{0}, Hex: {1}", destination, new Vector2((int)destination.X / 3, (int)destination.Y / 2));
            this.destinationHex = WorldMap.getHex(new Vector2((int)destination.X / 3, (int)destination.Y /2));
            this.destinationTile = destinationHex.tiles[(int)destination.X % 3, (int)destination.Y % 2];

            if (destinationHex.getAbsoluteCoords().X + destinationTile.coords.X * 78 < hex.getAbsoluteCoords().X + tile.coords.X * 78)
            {
                currentMovement = this.moveLeft;
            }
            else { currentMovement = this.moveRight; }
        }
        public override float getDrawDepth() //!!! not relaying the proper depth
        {
            return (this.getAbsoluteCoords().Y + this.animateAdjustVector.Y + this.moveAdjustVector.Y + (this.sprite.Height) * this.scale.Y) / (924f);
        }

        public Vector2 getAbsoluteCoords()
        {
            return this.hex.getAbsoluteCoords() + this.tile.coords * new Vector2(78, 129);
        }
        public Vector2 getDestinationCoords()
        {
            return destinationHex.getAbsoluteCoords() + destinationTile.coords * new Vector2(78, 129) + new Vector2(0, (1 + (-2 *((destinationTile.coords.X + destinationTile.coords.Y) % 2))) * (129 / 3));
        }

        public void startAnimation(Animation animation)
        {
            this.currentAnimation = animation;
            animated = true;

        }
        public void moveLeft()
        {
            if (!flipped) { this.flipped = true; }
            if (!animated) { this.startAnimation(animations[this.tag + "_Walk"]); }
        }
        public void moveRight()
        {
            if (flipped) { this.flipped = false; }
            if (!animated) { this.startAnimation(animations[this.tag + "_Walk"]); }
        }
        public void idle()
        {
            if (!animated) { this.startAnimation(animations[this.tag + "_Idle"]); }
        }
        public delegate void animate();

    }

    public class Animation
    {
        public List<Frame> frames = new List<Frame>();
        public string tag;

        public Animation(string tag) { this.tag = tag; }

        public void addFrame(float X, float Y, float rotation, int time)
        {
            frames.Add(new Frame(X, Y, rotation, time));

        }
        public Frame getFrame(int index) { return this.frames[index]; }
        public int frameTime(int index)
        {
            return frames[index].time;
        }
        public int Count() { return frames.Count; }

        public bool getLocation(int timeSinceLastFrame, int frameindex, bool flipped, Rectangle sprite, out Vector2 location, out float rotation)
        {
            float flip = (flipped) ? -1 : 1;
            if (timeSinceLastFrame > frames[frameindex].time)
            {
                location = frames[frameindex + 1].location * new Vector2(flip, 1);
                rotation = frames[frameindex + 1].rotation * flip;
                //Debug.Print("frame - {0} : {1}, {2}", flipped, location, rotation);
                /*if (flipped)
                {
                    Vector2 topRight = RotateAboutOrigin(location, new Vector2(location.X + sprite.Width, location.Y), rotation);
                    rotation = rotation * -1;
                    location = RotateAboutOrigin(topRight, location, rotation * 2);
                }*/
                //rotation = 0f;
                //location = new Vector2(0, 0);
                return true;
            }

            float percentThrough = (float)timeSinceLastFrame / (frames[frameindex + 1].time);

            float newX = (frames[frameindex + 1].location.X - frames[frameindex].location.X) * percentThrough;
            float newY = (frames[frameindex + 1].location.Y - frames[frameindex].location.Y) * percentThrough;
            location = new Vector2(newX + frames[frameindex].location.X, newY + frames[frameindex].location.Y) * new Vector2(flip, 1);
            rotation = (((frames[frameindex + 1].rotation - frames[frameindex].rotation) * percentThrough) + (frames[frameindex].rotation)) * flip;
            /*if (flipped)
            {
                Vector2 topRight = RotateAboutOrigin(location, new Vector2(location.X + sprite.Width, location.Y), rotation);
                rotation = rotation * -1;
                location = RotateAboutOrigin(topRight, location, rotation * 2);
            }*/
            //rotation = 0f;
            //Debug.Print("interframe - {0} : {1}, {2}", flipped, location, rotation);
            //location = new Vector2(0, 0);
            return false;
        }
        public static Vector2 RotateAboutOrigin(Vector2 point, Vector2 origin, float rotation)
        {
            return Vector2.Transform(point - origin, Matrix.CreateRotationZ(rotation)) + origin;
        }
    }
    public class Frame
    {
        public Vector2 location;
        public float rotation;
        public int time;

        public Frame(float X, float Y, float rotation, int time)
        {
            this.location = new Vector2(X, Y);
            this.rotation = rotation;
            this.time = time;
        }
    }

}
