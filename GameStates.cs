using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Graphics;
using System.Xml;
using System.Diagnostics;

namespace StrategyGame
{

    class WorldMap : GameState
    {

        bool debugging = false;

        Vector2 lastMouse;

        Vector2 closest;
        Vector2 closestCenter;
        double distanceToCenter;
        double degreeFromCenter;

        Vector2 currentMouse;
        public static int XBetweenTiles = 78;
        public static int YBetweenTiles = 129;

        Vector2 TileOver;

        DebugList debugL = new DebugList();

        Vector2 Vector = new Vector2(0, 0);
        //Tile[,] tiles = new Tile[100, 100];
        static Hex[,] hexes = new Hex[50, 35];
        List<HexType> hexTypes = new List<HexType>();

        Vector2 currentCorner = new Vector2(0, 0);


        Unit selectedUnit;
        bool dragged = false;
        bool LMBWasHeld = false;
        bool RMBWasHeld = false;

        Dictionary<Prop, Vector2> props = new Dictionary<Prop, Vector2>();
        List<Unit> units = new List<Unit>();

        Random rand = new Random();
        delegate void process(Vector2 pos, Vector2 current);

        List<Vector2> pathFromSelected;
        Vector2 lastVectorOver = new Vector2(0,0);

        public WorldMap()
        {
            this.tag = "WorldMap";
        }
        public override void entering(Game1 game, XmlDocument xmlReader)
        {
            currentMouse = new Vector2(Mouse.GetState().X, Mouse.GetState().Y);
            lastMouse = new Vector2(Mouse.GetState().X, Mouse.GetState().Y);

            this.parseProps(xmlReader);
            xmlReader.RemoveAll();


            for (int i = 0; i < hexes.GetLength(0); i++)
            {
                for (int j = 0; j < hexes.GetLength(1); j++)
                {
                    HexType type = (i + j == 0) ? hexTypes[1] : hexTypes[0];
                    hexes[i, j] = new Hex(new Vector2(i, j), type);
                    for (int k = 0; k < hexes[i, j].tiles.GetLength(0); k++)
                    {
                        for (int l = 0; l < hexes[i, j].tiles.GetLength(1); l++)
                        {
                            hexes[i, j].tiles[k, l] = new Tile(new Vector2(k, l));
                        }
                    }
                }
            }

            units.Add(new Unit(new Rectangle(0, 0, 180, 179), hexes[0, 0], hexes[0, 0].tiles[1, 0], new Vector2(.6f, .6f), "main", Game1.animations["main_Walk"]));

        }

        public override void leaving()
        {
        }

        public override void update(GameTime gameTime, Game1 game)
        {

            this.currentMouse = new Vector2(Mouse.GetState().X, Mouse.GetState().Y);

            KeyboardState state = Keyboard.GetState();

            if (state.IsKeyDown(Keys.Escape)) { game.changeState("LocalView"); }

            if (debugging)
            {
                Vector2 randVector = new Vector2(rand.Next(0, 890), rand.Next(0, 924));
                this.getTileOver(this.getClosestHex(randVector + currentCorner), randVector + currentCorner);
                debugL.update((closest * new Vector2(10, 10)) + TileOver, randVector);
            }
            else
            {
                this.getTileOver(this.getClosestHex(currentMouse + currentCorner), currentMouse + currentCorner);
            }

            int shiftOn = 1;
            if (state.IsKeyDown(Keys.LeftShift)) { shiftOn = 8; }
            if (state.IsKeyDown(Keys.W)) { currentCorner.Y -= shiftOn; }
            if (state.IsKeyDown(Keys.A)) { currentCorner.X -= shiftOn; }
            if (state.IsKeyDown(Keys.D)) { currentCorner.X += shiftOn; }
            if (state.IsKeyDown(Keys.S)) { currentCorner.Y += shiftOn; }


            if (Mouse.GetState().LeftButton == ButtonState.Pressed)
            {
                LMBWasHeld = true;
                currentCorner -= currentMouse - lastMouse;
                if (Math.Abs(currentMouse.X - lastMouse.X) > 1 && Math.Abs(currentMouse.Y - lastMouse.Y) > 1)
                {
                    dragged = true;
                }
            }
            else if (LMBWasHeld)
            {
                LMBWasHeld = false;
                if (dragged) { dragged = false; }
                else if (hexes[(int)closest.X, (int)closest.Y].tiles[(int)TileOver.X, (int)TileOver.Y].isOccupied())
                    selectedUnit = hexes[(int)closest.X, (int)closest.Y].tiles[(int)TileOver.X, (int)TileOver.Y].occupied;
                else selectedUnit = null;
            }

            if (Mouse.GetState().RightButton == ButtonState.Pressed)
            {
                RMBWasHeld = true;
            }
            else if (RMBWasHeld)
            {
                RMBWasHeld = false;
                if (selectedUnit != null)
                {
                    Hex destinationHex = hexes[(int)closest.X, (int)closest.Y];
                    Tile destinationTile = destinationHex.tiles[(int)TileOver.X, (int)TileOver.Y];
                    if (!destinationTile.isOccupied()) { selectedUnit.setPath(destinationHex, destinationTile); }
                }
            }
            lastMouse = new Vector2(currentMouse.X, currentMouse.Y);


            props.Clear();
            for (int i = 0; i < 5; i++)
            {
                for (int j = 0; j < 5; j++)
                {
                    Hex hex = hexes[i + MathHelper.Clamp((int)(currentCorner.X / (XBetweenTiles * 3)), 0, hexes.GetLength(0)),
                           j + MathHelper.Clamp((int)(currentCorner.Y / (YBetweenTiles * 2)), 0, hexes.GetLength(1))];
                    hex.update(gameTime);
                    foreach (Prop prop in hex.type.props)
                    {
                        Prop nProp = new Scenery(hex, hex.getAbsoluteCoords() + prop.coords, prop.rotation, hex.type.sprites[prop.tag], prop.scale, prop.flat, prop.tag);
                        if (onScreen(new Rectangle((int)nProp.coords.X, (int)nProp.coords.Y, (int)(nProp.sprite.Width * nProp.scale.X), (int)(nProp.sprite.Height * nProp.scale.Y))))
                            props[nProp] = hex.getAbsoluteCoords() + prop.coords;
                    }

                }
            }
            foreach (Unit unit in units)
            {
                unit.update(gameTime);
                Vector2 location = unit.hex.getAbsoluteCoords() +
                    new Vector2((float)XBetweenTiles / 3,
                    -1 * ((unit.tile.coords.X + unit.tile.coords.Y) % 2) * (YBetweenTiles / 3))
                    + unit.tile.coords * new Vector2(XBetweenTiles, YBetweenTiles)
                    + (unit.animateAdjustVector * unit.scale)
                    + unit.moveAdjustVector;
                if (this.onScreen(new Rectangle((int)location.X, (int)location.Y, (int)(unit.sprite.Width * unit.scale.X), (int)(unit.sprite.Height * unit.scale.Y))))
                {
                    props[unit] = location;
                }
            }


            if (selectedUnit != null && !(TileOver + new Vector2(closest.X * 3, closest.Y * 2)).Equals(lastVectorOver))
            {
                lastVectorOver = (TileOver + new Vector2(closest.X * 3, closest.Y * 2));
                pathFromSelected = AstarSearch(selectedUnit.tile.coords + selectedUnit.hex.coords * new Vector2(3, 2), lastVectorOver).ToList();
            }


        }

        public override void draw(SpriteBatch spriteBatch)
        {
            spriteBatch.Begin();
            //Debug.Print("=============================");
            Vector2 cornerHexRelative = new Vector2(MathHelper.Clamp((int)(currentCorner.X / (XBetweenTiles * 3)), 0, hexes.GetLength(0)),
                        MathHelper.Clamp((int)(currentCorner.Y / (YBetweenTiles * 2)), 0, hexes.GetLength(1)));

            List<Prop> visible = new List<Prop>(props.Keys);
            visible.Sort();

            for (int i = 0; i < 5; i++)
            {
                for (int j = 0; j < 5; j++)
                {
                    Color color = Color.White;
                    //Debug.Print("{0} : {1}",hexes[i,j].coords,hexes[i, j].center);
                    Hex hex = hexes[i + MathHelper.Clamp((int)(currentCorner.X / (XBetweenTiles * 3)), 0, hexes.GetLength(0)),
                        j + MathHelper.Clamp((int)(currentCorner.Y / (YBetweenTiles * 2)), 0, hexes.GetLength(1))];
                    //!!! FIX

                    //Debug.Print("{0},{1}", (int)(currentCorner.X / (XBetweenTiles * 3)), (int)(currentCorner.Y / (YBetweenTiles * 2)));

                    foreach (Tile tile in hex.tiles)
                    {
                        if (hex.coords == closest && TileOver.Equals(tile.coords)) { color = Color.BlanchedAlmond; }
                        else { color = Color.White; }

                        if (selectedUnit != null && pathFromSelected.Contains((tile.coords + new Vector2(hex.coords.X * 3, hex.coords.Y * 2)))) { color = Color.Red; }

                        SpriteEffects effect = ((tile.coords.X + tile.coords.Y) % 2 == 0) ? SpriteEffects.FlipVertically : SpriteEffects.None;
                        spriteBatch.Draw(Game1.triangle,
                            //new Vector2(
                            //    (tile.coords.X + (hex.coords.X * 3)) * XBetweenTiles, 
                            //    (tile.coords.Y  + ((hex.coords.X%2) + hex.coords.Y*2)) * YBetweenTiles) - currentCorner, 
                            hex.getAbsoluteCoords() + (tile.coords * new Vector2(XBetweenTiles, YBetweenTiles)) - currentCorner,
                                tile.sprite, color, 0f, Vector2.Zero, new Vector2(.5f, .5f), effect, 0f);
                    }
                    /*
                    foreach (Prop prop in hex.type.props)
                    {
                        spriteBatch.Draw(animator,
                               prop.coords + hex.getAbsoluteCoords() - currentCorner,
                                   hex.type.sprites[prop.tag], Color.White, 0f, ZERO, prop.scale, SpriteEffects.None, 0f);
                    }*/
                }
            }


            foreach (Prop prop in visible)
            {
                Color color = (prop.Equals(selectedUnit)) ? Color.BlanchedAlmond : Color.White;
                SpriteEffects effect = (prop.flipped) ? SpriteEffects.FlipHorizontally : SpriteEffects.None;
                spriteBatch.Draw(Game1.animator,
                            props[prop] - currentCorner,
                                prop.sprite, color, prop.rotation, Vector2.Zero, prop.scale, effect, 0f);
            }

            foreach (Tuple<Vector2, Color> tuple in debugL.dots)
            {
                spriteBatch.DrawString(Game1.output, "#", tuple.Item1 - currentCorner - new Vector2(3, 3), tuple.Item2);
            }

            spriteBatch.Draw(Game1.border, new Rectangle(0, 0, 890, 924), Color.White);

            spriteBatch.DrawString(Game1.output, "Mouse Location: " + (currentMouse + currentCorner), Vector2.Zero, Color.Black);
            spriteBatch.DrawString(Game1.output, "Closest Hex: " + closest, new Vector2(0, 16), Color.Black);
            spriteBatch.DrawString(Game1.output, "HexAbsoluteCoords: " + hexes[(int)closest.X, (int)closest.Y].getAbsoluteCoords(), new Vector2(0, 32), Color.Black);
            spriteBatch.DrawString(Game1.output, "Distance: " + distanceToCenter, new Vector2(0, 48), Color.Black);
            spriteBatch.DrawString(Game1.output, "Hex Corner: " + cornerHexRelative, new Vector2(0, 64), Color.Black);
            spriteBatch.DrawString(Game1.output, "TileOver: " + (TileOver + new Vector2(closest.X * 3, closest.Y * 2)) + " Occupied: " + hexes[(int)closest.X, (int)closest.Y].tiles[(int)TileOver.X, (int)TileOver.Y].isOccupied(), new Vector2(0, 80), Color.Black);
            spriteBatch.DrawString(Game1.output, "Visible Props: " + visible.Count, new Vector2(0, 96), Color.Black);

            int count = 0;
            foreach (Vector2 neighbor in getNeighbors(TileOver + new Vector2(closest.X * 3, closest.Y * 2)))
            {
                spriteBatch.DrawString(Game1.output, "Neighbors: " + neighbor, new Vector2(0, 112 + 16 * count), Color.Black);
                count++;
            }

            spriteBatch.End();
        }

        public bool onScreen(Rectangle rect)
        {
            if (rect.X < currentCorner.X - (rect.Width) || rect.X > currentCorner.X + (rect.Width) + 890) { return false; }
            if (rect.Y < currentCorner.Y - (rect.Height) || rect.Y > currentCorner.Y + (rect.Height) + 924) { return false; }
            return true;
        }

        public void getTileOver(Hex hexOver, Vector2 mouseLocation)
        {
            //mouselocation is absolute

            Vector2 axis = new Vector2(1, 0);
            double distance = Math.Sqrt(Math.Pow(mouseLocation.X - hexOver.center.X, 2) + Math.Pow(mouseLocation.Y - hexOver.center.Y, 2));
            double degree = Math.Atan2((mouseLocation.X - hexOver.center.X), (mouseLocation.Y - hexOver.center.Y));

            distanceToCenter = distance;
            degreeFromCenter = degree * (180 / Math.PI);

            if (Math.Abs(degreeFromCenter) <= 30)
            {
                TileOver = new Vector2(1, 1);
            }
            else if (Math.Abs(degreeFromCenter) <= 90)
            {
                TileOver = new Vector2(1 + (int)(degreeFromCenter / Math.Abs(degreeFromCenter)), 1);
            }
            else if (Math.Abs(degreeFromCenter) <= 150)
            {
                TileOver = new Vector2(1 + (int)(degreeFromCenter / Math.Abs(degreeFromCenter)), 0);
            }
            else
            {
                TileOver = new Vector2(1, 0);
            }
        }

        public Hex getClosestHex(Vector2 mouseLocation)
        {
            //mouselocation is absolute

            //Debug.Print("{0}", mouseLocation);

            Vector2 absolute = mouseLocation;
            Hex best = hexes[0, 0];
            //!!!
            /*
            float XLengths = (absolute.X - absolute.X % XBetweenTiles) / XBetweenTiles;
            float YLengths = (absolute.Y - absolute.Y % YBetweenTiles) / YBetweenTiles;
            Debug.Print("{0},{1}", XLengths, YLengths);
            for (int i = 0; i < 3; i++)
            {
                for (int j = 0; j < 3; j++)
                {
                    Hex current = hexes[MathHelper.Clamp((int)((XLengths) / 3) + i,0,hexes.GetLength(0)),
                        MathHelper.Clamp((int) ((YLengths)/2) + j,0,hexes.GetLength(1))];
                    if(best.distance(absolute) > current.distance(absolute))
                    {
                        best = current;
                    }
                }
            }*/

            foreach (Hex hex in hexes)
            {
                if (best.distance(absolute) > hex.distance(absolute))
                {
                    best = hex;
                }
            }

            this.closest = best.coords;
            this.closestCenter = best.center;
            return best;
        }

        public static Hex getHex(Vector2 coords) { return hexes[(int)coords.X, (int)coords.Y]; }

        public void parseProps(XmlDocument xmlReader)
        {
            xmlReader.Load("Content/TileProps.xml");
            foreach (XmlNode root in xmlReader.ChildNodes[1].ChildNodes)
            {
                hexTypes.Add(new HexType());
                foreach (XmlNode node in root.ChildNodes)
                {
                    switch (node.Name)
                    {
                        case "Sprite":
                            {
                                hexTypes[hexTypes.Count - 1].addSprite(node.Attributes[0].Value, new Rectangle(
                                    Convert.ToInt32(node.Attributes[1].Value),
                                    Convert.ToInt32(node.Attributes[2].Value),
                                    Convert.ToInt32(node.Attributes[3].Value),
                                    Convert.ToInt32(node.Attributes[4].Value)));
                                break;
                            }
                        case "Prop":
                            {
                                hexTypes[hexTypes.Count - 1].addProp(new Scenery(
                                    new Vector2(Convert.ToInt16(node.Attributes[0].Value), Convert.ToInt16(node.Attributes[1].Value)),
                                    (float)Convert.ToDouble(node.Attributes[2].Value),
                                    new Vector2((float)Convert.ToDouble(node.Attributes[3].Value.Split(',')[0]), (float)Convert.ToDouble(node.Attributes[3].Value.Split(',')[0])),
                                    node.Attributes[4].Value.Equals("true"),
                                    node.Attributes[5].Value));
                                break;
                            }
                    }
                }
            }
        }
        public static bool collides(Vector2 pos) 
        { 
            if (pos.X < 0 || pos.X >= hexes.GetLength(0) * 3 || pos.Y < 0 || pos.Y >= hexes.GetLength(1) * 2) return true;
            return false; 
        }

        public static Stack<Vector2> AstarSearch(Vector2 A, Vector2 B)
        {
            Queue<Vector2> frontier = new Queue<Vector2>();
            frontier.Enqueue(A);
            Dictionary<Vector2, Vector2> cameFrom = new Dictionary<Vector2, Vector2>();
            cameFrom.Add(A, new Vector2(-1,-1));

            process AStarProcess = delegate(Vector2 pos, Vector2 current) 
            {
                if (!cameFrom.ContainsKey(pos)
                        && !WorldMap.collides(pos))
                {
                    frontier.Enqueue(pos);
                    cameFrom.Add(pos, current);
                    //Debug.Print("{0},{1}", pos, current);
                }
            };

            while (frontier.Count > 0)
            {
                Vector2 current = frontier.Dequeue();
                if (current.X == B.X && current.Y == B.Y) { break; }

                List<Vector2> neighbors = getNeighbors(current);
                foreach (Vector2 neighbor in neighbors) { AStarProcess(neighbor, current); }
                //Debug.Print("===");
            }
            Stack<Vector2> path = new Stack<Vector2>();
            if (cameFrom.ContainsKey(B))
            {
                path.Push(B);
                //Debug.Print("PATH: {0}", B);
                Vector2 next = cameFrom[B];
                while (cameFrom.ContainsKey(next) && cameFrom[next].X >= 0 && cameFrom[next].Y >= 0) { path.Push(next); next = cameFrom[next]; }
            }
            return path;
        }
        public static List<Vector2> getNeighbors(Vector2 location)
        {
            //it WORKS - something else is broken
            //absolute location
            List<Vector2> neighbors = new List<Vector2>();
            Vector2 hexCorner = getHexCorner(new Vector2((int)location.X / 3, (int)location.Y / 2));
            
            int yAdjust = ((location.Y + ((int) location.X/3 % 2) + location.X) % 2 == 0) ? 1 : -1;
            if (!WorldMap.collides(new Vector2(location.X, location.Y + yAdjust))) { neighbors.Add(new Vector2(location.X, location.Y + yAdjust)); }
            //Vector2 left = new Vector2(location.X - 1, location.Y - ((int)location.X / 3 - ((int)location.X - 1) / 3));
            //if (!WorldMap.collides(new Vector2(left.X, left.Y - (int)left.Y / 3))) { neighbors.Add(left); }

            Vector2 left = new Vector2(location.X - 1, location.Y + ((int)location.X / 3 - ((int)location.X - 1) / 3) * (-1 + ((int)location.X / 3 % 2) * 2));
            if (!WorldMap.collides(new Vector2(left.X, left.Y))) { neighbors.Add(left); }
            Vector2 right = new Vector2(location.X + 1, location.Y - ((int)location.X / 3 - ((int)location.X + 1) / 3) * (-1 + ((int)location.X / 3 % 2) * 2));
            if (!WorldMap.collides(new Vector2(right.X, right.Y))) { neighbors.Add(right); }
            


            return neighbors;
        }
        public static Vector2 getHexCorner(Vector2 Hex)
        {
            return new Vector2(Hex.X * 3, Hex.Y * 2 + ((int)Hex.X % 2));
        }
    }

    public class SortedStack<T> : List<T>
    {
        public SortedStack()
        {
        }
        public T pop() { T val = this[this.Count - 1]; this.Remove(val); return val; }
        public T peek() { return this[this.Count - 1]; }
        //public void sortStack(Func<T, T> lambda) { this.Sort(lambda); }
        public void push(T inT) { this.Add(inT); }
    }

    abstract class GameState
    {
        public string tag;

        public abstract void entering(Game1 game, XmlDocument xmlReader);
        public abstract void leaving();
        public abstract void update(GameTime gameTime, Game1 game);
        public abstract void draw(SpriteBatch spriteBatch);
    }
}
