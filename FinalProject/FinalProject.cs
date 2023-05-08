using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;

namespace FinalProject
{
    public class FinalProject : Game
    {
        private GraphicsDeviceManager _graphics;
        private SpriteBatch _spriteBatch;

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
        float angle = 0;
        float angle2 = 0;
        float angleL = 0;
        float angleL2 = 0;
        MouseState preMouse;
        Model vegetationModel;

        // **** TEMPLATE ************//
        float distance = 25;
        float defaultDistance = 25;
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

        private float detailBranchAmplitude = 0.005f;
        private float defaultDetailBranchAmplitude = 0.005f;
        private float detailSideToSideAmplitude = 0.005f;
        private float defaultDetailSideToSideAmplitude = 0.005f;
        private float mainBendScale = 0.01f;
        private float defaultMainBendScale = 0.01f;
        private bool detailBranchOn = true;
        private bool detailSideToSideOn = true;
        private bool mainBendOn = true;
        private bool pauseWind = false;

        bool displayRed, displayGreen, displayBlue, displayAlpha = true;
        bool displayInformation = true;

        int lineHeight = 25;
        float windSpeedMultiplier = 1f;



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

            base.Initialize();
            displayRed = displayGreen = displayBlue = displayAlpha = true;

        }
        protected override void LoadContent()
        {
            _spriteBatch = new SpriteBatch(GraphicsDevice);
            vegetationModel = Content.Load<Model>("BananaTree2");
            effect = Content.Load<Effect>("WindAnimation");
            font = Content.Load<SpriteFont>("UI");
        }
        protected override void Update(GameTime gameTime)
        {
            previousKeyboardState = currentKeyboardState;
            currentKeyboardState = Keyboard.GetState();

            if (IsKeyPressed(Keys.Escape)) Exit();
            if (IsKeyPressed(Keys.H)) { displayInformation = !displayInformation; }

            // Update the time value based on the elapsed game time
            time = (float)gameTime.ElapsedGameTime.TotalMilliseconds;
            effect.Parameters["Time"].SetValue(time); // update the time value in the constant buffer

            SwapModels();
            CameraControls();
            LightControls();
            ControlParameters();
            DisplayRGBA();


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
            WindAnimation();
            
            if (displayInformation)
            {
                Information();
            }


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
            if (IsKeyPressed(Keys.R)) { mainBendScale = defaultMainBendScale; }

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
            if (IsKeyPressed (Keys.F)) { detailBranchAmplitude = defaultDetailBranchAmplitude; }

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
            if (IsKeyPressed(Keys.V)) { detailSideToSideAmplitude = defaultDetailSideToSideAmplitude; }

            if (IsKeyPressed(Keys.Space))
            {
                pauseWind = !pauseWind;
            }
            if (IsKeyPressed(Keys.Up)) { windSpeedMultiplier++; }
            if (IsKeyPressed(Keys.Down)) { windSpeedMultiplier--; }
        }
        private void CameraControls()
        {
            if (Keyboard.GetState().IsKeyDown(Keys.Left)) angleL += 0.02f;
            if (Keyboard.GetState().IsKeyDown(Keys.Right)) angleL -= 0.02f;
            if (Keyboard.GetState().IsKeyDown(Keys.Up)) angleL2 += 0.02f;
            if (Keyboard.GetState().IsKeyDown(Keys.Down)) angleL2 -= 0.02f;
            if (IsKeyPressed(Keys.Enter)) { angle = angle2 = angleL = angleL2 = 0; distance = defaultDistance; cameraTarget = Vector3.Zero; windSpeedMultiplier = 1f; }
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
        private void DisplayRGBA()
        {
            if (IsKeyPressed(Keys.D1)) { displayRed = !displayRed; }
            if (IsKeyPressed(Keys.D2)) { displayGreen = !displayGreen; }
            if (IsKeyPressed(Keys.D3)) { displayBlue = !displayBlue; }
            if (IsKeyPressed(Keys.D4)) { displayAlpha = !displayAlpha; }
        }
        private void SwapModels()
        {
            if (IsKeyPressed(Keys.F1)) { vegetationModel = Content.Load<Model>("BananaTree"); }
            if (IsKeyPressed(Keys.F2)) { vegetationModel = Content.Load<Model>("BananaTree2"); }
            if (IsKeyPressed(Keys.F3)) { vegetationModel = Content.Load<Model>("SimplePlant"); }
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

                newWindSpeed *= windSpeedMultiplier;
                timeSinceLastThing += 1f;
            }
            currentWindSpeed = Vector2.SmoothStep(newWindSpeed, lastWindSpeed, timeSinceLastThing);
        }
        private void Information()
        {
            _spriteBatch.Begin();
            _spriteBatch.DrawString(font, "VALUES", new Vector2(25, 25 + lineHeight * 0), Color.White);
            _spriteBatch.DrawString(font, string.Format("(Space)  Wind Active ({0}): X={1:0.00}, Y={2:0.00}, Multiplier={3:0.##}", !pauseWind ? "On" : "Off", newWindSpeed.X.ToString("0.00"), newWindSpeed.Y.ToString("0.00"), windSpeedMultiplier), new Vector2(25, 25 + lineHeight * 1), Color.White);
            _spriteBatch.DrawString(font, string.Format("(Q) (W) (E) (R)  Main bending ({0}): {1:0.000}", mainBendOn ? "On" : "Off", mainBendScale), new Vector2(25, 25 + lineHeight * 2), Color.White);
            _spriteBatch.DrawString(font, string.Format("(A) (S) (D) (F) Branch bending ({0}): {1:0.000}", detailBranchOn ? "On" : "Off", detailBranchAmplitude), new Vector2(25, 25 + lineHeight * 3), Color.White);
            _spriteBatch.DrawString(font, string.Format("(Z) (X) (C) (V) Leaf bending ({0}): {1:0.000}", detailSideToSideOn ? "On" : "Off", detailSideToSideAmplitude), new Vector2(25, 25 + lineHeight * 4), Color.White);
            _spriteBatch.DrawString(font, "Red: " + displayRed.ToString() + "\nGreen: " + displayGreen.ToString() + "\nBlue: " + displayBlue.ToString() + "\nAlpha: " + displayAlpha.ToString(), new Vector2(25, 25 + lineHeight * 5), Color.White);

            _spriteBatch.End();

        }
        private void WindAnimation()
        {
            // Set wind stuff that varies each time:
            foreach (EffectPass pass in effect.CurrentTechnique.Passes)
            {
                foreach (ModelMesh mesh in vegetationModel.Meshes)
                {
                    foreach (ModelMeshPart part in mesh.MeshParts)
                    {
                        effect.Parameters["WindSpeed"].SetValue(currentWindSpeed);
                        effect.Parameters["Time"].SetValue(totalTime);
                        effect.Parameters["BranchAmplitude"].SetValue(detailBranchOn ? detailBranchAmplitude : 0f);
                        effect.Parameters["DetailAmplitude"].SetValue(detailSideToSideOn ? detailSideToSideAmplitude : 0f);
                        effect.Parameters["BendScale"].SetValue(mainBendOn ? mainBendScale : 0f);

                        effect.Parameters["displayRed"].SetValue(displayRed);
                        effect.Parameters["displayGreen"].SetValue(displayGreen);
                        effect.Parameters["displayBlue"].SetValue(displayBlue);
                        effect.Parameters["displayAlpha"].SetValue(displayAlpha);


                        effect.Parameters["View"].SetValue(view);
                        effect.Parameters["Projection"].SetValue(projection); 
                        effect.Parameters["World"].SetValue(world);

                        // Since the leaf polygons are one-sided, draw the thing with two different cull-modes.
                        effect.Parameters["InvertNormal"].SetValue(false);
                        GraphicsDevice.RasterizerState = RasterizerState.CullCounterClockwise;
                        effect.CurrentTechnique.Passes[0].Apply();
                        GraphicsDevice.SetVertexBuffer(part.VertexBuffer);
                        GraphicsDevice.Indices = part.IndexBuffer;
                        GraphicsDevice.DrawIndexedPrimitives( PrimitiveType.TriangleList, part.VertexOffset, part.StartIndex, part.PrimitiveCount );

                        // We need to reverse the normal for proper lighting when using reverse winding order.
                        effect.Parameters["InvertNormal"].SetValue(true);
                        GraphicsDevice.RasterizerState = RasterizerState.CullClockwise;
                        effect.CurrentTechnique.Passes[0].Apply();
                        GraphicsDevice.DrawIndexedPrimitives(PrimitiveType.TriangleList, part.VertexOffset, part.StartIndex, part.PrimitiveCount);

                        // Restore standard state
                        GraphicsDevice.RasterizerState = RasterizerState.CullCounterClockwise;
                    }
                }
            }
        }
        
    }
}