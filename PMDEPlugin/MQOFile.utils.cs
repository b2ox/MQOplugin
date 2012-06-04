using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace FileFormat
{
    /**
     * 名前重複修正や法線計算など
     * 読み書き自体には不要だが別形式との変換などで必要な処理
     */
    public partial class MQOFile
    {
        // 材質名やオブジェクト名の重複を修正
        public void FixNames()
        {
            int i, j, n;
            string s;
            bool retry;
            n = Material.Count;
            for (i = 1; i < n; i++)
            {
                retry = false;
                s = Material[i].Name;
                for (j = 0; j < i; j++)
                {
                    if (s == Material[j].Name)
                    {
                        Material[i].Name += "*";
                        retry = true;
                        break;
                    }
                }
                if (retry) i -= 1;
            }
            n = Object.Count;
            for (i = 1; i < n; i++)
            {
                retry = false;
                s = Object[i].Name;
                for (j = 0; j < i; j++)
                {
                    if (s == Object[j].Name)
                    {
                        Object[i].Name += "*";
                        retry = true;
                        break;
                    }
                }
                if (retry) i -= 1;
            }
        }

        public void CalcNormals()
        {
            Parallel.ForEach(Object, o => o.CalcNormals());
        }
    }

    public partial class MQOObject : IDisposable
    {
        public Decimal SmoothingValue
        {
            get
            {
                Decimal facet;
                try
                {
                    facet = Attribute.Find(a => a.Name == "facet").Values[0];
                }
                catch
                {
                    facet = 59.5M; // Metasequoiaのデフォルト値
                }
                // "スムージングを適用"にチェックがない場合はshading=0で、facet=0
                try
                {
                    if (Attribute.Find(a => a.Name == "shading").Values[0] == 0) facet = 0;
                }
                catch
                {
                }
                return (Decimal)Math.Cos((double) facet * Math.PI / 180);
            }
        }
        public MQOVertex[] Normal = null;
        public void CalcNormals()
        {
            // 仮頂点法線を初期化
            Normal = new MQOVertex[Vertex.Count];
            Parallel.For(0,Vertex.Count, i => Normal[i] = new MQOVertex(0, 0, 0));
            // 面法線を計算し仮頂点法線に加算
            Parallel.ForEach(Face, f =>
            {
                f.calcNormal(Vertex);
                if (f.normal != null) Parallel.ForEach(f.VertexID, i => Normal[i].AddEq(f.normal));
            });
            // 仮頂点法線を正規化
            Parallel.ForEach(Normal, n => n.Normalize());
        }
    }
    public partial class MQOFace : IDisposable
    {
        internal MQOVertex normal = null;
        internal void calcNormal(List<MQOVertex> vertex)
        {
            MQOVertex v1, v2, v3;
            switch (VertexID.Length)
            {
                case 3:
                    v1 = vertex[VertexID[1]].Sub(vertex[VertexID[0]]);
                    v2 = vertex[VertexID[2]].Sub(vertex[VertexID[0]]);
                    normal = v2.Cross(v1).Normalize();
                    break;
                case 4:
                    v1 = vertex[VertexID[1]].Sub(vertex[VertexID[0]]);
                    v2 = vertex[VertexID[2]].Sub(vertex[VertexID[0]]);
                    v3 = vertex[VertexID[3]].Sub(vertex[VertexID[0]]);
                    normal = v2.Cross(v1).Normalize();
                    try
                    {
                        normal.AddEq(v3.Cross(v2).Normalize());
                        normal = normal.Normalize();
                    }
                    catch
                    {
                        normal = null; // 法線がおかしな向きになる時はnullとする
                    }
                    break;
                default:
                    break;
            }
        }
    }
    public partial class MQOVertex
    {
        // 各種ベクトル演算
        public MQOVertex Add(MQOVertex v)
        {
            return new MQOVertex(X + v.X, Y + v.Y, Z + v.Z);
        }
        public MQOVertex Sub(MQOVertex v)
        {
            return new MQOVertex(X - v.X, Y - v.Y, Z - v.Z);
        }
        public MQOVertex AddEq(MQOVertex v)
        {
            X += v.X;
            Y += v.Y;
            Z += v.Z;
            return this;
        }
        public MQOVertex SubEq(MQOVertex v)
        {
            X -= v.X;
            Y -= v.Y;
            Z -= v.Z;
            return this;
        }
        public Decimal Length()
        {
            return (Decimal)Math.Sqrt((double)(X * X + Y * Y + Z * Z));
        }
        public MQOVertex GetNormal()
        {
            Decimal s = Length();
            if (s == 0) return null;
            return new MQOVertex(X / s, Y / s, Z / s);
        }
        public MQOVertex Normalize()
        {
            Decimal s = Length();
            if (s == 0) return null;
            X /= s;
            Y /= s;
            Z /= s;
            return this;
        }
        public Decimal Product(MQOVertex v)
        {
            return X * v.X + Y * v.Y + Z * v.Z;
        }
        public MQOVertex Cross(MQOVertex v)
        {
            return new MQOVertex(
                Y * v.Z - Z * v.Y,
                Z * v.X - X * v.Z,
                X * v.Y - Y * v.X);
        }

        // スムージング角(実際はそれのcos値)によりthis(面法線)かvertNorm(頂点法線)を返す
        public MQOVertex ChoiceNormal(Decimal smoothing, MQOVertex vertexNorm)
        {
            return (this.Product(vertexNorm) < smoothing) ? this : vertexNorm;
        }
    }
}
