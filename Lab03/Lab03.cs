using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace Lab03
{
    public class Lab03 : Game
    {
        private GraphicsDeviceManager _graphics;
        private SpriteBatch _spriteBatch;

        // My Code
        Effect effect;
        VertexPositionTexture[] vertices =
        {
            new VertexPositionTexture(new Vector3(0, 1, 0), new Vector2(0.5f,0)),
            new VertexPositionTexture(new Vector3(1, 0, 0), new Vector2(1,1)),
            new VertexPositionTexture(new Vector3(-1, 0, 0), new Vector2(0,1))
        };

        Model model;

        float angle;
        float distance = 1;
        Matrix view;
        Matrix world;
        Matrix projection;

        // float4 AmbientColor = new Vector4(0,0,0,0);
      


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
            effect = Content.Load<Effect>("SimplestRotate");
            //effect = Content.Load<Effect>("SimplestVertexShader");
            model = Content.Load<Model>("bunny");
        }

        protected override void Update(GameTime gameTime)
        {
            if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed || Keyboard.GetState().IsKeyDown(Keys.Escape))
                Exit();

            // TODO: Add your update logic here

            // Keyboard Controls
            if (Keyboard.GetState().IsKeyDown(Keys.Left))
            {
                angle += 0.02f;
                Vector3 offset = new Vector3(
                    (float)System.Math.Cos(angle),
                    (float)System.Math.Sin(angle),
                    0);
                effect.Parameters["offset"].SetValue(offset);
            }
            if (Keyboard.GetState().IsKeyDown(Keys.Right))
            {
                angle -= 0.02f;
                Vector3 offset = new Vector3(
                    (float)System.Math.Cos(angle),
                    (float)System.Math.Sin(angle),
                    0);
                effect.Parameters["offset"].SetValue(offset);
            }
            if (Keyboard.GetState().IsKeyDown(Keys.Up))
            {
                distance -= 0.02f;
            }
            if (Keyboard.GetState().IsKeyDown(Keys.Down))
            {
                distance += 0.02f;
            }

            Matrix world = Matrix.Identity;
            Matrix view = Matrix.CreateLookAt(new Vector3(angle, 0, distance), new Vector3(), new Vector3(0, 1, 0));
            Matrix projection = Matrix.CreatePerspectiveFieldOfView(
                MathHelper.ToRadians(90),
                GraphicsDevice.Viewport.AspectRatio,
                0.1f, 100);

            effect.Parameters["World"].SetValue(world);
            effect.Parameters["View"].SetValue(view);
            effect.Parameters["Projection"].SetValue(projection);

            base.Update(gameTime);
        }

        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.CornflowerBlue);

            //Allows Opacity/Blending?

            GraphicsDevice.BlendState = BlendState.AlphaBlend;

            // TODO: Add your drawing code here
            effect.CurrentTechnique = effect.Techniques[0];
            foreach (EffectPass pass in effect.CurrentTechnique.Passes)
            {


                foreach (ModelMesh mesh in model.Meshes)
                {
                    pass.Apply();
                    foreach (ModelMeshPart part in mesh.MeshParts)
                    {
                        

                        effect.Parameters["World"].SetValue(mesh.ParentBone.Transform);
                        effect.Parameters["View"].SetValue(view);
                        effect.Parameters["Projection"].SetValue(projection);
                        effect.Parameters["AmbientColor"].SetValue(ambient);
                        effect.Parameters["AmbientIntensity"].SetValue(ambientIntensity);
                        
                        GraphicsDevice.SetVertexBuffer(part.VertexBuffer);
                        GraphicsDevice.Indices = part.IndexBuffer;

                        GraphicsDevice.DrawIndexedPrimitives(
                        PrimitiveType.TriangleList, part.VertexOffset, 0,
                        part.NumVertices, part.StartIndex, part.PrimitiveCount);
                    }
                    
                    // set buffers and draw mesh model
                }
            };

            model.Draw(world, view, projection);
            base.Draw(gameTime);
        }
    }
}