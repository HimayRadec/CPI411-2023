using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

using CPI411.SimpleEngine;

namespace Lab05
{
    public class Lab05 : Game
    {
        private GraphicsDeviceManager _graphics;
        private SpriteBatch _spriteBatch;

        float xSensitivity = 0.01f;
        float ySensitivity = 0.01f;
        float angle;
        float angle2;

        Matrix view;
        Matrix world;
        Matrix projection;
        Vector3 cameraPosition;

        Skybox skybox;
        string[] skyboxTextures = {
                "skybox/SunsetPNG1",
                "skybox/SunsetPNG2",
                "skybox/SunsetPNG3",
                "skybox/SunsetPNG4",
                "skybox/SunsetPNG5",
                "skybox/SunsetPNG6"
            };  

        public Lab05()
        {
            _graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";
            IsMouseVisible = true;
        }

        protected override void Initialize()
        {
            // TODO: Add your initialization logic here

            base.Initialize();
        }

        protected override void LoadContent()
        {
            _spriteBatch = new SpriteBatch(GraphicsDevice);



            skybox = new Skybox(skyboxTextures, Content, _graphics.GraphicsDevice);
        }

        protected override void Update(GameTime gameTime)
        {
            if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed || Keyboard.GetState().IsKeyDown(Keys.Escape))
                Exit();

            // TODO: Add your update logic here
            world = Matrix.Identity;
            cameraPosition = Vector3.Transform(
                new Vector3(0, 0, 20),
                Matrix.CreateRotationX(angle2) * Matrix.CreateRotationY(angle)
                );
            view = Matrix.CreateLookAt(
                cameraPosition,
                new Vector3(),
                Vector3.Up
                );
            projection = Matrix.CreatePerspectiveFieldOfView(
                MathHelper.ToRadians(90),
                1.33f,
                0.1f,
                100
                );

            base.Update(gameTime);
        }

        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.CornflowerBlue);

            skybox.Draw(view, projection, cameraPosition);

            base.Draw(gameTime);
        }
    }
}