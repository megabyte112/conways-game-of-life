using System;
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
        static int height;
        static int width;
        const int cellwidth = 16;
        bool[,] grid;
        int[,] adjacent;
        MouseState mouse = Mouse.GetState();
        KeyboardState keyboard = Keyboard.GetState();
        bool advance = true;
        string status = "Running  (Esc = Exit, Enter = Random, Backspace = Clear, Space = Pause)";
        public gameoflife()
        {
            _graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";
            
            // limit framerate to 15fps
            this.IsFixedTimeStep = true;
            this.TargetElapsedTime = TimeSpan.FromSeconds(1d / 15d);

            IsMouseVisible = true;
        }

        protected override void Initialize()
        {
            // how many squares should be used depending on screen resolution
            width = (int)Math.Floor(Convert.ToDouble(GraphicsAdapter.DefaultAdapter.CurrentDisplayMode.Width / cellwidth)) - 10;
            height = (int)Math.Floor(Convert.ToDouble(GraphicsAdapter.DefaultAdapter.CurrentDisplayMode.Height / cellwidth))- 6;
            grid = new bool[height, width];
            adjacent = new int[height, width];

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

            // set window size
            _graphics.PreferredBackBufferHeight = height * 16;
            _graphics.PreferredBackBufferWidth = width * 16;
            _graphics.ApplyChanges();
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
            if (advance)    // only run when not paused
            {
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
            else if (lastmousestate.LeftButton == ButtonState.Released && mouse.LeftButton == ButtonState.Pressed)
            {
                // mouseposition divided by cell width gives you the coordinate of the cell
                int cellx = (int)Math.Floor(Convert.ToDouble(mouse.X / cellwidth));
                int celly = (int)Math.Floor(Convert.ToDouble(mouse.Y / cellwidth));

                // toggle state of cell
                if (grid[celly, cellx] == false)
                {
                    grid[celly, cellx] = true;
                }
                else
                {
                    grid[celly, cellx] = false;
                }
            }
            else if (keyboard.IsKeyDown(Keys.Enter) && !lastkeyboardupdate.IsKeyDown(Keys.Enter))
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
            }
            else if (keyboard.IsKeyDown(Keys.Space) && !lastkeyboardupdate.IsKeyDown(Keys.Space))
            {
                if (advance)
                {
                    advance = false;
                    status = "Paused";
                }
                else
                {
                    advance = true;
                    status = "Running";
                }
            }
            else if (keyboard.IsKeyDown(Keys.Back))
            {
                for (int y = 0; y < height; y++)
                {
                    for (int x = 0; x < width; x++)
                    {
                        grid[y, x] = false;
                    }
                }
            }
            base.Update(gameTime);
        }

        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.Black);
            _spriteBatch.Begin();
            _spriteBatch.DrawString(font, (" Live: "+GetAlive(grid)+"    Dead: " + GetDead(grid)
            + "    Status: "+ status), Vector2.Zero, Color.White);
            for (int y = 2; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    if (grid[y,x] == true)
                    {
                        _spriteBatch.Draw(square, new Vector2 (x * cellwidth, y * cellwidth), Color.White);
                    }
                }
            }
            _spriteBatch.End();
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
            return alive;
        }
        static int GetDead(bool[,] grid)
        {
            int dead = 0;
            foreach (var x in grid)
            {
                if (!x)
                {
                    dead++;
                }
            }
            return dead;
        }
        static int[,] GetAdjacent(bool[,] grid)
        {
            int[,] adjacent = new int[height, width];
            int value;
            for (int y = 2; y < height - 1; y++)
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
    }
}
