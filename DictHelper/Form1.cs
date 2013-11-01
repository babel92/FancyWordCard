using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using System.Net;
using System.IO;
using System.Text.RegularExpressions;
using System.Threading;

namespace DictHelper
{
    public partial class Form1 : Form
    {
        List<Entry> Words;

        public Form1()
        {
            InitializeComponent();
            Words = new List<Entry>();
        }

        private string GetSource(string Word)
        {
            string Base = "http://dict.youdao.com/m/";
            HttpWebRequest Req = HttpWebRequest.Create(Base + Word + '/') as HttpWebRequest;
            Req.Method = "GET";
            HttpWebResponse Resp = Req.GetResponse() as HttpWebResponse;
            StreamReader Reader = new StreamReader(Resp.GetResponseStream(),true);
            string Src = Reader.ReadToEnd();
            return Src.IndexOf("出错")==-1?Src:null;
        }

        private string[] GetExplanation(string Word)
        {
            string Tag1="<div class=\"content\">";
            string Tag2="</div>";
            string Src = GetSource(Word);
            if (Src == null)
                return null;
            int Content = Src.IndexOf(Tag1) + Tag1.Length;
            Src=Src.Substring(Content);
            Src=Src.Substring(0,Src.IndexOf(Tag2));
            string[] Tag3 = { "<br/>" };
            return Regex.Replace(Src.Trim(), @"\s+", " ").Split(Tag3,StringSplitOptions.RemoveEmptyEntries);
        }

        private string GetExplanationOneLiner(string Word)
        {
            string[] Res = GetExplanation(Word);
            if (Res == null)
                return "";
            else if (Res.Length == 1)
                return Res[0];
            else
            {
                string Ret = "";
                foreach (string str in Res)
                    Ret += str + ' ';
                return Ret;
            }
        }
        
        List<ManualResetEvent> Event;

        private void ThreadPoolWorker(object arg)
        {
            int Index=(int)arg;
            Words[Index].Explanation = GetExplanationOneLiner(Words[Index].Word);
            SetLVExplanation(Index,Words[Index].Explanation);
            SetComplete();
            Event[Index % 64].Set();
        }

        private void SetLVExplanation(int Index, string Exp)
        {
            if (InvokeRequired)
                Invoke(new MethodInvoker(delegate { SetLVExplanation(Index, Exp); }));
            else
            {
                listView1.Items[Index].SubItems[2].Text = Exp;
            }
        }

        private int Complete;

        private void SetComplete()
        {
            if (InvokeRequired)
                Invoke(new MethodInvoker(delegate { SetComplete(); }));
            else
            {
                toolStripStatusLabel1.Text = "Complete: " + ++Complete + "/" + Words.Count;
            }
        }

        private void WorkerThread()
        {
            Event=new List<ManualResetEvent>();
            Complete = 0;

            for (int i = 0; i < Words.Count; ++i)
            {
                Event.Add(new ManualResetEvent(false));
                ThreadPool.QueueUserWorkItem(ThreadPoolWorker,i);
                if (Event.Count == 64 || i==Words.Count-1)
                {
                    WaitHandle.WaitAll(Event.ToArray());
                    Event.Clear();
                }
            }

            StreamWriter sw=new StreamWriter(openFileDialog1.FileName.Insert(openFileDialog1.FileName.LastIndexOf('.'),"_out"));
            foreach (Entry Word in Words)
            {
                sw.WriteLine(Word.Word + '\t' + Word.Explanation);
            }
            sw.Close();

            MessageBox.Show("Work Finished", "Done");
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            if (DialogResult.OK == openFileDialog1.ShowDialog())
            {
                StreamReader sr = new StreamReader(openFileDialog1.FileName);
                string Line = sr.ReadLine();
                int i = 0;
                while (Line != null)
                {
                    if (Line.Trim().Length > 0)
                    {
                        Words.Add(new Entry(Line, null));
                        listView1.Items.Add(new ListViewItem(new [] {(++i).ToString(),Line,""}));
                    }
                    Line = sr.ReadLine();
                }
                sr.Close();
                if (Words.Count == 0)
                {
                    MessageBox.Show("File is empty!", null);
                    Application.Exit();
                }
                Thread T = new Thread(new ThreadStart(WorkerThread));
                T.Start();
            }
            else
                Application.Exit();
        }
    }

    class Entry
    {
        public String Word;
        public String Explanation;
        public Entry(String W, String E)
        {
            Word = W;
            Explanation = E;
        }
    }

}
