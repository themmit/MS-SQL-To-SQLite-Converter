using System;
using System.Data;
using System.Data.SQLite;
using System.Threading;
using System.Windows;
using System.ComponentModel;

namespace MS_SQL_To_SQLite_Converter {

    public partial class MainWindow : Window {

        SQLiteConnection con;
        DataTable dt;
        Thread test;
        BackgroundWorker backgroundWorker;

        public MainWindow() {
            InitializeComponent();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e) {
            updateStatus("Widnow Loaded");
            try {
                con = new SQLiteConnection("Data Source=S:\\Budget\\test.db; Version=3;datetimeformat=CurrentCulture;new=false;Readonly=false;");
                updateStatus("SQLite DB loaded");
                con.Open();
                updateStatus("SQLite db test connection success.");
                con.Close();
                Thread thread = new Thread(new ThreadStart(loadDT));
                thread.Start();
                updateStatus("Click to start event.");
                test = new Thread(new ThreadStart(Insmsg));
                test.IsBackground = true;
                backgroundWorker = new BackgroundWorker();
                backgroundWorker.DoWork += BackgroundWorker_DoWork;
                backgroundWorker.RunWorkerCompleted += BackgroundWorker_RunWorkerCompleted;
                backgroundWorker.WorkerSupportsCancellation = true;
            } catch (Exception) {
                updateStatus("SQLite db test connection failed.");
                convert_now.IsEnabled = false;
                convert_now_bw.IsEnabled = false;
                updateStatus("Restart to retry.");
            }
        }

        private void BackgroundWorker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e) {
            updateStatus("Completed - BackgroundWorker");
        }

        private void BackgroundWorker_DoWork(object sender, DoWorkEventArgs e) {
            Insmsg();
        }

        public delegate void UpdateTextCallback(string message);

        private void loadDT() {
            try {
                dt = new DataTable();
                dt.Columns.Add("msg");
                for (int i = 1; i <= 10; i++) {
                    dt.Rows.Add(new object[] { "Row : " + i.ToString() });
                }
            } catch (Exception ex) {
            }
        }

        public void Insmsg() {
            try {
                con.Open();
                for (int i = 0; i < dt.Rows.Count; i++) {
                    string query = "insert into msg values ('" + dt.Rows[i][0].ToString() + "')";
                    SQLiteCommand cmd = new SQLiteCommand(query, con);
                    cmd.ExecuteNonQuery();
                    progress.Dispatcher.Invoke(new UpdateTextCallback(this.updateStatus), new object[] { "Row Number: " + i });
                }
                con.Close();
                progress.Dispatcher.Invoke(new UpdateTextCallback(this.updateStatus), new object[] { "Completed" });
            } catch (Exception ex) {
                MessageBox.Show(ex.Message);
            }
        }

        private void updateStatus(String text) {
            progress.Items.Add(text);
        }

        private void Button_Click(object sender, RoutedEventArgs e) {
            if (!test.IsAlive) {
                updateStatus("Copy Paste Started - Threading");
                test.Start();
            } else {
                test.Abort();
                progress.Items.Add("Aborted");
            }
        }

        private void Convert_now_bw_Click(object sender, RoutedEventArgs e) {
            if (!backgroundWorker.IsBusy) {
                backgroundWorker.RunWorkerAsync();
            }
        }
    }
}
