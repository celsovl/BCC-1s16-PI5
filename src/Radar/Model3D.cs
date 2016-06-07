using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Text;
using OpenTK.Graphics.OpenGL;

namespace Radar
{
    public class Model3D
    {
        public static Model3D FromFile(string path)
        {
            return new ObjSerializer().Deserialize(new StreamReader(path));
        }

        public List<float[]> Vectors = new List<float[]>();
        public List<float[]> VectorsTextureCoord = new List<float[]>();
        public List<float[]> Normals = new List<float[]>();
        public List<int[]> Faces = new List<int[]>();

        public float[] Scale = new float[] { 1, 1, 1 };
        public float[] Rotate = new float[] { 0, 0, 0 };
        public float[] Translate = new float[] { 0, 0, 0 };
        public Model3DTexture2D Texture;

        public void AddVector(float p1, float p2, float p3)
        {
            Vectors.Add(new float[] { p1, p3, p2 });
        }

        public void AddTextureCoord(float p1, float p2)
        {
            VectorsTextureCoord.Add(new float[] { p1, p2 });
        }

        public void AddTextureCoord(float p1, float p2, float p3)
        {
            VectorsTextureCoord.Add(new float[] { p1, p2, p3 });
        }

        public int AddNormal(float p1, float p2, float p3)
        {
            Normals.Add(new float[] { p1, p3, p2 });
            return Normals.Count;
        }

        public void Draw()
        {
            GL.PushMatrix();

            GL.Translate(Translate[0], Translate[1], Translate[2]);
            GL.Scale(Scale[0], Scale[1], Scale[2]);
            GL.Rotate(Rotate[0], 1, 0, 0);
            GL.Rotate(Rotate[1], 0, 1, 0);
            GL.Rotate(Rotate[2], 0, 0, 1);

            if (Texture != null)
                Texture.Bind();

            foreach (int[] face in Faces)
            {
                GL.Begin(PrimitiveType.Polygon);

                GL.Color3(Color.Gray);
                if (face[2] > -1)
                    GL.Normal3(Normals[face[2] - 1]);
                if (face[1] > -1 && Texture != null)
                    GL.TexCoord2(VectorsTextureCoord[face[1] - 1]);
                GL.Vertex3(Vectors[face[0] - 1]);

                if (face[5] > -1)
                    GL.Normal3(Normals[face[5] - 1]);
                if (face[4] > -1 && Texture != null)
                    GL.TexCoord2(VectorsTextureCoord[face[4] - 1]);
                GL.Vertex3(Vectors[face[3] - 1]);

                if (face[8] > -1)
                    GL.Normal3(Normals[face[8] - 1]);
                if (face[7] > -1 && Texture != null)
                    GL.TexCoord2(VectorsTextureCoord[face[7] - 1]);
                GL.Vertex3(Vectors[face[6] - 1]);

                if (face.Length > 9)
                {
                    if (face[11] > -1)
                        GL.Normal3(Normals[face[11] - 1]);
                    if (face[10] > -1 && Texture != null)
                        GL.TexCoord2(VectorsTextureCoord[face[10] - 1]);
                    GL.Vertex3(Vectors[face[9] - 1]);
                }

                GL.End();
            }

            GL.PopMatrix();
        }

        public void AddFace(int p1, int p2, int p3, int p4, int p5, int p6, int p7, int p8, int p9)
        {
            if (p3 == -1)
            {
                float[] normals = CalculateNormals(p1, p4, p7);
                p3 = p6 = p9 = AddNormal(normals[0], normals[1], normals[2]);
            }
            Faces.Add(new int[] { p1, p2, p3, p4, p5, p6, p7, p8, p9 });
        }

        public void AddFace(int p1, int p2, int p3, int p4, int p5, int p6, int p7, int p8, int p9, int p10, int p11, int p12)
        {
            if (p3 == -1)
            {
                float[] normals = CalculateNormals(p1, p4, p7);
                p3 = p6 = p9 = p12 = AddNormal(normals[0], normals[1], normals[2]);
            }
            Faces.Add(new int[] { p1, p2, p3, p4, p5, p6, p7, p8, p9, p10, p11, p12 });
        }

        private float[] CalculateNormals(int p1, int p4, int p7)
        {
            float[] t1 = Vectors[p1-1];
            float[] t2 = Vectors[p4-1];
            float[] t3 = Vectors[p7-1];

            float[] u = new float[] {
                t1[0] - t2[0],
                t1[1] - t2[1],
                t1[2] - t2[2]
            };

            float[] v = new float[] {
                t3[0] - t2[0],
                t3[1] - t2[1],
                t3[2] - t2[2]
            };

            return new float[] {
                u[1] * v[2] - u[2] * v[1],
                u[2] * v[0] - u[0] * v[2],
                u[0] * v[1] - u[1] * v[0]
            };
        }

        public void NormalizeVectors()
        {
            float mp = float.MinValue;
            foreach (float[] v in Vectors)
            {
                float p = (float)Math.Sqrt(v[0] * v[0] + v[1] * v[1] + v[2] * v[2]);
                if (p > mp)
                    mp = p;
            }

            foreach (float[] v in Vectors)
            {
                v[0] = v[0] / mp;
                v[1] = v[1] / mp;
                v[2] = v[2] / mp;
            }
        }
    }

    public class Model3DTexture2D : IDisposable
    {
        public int TextureID;

        public Model3DTexture2D(string filename)
        {
            if (String.IsNullOrEmpty(filename))
                throw new ArgumentException(filename);

            this.TextureID = GL.GenTexture();
            GL.BindTexture(TextureTarget.Texture2D, this.TextureID);

            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.Linear);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Linear);

            using (Bitmap bmp = new Bitmap(filename))
            {
                BitmapData bmp_data = bmp.LockBits(new Rectangle(0, 0, bmp.Width, bmp.Height), ImageLockMode.ReadOnly, System.Drawing.Imaging.PixelFormat.Format32bppArgb);

                GL.TexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.Rgba, bmp_data.Width, bmp_data.Height, 0,
                    OpenTK.Graphics.OpenGL.PixelFormat.Bgra, PixelType.UnsignedByte, bmp_data.Scan0);

                bmp.UnlockBits(bmp_data);
            }
        }

        public void Dispose()
        {
        }

        public void Bind()
        {
            GL.BindTexture(TextureTarget.Texture2D, this.TextureID);
        }
    }
}
