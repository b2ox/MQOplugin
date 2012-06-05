using System;
using System.Windows.Forms;
using System.ComponentModel;
using System.Threading.Tasks;
using PEPlugin;
using PEPlugin.Pmx;

namespace PMDEPlugin
{
    public class MQOImporter : IPEImportPlugin
    {
        public String Caption { get { return "Metasequoia"; } }
        public String Ext { get { return ".mqo"; } }

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
            using (FileFormat.MQOFile mqo = FileFormat.MQOFile.load(mqopath))
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
                pc = 0;
                mqo.Object.ForEach(mObj =>
                {
                    bw.ReportProgress(cw * pc++, String.Format("オブジェクト'{0}'の変換中", mObj.Name));
                    if (!mObj.Visible) return;
                    mObj.CalcNormals();
                    Decimal smoothing = mObj.SmoothingValue;
                    mObj.Face.ForEach(fc =>
                    {
                        if (fc.MatID < 0) fc.MatID = 0; // 材質割り当てのない面には最初の材質を割り当てる

                        Func<int, IPXVertex> get_vertex;
                        if (fc.normal == null)
                        {
                            get_vertex = i => getVertex(pmx, mObj, fc.VertexID[i], fc.UVID[i]);
                        }
                        else
                        {
                            get_vertex = i => getVertex(pmx, mObj, fc.VertexID[i], fc.UVID[i],
                                fc.normal.ChoiceNormal(smoothing, mObj.Normal[fc.VertexID[i]]));
                        }
                        Action<int, int, int> setFace =
                            (v0, v1, v2) =>
                            {
                                var xf = bld.Face();
                                xf.Vertex1 = get_vertex(v0);
                                xf.Vertex2 = get_vertex(v1);
                                xf.Vertex3 = get_vertex(v2);
                                pmx.Material[fc.MatID].Faces.Add(xf);
                            };
                        switch (fc.VertexID.Length)
                        {
                            case 3:
                                setFace(0, 1, 2);
                                break;

                            case 4:
                                setFace(0, 1, 2);
                                setFace(0, 2, 3);
                                break;
                        }
                    });
                });
            }
        }

        private IPXVertex getVertex(IPXPmx pmx, FileFormat.MQOObject mObj, int VertID, int uvID, FileFormat.MQOVertex norm = null)
        {
            IPXVertex v = bld.Vertex();
            FileFormat.MQOVertex mv = mObj.Vertex[VertID];
            v.Position.X = (float)(mv.X);
            v.Position.Y = (float)(mv.Y);
            v.Position.Z = -(float)(mv.Z);
            FileFormat.MQOUV muv = mObj.UV[uvID];
            v.UV.U = (float)muv.U;
            v.UV.V = (float)muv.V;
            if (norm != null)
            {
                v.Normal.X = (float)norm.X;
                v.Normal.Y = (float)norm.Y;
                v.Normal.Z = -(float)norm.Z;
            }
            if (!pmx.Vertex.Contains(v)) pmx.Vertex.Add(v);
            return v;
        }
    }
}
