using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System.Security.Cryptography;

namespace Assignment01
{
    public class Assignment01 : Game
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
        float distance = 10;
        float defaultDistance = 10;

        bool showMenu = true;
        bool showValues = true;

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



        #region - Variables -

        int currentShader;
        string currentShaderType;

        Vector4 specularColor = new Vector4(1, 1, 1, 1);
        Vector4 ambient = new Vector4(0, 0, 0, 0);
        Vector4 diffuseColor = new Vector4(1, 1, 1, 1);
        float ambientIntensity = 0;
        float diffuseIntensity = 1f;
        float specularIntensity = 1f;
        float shininess = 10f;

        #endregion 

        public Assignment01()
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
            spriteBatch = new SpriteBatch(GraphicsDevice);

            // TODO: use this.Content to load your game content here
            model = Content.Load<Model>("Box");
            effect = Content.Load<Effect>("Shaders");
            font = Content.Load<SpriteFont>("UI");
        }

        protected override void Update(GameTime gameTime)
        {
            if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed || Keyboard.GetState().IsKeyDown(Keys.Escape))
                Exit();

            previousKeyboardState = currentKeyboardState;
            currentKeyboardState = Keyboard.GetState();

            if (IsKeyPressed(Keys.H)) showValues = !showValues;
            if (IsKeyPressed(Keys.OemQuestion)) showMenu = !showMenu;
            if (IsKeyPressed(Keys.F)) ResetValues();

            LightControls();
            CameraControls();

            // Keep
            #region - Geometry Loader -

            // The models provided under the "assignment one" are smaller than
            // the models provided under the previous labs, so this is why it appears that way.
            if (Keyboard.GetState().IsKeyDown(Keys.D1)) // Box
            {
                model = Content.Load<Model>("Box");
                distance = 2f;
            }
            if (Keyboard.GetState().IsKeyDown(Keys.D2)) // Sphere
            {
                model = Content.Load<Model>("Sphere");
                distance = 2f;
            }
            if (Keyboard.GetState().IsKeyDown(Keys.D3)) // Tea Pot
            {
                model = Content.Load<Model>("Teapot");
                distance = 2f;
            }
            if (Keyboard.GetState().IsKeyDown(Keys.D4)) // Torus
            {
                model = Content.Load<Model>("Torus");
                distance = 20f;
            }
            if (Keyboard.GetState().IsKeyDown(Keys.D5)) // Bunny
            {
                model = Content.Load<Model>("Bunny");
                distance = 20f;
            }

            #endregion

            #region - Shader/Lighting Models -

            #region - Shader Effects -
            if (Keyboard.GetState().IsKeyDown(Keys.F1)) // Gourand Vertex
            {
                currentShader = 0;
                currentShaderType = "Gourand Vertex";
            }
            if (Keyboard.GetState().IsKeyDown(Keys.F2)) // Phong Pixel
            {
                currentShader = 1;
                currentShaderType = "Phong Pixel";
            }
            if (Keyboard.GetState().IsKeyDown(Keys.F3)) // PhongBlinn 
            {
                currentShader = 2;
                currentShaderType = "PhongBlinn";
            }
            if (Keyboard.GetState().IsKeyDown(Keys.F4)) // Schlick 
            {
                currentShader = 3;
                currentShaderType = "Schlick";
            }
            if (Keyboard.GetState().IsKeyDown(Keys.F5)) // Toon 
            {
                currentShader = 4;
                currentShaderType = "Toon";
            }
            if (Keyboard.GetState().IsKeyDown(Keys.F6)) // HalfLife 
            {
                currentShader = 5;
                currentShaderType = "HalfLife";
            }
            #endregion

            if (Keyboard.GetState().IsKeyDown(Keys.L) && !Keyboard.GetState().IsKeyDown(Keys.LeftShift)) diffuseIntensity += .01f;
            if (Keyboard.GetState().IsKeyDown(Keys.L) && Keyboard.GetState().IsKeyDown(Keys.LeftShift)) diffuseIntensity -= .01f;

            if (Keyboard.GetState().IsKeyDown(Keys.R) && !Keyboard.GetState().IsKeyDown(Keys.LeftShift)) diffuseColor.X += .01f;
            if (Keyboard.GetState().IsKeyDown(Keys.R) && Keyboard.GetState().IsKeyDown(Keys.LeftShift)) diffuseColor.X -= .01f;

            if (Keyboard.GetState().IsKeyDown(Keys.G) && !Keyboard.GetState().IsKeyDown(Keys.LeftShift)) diffuseColor.Y += .01f;
            if (Keyboard.GetState().IsKeyDown(Keys.G) && Keyboard.GetState().IsKeyDown(Keys.LeftShift)) diffuseColor.Y -= .01f;

            if (Keyboard.GetState().IsKeyDown(Keys.B) && !Keyboard.GetState().IsKeyDown(Keys.LeftShift)) diffuseColor.Z += .01f;
            if (Keyboard.GetState().IsKeyDown(Keys.B) && Keyboard.GetState().IsKeyDown(Keys.LeftShift)) diffuseColor.Z -= .01f;

            if (Keyboard.GetState().IsKeyDown(Keys.S) && !Keyboard.GetState().IsKeyDown(Keys.LeftShift)) shininess += .1f;
            if (Keyboard.GetState().IsKeyDown(Keys.S) && Keyboard.GetState().IsKeyDown(Keys.LeftShift)) shininess -= .1f;

            if (Keyboard.GetState().IsKeyDown(Keys.OemPlus)) specularIntensity += .1f;
            if (Keyboard.GetState().IsKeyDown(Keys.OemMinus)) specularIntensity -= .1f;
             

            #endregion


            


            base.Update(gameTime);
        }

        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.CornflowerBlue);
            GraphicsDevice.BlendState = BlendState.Opaque; // BUG: Causes the Objects to render weirdley




            // TODO: Add your drawing code here

            effect.CurrentTechnique = effect.Techniques[currentShader];
            foreach (EffectPass pass in effect.CurrentTechnique.Passes)
            {
                foreach (ModelMesh mesh in model.Meshes)
                {
                    foreach (ModelMeshPart part in mesh.MeshParts)
                    {
                        // ?? Where is this data going ??
                        effect.Parameters["World"].SetValue(mesh.ParentBone.Transform);
                        effect.Parameters["View"].SetValue(view);
                        effect.Parameters["Projection"].SetValue(projection);
                        effect.Parameters["AmbientColor"].SetValue(ambient);
                        effect.Parameters["AmbientIntensity"].SetValue(ambientIntensity);
                        effect.Parameters["DiffuseColor"].SetValue(diffuseColor);
                        effect.Parameters["DiffuseIntensity"].SetValue(diffuseIntensity);

                        Matrix worldInverseTranspose = Matrix.Transpose(Matrix.Invert(mesh.ParentBone.Transform));
                        effect.Parameters["WorldInverseTranspose"].SetValue(worldInverseTranspose);

                        // Lab04
                        effect.Parameters["SpecularColor"].SetValue(specularColor);
                        effect.Parameters["SpecularIntensity"].SetValue(specularIntensity);
                        effect.Parameters["Shininess"].SetValue(shininess);
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

            spriteBatch.Begin();
            if (showMenu) DisplayHelp();
            if (showValues) DisplayValues();
            spriteBatch.End();  

            base.Draw(gameTime);
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
        void DisplayValues()
        {
            int height = 20;
            int line = 1;
            float leftMargin = 20f;

            spriteBatch.DrawString(font, "Shader Type: " + currentShaderType, new Vector2(leftMargin, height * line++), Color.White);
            spriteBatch.DrawString(font, "Camera Angle: (" + (int)(angle * 100) + "," + (int)(angle2 * 100) + ")", new Vector2(leftMargin, height * line++), Color.White);
            spriteBatch.DrawString(font, "Light Angle: (" + (int)(angleL * 100) + "," + (int)(angleL2 * 100) + ")", new Vector2(leftMargin, height * line++), Color.White);
            spriteBatch.DrawString(font, "Light Intensity: " + diffuseIntensity.ToString("0.00"), new Vector2(leftMargin, height * line++), Color.White);
            spriteBatch.DrawString(font, "R: " + diffuseColor.X.ToString("0.00"), new Vector2(leftMargin, height * line++), Color.White);
            spriteBatch.DrawString(font, "G: " + diffuseColor.Y.ToString("0.00"), new Vector2(leftMargin, height * line++), Color.White);
            spriteBatch.DrawString(font, "B: " + diffuseColor.Z.ToString("0.00"), new Vector2(leftMargin, height * line++), Color.White);
            spriteBatch.DrawString(font, "Specular Intensity: " + specularIntensity.ToString("0.00"), new Vector2(leftMargin, height * line++), Color.White);
            spriteBatch.DrawString(font, "Shininess: " + shininess.ToString("0.00"), new Vector2(leftMargin, height * line++), Color.White);


            //spriteBatch.DrawString(font, "VALUES", new Vector2(525, 25), Color.Black);
            //spriteBatch.DrawString(font, "Camera Angle: (" + (int)(angle * 100) + "," + (int)(angle2 * 100) + ")", new Vector2(525, 45), Color.Black);
            //spriteBatch.DrawString(font, "Light Angle: (" + (int)(angleL * 100) + "," + (int)(angleL2 * 100) + ")", new Vector2(525, 65), Color.Black);
            //spriteBatch.DrawString(font, "Shader Type: " + currentShaderType, new Vector2(525, 85), Color.Black);
            //spriteBatch.DrawString(font, "Light Intensity: " + diffuseIntensity.ToString("0.00"), new Vector2(525, 105), Color.Black);
            //spriteBatch.DrawString(font, "R: " + diffuseColor.X.ToString("0.00"), new Vector2(525, 125), Color.Black);
            //spriteBatch.DrawString(font, "G: " + diffuseColor.Y.ToString("0.00"), new Vector2(525, 145), Color.Black);
            //spriteBatch.DrawString(font, "B: " + diffuseColor.Z.ToString("0.00"), new Vector2(525, 165), Color.Black);
            //spriteBatch.DrawString(font, "Specular Intensity: " + specularIntensity.ToString("0.00"), new Vector2(525, 185), Color.Black);
            //spriteBatch.DrawString(font, "Shininess: " + shininess.ToString("0.00"), new Vector2(525, 205), Color.Black);

        }
        void DisplayHelp()
        {
            int height = 20;
            int line = 1;
            float leftMargin = _graphics.PreferredBackBufferWidth * 0.65f;

            spriteBatch.DrawString(font, "CONTROLS", new Vector2(leftMargin, height * line++), Color.White);
            spriteBatch.DrawString(font, "Toggle Controls: ?", new Vector2(leftMargin, height * line++), Color.White);
            spriteBatch.DrawString(font, "Rotate the camera : Mouse Left Drag", new Vector2(leftMargin, height * line++), Color.White);
            spriteBatch.DrawString(font, "Change the distance of camera to the center: Mouse Right Drag", new Vector2(leftMargin, height * line++), Color.White);
            spriteBatch.DrawString(font, "Translate the camera: Mouse Middle Drag", new Vector2(leftMargin, height * line++), Color.White);
            spriteBatch.DrawString(font, "Rotate the light: Arrow keys", new Vector2(leftMargin, height * line++), Color.White);
            spriteBatch.DrawString(font, "Change Shader: F1-F6", new Vector2(leftMargin, height * line++), Color.White);
            spriteBatch.DrawString(font, "Change Object: 1-5", new Vector2(leftMargin, height * line++), Color.White);
            spriteBatch.DrawString(font, "RGB properties: Shift/R/G/B", new Vector2(leftMargin, height * line++), Color.White);
            spriteBatch.DrawString(font, "Specular Intensity: +/-", new Vector2(leftMargin, height * line++), Color.White);
            spriteBatch.DrawString(font, "Reset camera and light: Enter ", new Vector2(leftMargin, height * line++), Color.White);
            spriteBatch.DrawString(font, "Reset Values: R", new Vector2(leftMargin, height * line++), Color.White);
            spriteBatch.DrawString(font, "Information: ?", new Vector2(leftMargin, height * line++), Color.White);
            spriteBatch.DrawString(font, "Values: H", new Vector2(leftMargin, height * line++), Color.White);
        }
        void ResetValues()
        {
            specularColor = new Vector4(1, 1, 1, 1);
            ambient = new Vector4(0, 0, 0, 0);
            diffuseColor = new Vector4(1, 1, 1, 1);
            ambientIntensity = 0;
            diffuseIntensity = 1f;
            specularIntensity = 1f;
            shininess = 10f;
        }
    }
        
}