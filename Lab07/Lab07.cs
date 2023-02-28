using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace Lab07
{
    public class Lab07 : Game
    {
        private GraphicsDeviceManager _graphics;
        private SpriteBatch _spriteBatch;

        // Content
        Model model;
        Texture2D texture;
        Effect effect;

        // Camera
        float angle;            // X Rotation
        float angle2;           // Y Rotation
        float distance = 10f;
        Vector3 cameraPosition;
        Vector3 lightPosition = new Vector3(100, 100, 100);
        Matrix world;
        Matrix view;
        Matrix projection;
        MouseState preMouseState;

        // Lighting Angles
        private float angleL;
        private float angleL2;

        // Move Camera
        private float cameraTarget;

        public Lab07()
        {
            _graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";
            IsMouseVisible = true;

            _graphics.GraphicsProfile = GraphicsProfile.HiDef;
        }

        protected override void Initialize()
        {

            base.Initialize();
        }

        protected override void LoadContent()
        {
            _spriteBatch = new SpriteBatch(GraphicsDevice);

            model = Content.Load<Model>("Plane");
            texture = Content.Load<Texture2D>("NormalMap/round");
            effect = Content.Load<Effect>("BumpMap");
        }

        protected override void Update(GameTime gameTime)
        {
            if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed || Keyboard.GetState().IsKeyDown(Keys.Escape))
                Exit();


            #region - Camera Movement -

            MouseState currentMouseState = Mouse.GetState();

            // Rotate Camera
            if (currentMouseState.LeftButton == ButtonState.Pressed && 
                preMouseState.LeftButton == ButtonState.Pressed)
            {
                angle -= (preMouseState.X - currentMouseState.X) / 100f;
                angle2 -= (preMouseState.Y - currentMouseState.Y) / 100f;
            }
            // Zoom In/Out
            if (Mouse.GetState().RightButton == ButtonState.Pressed)
            {
                distance += (Mouse.GetState().X - preMouseState.X) / 100f;
            }
            /* TODO: Fix the cameraTarget semantic error.
            if (Mouse.GetState().MiddleButton == ButtonState.Pressed)
            {
                Vector3 ViewRight = Vector3.Transform(Vector3.UnitX,
                Matrix.CreateRotationX(angle2) *
                Matrix.CreateRotationY(angle));
                Vector3 ViewUp = Vector3.Transform(Vector3.UnitY,
                Matrix.CreateRotationX(angle2) *
                Matrix.CreateRotationY(angle));
                cameraTarget -= ViewRight * ((Mouse.GetState().X - preMouseState.X) / 10f);
                cameraTarget += ViewUp * ((Mouse.GetState().Y - preMouseState.Y) / 10f);
            }
            */
            preMouseState = currentMouseState;

            #endregion

            #region - Lighting Angle -

            if (Keyboard.GetState().IsKeyDown(Keys.Left)) angleL += 0.02f;
            if (Keyboard.GetState().IsKeyDown(Keys.Right)) angleL -= 0.02f;
            if (Keyboard.GetState().IsKeyDown(Keys.Up)) angleL2 += 0.02f;
            if (Keyboard.GetState().IsKeyDown(Keys.Down)) angleL2 -= 0.02f;

            lightPosition = Vector3.Transform(new Vector3(0, 0, 10), Matrix.CreateRotationX(angleL2) * Matrix.CreateRotationY(angleL));

            #endregion

            #region - Matrix Transformations -

            world = Matrix.Identity;
            cameraPosition = Vector3.Transform(
                new Vector3(0, 0, distance),
                Matrix.CreateRotationX(angle2) * Matrix.CreateRotationY(angle)
                );
            view = Matrix.CreateLookAt(
                cameraPosition,
                new Vector3(),
                Vector3.Transform(
                    Vector3.Up,
                    Matrix.CreateRotationX(angle2) * Matrix.CreateRotationY(angle))
                );
            projection = Matrix.CreatePerspectiveFieldOfView(
                MathHelper.ToRadians(90),
                1.33f,
                0.1f, 
                100
                );

            #endregion

            base.Update(gameTime);
        }

        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.CornflowerBlue);

            // TODO: Add your drawing code here
            //GraphicsDevice.BlendState = BlendState.Opaque;
            //GraphicsDevice.DepthStencilState = new DepthStencilState();

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
                        effect.Parameters["CameraPosition"].SetValue(cameraPosition);

                        Matrix worldInverseTranspose = Matrix.Transpose(Matrix.Invert(mesh.ParentBone.Transform));
                        effect.Parameters["WorldInverseTranspose"].SetValue(worldInverseTranspose);
                        effect.Parameters["LightPosition"].SetValue(lightPosition);
                        effect.Parameters["AmbientColor"].SetValue(new Vector4(1,1,1,1));
                        effect.Parameters["AmbientIntensity"].SetValue(0);
                        effect.Parameters["DiffuseColor"].SetValue(new Vector4(1, 1, 1, 1));
                        effect.Parameters["DiffuseIntensity"].SetValue(1f);
                        effect.Parameters["SpecularColor"].SetValue(new Vector4(1, 1, 1, 1));
                        effect.Parameters["Shininess"].SetValue(20f);
                        effect.Parameters["normalMap"].SetValue(texture);

                        pass.Apply();
                        GraphicsDevice.SetVertexBuffer(part.VertexBuffer);
                        GraphicsDevice.Indices = part.IndexBuffer;
                        GraphicsDevice.DrawIndexedPrimitives(
                        PrimitiveType.TriangleList, part.VertexOffset, part.StartIndex, part.PrimitiveCount);
                    }
                }
            }

            base.Draw(gameTime);
        }
    }
}