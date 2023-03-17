using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Content;
using static System.Windows.Forms.VisualStyles.VisualStyleElement;

namespace CPI411.SimpleEngine
{
    public class Skybox
    {
        private Model skyBox;
        public TextureCube skyBoxTexture;
        private Effect skyBoxEffect;
        private float size = 50f; //Can make larger if I want


        //Constructor
        public Skybox(string[] skyboxTextures, int dim, ContentManager Content, GraphicsDevice g)
        {
            //Make these arguments part of constructor in the future to make it reusible.
            skyBox = Content.Load<Model>("cube"); //"skybox/cube" use sub-folders!
            skyBoxEffect = Content.Load<Effect>("skybox");



            skyBoxTexture = new TextureCube(g, dim, false, SurfaceFormat.Color);
            byte[] data = new byte[dim * dim * 4]; //512 x 514 for the image, x4 for rgba data.

            //This assigns all 6 faces of the skybox.
            Texture2D tempTexture = Content.Load<Texture2D>(skyboxTextures[0]);
            tempTexture.GetData<byte>(data); //All pixel data is stored into byte array data
            skyBoxTexture.SetData<byte>(CubeMapFace.NegativeX, data);

            tempTexture = Content.Load<Texture2D>(skyboxTextures[1]);
            tempTexture.GetData<byte>(data);
            skyBoxTexture.SetData<byte>(CubeMapFace.PositiveX, data);

            tempTexture = Content.Load<Texture2D>(skyboxTextures[2]);
            tempTexture.GetData<byte>(data);
            skyBoxTexture.SetData<byte>(CubeMapFace.NegativeY, data);

            tempTexture = Content.Load<Texture2D>(skyboxTextures[3]);
            tempTexture.GetData<byte>(data);
            skyBoxTexture.SetData<byte>(CubeMapFace.PositiveY, data);

            tempTexture = Content.Load<Texture2D>(skyboxTextures[4]);
            tempTexture.GetData<byte>(data);
            skyBoxTexture.SetData<byte>(CubeMapFace.NegativeZ, data);

            tempTexture = Content.Load<Texture2D>(skyboxTextures[5]);
            tempTexture.GetData<byte>(data);
            skyBoxTexture.SetData<byte>(CubeMapFace.PositiveZ, data);

        }
        public void Draw(Matrix view, Matrix projection, Vector3 cameraPosition)
        {
            foreach (EffectPass pass in skyBoxEffect.CurrentTechnique.Passes)
            {
                foreach (ModelMesh mesh in skyBox.Meshes)
                {

                    foreach (ModelMeshPart part in mesh.MeshParts)
                    {
                        part.Effect = skyBoxEffect;
                        part.Effect.Parameters["World"].SetValue(
                            Matrix.CreateScale(size) *
                            Matrix.CreateTranslation(cameraPosition));

                        part.Effect.Parameters["View"].SetValue(view);
                        part.Effect.Parameters["Projection"].SetValue(projection);
                        part.Effect.Parameters["SkyBoxTexture"].SetValue(skyBoxTexture);
                        part.Effect.Parameters["CameraPosition"].SetValue(cameraPosition);
                    }
                    mesh.Draw(); //Built-in draw method for monogame.
                }
            }
        }
    }
}
    
