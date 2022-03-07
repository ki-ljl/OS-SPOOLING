using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text.RegularExpressions;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace OS {
    public partial class Form2 : Form {

        int m = 12;
        int n = 14;
        int index = 0;

        int[,] res = new int[1000, 14];   //用于保存最后结果
        int[] Avaliable = new int[12];
        int[] copyAva = new int[12];
        int[,] Max = new int[14, 12];
        int[,] Allocation = new int[14, 12];
        int[,] copyAl = new int[14, 12];
        int[,] Need = new int[14, 12];
        int[,] copyNd = new int[14, 12];
        int[] request = new int[12];
        

        public Form2() {
            InitializeComponent();
            Font font = new Font(FontFamily.GenericMonospace, 12, FontStyle.Regular);
            richTextBox1.SelectionFont = font;
            richTextBox2.SelectionFont = font;
            richTextBox3.SelectionFont = font;
            textBox1.Font = font;
            textBox2.Font = font;
        }

        private void banker(int[] request, int i) {  //进程i请求资源
            for(int j = 0; j < m; j++) {
                if(request[j] <= Need[i, j]) {
                    continue;
                }else {
                    Console.WriteLine("超过最大需求，请求失败!");
                    return;
                }
            }
            for(int j = 0; j < m; j++) {
                if(request[j] <= Avaliable[j]) {
                    continue;
                }else {
                    Console.WriteLine("超过剩余资源数目，请求失败!");
                    return;
                }
            }
            //预分配
            int[] copyAvaliable = Avaliable;
            int[,] copyAllocation = Allocation;
            int[,] copyNeed = Need;
            for(int j = 0; j < m; j++) {
                copyAvaliable[j] -= request[j];
                copyAllocation[i, j] += request[j];
                copyNeed[i, j] -= request[j];
            }
            //安全性检查
            int[] arr = new int[n];
            for(int j = 0; j < n; j++) {
                arr[j] = j;
            }
            Permutation(arr, 0, arr.Length);
            DataTable dt = new DataTable();//建立数据表
            dt.Columns.Add(new DataColumn("序号", typeof(int)));
            for (int t = 0; t < n; t++) {
                dt.Columns.Add(new DataColumn("进程" + t, typeof(int)));
            }

            //dataGridView1.Rows.Clear();
            if (index != 0) {
                Console.WriteLine("状态安全!");
                DataRow dr;//行
                for (int t = 0; t < index; t++) {
                    dr = dt.NewRow();
                    dr["序号"] = t + 1;
                    for (int j = 0; j < n; j++) {
                        dr["进程" + j] = res[t, j];
                    }
                    dt.Rows.Add(dr);//在表的对象的行里添加此行
                }
                dataGridView1.DataSource = dt;
                MessageBox.Show("分配后状态安全，可以分配!", "提示", MessageBoxButtons.OKCancel, MessageBoxIcon.Exclamation);
                for (int x = 0; x < index; x++) {
                    Console.WriteLine("安全序列" + (x + 1) + ":");
                    for (int j = 0; j < n; j++) {
                        Console.Write(res[x, j] + " ");
                    }
                    Console.WriteLine("");
                }
            } else {
                MessageBox.Show("分配后状态不安全，不能分配!", "提示", MessageBoxButtons.OKCancel, MessageBoxIcon.Exclamation);
                Console.WriteLine("状态不安全!");
            }
        }

        private void Permutation(int[] arr, int l, int h) {
            if (l < h - 1) {
                Permutation(arr, l + 1, h);
                for (var i = l + 1; i < h; i++) {
                    var t = arr[l];
                    arr[l] = arr[i];
                    arr[i] = t;
                    Permutation(arr, l + 1, h);
                    t = arr[l];
                    arr[l] = arr[i];
                    arr[i] = t;
                }
            } else {
                //这里检查
                if(check(arr, Avaliable, Allocation, Need)) {
                    for(int x = 0; x < n; x++) {
                        res[index, x] = arr[x];
                    }
                    index++;
                }
            }
        }

        //安全性检查
        private bool check(int[] sequence, int[] Avaliable, int[,] Allocation, int[,] Need) {
            int[] work = new int[Avaliable.Length];
            Array.Copy(Avaliable, work, work.Length);
            //检查特定序列是否满足要求
            for (int x = 0; x < n; x++) {
                int i = sequence[x]; //当前进程号
                for(int j = 0; j < m; j++) {
                    if(Need[i, j] <= work[j]) {
                        continue;
                    }else {
                        return false;
                    }
                }
                //释放资源
                for(int j = 0; j < m; j++) {
                    work[j] += Allocation[i, j];
                }
            }
            return true;
        }

        //安全性检查，判断当前状态是否安全
        private void button1_Click(object sender, EventArgs e) {
            index = 0;
            //获得输入的n和m
            string n_m = textBox1.Text;
            string[] temp = Regex.Split(n_m, "\\s+", RegexOptions.IgnoreCase);
            n = int.Parse(temp[0].Trim());
            m = int.Parse(temp[1].Trim());
            Console.WriteLine("n = " + n + " m = " + m);
            //输入三个矩阵
            string allocation = richTextBox1.Text;  //n*m的矩阵
            string[] substrs = allocation.Split(Environment.NewLine.ToCharArray());
            for(int i = 0; i < n; i++) {
                string[] line = Regex.Split(substrs[i], "\\s+", RegexOptions.IgnoreCase);
                for(int j = 0; j < m; j++) {
                    Allocation[i, j] = int.Parse(line[j]);
                }
            }
            string need = richTextBox2.Text;  //n*m的矩阵
            substrs = need.Split(Environment.NewLine.ToCharArray());
            for (int i = 0; i < n; i++) {
                string[] line = Regex.Split(substrs[i], "\\s+", RegexOptions.IgnoreCase);
                for (int j = 0; j < m; j++) {
                    Max[i, j] = int.Parse(line[j]);
                }
            }
            string ava = richTextBox3.Text;  //1*m的矩阵
            string[] avas = Regex.Split(ava, "\\s+", RegexOptions.IgnoreCase);
            for(int i = 0; i < m; i++) {
                Avaliable[i] = int.Parse(avas[i]);
            }
            for(int i = 0; i < n; i++) {
                for(int j = 0; j < m; j++) {
                    Need[i, j] = Max[i, j] - Allocation[i, j];
                }
            }
            //输出一下三个矩阵
            Console.WriteLine("Allocation:");
            for(int i = 0; i < n; i++) {
                for(int j = 0; j < m; j++) {
                    Console.Write(Allocation[i, j] + " ");
                }
                Console.WriteLine("");
            }
            Console.WriteLine("Need:");
            for (int i = 0; i < n; i++) {
                for (int j = 0; j < m; j++) {
                    Console.Write(Need[i, j] + " ");
                }
                Console.WriteLine("");
            }
            Console.WriteLine("Avaliable:");
            for(int i = 0; i < m; i++) {
                Console.Write(Avaliable[i]);
            }
            Console.WriteLine("");
            int[] arr = new int[n];
            for (int i = 0; i < n; i++) {
                arr[i] = i;
            }

            Permutation(arr, 0, arr.Length);
            DataTable dt = new DataTable();//建立数据表
            dt.Columns.Add(new DataColumn("序号", typeof(int)));
            for (int i = 0; i < n; i++) {
                dt.Columns.Add(new DataColumn("进程" + i, typeof(int)));
            }
            
            //dataGridView1.Rows.Clear();

            if (index != 0) {
                Console.WriteLine("状态安全!");
                DataRow dr;//行
                for (int i = 0; i < index; i++) {
                    dr = dt.NewRow();
                    dr["序号"] = i + 1;
                    for (int j = 0; j < n; j++) {
                        dr["进程" + j] = res[i, j];
                    }
                    dt.Rows.Add(dr);//在表的对象的行里添加此行
                }
                dataGridView1.DataSource = dt;
                MessageBox.Show("状态安全!", "提示", MessageBoxButtons.OKCancel, MessageBoxIcon.Exclamation);
                for (int x = 0; x < index; x++) {
                    Console.WriteLine("安全序列" + (x + 1) + ":");
                    for (int j = 0; j < n; j++) {
                        Console.Write(res[x, j] + " ");
                    }
                    Console.WriteLine("");
                }
            } else {
                MessageBox.Show("当前状态不安全!", "提示", MessageBoxButtons.OKCancel, MessageBoxIcon.Exclamation);
                Console.WriteLine("状态不安全!");
            }
        }

        private void label5_Click(object sender, EventArgs e) {

        }

        private void textBox2_TextChanged(object sender, EventArgs e) {

        }

        private void button2_Click(object sender, EventArgs e) {
            index = 0;
            //获得输入的n和m
            string n_m = textBox1.Text;
            string[] temp = Regex.Split(n_m, "\\s+", RegexOptions.IgnoreCase);
            n = int.Parse(temp[0].Trim());
            m = int.Parse(temp[1].Trim());
            Console.WriteLine("n = " + n + " m = " + m);
            //输入三个矩阵
            string allocation = richTextBox1.Text;  //n*m的矩阵
            string[] substrs = allocation.Split(Environment.NewLine.ToCharArray());
            for (int i = 0; i < n; i++) {
                string[] line = Regex.Split(substrs[i], "\\s+", RegexOptions.IgnoreCase);
                for (int j = 0; j < m; j++) {
                    Allocation[i, j] = int.Parse(line[j]);
                }
            }
            string need = richTextBox2.Text;  //n*m的矩阵
            substrs = need.Split(Environment.NewLine.ToCharArray());
            for (int i = 0; i < n; i++) {
                string[] line = Regex.Split(substrs[i], "\\s+", RegexOptions.IgnoreCase);
                for (int j = 0; j < m; j++) {
                    Max[i, j] = int.Parse(line[j]);
                }
            }
            string ava = richTextBox3.Text;  //1*m的矩阵
            string[] avas = Regex.Split(ava, "\\s+", RegexOptions.IgnoreCase);
            for (int i = 0; i < m; i++) {
                Avaliable[i] = int.Parse(avas[i]);
            }
            for (int i = 0; i < n; i++) {
                for (int j = 0; j < m; j++) {
                    Need[i, j] = Max[i, j] - Allocation[i, j];
                }
            }
            //获取request向量
            string req = textBox2.Text;
            string[] temps = Regex.Split(req, "\\s+", RegexOptions.IgnoreCase);
            for(int i = 0; i < m; i++) {
                request[i] = int.Parse(temps[i + 1].Trim());
            }
            //银行家算法
            banker(request, int.Parse(temps[0].Trim()));
        }
    }
}