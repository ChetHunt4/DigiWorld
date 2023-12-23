using DigiWorldGame.Data;
using DigiWorldLib.World;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json.Serialization;
using System.Threading;

namespace DigiWorldGame
{
    public class Game1 : Game
    {
        private GraphicsDeviceManager _graphics;
        private SpriteBatch _spriteBatch;

        private string _projectDirectory { get; set; }
        private Configuration _configuration { get; set; }

        private Dictionary<string, Texture2D> _textures { get; set; }
        private Dictionary<string, Effect> _shaders { get; set; }

        private bool _wasClicked { get; set; }

        private RenderTarget2D _detailTexture { get; set; }
        private RenderTarget2D _thumbnailTexture { get; set; }

        //this stuff will probably get moved to a class of their own
        private bool _zoomed { get; set; }

        private int _mouseX { get; set; }
        private int _mouseY { get; set; }
        private int _mapSize { get; set; }
        private int _mapX { get; set; }
        private int _mapY { get; set; }
        private float _gridSize { get; set; }
        private int _gridX { get; set; }
        private int _gridY { get; set; }

        private SubTile _currentSubTile { get; set; }

        public Game1()
        {
            _graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";
            IsMouseVisible = true;
        }

        protected override void Initialize()
        {
            var workingDirectory = Environment.CurrentDirectory;
            _projectDirectory = Directory.GetParent(workingDirectory).Parent.Parent.FullName;
            var configText = File.ReadAllText(Path.Combine(_projectDirectory, "config.json"));
            if (!string.IsNullOrWhiteSpace(configText))
            {
                var settings = JsonConvert.DeserializeObject<Configuration>(configText);
                _configuration = settings;
            }
            if (_configuration != null)
            {
                _graphics.PreferredBackBufferWidth = _configuration.VideoSettings.Width;
                _graphics.PreferredBackBufferHeight = _configuration.VideoSettings.Height;
                _graphics.IsFullScreen = _configuration.VideoSettings.IsFullscreen;
                _graphics.ApplyChanges();
                initializeValues();
            }
            else
            {
                //could not load config file
                Exit();
            }
            loadGfx();
            base.Initialize();
        }

        protected override void LoadContent()
        {
            _spriteBatch = new SpriteBatch(GraphicsDevice);

            // TODO: use this.Content to load your game content here
        }

        protected override void Update(GameTime gameTime)
        {
            if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed || Keyboard.GetState().IsKeyDown(Keys.Escape))
                Exit();

            var mouseState = Mouse.GetState();

            _mouseX = mouseState.X;
            _mouseY = mouseState.Y;



            var mouseClickState = mouseState.LeftButton;
            if (!_zoomed)
            {
                _gridX = (int)((_mouseX - _mapX) / _gridSize);
                _gridY = (int)(_mouseY / _gridSize);
                if (mouseClickState == ButtonState.Pressed)
                {
                    _wasClicked = true;
                }
                else if (mouseClickState == ButtonState.Released && _wasClicked)
                {
                    _zoomed = !_zoomed;
                    _wasClicked = false;
                    loadTile(_gridX, _gridY, _configuration.TileName);
                }
            }

            base.Update(gameTime);
        }

        protected override void Draw(GameTime gameTime)
        {



            MouseState mouseState = Mouse.GetState();


            // TODO: Add your drawing code here
            if (!_zoomed)
            {
                drawZoomedOut(_mapX, _mapY, _mapSize, _mouseX, _mouseY, _gridX, _gridY, _gridSize);
            }
            else
            {
                drawZoomedIn(_mapX, _mapY, _mapSize, _mouseX, _mouseY, _gridX, _gridY, 32);
            }
            base.Draw(gameTime);
        }

        private void initializeValues()
        {
            _mapSize = _graphics.PreferredBackBufferHeight;
            _gridSize = _mapSize / 31;
            _mapX = _graphics.PreferredBackBufferWidth - _graphics.PreferredBackBufferHeight;
            _mapY = 0;
        }

        private void loadGfx()
        {
            _textures = new Dictionary<string, Texture2D>();
            Texture2D colorMap = Content.Load<Texture2D>("Gfx//ColorMap//WorldColorMapWater");
            Texture2D grassTile = Content.Load<Texture2D>("Gfx//WorldTiles//GrassTile");
            Texture2D waterTile = Content.Load<Texture2D>("Gfx//WorldTiles//WaterTile");
            _textures.Add("ColorMap", colorMap);
            _textures.Add("GrassTile", grassTile);
            _textures.Add("WaterTile", waterTile);

            _shaders = new Dictionary<string, Effect>();
            Effect basic = Content.Load<Effect>("Shaders//BasicColorRender");
            Effect overlay = Content.Load<Effect>("Shaders//ColorOverlay");
            Effect combine = Content.Load<Effect>("Shaders//CombineEffect");
            _shaders.Add("Basic", basic);
            _shaders.Add("Color", overlay);
            _shaders.Add("Combine", combine);
        }

        private void loadTile(int x, int y, string tilename)
        {
            var path = Path.Combine(_projectDirectory, "GameWorldData", tilename + "_" + x + "_" + y + ".json");
            var fileText = File.ReadAllText(path);
            _currentSubTile = JsonConvert.DeserializeObject<SubTile>(fileText);
        }

        private void drawZoomedOut(int x, int y, int size, int mouseX, int mouseY, int gridX, int gridY, float gridSize)
        {
            GraphicsDevice.Clear(Color.CornflowerBlue);
            Texture2D texture = _textures["ColorMap"];
            _spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.Opaque);
            _shaders["Basic"].CurrentTechnique.Passes[0].Apply();
            _spriteBatch.Draw(texture, new Rectangle(new Point(x, y), new Point(size, size)), Color.White);
            if (mouseX > x && mouseY > 0)
            {
                _shaders["Color"].CurrentTechnique.Passes[0].Apply();

                var overlayX = (int)(x + (gridX * gridSize));
                var overlayY = (int)(y + (gridY * gridSize));
                _spriteBatch.Draw(texture, new Rectangle(new Point(overlayX, overlayY), new Point((int)gridSize, (int)gridSize)), new Color(168, 158, 50, 86));
            }
            _spriteBatch.End();
        }

        private void initZoomedOut()
        {

        }

        private void drawZoomedIn(int x, int y, int size, int mouseX, int mouseY, int gridX, int gridY, float gridSize)
        {
            if (_detailTexture == null && _thumbnailTexture == null)
            {
                Texture2D colorTexture = _textures["ColorMap"];
                var waterResources = _currentSubTile.Resources.Where(w => w.ResourceName == "Ocean Water" || w.ResourceName == "Fresh Water").SelectMany(s => s.ResourceLocations).Distinct().ToList();
                float sourceX = (gridX / gridSize);
                float sourceY = (gridY / gridSize);
                int textureX = (int)(sourceX * colorTexture.Width);
                int textureY = (int)(sourceY * colorTexture.Height);
                _spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.Opaque);
                _shaders["Basic"].CurrentTechnique.Passes[0].Apply();

                _detailTexture = new RenderTarget2D(_graphics.GraphicsDevice, colorTexture.Width, colorTexture.Height, false, _graphics.PreferredBackBufferFormat, DepthFormat.Depth24);
                _thumbnailTexture = new RenderTarget2D(_graphics.GraphicsDevice, colorTexture.Width, colorTexture.Height, false, _graphics.PreferredBackBufferFormat, DepthFormat.Depth24);

                GraphicsDevice.SetRenderTarget(_detailTexture);

                GraphicsDevice.DepthStencilState = new DepthStencilState() { DepthBufferEnable = true };
                GraphicsDevice.Clear(Color.LightGray);
                float spaceX = _graphics.PreferredBackBufferWidth / gridSize;
                float spaceY = _graphics.PreferredBackBufferHeight / gridSize;

                for (int texy = 0; texy < gridSize; texy++)
                {
                    for (int texx = 0; texx < gridSize; texx++)
                    {
                        var rectangle = new Rectangle(new Point((int)(texx * spaceX), (int)(texy * spaceY)), new Point((int)spaceX + 1, (int)spaceY + 1));
                        if (waterResources.Contains(new System.Numerics.Vector2((gridX * gridSize) + texx, (gridY * gridSize) + texy)))
                        {
                            _spriteBatch.Draw(_textures["WaterTile"], rectangle, Color.White);
                        }
                        else
                        {
                            _spriteBatch.Draw(_textures["GrassTile"], rectangle, Color.White);
                        }
                    }
                }
                _spriteBatch.End();
                GraphicsDevice.SetRenderTarget(_thumbnailTexture);
                GraphicsDevice.Clear(Color.CornflowerBlue);
                _spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.Opaque);

                var sourceRectangle = new Rectangle(new Point(textureX, textureY), new Point((int)gridSize, (int)gridSize));
                _spriteBatch.Draw(colorTexture, new Rectangle(new Point(0, 0), new Point(colorTexture.Width, colorTexture.Height)), sourceRectangle, Color.White);
                _spriteBatch.End();
                GraphicsDevice.SetRenderTarget(null);
                GraphicsDevice.Clear(Color.CornflowerBlue);
            }
            _spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.Opaque);
            _shaders["Combine"].CurrentTechnique.Passes[0].Apply();
            //_shaders["Combine"].Parameters["BGTexture"].SetValue(bgTexture);
            _shaders["Combine"].Parameters["ForeTexture"].SetValue(_thumbnailTexture);
            _spriteBatch.Draw(_detailTexture, new Rectangle(new Point(x, y), new Point(size, size)), Color.White);
            _spriteBatch.End();
        }

    }
}