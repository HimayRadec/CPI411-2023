using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace Assignment04
{
    public class Assignment04 : Game
    {
        private GraphicsDeviceManager _graphics;
        private SpriteBatch _spriteBatch;
        // **** TEMPLATE ************//
        SpriteFont font;
        Effect effect;
        Matrix world = Matrix.Identity;
        Matrix view = Matrix.CreateLookAt(new Vector3(20, 0, 0), new Vector3(0, 0, 0), Vector3.UnitY);
        Matrix projection = Matrix.CreatePerspectiveFieldOfView(MathHelper.ToRadians(45), 800f / 600f, 0.1f, 100f);
        Vector3 cameraPosition, cameraTarget, lightPosition;
        Matrix lightView, lightProjection;
        float angle = 0;
        float angle2 = 0;
        float angleL = 0;
        float angleL2 = 0;
        float distance = 40;
        MouseState preMouse;
        Model model;
        Model[] models;
        Texture2D texture;
        // **** TEMPLATE ************//
        VertexPositionTexture[] vertices =
        {
            new VertexPositionTexture(new Vector3(0, 0, -20), new Vector2(0,0)),
            new VertexPositionTexture(new Vector3(0, 0, 20), new Vector2(1,0)),
            new VertexPositionTexture(new Vector3(20, 0, 0), new Vector2(1,1)),
            new VertexPositionTexture(new Vector3(-20, 0, 0), new Vector2(0,1))
        };

        public Assignment04()
        {
            _graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";
            IsMouseVisible = true;
            _graphics.GraphicsProfile = GraphicsProfile.HiDef;

        }

        protected override void Initialize()
        {
            // TODO: Add your initialization logic here

            base.Initialize();
        }

        protected override void LoadContent()
        {
            _spriteBatch = new SpriteBatch(GraphicsDevice);

            // TODO: use this.Content to load your game content here
            effect = Content.Load<Effect>("SimplestRotate");

        }

        protected override void Update(GameTime gameTime)
        {
            if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed || Keyboard.GetState().IsKeyDown(Keys.Escape))
                Exit();

            // TODO: Add your update logic here
            #region - TEMPLATE -
            // ************ TEMPLATE ************ //
            if (Keyboard.GetState().IsKeyDown(Keys.Left)) angleL += 0.02f;
            if (Keyboard.GetState().IsKeyDown(Keys.Right)) angleL -= 0.02f;
            if (Keyboard.GetState().IsKeyDown(Keys.Up)) angleL2 += 0.02f;
            if (Keyboard.GetState().IsKeyDown(Keys.Down)) angleL2 -= 0.02f;
            if (Keyboard.GetState().IsKeyDown(Keys.S)) { angle = angle2 = angleL = angleL2 = 0; distance = 30; cameraTarget = Vector3.Zero; }
            if (Mouse.GetState().LeftButton == ButtonState.Pressed)
            {
                angle -= (Mouse.GetState().X - preMouse.X) / 100f;
                angle2 += (Mouse.GetState().Y - preMouse.Y) / 100f;
            }
            if (Mouse.GetState().RightButton == ButtonState.Pressed)
            {
                distance += (Mouse.GetState().X - preMouse.X) / 100f;
            }

            if (Mouse.GetState().MiddleButton == ButtonState.Pressed)
            {
                Vector3 ViewRight = Vector3.Transform(Vector3.UnitX,
                    Matrix.CreateRotationX(angle2) * Matrix.CreateRotationY(angle));
                Vector3 ViewUp = Vector3.Transform(Vector3.UnitY,
                    Matrix.CreateRotationX(angle2) * Matrix.CreateRotationY(angle));
                cameraTarget -= ViewRight * (Mouse.GetState().X - preMouse.X) / 10f;
                cameraTarget += ViewUp * (Mouse.GetState().Y - preMouse.Y) / 10f;
            }
            preMouse = Mouse.GetState();
            // Update Camera
            cameraPosition = Vector3.Transform(new Vector3(0, 0, distance),
                Matrix.CreateRotationX(angle2) * Matrix.CreateRotationY(angle) * Matrix.CreateTranslation(cameraTarget));
            view = Matrix.CreateLookAt(
                cameraPosition,
                cameraTarget,
                Vector3.Transform(Vector3.UnitY, Matrix.CreateRotationX(angle2) * Matrix.CreateRotationY(angle)));

            // Update Light
            lightPosition = Vector3.Transform(
                new Vector3(0, 0, 10),
                Matrix.CreateRotationX(angleL2) * Matrix.CreateRotationY(angleL));

            lightView = Matrix.CreateLookAt(
                lightPosition,
                Vector3.Zero,
                Vector3.Transform(
                    Vector3.UnitY,
                    Matrix.CreateRotationX(angleL2) * Matrix.CreateRotationY(angleL)));
            lightProjection =
                Matrix.CreatePerspectiveFieldOfView(MathHelper.PiOver2, 1f, 1f, 50f);
            // ********************************** //
            #endregion

            effect.Parameters["World"].SetValue(world);
            effect.Parameters["View"].SetValue(view);
            effect.Parameters["Projection"].SetValue(projection);

            base.Update(gameTime);
        }

        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.CornflowerBlue);


            // TODO: Add your drawing code here
            foreach (var pass in effect.CurrentTechnique.Passes)
            {
                pass.Apply();
                GraphicsDevice.DrawUserPrimitives<VertexPositionTexture>(
                  PrimitiveType.TriangleList, vertices, 0, vertices.Length / 4);
            }
            base.Draw(gameTime);
        }
    }
}