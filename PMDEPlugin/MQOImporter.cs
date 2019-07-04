using System;
using System.Collections.Generic;
using System.Windows.Forms;
using System.ComponentModel;
using System.Threading.Tasks;
using PEPlugin;
using PEPlugin.Pmx;

namespace PMDEPlugin
{
    public class MQOImporter : IPEImportPlugin
    {
        public string Caption => "Metasequoia";
        public string Ext => ".mqo";

        private IPXPmxBuilder bld;
        private IPXPmx pmx = null;
        private string mqopath = null;
        public IPXPmx Import(string mqopath, IPERunArgs args)
        {
            this.mqopath = mqopath;

            using (ProgressDialog pd = new ProgressDialog("MQOImporter", new DoWorkEventHandler(ProgressDialog_DoWork)))
            {
                pd.CancelButtonEnabled = false;
                switch (pd.ShowDialog())
                {
                    case DialogResult.Abort:
                        //エラー情報を取得する
                        Exception ex = pd.Error;
                        MessageBox.Show(ex.Message, "MQOImporter: Error!!", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        break;
                    case DialogResult.Cancel:
                        MessageBox.Show("キャンセルされました。", "MQOImporter", MessageBoxButtons.OK, MessageBoxIcon.Stop);
                        break;
                    case DialogResult.OK:
                        //MessageBox.Show("読み込みが完了しました。", "MQOImporter", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        break;
                    default:
                        break;
                }
            }
            return pmx;
        }

        //DoWorkイベントハンドラ
        private void ProgressDialog_DoWork(object sender, DoWorkEventArgs e)
        {
            BackgroundWorker bw = (BackgroundWorker)sender;

            bw.ReportProgress(0, "ファイル読み込み中(バーは動きません (^-^;)");
            using (FileFormat.MQOFile mqo = FileFormat.MQOFile.load(mqopath, true)) // 三角面化して読み込む
            {
                if (mqo == null) throw new Exception("読み込み失敗。おそらくmqoファイルの構文エラー。");

                if (mqo.Object.Count == 0) throw new Exception("オブジェクトが空です。");

                // pmx作成
                bld = PEStaticBuilder.Pmx;
                pmx = bld.Pmx();
                pmx.Clear();

                // モデル名は最初のオブジェクト名を利用する
                pmx.ModelInfo.ModelName = mqo.Object[0].Name;

                // 材質
                int mc = mqo.Material.Count;
                if (mc == 0) throw new Exception("材質がありません。少なくとも1つ材質が必要です。");

                int cw = 100 / mc;
                int pc = 0;
                mqo.Material.ForEach(m =>
                {
                    bw.ReportProgress(cw * pc++, "材質の変換中");
                    IPXMaterial pm = bld.Material();
                    pm.Name = m.Name;
                    pm.Diffuse.R = (float)(m.Color.R * m.Diffuse);
                    pm.Diffuse.G = (float)(m.Color.G * m.Diffuse);
                    pm.Diffuse.B = (float)(m.Color.B * m.Diffuse);
                    pm.Diffuse.A = (float)m.Color.A;
                    pm.Ambient.R = (float)(m.Color.R * m.Ambient);
                    pm.Ambient.G = (float)(m.Color.G * m.Ambient);
                    pm.Ambient.B = (float)(m.Color.B * m.Ambient);
                    pm.Specular.R = (float)(m.Color.R * m.Specular);
                    pm.Specular.G = (float)(m.Color.G * m.Specular);
                    pm.Specular.B = (float)(m.Color.B * m.Specular);
                    pm.Power = (float)m.Power;
                    pm.Tex = m.Tex;
                    pmx.Material.Add(pm);
                });

                // 各オブジェクトを処理
                // ただし、非表示オブジェクトはスキップ
                mc = mqo.Object.Count;
                cw = 100 / mc;
                bw.ReportProgress(0, "法線を計算中");
                Parallel.ForEach(mqo.Object, mObj =>
                {
                    if (mObj.Visible) mObj.CalcNormals();
                    bw.ReportProgress(cw, 1);
                });

                // 先に頂点をすべて登録してから面を登録する
                // 頂点登録と面登録を交互に行うととんでもなく遅くなる
                mc = mqo.Material.Count;
                WorkFaceList workfacelist = new WorkFaceList(mc);
                WorkVertexDict workvertexdict = new WorkVertexDict();

                mc = mqo.Object.Count;
                cw = 100 / mc;
                pc = 0;
                for (int objID=0; objID<mc; objID++)
                {
                    var mObj = mqo.Object[objID];
                    bw.ReportProgress(cw * pc++, string.Format("'{0}'の変換中", mObj.Name));
                    mObj.Face.ForEach(fc =>
                    {
                        if (!mObj.Visible) return; // 非表示オブジェクトは無視

                        // 材質割り当てのない面は材質0として処理
                        int matID = fc.MatID < 0 ? 0 : fc.MatID;

                        int get_vertex(int i) => workvertexdict.RegistVertex(objID, fc.VertexID[i], fc.UVID[i], fc.NormalID[i]);
                        workfacelist.AddFace(matID, get_vertex(0), get_vertex(1), get_vertex(2));
                    });
                }

                workvertexdict.RegistToPmx(pmx, bld, mqo, bw);
                workfacelist.RegistToPmx(pmx, bld, workvertexdict, bw);
            }
        }
    }
    public class WorkVertexDict
    {
        internal class WorkVertex
        {
            public int ObjID, VertID, UvID, NormID;
            public IPXVertex Vertex;
            public WorkVertex(int objID, int vertID, int uvID, int normID)
            {
                ObjID = objID; VertID = vertID; UvID = uvID; NormID = normID;
            }
            public bool Eql(int objID, int vertID, int uvID, int normID)
            {
                return ObjID == objID && VertID == vertID && UvID == uvID && NormID == normID;
            }
        }
        private readonly List<WorkVertex> dict = new List<WorkVertex>();
        public WorkVertexDict() { }
        public int RegistVertex(int objID, int vertID, int uvID, int normID)
        {
            int n = dict.FindLastIndex(wv => wv.Eql(objID, vertID, uvID, normID));
            if (n < 0)
            {
                n = dict.Count;
                dict.Add(new WorkVertex(objID, vertID, uvID, normID));
            }
            return n;
        }
        public void RegistToPmx(IPXPmx pmx, IPXPmxBuilder bld, FileFormat.MQOFile mqo, BackgroundWorker bw)
        {
            int N = dict.Count;
            for(int i=0; i<N; i++)
            {
                bw.ReportProgress(100 * i / N, "頂点の登録中");
                var wv = dict[i];
                IPXVertex v = bld.Vertex();
                var mObj = mqo.Object[wv.ObjID];
                var mv = mObj.Vertex[wv.VertID];
                v.Position.X = (float)(mv.X);
                v.Position.Y = (float)(mv.Y);
                v.Position.Z = -(float)(mv.Z);
                FileFormat.MQOUV muv = mObj.UV[wv.UvID];
                v.UV.U = (float)muv.U;
                v.UV.V = (float)muv.V;
                v.Normal.X = (float)mObj.Normal[wv.NormID].X;
                v.Normal.Y = (float)mObj.Normal[wv.NormID].Y;
                v.Normal.Z = -(float)mObj.Normal[wv.NormID].Z;
                wv.Vertex = v;
                pmx.Vertex.Add(v);
            }
        }
        public IPXVertex GetVertex(int i)
        {
            return dict[i].Vertex;
        }
    }
    public class WorkFaceList
    {
        internal class WorkFace
        {
            public int V0, V1, V2;
            public WorkFace(int v0, int v1, int v2)
            {
                V0 = v0; V1 = v1; V2 = v2;
            }
        }
        private readonly List<WorkFace>[] list;
        public WorkFaceList(int matCount)
        {
            list = new List<WorkFace>[matCount];
            for (int i = 0; i < matCount; i++) list[i] = new List<WorkFace>();
        }
        public void AddFace(int matID, int v0, int v1, int v2)
        {
            list[matID].Add(new WorkFace(v0, v1, v2));
        }
        public void RegistToPmx(IPXPmx pmx, IPXPmxBuilder bld, WorkVertexDict dict, BackgroundWorker bw)
        {
            int N = list.Length;
            for (int i = 0; i < N; i++)
            {
                bw.ReportProgress(100 * i / N, "面の登録中");
                list[i].ForEach(f =>
                {
                    var xf = bld.Face();
                    xf.Vertex1 = dict.GetVertex(f.V0);
                    xf.Vertex2 = dict.GetVertex(f.V1);
                    xf.Vertex3 = dict.GetVertex(f.V2);
                    pmx.Material[i].Faces.Add(xf);
                });
            }
        }
    }
}
