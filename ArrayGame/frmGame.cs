using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Media;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace ArrayGame
{
    // todo:
    // implement sound effects for pickups/next floor/eating/taking damage/dying
    // implement music for game over
    // implement main menu (and music)
    // implement high scoring
    // move data to solution, set to copy
    public partial class frmGame : Form
    {
        public string Name;
        public Random Rand;

        const int MAP_WIDTH = 60;
        const int MAP_HEIGHT = 60;

        public TileType[,] Map;
        public FloorDirection Direction;
        public Cell[,] MapCells;
        public Player Player;

        private SoundPlayer _Player;
        private string _CurrentSong;

        public frmGame()
        {
            InitializeComponent();
            this.KeyDown += frmGame_KeyDown;
        }

        private string GeneratePrefix(Random rand)
        {
            return rand.Pick<string>(new List<string>() { "Amazing", "Amethyst", "Black", "Boiling", "Brilliant", "Broad", "Brutal", "Chaotic", "Crazy", "Cursed", "Dagger", "Dangerous", "Deadly", "Decayed", "Deep", "Dire", "Dying", "Emerald", "Eternal", "Evil", "False", "Forgotten", "Ghostly", "Giant", "Great", "Green", "Grizzly", "Haunted", "Hellish", "Hidden", "Icy", "Infernal", "Jade", "Lawful", "Mighty", "Misty", "Mysterious", "Rancid", "Red", "Rough", "Royal", "Scary", "Secluded", "Sword", "Tiny", "True", "Twisted", "Uncanny", "Unholy", "Unnamed", "Valiant", "Viscious", "Wailing", "Whispering" });
        }

        private string GenerateLocation(Random rand)
        {
            return rand.Pick<string>(new List<string>() { "Catacomb", "Cave", "Caverns", "Chamber", "Chambers", "Crypt", "Den", "Dungeon", "Grotto", "Hill", "Hills", "Hole", "Labyrinth", "Lair", "Mausoleum", "Maze", "Mortuary", "Mount", "Mountain", "Mountains", "Pit", "Quarter", "Realm", "Tomb", "Tunnel", "Tunnels", "Vault", "Vaults", "Ways" });
        }

        private string GenerateSuffix(Random rand)
        {
            return rand.Pick<string>(new List<string>() { "Ages", "Bane", "Catastrophe", "Chaos", "Darkness", "Death", "Demons", "Despair", "Destruction", "Devils", "Disease", "Doom", "Eternal Night", "Eternity", "Hatred", "Horror", "Illusion", "Insanity", "Lost Souls", "Madness", "Magic", "Might", "Misery", "Mortality", "Nightmares", "No Escape", "Power", "Ruin", "Screams", "Secrets", "Shadows", "Sorrow", "Spite", "Suffering", "Valor", "War", "Winter", "Woe", "Worms", "Worry" });
        }

        private void PlayRandomSong()
        {
            List<string> files = Directory.GetFiles(@"data\music\", "*.wav").ToList();

            // to avoid repeats, remove what's currently going.
            files.Remove(_CurrentSong);

            // pick a random song then play.
            _CurrentSong = Rand.Pick<string>(files);
            _Player = new SoundPlayer(_CurrentSong);
            _Player.Play();     
        }

        private void GenerateNewMap()
        {
            // first, create a list of miners
            List<Vector2> miners = new List<Vector2>();

            // create a new map
            Map = new TileType[MAP_WIDTH, MAP_HEIGHT];

            // set the default tile type
            for (int x = 0; x < MAP_WIDTH; x++)
            {
                for (int y = 0; y < MAP_HEIGHT; y++)
                {
                    Map[x, y] = TileType.OutOfBounds;
                }
            }

            // set the middle cell to be a floor and add a miner here
            Map[MAP_WIDTH / 2, MAP_HEIGHT / 2] = TileType.Floor;
            miners.Add(new Vector2(MAP_WIDTH / 2, MAP_HEIGHT / 2));

            // dig while miners haven't reached a specific amount.
            while (miners.Count < ((MAP_WIDTH + MAP_HEIGHT) / 5))
            {
                for (int i = 0; i < miners.Count; i++)
                {
                    // in the event that something beforehand destoyed one
                    if (miners[i].Equals(null))
                        continue;

                    // keep the miners within a set range so they don't go
                    // out of bounds.
                    miners[i] = new Vector2(miners[i].X.Clamp(2, MAP_WIDTH - 2), miners[i].Y);
                    miners[i] = new Vector2(miners[i].X, miners[i].Y.Clamp(2, MAP_HEIGHT - 2));

                    // laziness:
                    int x = miners[i].X;
                    int y = miners[i].Y;

                    // set the cell at this location to a floor
                    Map[x, y] = TileType.Floor;

                    // pick a random direction to walk.
                    int walk = Rand.Next(4);
                    switch (walk)
                    {
                        case 0: miners[i] += new Vector2(1, 0); break;
                        case 1: miners[i] += new Vector2(0, 1); break;
                        case 2: miners[i] += new Vector2(-1, 0); break;
                        case 3: miners[i] += new Vector2(0, -1); break;
                    }

                    // give a random chance to add a bunch of new miners 
                    // at the same place.
                    if (Rand.Next(100) < 8)
                    {
                        for (int a = 0; a < 3; a++)
                            miners.Add(miners[i]);
                    }

                    if (miners.Count > 1)
                    {
                        // check for already mined walls and remove if it is
                        if (Map[x - 1, y] == TileType.Floor &&
                            Map[x + 1, y] == TileType.Floor &&
                            Map[x, y - 1] == TileType.Floor &&
                            Map[x, y + 1] == TileType.Floor)
                            miners.Remove(miners[i]);
                    }
                }
            }

            // set every spot around the floor to be a wall...assuming it's not already a floor!
            for (int x = 0; x < MAP_WIDTH; x++)
            {
                for (int y = 0; y < MAP_HEIGHT; y++)
                {
                    if (Map[x, y] == TileType.Floor)
                    {
                        if (Map[x - 1, y - 1] != TileType.Floor)
                            Map[x - 1, y - 1] = TileType.Wall;

                        if (Map[x, y - 1] != TileType.Floor)
                            Map[x, y - 1] = TileType.Wall;

                        if (Map[x + 1, y - 1] != TileType.Floor)
                            Map[x + 1, y - 1] = TileType.Wall;

                        if (Map[x + 1, y] != TileType.Floor)
                            Map[x + 1, y] = TileType.Wall;

                        if (Map[x + 1, y + 1] != TileType.Floor)
                            Map[x + 1, y + 1] = TileType.Wall;

                        if (Map[x, y + 1] != TileType.Floor)
                            Map[x, y + 1] = TileType.Wall;

                        if (Map[x - 1, y + 1] != TileType.Floor)
                            Map[x - 1, y + 1] = TileType.Wall;

                        if (Map[x - 1, y] != TileType.Floor)
                            Map[x - 1, y] = TileType.Wall;
                    }
                }
            }

            // get a list of what actually is a floor.
            List<Vector2> floorPositions = new List<Vector2>();
            for (int x = 0; x < MAP_WIDTH; x++)
            {
                for (int y = 0; y < MAP_HEIGHT; y++)
                {
                    if (Map[x, y] == TileType.Floor)
                        floorPositions.Add(new Vector2(x, y));
                }
            }

            // if it's too small, retry.
            if (floorPositions.Count < ((MAP_HEIGHT + MAP_WIDTH) / 2))
            {
                // the chance to get stuck in an "infinite" loop/run out of stack 
                // space is impossibly low. 

                // with my luck, it will happen during demonstration :>
                GenerateNewMap();
                return;
            }

            // now set the player position to a random floor space.
            Player.Position = Rand.Pick<Vector2>(floorPositions);
            floorPositions.Remove(Player.Position);

            // set a staircase.
            Vector2 stairPosition = Rand.Pick<Vector2>(floorPositions);
            Map[stairPosition.X, stairPosition.Y] = TileType.Staircase;
            floorPositions.Remove(stairPosition);

            // now, set a few food/gold spots.
            int totalFood = Rand.Next(3);
            int totalGold = Rand.Next(10);

            // I don't care about food/good overwriting each other (artificial difficulty!), 
            // so I won't check it.
            for (int i = 0; i < totalFood; i++)
            {
                Vector2 foodPosition = Rand.Pick<Vector2>(floorPositions);
                Map[foodPosition.X, foodPosition.Y] = TileType.Food;
            }

            for (int i = 0; i < totalGold; i++)
            {
                Vector2 goldPosition = Rand.Pick<Vector2>(floorPositions);
                Map[goldPosition.X, goldPosition.Y] = TileType.Gold;
            }
        }

        public void Update()
        {
            // clear every cell
            foreach (Cell c in MapCells)
                c.SetData("•", Color.Black);

            // update the camera accordingly
            int cellX = 0;
            int cellY = 0;
            for (int x = Player.Position.X - 7; x < Player.Position.X + 8; x++)
            {
                for (int y = Player.Position.Y - 5; y < Player.Position.Y + 5; y++)
                {
                    if (x < 0 || x >= MAP_WIDTH || y < 0 || y >= MAP_HEIGHT)
                    {
                        // out of bounds. Don't try to access it.
                        cellY++;
                        continue;
                    }

                    // seems to be within bounds. Draw!
                    switch (Map[x, y])
                    {
                        case TileType.Floor: MapCells[cellX, cellY].FormLabel.Text = " "; break;
                        case TileType.OutOfBounds: MapCells[cellX, cellY].SetData("•", Color.Black); break;
                        case TileType.Wall: MapCells[cellX, cellY].SetData("#", Color.Brown); break;
                        case TileType.Staircase: MapCells[cellX, cellY].SetData("%", Color.Orange); break;
                        case TileType.Food: MapCells[cellX, cellY].SetData(":", Color.Bisque); break;
                        case TileType.Gold: MapCells[cellX, cellY].SetData("*", Color.Gold); break;
                    }
                    cellY++;
                }
                cellY = 0;
                cellX++;
            }

            // set the player's label
            MapCells[7, 5].SetData("@", Color.Green);
        }

        private void SetFloor()
        {
            string temp = "";
            switch (Direction)
            {
                case FloorDirection.Ascending: temp = "{0}F".FormatBy(Player.Floor.ToString()); break;
                case FloorDirection.Descending: temp = "B{0}F".FormatBy(Player.Floor.ToString()); break;
            }
            lblName.Text = "{0} {1}".FormatBy(this.Name, temp);
        }

        private void SetPoints()
        {
            lblPoints.Text = "{0} Points".FormatBy(this.Player.Points.ToString());
        }

        private void frmGame_Load(object sender, EventArgs e)
        {
            Rand = new Random();

            // create the player.
            Player = new Player();
           
            // first, generate a name.
            // name generator's...names from: http://www.goodsandgoodies.com/wq/bashwe-dungeonnames.pdf
            switch (Rand.Pick<NameStructure>(new List<NameStructure>() { NameStructure.Prefix, NameStructure.Prefix2x, NameStructure.Suffix, 
            NameStructure.PrefixAndSuffix, NameStructure.Prefix2xAndSuffix })) 
            {
                case NameStructure.Prefix:
                    this.Name = "The {0} {1}".FormatBy(GeneratePrefix(Rand), GenerateLocation(Rand));
                    break;

                case NameStructure.Prefix2x:
                    this.Name = "The {0}-{1} {2}".FormatBy(GeneratePrefix(Rand), GeneratePrefix(Rand), GenerateLocation(Rand));
                    break;

                case NameStructure.Suffix:
                    this.Name = "The {0} of {1}".FormatBy(GenerateLocation(Rand), GenerateSuffix(Rand));
                    break;

                case NameStructure.PrefixAndSuffix:
                    this.Name = "The {0} {1} of {2}".FormatBy(GeneratePrefix(Rand), GenerateLocation(Rand), GenerateSuffix(Rand));
                    break;

                case NameStructure.Prefix2xAndSuffix:
                    this.Name = "The {0}-{1} {2} of {3}".FormatBy(GeneratePrefix(Rand), GeneratePrefix(Rand), GenerateLocation(Rand), GenerateSuffix(Rand));
                    break;
            }
            
            // pick whether the dungeon is ascending or descending.
            Direction = Rand.Pick<FloorDirection>(new List<FloorDirection>() { FloorDirection.Ascending, FloorDirection.Descending });
            SetFloor();
            SetPoints();

            // set the grid up.
            MapCells = new Cell[15, 10];
            for (int x = 0; x < 15; x++)
            {
                for (int y = 0; y < 10; y++)
                {
                    Label lbl = new Label();
                    lbl.Text = " ";
                    lbl.AutoSize = false;
                    lbl.Size = new System.Drawing.Size(46, 46);
                    lbl.Location = new Point(46 * x, 46 * y);
                    lbl.TextAlign = ContentAlignment.MiddleCenter;
                    lbl.Font = new System.Drawing.Font("Segoe UI", 16);
                    pnlGame.Controls.Add(lbl);
                    MapCells[x, y] = new Cell(lbl);
                }
            }

            // play a random song.
            PlayRandomSong();

            // generate a new map and display.
            GenerateNewMap();
            Update();
        }

        void frmGame_KeyDown(object sender, KeyEventArgs e)
        {
            // move
            switch (e.KeyCode)
            {
                case Keys.Left:
                    // check if there's a tile or enemy.
                    if (!(Map[Player.Position.X - 1, Player.Position.Y] == TileType.Wall || Map[Player.Position.X - 1, Player.Position.Y] == TileType.OutOfBounds))
                    {
                        Player.Position.X -= 1;
                        Player.AddPoints(1);
                    }

                    break;
                case Keys.Up:
                    if (!(Map[Player.Position.X, Player.Position.Y - 1] == TileType.Wall || Map[Player.Position.X, Player.Position.Y - 1] == TileType.OutOfBounds))
                    {
                        Player.Position.Y -= 1;
                        Player.AddPoints(1);
                    }

                    break;
                case Keys.Right:
                    if (!(Map[Player.Position.X + 1, Player.Position.Y] == TileType.Wall || Map[Player.Position.X + 1, Player.Position.Y] == TileType.OutOfBounds))
                    {
                        Player.Position.X += 1;
                        Player.AddPoints(1);
                    }

                    break;
                case Keys.Down:
                    if (!(Map[Player.Position.X, Player.Position.Y + 1] == TileType.Wall || Map[Player.Position.X, Player.Position.Y + 1] == TileType.OutOfBounds))
                    {
                        Player.Position.Y += 1;
                        Player.AddPoints(1);
                    }

                    break;
                case Keys.Enter:
                    // check if we're on gold
                    if (Map[Player.Position.X, Player.Position.Y] == TileType.Gold)
                    {
                        // give the player some points
                        Player.AddPoints(50);

                        // set the tile back to normal
                        Map[Player.Position.X, Player.Position.Y] = TileType.Floor;
                    }

                    // check if we're on food
                    if (Map[Player.Position.X, Player.Position.Y] == TileType.Food)
                    {
                        // give the player some health, if they're not at full health

                        // set the tile back to normal, if the food was used

                    }

                    // check if we're on staircases
                    if (Map[Player.Position.X, Player.Position.Y] == TileType.Staircase)
                    {
                        // next floor!
                        Player.Floor++;
                        Player.AddPoints(100);
                        SetFloor();
                        GenerateNewMap();
                        Update();
                        PlayRandomSong();
                    }
                    break;
            }
            SetPoints();

            // DoEvents is an evil, evil function that should generally never be used
            // but it's the only thing keeping the display updated when holding
            // a key down. I'm conflicted by this use case.
            // I _could_ have used delegates, but by the time I came around to making
            // optimisations, I was already too far in to go through and make sweeping
            // changes and could do without anything breaking.
            Application.DoEvents();

            // and then redraw
            Update();
        }
    }
}
