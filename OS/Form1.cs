using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace OS {
    public partial class Form1 : Form {
        int k = 0;
        Job[] jobs;
        string []Tj = new string[100];
        string []Ys = new string[100];  // k <= 100
        public Form1() {
            InitializeComponent();
            Font font = new Font(FontFamily.GenericMonospace, 12, FontStyle.Regular);
            richTextBox1.SelectionFont = font;
            textBox1.Font = font;
        }

        private void button1_Click(object sender, EventArgs e) {
            k = Convert.ToInt32(textBox1.Text);
            //Console.WriteLine("k=" + k);
            string inputString = richTextBox1.Text;
            //Console.WriteLine(inputString);
            
            string[] substrs = inputString.Split(Environment.NewLine.ToCharArray());
            //Console.WriteLine(substrs.Length);
            int index = 0;
            for (int i = 0; i < substrs.Length; i++) {
                if(substrs[i] != Environment.NewLine && substrs[i] != "") {
                    //分割
                    string[] temp = Regex.Split(substrs[i], "\\s+", RegexOptions.IgnoreCase);
                    //Console.WriteLine(temp.Length);
                    //这里是防止每一行输入了不止两个数据，只取前两个
                    bool flag = false;
                    for(int j = 0; j < temp.Length; j++) {
                        if(temp[j] != " ") {
                            Tj[index] = temp[j];
                            for(int k = j + 1; k < temp.Length; k++) {
                                if(temp[k] != " ") {
                                    Ys[index] = temp[k];
                                    index++;
                                    flag = true;
                                    break;
                                }
                            }
                            if(flag) {
                                break;
                            }
                        }
                    }
                }
         
            }
            jobs = new Job[index];
            k = index;
            for (int i = 0; i < k; i++) {
                DateTime commitTime = DateTime.Parse(Tj[i].Substring(0, 2) + ":" + Tj[i].Substring(2, 2));
                string id = i.ToString();
                int runTime = int.Parse(Ys[i]);
                Job job = new Job(id, commitTime, runTime);
                jobs[i] = job;
                //Console.WriteLine(Tj[i] + " " + Ys[i]);
            }
            scheduling();
        }

        //作业调度程序

        private void scheduling() {
            //按照提交时间排序
            Console.WriteLine("初始排序后输出一下");
            Array.Sort(jobs, new compare1());
            foreach(Job job in jobs) {
                Console.WriteLine(job.id + " " + job.commitTime + " " + job.runTime);
            }
            //确定第一个被调度的作业，如果12提交时间相同就选择运行时间短的那个
            int current = 0;
            if(DateTime.Compare(jobs[0].commitTime, jobs[1].commitTime) == 0) {
                if(jobs[0].runTime - jobs[1].runTime > 0) {
                    current = 1;   //从1开始调度
                }
            }
            int cnt = 0;  //记录当前一共调度了几个
            DateTime now = jobs[current].commitTime;  //当前时间
            while(cnt < k) {
                //开始调度current
                jobs[current].beginTime = now;
                jobs[current].endTime = now + TimeSpan.FromMinutes(jobs[current].runTime);  //开始运行
                for(int i = 0; i < k; i++) {
                    if(jobs[i].flag || i == current) {
                        continue;
                    }else {
                        int c = DateTime.Compare(jobs[i].commitTime, jobs[current].endTime);
                        if(c < 0) {   //当前进程结束时进程i已经提交，则开始有了等待时间
                            TimeSpan s = jobs[current].endTime - jobs[i].commitTime;
                            jobs[i].waitTime = s.Hours * 60 + s.Minutes;
                        }
                    }
                }
                //运行结束
                now = jobs[current].endTime;
                jobs[current].flag = true;
                //挑选下一个要调度的进程
                float R = -1;
                bool flags = false;
                for(int i = 0; i < k; i++) {
                    if(jobs[i].flag) {
                        continue;
                    }else {
                        int c = DateTime.Compare(jobs[i].commitTime, now);
                        if(c < 0) {   //结束时已经提交了，则肯定有了等待时间，就能计算R
                            int waitTime = jobs[i].waitTime;
                            int runTime = jobs[i].runTime;
                            float r = 1 + ((float)waitTime / runTime);
                            flags = true;
                            if(r > R) {
                                R = r;
                                current = i;
                            }
                        }
                    }
                }
                if(!flags) {
                    //说明当前进程结束后，后面的所有进程都还没有提交，那就选择一个最近的，此时也要考虑是否存在提交时间相同的情况
                    for(int i = 0; i < k; i++) {
                        if(jobs[i].flag) {
                            continue;
                        }else {
                            if (i == k - 1) {
                                current = i;
                                break;
                            }else {
                                for(int j = i + 1; j < k; j++) {
                                    if(jobs[j].flag) {
                                        continue;
                                    }else {
                                        //判断i和j是否同时提交且运行时间相同
                                        if(DateTime.Compare(jobs[i].commitTime, jobs[j].commitTime) == 0) {
                                            current = jobs[i].runTime < jobs[j].runTime ? i : j;
                                            now = jobs[current].commitTime;
                                            break;
                                        }else {
                                            current = i;
                                            now = jobs[current].commitTime;
                                            break;
                                        }
                                    }
                                }
                                break;
                            }
                        }
                    }
                }
                Console.WriteLine("当前current：" + current);
                cnt++;
            }
            //计算周转时间和带权周转时间
            for(int i = 0; i < k; i++) {
                Job job = jobs[i];
                int runTime = job.runTime;
                int waitTime = job.waitTime;
                jobs[i].turnAroundTime = runTime + waitTime;
                jobs[i].weightTime = (float)jobs[i].turnAroundTime / runTime;
            }
            Console.WriteLine("调度结束后输出一下");
            Array.Sort(jobs, new compare1());
            foreach (Job job in jobs) {
                Console.WriteLine(job.id + " " + job.beginTime + " " + job.runTime);
            }

            //按照开始调度时间进行排序
            Array.Sort(jobs, new compare2());
            DataTable dt = new DataTable();//建立数据表
            dt.Columns.Add(new DataColumn("调度次序", typeof(string)));
            dt.Columns.Add(new DataColumn("作业号", typeof(string)));
            dt.Columns.Add(new DataColumn("调度时间", typeof(string)));
            dt.Columns.Add(new DataColumn("周转时间", typeof(string)));
            dt.Columns.Add(new DataColumn("带权周转时间", typeof(string)));
            DataRow dr;//行
            for(int i = 0; i < k; i++) {
                dr = dt.NewRow();
                dr["调度次序"] = i + 1;
                dr["作业号"] = (int.Parse(jobs[i].id) + 1) + "";
                dr["调度时间"] = jobs[i].beginTime.ToString("hh:mm");
                dr["周转时间"] = jobs[i].turnAroundTime;
                dr["带权周转时间"] = jobs[i].weightTime;
                dt.Rows.Add(dr);//在表的对象的行里添加此行
            }
            //计算平均周转时间
            float sum1 = 0, sum2 = 0;
            foreach(Job job in jobs) {
                sum1 += job.turnAroundTime;
                sum2 += job.weightTime;
            }
            dr = dt.NewRow();
            float avg = sum1 / k;
            dr["调度次序"] = "平均周转时间:" + Math.Round(avg, 2);
            dt.Rows.Add(dr);
            dr = dt.NewRow();
            avg = sum2 / k;
            dr["调度次序"] = "平均带权周转时间:" + Math.Round(avg, 2);
            dt.Rows.Add(dr);
            //文字居中显示
            DataGridViewCellStyle headerStyle = new DataGridViewCellStyle();
            headerStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;
            dataGridView1.ColumnHeadersDefaultCellStyle = headerStyle;
            dataGridView1.RowsDefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;
            //颜色交替
            dataGridView1.EnableHeadersVisualStyles = false;
            dataGridView1.EnableHeadersVisualStyles = false;
            dataGridView1.ColumnHeadersDefaultCellStyle.BackColor = Color.Wheat;
            dataGridView1.RowsDefaultCellStyle.BackColor = Color.FromArgb(255, 255, 255);
            dataGridView1.AlternatingRowsDefaultCellStyle.BackColor = Color.Wheat;
            dataGridView1.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
            dataGridView1.RowHeadersVisible = false;//datagridview前面的空白部分去除
            dataGridView1.ScrollBars = ScrollBars.None;//滚动条去除
            //绑定数据
            dataGridView1.DataSource = dt;
            //输出一下结果
            Console.WriteLine("输出一下运行结果:");
            foreach(Job job in jobs) {
                Console.WriteLine(job.id + " " + job.beginTime + " " + job.endTime + " " + job.waitTime);
            }

        }

        private void richTextBox1_TextChanged(object sender, EventArgs e) {
            limitLine(k);
            richTextBox1.SelectionStart = richTextBox1.TextLength;
            richTextBox1.Focus();
        }

        private void limitLine(int maxLength) {
            if(this.richTextBox1.Lines.Length > maxLength) {
                string[] lines = this.richTextBox1.Lines;
                string[] cutlines = new string[maxLength];
                for(int i = 0; i < maxLength; i++) {
                    cutlines[i] = lines[i];
                }
                this.richTextBox1.Lines = cutlines;
            }
        }

        private void textBox1_TextChanged(object sender, EventArgs e) {
            k = int.Parse(textBox1.Text);
        }

        class Job {
            public string id;
            public DateTime commitTime;
            public bool flag = false;
            public DateTime beginTime;
            public int runTime;
            public DateTime endTime;
            public int turnAroundTime = 0;
            public float weightTime = 0;
            public int waitTime = 0;
            public Job(string id, DateTime t, int x) {
                this.id = id;
                this.commitTime = t;
                this.runTime = x;
            }
        }

        public class compare1 : IComparer<Job> {
            int IComparer<Job>.Compare(Job x, Job y) {
                int c = DateTime.Compare(x.commitTime, y.commitTime);
                if (c < 0) {
                    return -1;
                } else {
                    return 1;
                };
            }
        }

        public class compare2 : IComparer<Job> {
            int IComparer<Job>.Compare(Job x, Job y) {
                int c = DateTime.Compare(x.beginTime, y.beginTime);
                if (c < 0) {
                    return -1;
                } else {
                    return 1;
                };
            }
        }


        private void dataGridView1_CellContentClick(object sender, DataGridViewCellEventArgs e) {

        }
    }
}