using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;
using System.Xml;
using Microsoft.Xna.Framework.Input;
using System.Diagnostics;

namespace StrategyGame
{
    class LocalView : GameState
    {
        XmlDocument xmlReader = new XmlDocument();

        Vector2 ZERO = new Vector2(0, 0);

        Player player;
        List<LocalScenery> scenery = new List<LocalScenery>();
        List<Rectangle> treeRectangles = new List<Rectangle>();
        Rectangle tree1 = new Rectangle(0, 214, 180, 338);
        Rectangle tree2 = new Rectangle(201, 219, 88, 338);
        Rectangle tree3 = new Rectangle(294, 217, 95, 150);
        Rectangle tree4 = new Rectangle(398, 220, 64, 148);
        Rectangle chickenRect = new Rectangle(193, 3, 77, 104);
        Rectangle knightRect = new Rectangle(0,0,119,378);
        Rectangle monkRect = new Rectangle(131, 1, 175, 275);

        Rectangle flake1 = new Rectangle(276, 9, 8, 8);
        Rectangle flake2 = new Rectangle(287, 9, 8, 8);
        Rectangle flake3 = new Rectangle(298, 9, 8, 8);
        Rectangle flake4 = new Rectangle(276, 19, 8, 8);

        List<Rectangle> snow = new List<Rectangle>();

        List<Snowflake> flakes = new List<Snowflake>();

        List<NPC> chickens = new List<NPC>();
        List<NPC> NPCs = new List<NPC>();
        int line = 0;

        public static Random rand = new Random();

        int flakeTimer = 0;

        BasicEffect BasicTextureEffect;

        public LocalView()
        {

        }
        public override void entering(Game1 game, XmlDocument xmlReader)
        {
            player = new Player();
            this.tag = "LocalView";

            treeRectangles.Add(tree1);
            treeRectangles.Add(tree2);
            treeRectangles.Add(tree3);
            treeRectangles.Add(tree4);

            snow.Add(flake1);
            snow.Add(flake2);
            snow.Add(flake3);
            snow.Add(flake4);

            for (int i = 0; i < 5; i++)
            {
                chickens.Add(new NPC(chickenRect, new Vector2(rand.Next(-100, 600), 552), "chicken", 1, Game1.animations["chicken_Walk"], Game1.animations["chicken_Idle"]));
                NPCs.Add(new NPC(knightRect, new Vector2(rand.Next(-100, 600), 860), "knight", 0, Game1.animations["knight_Walk"], Game1.animations["knight_Idle"]));
                NPCs.Add(new NPC(monkRect, new Vector2(rand.Next(700, 1200), 900), "monk", 1, Game1.animations["monk_Walk"], Game1.animations["monk_Idle"]));
            }
            for (int i = 0; i < 2000; i++)
            {
                scenery.Add(new LocalScenery(treeRectangles[rand.Next(0, 3)], "tree", rand.Next(-10000, 10000), rand.Next(2, 20)));
            }
            scenery.Sort();
            scenery.Reverse();

        }

        public override void leaving()
        {
        }

        public override void update(GameTime gameTime, Game1 game)
        {
            if (flakeTimer < 18) { flakeTimer += gameTime.ElapsedGameTime.Milliseconds; }

            KeyboardState state = Keyboard.GetState();

            if (state.IsKeyDown(Keys.D) && player.moveRight(Game1.animations["main_Walk"])) { line += 2; }
            if (state.IsKeyDown(Keys.A) && player.moveLeft(Game1.animations["main_Walk"])) { line -= 2; }

            if (state.IsKeyDown(Keys.OemTilde)) { game.changeState("WorldMap"); }

            if (flakeTimer >= 18)
            {
                flakes.Add(new Snowflake(snow[rand.Next(0, 4)], new Vector2(rand.Next(-1000 + line, 1691 + line), -8)));
                flakeTimer = 0;
            }
            for (int i = 0; i < flakes.Count; i++)
            {
                flakes[i].update(gameTime);
                if (flakes[i].toBeDeleted) flakes.Remove(flakes[i]);
            }
            //Debug.Print("{0}", flakes.Count);
            foreach (NPC npc in chickens)
            {
                npc.update(gameTime, (int)player.horseVector.X + line);
            }
            foreach (NPC npc in NPCs)
            {
                npc.update(gameTime, (int)player.horseVector.X + line);
            }
            player.update(gameTime);

        }

        public override void draw(SpriteBatch spriteBatch)
        {
            /*
            Matrix matrix = Matrix.CreateOrthographicOffCenter(-graphics.GraphicsDevice.Viewport.Width / 2f, graphics.GraphicsDevice.Viewport.Width / 2f, -graphics.GraphicsDevice.Viewport.Height / 2f, graphics.GraphicsDevice.Viewport.Height / 2f, 1, 1);
            Matrix projection = Matrix.CreatePerspective(GraphicsDevice.Viewport.Width, GraphicsDevice.Viewport.Height, 1f, 2);
            Matrix halfPixelOffset = Matrix.CreateTranslation(-0.5f, -0.5f, 0);

            BasicEffect basicEffect = new BasicEffect(graphics.GraphicsDevice);
            basicEffect.World = Matrix.Identity;
            basicEffect.View = Matrix.Identity;
            basicEffect.Projection = halfPixelOffset * projection;

            basicEffect.TextureEnabled = true;
            basicEffect.VertexColorEnabled = true;

            spriteBatch.Begin(0, null, null, null, null, basicEffect);
            */
            spriteBatch.Begin();
            //spriteBatch.Draw(background, ZERO, horseSprite, Color.White, animRotation, ZERO, new Vector2(1, 1), SpriteEffects.None, 0f);
            spriteBatch.Draw(Game1.background, new Rectangle(0, 0, 890, 924), Color.White);
            
            foreach (LocalScenery prop in scenery)
            {
                if (prop.getVector(line).X > 0 - prop.sprite.Width && prop.getVector(line).X < 890)
                    spriteBatch.Draw(Game1.animator, prop.getVector(line), prop.sprite, Color.White, prop.rotation, ZERO, prop.sizeVector, SpriteEffects.None, 0f);
            }

            //draw chicken
            foreach (NPC chicken in chickens)
            {
                if (chicken.flipped)
                    spriteBatch.Draw(Game1.animator, new Vector2(chicken.location.X - line, chicken.location.Y) + chicken.adjustVector, chicken.sprite, Color.White, chicken.adjustRot, new Vector2(chickenRect.Width / 2, chickenRect.Height / 2), new Vector2(1, 1), SpriteEffects.FlipHorizontally, 0f);
                else
                    spriteBatch.Draw(Game1.animator, new Vector2(chicken.location.X - line, chicken.location.Y) + chicken.adjustVector, chicken.sprite, Color.White, chicken.adjustRot, new Vector2(chickenRect.Width / 2, chickenRect.Height / 2), new Vector2(1, 1), SpriteEffects.None, 0f);
            }
            //draw knight
            Vector2 knightScale = new Vector2(.6f, .6f);
            foreach (NPC npc in NPCs)
            {
                if (npc.flipped)
                    spriteBatch.Draw(Game1.NPCs, (new Vector2(npc.location.X - line, npc.location.Y) + npc.adjustVector) * new Vector2(1, knightScale.Y), npc.sprite, Color.White, npc.adjustRot, new Vector2(knightRect.Width / 2, knightRect.Height / 2) * knightScale, knightScale, SpriteEffects.FlipHorizontally, 0f);
                else
                    spriteBatch.Draw(Game1.NPCs, (new Vector2(npc.location.X - line, npc.location.Y) + npc.adjustVector) * new Vector2(1, knightScale.Y), npc.sprite, Color.White, npc.adjustRot, new Vector2(knightRect.Width / 2, knightRect.Height / 2) * knightScale, knightScale, SpriteEffects.None, 0f);
            }
            //draw player - create drawList
            if (player.flipped)
            {
                //horse
                spriteBatch.Draw(Game1.animator, player.horseVector + player.adjustVector, player.horseSprite, 
                    Color.White, player.animationRot + player.adjustRot, 
                    new Vector2(player.horseSprite.Width / 2, player.horseSprite.Height / 2), 
                    new Vector2(1, 1), SpriteEffects.FlipHorizontally, 0f);
                //sword
                spriteBatch.Draw(Game1.animator, player.getSwordLocation(Mouse.GetState().Position.ToVector2()),
                    player.sword, Color.White, player.animationRot + player.adjustRot + player.getAngleFromMouse(Mouse.GetState().Position.ToVector2()) - (float)Math.PI / 2 - .4f,
                    new Vector2(0,player.sword.Height),
                    new Vector2(1, 1), SpriteEffects.FlipVertically, 0f);
                //arm
                spriteBatch.Draw(Game1.animator, player.getArmLocation(),
                    player.arm, Color.White, player.animationRot + player.adjustRot + player.getAngleFromMouse(Mouse.GetState().Position.ToVector2()) + (float)Math.PI,
                    new Vector2(0,player.arm.Height),
                    new Vector2(1, 1), SpriteEffects.FlipVertically, 0f); 
                //cloak
                spriteBatch.Draw(Game1.animator, player.horseVector + player.adjustVector, 
                    player.cloakOver, Color.White, player.animationRot + player.adjustRot, 
                    new Vector2(player.horseSprite.Width / 2, player.horseSprite.Height / 2) - player.cloakOverVector + new Vector2(12, 0), 
                    new Vector2(1, 1), SpriteEffects.FlipHorizontally, 0f);
            }
            else
            {
                //horse
                spriteBatch.Draw(Game1.animator, player.horseVector + player.adjustVector,
                    player.horseSprite, Color.White, player.animationRot + player.adjustRot, 
                    new Vector2(player.horseSprite.Width / 2, player.horseSprite.Height / 2), 
                    new Vector2(1, 1), SpriteEffects.None, 0f);
                //sword
                spriteBatch.Draw(Game1.animator, player.getSwordLocation(Mouse.GetState().Position.ToVector2()),
                    player.sword, Color.White, player.animationRot + player.adjustRot + player.getAngleFromMouse(Mouse.GetState().Position.ToVector2()) - (float)Math.PI/2 - .4f, 
                    Vector2.Zero, 
                    new Vector2(1, 1), SpriteEffects.None, 0f);
                //arm
                spriteBatch.Draw(Game1.animator, player.getArmLocation(),
                    player.arm, Color.White, player.animationRot + player.adjustRot + player.getAngleFromMouse(Mouse.GetState().Position.ToVector2()), 
                    Vector2.Zero, 
                    new Vector2(1, 1), SpriteEffects.None, 0f);
                //cloak
                spriteBatch.Draw(Game1.animator, player.horseVector + player.adjustVector,
                    player.cloakOver, Color.White, player.animationRot + player.adjustRot, 
                    new Vector2(player.horseSprite.Width / 2, player.horseSprite.Height / 2) - player.cloakOverVector, 
                    new Vector2(1, 1), SpriteEffects.None, 0f);
            }
            foreach (Snowflake flake in flakes)
            {
                if (flake.location.X - line > 0 && flake.location.X - line < 890)
                    spriteBatch.Draw(Game1.animator, new Vector2(flake.location.X - line, flake.location.Y), flake.sprite, Color.White, flake.rotation, new Vector2(4, 4), new Vector2(1, 1), SpriteEffects.None, 0f);
            }

            spriteBatch.Draw(Game1.border, new Rectangle(0, 0, 890, 924), Color.White);

            spriteBatch.DrawString(Game1.output, string.Format("Line: {0}\nPlayer: {1}\nArm Angle: {2}", line, player.horseVector.X + line, player.animationRot + player.adjustRot + player.getAngleFromMouse(Mouse.GetState().Position.ToVector2())), ZERO, Color.Black);

            spriteBatch.End();
        }
    }

    class LocalScenery : IComparable<LocalScenery>
    {
        public int distance = 1;
        public int XLocal = 0;
        public Rectangle sprite;
        public string tag;
        public float rotation = 0f;
        public Vector2 sizeVector = new Vector2(1, 1);

        public LocalScenery(Rectangle sprite, string tag, int XLocal, int distance)
        {
            this.sprite = sprite;
            this.tag = tag;
            this.XLocal = XLocal;
            this.distance = distance;
            this.sizeVector = new Vector2(MathHelper.Clamp(1.0f / distance + .5f, .2f, 1f), MathHelper.Clamp(1.0f / distance + .5f, .2f, 1f));
        }

        public Vector2 getVector(int line)
        {
            return new Vector2((((this.XLocal * distance) - line) / distance), 540 - ((int)Math.Sqrt(distance) * distance) + (this.sprite.Height * (1 - this.sizeVector.X)) - this.sprite.Height);
        }

        public int CompareTo(LocalScenery other)
        {
            return this.distance.CompareTo(other.distance);
        }
    }

    class Player
    {

        public Rectangle horseSprite = new Rectangle(0, 0, 180, 179);
        public Vector2 horseVector = new Vector2(500, 590);
        Animation currentAnimation;
        public bool animated = false;
        public float animationRot = 0f;
        public Vector2 adjustVector = new Vector2(0, 0);
        public float adjustRot = 0f;
        public Rectangle cloakOver = new Rectangle(318, 2, 23, 39);
        public Vector2 cloakOverVector = new Vector2(85, 29);
        public Rectangle arm = new Rectangle(278, 45, 59, 30);
        public Rectangle sword = new Rectangle(346, 0, 23, 129);

        int timeSinceLastFrame = 0;
        int frameIndex = 0;

        int speed = 3;

        public bool flipped = false;

        public void startAnimation(Animation animation)
        {
            animated = true;
            this.currentAnimation = animation;
        }

        public bool moveRight(Animation animation)
        {
            if (flipped) { this.flipped = false; }
            if (!animated) { this.startAnimation(animation); }

            if (this.horseVector.X < 500)
            {
                this.horseVector.X += speed;
                return false;
            }
            return true;
        }
        public bool moveLeft(Animation animation)
        {
            if (!flipped) { this.flipped = true; }
            if (!animated) { this.startAnimation(animation); }

            if (this.horseVector.X > 400)
            {
                this.horseVector.X -= speed;
                return false;
            }
            return true;
        }
        public Vector2 getArmLocation()
        {
            Vector2 center = horseVector + adjustVector;
            if (this.flipped) return Animation.RotateAboutOrigin(center + new Vector2(-4, -52), center, this.animationRot + this.adjustRot);
            return Animation.RotateAboutOrigin(center + new Vector2(2, -48), center, this.animationRot + this.adjustRot);
        }
        public Vector2 getSwordLocation(Vector2 mouseLocation)
        {
            Vector2 arm = this.getArmLocation();
            if (this.flipped) return Animation.RotateAboutOrigin(arm + new Vector2(40, 38), arm, getAngleFromMouse(mouseLocation) + this.animationRot + this.adjustRot);
            return Animation.RotateAboutOrigin(arm + new Vector2(40, 38),arm,getAngleFromMouse(mouseLocation) + this.animationRot + this.adjustRot);

        }
        public float getAngleFromMouse(Vector2 mouseLocation)
        {
            Vector2 center = this.horseVector + this.adjustVector + new Vector2(0, -15);
            float adjust = (float)Math.Atan2(mouseLocation.Y - center.Y, mouseLocation.X - center.X);
            if (this.flipped) return (MathHelper.Clamp(Math.Abs(adjust) - (float)Math.PI, (float)-Math.PI/4, (float)Math.PI/6) * (Math.Abs(adjust)/adjust));
            return MathHelper.Clamp(adjust,(float)-Math.PI/4,(float)Math.PI/6);
        }

        public void update(GameTime gameTime)
        {
            //Debug.Print("{0},{1}", adjustRot, adjustVector);
            if (animated)
            {
                timeSinceLastFrame += gameTime.ElapsedGameTime.Milliseconds;
                if (currentAnimation.getLocation(timeSinceLastFrame, frameIndex, this.flipped, this.horseSprite, out adjustVector, out adjustRot))
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
    }

    class Snowflake
    {
        public Rectangle sprite;
        public Vector2 location;
        public bool toBeDeleted;
        public int time = 0;
        public float rotation = 0f;

        public Snowflake(Rectangle sprite, Vector2 location)
        {
            this.sprite = sprite;
            this.location = location;
        }
        public void update(GameTime gameTime)
        {
            this.time += gameTime.ElapsedGameTime.Milliseconds;
            //this.rotation += time / 220;
            this.location.Y += .8f;
            this.location.X += (float)Math.Sqrt(1 + Math.Sin(time / 120));
            if (location.Y > 924) { toBeDeleted = true; }
        }
    }

    class NPC
    {
        public Rectangle sprite;
        public Vector2 location;
        public Vector2 origin;
        Animation currentAnimation;
        AI currentAI;
        public bool animated = false;
        string tag;
        public bool flipped = false;
        int speed = 2;
        int playerLine;

        public int timeSinceLastFrame = 0;
        public int frameIndex = 0;
        public Vector2 adjustVector;
        public float adjustRot;
        Dictionary<string, Animation> animations = new Dictionary<string, Animation>();

        List<animate> movements = new List<animate>();
        animate currentMovement;

        int idleTimer = 0;

        public Random rand = new Random(Guid.NewGuid().GetHashCode());

        public NPC(Rectangle sprite, Vector2 location, string tag, byte AI, params Animation[] anims)
        {
            this.sprite = sprite;
            this.location = location;
            this.origin = new Vector2(0, 0) + location;
            this.tag = tag;

            movements.Add(new animate(this.moveLeft));
            movements.Add(new animate(this.moveRight));
            movements.Add(new animate(this.idle));

            foreach (Animation animation in anims)
            {
                animations.Add(animation.tag, animation);
            }
            switch (AI)
            {
                case 0: { this.currentAI = idleAI; break; }
                case 1: { this.currentAI = avoidAI; this.speed = 1; break; }
            }
        }
        public void startAnimation(Animation animation)
        {
            this.currentAnimation = animation;
            animated = true;

        }

        public void idleAI(GameTime gameTime)
        {
            if (idleTimer < 3500) { idleTimer += gameTime.ElapsedGameTime.Milliseconds + rand.Next(-40, 60); }
            if (!animated && idleTimer >= 3500)
            {
                currentMovement = this.movements[rand.Next(0, this.movements.Count)];
                currentMovement();
                idleTimer = 0;
            }
        }
        public void avoidAI(GameTime gameTime)
        {
            if (!animated && this.location.X - playerLine < 200 && this.location.X - playerLine >= 0) { currentMovement = this.moveRight; currentMovement(); }
            else if (!animated && this.location.X - playerLine > -200 && this.location.X - playerLine < 0) { currentMovement = this.moveLeft; currentMovement(); }
            else 
            {
                idleAI(gameTime);
            }
        }

        public void update(GameTime gameTime, int playerLine)
        {
            this.playerLine = playerLine;
            this.currentAI(gameTime);
            //Debug.Print("{0},{1},{2}", this.flipped, this.animated, this.location);
            if (animated)
            {
                currentMovement();
                timeSinceLastFrame += gameTime.ElapsedGameTime.Milliseconds;
                if (currentAnimation.getLocation(timeSinceLastFrame, frameIndex, this.flipped, this.sprite, out adjustVector, out adjustRot))
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
        public void moveLeft()
        {
            if (!flipped) { this.flipped = true; }
            if (!animated) { this.startAnimation(animations[this.tag + "_Walk"]); }
            this.location.X -= speed;
        }
        public void moveRight()
        {
            if (flipped) { this.flipped = false; }
            if (!animated) { this.startAnimation(animations[this.tag + "_Walk"]); }
            this.location.X += speed;
        }
        public void idle()
        {
            if (!animated) { this.startAnimation(animations[this.tag + "_Idle"]); }
        }

        public delegate void animate();
        public delegate void AI(GameTime gameTime);
    }
}
