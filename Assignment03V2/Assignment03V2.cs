using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using CPI411.SimpleEngine;
using System;

namespace Assignment03V2
{
    public class Assignment03V2 : Game
    {

        #region - Defaults - 

        private GraphicsDeviceManager graphics;
        private SpriteBatch spriteBatch;
        SpriteFont font;
        Effect effect;

        Skybox skybox;
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
        float distance = 25;
        float defaultDistance = 25;

        Model model;
        Texture2D texture;

        bool showControls = true;
        bool showValues = true;

        MouseState preMouse;
        KeyboardState previousKeyboardState;
        KeyboardState currentKeyboardState;

        private bool IsKeyPressed(Keys key)
        {
            return !previousKeyboardState.IsKeyDown(key) && currentKeyboardState.IsKeyDown(key);
        }

        #endregion

        // Assignment 03
        float bumpHeight, normalMapRepeatU, normalMapRepeatV;
        int mipMap, normalizeNormalMap, normalizeTangentFrame, selfShadow;
        int shaderMode = 1;

        string shaderName;
        public Assignment03V2()
        {
            graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";
            IsMouseVisible = true;
            graphics.GraphicsProfile = GraphicsProfile.HiDef;

        }
        protected override void Initialize()
        {
            // TODO: Add your initialization logic here
            

            base.Initialize();
        }
        protected override void LoadContent()
        {
            spriteBatch = new SpriteBatch(GraphicsDevice);
            shaderMode = 6;

            // TODO: use this.Content to load your game content here
            font = Content.Load<SpriteFont>("Font");
            texture = Content.Load<Texture2D>("NormalMaps/art");
            model = Content.Load<Model>("Torus");
            effect = Content.Load<Effect>("Reflection");
            string[] skyboxTextures = { 
                "skybox/nvlobby_new_negx", "skybox/nvlobby_new_posx",
                "skybox/nvlobby_new_negy", "skybox/nvlobby_new_posy",
                "skybox/nvlobby_new_negz", "skybox/nvlobby_new_posz"
            };
            skybox = new Skybox(skyboxTextures, Content, graphics.GraphicsDevice);
            ResetValues(); 
        }
        protected override void Update(GameTime gameTime)
        {
            if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed || Keyboard.GetState().IsKeyDown(Keys.Escape))
                Exit();

            // TODO: Add your update logic here
            previousKeyboardState = currentKeyboardState;
            currentKeyboardState = Keyboard.GetState(); 

            CameraControls(); 
            LightControls();
            TextureLoader();
            ChangeShader();
            ValueControls();

            base.Update(gameTime);
        }
        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.CornflowerBlue);
            GraphicsDevice.BlendState = BlendState.Opaque;
            GraphicsDevice.DepthStencilState = new DepthStencilState();

            RasterizerState orginalRasterizerState = graphics.GraphicsDevice.RasterizerState;
            RasterizerState rasterizerState = new RasterizerState();
            rasterizerState.CullMode = CullMode.None;
            graphics.GraphicsDevice.RasterizerState = rasterizerState;
            skybox.Draw(view, projection, cameraPosition);
            graphics.GraphicsDevice.RasterizerState = orginalRasterizerState;

            // Step 1 set the textures and everythings

            // Step 2 draw the model
            if (shaderMode == 7)
            {
                DisplayImageNormalMap();
            } 
            else
            {
                DrawModel();

            }

            spriteBatch.Begin();
            if (showControls) DrawControls();
            if (showValues) DrawValues();
            spriteBatch.End();

            base.Draw(gameTime);
        }
        void SetReflection()
        {
            effect.Parameters["CameraPosition"].SetValue(cameraPosition);
            effect.Parameters["environmentMap"].SetValue(skybox.skyBoxTexture);
            effect.Parameters["reflectivity"].SetValue(0.99f);
            effect.Parameters["decalMap"].SetValue(texture);
        }

        #region - Default Functions -
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
                        if (shaderMode == 1) DrawTangentNormals();
                        if (shaderMode == 2) DrawRGBNormals();
                        if (shaderMode == 3) DrawBumpMap();
                        if (shaderMode == 4) DrawBumpReflection();
                        if (shaderMode == 5) DrawBumpRefraction();
                        if (shaderMode == 6) SetReflection();

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
        private void TextureLoader()
        {
            if (IsKeyPressed(Keys.D1)) texture = Content.Load<Texture2D>("NormalMaps/art");
            if (IsKeyPressed(Keys.D2)) texture = Content.Load<Texture2D>("NormalMaps/BumpTest");
            if (IsKeyPressed(Keys.D3)) texture = Content.Load<Texture2D>("NormalMaps/crossHatch");
            if (IsKeyPressed(Keys.D4)) texture = Content.Load<Texture2D>("NormalMaps/monkey");
            if (IsKeyPressed(Keys.D5)) texture = Content.Load<Texture2D>("NormalMaps/round");
            if (IsKeyPressed(Keys.D6)) texture = Content.Load<Texture2D>("NormalMaps/saint");
            if (IsKeyPressed(Keys.D7)) texture = Content.Load<Texture2D>("NormalMaps/science");
        }
        private void ChangeShader()
        {
            if (IsKeyPressed(Keys.F1)) 
            { 
                shaderMode = 1;
                shaderName = "Tangent Space";
                effect = Content.Load<Effect>("TangentSpace");
            }
            if (IsKeyPressed(Keys.F2)) 
            { 
                shaderMode = 2;
                shaderName = "RGB World Space";
                effect = Content.Load<Effect>("WorldSpace");

            }
            if (IsKeyPressed(Keys.F3)) 
            { 
                shaderMode = 3;
                shaderName = "Bump Map";
                effect = Content.Load<Effect>("BumpMap");
            }
            if (IsKeyPressed(Keys.F4)) 
            { 
                shaderMode = 4;
                shaderName = "Bump Reflection";
                effect = Content.Load<Effect>("BumpReflection");
            }
            if (IsKeyPressed(Keys.F5)) 
            { 
                shaderMode = 5;
                shaderName = "Bump Refraction";
                effect = Content.Load<Effect>("BumpRefraction");
            }
            if (IsKeyPressed(Keys.F6)) 
            { 
                shaderMode = 6;
                shaderName = "Default Reflection";
                effect = Content.Load<Effect>("Reflection");
            }
            if (IsKeyPressed(Keys.F10))
            {
                shaderMode = 7;
                shaderName = "Normal Map";
                effect = Content.Load<Effect>("ImageNormal");
            }

        }
        private void ValueControls()
        {
            if (Keyboard.GetState().IsKeyDown(Keys.U) && !Keyboard.GetState().IsKeyDown(Keys.LeftShift)) normalMapRepeatU += 0.02f;
            if (Keyboard.GetState().IsKeyDown(Keys.U) && Keyboard.GetState().IsKeyDown(Keys.LeftShift)) normalMapRepeatU -= 0.02f;

            if (Keyboard.GetState().IsKeyDown(Keys.W) && !Keyboard.GetState().IsKeyDown(Keys.LeftShift)) bumpHeight += 0.02f;
            if (Keyboard.GetState().IsKeyDown(Keys.W) && Keyboard.GetState().IsKeyDown(Keys.LeftShift)) bumpHeight -= 0.02f;

            if (Keyboard.GetState().IsKeyDown(Keys.V) && !Keyboard.GetState().IsKeyDown(Keys.LeftShift)) normalMapRepeatV += 0.02f;
            if (Keyboard.GetState().IsKeyDown(Keys.V) && Keyboard.GetState().IsKeyDown(Keys.LeftShift)) normalMapRepeatV -= 0.02f;

            if (IsKeyPressed(Keys.M)) mipMap = ++mipMap % 2;
            if (IsKeyPressed(Keys.P)) selfShadow = (++selfShadow % 2);
            if (IsKeyPressed(Keys.T)) normalizeTangentFrame = (++normalizeTangentFrame % 2);
            if (IsKeyPressed(Keys.N)) normalizeNormalMap = (++normalizeNormalMap % 4);


            // Defaults
            if (IsKeyPressed(Keys.OemQuestion)) showControls = !showControls;
            if (IsKeyPressed(Keys.H)) showValues = !showValues;
            if (IsKeyPressed(Keys.R)) ResetValues();

        }
        #endregion

        #region - Shaders -

        private void DisplayImageNormalMap()
        {
            effect.CurrentTechnique = effect.Techniques[0];
            effect.Parameters["normalMap"].SetValue(texture);
            effect.Parameters["BumpHeight"].SetValue(bumpHeight);
            effect.Parameters["NormalMapRepeatU"].SetValue(normalMapRepeatU);
            effect.Parameters["NormalMapRepeatV"].SetValue(normalMapRepeatV);
            effect.Parameters["NormalizeNormalMap"].SetValue(normalizeNormalMap);
            effect.Parameters["MipMap"].SetValue(mipMap);

            spriteBatch.Begin(0, null, null, null, null, effect);
            spriteBatch.Draw(texture, new Vector2(0,0), null, Color.White, 0, new Vector2(0,0), 1f, SpriteEffects.None, 0f);
            spriteBatch.End();
        }
        private void DrawTangentNormals() // DONE
        {
            // TangentSpace.fx
            effect.Parameters["NormalMapRepeatU"].SetValue(normalMapRepeatU);
            effect.Parameters["NormalMapRepeatV"].SetValue(normalMapRepeatV);
            effect.Parameters["SelfShadow"].SetValue(selfShadow);
            effect.Parameters["BumpHeight"].SetValue(bumpHeight);
            effect.Parameters["NormalizeTangentFrame"].SetValue(normalizeTangentFrame);
            effect.Parameters["NormalizeNormalMap"].SetValue(normalizeNormalMap);
            effect.Parameters["MipMap"].SetValue(mipMap);
            effect.Parameters["normalMap"].SetValue(texture);

        }
        private void DrawRGBNormals() // DONE
        {
            // TangentSpace.fx
            effect.Parameters["NormalMapRepeatU"].SetValue(normalMapRepeatU);
            effect.Parameters["NormalMapRepeatV"].SetValue(normalMapRepeatV);
            effect.Parameters["SelfShadow"].SetValue(selfShadow);
            effect.Parameters["BumpHeight"].SetValue(bumpHeight);
            effect.Parameters["NormalizeTangentFrame"].SetValue(normalizeTangentFrame);
            effect.Parameters["NormalizeNormalMap"].SetValue(normalizeNormalMap);
            effect.Parameters["MipMap"].SetValue(mipMap);
            effect.Parameters["normalMap"].SetValue(texture);

        }
        private void DrawBumpMap() /// ?????????????????????
        {
            effect.Parameters["CameraPosition"].SetValue(cameraPosition);
            effect.Parameters["LightPosition"].SetValue(lightPosition);

            effect.Parameters["NormalMapRepeatU"].SetValue(normalMapRepeatU);
            effect.Parameters["NormalMapRepeatV"].SetValue(normalMapRepeatV);
            effect.Parameters["SelfShadow"].SetValue(selfShadow);
            effect.Parameters["BumpHeight"].SetValue(bumpHeight);
            effect.Parameters["NormalizeTangentFrame"].SetValue(normalizeTangentFrame);
            effect.Parameters["NormalizeNormalMap"].SetValue(normalizeNormalMap);
            effect.Parameters["MipMap"].SetValue(mipMap);
            effect.Parameters["normalMap"].SetValue(texture);

            effect.Parameters["Shininess"].SetValue(100.0f);
            effect.Parameters["DiffuseIntensity"].SetValue(1.0f);
            effect.Parameters["DiffuseColor"].SetValue(new Vector4(1.0f, 1.0f, 1.0f, 1.0f));
            effect.Parameters["SpecularColorIntensity"].SetValue(0.1f);
            effect.Parameters["SpecularColor"].SetValue(new Vector4(1.0f, 1.0f, 1.0f, 1.0f));
            effect.Parameters["Shininess"].SetValue(100.0f);


        }
        private void DrawBumpReflection() /// ????????????????
        {
            effect.Parameters["CameraPosition"].SetValue(cameraPosition);
            effect.Parameters["LightPosition"].SetValue(lightPosition);

            effect.Parameters["NormalMapRepeatU"].SetValue(normalMapRepeatU);
            effect.Parameters["NormalMapRepeatV"].SetValue(normalMapRepeatV);
            effect.Parameters["SelfShadow"].SetValue(selfShadow);
            effect.Parameters["BumpHeight"].SetValue(bumpHeight);
            effect.Parameters["NormalizeTangentFrame"].SetValue(normalizeTangentFrame);
            effect.Parameters["NormalizeNormalMap"].SetValue(normalizeNormalMap);
            effect.Parameters["MipMap"].SetValue(mipMap);
            effect.Parameters["normalMap"].SetValue(texture);
            effect.Parameters["environmentMap"].SetValue(skybox.skyBoxTexture);

            effect.Parameters["Shininess"].SetValue(100.0f);
            effect.Parameters["DiffuseIntensity"].SetValue(1.0f);
            effect.Parameters["DiffuseColor"].SetValue(new Vector4(1.0f, 1.0f, 1.0f, 1.0f));
            effect.Parameters["SpecularColorIntensity"].SetValue(0.1f);
            effect.Parameters["SpecularColor"].SetValue(new Vector4(1.0f, 1.0f, 1.0f, 1.0f));
            effect.Parameters["Shininess"].SetValue(100.0f);

        }
        private void DrawBumpRefraction()
        {
            effect.Parameters["CameraPosition"].SetValue(cameraPosition);
            effect.Parameters["LightPosition"].SetValue(lightPosition);

            effect.Parameters["NormalMapRepeatU"].SetValue(normalMapRepeatU);
            effect.Parameters["NormalMapRepeatV"].SetValue(normalMapRepeatV);
            effect.Parameters["SelfShadow"].SetValue(selfShadow);
            effect.Parameters["BumpHeight"].SetValue(bumpHeight);
            effect.Parameters["NormalizeTangentFrame"].SetValue(normalizeTangentFrame);
            effect.Parameters["NormalizeNormalMap"].SetValue(normalizeNormalMap);
            effect.Parameters["MipMap"].SetValue(mipMap);
            effect.Parameters["normalMap"].SetValue(texture);
            effect.Parameters["environmentMap"].SetValue(skybox.skyBoxTexture);

            effect.Parameters["Shininess"].SetValue(100.0f);
            effect.Parameters["DiffuseIntensity"].SetValue(1.0f);
            effect.Parameters["DiffuseColor"].SetValue(new Vector4(1.0f, 1.0f, 1.0f, 1.0f));
            effect.Parameters["SpecularColorIntensity"].SetValue(0.1f);
            effect.Parameters["SpecularColor"].SetValue(new Vector4(1.0f, 1.0f, 1.0f, 1.0f));
            effect.Parameters["Shininess"].SetValue(100.0f);
        }
        /*
        private void DrawBumpRefraction()
        {
            shaderName = "Bump Refraction";

            effect = Content.Load<Effect>("BumpRefraction");
            effect.CurrentTechnique = effect.Techniques[0];

            foreach (EffectPass pass in effect.CurrentTechnique.Passes)
            {
                foreach (ModelMesh mesh in model.Meshes)
                {
                    foreach (ModelMeshPart part in mesh.MeshParts)
                    {
                        effect.Parameters["World"].SetValue(world);
                        effect.Parameters["View"].SetValue(view);
                        effect.Parameters["Projection"].SetValue(projection);
                        Matrix worldInverseTransposeMatrix = Matrix.Transpose(Matrix.Invert(mesh.ParentBone.Transform));
                        effect.Parameters["WorldInverseTranspose"].SetValue(worldInverseTransposeMatrix);

                        effect.Parameters["BumpHeight"].SetValue(bumpHeight);
                        effect.Parameters["NormalMapRepeatU"].SetValue(normalMapRepeatU);
                        effect.Parameters["NormalMapRepeatV"].SetValue(normalMapRepeatV);
                        effect.Parameters["MipMap"].SetValue(mipMap);
                        effect.Parameters["NormalizeNormalMap"].SetValue(normalizeNormalMap);
                        effect.Parameters["NormalizeTangentFrame"].SetValue(normalizeTangentFrame);

                        effect.Parameters["normalMap"].SetValue(texture);
                        effect.Parameters["LightPosition"].SetValue(lightPosition);
                        effect.Parameters["CameraPosition"].SetValue(cameraPosition);
                        effect.Parameters["DiffuseIntensity"].SetValue(1.0f);
                        effect.Parameters["DiffuseColor"].SetValue(new Vector4(1.0f, 1.0f, 1.0f, 1.0f));
                        effect.Parameters["SpecularColorIntensity"].SetValue(0.1f);
                        effect.Parameters["SpecularColor"].SetValue(new Vector4(1.0f, 1.0f, 1.0f, 1.0f));
                        effect.Parameters["Shininess"].SetValue(100.0f);

                        pass.Apply();
                        GraphicsDevice.SetVertexBuffer(part.VertexBuffer);
                        GraphicsDevice.Indices = part.IndexBuffer;
                        GraphicsDevice.DrawIndexedPrimitives(PrimitiveType.TriangleList, part.VertexOffset, part.StartIndex, part.PrimitiveCount);
                    }
                }
            }
        }
        */

        #endregion

        #region - Values -
        void DrawValues()
        {
            int height = 20;
            int line = 1;
            float leftMargin = 20f;

            spriteBatch.DrawString(font, "Shader = " + shaderName, new Vector2(leftMargin, height * line++), Color.White);
            spriteBatch.DrawString(font, "Texture = " + texture, new Vector2(leftMargin, height * line++), Color.White);
            spriteBatch.DrawString(font, "VALUES = " + showValues, new Vector2(leftMargin, height * line++), Color.White);

            spriteBatch.DrawString(font, "Normal ReapatU = " + normalMapRepeatU, new Vector2(leftMargin, height * line++), Color.White);
            spriteBatch.DrawString(font, "Normal ReapatV = " + normalMapRepeatV, new Vector2(leftMargin, height * line++), Color.White);
            spriteBatch.DrawString(font, "Bump Height = " + bumpHeight, new Vector2(leftMargin, height * line++), Color.White);
            spriteBatch.DrawString(font, "Mip Map = " + mipMap, new Vector2(leftMargin, height * line++), Color.White);
            spriteBatch.DrawString(font, "Self Shadow = " + selfShadow, new Vector2(leftMargin, height * line++), Color.White);
            spriteBatch.DrawString(font, "Normalize Tangent Frame = " + normalizeTangentFrame, new Vector2(leftMargin, height * line++), Color.White);
            spriteBatch.DrawString(font, "Normalize Normal Map = " + normalizeNormalMap, new Vector2(leftMargin, height * line++), Color.White);
            spriteBatch.DrawString(font, "Reset Values: R ", new Vector2(leftMargin, height * line++), Color.White);

        } // COMPLETE
        void DrawControls()
        {
            int height = 20;
            int line = 1;
            float leftMargin = graphics.PreferredBackBufferWidth * 0.75f;
            spriteBatch.DrawString(font, "Change Textures: 1-7 ", new Vector2(leftMargin, height * line++), Color.White);
            spriteBatch.DrawString(font, "Change Shader: F1-F ", new Vector2(leftMargin, height * line++), Color.White);
            spriteBatch.DrawString(font, "Change Normal ReapatU: U/u ", new Vector2(leftMargin, height * line++), Color.White);
            spriteBatch.DrawString(font, "Change Normal ReapatV: V/v ", new Vector2(leftMargin, height * line++), Color.White);
            spriteBatch.DrawString(font, "Change Bump Height: W/w ", new Vector2(leftMargin, height * line++), Color.White);
            spriteBatch.DrawString(font, "Change Mip Map: M ", new Vector2(leftMargin, height * line++), Color.White);
            spriteBatch.DrawString(font, "Change Self Shadow: P ", new Vector2(leftMargin, height * line++), Color.White);
            spriteBatch.DrawString(font, "Normalize Tangent Frame: T ", new Vector2(leftMargin, height * line++), Color.White);
            spriteBatch.DrawString(font, "Normalize Normal Map: N ", new Vector2(leftMargin, height * line++), Color.White);
            spriteBatch.DrawString(font, "Reset Camera/Light: Enter ", new Vector2(leftMargin, height * line++), Color.White);
        } // COMPLETE
        void ResetValues()
        { 
            normalizeTangentFrame = normalizeNormalMap = mipMap = mipMap = selfShadow = 0;
            normalMapRepeatU = normalMapRepeatV = bumpHeight = 1.0f;
        }  // COMPLETE

        #endregion
    }
}