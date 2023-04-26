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
        Matrix projection = Matrix.CreatePerspectiveFieldOfView(MathHelper.ToRadians(45), 800f / 600f, 0.1f, 100f);
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
        float distance = 90;
        MouseState preMouse;
        Model model;
        Model[] models;
        Texture2D texture;
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

        private float detailBranchAmplitude = 0.05f;
        private float detailSideToSideAmplitude = 0.05f;
        private float mainBendScale = 0.01f;
        private bool detailBranchOn = true;
        private bool detailSideToSideOn = true;
        private bool mainBendOn = true;



        public FinalProject()
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
            model = Content.Load<Model>("SimplePlant");
            effect = Content.Load<Effect>("SimplestPhongLighting");

            // TODO: use this.Content to load your game content here
        }

        protected override void Update(GameTime gameTime)
        {
            if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed || Keyboard.GetState().IsKeyDown(Keys.Escape))
                Exit();

            CameraControls();
            LightControls();
            ControlParameters();
            SwapTechnique();

            //float time = (float)gameTime.ElapsedGameTime.TotalMilliseconds;

            // Change wind every second, and smoothly interpolate to the new strength/direction.
            float timeEllapsed = (float)gameTime.ElapsedGameTime.TotalSeconds;
            totalTime += timeEllapsed;
            timeSinceLastThing -= timeEllapsed;
            if (timeSinceLastThing < 0f)
            {
                lastWindSpeed = newWindSpeed;
                float x = (float)random.NextDouble();
                x = (float)Math.Pow(x, 3);
                float y = (float)random.NextDouble();
                y = (float)Math.Pow(y, 3);
                newWindSpeed = new Vector2(x * 2f - 1f,
                    y * 2f - 1f);
                newWindSpeed *= 10f;
                timeSinceLastThing += 1f;
            }
            currentWindSpeed = Vector2.SmoothStep(newWindSpeed, lastWindSpeed, timeSinceLastThing);

            base.Update(gameTime);
        }



        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.CornflowerBlue);

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
                        Matrix worldInverseTranspose = Matrix.Transpose(Matrix.Invert(mesh.ParentBone.Transform));
                        effect.Parameters["WorldInverseTranspose"].SetValue(worldInverseTranspose);

                        effect.Parameters["AmbientColor"].SetValue(ambient);
                        effect.Parameters["AmbientIntensity"].SetValue(ambientIntensity);

                        effect.Parameters["DiffuseColor"].SetValue(diffuseColor);
                        effect.Parameters["DiffuseIntensity"].SetValue(1f);

                        effect.Parameters["Shininess"].SetValue(20f);
                        effect.Parameters["SpecularColor"].SetValue(specularColor);
                        effect.Parameters["SpecularIntensity"].SetValue(1);

                        effect.Parameters["LightPosition"].SetValue(lightPosition);
                        effect.Parameters["CameraPosition"].SetValue(cameraPosition);

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


            base.Draw(gameTime);
        }

        private bool IsKeyPressed(Keys key)
        {
            return !previousKeyboardState.IsKeyDown(key) && currentKeyboardState.IsKeyDown(key);
        }
        private void ControlParameters()
        {
            if (currentKeyboardState.IsKeyDown(Keys.Q))
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
                Matrix.CreatePerspectiveFieldOfView(MathHelper.PiOver2, 1f, 1f, 50f);
        }
        private void SwapTechnique()
        {
            if (Keyboard.GetState().IsKeyDown(Keys.D0)) { currentTechnique = 0; }
            if (Keyboard.GetState().IsKeyDown(Keys.D1)) { currentTechnique = 1; }
            if (Keyboard.GetState().IsKeyDown(Keys.D2)) { currentTechnique = 2; }
            if (Keyboard.GetState().IsKeyDown(Keys.D3)) { currentTechnique = 3; }
        }
    }
}