using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;

namespace StrategyGame
{

    public class HexType
    {
        public List<Prop> props = new List<Prop>();
        public Dictionary<string, Rectangle> sprites = new Dictionary<string, Rectangle>();
        public string tag;
        public void addProp(Prop prop) { this.props.Add(prop); }
        public void addSprite(string tag, Rectangle sprite) { this.sprites[tag] = sprite; }
    }
    public class Scenery : Prop
    {
        public Scenery(Hex hex, Vector2 coords, float rotation, Rectangle sprite, Vector2 scale, bool flat, string tag)
            : base(hex, coords, rotation, sprite, scale, flat, tag)
        {
        }
        public Scenery(Vector2 coords, float rotation, Vector2 scale, bool flat, string tag)
            : base(coords, rotation, scale, flat, tag)
        {
        }
        public override float getDrawDepth()
        {
            if (this.flat) return 0f;
            return (this.coords.Y + hex.getAbsoluteCoords().Y + (this.sprite.Height) * this.scale.Y) / (924f);
        }
    }

    public abstract class Prop : IComparable<Prop>
    {
        public Vector2 coords;
        public float rotation;
        public Vector2 scale;
        public bool flat;
        public string tag;
        public Rectangle sprite;
        public Hex hex;
        public bool flipped = false;

        public Prop(Hex hex, Vector2 coords, float rotation, Rectangle sprite, Vector2 scale, bool flat, string tag)
        {
            this.hex = hex;
            this.coords = coords;
            this.rotation = rotation;
            this.sprite = sprite;
            this.scale = scale;
            this.flat = flat;
            this.tag = tag;
        }
        public Prop(Vector2 coords, float rotation, Vector2 scale, bool flat, string tag)
        {
            this.coords = coords;
            this.rotation = rotation;
            this.scale = scale;
            this.flat = flat;
            this.tag = tag;
        }
        public abstract float getDrawDepth();

        public int CompareTo(Prop other)
        {
            return this.getDrawDepth().CompareTo(other.getDrawDepth());
        }
    }

    public class Hex
    {
        public Tile[,] tiles = new Tile[3, 2];
        public Vector2 coords;
        public Vector2 center;
        public HexType type;

        public Hex(Vector2 coords, HexType type)
        {
            this.type = type;
            this.coords = coords;
            this.center = (coords * new Vector2(234, 258)) + new Vector2(156, 129 + (this.coords.X % 2 * 129));
            //this.center = new Vector2(this.coords.X * (Game1.XBetweenTiles * 3), (this.coords.Y) * Game1.YBetweenTiles + ((this.coords.Y % 2) *(Game1.YBetweenTiles * 2)));
            //2 X widths and 1 Y height
        }
        public void update(GameTime gameTime)
        {

        }
        public double distance(Vector2 absolute)
        {
            return Math.Sqrt(Math.Pow(this.center.X - absolute.X, 2) + Math.Pow(this.center.Y - absolute.Y, 2));
        }
        public Vector2 getAbsoluteCoords()
        {
            return new Vector2(WorldMap.XBetweenTiles * (3 * this.coords.X), WorldMap.YBetweenTiles * ((this.coords.X % 2) + (this.coords.Y * 2)));
        }
    }

    public class Tile
    {
        public Rectangle sprite = new Rectangle(6, 8, 312, 266);
        public Vector2 coords;
        public Unit occupied;

        public Tile(Vector2 coords)
        {
            this.coords = coords;
        }
        public void occupy(Unit unit)
        {
            if (occupied == null) { this.occupied = unit; }
            else { throw new Exception("Tile already Occupied"); }
        }
        public bool isOccupied() { return this.occupied != null; }
        public void deoccupy() { this.occupied = null; }
    }


    class DebugList
    {
        public Dictionary<Vector2, Color> places;
        public List<Tuple<Vector2, Color>> dots;

        public DebugList()
        {
            this.places = new Dictionary<Vector2, Color>();
            this.dots = new List<Tuple<Vector2, Color>>();
        }

        public void update(Vector2 relative, Vector2 absolute)
        {
            try
            {
                dots.Add(new Tuple<Vector2, Color>(absolute, places[relative]));
            }
            catch (Exception e)
            {
                Random rand = new Random();
                this.places[relative] = new Color(rand.Next(0, 255), rand.Next(0, 255), rand.Next(0, 255));
            }
        }

    }
}
