using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;

namespace FileFormat
{
    /**
     * mqoファイルを読み込んでDOMなオブジェクトに格納したり
     * DOMなオブジェクトをmqo形式で書きだしたりする
     * pmxとの相互変換に必要な部分だけの手抜き実装
     * 
     * publicなものは先頭大文字、private,internalなものは先頭小文字
     * 
     * このファイルはDOMの定義部分
     */
    public partial class MQOFile : IDisposable
    {
        static internal Encoding sjis = Encoding.GetEncoding("Shift_JIS");

        public decimal Version = -1;

        public MQOScene Scene = null;
        public List<MQOBackImage> BackImage = new List<MQOBackImage>();
        public List<MQOMaterial> Material = new List<MQOMaterial>();
        public List<MQOObject> Object = new List<MQOObject>();

        #region IDisposable Support
        private bool disposedValue = false; // 重複する呼び出しを検出するには

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    // TODO: マネージ状態を破棄します (マネージ オブジェクト)。
                    Scene?.Dispose();
                    Scene = null;
                    BackImage.ForEach(b => b.Dispose());
                    BackImage = null;
                    Material.ForEach(m => m.Dispose());
                    Material = null;
                    Object.ForEach(o => o.Dispose());
                    Object = null;
                }

                // TODO: アンマネージ リソース (アンマネージ オブジェクト) を解放し、下のファイナライザーをオーバーライドします。
                // TODO: 大きなフィールドを null に設定します。

                disposedValue = true;
            }
        }

        // TODO: 上の Dispose(bool disposing) にアンマネージ リソースを解放するコードが含まれる場合にのみ、ファイナライザーをオーバーライドします。
        // ~MQOFile()
        // {
        //   // このコードを変更しないでください。クリーンアップ コードを上の Dispose(bool disposing) に記述します。
        //   Dispose(false);
        // }

        // このコードは、破棄可能なパターンを正しく実装できるように追加されました。
        public void Dispose()
        {
            // このコードを変更しないでください。クリーンアップ コードを上の Dispose(bool disposing) に記述します。
            Dispose(true);
            // TODO: 上のファイナライザーがオーバーライドされる場合は、次の行のコメントを解除してください。
            // GC.SuppressFinalize(this);
        }
        #endregion

    }

    public partial class MQOScene : IDisposable
    {
        List<MQOAttribute> Attribute = new List<MQOAttribute>();
        #region IDisposable Support
        private bool disposedValue = false; // 重複する呼び出しを検出するには

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    // TODO: マネージ状態を破棄します (マネージ オブジェクト)。
                    Attribute.Clear();
                    Attribute = null;
                }

                // TODO: アンマネージ リソース (アンマネージ オブジェクト) を解放し、下のファイナライザーをオーバーライドします。
                // TODO: 大きなフィールドを null に設定します。

                disposedValue = true;
            }
        }

        // TODO: 上の Dispose(bool disposing) にアンマネージ リソースを解放するコードが含まれる場合にのみ、ファイナライザーをオーバーライドします。
        // ~MQOScene()
        // {
        //   // このコードを変更しないでください。クリーンアップ コードを上の Dispose(bool disposing) に記述します。
        //   Dispose(false);
        // }

        // このコードは、破棄可能なパターンを正しく実装できるように追加されました。
        public void Dispose()
        {
            // このコードを変更しないでください。クリーンアップ コードを上の Dispose(bool disposing) に記述します。
            Dispose(true);
            // TODO: 上のファイナライザーがオーバーライドされる場合は、次の行のコメントを解除してください。
            // GC.SuppressFinalize(this);
        }
        #endregion
    }
    public partial class MQOBackImage : IDisposable
    {
        public string Part, Path;
        public decimal X, Y, W, H;
        public MQOBackImage(string part, string path, decimal x, decimal y, decimal w, decimal h)
        {
            Part = part;
            Path = path;
            X = x;
            Y = y;
            W = w;
            H = h;
        }

        #region IDisposable Support
        private bool disposedValue = false; // 重複する呼び出しを検出するには

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    // TODO: マネージ状態を破棄します (マネージ オブジェクト)。
                    Part = null;
                    Path = null;
                }

                // TODO: アンマネージ リソース (アンマネージ オブジェクト) を解放し、下のファイナライザーをオーバーライドします。
                // TODO: 大きなフィールドを null に設定します。

                disposedValue = true;
            }
        }

        // TODO: 上の Dispose(bool disposing) にアンマネージ リソースを解放するコードが含まれる場合にのみ、ファイナライザーをオーバーライドします。
        // ~MQOBackImage()
        // {
        //   // このコードを変更しないでください。クリーンアップ コードを上の Dispose(bool disposing) に記述します。
        //   Dispose(false);
        // }

        // このコードは、破棄可能なパターンを正しく実装できるように追加されました。
        public void Dispose()
        {
            // このコードを変更しないでください。クリーンアップ コードを上の Dispose(bool disposing) に記述します。
            Dispose(true);
            // TODO: 上のファイナライザーがオーバーライドされる場合は、次の行のコメントを解除してください。
            // GC.SuppressFinalize(this);
        }
        #endregion
    }
    public class MQOColor
    {
        public decimal R, G, B, A = 0;
        public void SetValue(int i, decimal v)
        {
            switch (i)
            {
                case 0:
                    R = v; break;
                case 1:
                    G = v; break;
                case 2:
                    B = v; break;
                case 3:
                    A = v; break;
                default:
                    throw new Exception("out of index");
            }
        }
    }
    public partial class MQOMaterial : IDisposable
    {
        public string Name;
        public string Tex, Alpha, Bump;
        public MQOColor Color;
        public decimal Diffuse, Ambient, Emission, Specular, Power = 0;
        public MQOMaterial(string name)
        {
            Name = name;
            Tex = "";
            Alpha = "";
            Bump = "";
            Color = new MQOColor();
        }

        #region IDisposable Support
        private bool disposedValue = false; // 重複する呼び出しを検出するには

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    // TODO: マネージ状態を破棄します (マネージ オブジェクト)。
                    Name = null;
                    Tex = null;
                    Alpha = null;
                    Bump = null;
                    Color = null;
                }

                // TODO: アンマネージ リソース (アンマネージ オブジェクト) を解放し、下のファイナライザーをオーバーライドします。
                // TODO: 大きなフィールドを null に設定します。

                disposedValue = true;
            }
        }

        // TODO: 上の Dispose(bool disposing) にアンマネージ リソースを解放するコードが含まれる場合にのみ、ファイナライザーをオーバーライドします。
        // ~MQOMaterial()
        // {
        //   // このコードを変更しないでください。クリーンアップ コードを上の Dispose(bool disposing) に記述します。
        //   Dispose(false);
        // }

        // このコードは、破棄可能なパターンを正しく実装できるように追加されました。
        public void Dispose()
        {
            // このコードを変更しないでください。クリーンアップ コードを上の Dispose(bool disposing) に記述します。
            Dispose(true);
            // TODO: 上のファイナライザーがオーバーライドされる場合は、次の行のコメントを解除してください。
            // GC.SuppressFinalize(this);
        }
        #endregion
    }
    public partial class MQOObject : IDisposable
    {
        public string Name;
        public List<MQOAttribute> Attribute = new List<MQOAttribute>();
        public List<MQOVertex> Vertex = new List<MQOVertex>();
        public List<MQOUV> UV = new List<MQOUV>();
        public List<MQOFace> Face = new List<MQOFace>();
        public MQOObject(string name)
        {
            Name = name;
        }
        public bool Visible
        {
            // visible属性があればその値が15かどうか、ない場合はtrue
            get => MQOAttribute.FindOrDefault(Attribute, "visible", 15) == 15;
            set
            {
                var visible = Attribute.Find(a => a.Name == "visible");
                if (visible == null)
                {
                    visible = new MQOAttribute
                    {
                        Name = "visible",
                        Values = new decimal[] { value ? 15 : 0 }
                    };
                    Attribute.Add(visible);
                }
                else
                    visible.Values[0] = value ? 15 : 0;
            }
        }
        internal int getVertexIndex(decimal x, decimal y, decimal z, bool addFlag = true)
        {
            int idx = Vertex.FindLastIndex(xyz => xyz.X == x && xyz.Y == y && xyz.Z == z);
            if (addFlag && idx < 0)
            {
                idx = Vertex.Count;
                Vertex.Add(new MQOVertex(x, y, z));
            }
            return idx;
        }
        internal int getUVIndex(decimal u, decimal v, bool addFlag = true)
        {
            int idx = UV.FindLastIndex(uv => uv.U == u && uv.V == v);
            if (addFlag && idx < 0)
            {
                idx = UV.Count;
                UV.Add(new MQOUV(u, v));
            }
            return idx;
        }

        #region IDisposable Support
        private bool disposedValue = false; // 重複する呼び出しを検出するには

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    // TODO: マネージ状態を破棄します (マネージ オブジェクト)。
                    Attribute.Clear();
                    Vertex.Clear();
                    UV.Clear();
                    Face.Clear();
                    Attribute = null;
                    Vertex = null;
                    UV = null;
                    Face = null;
                }

                // TODO: アンマネージ リソース (アンマネージ オブジェクト) を解放し、下のファイナライザーをオーバーライドします。
                // TODO: 大きなフィールドを null に設定します。

                disposedValue = true;
            }
        }

        // TODO: 上の Dispose(bool disposing) にアンマネージ リソースを解放するコードが含まれる場合にのみ、ファイナライザーをオーバーライドします。
        // ~MQOObject()
        // {
        //   // このコードを変更しないでください。クリーンアップ コードを上の Dispose(bool disposing) に記述します。
        //   Dispose(false);
        // }

        // このコードは、破棄可能なパターンを正しく実装できるように追加されました。
        public void Dispose()
        {
            // このコードを変更しないでください。クリーンアップ コードを上の Dispose(bool disposing) に記述します。
            Dispose(true);
            // TODO: 上のファイナライザーがオーバーライドされる場合は、次の行のコメントを解除してください。
            // GC.SuppressFinalize(this);
        }
        #endregion
    }
    public partial class MQOAttribute : IDisposable
    {
        public string Name = "";
        public decimal[] Values = null;

        public static decimal FindOrDefault(List<MQOAttribute> attrs, string name, decimal defVal)
        {
            var attr = attrs.FirstOrDefault(a => a.Name == name);
            return attr == null ? defVal : attr.Values[0];
        }

        #region IDisposable Support
        private bool disposedValue = false; // 重複する呼び出しを検出するには

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    // TODO: マネージ状態を破棄します (マネージ オブジェクト)。
                    Name = null;
                    Values = null;
                }

                // TODO: アンマネージ リソース (アンマネージ オブジェクト) を解放し、下のファイナライザーをオーバーライドします。
                // TODO: 大きなフィールドを null に設定します。

                disposedValue = true;
            }
        }

        // TODO: 上の Dispose(bool disposing) にアンマネージ リソースを解放するコードが含まれる場合にのみ、ファイナライザーをオーバーライドします。
        // ~MQOAttribute()
        // {
        //   // このコードを変更しないでください。クリーンアップ コードを上の Dispose(bool disposing) に記述します。
        //   Dispose(false);
        // }

        // このコードは、破棄可能なパターンを正しく実装できるように追加されました。
        public void Dispose()
        {
            // このコードを変更しないでください。クリーンアップ コードを上の Dispose(bool disposing) に記述します。
            Dispose(true);
            // TODO: 上のファイナライザーがオーバーライドされる場合は、次の行のコメントを解除してください。
            // GC.SuppressFinalize(this);
        }
        #endregion
    }
    public partial class MQOVertex
    {
        public decimal X, Y, Z = 0;
        public MQOVertex(decimal x, decimal y, decimal z)
        {
            X = x; Y = y; Z = z;
        }
        public bool Equals(MQOVertex other) => other != null && other.X == X && other.Y == Y && other.Z == Z;
    }
    public partial class MQOUV
    {
        public decimal U, V;
        public MQOUV(decimal u, decimal v)
        {
            U = u; V = v;
        }
        public bool Equals(MQOUV other)
        {
            return U == other.U && V == other.V;
        }
    }
    public partial class MQOFace : IDisposable
    {
        public int MatID;
        public int[] VertexID;
        public int[] UVID;

        #region IDisposable Support
        private bool disposedValue = false; // 重複する呼び出しを検出するには

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    // TODO: マネージ状態を破棄します (マネージ オブジェクト)。
                    VertexID = null;
                    UVID = null;
                }

                // TODO: アンマネージ リソース (アンマネージ オブジェクト) を解放し、下のファイナライザーをオーバーライドします。
                // TODO: 大きなフィールドを null に設定します。

                disposedValue = true;
            }
        }

        // TODO: 上の Dispose(bool disposing) にアンマネージ リソースを解放するコードが含まれる場合にのみ、ファイナライザーをオーバーライドします。
        // ~MQOFace()
        // {
        //   // このコードを変更しないでください。クリーンアップ コードを上の Dispose(bool disposing) に記述します。
        //   Dispose(false);
        // }

        // このコードは、破棄可能なパターンを正しく実装できるように追加されました。
        public void Dispose()
        {
            // このコードを変更しないでください。クリーンアップ コードを上の Dispose(bool disposing) に記述します。
            Dispose(true);
            // TODO: 上のファイナライザーがオーバーライドされる場合は、次の行のコメントを解除してください。
            // GC.SuppressFinalize(this);
        }
        #endregion
    }
}
