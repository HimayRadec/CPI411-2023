using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

using CPI411.SimpleEngine;
using System;

namespace Assignment04V2
{
    public class Assignment04V2 : Game
    {
        private GraphicsDeviceManager _graphics;
        private SpriteBatch spriteBatch;
        // **** TEMPLATE ************//
        SpriteFont font;
        Effect effect;
        Matrix world = Matrix.Identity;
        Matrix view = Matrix.CreateLookAt(new Vector3(20, 0, 0), new Vector3(0, 0, 0), Vector3.UnitY);
        Matrix projection = Matrix.CreatePerspectiveFieldOfView(MathHelper.ToRadians(45), 800f / 600f, 0.1f, 100f);
        Vector3 cameraPosition, cameraTarget, lightPosition;
        Matrix lightView, lightProjection;

        float angle = 0;
        float angle2 = 0;
        float angleL = 0;
        float angleL2 = 0;
        float distance = 40;

        bool showControls = true;
        bool showValues = true;

        Model model;
        Model[] models;
        Texture2D texture;

        MouseState preMouse;
        KeyboardState previousKeyboardState;
        KeyboardState currentKeyboardState;

        private bool IsKeyPressed(Keys key)
        {
            return (!previousKeyboardState.IsKeyDown(key) && currentKeyboardState.IsKeyDown(key));
        }

        // **** TEMPLATE ************//

        // Assignment 04
        ParticleManager particleManager;
        Vector3 particlePosition = new Vector3(0, 0, 0);
        Vector3 particleVelocity = new Vector3(0, 0, 0);
        Vector3 particleAcceleration = new Vector3(0, -3f, 0);
        float particleWind = 0f;
        float particleSpeed = 1f;

        System.Random random;

        int emissionShape = 0;
        enum EmissionShape
        {
            Square,
            Curve,
            Ring
        };

        int emissionType = 0;
        enum EmissionType
        {
            F1,
            F2,
            F3
        };

        int particleMaxNum = 10000;
        int particleEmissionNum = 10;
        int particleEmissionSpan = 50;
        int particleTotalTime = 0;
        int particleMaxAge = 1;
        int particleBoundOn = 0;
        float emissionSize = 1.0f;
        float particleResilence = 1.0f;
        float particleFriction = 1.0f;
        float particleGroundY = -2.0f;

        public Assignment04V2()
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
            effect = Content.Load<Effect>("ParticleShader");
            texture = Content.Load<Texture2D>("fire");
            model = Content.Load<Model>("Plane");


            random = new System.Random();
            particleManager = new ParticleManager(GraphicsDevice, 120);
            particlePosition = new Vector3(0, 0, 0);
        }

        protected override void Update(GameTime gameTime)
        {
            if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed || Keyboard.GetState().IsKeyDown(Keys.Escape))
                Exit();
            previousKeyboardState = currentKeyboardState;
            currentKeyboardState = Keyboard.GetState();

            #region - TEMPLATE -
            // ************ TEMPLATE ************ //
            if (Keyboard.GetState().IsKeyDown(Keys.Left)) angleL += 0.02f;
            if (Keyboard.GetState().IsKeyDown(Keys.Right)) angleL -= 0.02f;
            if (Keyboard.GetState().IsKeyDown(Keys.Up)) angleL2 += 0.02f;
            if (Keyboard.GetState().IsKeyDown(Keys.Down)) angleL2 -= 0.02f;
            if (Keyboard.GetState().IsKeyDown(Keys.S)) { angle = angle2 = angleL = angleL2 = 0; distance = 30; cameraTarget = Vector3.Zero; }
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
            // ********************************** //
            #endregion

            // Lab 10 
            if (Keyboard.GetState().IsKeyDown(Keys.P))
            {

                _generateParticles();
            }

            // Update Particles
            particleManager.Update(gameTime.ElapsedGameTime.Milliseconds * 0.001f);
            base.Update(gameTime);
        }

        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.CornflowerBlue);

            // Draw the torus model
            Matrix modelWorldMatrix = Matrix.Identity;
            foreach (ModelMesh mesh in model.Meshes)
            {
                foreach (BasicEffect effect in mesh.Effects)
                {
                    effect.World = modelWorldMatrix;
                    effect.View = view;
                    effect.Projection = projection;
                }
                mesh.Draw();
            }

            // Set depth buffer to read-only before drawing particles
            GraphicsDevice.DepthStencilState = new DepthStencilState { DepthBufferEnable = true, DepthBufferWriteEnable = false };

            // Draw particles
            GraphicsDevice.BlendState = BlendState.AlphaBlend;
            GraphicsDevice.RasterizerState = RasterizerState.CullNone;

            effect.CurrentTechnique = effect.Techniques[0];
            effect.CurrentTechnique.Passes[0].Apply();
            effect.Parameters["World"].SetValue(Matrix.Identity);
            effect.Parameters["View"].SetValue(view);
            effect.Parameters["Projection"].SetValue(projection);
            effect.Parameters["Texture"].SetValue(texture);
            effect.Parameters["InverseCamera"].SetValue(
                Matrix.CreateRotationX(angle2) * Matrix.CreateRotationY(angle)
                );

            particleManager.Draw(GraphicsDevice);

            // Reset the depth buffer to its default state after drawing particles
            GraphicsDevice.DepthStencilState = DepthStencilState.Default;

            base.Draw(gameTime);
        }

        private Vector3 _getParticleVelocity()
        {
            if (emissionType == (int)EmissionType.F1)
                return new Vector3(0, 2, 0);
            else if (emissionType == (int)EmissionType.F2)
                return new Vector3((float)random.NextDouble() * 2 - 1, (float)random.NextDouble() * 2 - 1, (float)random.NextDouble() * 2 - 1);
            else return particleVelocity;
        }
        private Vector3 _getParticleAcceleration()
        {
            if (emissionType == (int)EmissionType.F1)
                return new Vector3(0, 0, 0);
            else return particleAcceleration;
        }
        private Vector3 _getParticlePosition()
        {
            if (emissionShape == (int)EmissionShape.Square)
            {
                return new Vector3(
                    (float)(emissionSize * (random.NextDouble() - 0.5)),
                    0,
                    (float)(emissionSize * (random.NextDouble() - 0.5)));
            }
            else if (emissionShape == (int)EmissionShape.Curve)
            {
                double randomAngle = System.Math.PI * (random.NextDouble() * 2.0 - 1.0);
                return new Vector3(
                    (float)randomAngle / 3.0f * emissionSize,
                    0,
                    emissionSize * (float)System.Math.Sin(randomAngle));
            }
            else if (emissionShape == (int)EmissionShape.Ring)
            {
                double randomAngle = System.Math.PI * (random.NextDouble() * 2.0 - 1.0);
                return new Vector3(
                    emissionSize * (float)System.Math.Sin(randomAngle),
                    0,
                    emissionSize * (float)System.Math.Cos(randomAngle));
            }
            else return new Vector3(0, 0, 0);

        }
        private void _generateParticles()
        {
            if (particleTotalTime % particleEmissionSpan == 0)
            {
                for (int i = 0; i < particleEmissionNum; i++)
                {
                    double angle = System.Math.PI * (i * 6) / 180.0;
                    Particle particle = particleManager.getNext();
                    particle.Position = _getParticlePosition();
                    particle.Velocity = particleSpeed * _getParticleVelocity();
                    particle.Acceleration = _getParticleAcceleration();
                    particle.MaxAge = particleMaxAge;
                    particle.Init();
                }
            }
        }

        private void ChangeParticles()
        {


            if (Keyboard.GetState().IsKeyDown(Keys.F1) && previousKeyboardState.IsKeyDown(Keys.F1))
            {

                emissionShape = (int)EmissionShape.Square;
                emissionType = (int)EmissionType.F1;
                _generateParticles();
                particleTotalTime++;
            }
            if (!Keyboard.GetState().IsKeyDown(Keys.F1) && previousKeyboardState.IsKeyDown(Keys.F1)) particleTotalTime = 0;

            if (Keyboard.GetState().IsKeyDown(Keys.F2) && previousKeyboardState.IsKeyDown(Keys.F2))
            {
                // emissionShape = (int)EmissionShape.Ring;
                emissionShape = (int)EmissionShape.Square;
                emissionType = (int)EmissionType.F2;
                _generateParticles();
                particleTotalTime++;
            }
            if (!Keyboard.GetState().IsKeyDown(Keys.F2) && previousKeyboardState.IsKeyDown(Keys.F2)) particleTotalTime = 0;

            if (Keyboard.GetState().IsKeyDown(Keys.F3) && previousKeyboardState.IsKeyDown(Keys.F3))
            {
                // emissionShape = (int)EmissionShape.Ring;
                emissionType = (int)EmissionType.F3;
                _generateParticles();
                ++particleTotalTime;
            }
            if (!Keyboard.GetState().IsKeyDown(Keys.F3) && previousKeyboardState.IsKeyDown(Keys.F3)) particleTotalTime = 0;

            if (Keyboard.GetState().IsKeyDown(Keys.F4) && previousKeyboardState.IsKeyDown(Keys.F4))
            {
                if (emissionShape == (int)EmissionShape.Square) emissionShape = (int)EmissionShape.Curve;
                else if (emissionShape == (int)EmissionShape.Curve) emissionShape = (int)EmissionShape.Ring;
                else if (emissionShape == (int)EmissionShape.Ring) emissionShape = (int)EmissionShape.Square;
            }

            // if (IsKeyPressed(Keys.F)) ResetValues();

        }

        private void ValueControls()
        {
            if (Keyboard.GetState().IsKeyDown(Keys.N) && !Keyboard.GetState().IsKeyDown(Keys.LeftShift)) particleEmissionNum = MathHelper.Clamp(particleEmissionNum + 1, 1, 100);
            if (Keyboard.GetState().IsKeyDown(Keys.N) && Keyboard.GetState().IsKeyDown(Keys.LeftShift)) particleEmissionNum = MathHelper.Clamp(particleEmissionNum - 1, 1, 100);

            if (Keyboard.GetState().IsKeyDown(Keys.E) && !Keyboard.GetState().IsKeyDown(Keys.LeftShift)) particleEmissionSpan = MathHelper.Clamp(particleEmissionSpan + 1, 1, 100);
            if (Keyboard.GetState().IsKeyDown(Keys.E) && Keyboard.GetState().IsKeyDown(Keys.LeftShift)) particleEmissionSpan = MathHelper.Clamp(particleEmissionSpan - 1, 1, 100);

            if (Keyboard.GetState().IsKeyDown(Keys.A) && !Keyboard.GetState().IsKeyDown(Keys.LeftShift)) particleMaxAge = MathHelper.Clamp(particleMaxAge + 1, 1, 10);
            if (Keyboard.GetState().IsKeyDown(Keys.A) && Keyboard.GetState().IsKeyDown(Keys.LeftShift)) particleMaxAge = MathHelper.Clamp(particleMaxAge - 1, 1, 10);

            if (Keyboard.GetState().IsKeyDown(Keys.S) && !Keyboard.GetState().IsKeyDown(Keys.LeftShift)) emissionSize = MathHelper.Clamp(emissionSize + 0.01f, 0f, 10.0f);
            if (Keyboard.GetState().IsKeyDown(Keys.S) && Keyboard.GetState().IsKeyDown(Keys.LeftShift)) emissionSize = MathHelper.Clamp(emissionSize - 0.01f, 0f, 10.0f);

            if (Keyboard.GetState().IsKeyDown(Keys.R) && !Keyboard.GetState().IsKeyDown(Keys.LeftShift)) particleResilence = MathHelper.Clamp(particleResilence + 0.01f, 0f, 1.0f);
            if (Keyboard.GetState().IsKeyDown(Keys.R) && Keyboard.GetState().IsKeyDown(Keys.LeftShift)) particleResilence = MathHelper.Clamp(particleResilence - 0.01f, 0f, 1.0f);

            if (Keyboard.GetState().IsKeyDown(Keys.F) && !Keyboard.GetState().IsKeyDown(Keys.LeftShift)) particleFriction = MathHelper.Clamp(particleFriction + 0.01f, 0f, 1.0f);
            if (Keyboard.GetState().IsKeyDown(Keys.F) && Keyboard.GetState().IsKeyDown(Keys.LeftShift)) particleFriction = MathHelper.Clamp(particleFriction - 0.01f, 0f, 1.0f);

            if (Keyboard.GetState().IsKeyDown(Keys.W) && !Keyboard.GetState().IsKeyDown(Keys.LeftShift)) particleWind = MathHelper.Clamp(particleWind + 0.01f, 0f, 10.0f);
            if (Keyboard.GetState().IsKeyDown(Keys.W) && Keyboard.GetState().IsKeyDown(Keys.LeftShift)) particleWind = MathHelper.Clamp(particleWind - 0.01f, 0f, 10.0f);

            if (Keyboard.GetState().IsKeyDown(Keys.V) && !Keyboard.GetState().IsKeyDown(Keys.LeftShift)) particleSpeed += 0.01f;
            if (Keyboard.GetState().IsKeyDown(Keys.V) && Keyboard.GetState().IsKeyDown(Keys.LeftShift)) particleSpeed -= 0.01f;


            // Defaults
            if (IsKeyPressed(Keys.OemQuestion)) showControls = !showControls;
            if (IsKeyPressed(Keys.H)) showValues = !showValues;
            if (IsKeyPressed(Keys.F)) ResetValues();

        } // COMPLETE
        void DrawValues()
        {
            int height = 20;
            int line = 1;
            float leftMargin = 20f;

            spriteBatch.DrawString(font, "Emission Shape: " + Enum.GetName(typeof(EmissionShape), emissionShape), new Vector2(leftMargin, height * line++), Color.White);
            spriteBatch.DrawString(font, "Emission Type " + emissionType, new Vector2(leftMargin, height * line++), Color.White);
            spriteBatch.DrawString(font, "Particle Emission Num = " + particleEmissionNum, new Vector2(leftMargin, height * line++), Color.White);
            spriteBatch.DrawString(font, "Particle Emission Span = " + particleEmissionSpan, new Vector2(leftMargin, height * line++), Color.White);
            spriteBatch.DrawString(font, "Particle MaxAge = " + particleMaxAge, new Vector2(leftMargin, height * line++), Color.White);
            spriteBatch.DrawString(font, "Emission Size = " + emissionSize, new Vector2(leftMargin, height * line++), Color.White);
            spriteBatch.DrawString(font, "Particle Resilence = " + particleResilence, new Vector2(leftMargin, height * line++), Color.White);
            spriteBatch.DrawString(font, "Particle Friction = " + particleFriction, new Vector2(leftMargin, height * line++), Color.White);
            spriteBatch.DrawString(font, "Particle Wind = " + particleWind, new Vector2(leftMargin, height * line++), Color.White);
            spriteBatch.DrawString(font, "Particle Speed = " + particleSpeed, new Vector2(leftMargin, height * line++), Color.White);
            spriteBatch.DrawString(font, "Reset Values: F ", new Vector2(leftMargin, height * line++), Color.White);
            spriteBatch.DrawString(font, "P pressed: " + Keyboard.GetState().IsKeyDown(Keys.P), new Vector2(leftMargin, height * line++), Color.White);

        } // COMPLETE
        void DrawControls()
        {
            int height = 20;
            int line = 1;
            float leftMargin = _graphics.PreferredBackBufferWidth * 0.75f;

            spriteBatch.DrawString(font, "Change Particles: F1-F3 ", new Vector2(leftMargin, height * line++), Color.White);
            spriteBatch.DrawString(font, "particleEmissionNum: N/n ", new Vector2(leftMargin, height * line++), Color.White);
            spriteBatch.DrawString(font, "particleEmissionSpan: E/e", new Vector2(leftMargin, height * line++), Color.White);
            spriteBatch.DrawString(font, "particleMaxAge: A/a ", new Vector2(leftMargin, height * line++), Color.White);
            spriteBatch.DrawString(font, "emissionSize: S/s ", new Vector2(leftMargin, height * line++), Color.White);
            spriteBatch.DrawString(font, "particleResilence: R/r ", new Vector2(leftMargin, height * line++), Color.White);
            spriteBatch.DrawString(font, "particleFriction: F/f ", new Vector2(leftMargin, height * line++), Color.White);
            spriteBatch.DrawString(font, "particleWind: W/w ", new Vector2(leftMargin, height * line++), Color.White);
            spriteBatch.DrawString(font, "particleSpeed:V/v ", new Vector2(leftMargin, height * line++), Color.White);
            spriteBatch.DrawString(font, "Reset Camera/Light: Enter ", new Vector2(leftMargin, height * line++), Color.White);
        } // COMPLETE
        void ResetValues()
        {
            particleMaxNum = 10000;
            particleEmissionNum = 10;
            particleEmissionSpan = 50;
            particleTotalTime = 0;
            particleMaxAge = 1;
            particleBoundOn = 0;
            emissionSize = 1.0f;
            particleResilence = 1.0f;
            particleFriction = 1.0f;
            particleGroundY = -2.0f;
        }  // COMPLETE

    }
}