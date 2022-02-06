using System;
using System.IO;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace gameoflife
{
    public class gameoflife : Game
    {
        static Gosper gosper = new Gosper();
        static Camera camera;
        private GraphicsDeviceManager _graphics;
        private SpriteBatch _spriteBatch;
        Texture2D square;
        SpriteFont font;
        SpriteFont infofont;
        Random r = new Random();
        const int height = 242;
        const int width = 428;
        float cellwidth = 16;
        bool[,] grid;
        int[,] adjacent;
        MouseState mouse = Mouse.GetState();
        KeyboardState keyboard = Keyboard.GetState();
        static bool advance = false;
        string status = "Waiting";
        string savestatus;
        bool showcontrols = true;
        bool type;
        double framerate;
        int cellx;
        int celly;
        Vector2 mouseworldpos;
        Vector2 offset;
        static float camerazoomspeed;
        int count;
        int startxpos;
        int startypos;
        int deltaxpos;
        int deltaypos;
        Vector2 moved;
        Vector2 lastmoved;
        Vector2 deltamoved;
        bool cameracanmove;
        bool islowframerate;
        DiscordRPC.DiscordRpcClient client;
        string controlsmessage = @"
            Controls:
                Space - Pause/Play
                Backspace - Clear
                Enter - Random Generation
                Ctrl + 0-9 - Save Current State
                0-9 - Load Saved State
                Esc - Exit
                G - Generate a Gosper Glider Gun
                Scroll Wheel - Zoom in/out
                Left Click - Toggle cell state
                Left Click + drag (while paused) - Draw
                Right Click + drag - Pan Camera
                F11 - Toggle Fullscreen/Windowed mode
                Delete - Low Framerate Mode (Fix Performance Issues)
                C - Show This Menu
                    
                        Press Space to begin";
        public gameoflife()
        {
            _graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";
            _graphics.HardwareModeSwitch = false;
            
            // limit framerate to 15fps
            this.IsFixedTimeStep = true;
            this.TargetElapsedTime = TimeSpan.FromSeconds(1d / 60d);

            IsMouseVisible = true;
        }

        protected override void Initialize()
        {
            grid = new bool[height, width];
            adjacent = new int[height, width];
            savestatus = "Press Enter to randomise";

            // discord
            client = new DiscordRPC.DiscordRpcClient("939845054497955871");
            client.Initialize();
            UpdateDiscord(status);

            if (!Directory.Exists("saves"))
            {
                Directory.CreateDirectory("saves");
            }

            Window.AllowUserResizing = true;
            Window.ClientSizeChanged += OnResize;
            Window.Title = "Conway's Game of Life";
            _graphics.PreferredBackBufferHeight = 720;
            _graphics.PreferredBackBufferWidth = 1280;
            _graphics.ApplyChanges();
            camera = new Camera(_graphics.GraphicsDevice.Viewport);
            count = 1;
            cameracanmove = false;
            islowframerate = false;
            base.Initialize();
        }

        protected override void LoadContent()
        {
            _spriteBatch = new SpriteBatch(GraphicsDevice);

            // load font and square texture
            font = this.Content.Load<SpriteFont>("font");
            infofont = this.Content.Load<SpriteFont>("info");
            square = this.Content.Load<Texture2D>("square");
        }

        protected override void Update(GameTime gameTime)
        {
            var lastmousestate = mouse;
            var lastkeyboardupdate = keyboard;
            mouseworldpos = Vector2.Transform(new Vector2(mouse.X, mouse.Y), Matrix.Invert(camera.Transform))/cellwidth;
            cellx = (int)mouseworldpos.X;
            celly = (int)mouseworldpos.Y;
            mouse = Mouse.GetState();
            keyboard = Keyboard.GetState();
            adjacent = GetAdjacent(grid);
            camera.MoveVector(-deltamoved * (1/camera.Zoom));
            camera.Update();
            
            if (advance && count == 4)    // only run when not paused and once every 4 frames
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
            if (lastmousestate.LeftButton == ButtonState.Released && mouse.LeftButton == ButtonState.Pressed && !showcontrols)
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
            else if (mouse.LeftButton == ButtonState.Pressed && !advance && !showcontrols)
            {
                if (cellx >= width-1)
                {
                    cellx = width - 2;
                }
                else if (cellx < 0)
                {
                    cellx = 1;
                }
                if (celly >= height-1)
                {
                    celly = height - 2;
                }
                else if (celly < 0)
                {
                    celly = 1;
                }
                grid[celly, cellx] = type;
            }
            if (mouse.RightButton == ButtonState.Pressed && lastmousestate.RightButton != ButtonState.Pressed && cameracanmove)
            {
                startxpos = mouse.X;
                startypos = mouse.Y;
            }
            else if (mouse.RightButton == ButtonState.Pressed && cameracanmove)
            {
                deltaxpos = mouse.X - lastmousestate.X;
                deltaypos = mouse.Y - lastmousestate.Y;
                lastmoved = moved;
                moved = moved + new Vector2(deltaxpos, deltaypos);
                deltamoved = moved - lastmoved;
            }
            else if (deltamoved.X > 1 || deltamoved.X < -1 || deltamoved.Y > 1 || deltamoved.Y < -1)
            {
                deltamoved/=1.1f;
            }
            else
            {
                deltamoved=Vector2.Zero;
            }
            if (camera.X - 6848 > 0 && mouse.RightButton == ButtonState.Released)
            {
                deltamoved += new Vector2(10, 0);
            }
            else if (camera.X < 0 && mouse.RightButton == ButtonState.Released)
            {
                deltamoved += new Vector2(-10, 0);
            }
            if (camera.Y - 3872 > 0 && mouse.RightButton == ButtonState.Released)
            {
                deltamoved += new Vector2(0, 10);
            }
            else if (camera.Y < 0 && mouse.RightButton == ButtonState.Released)
            {
                deltamoved += new Vector2(0, -10);
            }
            else if (!showcontrols)
            {
                cameracanmove = true;
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
                if (savestatus == "Press Enter to randomise")
                {
                    savestatus = "Press Space to Start";
                }
                else
                {
                    savestatus = "";
                }
            }
            else if (keyboard.IsKeyDown(Keys.Space) && !lastkeyboardupdate.IsKeyDown(Keys.Space))
            {
                // toggle pause
                
                if (!showcontrols)
                {
                    savestatus = "";
                }
                if (showcontrols)
                {
                    camera.Zoom = 0.18f;
                    showcontrols = false;
                    status = "Paused";
                }
                else if (advance)
                {
                    advance = false;
                    status = "Paused";
                }
                else
                {
                    advance = true;
                    status = "Running";
                }
                UpdateDiscord(status);
            }
            else if (keyboard.IsKeyDown(Keys.Back) && !showcontrols)
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
                camera.Zoom = 0.2f;
                camera.X = 3424;
                camera.Y = 1936;
                advance = false;
            }
            else if (!keyboard.IsKeyDown(Keys.LeftControl) && keyboard.IsKeyDown(Keys.D1) && !lastkeyboardupdate.IsKeyDown(Keys.D1))
            {
                // Load from slot 1
                if (File.Exists("saves/1"))
                {
                    grid = Load(grid, "1");
                    savestatus = "Loaded from slot 1";
                    camera.Zoom = 0.2f;
                    camera.X = 3424;
                    camera.Y = 1936;
                    advance = false;
                }
            }
            else if (keyboard.IsKeyDown(Keys.LeftControl) && keyboard.IsKeyDown(Keys.D2) && !lastkeyboardupdate.IsKeyDown(Keys.D2))
            {
                // save to slot 2
                Save(grid, "2");
                savestatus = "Saved to slot 2";
                camera.Zoom = 0.2f;
                camera.X = 3424;
                camera.Y = 1936;
                advance = false;
            }
            else if (!keyboard.IsKeyDown(Keys.LeftControl) && keyboard.IsKeyDown(Keys.D2) && !lastkeyboardupdate.IsKeyDown(Keys.D2))
            {
                // Load from slot 2
                if (File.Exists("saves/2"))
                {
                    grid = Load(grid, "2");
                    savestatus = "Loaded from slot 2";
                    camera.Zoom = 0.2f;
                    camera.X = 3424;
                    camera.Y = 1936;
                    advance = false;
                }
            }
            else if (keyboard.IsKeyDown(Keys.LeftControl) && keyboard.IsKeyDown(Keys.D3) && !lastkeyboardupdate.IsKeyDown(Keys.D3))
            {
                // save to slot 3
                Save(grid, "3");
                savestatus = "Saved to slot 3";
                camera.Zoom = 0.2f;
                camera.X = 3424;
                camera.Y = 1936;
                advance = false;
            }
            else if (!keyboard.IsKeyDown(Keys.LeftControl) && keyboard.IsKeyDown(Keys.D3) && !lastkeyboardupdate.IsKeyDown(Keys.D3))
            {
                // Load from slot 3
                if (File.Exists("saves/3"))
                {
                    grid = Load(grid, "3");
                    savestatus = "Loaded from slot 3";
                    camera.Zoom = 0.2f;
                    camera.X = 3424;
                    camera.Y = 1936;
                    advance = false;
                }
            }
            else if (keyboard.IsKeyDown(Keys.LeftControl) && keyboard.IsKeyDown(Keys.D4) && !lastkeyboardupdate.IsKeyDown(Keys.D4))
            {
                // save to slot 4
                Save(grid, "4");
                savestatus = "Saved to slot 4";
                camera.Zoom = 0.2f;
                camera.X = 3424;
                camera.Y = 1936;
                advance = false;
            }
            else if (!keyboard.IsKeyDown(Keys.LeftControl) && keyboard.IsKeyDown(Keys.D4) && !lastkeyboardupdate.IsKeyDown(Keys.D4))
            {
                // Load from slot 4
                if (File.Exists("saves/4"))
                {
                    grid = Load(grid, "4");
                    savestatus = "Loaded from slot 4";
                    camera.Zoom = 0.2f;
                    camera.X = 3424;
                    camera.Y = 1936;
                    advance = false;
                }
            }
            else if (keyboard.IsKeyDown(Keys.LeftControl) && keyboard.IsKeyDown(Keys.D5) && !lastkeyboardupdate.IsKeyDown(Keys.D5))
            {
                // save to slot 5
                Save(grid, "5");
                savestatus = "Saved to slot 5";
                camera.Zoom = 0.2f;
                camera.X = 3424;
                camera.Y = 1936;
                advance = false;
            }
            else if (!keyboard.IsKeyDown(Keys.LeftControl) && keyboard.IsKeyDown(Keys.D5) && !lastkeyboardupdate.IsKeyDown(Keys.D5))
            {
                // Load from slot 5
                if (File.Exists("saves/5"))
                {
                    grid = Load(grid, "5");
                    savestatus = "Loaded from slot 5";
                    camera.Zoom = 0.2f;
                    camera.X = 3424;
                    camera.Y = 1936;
                    advance = false;
                }
            }
            else if (keyboard.IsKeyDown(Keys.LeftControl) && keyboard.IsKeyDown(Keys.D6) && !lastkeyboardupdate.IsKeyDown(Keys.D6))
            {
                // save to slot 6
                Save(grid, "6");
                savestatus = "Saved to slot 6";
                camera.Zoom = 0.2f;
                camera.X = 3424;
                camera.Y = 1936;
                advance = false;
            }
            else if (!keyboard.IsKeyDown(Keys.LeftControl) && keyboard.IsKeyDown(Keys.D6) && !lastkeyboardupdate.IsKeyDown(Keys.D6))
            {
                // Load from slot 6
                if (File.Exists("6"))
                {
                    grid = Load(grid, "saves/6");
                    savestatus = "Loaded from slot 6";
                    camera.Zoom = 0.2f;
                    camera.X = 3424;
                    camera.Y = 1936;
                    advance = false;
                }
            }
            else if (keyboard.IsKeyDown(Keys.LeftControl) && keyboard.IsKeyDown(Keys.D7) && !lastkeyboardupdate.IsKeyDown(Keys.D7))
            {
                // save to slot 7
                Save(grid, "7");
                savestatus = "Saved to slot 7";
                camera.Zoom = 0.2f;
                camera.X = 3424;
                camera.Y = 1936;
                advance = false;
            }
            else if (!keyboard.IsKeyDown(Keys.LeftControl) && keyboard.IsKeyDown(Keys.D7) && !lastkeyboardupdate.IsKeyDown(Keys.D7))
            {
                // Load from slot 7
                if (File.Exists("saves/7"))
                {
                    grid = Load(grid, "7");
                    savestatus = "Loaded from slot 7";
                    camera.Zoom = 0.2f;
                    camera.X = 3424;
                    camera.Y = 1936;
                    advance = false;
                }
            }
            else if (keyboard.IsKeyDown(Keys.LeftControl) && keyboard.IsKeyDown(Keys.D8) && !lastkeyboardupdate.IsKeyDown(Keys.D8))
            {
                // save to slot 8
                Save(grid, "8");
                savestatus = "Saved to slot 8";
                camera.Zoom = 0.2f;
                camera.X = 3424;
                camera.Y = 1936;
                advance = false;
            }
            else if (!keyboard.IsKeyDown(Keys.LeftControl) && keyboard.IsKeyDown(Keys.D8) && !lastkeyboardupdate.IsKeyDown(Keys.D8))
            {
                // Load from slot 8
                if (File.Exists("saves/8"))
                {
                    grid = Load(grid, "8");
                    savestatus = "Loaded from slot 8";
                    camera.Zoom = 0.2f;
                    camera.X = 3424;
                    camera.Y = 1936;
                    advance = false;
                }
            }
            else if (keyboard.IsKeyDown(Keys.LeftControl) && keyboard.IsKeyDown(Keys.D9) && !lastkeyboardupdate.IsKeyDown(Keys.D9))
            {
                // save to slot 9
                Save(grid, "9");
                savestatus = "Saved to slot 9";
                camera.Zoom = 0.2f;
                camera.X = 3424;
                camera.Y = 1936;
                advance = false;
            }
            else if (!keyboard.IsKeyDown(Keys.LeftControl) && keyboard.IsKeyDown(Keys.D9) && !lastkeyboardupdate.IsKeyDown(Keys.D9))
            {
                // Load from slot 9
                if (File.Exists("saves/9"))
                {
                    grid = Load(grid, "9");
                    savestatus = "Loaded from slot 9";
                    camera.Zoom = 0.2f;
                    camera.X = 3424;
                    camera.Y = 1936;
                    advance = false;
                }
            }
            else if (keyboard.IsKeyDown(Keys.LeftControl) && keyboard.IsKeyDown(Keys.D0) && !lastkeyboardupdate.IsKeyDown(Keys.D0))
            {
                // save to slot 0
                Save(grid, "0");
                savestatus = "Saved to slot 0";
                camera.Zoom = 0.2f;
                camera.X = 3424;
                camera.Y = 1936;
                advance = false;
                
            }
            else if (!keyboard.IsKeyDown(Keys.LeftControl) && keyboard.IsKeyDown(Keys.D0) && !lastkeyboardupdate.IsKeyDown(Keys.D0))
            {
                // Load from slot 0
                if (File.Exists("saves/0"))
                {
                    grid = Load(grid, "0");
                    savestatus = "Loaded from slot 0";
                    camera.Zoom = 0.2f;
                    camera.X = 3424;
                    camera.Y = 1936;
                    advance = false;
                }
            }
            else if (keyboard.IsKeyDown(Keys.G) && !lastkeyboardupdate.IsKeyDown(Keys.G))
            {
                grid = Gosper(grid);
                camera.X = 1000;
                camera.Y = 600;
                camera.Zoom = 0.8f;
            }
            else if (keyboard.IsKeyDown(Keys.F11) && !lastkeyboardupdate.IsKeyDown(Keys.F11))
            {
                _graphics.ToggleFullScreen();
                camera = new Camera(_graphics.GraphicsDevice.Viewport);
                if (!showcontrols)
                {
                    camera.Zoom = 0.2f;
                }
            }
            else if (keyboard.IsKeyDown(Keys.Delete) && !lastkeyboardupdate.IsKeyDown(Keys.Delete))
            {
                islowframerate = !islowframerate;
                if (!islowframerate)
                {
                    this.TargetElapsedTime = TimeSpan.FromSeconds(1d / 60d);
                }
                else
                {
                    this.TargetElapsedTime = TimeSpan.FromSeconds(1d / 30d);
                }
            }
            else if (keyboard.IsKeyDown(Keys.C) && !lastkeyboardupdate.IsKeyDown(Keys.C))
            {
                showcontrols = true;
                camera.X = 3424;
                camera.Y = 1936;
                camera.Zoom = 1.5f;
                advance = false;
                status = "Waiting";
                UpdateDiscord(status);
            }
            if (mouse.ScrollWheelValue > lastmousestate.ScrollWheelValue && cellwidth > 4 && !showcontrols)
            {
                camerazoomspeed = 10f;
            }
            else if (mouse.ScrollWheelValue < lastmousestate.ScrollWheelValue && cellwidth < 32 && !showcontrols)
            {
                camerazoomspeed = -10f;
            }
            if (camerazoomspeed > 0)
            {
                camera.Zoom+=0.002f * camerazoomspeed;
                camerazoomspeed -= 1f;
            }
            else if (camerazoomspeed < 0)
            {
                camera.Zoom+=0.002f * camerazoomspeed;
                camerazoomspeed += 1f;
            }
            if (camera.Zoom < 0.18f)
            {
                camerazoomspeed+=2f;
            }
            else if (camera.Zoom > 1.5f)
            {
                camerazoomspeed-=2f;
            }
            if (camerazoomspeed < 1 && camerazoomspeed > -1)
            {
                camerazoomspeed = 0;
            }
            if (count != 4)
            {
                count++;
            }
            else
            {
                if (islowframerate)
                {
                    count = 3;
                }
                else
                {
                    count = 1;
                }
            }
            offset = offset + deltamoved;
            base.Update(gameTime);
        }

        protected override void Draw(GameTime gameTime)
        {
            string info = "Status: "+status+"    Live: "+GetAlive(grid)+"    Dead: "+GetDead(grid);
            GraphicsDevice.Clear(Color.Black);
            _spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, null, null, null, null, camera.Transform);
            if (showcontrols)
            {
                _spriteBatch.DrawString(font, (controlsmessage), new Vector2(3100, 1660), Color.White );
            }
            else
            {    // draw grid
                int y;
                int x;
                _spriteBatch.DrawString(infofont, info, new Vector2(30, -300), Color.White);
                _spriteBatch.DrawString(infofont, savestatus, new Vector2(30, 3900), Color.White);
                for (y = 1; y < height-1; y++)
                {
                    for (x = 1; x < width-1; x++)
                    {
                        if (grid[y,x] == true)
                        {
                            _spriteBatch.Draw(square, new Vector2 (x * cellwidth, y * cellwidth), null, Color.White, 0f, Vector2.Zero, 1f, SpriteEffects.None, 0f);
                        }
                        if (y == celly && x == cellx && !advance)
                        {
                            if (grid[y, x])
                            {
                                _spriteBatch.Draw(square, new Vector2 (x * cellwidth, y * cellwidth), null, Color.DarkGray, 0f, Vector2.Zero, 1f, SpriteEffects.None, 0f);
                            }
                            else
                            {
                                _spriteBatch.Draw(square, new Vector2 (x * cellwidth, y * cellwidth), null, Color.Gray, 0f, Vector2.Zero, 1f, SpriteEffects.None, 0f);
                            }
                        }
                    }
                }
                for (int i = 0; i < width-1; i++)
                {
                    _spriteBatch.Draw(square, new Vector2 (i * cellwidth, 0), null, Color.LightGray, 0f, Vector2.Zero, 1f, SpriteEffects.None, 0f);
                    _spriteBatch.Draw(square, new Vector2 (i * cellwidth, (height-1)*cellwidth), null, Color.LightGray, 0f, Vector2.Zero, 1f, SpriteEffects.None, 0f);
                }
                for (int j = 0; j < height; j++)
                {
                    _spriteBatch.Draw(square, new Vector2 (0, j * cellwidth), null, Color.LightGray, 0f, Vector2.Zero, 1f, SpriteEffects.None, 0f);
                    _spriteBatch.Draw(square, new Vector2 ((width-1)*cellwidth, j*cellwidth), null, Color.LightGray, 0f, Vector2.Zero, 1f, SpriteEffects.None, 0f);
                }

            }
            _spriteBatch.End();
            framerate = Math.Round(1 / gameTime.ElapsedGameTime.TotalSeconds);
            base.Draw(gameTime);
        }
        static int GetAlive(bool[,] grid)
        {
            int alive = 0;
            foreach (var x in grid)
            {
                if (x)
                {
                    alive++;
                }
            }
            if (alive > 102240)
            {
                alive = 102240;
            }
            return alive;
        }
        static int GetDead(bool[,] grid)
        {
            int dead = -1336;
            foreach (var x in grid)
            {
                if (!x)
                {
                    dead++;
                }
            }
            if (dead < 0)
            {
                dead = 0;
            }
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
            string savename = "saves/"+slot;
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
            string loadname = "saves/"+slot;
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
            for (int y = 0; y < 18; y++)
            {
                for (int x = 0; x < 40; x++)
                {
                    grid[y, x] = gosper.grid[y, x];
                }
            }
            advance = true;
            return grid;
        }
        void UpdateDiscord(string status)
        {
            client.SetPresence(new DiscordRPC.RichPresence()
            {
                Details = "Status: "+status,
                Assets = new DiscordRPC.Assets()
                {
                    LargeImageKey = "icon",
                    LargeImageText = "Conway's Game of Life"
                }
            });
        }
        void OnResize(Object sender, EventArgs e)
        {
            camera = new Camera(_graphics.GraphicsDevice.Viewport);
            camera.Zoom = 0.18f;
            if (showcontrols)
            {
                camera.Zoom = 1.5f;
            }
        }
    }
}
