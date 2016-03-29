using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System.Diagnostics;
using System;
using System.Collections.Generic;
using System.Xml;

namespace StrategyGame
{
    /// <summary>
    /// This is the main type for your game.
    /// </summary>
    public class Game1 : Game
    {
        GraphicsDeviceManager graphics;
        SpriteBatch spriteBatch;
        GameState state;
        public static Texture2D triangle;
        public static Texture2D border;
        public static Texture2D animator;
        public static SpriteFont output;
        public static Texture2D background;
        public static Texture2D NPCs;
        public static Dictionary<string, Animation> animations = new Dictionary<string, Animation>();
        XmlDocument xmlReader = new XmlDocument();

        public Game1()
        {
            graphics = new GraphicsDeviceManager(this);
            graphics.PreferredBackBufferWidth = 890; //26
            graphics.PreferredBackBufferHeight = 924; //16
            graphics.IsFullScreen = false;
            graphics.ApplyChanges();
            Content.RootDirectory = "Content";
            this.IsMouseVisible = true;
        }

        /// <summary>
        /// Allows the game to perform any initialization it needs to before starting to run.
        /// This is where it can query for any required services and load any non-graphic
        /// related content.  Calling base.Initialize will enumerate through any components
        /// and initialize them as well.
        /// </summary>
        protected override void Initialize()
        {

            base.Initialize();
        }

        /// <summary>
        /// LoadContent will be called once per game and is the place to load
        /// all of your content.
        /// </summary>
        protected override void LoadContent()
        {
            triangle = this.Content.Load<Texture2D>("HexagonTriangleTile");
            border = this.Content.Load<Texture2D>("border");
            animator = this.Content.Load<Texture2D>("Animator");
            output = this.Content.Load<SpriteFont>("Output18pt");
            background = this.Content.Load<Texture2D>("background");
            NPCs = this.Content.Load<Texture2D>("NPCs");
            this.parseAnimations();
            this.state = new WorldMap();
            state.entering(this, xmlReader);
            spriteBatch = new SpriteBatch(GraphicsDevice);
            
        }

        protected override void UnloadContent()
        {
        }

        protected override void Update(GameTime gameTime)
        {
            state.update(gameTime, this);

            base.Update(gameTime);
        }

        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(new Color(251,242,211));
            state.draw(spriteBatch);

            base.Draw(gameTime);
        }
        public void changeState(string state)
        {
            this.state.leaving();
            switch(state)
            {
                case "WorldMap": { this.state = new WorldMap(); break; }
                case "LocalView": { this.state = new LocalView(); break; }
            }
            this.state.entering(this, xmlReader);
        }

        protected void parseAnimations()
        {

            xmlReader.Load("Content/animations.xml");
            foreach (XmlNode root in xmlReader.ChildNodes[1].ChildNodes)
            {
                animations[root.Attributes[0].Value] = new Animation(root.Attributes[0].Value);
                foreach (XmlNode node in root.ChildNodes)
                {
                    animations[root.Attributes[0].Value].addFrame((float)Convert.ToDouble(node.Attributes[0].Value),
                        (float)Convert.ToDouble(node.Attributes[1].Value),
                        (float)Convert.ToDouble(node.Attributes[2].Value),
                        Convert.ToInt16(node.Attributes[3].Value));

                }
            }
        }
    }
}
