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
        static int height = 60;
        static int width = 100;
        const int cellwidth = 16;
        bool[,] grid = new bool[height, width];
        int[,] adjacent = new int[height, width];
        MouseState mouse = Mouse.GetState();
        KeyboardState keyboard = Keyboard.GetState();
        bool advance = true;
        int timesrestarted = 0;
        public gameoflife()
        {
            _graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";
            this.IsFixedTimeStep = true;
            this.TargetElapsedTime = TimeSpan.FromSeconds(1d / 15d);
            IsMouseVisible = true;
        }

        protected override void Initialize()
        {
            _graphics.PreferredBackBufferHeight = height * cellwidth;
            _graphics.PreferredBackBufferWidth = width * cellwidth;
            _graphics.ApplyChanges();

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

            base.Initialize();
        }

        protected override void LoadContent()
        {
            _spriteBatch = new SpriteBatch(GraphicsDevice);
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
            if (advance)
            {
                for (int y = 0; y < height; y++)
                {
                    for (int x = 0; x < width; x++)
                    {
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
                timesrestarted = 0;
            }
            if (Keyboard.GetState().IsKeyDown(Keys.Escape))
                Exit();
            else if (lastmousestate.LeftButton == ButtonState.Released && mouse.LeftButton == ButtonState.Pressed)
            {
                int cellx = (int)Math.Floor(Convert.ToDouble(mouse.X / cellwidth));
                int celly = (int)Math.Floor(Convert.ToDouble(mouse.Y / cellwidth));
                if (grid[celly, cellx] == false)
                {
                    grid[celly, cellx] = true;
                }
                else
                {
                    grid[celly, cellx] = false;
                }
            }
            else if (Keyboard.GetState().IsKeyDown(Keys.Enter) && !lastkeyboardupdate.IsKeyDown(Keys.Enter) && timesrestarted == 0)
            {
                timesrestarted++;
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
            }
            else if(Keyboard.GetState().IsKeyDown(Keys.Space) && !lastkeyboardupdate.IsKeyDown(Keys.Space))
            {
                if (advance)
                {
                    advance = false;
                }
                else
                {
                    advance = true;
                }
            }
            base.Update(gameTime);
        }

        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.Black);
            _spriteBatch.Begin();
            _spriteBatch.DrawString(font, (" Time: " + (gameTime.TotalGameTime).ToString().Substring(0, 8) + 
            "    Alive: "+GetAlive(grid)+"    Dead: " + GetDead(grid)), Vector2.Zero, Color.White);
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
