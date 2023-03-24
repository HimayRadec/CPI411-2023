using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;


namespace Assignment03
{
    public class Assignment03 : Game
    {
        GraphicsDeviceManager graphics;
        SpriteBatch spriteBatch;
        SpriteFont font;
        Effect effect;
        //Skybox skybox;
        Matrix world = Matrix.Identity;
        Matrix view = Matrix.CreateLookAt(new Vector3(20, 0, 0), new Vector3(0, 0, 0), Vector3.UnitY);
        Matrix projection = Matrix.CreatePerspectiveFieldOfView(MathHelper.ToRadians(45), 800f / 600f, 0.1f, 100f);
        Vector3 cameraPosition;
        Vector3 lightPosition;

        float angle = 0;
        float angle2 = 0;
        float angleL = 0;
        float angleL2 = 0;
        float distance = 30;
        MouseState preMouse;

        Vector3 viewVector;
        Vector3 cameraTarget = new Vector3(0, 0, 0);

        Model model;
        Texture2D texture;
        Texture2D art;
        Texture2D bumpTest;
        Texture2D crossHatch;
        Texture2D monkey;
        Texture2D nm;
        Texture2D round;
        Texture2D saint;
        Texture2D science;
        Texture2D square;

        float height = 1.0f;


        public Assignment03()
        {
            graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";
            // ***** From MonoGame3.6 Need this statement
            graphics.GraphicsProfile = GraphicsProfile.HiDef;
            // **********************************************
        }
        protected override void Initialize()
        {
            base.Initialize();
            texture = round;
        }

        protected override void LoadContent()
        {
            spriteBatch = new SpriteBatch(GraphicsDevice);
            //font = Content.Load<SpriteFont>("font");
            model = Content.Load<Model>("Plane");
            effect = Content.Load<Effect>("BumpMap");

            art = Content.Load<Texture2D>("NormalsMaps/art");
            bumpTest = Content.Load<Texture2D>("NormalsMaps/BumpTest");
            crossHatch = Content.Load<Texture2D>("NormalsMaps/crossHatch");
            monkey = Content.Load<Texture2D>("NormalsMaps/monkey");
            //nm = Content.Load<Texture2D>("NormalsMaps/nm");
            round = Content.Load<Texture2D>("NormalsMaps/round");
            saint = Content.Load<Texture2D>("NormalsMaps/saint");
            science = Content.Load<Texture2D>("NormalsMaps/science");
            square = Content.Load<Texture2D>("NormalsMaps/square");

        }

        protected override void UnloadContent() { }

        protected override void Update(GameTime gameTime)
        {
            if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed || Keyboard.GetState().IsKeyDown(Keys.Escape))
                Exit();

            cameraPosition = distance * new Vector3((float)System.Math.Sin(angle), 0, (float)System.Math.Cos(angle));

            if (Keyboard.GetState().IsKeyDown(Keys.Left)) angleL += 0.02f;
            if (Keyboard.GetState().IsKeyDown(Keys.Right)) angleL -= 0.02f;
            if (Keyboard.GetState().IsKeyDown(Keys.Up)) angleL2 += 0.02f;
            if (Keyboard.GetState().IsKeyDown(Keys.Down)) angleL2 -= 0.02f;

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

            // Reset camera and light
            if (Keyboard.GetState().IsKeyDown(Keys.S))
            {
                distance = 30;
                angle = 0;
                angle2 = 0;
                angleL = 0;
                angleL2 = 0;
            }

            // Image Loader
            if (Keyboard.GetState().IsKeyDown(Keys.D1)) texture = art;
            if (Keyboard.GetState().IsKeyDown(Keys.D2)) texture = bumpTest;
            if (Keyboard.GetState().IsKeyDown(Keys.D3)) texture = crossHatch;
            if (Keyboard.GetState().IsKeyDown(Keys.D4)) texture = monkey;
            // if (Keyboard.GetState().IsKeyDown(Keys.D5)) texture = art;
            if (Keyboard.GetState().IsKeyDown(Keys.D6)) texture = round;
            if (Keyboard.GetState().IsKeyDown(Keys.D7)) texture = saint;
            if (Keyboard.GetState().IsKeyDown(Keys.D8)) texture = science;
            if (Keyboard.GetState().IsKeyDown(Keys.D9)) texture = square;

            // Hieght
            if (Keyboard.GetState().IsKeyDown(Keys.W) && !Keyboard.GetState().IsKeyDown(Keys.LeftShift)) height += 0.1f;
            if (Keyboard.GetState().IsKeyDown(Keys.W) && Keyboard.GetState().IsKeyDown(Keys.LeftShift)) height -= 0.1f;

            preMouse = Mouse.GetState();
            // Update Camera
            cameraPosition = Vector3.Transform(new Vector3(0, 0, distance),
                Matrix.CreateRotationX(angle2) * Matrix.CreateRotationY(angle) * Matrix.CreateTranslation(cameraTarget)
                );
            view = Matrix.CreateLookAt(cameraPosition, 
                cameraTarget, 
                Vector3.Transform(Vector3.UnitY, Matrix.CreateRotationX(angle2) * Matrix.CreateRotationY(angle)));

            // Update Light
            lightPosition = Vector3.Transform(
                new Vector3(0, 0, 10),
                Matrix.CreateRotationX(angleL2) * Matrix.CreateRotationY(angleL));

            base.Update(gameTime);
        }

        /// <summary>
        /// This is called when the game should draw itself.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.CornflowerBlue);

            GraphicsDevice.BlendState = BlendState.Opaque;
            GraphicsDevice.DepthStencilState = new DepthStencilState();

            effect.CurrentTechnique = effect.Techniques[0];

            foreach (EffectPass pass in effect.CurrentTechnique.Passes)
            {
                foreach (ModelMesh mesh in model.Meshes)
                {
                    foreach (ModelMeshPart part in mesh.MeshParts)
                    {
                        effect.Parameters["World"].SetValue(mesh.ParentBone.Transform);
                        effect.Parameters["View"].SetValue(view);
                        effect.Parameters["Projection"].SetValue(projection);
                        Matrix worldInverseTransposeMatrix = Matrix.Transpose(Matrix.Invert(mesh.ParentBone.Transform));
                        effect.Parameters["WorldInverseTranspose"].SetValue(worldInverseTransposeMatrix);

                        effect.Parameters["CameraPosition"].SetValue(cameraPosition);
                        effect.Parameters["LightPosition"].SetValue(lightPosition);
                        effect.Parameters["DiffuseIntensity"].SetValue(1.0f);
                        effect.Parameters["DiffuseColor"].SetValue(new Vector4(1.0f, 1.0f, 1.0f, 1.0f));
                        effect.Parameters["SpecularIntensity"].SetValue(1.0f);
                        effect.Parameters["SpecularColor"].SetValue(new Vector4(1.0f, 1.0f, 1.0f, 1.0f));
                        effect.Parameters["Shininess"].SetValue(100.0f);

                        effect.Parameters["height"].SetValue(height);
                        effect.Parameters["normalMap"].SetValue(texture);

                        pass.Apply();
                        GraphicsDevice.SetVertexBuffer(part.VertexBuffer);
                        GraphicsDevice.Indices = part.IndexBuffer;

                        GraphicsDevice.DrawIndexedPrimitives(
                            PrimitiveType.TriangleList,
                            part.VertexOffset,
                            part.StartIndex,
                            part.PrimitiveCount);
                    }
                }
            }
        }
    }
}
