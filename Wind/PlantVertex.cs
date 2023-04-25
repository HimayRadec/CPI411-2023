using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace VegetationWindSample
{
    struct PlantVertex : IVertexType
    {
        public PlantVertex(Vector3 position, Vector3 normal, Vector2 texCoord, Color color)
        {
            Position = position;
            Normal = normal;
            TexCoord = texCoord;
            Color = color;
        }

        public Vector3 Position;
        public Vector3 Normal;
        public Vector2 TexCoord;
        public Color Color;

        public static readonly VertexElement[] VertexElements = new VertexElement[]
        {
            new VertexElement(0, VertexElementFormat.Vector3, VertexElementUsage.Position, 0),
            new VertexElement(12, VertexElementFormat.Vector3, VertexElementUsage.Normal, 0),
            new VertexElement(24, VertexElementFormat.Vector2, VertexElementUsage.TextureCoordinate, 0),
            new VertexElement(32, VertexElementFormat.Color, VertexElementUsage.Color, 0),
        };

        private static VertexDeclaration vertexDeclaration = new VertexDeclaration(VertexElements);
        public VertexDeclaration VertexDeclaration
        {
            get { return vertexDeclaration; }
        }
    }

    /*
    struct PlantVertexOrig : IVertexType
    {
        public Vector3 Position;
        public Vector3 Normal;
        public Vector2 TexCoord;
        public Color Color;
        public Vector3 Tangent;
        public Vector3 Binormal;

        public static readonly VertexElement[] VertexElements = new VertexElement[]
        {
            new VertexElement(0, VertexElementFormat.Vector3, VertexElementUsage.Position, 0),
            new VertexElement(12, VertexElementFormat.Vector3, VertexElementUsage.Normal, 0),
            new VertexElement(24, VertexElementFormat.Vector2, VertexElementUsage.TextureCoordinate, 0),
            new VertexElement(32, VertexElementFormat.Color, VertexElementUsage.Color, 0),
            new VertexElement(36, VertexElementFormat.Vector3, VertexElementUsage.Tangent, 0),
            new VertexElement(48, VertexElementFormat.Vector3, VertexElementUsage.Binormal, 0),
        };

        private static VertexDeclaration vertexDeclaration = new VertexDeclaration(VertexElements);
        public VertexDeclaration VertexDeclaration
        {
            get { return vertexDeclaration; }
        }
    }*/

}
