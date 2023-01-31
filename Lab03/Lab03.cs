using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace Lab03
{
    public class Lab03 : Game
    {
        private GraphicsDeviceManager _graphics;
        private SpriteBatch _spriteBatch;
        
        // Lab03
        Model model;
        // ?? ambient, diffuseColor ??
        Vector4 ambient = new Vector4(0,0,0,0);
        float ambientIntensity = 0;
        Vector4 diffuseColor = new Vector4(1,1,1,1);
        Vector3 lightPosition = new Vector3(1,1,1);

        // Main Exercise
        MouseState previousMouseState;
        float angle2;
        float xSensitivity = 0.1f;
        float ySensitivity = 0.1f;

        // Lab02
        Effect effect;
        float angle;
        Matrix view;
        Matrix world;
        Matrix projection;
        Vector3 cameraPosition;

        public Lab03()
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

            model = Content.Load<Model>("bunny");
            effect = Content.Load<Effect>("SimplestLighting");

        }

        protected override void Update(GameTime gameTime)
        {
            if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed || Keyboard.GetState().IsKeyDown(Keys.Escape))
                Exit();

            // TODO: Add your update logic here

            if (Keyboard.GetState().IsKeyDown(Keys.Left)) {
                    angle += 0.02f;

                    cameraPosition = new Vector3(
                        (float)System.Math.Cos(angle),
                        0,
                        (float)System.Math.Sin(angle)
                        );
            }
            if (Keyboard.GetState().IsKeyDown(Keys.Right))
            {
                angle -= 0.02f;
                cameraPosition = new Vector3(
                        (float)System.Math.Cos(angle),
                        0,
                        (float)System.Math.Sin(angle)
                        );
            }

            if (Mouse.GetState().LeftButton == ButtonState.Pressed)
            {
                // Bunny view resets when dragging?
                angle += (previousMouseState.X - Mouse.GetState().X) * xSensitivity;
                angle2 += (previousMouseState.Y - Mouse.GetState().Y) * ySensitivity;
            }
            
            Vector3 camera = Vector3.Transform(
                new Vector3(0, 0, 20),
                Matrix.CreateRotationX(angle2) * Matrix.CreateRotationY(angle)
                );

            world = Matrix.Identity;
            view = Matrix.CreateLookAt(camera, Vector3.Zero, Vector3.UnitY);
            projection = Matrix.CreatePerspectiveFieldOfView(
                MathHelper.ToRadians(90),
                1.33f,
                0.1f,
                100
                );

            previousMouseState = Mouse.GetState();

            base.Update(gameTime);
        }

        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.CornflowerBlue);

            // TODO: Add your drawing code here

            // model.Draw(world, view, projection); // built in draw method without shader

            effect.CurrentTechnique = effect.Techniques[0];
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
                        effect.Parameters["DiffuseLightDirection"].SetValue(lightPosition);
                        effect.Parameters["DiffuseIntensity"].SetValue(1f);

                        Matrix worldInverseTranspose = Matrix.Transpose(Matrix.Invert(mesh.ParentBone.Transform));
                        effect.Parameters["WorldInverseTranspose"].SetValue(worldInverseTranspose);

                        pass.Apply();
                        // ?? What is VertexBuffer, IndexBuffer ??
                        GraphicsDevice.SetVertexBuffer(part.VertexBuffer);
                        GraphicsDevice.Indices = part.IndexBuffer;

                        GraphicsDevice.DrawIndexedPrimitives(
                            PrimitiveType.TriangleList, part.VertexOffset, 0,
                            part.NumVertices, part.StartIndex, part.PrimitiveCount
                            );
                    }
                }
            }

                    base.Draw(gameTime);
        }
    }
}