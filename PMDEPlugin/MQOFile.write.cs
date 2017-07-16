using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace FileFormat
{
    /**
     * MQOFileクラスのmqoファイル書き出し部分
     * 色々手抜き出力
     * 出力したmqoは必ずMetasequoiaで開いて保存し直すこと
     */
    public partial class MQOFile
    {
        public void WriteTo(String path)
        {
            using (TextWriter tw = new StreamWriter(path, false, sjis)) { WriteTo(tw); }
        }

        public void WriteTo(TextWriter tw)
        {
            // ヘッダ
            tw.WriteLine("Metasequoia Document");
            tw.WriteLine("Format Text Ver 1.0");

            // 視点情報の書き出しなどは行わない

            // 材質の書き出し
            tw.WriteLine("Material " + Material.Count + " {");
            Material.ForEach(m => tw.WriteLine("\t" + m.ToString()));
            tw.WriteLine("}");

            // オブジェクトの書き出し
            Object.ForEach(o => o.writeTo(tw));

            // フッタ
            tw.WriteLine("Eof");
        }
    }

    public partial class MQOScene : IDisposable
    {
        internal void writeTo(TextWriter tw)
        {
            tw.WriteLine("Scene {");
            Attribute.ForEach(s => tw.WriteLine("\t" + s.ToString()));
            tw.WriteLine("}");
        }
    }
    public partial class MQOBackImage : IDisposable
    {
        public override string ToString()
        {
            return "";
        }
    }
    public partial class MQOMaterial : IDisposable
    {
        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendFormat("\"{0}\"", Name);
            sb.AppendFormat(" col({0} {1} {2} {3})", Color.R, Color.G, Color.B, Color.A);
            sb.AppendFormat(" dif({0})", Diffuse);
            sb.AppendFormat(" amb({0})", Ambient);
            sb.AppendFormat(" emi({0})", Emission);
            sb.AppendFormat(" spc({0})", Specular);
            sb.AppendFormat(" power({0})", Power);
            if (Tex != "") sb.AppendFormat(" tex(\"{0}\")", Tex);
            if (Alpha != "") sb.AppendFormat(" aplane(\"{0}\")", Alpha);
            if (Bump != "") sb.AppendFormat(" bump(\"{0}\")", Bump);
            return sb.ToString();
        }
    }
    public partial class MQOObject : IDisposable
    {
        internal void writeTo(TextWriter tw)
        {
            tw.WriteLine("Object \"" + Name + "\" {");

            Attribute.ForEach(s => tw.WriteLine("\t" + s.ToString()));

            tw.WriteLine("\tvertex " + Vertex.Count + " {");
            Vertex.ForEach(v => tw.WriteLine("\t\t" + v.ToString()));
            tw.WriteLine("\t}");

            tw.WriteLine("\tface " + Face.Count + " {");
            Face.ForEach(f => tw.WriteLine("\t\t" + f.toString(this)));
            tw.WriteLine("\t}");

            tw.WriteLine("}");
        }
    }
    public partial class MQOAttribute
    {
        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendFormat("{0}", Name);
            foreach (var v in Values) sb.AppendFormat(" {0}", v);
            return sb.ToString();
        }
    }
    public partial class MQOFace : IDisposable
    {
        internal string toString(MQOObject mqoObj)
        {
            StringBuilder sb = new StringBuilder();
            sb.Append(VertexID.Length);
            sb.Append(" V(");
            for (int i = 0; i < VertexID.Length; i++) sb.AppendFormat(i > 0 ? " {0}" : "{0}", VertexID[i]);
            sb.AppendFormat(") M({0})", MatID);
            if (UVID.Length == VertexID.Length)
            {
                sb.Append(" UV(");
                for (int i = 0; i < UVID.Length; i++) sb.AppendFormat(i > 0 ? " {0}" : "{0}", mqoObj.UV[UVID[i]]);
                sb.Append(")");
            }
            return sb.ToString();
        }
    }

    public partial class MQOVertex
    {
        public override string ToString()
        {
            return String.Format("{0} {1} {2}", X, Y, Z);
        }
    }

    public partial class MQOUV
    {
        public override string ToString()
        {
            return String.Format("{0} {1}", U, V);
        }
    }
}
