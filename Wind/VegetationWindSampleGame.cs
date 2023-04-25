#region File Description
#endregion

#region Using Statements
using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Input.Touch;
#endregion

namespace VegetationWindSample
{
    /// <summary>
    /// </summary>
    public class VegetationWindSampleGame : Microsoft.Xna.Framework.Game
    {
        #region Constants

        // the following constants control the speed at which the camera moves
        // how fast does the camera move up, down, left, and right?
        const float CameraRotateSpeed = .002f;

        // the following constants control how the camera's default position
        const float CameraDefaultArc = -25.0f;
        const float CameraDefaultRotation = 185;
        const float CameraDefaultDistance = 37.0f;

        #endregion

        #region Fields

        GraphicsDeviceManager graphics;
        Effect windEffect;

        // a SpriteBatch and SpriteFont, which we will use to draw the objects' names
        // when they are selected.
        SpriteBatch spriteBatch;
        SpriteFont spriteFont;

        // the table that all of the objects are drawn on, and table model's 
        // absoluteBoneTransforms. Since the table is not animated, these can be 
        // calculated once and saved.
        Model table;
        Matrix[] tableAbsoluteBoneTransforms;

        // these are the models that we will draw on top of the table. we'll store them
        // and their bone transforms in arrays. Again, since these models aren't
        // animated, we can calculate their bone transforms once and save the result.
        // each model will need one more matrix: a world transform. This matrix will be
        // used to place each model at a different location in the world.

        Matrix viewMatrix;
        Matrix projectionMatrix;

        // this variable will store the current rotation value as the camera
        // rotates around the scene
        float cameraRotation = CameraDefaultRotation;

        #endregion

        #region Initialization

        public VegetationWindSampleGame()
        {
            graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";

#if WINDOWS_PHONE
            // Frame rate is 30 fps by default for Windows Phone.
            TargetElapsedTime = TimeSpan.FromTicks(333333);
            graphics.IsFullScreen = true;
#endif
        }

        protected override void Initialize()
        {
            // Set up the world transforms that each model will use. They'll be
            // positioned in a line along the x axis.
            plantWorldTransform = Matrix.CreateTranslation(new Vector3(0, 5f, 0));
            base.Initialize();
        }

        Matrix plantWorldTransform;
        VertexBuffer plantVB;
        IndexBuffer plantIB;
        Texture2D plantTexture;

        /// <summary>
        /// Load your graphics content.
        /// </summary>
        protected override void LoadContent()
        {
            plantTexture = GeneratePlant.GetTexture(GraphicsDevice);

            GeneratePlant.Generate(GraphicsDevice, out plantVB, out plantIB);

            table = Content.Load<Model>("grid");
            tableAbsoluteBoneTransforms = new Matrix[table.Bones.Count];
            table.CopyAbsoluteBoneTransformsTo(tableAbsoluteBoneTransforms);

            // create a spritebatch and load the font, which we'll use to draw the
            // models' names.
            spriteBatch = new SpriteBatch(graphics.GraphicsDevice);
            spriteFont = Content.Load<SpriteFont>("hudFont");

            // calculate the projection matrix now that the graphics device is created.
            projectionMatrix = Matrix.CreatePerspectiveFieldOfView(
                MathHelper.PiOver4, GraphicsDevice.Viewport.AspectRatio, .01f, 1000);

            windEffect = Content.Load<Effect>(@"Wind");

        }

        #endregion

        #region Update and Draw

        Vector2 lastWindSpeed;
        Random random = new Random();
        Vector2 newWindSpeed;
        Vector2 currentWindSpeed;
        float timeSinceLastThing;
        float totalTime;

        KeyboardState previousKeyboardState;
        KeyboardState currentKeyboardState;

        private bool IsKeyPressed(Keys key)
        {
            return !previousKeyboardState.IsKeyDown(key) && currentKeyboardState.IsKeyDown(key);
        }

        /// <summary>
        /// Allows the game to run logic.
        /// </summary>
        protected override void Update(GameTime gameTime)
        {
            previousKeyboardState = currentKeyboardState;
            currentKeyboardState = Keyboard.GetState();

            // Check for exit.
            if (Keyboard.GetState().IsKeyDown(Keys.Escape) ||
                GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed)
            {
                Exit();
            }

            ControlParameters();

            // we rotate our view around the models over time
            float time = (float)gameTime.ElapsedGameTime.TotalMilliseconds;

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

            cameraRotation += time * CameraRotateSpeed;
            Matrix unrotatedView = Matrix.CreateLookAt(
                new Vector3(0, 5f, -CameraDefaultDistance), new Vector3(0, 5f, 0), Vector3.Up);

            viewMatrix = Matrix.CreateRotationY(MathHelper.ToRadians(cameraRotation)) *
                          Matrix.CreateRotationX(MathHelper.ToRadians(CameraDefaultArc)) *
                          unrotatedView;

            base.Update(gameTime);
        }

        private float detailBranchAmplitude = 0.05f;
        private float detailSideToSideAmplitude = 0.05f;
        private float mainBendScale = 0.01f;
        private bool detailBranchOn = true;
        private bool detailSideToSideOn = true;
        private bool mainBendOn = true;

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

        /// <summary>
        /// This is called when the game should draw itself.
        /// </summary>
        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.CornflowerBlue);

            GraphicsDevice.DepthStencilState = DepthStencilState.Default;
            GraphicsDevice.BlendState = BlendState.Opaque;
            GraphicsDevice.SamplerStates[0] = SamplerState.LinearWrap;

            // draw the table. DrawModel is a function defined below draws a model using
            // a world matrix and the model's bone transforms.
            DrawModel(table, Matrix.Identity, tableAbsoluteBoneTransforms);

            DrawPlant(plantWorldTransform);

            int lineHeight = 22;

            spriteBatch.Begin();
            spriteBatch.DrawString(spriteFont, "(Toggle) (Decrease) (Increase)", Vector2.Zero, Color.Goldenrod);
            spriteBatch.DrawString(spriteFont, string.Format("(Q) (W) (E)  Main bending ({0}): {1:0.000}", mainBendOn ? "On" : "Off", mainBendScale), new Vector2(0, lineHeight), Color.White);
            spriteBatch.DrawString(spriteFont, string.Format("(A) (S) (D)  Branch bending ({0}): {1:0.000}", detailBranchOn ? "On" : "Off", detailBranchAmplitude), new Vector2(0, lineHeight * 2), Color.White);
            spriteBatch.DrawString(spriteFont, string.Format("(Z) (X) (C)  S-2-S bending ({0}): {1:0.000}", detailSideToSideOn ? "On" : "Off", detailSideToSideAmplitude), new Vector2(0, lineHeight * 3), Color.White);
            spriteBatch.End();
            base.Draw(gameTime);
        }

        Matrix absoluteBoneTransform = new Matrix(1.00112081f, 0, -0.0000001750415f, 0, -0.0000001750415f, 0, -1.00112057f, 0, 0, 1.00112057f, 0, 0, 2.47922778f, -4.368915f, -0.118832514f, 1.0f);

        private void DrawPlant(Matrix worldTransform)
        {
            // Set wind stuff that varies each time:
            windEffect.Parameters["WindSpeed"].SetValue(currentWindSpeed);
            windEffect.Parameters["Time"].SetValue(totalTime);
            windEffect.Parameters["BranchAmplitude"].SetValue(detailBranchOn ? detailBranchAmplitude : 0f);
            windEffect.Parameters["DetailAmplitude"].SetValue(detailSideToSideOn ? detailSideToSideAmplitude : 0f);
            windEffect.Parameters["BendScale"].SetValue(mainBendOn ? mainBendScale : 0f);

            windEffect.Parameters["Texture"].SetValue(plantTexture);
            windEffect.Parameters["View"].SetValue(viewMatrix);
            windEffect.Parameters["Projection"].SetValue(projectionMatrix);
            Matrix finalWorld = absoluteBoneTransform * worldTransform;
            windEffect.Parameters["World"].SetValue(finalWorld);

            // Since the leaf polygons are one-sided, draw the thing with two different cull-modes.
            windEffect.Parameters["InvertNormal"].SetValue(false);
            GraphicsDevice.RasterizerState = RasterizerState.CullCounterClockwise;
            windEffect.CurrentTechnique.Passes[0].Apply();
            GraphicsDevice.SetVertexBuffer(plantVB);
            GraphicsDevice.Indices = plantIB;
            GraphicsDevice.DrawIndexedPrimitives(PrimitiveType.TriangleList, 0, 0, plantVB.VertexCount, 0, plantIB.IndexCount / 3);

            // We need to reverse the normal for proper lighting when using reverse winding order.
            windEffect.Parameters["InvertNormal"].SetValue(true);
            GraphicsDevice.RasterizerState = RasterizerState.CullClockwise;
            windEffect.CurrentTechnique.Passes[0].Apply();
            GraphicsDevice.DrawIndexedPrimitives(PrimitiveType.TriangleList, 0, 0, plantVB.VertexCount, 0, plantIB.IndexCount / 3);

            // Restore standard state
            GraphicsDevice.RasterizerState = RasterizerState.CullCounterClockwise;
        }

        /// <summary>
        /// DrawModel is a helper function that takes a model, world matrix, and
        /// bone transforms. It does just what its name implies, and draws the model.
        /// </summary>
        /// <param name="model">the model to draw</param>
        /// <param name="worldTransform">where to draw the model</param>
        /// <param name="absoluteBoneTransforms">the model's bone transforms. this can
        /// be calculated using the function Model.CopyAbsoluteBoneTransformsTo</param>
        private void DrawModel(Model model, Matrix worldTransform,
                               Matrix[] absoluteBoneTransforms, bool isPlant = false)
        {
            // nothing tricky in here; this is the same model drawing code that we see
            // everywhere. we'll loop over all of the meshes in the model, set up their
            // effects, and then draw them.
            foreach (ModelMesh mesh in model.Meshes)
            {
                foreach (Effect effect in mesh.Effects)
                {
                    BasicEffect basicEffect = effect as BasicEffect;
                    basicEffect.EnableDefaultLighting();
                    basicEffect.View = viewMatrix;
                    basicEffect.Projection = projectionMatrix;
                    basicEffect.World = absoluteBoneTransforms[mesh.ParentBone.Index] * worldTransform;
                }
                mesh.Draw();
            }
        }
        #endregion
    }
}
