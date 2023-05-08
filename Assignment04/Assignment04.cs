using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using CPI411.SimpleEngine;
using System.Diagnostics;
using System;

namespace Assignment04
{
    public class Assignment04 : Game
    {
        private GraphicsDeviceManager graphics;
        private SpriteBatch spriteBatch;
        SpriteFont font;


        // Defaults 
        Effect effect;

        Matrix world = Matrix.Identity;
        Matrix view = Matrix.CreateLookAt(new Vector3(20, 0, 0), new Vector3(0, 0, 0), Vector3.UnitY);
        Matrix projection = Matrix.CreatePerspectiveFieldOfView(
            MathHelper.ToRadians(45),
            800f / 600f,
            0.01f,
            10000f);
        Matrix faceCamera;
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

        bool showControls = true;
        bool showValues = true;

        MouseState preMouse;
        KeyboardState previousKeyboardState;
        KeyboardState currentKeyboardState;

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
            Square, // 0
            Curve, // 1
            Ring // 2
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

        string emissionTypeString;

        private bool IsKeyPressed(Keys key)
        {
            return (!previousKeyboardState.IsKeyDown(key) && currentKeyboardState.IsKeyDown(key));
        }
        public Assignment04()
        {
            graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";
            IsMouseVisible = true;
            graphics.GraphicsProfile = GraphicsProfile.HiDef;
            //graphics.PreferredBackBufferWidth = GraphicsAdapter.DefaultAdapter.CurrentDisplayMode.Width; // Set the width of the window to the user's screen width
            //graphics.PreferredBackBufferHeight = GraphicsAdapter.DefaultAdapter.CurrentDisplayMode.Height; // Set the height of the window to the user's screen height

        }
        protected override void Initialize()
        {
            // TODO: Add your initialization logic here

            base.Initialize();
        }
        protected override void LoadContent()
        {
            spriteBatch = new SpriteBatch(GraphicsDevice);
            font = Content.Load<SpriteFont>("Font");
            effect = Content.Load<Effect>("ParticleShader");
            texture = Content.Load<Texture2D>("fire");
            model = Content.Load<Model>("Plane");
            angle = angle2 = 0;
            distance = 10;
            random = new System.Random();
            
            particleManager = new ParticleManager(GraphicsDevice, particleMaxNum);// particleManager = new ParticleManager(GraphicsDevice, particleMaxNum);

            particlePosition = new Vector3(0,0,0);

            emissionTypeString = "Fountain Basic";
        }
        protected override void Update(GameTime gameTime)
        {
            if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed || Keyboard.GetState().IsKeyDown(Keys.Escape))
                Exit();

            // Update keyboard state
            previousKeyboardState = currentKeyboardState;
            currentKeyboardState = Keyboard.GetState();

            // Toggle Draw Value/Controls
            if (IsKeyPressed(Keys.H)) showValues = !showValues;
            if (IsKeyPressed(Keys.OemQuestion)) showControls = !showControls;

            // TODO: Add your update logic here
            CameraControls();
            LightControls();
            ValueControls();
            ChangeEmissionTexture();
            ChangeEmissionType(); 
            if (Keyboard.GetState().IsKeyDown(Keys.P)) GenerateParticle();

            if (emissionType == (int)EmissionType.F3)
            {
                particleManager.Bounce(particleGroundY, particleResilence, particleFriction);
            }

            particleManager.Update(gameTime.ElapsedGameTime.Milliseconds * 0.001f);
            base.Update(gameTime);
        }
        protected override void Draw(GameTime gameTime)
        {

            GraphicsDevice.Clear(Color.CornflowerBlue);
            GraphicsDevice.BlendState = BlendState.AlphaBlend;
            GraphicsDevice.DepthStencilState = new DepthStencilState();
            GraphicsDevice.DepthStencilState = DepthStencilState.DepthRead;
            GraphicsDevice.RasterizerState = RasterizerState.CullNone;
            // TODO: Add your drawing code here

            model.Draw(world * Matrix.CreateTranslation(0, particleGroundY, 0), view, projection);

            effect.CurrentTechnique = effect.Techniques[0];
            effect.CurrentTechnique.Passes[0].Apply();
            effect.Parameters["World"].SetValue(Matrix.Identity);
            effect.Parameters["Texture"].SetValue(texture);
            effect.Parameters["View"].SetValue(view);
            effect.Parameters["Projection"].SetValue(projection);
            effect.Parameters["InverseCamera"].SetValue(Matrix.CreateRotationX(angle2) * Matrix.CreateRotationY(angle));
            particleManager.Draw(GraphicsDevice);
            GraphicsDevice.RasterizerState = RasterizerState.CullNone;

            spriteBatch.Begin();
            if (showControls) DrawControls();
            if (showValues) DrawValues();
            spriteBatch.End();


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
            else return new Vector3(0,0,0);

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
        private void ChangeEmissionType()
        {
            if (Keyboard.GetState().IsKeyDown(Keys.F1) && previousKeyboardState.IsKeyDown(Keys.F1)) 
            {
                emissionTypeString = "Fountain Basic";

                emissionShape = (int)EmissionShape.Square;
                emissionType = (int)EmissionType.F1;
                _generateParticles();
                particleTotalTime++;
            }
            if (!Keyboard.GetState().IsKeyDown(Keys.F1) && previousKeyboardState.IsKeyDown(Keys.F1)) particleTotalTime = 0;

            if (Keyboard.GetState().IsKeyDown(Keys.F2) && previousKeyboardState.IsKeyDown(Keys.F2))
            {
                emissionTypeString = "Fountain Medium";

                emissionShape = (int)EmissionShape.Square;
                emissionType = (int)EmissionType.F2;
               // _generateParticles();
               // particleTotalTime++;
            }  
            if (!Keyboard.GetState().IsKeyDown(Keys.F2) && previousKeyboardState.IsKeyDown(Keys.F2)) particleTotalTime = 0;

            if (Keyboard.GetState().IsKeyDown(Keys.F3) && previousKeyboardState.IsKeyDown(Keys.F3))
            {
                emissionTypeString = "Fountain Advanced";

                emissionType = (int) EmissionType.F3;
               // _generateParticles();
                //++particleTotalTime;
            }
            if (!Keyboard.GetState().IsKeyDown(Keys.F3) && previousKeyboardState.IsKeyDown(Keys.F3)) particleTotalTime = 0;

            if (IsKeyPressed(Keys.F4))
            {

                if (emissionShape == (int)EmissionShape.Square) emissionShape = (int)EmissionShape.Curve;
                else if (emissionShape == (int)EmissionShape.Curve) emissionShape = (int)EmissionShape.Ring;
                else if (emissionShape == (int)EmissionShape.Ring) emissionShape = (int)EmissionShape.Square;
            }

            if (IsKeyPressed(Keys.F)) ResetValues();

        }
        private void ChangeEmissionTexture()
        {
            if (IsKeyPressed(Keys.D1)) texture = null;
            if (IsKeyPressed(Keys.D2)) texture = Content.Load<Texture2D>("smoke");
            if (IsKeyPressed(Keys.D3)) texture = Content.Load<Texture2D>("water");
            if (IsKeyPressed(Keys.D4)) texture = Content.Load<Texture2D>("fire");
        }
        private void GenerateParticle()
        {
            _generateParticles();
            ++particleTotalTime;
        }
        private void ValueControls()
        {
            if (Keyboard.GetState().IsKeyDown(Keys.N) && !Keyboard.GetState().IsKeyDown(Keys.LeftShift)) particleEmissionNum = MathHelper.Clamp(particleEmissionNum + 1, 1, 100)  ;
            if (Keyboard.GetState().IsKeyDown(Keys.N) && Keyboard.GetState().IsKeyDown(Keys.LeftShift)) particleEmissionNum = MathHelper.Clamp(particleEmissionNum - 1, 1, 100)  ;

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

            if (Keyboard.GetState().IsKeyDown(Keys.V) && !Keyboard.GetState().IsKeyDown(Keys.LeftShift)) particleSpeed += 0.01f ;
            if (Keyboard.GetState().IsKeyDown(Keys.V) && Keyboard.GetState().IsKeyDown(Keys.LeftShift)) particleSpeed -= 0.01f;


            // Defaults
            if (IsKeyPressed(Keys.OemQuestion)) showControls = !showControls;
            if (IsKeyPressed(Keys.H)) showValues = !showValues;
            if (IsKeyPressed(Keys.R)) ResetValues();

        } // COMPLETE
        void DrawValues()
        {
            int height = 20;
            int line = 1;
            float leftMargin = 20f;

            spriteBatch.DrawString(font, "Emission Shape = " + Enum.GetName(typeof(EmissionShape), emissionShape), new Vector2(leftMargin, height * line++), Color.White);
            spriteBatch.DrawString(font, "Emission Type = " + emissionTypeString, new Vector2(leftMargin, height * line++), Color.White);
            spriteBatch.DrawString(font, "Particle Texture = " + texture, new Vector2(leftMargin, height * line++), Color.White);
            spriteBatch.DrawString(font, "Particle Emission Num = " + particleEmissionNum, new Vector2(leftMargin, height * line++), Color.White);
            spriteBatch.DrawString(font, "Particle Emission Span = " + particleEmissionSpan, new Vector2(leftMargin, height * line++), Color.White);
            spriteBatch.DrawString(font, "Particle MaxAge = " + particleMaxAge, new Vector2(leftMargin, height * line++), Color.White);
            spriteBatch.DrawString(font, "Emission Size = " + emissionSize, new Vector2(leftMargin, height * line++), Color.White);
            spriteBatch.DrawString(font, "Particle Resilence = " + particleResilence, new Vector2(leftMargin, height * line++), Color.White);
            spriteBatch.DrawString(font, "Particle Friction = " + particleFriction, new Vector2(leftMargin, height * line++), Color.White);
            spriteBatch.DrawString(font, "Particle Wind = " + particleWind, new Vector2(leftMargin, height * line++), Color.White);
            spriteBatch.DrawString(font, "Particle Speed = " + particleSpeed, new Vector2(leftMargin, height * line++), Color.White);
            spriteBatch.DrawString(font, "Reset Values: R ", new Vector2(leftMargin, height * line++), Color.White);

        } // COMPLETE
        void DrawControls()
        {
            int height = 20;
            int line = 1;
            float leftMargin = graphics.PreferredBackBufferWidth * 0.75f;
            spriteBatch.DrawString(font, "Emission Shape: F4 ", new Vector2(leftMargin, height * line++), Color.White);
            spriteBatch.DrawString(font, "Emission Type: F1-F3 ", new Vector2(leftMargin, height * line++), Color.White); 
            spriteBatch.DrawString(font, "Particle Texture: 1-4 ", new Vector2(leftMargin, height * line++), Color.White); 
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
            particleWind = 0f;
            particleSpeed = 1f;
        }  // COMPLETE
    }
}