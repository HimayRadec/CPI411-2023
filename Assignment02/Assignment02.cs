using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using CPI411.SimpleEngine;

// Only Bug is the Torus model is a bit oversized so it doesn't work when trying to view.
// Also the teapot one is only viewable at a certain angle.

namespace Assignment02
{
    public class Assignment02 : Game
    {

        #region - Default Variables - 

        private GraphicsDeviceManager _graphics;
        private SpriteBatch spriteBatch;
        SpriteFont font;
        Effect effect;

        Matrix world = Matrix.Identity;
        Matrix view = Matrix.CreateLookAt(new Vector3(20, 0, 0), new Vector3(0, 0, 0), Vector3.UnitY);
        Matrix projection = Matrix.CreatePerspectiveFieldOfView(
            MathHelper.ToRadians(45),
            800f / 600f,
            0.01f,
            10000f); 
        Vector3 cameraPosition, cameraTarget, lightPosition;
        Matrix lightView, lightProjection;

        float angle = 0;
        float angle2 = 0;
        float angleL = 0;
        float angleL2 = 0;
        float distance = 5;
        float defaultDistance = 5;

        Model model;
        Texture2D texture;

        MouseState preMouse;
        KeyboardState previousKeyboardState;
        KeyboardState currentKeyboardState;
        private bool IsKeyPressed(Keys key)
        {
            return !previousKeyboardState.IsKeyDown(key) && currentKeyboardState.IsKeyDown(key);
        }



        #endregion


        #region - Assignment02 Variables -

        Skybox skybox;

        float reflectivity = 0.99f;
        float refractivity = 1.01f;
        Vector3 etaRatio = new Vector3(0.9f, 0.8f, 0.7f);
        Vector3 fresnalTerm = new Vector3(0.0f, 0.5f, 3.0f);
        int shaderMode = 0;
        bool showMenu = true;
        bool showValues = true;

        #endregion

        public Assignment02()
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
            spriteBatch = new SpriteBatch(GraphicsDevice);

            // TODO: use this.Content to load your game content here
            model = Content.Load<Model>("Helicopter");
            effect = Content.Load<Effect>("Reflection");
            font = Content.Load<SpriteFont>("Font");
            texture = Content.Load<Texture2D>("HelicopterTexture");

            string[] skyboxTextures = { "Environment Maps/nvlobby_new_negx", "Environment Maps/nvlobby_new_posx",
                "Environment Maps/nvlobby_new_negy", "Environment Maps/nvlobby_new_posy",
                "Environment Maps/nvlobby_new_negz", "Environment Maps/nvlobby_new_posz"};
            skybox = new Skybox(skyboxTextures, Content, _graphics.GraphicsDevice);

        }

        protected override void Update(GameTime gameTime)
        {
            
            if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed || Keyboard.GetState().IsKeyDown(Keys.Escape))
                Exit();

            // TODO: Add your update logic here

            previousKeyboardState = currentKeyboardState;
            currentKeyboardState = Keyboard.GetState();

            #region - A. Basic User Interfaces -

            CameraControls();
            LightControls();

            #endregion
            #region - Geometry Loader and Image Loader -

            GeometryAndImageLoader();

            #endregion
            ValueControls();
            base.Update(gameTime);
        }

        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.CornflowerBlue);
            GraphicsDevice.BlendState = BlendState.Opaque;
            GraphicsDevice.DepthStencilState = new DepthStencilState();

            RasterizerState originalRasterizerState = _graphics.GraphicsDevice.RasterizerState;
            RasterizerState rasterizerState = new RasterizerState();
            rasterizerState.CullMode = CullMode.None;

            _graphics.GraphicsDevice.RasterizerState = rasterizerState;
            skybox.Draw(view, projection, cameraPosition);
            _graphics.GraphicsDevice.RasterizerState = originalRasterizerState;

            DrawModel();


            spriteBatch.Begin();
            if (showMenu) DisplayHelp();
            if (showValues) DisplayValues();
            spriteBatch.End();

        }

        private void CameraControls()
        {
            // Reset
            if (IsKeyPressed(Keys.Enter)) { angle = angle2 = angleL = angleL2 = 0; distance = defaultDistance; cameraTarget = Vector3.Zero; }

            // Camera Buttons
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
            // Lights Buttons
            if (Keyboard.GetState().IsKeyDown(Keys.Left)) angleL += 0.02f;
            if (Keyboard.GetState().IsKeyDown(Keys.Right)) angleL -= 0.02f;
            if (Keyboard.GetState().IsKeyDown(Keys.Up)) angleL2 += 0.02f;
            if (Keyboard.GetState().IsKeyDown(Keys.Down)) angleL2 -= 0.02f;

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
        private void ValueControls()
        {
            if (Keyboard.GetState().IsKeyDown(Keys.OemPlus)) reflectivity += 0.01f;
            if (Keyboard.GetState().IsKeyDown(Keys.OemMinus)) reflectivity -= 0.01f;

            if (Keyboard.GetState().IsKeyDown(Keys.S) && !Keyboard.GetState().IsKeyDown(Keys.LeftShift)) refractivity += 0.01f;
            if (Keyboard.GetState().IsKeyDown(Keys.S) && Keyboard.GetState().IsKeyDown(Keys.LeftShift)) refractivity -= 0.01f;

            if (Keyboard.GetState().IsKeyDown(Keys.Q) && !Keyboard.GetState().IsKeyDown(Keys.LeftShift)) fresnalTerm.X += 0.01f;
            if (Keyboard.GetState().IsKeyDown(Keys.Q) && Keyboard.GetState().IsKeyDown(Keys.LeftShift)) fresnalTerm.X -= 0.01f;

            if (Keyboard.GetState().IsKeyDown(Keys.W) && !Keyboard.GetState().IsKeyDown(Keys.LeftShift)) fresnalTerm.Y += 0.01f;
            if (Keyboard.GetState().IsKeyDown(Keys.W) && Keyboard.GetState().IsKeyDown(Keys.LeftShift)) fresnalTerm.Y -= 0.01f;

            if (Keyboard.GetState().IsKeyDown(Keys.E) && !Keyboard.GetState().IsKeyDown(Keys.LeftShift)) fresnalTerm.Z += 0.01f;
            if (Keyboard.GetState().IsKeyDown(Keys.E) && Keyboard.GetState().IsKeyDown(Keys.LeftShift)) fresnalTerm.Z -= 0.01f;

            if (Keyboard.GetState().IsKeyDown(Keys.R) && !Keyboard.GetState().IsKeyDown(Keys.LeftShift)) etaRatio.X += 0.01f;
            if (Keyboard.GetState().IsKeyDown(Keys.R) && Keyboard.GetState().IsKeyDown(Keys.LeftShift)) etaRatio.X -= 0.01f;

            if (Keyboard.GetState().IsKeyDown(Keys.G) && !Keyboard.GetState().IsKeyDown(Keys.LeftShift)) etaRatio.Y += 0.01f;
            if (Keyboard.GetState().IsKeyDown(Keys.G) && Keyboard.GetState().IsKeyDown(Keys.LeftShift)) etaRatio.Y -= 0.01f;

            if (Keyboard.GetState().IsKeyDown(Keys.B) && !Keyboard.GetState().IsKeyDown(Keys.LeftShift)) etaRatio.Z += 0.01f;
            if (Keyboard.GetState().IsKeyDown(Keys.B) && Keyboard.GetState().IsKeyDown(Keys.LeftShift)) etaRatio.Z -= 0.01f;

            if (IsKeyPressed(Keys.OemQuestion)) showMenu = !showMenu;
            if (IsKeyPressed(Keys.H)) showValues = !showValues;
            if (IsKeyPressed(Keys.F)) ResetValues();

        }
        private void GeometryAndImageLoader()
        {
            if (IsKeyPressed(Keys.D1)) { model = Content.Load<Model>("Box"); }
            if (IsKeyPressed(Keys.D2)) { model = Content.Load<Model>("Sphere"); }
            if (IsKeyPressed(Keys.D3)) { model = Content.Load<Model>("Torus"); }
            if (IsKeyPressed(Keys.D4)) { model = Content.Load<Model>("Teapot"); }
            if (IsKeyPressed(Keys.D5)) { model = Content.Load<Model>("Bunny"); }
            if (IsKeyPressed(Keys.D6)) {
                model = Content.Load<Model>("Helicopter");
                texture = Content.Load<Texture2D>("HelicopterTexture");
            }
            // Skybox textures as test colors
            if (IsKeyPressed(Keys.D7)) 
            {
                string[] skyboxTextures = { "Environment Maps/test_negx", "Environment Maps/test_posx",
                "Environment Maps/test_negy", "Environment Maps/test_posy",
                "Environment Maps/test_negz", "Environment Maps/test_posz"};
                skybox = new Skybox(skyboxTextures, Content, _graphics.GraphicsDevice);

            }
            // Skybox textures as an NVIDIA office room
            if (IsKeyPressed(Keys.D8)) 
            {
                string[] skyboxTextures = { "Environment Maps/nvlobby_new_negx", "Environment Maps/nvlobby_new_posx",
                "Environment Maps/nvlobby_new_negy", "Environment Maps/nvlobby_new_posy",
                "Environment Maps/nvlobby_new_negz", "Environment Maps/nvlobby_new_posz"};
                skybox = new Skybox(skyboxTextures, Content, _graphics.GraphicsDevice);

            }
            // Skybox textures as a daytime sky
            if (IsKeyPressed(Keys.D9)) 
            {
                string[] skyboxTextures = { "Environment Maps/grandcanyon_negx", "Environment Maps/grandcanyon_posx",
                "Environment Maps/grandcanyon_negy", "Environment Maps/grandcanyon_posy",
                "Environment Maps/grandcanyon_negz", "Environment Maps/grandcanyon_posz"};
                skybox = new Skybox(skyboxTextures, Content, _graphics.GraphicsDevice);

            }
            // Skybox textures developed by yourself
            if (IsKeyPressed(Keys.D0)) 
            {
                string[] skyboxTextures = { "Environment Maps/hills_negx", "Environment Maps/hills_posx",
                "Environment Maps/hills_negy", "Environment Maps/hills_posy",
                "Environment Maps/hills_negz", "Environment Maps/hills_posz"};
                skybox = new Skybox(skyboxTextures, Content, _graphics.GraphicsDevice);
            }
            if (IsKeyPressed(Keys.F7)) { shaderMode = 0; }
            if (IsKeyPressed(Keys.F8)) { shaderMode = 1; }
            if (IsKeyPressed(Keys.F9)) { shaderMode = 2; }
            if (IsKeyPressed(Keys.F10)) { shaderMode = 3; }

        }
        private void DrawModel()
        {
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
                        effect.Parameters["environmentMap"].SetValue(skybox.skyBoxTexture);
                        effect.Parameters["CameraPosition"].SetValue(cameraPosition);
                        if (shaderMode == 0) SetReflection();
                        if (shaderMode == 1) SetRefraction();
                        if (shaderMode == 2) SetDespersion();
                        if (shaderMode == 3) SetFresnal();

                        pass.Apply();
                        GraphicsDevice.SetVertexBuffer(part.VertexBuffer);
                        GraphicsDevice.Indices = part.IndexBuffer;

                        GraphicsDevice.DrawIndexedPrimitives(
                            PrimitiveType.TriangleList, part.VertexOffset, 0,
                            part.NumVertices, part.StartIndex, part.PrimitiveCount);
                    }
                }
            }
        }
        void DisplayValues()
        {
            int height = 20;
            int line = 1;
            float leftMargin = 20f;

            spriteBatch.DrawString(font, "Active Shader " + effect, new Vector2(leftMargin, height * line++), Color.White);
            spriteBatch.DrawString(font, "Reflectivity " + reflectivity, new Vector2(leftMargin, height * line++), Color.White);
            spriteBatch.DrawString(font, "Refractivity " + refractivity, new Vector2(leftMargin, height * line++), Color.White);
            spriteBatch.DrawString(font, "etaRatio.R  " + etaRatio.X, new Vector2(leftMargin, height * line++), Color.White);
            spriteBatch.DrawString(font, "etaRatio.G  " + etaRatio.Y, new Vector2(leftMargin, height * line++), Color.White);
            spriteBatch.DrawString(font, "etaRatio.B  " + etaRatio.Z, new Vector2(leftMargin, height * line++), Color.White);
            spriteBatch.DrawString(font, "Fresnal Bias " + fresnalTerm.X, new Vector2(leftMargin, height * line++), Color.White);
            spriteBatch.DrawString(font, "Fresnal Scale " + fresnalTerm.Y, new Vector2(leftMargin, height * line++), Color.White);
            spriteBatch.DrawString(font, "Fresnal Power " + fresnalTerm.Z, new Vector2(leftMargin, height * line++), Color.White);
            spriteBatch.DrawString(font, "Reset Values: F ", new Vector2(leftMargin, height * line++), Color.White);

        }
        void DisplayHelp()
        {
            int height = 20;
            int line = 1;
            float leftMargin = _graphics.PreferredBackBufferWidth * 0.75f;

            spriteBatch.DrawString(font, "Shaders: F7 - F10 ", new Vector2(leftMargin, height * line++), Color.White);
            spriteBatch.DrawString(font, "Models: 1 - 6 " , new Vector2(leftMargin, height * line++), Color.White);
            spriteBatch.DrawString(font, "Skybox Textures: 7 - 0 ", new Vector2(leftMargin, height * line++), Color.White);
            spriteBatch.DrawString(font, "Reflectivity: +/- ", new Vector2(leftMargin, height * line++), Color.White);
            spriteBatch.DrawString(font, "Reflectivity: S/s ", new Vector2(leftMargin, height * line++), Color.White);
            spriteBatch.DrawString(font, "eta Ratio: R/r ", new Vector2(leftMargin, height * line++), Color.White);
            spriteBatch.DrawString(font, "eta Ratio: G/g ", new Vector2(leftMargin, height * line++), Color.White);
            spriteBatch.DrawString(font, "eta Ratio: B/b ", new Vector2(leftMargin, height * line++), Color.White);
            spriteBatch.DrawString(font, "Fresnal Power: Q/q ", new Vector2(leftMargin, height * line++), Color.White);
            spriteBatch.DrawString(font, "Fresnal Scale: W/w ", new Vector2(leftMargin, height * line++), Color.White);
            spriteBatch.DrawString(font, "Fresnal Bias: E/e ", new Vector2(leftMargin, height * line++), Color.White);
            spriteBatch.DrawString(font, "Light Controls: Arrows", new Vector2(leftMargin, height * line++), Color.White);
            spriteBatch.DrawString(font, "Reset Camera/Light: Enter ", new Vector2(leftMargin, height * line++), Color.White);
        }
        void ResetValues()
        { 

            reflectivity = 0.99f;
            refractivity = 1.01f;
            etaRatio = new Vector3(0.9f, 0.8f, 0.7f);
            fresnalTerm = new Vector3(0.0f, 0.5f, 3.0f);
        }
        void SetReflection()
        {
            effect = Content.Load<Effect>("Reflection");
            effect.Parameters["reflectivity"].SetValue(reflectivity);
            effect.Parameters["decalMap"].SetValue(texture);
        }
        void SetRefraction()
        {
            effect = Content.Load<Effect>("Refraction");
            effect.Parameters["refractivity"].SetValue(refractivity);
            effect.Parameters["decalMap"].SetValue(texture);
            effect.Parameters["etaRatio"].SetValue(etaRatio);
        }
        void SetDespersion()
        {
            effect = Content.Load<Effect>("RefractionAndDispersion");
            effect.Parameters["refractivity"].SetValue(refractivity);
            effect.Parameters["decalMap"].SetValue(texture);
            effect.Parameters["etaRatio"].SetValue(etaRatio);
        }
        void SetFresnal()
        {
            effect = Content.Load<Effect>("Fresnal");
            effect.Parameters["reflectivity"].SetValue(reflectivity);
            effect.Parameters["decalMap"].SetValue(texture);
            effect.Parameters["etaRatio"].SetValue(etaRatio);
            effect.Parameters["fresnalTerm"].SetValue(fresnalTerm);
        }
    }
}