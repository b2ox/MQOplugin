using System;
using System.ComponentModel;
using System.Windows.Forms;

namespace PMDEPlugin
{
    // cf. http://dobon.net/vb/dotnet/programing/progressdialogbw.html
    public partial class ProgressDialog : Form
    {
        /// <summary>
        /// ProgressDialogクラスのコンストラクタ
        /// </summary>
        /// <param name="caption">タイトルバーに表示するテキスト</param>
        /// <param name="doWorkHandler">バックグラウンドで実行するメソッド</param>
        /// <param name="argument">doWorkで取得できるパラメータ</param>    public partial class ProgressDialog : Form
        public ProgressDialog(string caption, DoWorkEventHandler doWork, object argument = null)
        {
            InitializeComponent();

            //初期設定
            this.workerArgument = argument;
            this.Text = caption;
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.ShowInTaskbar = false;
            this.StartPosition = FormStartPosition.CenterParent;
            this.ControlBox = false;
            this.messageLabel.Text = "";
            this.progressBar1.Value = 0;
            this.backgroundWorker1.WorkerReportsProgress = true;
            this.backgroundWorker1.WorkerSupportsCancellation = true;

            //イベント
            this.Shown += new EventHandler(ProgressDialog_Shown);
            this.cancelAsyncButton.Click += new EventHandler(cancelAsyncButton_Click);
            this.backgroundWorker1.DoWork += doWork;
            this.backgroundWorker1.ProgressChanged +=
                new ProgressChangedEventHandler(backgroundWorker1_ProgressChanged);
            this.backgroundWorker1.RunWorkerCompleted +=
                new RunWorkerCompletedEventHandler(backgroundWorker1_RunWorkerCompleted);
        }

        private object workerArgument = null;

        private object _result = null;
        /// <summary>
        /// DoWorkイベントハンドラで設定された結果
        /// </summary>
        public object Result
        {
            get
            {
                return this._result;
            }
        }

        private Exception _error = null;
        /// <summary>
        /// バックグラウンド処理中に発生したエラー
        /// </summary>
        public Exception Error
        {
            get
            {
                return this._error;
            }
        }

        /// <summary>
        /// 進行状況ダイアログで使用しているBackgroundWorkerクラス
        /// </summary>
        public BackgroundWorker BackgroundWorker
        {
            get
            {
                return this.backgroundWorker1;
            }
        }

        //フォームが表示されたときにバックグラウンド処理を開始
        private void ProgressDialog_Shown(object sender, EventArgs e)
        {
            this.backgroundWorker1.RunWorkerAsync(this.workerArgument);
        }

        //キャンセルボタンが押されたとき
        private void cancelAsyncButton_Click(object sender, EventArgs e)
        {
            cancelAsyncButton.Enabled = false;
            backgroundWorker1.CancelAsync();
        }

        //ReportProgressメソッドが呼び出されたとき
        private void backgroundWorker1_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            if (e.UserState is string) // ReportProgress(pp, message)の場合
            {
                //メッセージのテキストを変更する
                this.messageLabel.Text = (string)e.UserState;

                //プログレスバーの値を変更する
                if (e.ProgressPercentage < this.progressBar1.Minimum)
                {
                    this.progressBar1.Value = this.progressBar1.Minimum;
                }
                else if (this.progressBar1.Maximum < e.ProgressPercentage)
                {
                    this.progressBar1.Value = this.progressBar1.Maximum;
                }
                else
                {
                    this.progressBar1.Value = e.ProgressPercentage;
                }

            }
            else if (e.UserState is int && (int)e.UserState == 1) // ReportProgress(pp, 1)の場合
            {
                var x = this.progressBar1.Value + e.ProgressPercentage;
                this.progressBar1.Value = (x > this.progressBar1.Maximum) ? this.progressBar1.Maximum : x;
            }
        }

        //バックグラウンド処理が終了したとき
        private void backgroundWorker1_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            if (e.Error != null)
            {
                MessageBox.Show(this,
                    "エラー",
                    "エラーが発生しました。\n\n" + e.Error.Message,
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
                this._error = e.Error;
                this.DialogResult = DialogResult.Abort;
            }
            else if (e.Cancelled)
            {
                this.DialogResult = DialogResult.Cancel;
            }
            else
            {
                this._result = e.Result;
                this.DialogResult = DialogResult.OK;
            }

            this.Close();
        }

        public bool CancelButtonEnabled
        {
            get
            {
                return cancelAsyncButton.Enabled;
            }
            set
            {
                cancelAsyncButton.Enabled = value;
            }
        }
    }
}
