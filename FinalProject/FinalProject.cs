using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;
using System.Security.Cryptography;

namespace FinalProject
{
    public class FinalProject : Game
    {
        private GraphicsDeviceManager _graphics;
        private SpriteBatch _spriteBatch;

        bool test;
        // **** TEMPLATE ************//
        SpriteFont font;
        Effect effect;
        Matrix world = Matrix.Identity;
        Matrix view = Matrix.CreateLookAt(new Vector3(20, 0, 0), new Vector3(0, 0, 0), Vector3.UnitY);
        Matrix projection = Matrix.CreatePerspectiveFieldOfView(
            MathHelper.ToRadians(45), 
            800f / 600f, 
            0.01f, 
            1000f);
        Vector3 cameraPosition, cameraTarget, lightPosition;
        Matrix lightView, lightProjection;
        Vector4 ambient = new Vector4(0, 0, 0, 0);
        Vector4 specularColor = new Vector4(1, 1, 1, 1);
        float ambientIntensity = 0;
        Vector4 diffuseColor = new Vector4(1, 1, 1, 1);
        float angle = 0;
        float angle2 = 0;
        float angleL = 0;
        float angleL2 = 0;
        float distance = 25;
        MouseState preMouse;
        Model model;
        // **** TEMPLATE ************//

        KeyboardState previousKeyboardState;
        KeyboardState currentKeyboardState;

        int currentTechnique = 0;

        Vector2 lastWindSpeed;
        Random random = new Random();
        Vector2 newWindSpeed;
        Vector2 currentWindSpeed;
        float timeSinceLastThing;
        float totalTime;
        float time;

        private float detailBranchAmplitude = 0.05f;
        private float detailSideToSideAmplitude = 0.05f;
        private float mainBendScale = 0.01f;
        private bool detailBranchOn = true;
        private bool detailSideToSideOn = true;
        private bool mainBendOn = true;
        private bool pauseWind = false;

        Texture2D plantTexture;


        private bool IsKeyPressed(Keys key)
        {
            return !previousKeyboardState.IsKeyDown(key) && currentKeyboardState.IsKeyDown(key);
        }


        public FinalProject()
        {
            _graphics = new GraphicsDeviceManager(this);
            _graphics.PreferredBackBufferWidth = GraphicsAdapter.DefaultAdapter.CurrentDisplayMode.Width; // Set the width of the window to the user's screen width
            _graphics.PreferredBackBufferHeight = GraphicsAdapter.DefaultAdapter.CurrentDisplayMode.Height; // Set the height of the window to the user's screen height

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
            model = Content.Load<Model>("BananaTree2");
            effect = Content.Load<Effect>("Bending");
            plantTexture = Content.Load<Texture2D>("logo_mg");
            font = Content.Load<SpriteFont>("UI");




            // TODO: use this.Content to load your game content here
        }

        protected override void Update(GameTime gameTime)
        {
            previousKeyboardState = currentKeyboardState;
            currentKeyboardState = Keyboard.GetState();

            if (IsKeyPressed(Keys.Escape)) Exit();

            // Update the time value based on the elapsed game time
            time += (float)gameTime.ElapsedGameTime.TotalSeconds;
            effect.Parameters["Time"].SetValue(time); // update the time value in the constant buffer

            if (Keyboard.GetState().IsKeyDown(Keys.Escape) ||
                GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed)
            {
                Exit();
            }


            

            CameraControls();
            LightControls();
            ControlParameters();
            SwapTechnique();

            //float time = (float)gameTime.ElapsedGameTime.TotalMilliseconds;

            // Change wind every second, and smoothly interpolate to the new strength/direction.
            float timeEllapsed = (float)gameTime.ElapsedGameTime.TotalSeconds;
            totalTime += timeEllapsed;
            timeSinceLastThing -= timeEllapsed;
            if (!pauseWind)
            {
                CalculateWindSpeed();
            }


            base.Update(gameTime);
        }



        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.CornflowerBlue);
            RasterizerState rasterizerState = new RasterizerState
            {
                CullMode = CullMode.None
            };
            GraphicsDevice.RasterizerState = rasterizerState;

            // TODO: Add your drawing code here
            effect.CurrentTechnique = effect.Techniques[currentTechnique];
            foreach (EffectPass pass in effect.CurrentTechnique.Passes)
            {
                foreach (ModelMesh mesh in model.Meshes)
                {
                    foreach (ModelMeshPart part in mesh.MeshParts)
                    {
                        effect.Parameters["World"].SetValue(mesh.ParentBone.Transform);
                        effect.Parameters["View"].SetValue(view);
                        effect.Parameters["Projection"].SetValue(projection);
                        // Matrix worldInverseTranspose = Matrix.Transpose(Matrix.Invert(mesh.ParentBone.Transform));
                        // effect.Parameters["WorldInverseTranspose"].SetValue(worldInverseTranspose);

                        // effect.Parameters["AmbientColor"].SetValue(ambient);
                        // effect.Parameters["AmbientIntensity"].SetValue(ambientIntensity);

                        // effect.Parameters["DiffuseColor"].SetValue(diffuseColor);
                        // effect.Parameters["DiffuseIntensity"].SetValue(1f);

                        //effect.Parameters["Shininess"].SetValue(20f);
                        // effect.Parameters["SpecularColor"].SetValue(specularColor);
                        // effect.Parameters["SpecularIntensity"].SetValue(1);

                        // effect.Parameters["LightPosition"].SetValue(lightPosition);
                        // effect.Parameters["CameraPosition"].SetValue(cameraPosition);

                        effect.Parameters["Amplitude"].SetValue(0.1f);
                        effect.Parameters["Frequency"].SetValue(10f);

                        effect.Parameters["WindSpeed"].SetValue(currentWindSpeed);
                        effect.Parameters["Time"].SetValue(totalTime);
                        effect.Parameters["BranchAmplitude"].SetValue(detailBranchOn ? detailBranchAmplitude : 0f);
                        effect.Parameters["DetailAmplitude"].SetValue(detailSideToSideOn ? detailSideToSideAmplitude : 0f);
                        effect.Parameters["BendScale"].SetValue(mainBendOn ? mainBendScale : 0f);

                        effect.Parameters["Texture"].SetValue(plantTexture);

                        //TODO: throw into it's own function that I can toggle on and off using a boolean

                        ///effect.Parameters["InvertNormal"].SetValue(false);
                        ///GraphicsDevice.RasterizerState = RasterizerState.CullCounterClockwise;
                        ///effect.CurrentTechnique.Passes[0].Apply();

                        ///effect.Parameters["InvertNormal"].SetValue(true);
                        ///GraphicsDevice.RasterizerState = RasterizerState.CullClockwise;
                        ///effect.CurrentTechnique.Passes[0].Apply();
                        ///
                        //



                        pass.Apply();
                        // ?? What is VertexBuffer, IndexBuffer ??
                        GraphicsDevice.SetVertexBuffer(part.VertexBuffer);
                        GraphicsDevice.Indices = part.IndexBuffer;

                        GraphicsDevice.DrawIndexedPrimitives(
                            PrimitiveType.TriangleList,
                            part.VertexOffset,
                            part.StartIndex,
                            part.PrimitiveCount
                            );
                    }
                }
            }

            DisplayValues();


            base.Draw(gameTime);
        }

        
        private void ControlParameters()
        {
            if (IsKeyPressed(Keys.Q))
            {
                mainBendOn = !mainBendOn;
            }
            if (currentKeyboardState.IsKeyDown(Keys.W))
            {
                mainBendScale *= 0.99f;
            }
            if (currentKeyboardState.IsKeyDown(Keys.E))
            {
                mainBendScale *= 1.01f;
            }

            if (IsKeyPressed(Keys.A))
            {
                detailBranchOn = !detailBranchOn;
            }
            if (currentKeyboardState.IsKeyDown(Keys.S))
            {
                detailBranchAmplitude *= 0.99f;
            }
            if (currentKeyboardState.IsKeyDown(Keys.D))
            {
                detailBranchAmplitude *= 1.01f;
            }

            if (IsKeyPressed(Keys.Z))
            {
                detailSideToSideOn = !detailSideToSideOn;
            }
            if (currentKeyboardState.IsKeyDown(Keys.X))
            {
                detailSideToSideAmplitude *= 0.99f;
            }
            if (currentKeyboardState.IsKeyDown(Keys.C))
            {
                detailSideToSideAmplitude *= 1.01f;
            }
            if (IsKeyPressed(Keys.Space))
            {
                pauseWind = !pauseWind;
            }
        }
        private void CameraControls()
        {
            if (Keyboard.GetState().IsKeyDown(Keys.Left)) angleL += 0.02f;
            if (Keyboard.GetState().IsKeyDown(Keys.Right)) angleL -= 0.02f;
            if (Keyboard.GetState().IsKeyDown(Keys.Up)) angleL2 += 0.02f;
            if (Keyboard.GetState().IsKeyDown(Keys.Down)) angleL2 -= 0.02f;
            if (Keyboard.GetState().IsKeyDown(Keys.S)) { angle = angle2 = angleL = angleL2 = 0; distance = 90; cameraTarget = Vector3.Zero; }
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
        }
        private void LightControls()
        {
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
                Matrix.CreatePerspectiveFieldOfView(MathHelper.PiOver2, 1f, 1f, 300f);
        }
        private void SwapTechnique()
        {
            if (Keyboard.GetState().IsKeyDown(Keys.D0)) { currentTechnique = 0; }
            if (Keyboard.GetState().IsKeyDown(Keys.D1)) { currentTechnique = 1; }
            if (Keyboard.GetState().IsKeyDown(Keys.D2)) { currentTechnique = 2; }
            if (Keyboard.GetState().IsKeyDown(Keys.D3)) { currentTechnique = 3; }
            if (Keyboard.GetState().IsKeyDown(Keys.D4)) { currentTechnique = 4; }
            if (Keyboard.GetState().IsKeyDown(Keys.D5)) { currentTechnique = 5; }
        }
        private void CalculateWindSpeed()
        {

            if (timeSinceLastThing < 0f)
            {
                lastWindSpeed = newWindSpeed;

                float x = (float)random.NextDouble();
                x = (float)Math.Pow(x, 3);

                float y = (float)random.NextDouble();
                y = (float)Math.Pow(y, 3);
                newWindSpeed = new Vector2(x * 2f - 1f,
                    y * 2f - 1f);

                newWindSpeed *= 3f;
                timeSinceLastThing += 1f;
            }
            currentWindSpeed = Vector2.SmoothStep(newWindSpeed, lastWindSpeed, timeSinceLastThing);
        }

        private void DisplayValues()
        {
            _spriteBatch.Begin();
            _spriteBatch.DrawString(font, "VALUES", new Vector2(25, 25), Color.Black);
            _spriteBatch.DrawString(font, "Wind Velocity: (" +  (newWindSpeed.X.ToString("0.00")) + "," + (newWindSpeed.Y.ToString("0.00")) + ")", new Vector2(25, 45), Color.Black);
            _spriteBatch.DrawString(font, "Wind Velocity: (" +  (pauseWind) + ")", new Vector2(25, 65), Color.Black);
            _spriteBatch.End();

        }
    }
}