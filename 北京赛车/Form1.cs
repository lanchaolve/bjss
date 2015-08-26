using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace 北京赛车
{
    public partial class Form1 : Form
    {
        string Link;
        sscEntities entitys;
        List<bjsc> resultData;
        List<cjhm> baseData;
        public Form1()
        {
            InitializeComponent();
            Link = string.Empty;
            entitys = new sscEntities();
            //LoadAndGetData();
            baseData = IsSuccess();
            dataGridView2.DataSource = baseData;
            resultData = new List<bjsc>();
        }
        int _executeCounter = 0;
        private async void LoadAndGetData()
        {

            var t = new Thread(new ThreadStart(() => { }));
            t.IsBackground = true;
            t.Start();
            _executeCounter = 0;

            while (true)
            {
                await Task.Run(() =>
                {


                    _executeCounter++;
                    UpdateLogUI(string.Format("开始第{0}次任务", _executeCounter));
                    try
                    {
                        Random r = new Random();
                        Link = String.Format("http://www.1396b.com/Pk10/ajax?ajaxhandler=GetNewestRecord&t={0}", r.NextDouble().ToString());
                        string result = SendRequest(Link);

                        bjsc model = Newtonsoft.Json.JsonConvert.DeserializeObject<bjsc>(result);
                        bjsc addModel = new bjsc() { numbers = model.numbers, period = model.period };

                    
                    }
                    catch (Exception ex)
                    {

                        UpdateLogUI(ex.Message);
                    }
                    Thread.Sleep(120000);
                });
            }




        }

        private List<cjhm> IsSuccess()
        {
            List<cjhm> result = new List<cjhm>();
            for (int i = 1; i < 9; i++)
            {
                for (int j = 1; j < 11; j++)
                {
                    if (j > i)
                    {
                        for (int k = 1; k < 11; k++)
                        {
                            if (k > i && k > j)
                            {
                                cjhm li = new cjhm();
                                li.number1 = i;
                                li.number2 = j;
                                li.number3 = k;
                                li.id = i + "," + j + "," + k;
                                result.Add(li);
                            }
                        }
                    }
                }
            }
            return result;
        }

        /// <summary>
        /// 更新UI控件
        /// </summary>
        /// <param name="message"></param>
        private void UpdateLogUI(string message)
        {
            BeginInvoke(new Action(() =>
            {
                listBox1.Items.Insert(0, message);
            }));
        }

        private void UpdateLogSyncUI(string message)
        {
            listBox1.Items.Insert(0, message);
        }

        /// <summary>
        /// 更新UI控件
        /// </summary>
        /// <param name="message"></param>
        private void UpdateStatus(string message)
        {
            BeginInvoke(new Action(() =>
            {
                status.Text = message;
            }));
        }

        public string SendRequest(string url)
        {
            string strResult = "";
            if (url == null || url == "")
                return null;
            try
            {
                System.Net.WebRequest wrq = System.Net.WebRequest.Create(url);
                wrq.Method = "GET";
                ServicePointManager.SecurityProtocol = SecurityProtocolType.Ssl3;
                System.Net.WebResponse wrp = wrq.GetResponse();
                System.IO.StreamReader sr = new System.IO.StreamReader(wrp.GetResponseStream(), System.Text.Encoding.GetEncoding("gb2312"));
                strResult = sr.ReadToEnd();
            }
            catch (Exception ex)
            {
                return ex.Message;
            }
            return strResult;
        }

        public async void LoadJsonData()
        {
            var t = new Thread(new ThreadStart(() => { }));
            t.IsBackground = true;
            t.Start();

            resultData = new List<bjsc>();

            await Task.Run(() =>
            {
                try
                {
                    Random r = new Random();
                    Link = String.Format("http://m.1396mp.com/api/pk10/history?date={0}", DateTime.Now.ToString("yyyy-MM-dd"));
                    string result = SendRequest(Link);
                    suib model = Newtonsoft.Json.JsonConvert.DeserializeObject<suib>(result);

                    foreach (var d in model.itemArray)
                    {
                        bjsc _bjsc = new bjsc() { numbers = d[3], period = int.Parse(d[2]) };
                        resultData.Add(_bjsc);
                    }
                    UpdateLogUI("数据抓取完毕！");

                    BeginInvoke(new Action(() =>
                    {
                        dataGridView1.DataSource = resultData.OrderByDescending(p => p.period).ToList();
                        button1.Enabled = true;
                    }));
                }
                catch (Exception ex)
                {
                    UpdateLogUI(ex.Message);
                }
            });
        }

        private void button1_Click(object sender, EventArgs e)
        {
            button1.Enabled = false;
            //CatchData();

            LoadJsonData();
        }

        private void button2_Click(object sender, EventArgs e)
        {
            var hmdata = resultData.OrderBy(p => p.period).ToList();
            
            foreach (var bd in baseData)
            {

                foreach (var hmd in hmdata)
                {
                    int earnCount = 0;

                    int pres = hmd.period - 1;
                    var hcpre = hmdata.SingleOrDefault(p => p.period == pres);
                    if (hcpre != null)
                    {
                        var current = hmd.numbers.Split(',').ToList();
                        var pre = hcpre.numbers.Split(',').ToList();

                        #region 算法
                        //n1
                        int index1 = pre.IndexOf(bd.number1.ToString());
                        if (index1 == 0)
                        {
                            if (current.Take(3).Contains(bd.number1.ToString()))
                            {
                                earnCount++;
                            }
                        }
                        else if (index1 == 9)
                        {
                            if (current.Skip(7).Take(3).Contains(bd.number1.ToString()))
                            {
                                earnCount++;
                            }
                        }
                        else
                        {
                            if (current.Skip(index1 -1).Take(3).Contains(bd.number1.ToString()))
                            {
                                earnCount++;
                            }
                        }

                        //n2
                        int index2 = pre.IndexOf(bd.number2.ToString());
                        if (index2 == 0)
                        {
                            if (current.Take(3).Contains(bd.number2.ToString()))
                            {
                                earnCount++;
                            }
                        }
                        else if (index2 == 9)
                        {
                            if (current.Skip(7).Take(3).Contains(bd.number2.ToString()))
                            {
                                earnCount++;
                            }
                        }
                        else
                        {
                            if (current.Skip(index2 - 1).Take(3).Contains(bd.number2.ToString()))
                            {
                                earnCount++;
                            }
                        }

                        //n3
                        int index3 = pre.IndexOf(bd.number3.ToString());
                        if (index3 == 0)
                        {
                            if (current.Take(3).Contains(bd.number3.ToString()))
                            {
                                earnCount++;
                            }
                        }
                        else if (index3 == 9)
                        {
                            if (current.Skip(7).Take(3).Contains(bd.number3.ToString()))
                            {
                                earnCount++;
                            }
                        }
                        else
                        {
                            if (current.Skip(index3 - 1).Take(3).Contains(bd.number3.ToString()))
                            {
                                earnCount++;
                            }
                        }

                        switch (earnCount)
                        {
                            case 1: bd.中一++; break;
                            case 0: bd.没中++; break;
                        }

                        if (earnCount > 1)
                        {
                            bd.没中 = 0;
                            bd.中一 = 0;
                        }

                        #endregion

                    }


                    bd.总 = bd.没中 + bd.中一;
                }
            }


            dataGridView2.DataSource = baseData.OrderBy(p => p.中一).OrderByDescending(p => p.没中).ToList();
        }

        private void timer1_Tick(object sender, EventArgs e)
        {

            try
            {
                resultData = new List<bjsc>();
                Random r = new Random();
                Link = String.Format("http://m.1396mp.com/api/pk10/history?date={0}", DateTime.Now.ToString("yyyy-MM-dd"));
                string result = SendRequest(Link);
                suib model = Newtonsoft.Json.JsonConvert.DeserializeObject<suib>(result);

                foreach (var d in model.itemArray)
                {
                    bjsc _bjsc = new bjsc() { numbers = d[3], period = int.Parse(d[2]) };

                    resultData.Add(_bjsc);
                }

                UpdateLogSyncUI("数据抓取完毕！");

                dataGridView1.DataSource = resultData.OrderByDescending(p => p.period).ToList();

                var hmdata = resultData.OrderBy(p => p.period).ToList();

                foreach (var bd in baseData)
                {
                    foreach (var hmd in hmdata)
                    {
                        int earnCount = 0;

                        int pres = hmd.period - 1;
                        var hcpre = hmdata.SingleOrDefault(p => p.period == pres);
                        if (hcpre != null)
                        {
                            var current = hmd.numbers.Split(',').ToList();
                            var pre = hcpre.numbers.Split(',').ToList();

                            #region 算法
                            //n1
                            int index1 = pre.IndexOf(bd.number1.ToString());
                            if (index1 == 0)
                            {
                                if (current.Take(3).Contains(bd.number1.ToString()))
                                {
                                    earnCount++;
                                }
                            }
                            else if (index1 == 9)
                            {
                                if (current.Skip(7).Take(3).Contains(bd.number1.ToString()))
                                {
                                    earnCount++;
                                }
                            }
                            else
                            {
                                if (current.Skip(index1 - 1).Take(3).Contains(bd.number1.ToString()))
                                {
                                    earnCount++;
                                }
                            }

                            //n2
                            int index2 = pre.IndexOf(bd.number2.ToString());
                            if (index2 == 0)
                            {
                                if (current.Take(3).Contains(bd.number2.ToString()))
                                {
                                    earnCount++;
                                }
                            }
                            else if (index2 == 9)
                            {
                                if (current.Skip(7).Take(3).Contains(bd.number2.ToString()))
                                {
                                    earnCount++;
                                }
                            }
                            else
                            {
                                if (current.Skip(index2 - 1).Take(3).Contains(bd.number2.ToString()))
                                {
                                    earnCount++;
                                }
                            }

                            //n3
                            int index3 = pre.IndexOf(bd.number3.ToString());
                            if (index3 == 0)
                            {
                                if (current.Take(3).Contains(bd.number3.ToString()))
                                {
                                    earnCount++;
                                }
                            }
                            else if (index3 == 9)
                            {
                                if (current.Skip(7).Take(3).Contains(bd.number3.ToString()))
                                {
                                    earnCount++;
                                }
                            }
                            else
                            {
                                if (current.Skip(index3 - 1).Take(3).Contains(bd.number3.ToString()))
                                {
                                    earnCount++;
                                }
                            }

                            switch (earnCount)
                            {
                                case 1: bd.中一++; break;
                                case 0: bd.没中++; break;
                            }

                            if (earnCount > 1)
                            {
                                bd.没中 = 0;
                                bd.中一 = 0;
                            }

                            #endregion

                        }


                        bd.总 = bd.没中 + bd.中一;
                    }
                }


                dataGridView2.DataSource = baseData.OrderBy(p => p.中一).OrderByDescending(p => p.没中).ToList();
            }
            catch (Exception ex)
            {
                UpdateLogSyncUI(ex.Message);
            }


        }

        
    }
}
