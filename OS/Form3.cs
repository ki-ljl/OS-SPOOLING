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
    public partial class Form3 : Form {

        int L, m, k;
        int index = 0;  //指向当前元素
        int[] arrs;  //保存页面走向
        int loss = 0; //记录未命中次数
        //Queue<int> queue = new Queue<int>();  //FIFO
        //FIFO应该也用一个数组表示
        List<int> queue = new List<int>();
        List<int> weight_queue = new List<int>();
        List<int> weight = new List<int>();
        List<int> items = new List<int>();  //LRU的页面分布

        private void Form3_Load(object sender, EventArgs e) {

        }

        private void textBox1_TextChanged(object sender, EventArgs e) {

        }

        private void button3_Click(object sender, EventArgs e) {
            //先获取L, m, k
            string text = textBox1.Text;
            string[] temp = Regex.Split(text, "\\s+", RegexOptions.IgnoreCase);
            L = int.Parse(temp[0].Trim());
            m = int.Parse(temp[1].Trim());
            k = int.Parse(temp[2].Trim());
            text = "";
            for(int i = 0; i < m; i++) {
                weight_queue.Add(0);
                //weight.Add(0);
            }
            Random rd = new Random();
            arrs = new int[L];  //保存每一个变量的权重
            for (int i = 0; i < L; i++) {
                arrs[i] = rd.Next(0, k);
                if(i != L - 1) {
                    text += (arrs[i] + " ");
                }else {
                    text += arrs[i];
                }
                //Console.WriteLine(rd.Next(0, k));
            }
            this.textBox2.Text = text;
        }

        private void textBox3_TextChanged(object sender, EventArgs e) {

        }

        private void button4_Click(object sender, EventArgs e) {
            this.label4.Text = "当前第0次";
            this.label6.Text = "累计不命中0次";
            this.textBox1.Text = "";
            this.textBox2.Text = "";
            this.textBox3.Text = "";
            this.textBox4.Text = "";
            this.textBox5.Text = "";
            weight.Clear();
            weight_queue.Clear();
            queue.Clear();
            items.Clear();
            index = 0;
            loss = 0;
        }

        private void button2_Click(object sender, EventArgs e) {
            if(index == L) {
                MessageBox.Show("已结束!", "提示", MessageBoxButtons.OKCancel, MessageBoxIcon.Exclamation);
            }else {
                //方便操作，在这里也获取一下页面走向
                string text = textBox2.Text;
                string[] temp = Regex.Split(text, "\\s+", RegexOptions.IgnoreCase);
                for (int i = 0; i < L; i++) {
                    arrs[i] = int.Parse(temp[i].Trim());
                }
                for (int i = 0; i < weight.Count; i++) {
                    weight[i]++;
                }
                int current = arrs[index];
                string str = "";
                //this.textBox5.Text = str;
                index++;
                this.label4.Text = "当前第" + index + "次";
                if (!items.Contains(current)) {
                    if (weight.Count < m) {
                        weight.Add(0);
                    }
                    loss++;
                    this.label6.Text = "累计不命中" + loss + "次";
                    if (items.Count == m) {
                        //选择一个权值最大的位置替换出去
                        int replace = weight.IndexOf(weight.Max());  //替换replace
                        items[replace] = current;
                        weight[replace] = 0;
                    } else {
                        items.Add(current);
                    }
                } else {
                    //命中后权重为0
                    //区别就在这里，FIFO命中后权重不会清零
                    weight[items.IndexOf(current)] = 0;
                }
                for (int i = 0; i < weight.Count; i++) {
                    str += (weight[i] + " ");
                }
                this.textBox5.Text = str;
                str = "";
                for (int i = 0; i < items.Count; i++) {
                    if (i != items.Count - 1) {
                        str += (items[i] + " ");
                    } else {
                        str += items[i];
                    }
                }
                this.textBox3.Text = str;
                if (index == L) {
                    this.textBox4.Text = (Math.Round((double)loss / L, 2) * 100).ToString() + "%";
                }
            }
        }

        private void button1_Click(object sender, EventArgs e) {
            if(index == L) {
                MessageBox.Show("已结束!", "提示", MessageBoxButtons.OKCancel, MessageBoxIcon.Exclamation);
            }else {
                //方便操作，在这里也获取一下页面走向
                string text = textBox2.Text;
                string[] temp = Regex.Split(text, "\\s+", RegexOptions.IgnoreCase);
                for (int i = 0; i < L; i++) {
                    arrs[i] = int.Parse(temp[i].Trim());
                }
                //一共点击L次，每次点击时就将队列中的值显示出来并更新队列
                //逗留时间均+1
                for (int i = 0; i < m; i++) {
                    weight_queue[i]++;
                }
                //先更新队列，当前应该考虑index位置的页
                int current = arrs[index];
                index++;
                this.label4.Text = "当前第" + index + "次";
                if (!queue.Contains(current)) {
                    loss++;
                    this.label6.Text = "累计不命中" + loss + "次";
                    if (queue.Count == m) {
                        //寻找逗留时间最长的位置
                        int s = weight_queue.IndexOf(weight_queue.Max());
                        queue[s] = current;  //替换掉
                        weight_queue[s] = 0; //时间清零
                    } else {
                        queue.Add(current);
                    }
                }
                this.label6.Text = "累计不命中" + loss + "次";
                string str = "";
                for (int i = 0; i < queue.Count; i++) {
                    if (i != queue.Count - 1) {
                        str += (queue[i] + " ");
                    } else {
                        str += queue[i];
                    }
                }
                this.textBox3.Text = str;
                if (index == L) {
                    this.textBox4.Text = (Math.Round((double)loss / L, 2) * 100).ToString() + "%";
                }
            }
        }

        public Form3() {
            InitializeComponent();
            Font font = new Font(FontFamily.GenericMonospace, 9, FontStyle.Regular);
            //label1.Font = font;
            //label2.Font = font;
            //label3.Font = font;
            //label4.Font = font;
            //label5.Font = font;
            //label6.Font = font;
            //label7.Font = font;
            textBox1.Font = font;
            textBox2.Font = font;
            textBox3.Font = font;
            textBox4.Font = font;
            textBox5.Font = font;
        }
    }
}
