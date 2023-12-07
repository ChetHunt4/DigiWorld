using DigiWorldGame.Data;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json.Serialization;

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

        //this stuff will probably get moved to a class of their own
        private bool _zoomed { get; set; }

        private int _x { get; set; }
        private int _y { get; set; }


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

            // TODO: Add your update logic here

            base.Update(gameTime);
        }

        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.CornflowerBlue);

            // TODO: Add your drawing code here
            if (!_zoomed)
            {
                Texture2D texture = _textures["ColorMap"];
                int x = _graphics.PreferredBackBufferWidth - texture.Width;
                int y = 0;

                _spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.Opaque);
                _shaders["Basic"].CurrentTechnique.Passes[0].Apply();
                _spriteBatch.Draw(texture, new Vector2(x, y), Color.White);
                _spriteBatch.End();
            }
            base.Draw(gameTime);
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
            _shaders.Add("Basic", basic);
        }
    }
}