using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Threading;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Runtime.CompilerServices;

namespace OS {
    public partial class OS_CD : Form {
        static int MaxWellLen = 15;  //输出井长为300字节，每个文件长度不得超过500字节
        static int MaxFileCount = 10;  //假设每个用户最多输出10个文件
        List<PCB> waitQueue = new List<PCB>();
        Queue<OutputReqBlock> printQueue = new Queue<OutputReqBlock>();  //打印队列
        int blockCount = 3;  //请求输出块最多4个
        OutputWell well = new OutputWell();
        int SPOOLing_status = 0;  //SPOOLing进程的状态，初始为可运行
        Random rd = new Random();  //随机函数
        int indexPro = 1;
        DataTable dt;
        bool flag_a = false;   //指定是否初始化
        bool flag_b = false;
        public static Object o = new Object();
        List<List<string>> res = new List<List<string>>();  //保存结果，最后统一添加
        bool final_a = false, final_b = false, final_out = false;  //指定是否结束

        public OS_CD() {
            InitializeComponent();
            dt = new DataTable();
            dt.Columns.Add(new DataColumn("序号", typeof(string)));
            dt.Columns.Add(new DataColumn("当前运行进程", typeof(string)));
            dt.Columns.Add(new DataColumn("进程状态", typeof(string)));
            dt.Columns.Add(new DataColumn("输出井状态", typeof(string)));
            dt.Columns.Add(new DataColumn("可用请求块个数", typeof(string)));
            dt.Columns.Add(new DataColumn("文件序号", typeof(string)));
            dt.Columns.Add(new DataColumn("文件长度", typeof(string)));
            dataGridView1.EnableHeadersVisualStyles = false;
            dataGridView1.RowsDefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;
            dataGridView1.ColumnHeadersDefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;
            dataGridView1.ColumnHeadersDefaultCellStyle.Font = new Font("楷体", 11, FontStyle.Bold);
            dataGridView1.ColumnHeadersDefaultCellStyle.ForeColor = Color.Red;
            //设置DataGridView文本居中
            //this.dataGridView1.DataSource = dt;
            comboBox1.Items.Add("A");
            comboBox1.Items.Add("B");
            comboBox1.SelectedIndex = comboBox1.Items.IndexOf("A");
        }


        class PCB {
            /*
             * 进程描述
             */
            public int id;
            public int status;   //状态，0表示可执行，123表示三个等待状态，4表示结束
            public string[] contents = new string[MaxFileCount];  //要输出的内容
            public int[] flags = new int[MaxFileCount];  //为1表示该文件已经被输出，初始全部为0
            public int fileCount;  //用户真实输入的文件个数
        }

        class OutputReqBlock {
            /*
             * 输出请求块
             */
            public int id;  //要求输出进程的id
            public int start;  //信息在输出井中的起始位置
            public int length; //信息长度
            public int fileIndex; //要输出文件的序号
            public OutputReqBlock(int id, int start, int length, int fileIndex) {
                this.id = id;
                this.start = start;
                this.length = length;
                this.fileIndex = fileIndex;
            }
        }

        class OutputWell {
            /*
             * 输出井
             */
            public char[] buffer = new char[MaxWellLen];  //输出缓冲区
            public int begin = 0;   //当前可用位置
            public int restSize = MaxWellLen;  //剩余容量
        }

        private void button1_Click(object sender, EventArgs e) {
            /*
             * 初始化各个请求进程以及输出井
            */
            string x = comboBox1.Text;
            string fileString = richTextBox1.Text;  //对进程要输出的文件进行切割，以#作为间隔
            //检查是否以#结尾
            if (fileString[fileString.Length - 1] != '#') {
                MessageBox.Show("请检查输入内容是否以#结尾！", "提示", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                return;
            }
            int index = x == "A" ? 0 : 1;
            PCB pcb = new PCB();
            pcb.id = index;
            pcb.status = 0; //可执行状态
            string[] contents = new string[MaxFileCount];
            int[] flags = new int[MaxFileCount];
            int j = 0;
            int ptr = -1;
            //切割
            while(ptr != fileString.Length - 1) {
                int start = ptr + 1;
                ptr = fileString.IndexOf("#", start, fileString.Length - start);
                contents[j] = fileString.Substring(start, ptr - start);
                flags[j] = 0;
                j++;
            }
            pcb.contents = contents;
            pcb.fileCount = j;  //文件个数
            pcb.flags = flags;
            //检查用户是否重复点击了初始化按钮，是就覆盖
            if(index == 0) {
                if(waitQueue.Count >= 1) {
                    waitQueue[0] = pcb;
                }else {
                    waitQueue.Add(pcb);
                }
                flag_a = true;
            }else {
                if(waitQueue.Count == 2) {
                    waitQueue[1] = pcb;
                }else {
                    waitQueue.Add(pcb);
                }
                flag_b = true;
            }
            if (index == 0) {
                MessageBox.Show("进程A初始化成功！", "提示", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
            }
            if (index == 1) {
                MessageBox.Show("进程B初始化成功！", "提示", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
            }
        }

        private void richTextBox1_TextChanged(object sender, EventArgs e) {

        }
        
        private void dispatchProcess(int x) {
            /*
             * 输出服务程序，将要输出的文件送到输出井，然后形成一个输出块
             */
            string str = x == 0 ? "A" : "B";
            Console.WriteLine("正在运行用户进程" + str + "...");
            int fileIndex = -1;
            for (int i = 0; i < waitQueue[x].fileCount; i++) {
                if (waitQueue[x].flags[i] == 0) {   //寻找一个未被放入输出井的文件
                    fileIndex = i;
                    break;
                }
            }
            //申请一个输出块
            if (fileIndex == -1) {
                waitQueue[x].status = 4;  //用户进程结束
                return;
            } else {
                //检查输出井是否还有位置
                int len = waitQueue[x].contents[fileIndex].Length;
                if (well.restSize >= len) {   //还有剩余空间
                    //形成请求块并唤醒可能沉睡的SPOOLing输出进程
                    //检查是否还有剩余请求块
                    if(blockCount > 0) {
                        OutputReqBlock outBlock = new OutputReqBlock(x, well.begin, len, fileIndex);
                        printQueue.Enqueue(outBlock);  //请求块放入队列等待输出
                        if (SPOOLing_status == 1) {
                            SPOOLing_status = 0; //唤醒
                        }
                        //修改输出井参数
                        for (int i = well.begin; i < well.begin + len; i++) {
                            well.buffer[i] = waitQueue[x].contents[fileIndex][i - well.begin];
                        }
                        //修改进程本身参数
                        waitQueue[x].flags[fileIndex] = 1;
                        //唤醒可能沉睡的SPOOLing输出进程
                        SPOOLing_status = 0; //可执行状态
                        List<string> temp = new List<string>();
                        temp.Add("" + indexPro);
                        indexPro++;
                        temp.Add("进程" + str);
                        temp.Add("可执行");
                        temp.Add("可用空间:" + well.restSize);
                        temp.Add("" + blockCount);
                        temp.Add("" + fileIndex);
                        temp.Add("" + len);
                        res.Add(temp);
                        well.begin += len;
                        well.restSize -= len;
                        blockCount--;
                        Console.WriteLine("当前调用结束");
                        return;
                    } else {
                        Console.WriteLine("无可用请求块！");
                        waitQueue[x].status = 3;  //没有可用的请求输出块，等待状态3
                        List<string> temp = new List<string>();
                        temp.Add("" + indexPro);
                        indexPro++;
                        temp.Add("进程" + str);
                        temp.Add("等待状态3");
                        temp.Add("可用空间:" + well.restSize);
                        temp.Add("" + blockCount);
                        temp.Add("" + fileIndex);
                        temp.Add("" + len);
                        res.Add(temp);
                        Console.WriteLine("当前调用结束");
                        return;
                    }
                } else {
                    waitQueue[x].status = 1; //输出井已满，等待状态1
                    Console.WriteLine("输出已满！");
                    List<string> temp = new List<string>();
                    temp.Add("" + indexPro);
                    indexPro++;
                    temp.Add("进程" + str);
                    temp.Add("等待状态1");
                    temp.Add("可用空间:" + well.restSize + ",已满!");
                    temp.Add("" + blockCount);
                    temp.Add("" + fileIndex);
                    temp.Add("" + len);
                    res.Add(temp);
                    Console.WriteLine("当前调用结束");
                    return;
                }
            }
        }

        private void SPOOLing() {
            /*
             * 从输出队列找到一个请求输出块并输出
             */
            Console.WriteLine("当前正在调用输出进程...");
             if(well.restSize == MaxWellLen) {   //输出井空
                SPOOLing_status = 2;   //输出井空，等待状态2
                Console.WriteLine("输出井空，无法打印!");
                List<string> temps = new List<string>();
                temps.Add("" + indexPro);
                indexPro++;
                temps.Add("输出进程");
                temps.Add("等待状态2");
                temps.Add("空");
                temps.Add("" + blockCount);
                temps.Add("无");
                temps.Add("无");
                res.Add(temps);
                return;
            }
            if (SPOOLing_status == 2) {//虽然输出井不为空，但进程处于等待状态2
                Console.WriteLine("输出井空，无法打印!");
                List<string> temps = new List<string>();
                temps.Add("" + indexPro);
                indexPro++;
                temps.Add("输出进程");
                temps.Add("等待状态2");
                temps.Add("空");
                temps.Add("" + blockCount);
                temps.Add("无");
                temps.Add("无");
                res.Add(temps);
                return;
            }
            //没有输出请求块可以输出：
            if(printQueue.Count == 0) {
                Console.WriteLine("当前没有需要输出的请求块！");
                List<string> temps = new List<string>();
                temps.Add("" + indexPro);
                indexPro++;
                temps.Add("输出进程");
                temps.Add("等待状态2");
                temps.Add("空");
                temps.Add("" + blockCount);
                temps.Add("无");
                temps.Add("无");
                res.Add(temps);
                return;
            }
            //输出一个请求块，然后将等待的进程全部唤醒
            OutputReqBlock block = printQueue.Dequeue();
            Console.WriteLine("进程" + block.id + "正在输出...");
            int begin = block.start;
            int length = block.length;
            for(int i = begin; i < begin + length; i++) {
                Console.Write(well.buffer[i]);
            }
            Console.WriteLine();
            //输出一下buffer
            Console.WriteLine("输出buffer：");
            for (int i = 0; i < well.begin; i++) {
                Console.Write(well.buffer[i]);
            }
            Console.WriteLine();
            //显示在文本框中
            string id = block.id == 0 ? "A" : "B";
            string strs = this.richTextBox2.Text;
            strs += "正在输出进程" + id + "的文件" + block.fileIndex + "\n";
            for(int i = begin; i < begin + length; i++) {
                strs += well.buffer[i];
            }
            strs += "\n";
            this.richTextBox2.Text = strs;
            Console.WriteLine();
            //唤醒进程
            waitQueue[0].status = 0;
            waitQueue[1].status = 0;  //全部唤醒
            string str = block.id == 0 ? "A" : "B";
            List<string> temp = new List<string>();
            temp.Add("" + indexPro);
            indexPro++;
            temp.Add("输出进程");
            temp.Add("可执行");
            temp.Add("可用空间:" + well.restSize);
            temp.Add("" + blockCount);
            temp.Add("输出进程" + str + "的文件" + block.fileIndex);
            temp.Add("" + block.length);
            res.Add(temp);
            //释放资源
            well.begin -= length;
            well.restSize += length;
            //数组前移
            for(int i = begin; i < well.begin; i++) {
                well.buffer[i] = well.buffer[i + length];
            }
            //修改每个输出块的起始位置
            for(int i = 0; i < printQueue.Count; i++) {
                OutputReqBlock blocks = printQueue.Dequeue();
                blocks.start -= length;
                printQueue.Enqueue(blocks);
            }
            blockCount++;   //释放了一个请求块
            return;
        }

        private void button2_Click(object sender, EventArgs e) {
            /*
             *主程序
             */
             //有进程未初始化，程序不能执行
             if(!flag_a || !flag_b) {
                MessageBox.Show("尚未初始化完毕！", "提示", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                return;
            }
            while(true) {
                int decision = dispatch();  //调度
                switch (decision) {
                    case 0:
                        //进程A
                        if (waitQueue[0].status == 4 && !final_a) {   //用户进程A已经结束
                            Console.WriteLine("用户进程A" + "已经运行结束...");
                            List<string> temp = new List<string>();
                            temp.Add("" + indexPro);
                            indexPro++;
                            temp.Add("进程A");
                            temp.Add("结束");
                            temp.Add("可用空间:" + well.restSize);
                            temp.Add("" + blockCount);
                            temp.Add("无");
                            temp.Add("无");
                            res.Add(temp);
                            final_a = true;
                        }else {
                            if (waitQueue[0].status == 0) {  //可执行状态
                                dispatchProcess(0);
                            }
                        }
                        break;
                    case 1:
                        //进程B
                        if (waitQueue[1].status == 4 && !final_b) {   //用户进程B已经结束
                            Console.WriteLine("用户进程B" + "已经运行结束...");
                            List<string> temp = new List<string>();
                            temp.Add("" + indexPro);
                            indexPro++;
                            temp.Add("进程B");
                            temp.Add("结束");
                            temp.Add("可用空间:" + well.restSize);
                            temp.Add("" + blockCount);
                            temp.Add("无");
                            temp.Add("无");
                            res.Add(temp);
                            final_b = true;
                        }else {
                            if (waitQueue[1].status == 0) {  //可执行状态
                                dispatchProcess(1);
                            }
                        }
                        break;
                    case 2:
                        //输出进程结束
                        if (waitQueue[0].status == 4 && waitQueue[1].status == 4 && printQueue.Count == 0 && !final_out) {
                            List<string> temps = new List<string>();
                            temps.Add("" + indexPro);
                            indexPro++;
                            temps.Add("输出进程");
                            temps.Add("结束");
                            temps.Add("空");
                            temps.Add("" + blockCount);
                            temps.Add("无");
                            temps.Add("无");
                            res.Add(temps);
                            final_out = true;
                        }else {
                            if(SPOOLing_status == 0) {
                                SPOOLing();
                            }
                        }
                        break;
                    default: break;
                }
                if(final_a && final_b && final_out) {//两个用户进程和输出进程都已经输出
                    break;
                }
            }
            //结果展示在表格中
            Console.WriteLine("打印结束！");
            for(int i = 0; i < res.Count; i++) {
                List<string> temp = res[i];
                DataRow dr = dt.NewRow();
                dr["序号"] = temp[0];  //序号
                dr["当前运行进程"] = temp[1];   //当前进程
                dr["进程状态"] = temp[2];  //进程状态
                dr["输出井状态"] = temp[3];  //输出井状态
                dr["可用请求块个数"] = temp[4];  //请求块个数
                dr["文件序号"] = temp[5];  //文件序号
                dr["文件长度"] = temp[6];  //文件长度
                dt.Rows.Add(dr);
            }
            this.dataGridView1.DataSource = dt;  //展示
        }

        private void label5_Click(object sender, EventArgs e) {

        }

        private void OS_CD_Load(object sender, EventArgs e) {
            
        }

        private int dispatch() {
            /*
             * 进程调度
             */
            double res = rd.NextDouble();  //产生一个01之间的小数
            if(res <= 0.45) {
                return 0;
            }else if(res <= 0.9) {
                return 1;
            }else {
                return 2;   //012分别表示两个进程和SPOOLing输出进程
            }
        }
    }
}