using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace Lab04
{
    public class Lab04 : Game
    {
        private GraphicsDeviceManager _graphics;
        private SpriteBatch _spriteBatch;

        // Lab04
        Vector4 specularColor = new Vector4(1, 1, 1, 1);
        int currentShader = 0;

        // Lab03
        Model model; 

        // ?? ambient, diffuseColor ??
        Vector4 ambient = new Vector4(0, 0, 0, 0);
        float ambientIntensity = 0;
        Vector4 diffuseColor = new Vector4(1, 1, 1, 1);
        Vector3 lightPosition = new Vector3(1, 1, 1);

        // Main Exercise
        MouseState preMouseState;
        float angle2;
        float xSensitivity = 0.01f;
        float ySensitivity = 0.01f;

        // Lab02
        Effect effect;
        float angle;
        Matrix view;
        Matrix world;
        Matrix projection;
        Vector3 cameraPosition;

        public Lab04()
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
            model = Content.Load<Model>("Torus");
            effect = Content.Load<Effect>("SimplestPhongLighting");
        }

        protected override void Update(GameTime gameTime)
        {
            if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed || Keyboard.GetState().IsKeyDown(Keys.Escape))
                Exit();

            // TODO: Add your update logic here

            // Swap between shaders
            if (Keyboard.GetState().IsKeyDown(Keys.D0))
            {
                currentShader = 0;
            }
            if (Keyboard.GetState().IsKeyDown(Keys.D1))
            {
                currentShader = 1;
            }

            MouseState currentMouseState = Mouse.GetState();

            // Movement with mouse
            if (currentMouseState.LeftButton == ButtonState.Pressed && preMouseState.LeftButton == ButtonState.Pressed)
            {
                angle -= (preMouseState.X - currentMouseState.X) * xSensitivity;
                angle2 -= (preMouseState.Y - currentMouseState.Y) * ySensitivity;
            }

            preMouseState = currentMouseState;

            /*
            world = Matrix.Identity;
            view = Matrix.CreateRotationY(angle) * 
                Matrix.CreateRotationX(angle2) *
                Matrix.CreateTranslation(new Vector3(0, 0, -20));
            */
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
                        effect.Parameters["DiffuseIntensity"].SetValue(1f);

                        Matrix worldInverseTranspose = Matrix.Transpose(Matrix.Invert(mesh.ParentBone.Transform));
                        effect.Parameters["WorldInverseTranspose"].SetValue(worldInverseTranspose);

                        // Lab04
                        effect.Parameters["SpecularColor"].SetValue(specularColor);
                        // effect.Parameters["SpecularIntensity"].SetValue(1);
                        effect.Parameters["Shininess"].SetValue(20f);
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
    }
}