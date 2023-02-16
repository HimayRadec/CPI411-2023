using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using CPI411.SimpleEngine;

namespace Lab06
{
    public class Lab06 : Game
    {
        private GraphicsDeviceManager _graphics;
        private SpriteBatch _spriteBatch;

        Model model;
        Skybox skybox;
        Texture2D texture;
        Effect effect;

        // Basic Variables For 3D
        float angle;
        float angle2;

        Matrix view;
        Matrix world;
        Matrix projection;
        Vector3 cameraPosition;
        MouseState preMouseState;

        public Lab06()
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
            model = Content.Load<Model>("Helicopter");
            texture = Content.Load<Texture2D>("HelicopterTexture");
            effect = Content.Load<Effect>("Reflection");
            string[] skyboxTextures = {
                "skybox/SunsetPNG1",
                "skybox/SunsetPNG2",
                "skybox/SunsetPNG3",
                "skybox/SunsetPNG4",
                "skybox/SunsetPNG5",
                "skybox/SunsetPNG6"
            };
            skybox = new Skybox(skyboxTextures, Content, GraphicsDevice);
        }

        protected override void Update(GameTime gameTime)
        {
            if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed || Keyboard.GetState().IsKeyDown(Keys.Escape))
                Exit();

            // TODO: Add your update logic here
            world = Matrix.Identity;
            cameraPosition = Vector3.Transform(
                new Vector3(0, 0, 3),
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
            RasterizerState originalRasterizerState = _graphics.GraphicsDevice.RasterizerState;
            RasterizerState rasterizerState = new RasterizerState();
            rasterizerState.CullMode = CullMode.None;
            _graphics.GraphicsDevice.RasterizerState = rasterizerState;
            skybox.Draw(view, projection, cameraPosition);
            _graphics.GraphicsDevice.RasterizerState = originalRasterizerState;
            DrawModelWithEffect();

            base.Draw(gameTime);
        }
        private void DrawModelWithEffect()
        {
            effect.CurrentTechnique = effect.Techniques[0];
            foreach (EffectPass pass in effect.CurrentTechnique.Passes)
            {
                foreach (ModelMesh mesh in model.Meshes)
                {
                    foreach (ModelMeshPart part in mesh.MeshParts)
                    {
                        Matrix worldInverseTranspose = Matrix.Transpose(Matrix.Invert(mesh.ParentBone.Transform));
                        effect.Parameters["WorldInverseTranspose"].SetValue(worldInverseTranspose);

                        effect.Parameters["World"].SetValue(mesh.ParentBone.Transform);
                        effect.Parameters["View"].SetValue(view);
                        effect.Parameters["Projection"].SetValue(projection);

                        // effect.Parameters["decalMap"].SetValue(texture);

                        effect.Parameters["CameraPosition"].SetValue(cameraPosition);

                        // effect.Parameters["environmentMap"].SetValue(cameraPosition);

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
    }
}