using System;
using System.Windows.Forms;
using System.ComponentModel;
using System.Threading.Tasks;
using PEPlugin;
using PEPlugin.Pmx;

namespace PMDEPlugin
{
    public class MQOExporter : IPEExportPlugin
    {
        public string Caption => "Metasequoia";
        public string Ext => ".mqo";

        private IPXPmx pmx = null;
        private string mqopath = null;
        public void Export(IPXPmx pmx, string path, IPERunArgs args)
        {
            this.pmx = pmx;
            this.mqopath = path;

            using (ProgressDialog pd = new ProgressDialog("MQOExporter", new DoWorkEventHandler(ProgressDialog_DoWork)))
            {
                pd.CancelButtonEnabled = false;
                switch (pd.ShowDialog())
                {
                    case DialogResult.Abort:
                        //エラー情報を取得する
                        Exception ex = pd.Error;
                        MessageBox.Show(ex.Message, "MQOExporter: Error!!", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        break;
                    case DialogResult.Cancel:
                        MessageBox.Show("キャンセルされました。", "MQOExporter", MessageBoxButtons.OK, MessageBoxIcon.Stop);
                        break;
                    case DialogResult.OK:
                        MessageBox.Show("書き出しが完了しました。", "MQOExporter", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        break;
                    default:
                        break;
                }
            }
        }

        //DoWorkイベントハンドラ
        private void ProgressDialog_DoWork(object sender, DoWorkEventArgs e)
        {
            BackgroundWorker bw = (BackgroundWorker)sender;

            FileFormat.MQOFile mqo = new FileFormat.MQOFile();

            int mc = pmx.Material.Count;
            int cw = 100 / mc;

            // 形状の変換
            // pmxは材質ごとに面がある
            bw.ReportProgress(0, "材質と形状の変換中");
            for (var i = 0; i < pmx.Material.Count; i++) { mqo.Material.Add(null); mqo.Object.Add(null); }
            Parallel.For(0, pmx.Material.Count, matID =>
            {
                var pmxMat = pmx.Material[matID];

                // 材質変換
                var mqoMat = new FileFormat.MQOMaterial(pmxMat.Name);
                mqoMat.Color.R = (decimal)pmxMat.Diffuse.R;
                mqoMat.Color.G = (decimal)pmxMat.Diffuse.G;
                mqoMat.Color.B = (decimal)pmxMat.Diffuse.B;
                mqoMat.Color.A = (decimal)pmxMat.Diffuse.A;
                mqoMat.Diffuse = 1;
                mqoMat.Ambient = (decimal)(pmxMat.Ambient.R + pmxMat.Ambient.G + pmxMat.Ambient.B)/3;
                mqoMat.Specular = (decimal)(pmxMat.Specular.R + pmxMat.Specular.G + pmxMat.Specular.B)/3;
                mqoMat.Power = (decimal)pmxMat.Power;
                if (pmxMat.Tex != null) mqoMat.Tex = pmxMat.Tex;
                mqo.Material[matID] = mqoMat;

                // 形状変換
                var mqoObj = new FileFormat.MQOObject(pmxMat.Name);
                foreach (var face in pmxMat.Faces)
                {
                    int v1 = appendVertex(mqoObj, face.Vertex1);
                    int v2 = appendVertex(mqoObj, face.Vertex2);
                    int v3 = appendVertex(mqoObj, face.Vertex3);
                    int uv1 = appendUV(mqoObj, face.Vertex1);
                    int uv2 = appendUV(mqoObj, face.Vertex2);
                    int uv3 = appendUV(mqoObj, face.Vertex3);
                    var mqoFace = new FileFormat.MQOFace
                    {
                        MatID = matID,
                        VertexID = new int[] { v1, v2, v3 },
                        UVID = new int[] { uv1, uv2, uv3 }
                    };
                    mqoObj.Face.Add(mqoFace);
                }
                mqo.Object[matID] = mqoObj;
                bw.ReportProgress(cw, 1);
            });

            mqo.FixNames();

            bw.ReportProgress(0, "書き出し中(バーは動きません (^-^;)");
            // 書き出し
            mqo.WriteTo(mqopath);

            bw.ReportProgress(100, "書き出し完了");
            //結果を設定する
            //e.Result = true;
        }
        private int appendVertex(FileFormat.MQOObject mqoObj, IPXVertex v)
        {
            // 注: pmxとmqoはZ軸の向きが逆
            return mqoObj.getVertexIndex((decimal)v.Position.X, (decimal)v.Position.Y, -(decimal)v.Position.Z);
        }
        private int appendUV(FileFormat.MQOObject mqoObj, IPXVertex v)
        {
            return mqoObj.getUVIndex((decimal)v.UV.U, (decimal)v.UV.V);
        }
    }
}
