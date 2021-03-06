# OS-SPOOLING
基于C# winform的操作系统课程设计：SPOOLING假脱机输入输出技术模拟。

# 一、需求分析
## 1.题目要求
要求设计一个SPOOLING输出进程和两个请求输出的用户进程，以及一个SPOOLING输出服务程序。当请求输出的用户进程希望输出一系列信息时，调用输出服务程序，由输出服务程序将该信息送入输出井。待遇到一个输出结束标志时，表示进程该次的输出文件输出结束。之后，申请一个输出请求块(用来记录请求输出的用户进程的名字、信息在输出井中的位置、要输出信息的长度等)，等待SPOOLING进程进行输出。

SPOOLING输出进程工作时，根据请求块记录的各进程要输出的信息，将其实际输出到打印机或显示器。这里，SP00LING输出进程与请求输出的用户进程可并发运行。

进程调度采用随机算法，这与进程输出信息的随机性相一致。两个请求输出的用户进程的调度概率各为45％，SPOOLING输出进程为10％，这由随机数发生器产生的随机数来模拟决定。

进程基本状态有3种，分别为可执行、等待和结束。可执行态就是进程正在运行或等待调度的状态；等待状态又分为等待状态1、等待状态2和等待状态3。

状态变化的条件为：

①进程执行完成时，置为“结束”态。

②服务程序在将输出信息送输出井时，如发现输出井已满，将调用进程置为“等待状态1”。

③SPOOLING进程在进行输出时，若输出井空，则进入“等待状态2”。

④SPOOLING进程输出一个信息块后，应立即释放该信息块所占的输出井空间，并将正在等待输出的进程置为“可执行状态”。

⑤服务程序在输出信息到输出井并形成输出请求信息块后，若SPOOLING进程处于等待态，则将其置为“可执行状态”。

⑥当用户进程申请请求输出块时，若没有可用请求块时，调用进程进入“等待状态3”。

## 2.用户进程分析
系统中一共有两个请求输出的用户进程，两个进程分别命名为用户进程A和用户进程B。用户需要输出的文件可能不止一个，文件之间由一个输出结束标志隔开，本文实验采用的文件输出结束标志为#号。

用户需要在初始化阶段输入要进行输出的全部文件内容，然后存进一个数组中。在用户进程被调度时，如果满足以下三个条件：当前还有文件没有被输出、输出井中剩余空间能够放下该文件、有可用的输出请求块，就将该文件送到输出井，然后申请一个输出请求块，加入请求块等待队列，等待SPOOLING输出进程进行调度输出。

## 3.SPOOLING输出进程分析

轮到SPOOLING输出进程占用CPU时，SPOOLING输出进程首先检查请求块等待队列中是否有输出块需要输出，没有就进入等待状态。否则，进行输出，然后释放输出井空间以及相应的请求输出块，并唤醒因为没有可用输出块而陷入沉睡的用户进程。

## 4.并发分析

SPOOLING输出进程与用户进程可以并发执行。本文将进程的一次执行过程（执行后进程不一定结束）抽象成一个函数，每次先产生一个随机数，根据随机数执行某一进程，如果该进程因为某些情况被阻塞，则产生下一个随机数，调度另外的进程。用户进程和SPOOLING输出进程因为各种原因被轮流调度，即为并发执行。

# 二、整体功能及设计
## 1.功能划分
整个系统的功能分为以下几个部分：初始化函数、调度函数、用户进程函数、SPOOLING输出函数。初始化函数用于实现用户文件的初始输入与保存；调度函数实现用户进程和SPOOLING输出进程间的切换；用户进程函数实现进程被调度后所完成的一系列动作；SPOOLING输出函数表示输出操作。

系统运行总的流程图如图1所示：

![在这里插入图片描述](https://img-blog.csdnimg.cn/7ab95f62accf45ddbe1f34b5be2799e0.png#pic_center)

系统首先利用初始化函数对用户输入的内容进行初始化，然后产生一个0到1之间的随机数R，根据R的大小判断当前应该执行用户进程还是输出进程。等用户进程和输出进程都执行完毕后，程序运行结束，否则继续进行调度。

## 2.初始化函数
用户需要输入自己想要“打印”的内容，初始化函数接受用户输入的内容后按照文件结束符进行切割，并将切割好的文件放入一个数组中，等用户进程被调度时再将其送往输出井中。

## 3.调度函数

进程调度采用随机算法，两个请求输出的用户进程的调度概率各为45％，SPOOLING输出进程为10％，本文采用随机数来实现这一要求。进行进程调度时，随机生成一个0到1之间的小数。如果该数小于等于0.45就将用户进程A投入运行；如果该数处于0.45到0.9之间，就将用户进程B投入运行；如果该数大于0.9，就将SPOOLING输出进程投入运行。

## 4.用户进程函数

用户进程函数首先需要检查当前进程是否满足三个条件：还有文件没有被输出、输出井中剩余空间能够放下该文件、有可用的输出请求块，满足三个条件就将该文件送到输出井并申请相应请求块。

用户进程函数执行的流程图如图2所示：

![在这里插入图片描述](https://img-blog.csdnimg.cn/af2fd7692f3d4972bd330eb92178a600.png#pic_center)

用户进程执行时，如果发现文件已经输出完毕，则进程运行结束。否则判断输出井是否有剩余空间，无进入等待状态1。输出井有剩余空间继续判断是否有可用输出块，有就将文件送往输出井并请求一个输出块，并唤醒可能沉睡的输出进程，否则进入等待状态3。

## 5.SPOOLING输出函数

SPOOLING输出函数检查是否有可输出的请求块，有则进行输出并释放相关资源，否则SPOOLING输出进程等待。

SPOOLING输出函数的流程图如图3所示：

![在这里插入图片描述](https://img-blog.csdnimg.cn/0e25a6259f3e4ab6982312906b265091.png#pic_center)

# 三、编程实现

## 1.数据结构

### 1.1 用户进程PCB

PCB的定义如下：

```csharp
class PCB {
      /*
      * 进程描述
      */
      public int id;  //序号
      public int status;   //状态，0表示可执行，123表示三个等待状态，4表示结束
      public string[] contents = new string[MaxFileCount];  //要输出的内容
      public int[] flags = new int[MaxFileCount]; //为1表示该文件已经被输出，初始全部为0
      public int fileCount;  //用户真实输入的文件个数
}
```

用户进程中包括序号id、进程状态status、要输出的内容contents、文件输出标志flags以及真实文件个数fileCount。

其中，用户进程可能存在的进程状态有：0表示可执行状态、1表示等待状态1、3表示等待状态3、4表示进程结束。

### 1.2 输出请求块OutPutReqBlock

OutPutReqBlock定义如下：

```csharp
class OutputReqBlock {
      /*
      * 输出请求块
      */
      public int id;  //要求进行输出的进程的id
      public int start;  //文件在输出井中的起始位置
      public int length; //文件长度
      public int fileIndex; //要输出文件的序号
      public OutputReqBlock(int id, int start, int length, int fileIndex) {
             this.id = id;
             this.start = start;
             this.length = length;
             this.fileIndex = fileIndex;
    }
}
```
请求输出块中包括：请求该请求块的进程id，文件在输出井中的起始位置start、文件长度length、要输出的文件在用户所有文件中的序号。

### 1.3 输出井OutputWell
OutputWell的定义如下：

```csharp
class OutputWell {
      /*
       * 输出井
       */
       public char[] buffer = new char[MaxWellLen];  //输出缓冲区
       public int begin = 0;   //当前可用位置
       public int restSize = MaxWellLen;  //剩余容量
}
```

输出井的参数有：缓冲区buffer，用于存放用户放入的数据；当前可用位置begin，文件在输出井中按顺序存放，begin始终指向当前可用缓冲区的起始位置；剩余容量restSize，缓冲区中剩余的容量，初始时为缓冲区长度MaxWellLen。

## 2.初始化函数
用户在文本框中输入要&ldquo;打印&rdquo;的信息，然后选择输出内容属于哪一个进程（A or B），最后点击初始化按钮，即可启动初始化函数。初始化函数首先利用一个string对象存储用户输入的内容。随后，检查用户输入的内容是否以#号结尾，不合法则提示用户重新输入。输入合法后，将用户输入的内容按#号进行切割，切割形成多个字符串，最后利用生成的各项信息初始化一个PCB对象，放入等待队列waitQueue中。

由于用户可能多次点击初始化按钮，所以每次点击前需要判断当前进程是否已经初始化完成，如果已经初始化完成用户却再次点击初始化按钮，则会覆盖原来的内容。

输出井在系统界面进行加载时自动初始化。

初始化函数代码略！

## 3.调度函数
为了实现随机性，每次要进行调度时，就利用C#的Random函数生成一个0到1之间的随机数。如果该随机数小于等于0.45表示接下来要调度用户进程A；如果该随机数处于0.45到0.9之间表示接下来要调度用户进程B；如果该随机数大于0.9表示接下来要调度SPOOLING输出进程。

调度函数的实现如下：

```csharp
private int dispatch() {
      /*
       * 进程调度
       */
       double res = rd.NextDouble();  //产生一个01之间的小数
       if(res <= 0.45) {
           return 0;
       }else if(res <= 0.9) {
           return 1;
       }else {
           return 2;   //012分别表示两个进程和SPOOLing输出进程
    }
}
```

## 4.用户进程函数
用于实现用户进程运行时所进行的一系列操作。

用户进程被调度时，首先检查是否还有文件未送到输出井，没有则置当前用户进程为结束状态，函数返回。

用户进程尚未结束，说明还有文件未被送入输出井。循环查找一个尚未输出的文件块（相应flag标志为1），接着查询输出井中的剩余空间是否还能放下此文件块，如果不能，将进程状态置为等待状态1，函数返回。若还有剩余空间，接着检查是否还有可用的请求输出块，如果没有将进程置为等待状态3，函数返回。否则将文件块送入输出井并修改输出井相关参数，然后申请一个请求输出块，放入输出队列printQueue中，等待SPOOLING输出进程被调度时进行打印输出。最后，如果SPOOLING输出进程处于等待状态，该用户进程需要将其唤醒。

用户进程函数运行时的各种情况通过一个列表进行保存，用于最后的结果展示。列表内容包括：当前调度序号、进程号、进程状态、输出井状态、可用请求块个数、文件序号、文件长度。

用户进程函数代码略！

## 5.SPOOLING输出函数
输出函数的功能是选择一个请求输出块，然后对其中的内容进行输出，最后释放掉相应资源。

首先检查输出井是否为空，空就置输出进程为等待状态2，函数返回。否则检查请求输出队列中是否有需要输出的请求输出块，没有函数返回。否则从请求输出队列中取出队首的请求输出块，然后输出请求块，并释放相应的输出井空间和请求块。

输出函数进行输出时，要将输出内容显示到文件输出区域。

输出函数代码略！

## 6.主函数
用户点击&ldquo;程序运行&rdquo;按钮后开始运行主函数，主函数中根据当前情况来动态调整运行的进程。

主函数首先判断两个用户进程是否都已经初始化完毕，初始化完毕才能运行，否则提示出错。

初始化完毕后再次点击&ldquo;程序运行&rdquo;按钮，只要有一个进程未处于结束状态，或者有一个请求块尚未被输出，则继续调度。调度时要判断当前进程是否已经结束，结束就输出相关状态。

主函数代码略！


## 7.参数说明

实验中用到的各种参数说明如表1所示：
|参数名称|MaxWellLen |MaxFileCount|blockCount|
|--|--|--|--|
|参数说明 |输出井长度   |一个用户可以输出的最大文件数  |请求块个数  |
|参数值 |15|10 |3  |


# 四、使用说明
## 1.界面

系统界面如图4所示：

![在这里插入图片描述](https://img-blog.csdnimg.cn/e3a3cf5276e04239b063086202766ca6.png#pic_center)

系统界面分为三个版块：初始化、调度过程以及文件输出区。初始化版块包含一个文本框、一个选择框、一个按钮，用户在文本框中输入需要打印的文件，然后进行初始化。调度过程版块主要为一个表格，用于展示进程调度的详细过程。文件输出区版块用于展示所有文件的打印过程。

## 2.初始化

用户首先选择一个进程，初始默认为A，随后将要输出的文件放入初始化版块的文本框中，然后点击初始化按钮，初始化成功，如图5所示：

![在这里插入图片描述](https://img-blog.csdnimg.cn/5ee1ca103ad245e5bca15b63e0ddcd77.png#pic_center)

对于进程B同进行上述操作，如图6所示：

![在这里插入图片描述](https://img-blog.csdnimg.cn/740986076d8643ac8b0ee5234b4b82e7.png#pic_center)
## 3.结果展示

两个用户进程初始化完成后，点击程序运行按钮，结果展示如图7和图8所示：

![&nbsp;](https://img-blog.csdnimg.cn/c6219426150942aba343eb3b8021f36a.png#pic_center)

![在这里插入图片描述](https://img-blog.csdnimg.cn/81d243dbe6234430b6c38a89b4b7e84a.png#pic_center)
# 五、结果分析
下面对图7内容做简单分析。如图9所示：

![在这里插入图片描述](https://img-blog.csdnimg.cn/d3231cfabb044e86ab3f627dce57c8b3.png#pic_center)

第1次调度了输出进程，因为此时输出井空，所以输出进程状态为等待状态2，此时可用请求块个数为3。第2次调度进程A，进程A状态为可执行，可用请求块个数为3，A将文件0送到输出井，文件0（&ldquo;abcd&rdquo;）长度为4。第3次调度输出进程，输出井可用空间为15-4=11，可用请求块个数变为2，输出A进程的文件0，如文件输出区所示，并释放相关空间。
