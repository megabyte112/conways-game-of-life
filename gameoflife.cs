using System;
using System.IO;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace gameoflife
{
    public class gameoflife : Game
    {
        private GraphicsDeviceManager _graphics;
        private SpriteBatch _spriteBatch;
        Texture2D square;
        SpriteFont font;
        Random r = new Random();
        const int height = 242;
        const int width = 428;
        float cellwidth = 16;
        bool[,] grid;
        int[,] adjacent;
        MouseState mouse = Mouse.GetState();
        KeyboardState keyboard = Keyboard.GetState();
        bool advance = false;
        string status = "Waiting (Press Space to Start)";
        string savestatus;
        bool showcontrols = true;
        int[] possibleframerates = { 5, 10, 15, 20, 25, 30, 35, 40, 45, 50, 55, 60 };
        int currentframerateindex = 11;
        bool type;
        double framerate;
        int cellx;
        int celly;
        int windowheight;
        int windowwidth;
        Vector2 offset;
        static float camerazoomspeed;
        int count;
        int startxpos;
        int startypos;
        int deltaxpos;
        int deltaypos;
        Vector2 moved;
        string controlsmessage = @"
            Controls:
                Space                     Pause/Play
                Backspace              Clear
                Enter                       Random Generation
                Ctrl + 0-9                 Save Current State
                0-9                           Load Saved State
                Esc                          Exit
                G                             Generate a Gosper Glider Gun
                Left/Right arrow       Control Framerate
                Scroll Wheel            Zoom in/out

                  Click on a cell to toggle its state
                    
                        Press Space to begin";
        public gameoflife()
        {
            _graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";
            
            // limit framerate to 15fps
            this.IsFixedTimeStep = true;
            this.TargetElapsedTime = TimeSpan.FromSeconds(1d / 60d);

            IsMouseVisible = true;
        }

        protected override void Initialize()
        {
            grid = new bool[height, width];
            adjacent = new int[height, width];
            savestatus = "";

            // randomise grid
            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    if (r.Next(0,2) == 1)
                    {
                        grid[y,x] = true;
                    }
                }
            }
            _graphics.HardwareModeSwitch = false;
            Window.AllowUserResizing = true;
            _graphics.PreferredBackBufferHeight = 720;
            _graphics.PreferredBackBufferWidth = 1280;
            _graphics.ApplyChanges();
            count = 1;
            base.Initialize();
        }

        protected override void LoadContent()
        {
            _spriteBatch = new SpriteBatch(GraphicsDevice);

            // load font and square texture
            font = this.Content.Load<SpriteFont>("font");
            square = this.Content.Load<Texture2D>("square");
        }

        protected override void Update(GameTime gameTime)
        {
            var lastmousestate = mouse;
            var lastkeyboardupdate = keyboard;
            mouse = Mouse.GetState();
            keyboard = Keyboard.GetState();
            adjacent = GetAdjacent(grid);
            // mouseposition divided by cell width gives you the coordinate of the cell, compensate for zoom
            celly = (int)Math.Floor(Convert.ToDouble(mouse.Y - offset.Y)/ cellwidth);
            cellx = (int)Math.Floor(Convert.ToDouble(mouse.X - offset.X)/ cellwidth);
            if (advance && count == 1)    // only run when not paused and once every 4 frames
            {
                adjacent = GetAdjacent(grid);
                status = "Running";
                for (int y = 0; y < height; y++)
                {
                    for (int x = 0; x < width; x++)
                    {
                        // conway's rules
                        if (grid[y, x] && adjacent[y,x] < 2)
                        {
                            grid[y, x] = false;
                        }
                        else if (grid[y, x] && adjacent[y, x] > 3)
                        {
                            grid[y, x] = false;
                        }
                        else if ((!grid[y, x]) && adjacent[y, x] == 3)
                        {
                            grid[y, x] = true;
                        }
                    }
                }
            }
            if (keyboard.IsKeyDown(Keys.Escape))
                Exit();
            if (lastmousestate.LeftButton == ButtonState.Released && mouse.LeftButton == ButtonState.Pressed)
            {
                if (cellx < width && celly < height && cellx > 0 && celly > 0)
                {
                    // toggle state of cell
                    if (grid[celly, cellx] == false)
                    {
                        grid[celly, cellx] = true;
                    }
                    else
                    {
                        grid[celly, cellx] = false;
                    }
                    type = grid[celly, cellx];
                }
                savestatus = "";
            }
            else if (mouse.LeftButton == ButtonState.Pressed && !advance)
            {
                if (cellx >= width)
                {
                    cellx = width - 1;
                }
                else if (cellx < 0)
                {
                    cellx = 1;
                }
                if (celly >= height)
                {
                    celly = height - 1;
                }
                else if (celly < 0)
                {
                    celly = 1;
                }
                grid[celly, cellx] = type;
            }
            if (mouse.RightButton == ButtonState.Pressed && lastmousestate.RightButton != ButtonState.Pressed)
            {
                startxpos = mouse.X;
                startypos = mouse.Y;
            }
            else if (mouse.RightButton == ButtonState.Pressed)
            {
                deltaxpos = mouse.X - lastmousestate.X;
                deltaypos = mouse.Y - lastmousestate.Y;
                moved = moved + new Vector2(deltaxpos, deltaypos);
            }
            if (keyboard.IsKeyDown(Keys.Enter) && !lastkeyboardupdate.IsKeyDown(Keys.Enter))
            {
                for (int y = 0; y < height; y++)
                {
                    for (int x = 0; x < width; x++)
                    {
                        if (r.Next(0,2) == 1)
                        {
                            grid[y,x] = true;
                        }
                        else
                        {
                            // old white spaces need to be overwritten
                            grid[y, x] = false;
                        }
                    }
                }
                savestatus = "";
            }
            else if (keyboard.IsKeyDown(Keys.Space) && !lastkeyboardupdate.IsKeyDown(Keys.Space))
            {
                // toggle pause
                if (advance)
                {
                    advance = false;
                    status = "Paused";
                    this.TargetElapsedTime = TimeSpan.FromSeconds(1d / 60d);
                }
                else
                {
                    advance = true;
                    this.TargetElapsedTime = TimeSpan.FromSeconds(1d / Convert.ToDouble(possibleframerates[currentframerateindex]));
                }
                savestatus = "";
                showcontrols = false;
            }
            else if (keyboard.IsKeyDown(Keys.Back))
            {
                // clear the grid
                for (int y = 0; y < height; y++)
                {
                    for (int x = 0; x < width; x++)
                    {
                        grid[y, x] = false;
                    }
                }
                savestatus = "";
            }
            else if (keyboard.IsKeyDown(Keys.LeftControl) && keyboard.IsKeyDown(Keys.D1) && !lastkeyboardupdate.IsKeyDown(Keys.D1))
            {
                // save to slot 1
                Save(grid, "1");
                savestatus = "Saved to slot 1";
            }
            else if (!keyboard.IsKeyDown(Keys.LeftControl) && keyboard.IsKeyDown(Keys.D1) && !lastkeyboardupdate.IsKeyDown(Keys.D1))
            {
                // Load from slot 1
                if (File.Exists("1"))
                {
                    grid = Load(grid, "1");
                    savestatus = "Loaded from slot 1";
                }
            }
            else if (keyboard.IsKeyDown(Keys.LeftControl) && keyboard.IsKeyDown(Keys.D2) && !lastkeyboardupdate.IsKeyDown(Keys.D2))
            {
                // save to slot 2
                Save(grid, "2");
                savestatus = "Saved to slot 2";
            }
            else if (!keyboard.IsKeyDown(Keys.LeftControl) && keyboard.IsKeyDown(Keys.D2) && !lastkeyboardupdate.IsKeyDown(Keys.D2))
            {
                // Load from slot 2
                if (File.Exists("2"))
                {
                    grid = Load(grid, "2");
                    savestatus = "Loaded from slot 2";
                }
            }
            else if (keyboard.IsKeyDown(Keys.LeftControl) && keyboard.IsKeyDown(Keys.D3) && !lastkeyboardupdate.IsKeyDown(Keys.D3))
            {
                // save to slot 3
                Save(grid, "3");
                savestatus = "Saved to slot 3";
            }
            else if (!keyboard.IsKeyDown(Keys.LeftControl) && keyboard.IsKeyDown(Keys.D3) && !lastkeyboardupdate.IsKeyDown(Keys.D3))
            {
                // Load from slot 3
                if (File.Exists("3"))
                {
                    grid = Load(grid, "3");
                    savestatus = "Loaded from slot 3";
                }
            }
            else if (keyboard.IsKeyDown(Keys.LeftControl) && keyboard.IsKeyDown(Keys.D4) && !lastkeyboardupdate.IsKeyDown(Keys.D4))
            {
                // save to slot 4
                Save(grid, "4");
                savestatus = "Saved to slot 4";
            }
            else if (!keyboard.IsKeyDown(Keys.LeftControl) && keyboard.IsKeyDown(Keys.D4) && !lastkeyboardupdate.IsKeyDown(Keys.D4))
            {
                // Load from slot 4
                if (File.Exists("4"))
                {
                    grid = Load(grid, "4");
                    savestatus = "Loaded from slot 4";
                }
            }
            else if (keyboard.IsKeyDown(Keys.LeftControl) && keyboard.IsKeyDown(Keys.D5) && !lastkeyboardupdate.IsKeyDown(Keys.D5))
            {
                // save to slot 5
                Save(grid, "5");
                savestatus = "Saved to slot 5";
            }
            else if (!keyboard.IsKeyDown(Keys.LeftControl) && keyboard.IsKeyDown(Keys.D5) && !lastkeyboardupdate.IsKeyDown(Keys.D5))
            {
                // Load from slot 5
                if (File.Exists("5"))
                {
                    grid = Load(grid, "5");
                    savestatus = "Loaded from slot 5";
                }
            }
            else if (keyboard.IsKeyDown(Keys.LeftControl) && keyboard.IsKeyDown(Keys.D6) && !lastkeyboardupdate.IsKeyDown(Keys.D6))
            {
                // save to slot 6
                Save(grid, "6");
                savestatus = "Saved to slot 6";
            }
            else if (!keyboard.IsKeyDown(Keys.LeftControl) && keyboard.IsKeyDown(Keys.D6) && !lastkeyboardupdate.IsKeyDown(Keys.D6))
            {
                // Load from slot 6
                if (File.Exists("6"))
                {
                    grid = Load(grid, "6");
                    savestatus = "Loaded from slot 6";
                }
            }
            else if (keyboard.IsKeyDown(Keys.LeftControl) && keyboard.IsKeyDown(Keys.D7) && !lastkeyboardupdate.IsKeyDown(Keys.D7))
            {
                // save to slot 7
                Save(grid, "7");
                savestatus = "Saved to slot 7";
            }
            else if (!keyboard.IsKeyDown(Keys.LeftControl) && keyboard.IsKeyDown(Keys.D7) && !lastkeyboardupdate.IsKeyDown(Keys.D7))
            {
                // Load from slot 7
                if (File.Exists("7"))
                {
                    grid = Load(grid, "7");
                    savestatus = "Loaded from slot 7";
                }
            }
            else if (keyboard.IsKeyDown(Keys.LeftControl) && keyboard.IsKeyDown(Keys.D8) && !lastkeyboardupdate.IsKeyDown(Keys.D8))
            {
                // save to slot 8
                Save(grid, "8");
                savestatus = "Saved to slot 8";
            }
            else if (!keyboard.IsKeyDown(Keys.LeftControl) && keyboard.IsKeyDown(Keys.D8) && !lastkeyboardupdate.IsKeyDown(Keys.D8))
            {
                // Load from slot 8
                if (File.Exists("8"))
                {
                    grid = Load(grid, "8");
                    savestatus = "Loaded from slot 8";
                }
            }
            else if (keyboard.IsKeyDown(Keys.LeftControl) && keyboard.IsKeyDown(Keys.D9) && !lastkeyboardupdate.IsKeyDown(Keys.D9))
            {
                // save to slot 9
                Save(grid, "9");
                savestatus = "Saved to slot 9";
            }
            else if (!keyboard.IsKeyDown(Keys.LeftControl) && keyboard.IsKeyDown(Keys.D9) && !lastkeyboardupdate.IsKeyDown(Keys.D9))
            {
                // Load from slot 9
                if (File.Exists("9"))
                {
                    grid = Load(grid, "9");
                    savestatus = "Loaded from slot 9";
                }
            }
            else if (keyboard.IsKeyDown(Keys.LeftControl) && keyboard.IsKeyDown(Keys.D0) && !lastkeyboardupdate.IsKeyDown(Keys.D0))
            {
                // save to slot 0
                Save(grid, "0");
                savestatus = "Saved to slot 0";
            }
            else if (!keyboard.IsKeyDown(Keys.LeftControl) && keyboard.IsKeyDown(Keys.D0) && !lastkeyboardupdate.IsKeyDown(Keys.D0))
            {
                // Load from slot 0
                if (File.Exists("h"+height+"w"+width+"n0"))
                {
                    grid = Load(grid, "0");
                    savestatus = "Loaded from slot 0";
                }
            }
            else if (keyboard.IsKeyDown(Keys.G) && !lastkeyboardupdate.IsKeyDown(Keys.G))
            {
                if (File.Exists("Content/gosper"))
                {
                    grid = Gosper(grid);
                    cellwidth = 4;
                }
                else
                {
                    savestatus = "Missing Gosper Save File!";
                }
            }
            else if (keyboard.IsKeyDown(Keys.Right) && !lastkeyboardupdate.IsKeyDown(Keys.Right) && currentframerateindex < 11)
            {
                this.TargetElapsedTime = TimeSpan.FromSeconds(1d / Convert.ToDouble(possibleframerates[++currentframerateindex]));
            }
            else if (keyboard.IsKeyDown(Keys.Left) && !lastkeyboardupdate.IsKeyDown(Keys.Left) && currentframerateindex > 0)
            {
                this.TargetElapsedTime = TimeSpan.FromSeconds(1d / Convert.ToDouble(possibleframerates[--currentframerateindex]));
            }
            else if (keyboard.IsKeyDown(Keys.F11) && !lastkeyboardupdate.IsKeyDown(Keys.F11))
            {
                if (_graphics.IsFullScreen)
                {
                    _graphics.PreferredBackBufferHeight = windowheight;
                    _graphics.PreferredBackBufferWidth = windowwidth;
                }
                else
                {
                    windowheight = Window.ClientBounds.Height;
                    windowwidth = Window.ClientBounds.Width;
                    _graphics.PreferredBackBufferHeight = GraphicsAdapter.DefaultAdapter.CurrentDisplayMode.Height;
                    _graphics.PreferredBackBufferWidth = GraphicsAdapter.DefaultAdapter.CurrentDisplayMode.Width;
                }
                _graphics.ToggleFullScreen();
                _graphics.ApplyChanges();
            }
            if (mouse.ScrollWheelValue > lastmousestate.ScrollWheelValue && cellwidth > 4)
            {
                camerazoomspeed = -20;
            }
            else if (mouse.ScrollWheelValue < lastmousestate.ScrollWheelValue && cellwidth < 32)
            {
                camerazoomspeed = 20;
            }
            if (camerazoomspeed > 0)
            {
                cellwidth=(camerazoomspeed/32)+cellwidth;
                camerazoomspeed -= 1f;
                if (cellwidth>32)
                {
                    camerazoomspeed = 0;
                    cellwidth = 32;
                }
            }
            else if (camerazoomspeed < 0)
            {
                cellwidth=(camerazoomspeed/32)+cellwidth;
                camerazoomspeed += 1f;
                if (cellwidth<4) 
                {
                    camerazoomspeed = 0;
                    cellwidth = 4;
                }
            }
            if (count != 4)
            {
                count++;
            }
            else
            {
                count = 1;
            }
            offset = new Vector2(-(width*cellwidth/2)+Window.ClientBounds.Width/2, -(height*cellwidth/2)+Window.ClientBounds.Height/2) + moved;
            base.Update(gameTime);
        }

        protected override void Draw(GameTime gameTime)
        {
            Window.Title = ("Conway's Game of Life      Live: "+GetAlive(grid, showcontrols)+"    Dead: " + GetDead(grid, showcontrols)
            + "    Status: "+ status + "    FPS: "+framerate+"    "+savestatus);
            float scale = cellwidth / 16;
            GraphicsDevice.Clear(Color.Black);
            _spriteBatch.Begin();
            if (showcontrols)
            {
                _spriteBatch.DrawString(font, (controlsmessage), new Vector2(32, 32), Color.White );
            }
            else
            {    // draw grid
                int y;
                int x;
                for (y = 1; y < height-1; y++)
                {
                    for (x = 1; x < width-1; x++)
                    {
                        if (grid[y,x] == true)
                        {
                            _spriteBatch.Draw(square, new Vector2 (x * cellwidth, y * cellwidth) + offset, null, Color.White, 0f, Vector2.Zero, scale, SpriteEffects.None, 0f);
                        }
                        if (y == celly && x == cellx && !advance)
                        {
                            if (grid[y, x])
                            {
                                _spriteBatch.Draw(square, new Vector2 (x * cellwidth, y * cellwidth) + offset, null, Color.DarkGray, 0f, Vector2.Zero, scale, SpriteEffects.None, 0f);
                            }
                            else
                            {
                                _spriteBatch.Draw(square, new Vector2 (x * cellwidth, y * cellwidth) + offset, null, Color.Gray, 0f, Vector2.Zero, scale, SpriteEffects.None, 0f);
                            }
                        }
                    }
                }

            }
            _spriteBatch.End();
            framerate = Math.Round(1 / gameTime.ElapsedGameTime.TotalSeconds);
            base.Draw(gameTime);
        }
        static int GetAlive(bool[,] grid, bool showcontrols)
        {
            int alive = 0;
            foreach (var x in grid)
            {
                if (x)
                {
                    alive++;
                }
            }
            if (showcontrols) return 0;
            return alive;
        }
        static int GetDead(bool[,] grid, bool showcontrols)
        {
            int dead = 0;
            foreach (var x in grid)
            {
                if (!x)
                {
                    dead++;
                }
            }
            if (showcontrols) return 0;
            return dead;
        }
        static int[,] GetAdjacent(bool[,] grid)
        {
            int[,] adjacent = new int[height, width];
            int value;
            for (int y = 1; y < height - 1; y++)
            {
                for (int x = 1; x < width - 1; x++)
                {
                    value = 0;
                    for (int a = -1; a < 2; a++)
                    {
                        for (int b = -1; b < 2; b++)
                        {
                            if (grid[y+b, x+a] && !(a == 0 && b == 0))
                            {
                                value++;
                            }
                        }
                    }
                    adjacent[y, x] = value;
                }
            }
            return adjacent;
        }
        static void Save(bool[,] grid, string slot)
        {
            string savename = slot;
            var stream = File.Open(savename, FileMode.Create);
            var bw = new BinaryWriter(stream);
            foreach (var x in grid)
            {
                bw.Write(x);
            }
            bw.Close();
            stream.Close();
        }
        static bool[,] Load(bool[,] grid, string slot)
        {
            string loadname = slot;
            var stream = File.Open(loadname, FileMode.Open);
            var br = new BinaryReader(stream);
            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    grid[y, x] = br.ReadBoolean();
                }
            }
            br.Close();
            stream.Close();
            return grid;
        }
        static bool[,] Gosper(bool[,] grid)
        {
            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    grid[y, x] = false;
                }
            }
            string loadname = "Content/gosper";
            var stream = File.Open(loadname, FileMode.Open);
            var br = new BinaryReader(stream);
            for (int y = 0; y < 18; y++)
            {
                for (int x = 0; x < 40; x++)
                {
                    grid[y, x] = br.ReadBoolean();
                }
            }
            br.Close();
            stream.Close();
            return grid;
        }

    }
}
