using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Microsoft.WindowsAPICodePack.Dialogs;
using VM.Core;
using VM.PlatformSDKCS;
using VMControls.Winform.Release;

namespace DeepLearningDemoCs
{


    public partial class MainForm : Form
    {

        private RenderControl renderControl;
        private MainViewControl mainViewControl;
        public bool mSolutionIsLoad = false;  //mSolutionIsLoad = false, indicates that the solution is not loaded
        public VmProcedure vmProcedure = null;
        public ProcessInfoList vmProcessInfoList = new ProcessInfoList();
        public string vmSolutionPath = null;//Solution Path
        private string logPath = Application.StartupPath + "/Log/Message";//Log Path
        private Timer LoadSolutionIndicateTimer = new Timer();
        public int choose = 5;

        private static string GetJoinedStringOutput(VmProcedure procedure, string outputName, string separator = ";")
        {
            var output = procedure.ModuResult.GetOutputString(outputName);
            if (output.astStringVal == null || output.astStringVal.Length == 0)
            {
                return string.Empty;
            }

            return string.Join(separator,
                output.astStringVal
                      .Select(item => item.strValue)
                      .Where(value => !string.IsNullOrEmpty(value)))
                .Replace("\0", string.Empty);
        }

        public MainForm()
        {
            InitializeComponent();
            SetFont(this, new Font("Microsoft YaHei", 15f));
            renderControl = new RenderControl();
            mainViewControl = new MainViewControl();
            renderControl.Dock = DockStyle.Fill;
            mainViewControl.Dock = DockStyle.Fill;
            buttonRender.BackColor = Color.Orange;
            buttonConfig.BackColor = Color.Gray;
            renderPanel.Controls.Add(mainViewControl);
            LoadSolutionIndicateTimer.Interval = 300;
            //LoadSolutionIndicateTimer.Tick += LoadSolutionIndicateTimer_Tick;
            VmSolution.OnWorkStatusEvent += VmSolution_OnWorkStatusEvent;//Registration callback for the procedure working status
            VmSolution.OnProcessStatusStartEvent += VmSolution_OnProcessStatusStartEvent;   // Registration callback for the start of the procedure continuous run
            VmSolution.OnProcessStatusStopEvent += VmSolution_OnProcessStatusStopEvent; // Registration callback for the stop of the procedure continuous run
        }

        private void SetFont(Control control, Font font)
        {
            control.Font = font;
            foreach (Control child in control.Controls)
            {
                SetFont(child, font);
            }
        }


        /// <summary>
        /// Callback function for the start of the procedure continuous run
        /// </summary>
        /// <param name="statusInfo"></param>
        private void VmSolution_OnProcessStatusStartEvent(ImvsSdkDefine.IMVS_STATUS_PROCESS_START_CONTINUOUSLY_INFO statusInfo)
        {
            this.Invoke(new Action(() =>
            {
                if (statusInfo.nStatus == 0)
                {
                    string strMessage = null;
                    buttonContiRun.Text = "停止连续运行";

                    //Disable button
                    //buttonSelectSolu.Enabled = false;
                    buttonRunOnce.Enabled = false;
                    //buttonLoadSolu.Enabled = false;
                    buttonSaveSolu.Enabled = false;
                    comboProcedure.Enabled = false;

                    strMessage = "Start continuous run!";
                    LogFunction(strMessage);
                }
            }));
        }

        public class AutoClosingMessageBox : Form
        {
            private System.Windows.Forms.Timer timer;

            public AutoClosingMessageBox(string message, string title, int duration = 1000)
            {
                this.Text = title;
                this.FormBorderStyle = FormBorderStyle.FixedDialog;
                this.StartPosition = FormStartPosition.CenterScreen;
                this.ClientSize = new Size(300, 100);
                this.ControlBox = false; // 取消关闭按钮

                Label label = new Label()
                {
                    Text = message,
                    AutoSize = false,
                    TextAlign = ContentAlignment.MiddleCenter,
                    Dock = DockStyle.Fill,
                    Font = new Font("Microsoft YaHei", 10)
                };
                this.Controls.Add(label);

                timer = new System.Windows.Forms.Timer();
                timer.Interval = duration;
                timer.Tick += (s, e) =>
                {
                    timer.Stop();
                    this.Close();
                };
                timer.Start();
            }

            public static void Show(string message, string title = "提示", int duration = 1000)
            {
                AutoClosingMessageBox msg = new AutoClosingMessageBox(message, title, duration);
                msg.Show();
            }

            private void InitializeComponent()
            {
                this.SuspendLayout();
                // 
                // AutoClosingMessageBox
                // 
                this.ClientSize = new System.Drawing.Size(1424, 698);
                this.Name = "AutoClosingMessageBox";
                this.ResumeLayout(false);

            }
        }


        /// <summary>
        /// Callback function for the stop of the procedure continuous run
        /// </summary>
        /// <param name="statusInfo"></param>
        private void VmSolution_OnProcessStatusStopEvent(ImvsSdkDefine.IMVS_STATUS_PROCESS_STOP_INFO statusInfo)
        {
            this.Invoke(new Action(() =>
            {
                if (statusInfo.nStopAction == 1)
                {
                    string strMessage = null;
                    buttonContiRun.Text = "连续运行";

                    //Enable button
                    //buttonSelectSolu.Enabled = true;
                    buttonRunOnce.Enabled = true;
                    //buttonLoadSolu.Enabled = true;
                    buttonSaveSolu.Enabled = true;
                    comboProcedure.Enabled = true;

                    strMessage = "End Run!";
                    LogFunction(strMessage);
                }
            }));
        }

        /// <summary>
        /// The button for loading the solution flashes, prompting you to load the solution after selecting the path
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        //private void LoadSolutionIndicateTimer_Tick(object sender, EventArgs e)
        //{
        //    if (!mSolutionIsLoad)
        //    {
        //        if (buttonLoadSolu.BackColor == Color.DimGray)
        //        {
        //            buttonLoadSolu.BackColor = Color.Orange;
        //        }
        //        else
        //        {
        //            buttonLoadSolu.BackColor = Color.DimGray;
        //        }
        //    }
        //    if (mSolutionIsLoad)
        //    {
        //        buttonLoadSolu.BackColor = Color.DimGray;
        //    }
        //}

        /// <summary>
        /// Load Form
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void MainForm_Load(object sender, EventArgs e)
        {
            renderPanel.Controls.Clear();
            renderPanel.Controls.Add(renderControl);
        }

        /// <summary>
        /// Callback function for the procedure working status
        /// </summary>
        /// <param name="workStatusInfo"></param>
        //private void VmSolution_OnWorkStatusEvent(ImvsSdkDefine.IMVS_MODULE_WORK_STAUS workStatusInfo)
        //{
        //    if (workStatusInfo.nWorkStatus == 0)//When the process is running, the nWorkStatus is 1
        //    {
        //        try
        //        {
        //            switch (workStatusInfo.nProcessID)
        //            {
        //                case 10000:
        //                    if (vmProcessInfoList.nNum==0) return;
        //                    VmProcedure vmProcedure = (VmProcedure)VmSolution.Instance[vmProcessInfoList.astProcessInfo[0].strProcessName];
        //                    if (vmProcedure == null) return;
        //                    List<VmDynamicIODefine.IoNameInfo> ioNameInfos = vmProcedure.ModuResult.GetAllOutputNameInfo();
        //                    foreach (var item in ioNameInfos)
        //                    {
        //                        if (item.Name == "out"&&item.TypeName!= IMVS_MODULE_BASE_DATA_TYPE.IMVS_GRAP_TYPE_STRING)
        //                        {
        //                            Task.Run(() =>
        //                            {
        //                                UpdateResult("The result argument (out) is not string format！");

        //                            });
        //                            return;
        //                        }
        //                    }
        //                    var vmResult = vmProcedure.ModuResult.GetOutputString("out").astStringVal[0].strValue;

        //                    Task.Run(() =>
        //                    {
        //                        UpdateResult(vmResult);
        //                        LogFunction("Process running time：" + vmProcedure.ProcessTime.ToString() + "ms");
        //                    });
        //                    break;
        //                default:
        //                    break;
        //            }
        //        }
        //        catch (VmException ex)
        //        {
        //            LogFunction("Failed to get results, Error code: 0x" + Convert.ToString(ex.errorCode, 16));
        //            return;
        //        }
        //        catch (Exception ex)
        //        {
        //            LogFunction("Failed to get results: " + ex.ToString());
        //            return;
        //        }
        //    }
        //}

        private void VmSolution_OnWorkStatusEvent(ImvsSdkDefine.IMVS_MODULE_WORK_STAUS workStatusInfo)
        {
            if (workStatusInfo.nWorkStatus == 0) // 流程运行结束
            {
                try
                {
                    switch (workStatusInfo.nProcessID)
                    {
                 
                        case 10000:
                            if (vmProcessInfoList.nNum == 0) return;

                            VmProcedure currentProcedure = vmProcedure;
                            if (currentProcedure == null && vmProcessInfoList.nNum > 0)
                            {
                                currentProcedure = (VmProcedure)VmSolution.Instance[vmProcessInfoList.astProcessInfo[0].strProcessName];
                            }
                            if (currentProcedure == null) return;

                            List<VmDynamicIODefine.IoNameInfo> currentOutputInfos = currentProcedure.ModuResult.GetAllOutputNameInfo();
                            bool hasOutString = currentOutputInfos.Any(item => item.Name == "out"
                                                                        && item.TypeName == IMVS_MODULE_BASE_DATA_TYPE.IMVS_GRAP_TYPE_STRING);

                            // 解决左上角不能选图像输出问题：手动显示渲染控件的图像源下拉框
                            this.BeginInvoke(new Action(() =>
                            {
                                try
                                {
                                    // 根据实际 SDK API，使左上角的图像下拉选择框可见
                                    renderControl.vmRenderControl1.ChangeImageComboBoxVisibility(true);
                                }
                                catch
                                {
                                    // 忽略异常
                                }
                            }));

                            if (choose == 5)
                            {
                                string additionalTaskResult = hasOutString
                                    ? GetJoinedStringOutput(currentProcedure, "out")
                                    : GetJoinedStringOutput(currentProcedure, "out0");

                                string[] expectedItems = { "杯", "碗", "勺", "筷" };
                                List<string> missingItems = expectedItems
                                    .Where(item => !additionalTaskResult.Contains(item))
                                    .ToList();

                                Task.Run(() =>
                                {
                                    UpdateResult($"附加任务原始结果：{additionalTaskResult}");
                                    if (missingItems.Count > 0)
                                    {
                                        UpdateResult($"附加任务缺失项：{string.Join("、", missingItems)}");
                                    }
                                    else
                                    {
                                        UpdateResult("附加任务缺失项：无");
                                    }
                                    LogFunction("Process running time：" + currentProcedure.ProcessTime.ToString() + "ms");
                                });
                                break;
                            }

                            if (hasOutString)
                            {
                                var vmOutResult = GetJoinedStringOutput(currentProcedure, "out");
                                Task.Run(() =>
                                {
                                    UpdateResult(vmOutResult);
                                    LogFunction("Process running time：" + currentProcedure.ProcessTime.ToString() + "ms");
                                });
                                break;
                            }

                            // 计数检测的输出解析 (预留，需要你根据实际配置补全字段名)
                            if (choose == 4)
                            {
                                // 这里假设你在 VM 里把个数作为流程输出抛出，命名为 out0，且类型是 Int
                                // string result1 = currentProcedure.ModuResult.GetOutputInt("out0").pIntVal[0].ToString();
                                // Task.Run(() =>
                                // {
                                //     UpdateResult($"计数结果：{result1} 个");
                                //     LogFunction("Process running time：" + currentProcedure.ProcessTime.ToString() + "ms");
                                // });
                                return; // 暂时直接 return，防止掉进最后的 default 报错
                            }

                            if (choose == 3)
                            {
                                // 获取输出值
                                string result1 = GetJoinedStringOutput(currentProcedure, "out0");
                                //string result1 = vmProcedure.ModuResult.GetOutputInt("out0").pIntVal[0].ToString();
                                string result2 = currentProcedure.ModuResult.GetOutputFloat("out1").pFloatVal[0].ToString("F4");

                                Task.Run(() =>
                                {
                                    UpdateResult($"检测结果-缺陷类别名称：{result1}");
                                    UpdateResult($"缺陷目标置信度： {result2}");
                                    LogFunction("Process running time：" + currentProcedure.ProcessTime.ToString() + "ms");
                                });
                            }
                            else if(choose==2)
                            {
                                // 获取输出值
                                string result1 = GetJoinedStringOutput(currentProcedure, "out0");
                                //string result1 = vmProcedure.ModuResult.GetOutputInt("out0").pIntVal[0].ToString();
                                string result2 = currentProcedure.ModuResult.GetOutputFloat("out1").pFloatVal[0].ToString("F4");

                                Task.Run(() =>
                                {
                                    UpdateResult($"检测结果-分类类别名称：{result1}");
                                    UpdateResult($"分类类别概率： {result2}");
                                    LogFunction("Process running time：" + currentProcedure.ProcessTime.ToString() + "ms");
                                });

                            }
                            else if (choose == 1)
                            {
                                // 获取输出值
                                string result1 = GetJoinedStringOutput(currentProcedure, "out");
                                //string result1 = vmProcedure.ModuResult.GetOutputInt("out0").pIntVal[0].ToString();
                                //string result2 = vmProcedure.ModuResult.GetOutputFloat("out1").pFloatVal[0].ToString("F4");

                                Task.Run(() =>
                                {
                                    UpdateResult($"检测结果：{result1}");
                                    //UpdateResult($"缺陷目标置信度： {result2}");
                                    LogFunction("Process running time：" + currentProcedure.ProcessTime.ToString() + "ms");
                                });

                            }
                            else if(choose == 0)
                            {
                                // 获取输出值
                                string result1 = currentProcedure.ModuResult.GetOutputInt("out0").pIntVal[0].ToString();
                                bool hasOutInChoose0 = currentOutputInfos.Any(item => item.Name == "out"
                                                                                && item.TypeName == IMVS_MODULE_BASE_DATA_TYPE.IMVS_GRAP_TYPE_STRING);
                                string result2;
                                if (hasOutInChoose0)
                                {
                                    result2 = GetJoinedStringOutput(currentProcedure, "out");
                                }
                                else
                                {
                                    result2 = GetJoinedStringOutput(currentProcedure, "out1");
                                }
                                //string result1 = vmProcedure.ModuResult.GetOutputInt("out0").pIntVal[0].ToString();
                                string result3 = currentProcedure.ModuResult.GetOutputFloat("out2").pFloatVal[0].ToString("F4");

                                Task.Run(() =>
                                {
                                    UpdateResult($"字符个数：{result1}");
                                    UpdateResult($"检测字符串： {result2}");
                                    UpdateResult($"字符串置信度： {result3}");
                                    LogFunction("Process running time：" + currentProcedure.ProcessTime.ToString() + "ms");
                                });

                            }
                            else
                            {
                                // 其他流程默认读取 string 类型的 out（之前的逻辑）
                                List<VmDynamicIODefine.IoNameInfo> ioNameInfos = currentProcedure.ModuResult.GetAllOutputNameInfo();
                                foreach (var item in ioNameInfos)
                                {
                                    if (item.Name == "out" && item.TypeName != IMVS_MODULE_BASE_DATA_TYPE.IMVS_GRAP_TYPE_STRING)
                                    {
                                        Task.Run(() =>
                                        {
                                            UpdateResult("The result argument (out) is not string format！");
                                        });
                                        return;
                                    }
                                }

                                var vmResult = GetJoinedStringOutput(currentProcedure, "out");
                                Task.Run(() =>
                                {
                                    UpdateResult(vmResult);
                                    LogFunction("Process running time：" + currentProcedure.ProcessTime.ToString() + "ms");
                                });
                            }

                            break;

                        default:
                            break;
                    }
                }
                catch (VmException ex)
                {
                    LogFunction("Failed to get results, Error code: 0x" + Convert.ToString(ex.errorCode, 16));
                    return;
                }
                catch (Exception ex)
                {
                    LogFunction("Failed to get results: " + ex.ToString());
                    return;
                }
            }
        }


        /// <summary>
        /// Update Results
        /// </summary>
        /// <param name="result"></param>
        public void UpdateResult(string result)
        {
            try
            {
                string[] str = result.Split(',');
                if (str[0] == "OK")
                {
                    this.BeginInvoke(new Action(() =>
                    {
                        //labelResult.Text = "OK";
                        //labelResult.BackColor = Color.FromArgb(255, 0, 192, 0);
                    }));
                }
                else
                {
                    this.BeginInvoke(new Action(() =>
                    {
                        //labelResult.Text = "NG";
                        //labelResult.BackColor = Color.FromArgb(255, 255, 0, 0);
                    }));
                }
                this.BeginInvoke(new Action(() =>
                {
                    // 限制界面列表不要太长，防止连续运行时产生内存泄露和严重的界面卡顿
                    if (listBoxResult.Items.Count > 100)
                    {
                        listBoxResult.Items.RemoveAt(0);
                    }
                    listBoxResult.Items.Add("Results: " + (result ?? "null"));

                    // 让最新的结果始终展示在最下面（自动滚动）
                    if (listBoxResult.Items.Count > 0)
                    {
                        listBoxResult.TopIndex = listBoxResult.Items.Count - 1;
                    }
                }));
            }
            catch (Exception ex)
            {
                LogFunction("Failed to update results:" + ex.ToString());
                return;
            }
        }

        /// <summary>
        /// Display picture page
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void buttonRender_Click(object sender, EventArgs e)
        {
            renderPanel.Controls.Clear();
            renderPanel.Controls.Add(renderControl);
            buttonRender.BackColor = Color.Orange;
            buttonConfig.BackColor = Color.Gray;
        }

        /// <summary>
        /// Configure parameter page
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void buttonConfig_Click(object sender, EventArgs e)
        {
            renderPanel.Controls.Clear();
            renderPanel.Controls.Add(mainViewControl);
            buttonRender.BackColor = Color.Gray;
            buttonConfig.BackColor = Color.Orange;
        }

        /// <summary>
        /// Select solution path
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private readonly string solutionFolder = AppDomain.CurrentDomain.BaseDirectory;
        private string pathCodeRecognition => Path.Combine(solutionFolder, "字符.sol");
        private string pathSizeDetection => Path.Combine(solutionFolder, "尺寸.sol");
        private string pathClassification => Path.Combine(solutionFolder, "分类.sol");
        private string pathDefectDetection => Path.Combine(solutionFolder, "缺陷.sol");
        private string pathCounting => Path.Combine(solutionFolder, "计数.sol");
        private string pathAdditionalTask => Path.Combine(solutionFolder, "附加任务.sol");

        private void LoadSolution(string solutionPath)
        {
            try
            {
                VmSolution.Load(solutionPath);
                mSolutionIsLoad = true;
                MessageBox.Show("方案加载成功：" + solutionPath);
                LogFunction(solutionPath);
            }
            catch (Exception ex)
            {
                MessageBox.Show("方案加载失败：" + ex.Message);
            }
        }

        private void LoadSolutionWithUI(string path)
        {
            string strMsg = null;
            LoadSolutionIndicateTimer.Enabled = false;
            //buttonLoadSolu.BackColor = Color.Orange;
            //buttonLoadSolu.Enabled = false;

            // Disable related buttons
            //buttonSelectSolu.Enabled = false;
            buttonRunOnce.Enabled = false;
            buttonContiRun.Enabled = false;
            buttonSaveSolu.Enabled = false;
            comboProcedure.Enabled = false;
            buttonRender.Enabled = false;
            buttonConfig.Enabled = false;

            try
            {
                if (path != null && File.Exists(path))
                {
                    VmSolution.Load(path);
                    vmSolutionPath = path;
                    vmProcessInfoList = VmSolution.Instance.GetAllProcedureList();
                    vmProcedure = VmSolution.Instance[vmProcessInfoList.astProcessInfo[0].strProcessName] as VmProcedure;

                    comboProcedure.Items.Clear();
                    for (int i = 0; i < vmProcessInfoList.nNum; i++)
                    {
                        comboProcedure.Items.Add(vmProcessInfoList.astProcessInfo[i].strProcessName);
                    }

                    if (comboProcedure.Items.Count > 0)
                    {
                        comboProcedure.SelectedIndex = 0;
                        comboProcedure.Text = comboProcedure.SelectedItem.ToString();
                    }

                    renderControl.vmRenderControl1.ModuleSource = vmProcedure;
                    mSolutionIsLoad = true;
                    strMsg = "Succeeded to load solution!";
                    LogFunction(strMsg);
                }
                else
                {
                    strMsg = "The Solution is null or file not found!";
                    LogFunction(strMsg);
                }
            }
            catch (VmException ex)
            {
                strMsg = "Failed to load solution, Error code: 0x" + Convert.ToString(ex.errorCode, 16);
                LogFunction(strMsg);
            }
            catch (Exception ex)
            {
                strMsg = "Failed to load solution: " + ex.ToString();
                LogFunction(strMsg);
            }
            finally
            {
                //buttonLoadSolu.BackColor = Color.DimGray;
                //buttonLoadSolu.Enabled = true;

                // Enable related buttons
                //buttonSelectSolu.Enabled = true;
                buttonRunOnce.Enabled = true;
                buttonContiRun.Enabled = true;
                buttonSaveSolu.Enabled = true;
                comboProcedure.Enabled = true;
                buttonRender.Enabled = true;
                buttonConfig.Enabled = true;
            }
        }


        /// <summary>
        /// Save Solution
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void buttonSaveSolu_Click(object sender, EventArgs e)
        {
            string strMsg = null;

            if (mSolutionIsLoad == true)
            {
                try
                {
                    VmSolution.Save();
                }
                catch (VmException ex)
                {
                    strMsg = "Failed to save solution, Error code: 0x" + Convert.ToString(ex.errorCode, 16);
                    LogFunction(strMsg);
                    return;
                }
                strMsg = "Succeeded to save solution!";
                LogFunction(strMsg);
            }
            else
            {
                strMsg = "Please to load the solution";
                LogFunction(strMsg);
            }
        }

        /// <summary>
        /// Obtain all processes in the solution
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void comboProcedure_DropDown(object sender, EventArgs e)
        {
            string strMsg = null;
            try
            {
                if (mSolutionIsLoad)
                {
                    comboProcedure.Items.Clear();
                    vmProcessInfoList = VmSolution.Instance.GetAllProcedureList();//Obtain all processes in the solution
                    for (int item = 0; item < vmProcessInfoList.nNum; item++)
                    {
                        comboProcedure.Items.Add(vmProcessInfoList.astProcessInfo[item].strProcessName);
                    }
                }
                else
                {
                    strMsg ="Pleaase to load solution";
                    LogFunction(strMsg);
                }
            }
            catch (VmException ex)
            {
                strMsg = "Failed to obtain all processes, Error code: 0x" + Convert.ToString(ex.errorCode, 16);
                LogFunction(strMsg);
                return;
            }
            catch (Exception ex)
            {
                strMsg = "Failed to obtain all processes!" + ex.ToString();
                LogFunction(strMsg);
                return;
            }
        }

        /// <summary>
        /// Run once
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void buttonRunOnce_Click(object sender, EventArgs e)
        {
            string strMsg = null;
            try
            {
                if (comboProcedure.Text != "")
                {
                    vmProcedure = (VmProcedure)VmSolution.Instance[comboProcedure.Text];
                    if (null == vmProcedure) return;
                    renderControl.vmRenderControl1.ModuleSource = vmProcedure;//RenderControl binding procedure
                    vmProcedure.Run();
                }
                else
                {
                    strMsg = "Please to select procedure! ";
                    LogFunction(strMsg);
                    return;
                }
            }
            catch (VmException ex)
            {
                strMsg = "Failed to run procedure once, Error code: 0x" + Convert.ToString(ex.errorCode, 16);
                LogFunction(strMsg);
                return;
            }
            catch (Exception ex)
            {
                strMsg = "Failed to run procedure once: " + ex.ToString();
                LogFunction(strMsg);
                return;
            }
        }

        /// <summary>
        /// Run Continuous
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void buttonContiRun_Click(object sender, EventArgs e)
        {
            string strMsg = null;

            try
            {
                if (comboProcedure.Text != "")
                {
                    vmProcedure = (VmProcedure)VmSolution.Instance[comboProcedure.Text];
                    if (null == vmProcedure)
                    {
                        strMsg = comboProcedure.Text + "The procedure does not exist! ";
                        LogFunction(strMsg);
                        return;
                    }
                    vmProcedure.ContinuousRunEnable = vmProcedure.ContinuousRunEnable^true;
                }
                else
                {
                    strMsg = "Please to select procedure! ";
                    LogFunction(strMsg);
                    return;
                }
            }
            catch (VmException ex)
            {
                strMsg = "Failed to run procedure continuous, Error code: 0x" + Convert.ToString(ex.errorCode, 16);
                LogFunction(strMsg);
                return;
            }
            catch (Exception ex)
            {
                strMsg = "Failed to run procedure continuous: " + ex.ToString();
                LogFunction(strMsg);
                return;
            }
        }

        /// <summary>
        /// Print Log
        /// </summary>
        /// <param name="strMsg"></param>
        public void LogFunction(string strMsg)
        {
            this.BeginInvoke(new Action(() =>
            {
                ListViewItem listViewItem = new ListViewItem();
                listViewItem.SubItems.Add("");
                listViewItem.SubItems[0].Text = DateTime.Now.ToString();
                listViewItem.SubItems[1].Text = strMsg;
                listViewLog.Items.Insert(0, listViewItem);
            }));
            SaveLog(strMsg);
        }

        /// <summary>
        /// Clear Log
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ClearToolStripMenuItem_Click(object sender, EventArgs e)
        {
            listViewLog.Items.Clear();
        }

        /// <summary>
        /// Save Log
        /// </summary>
        /// <param name="str"></param>
        private void SaveLog(string str)
        {
            Task.Run(() =>
            {
                try
                {
                    if (!Directory.Exists(logPath))
                    {
                        Directory.CreateDirectory(logPath);
                    }
                    string filename = logPath + "/" + DateTime.Now.ToString("yyyy-MM-dd") + ".log";
                    StreamWriter mySw = File.AppendText(filename);
                    mySw.WriteLine(DateTime.Now.ToString("yyyy-MM-dd hh:mm:ss::ffff\t") + str);
                    mySw.Close();
                }
                catch
                {
                    return;
                }
            });
        }

        /// <summary>
        /// Close Form
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void MainForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            if(null != vmSolutionPath && true == mSolutionIsLoad)
            {
                if (MessageBox.Show("Save solution or not?", "Information", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
                {
                    try
                    {
                        if (vmProcedure != null)
                        {
                            VmSolution.Save();
                        }
                    }
                    catch (Exception ex)
                    {
                        string strMsg = "Failed to save solution!";
                        LogFunction(strMsg);
                    }
                }
            }
        }
        /// <summary>
        ///  Switch between Chinese and English
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void buttonChineseOREnglish_Click(object sender, EventArgs e)
        {
            if (System.Threading.Thread.CurrentThread.CurrentUICulture.Name == "zh-CN")
            {
                System.Threading.Thread.CurrentThread.CurrentUICulture = new CultureInfo("en-us");
                LoadLanguage(this, typeof(MainForm));
            }
            else
            {
                System.Threading.Thread.CurrentThread.CurrentUICulture = new CultureInfo("zh-cn");
                LoadLanguage(this, typeof(MainForm));
            }
        }
        public static void LoadLanguage(Form form, Type formType)
        {
            if (form != null)
            {
                ComponentResourceManager resources = new ComponentResourceManager(formType);
                resources.ApplyResources(form, "$this");
                Loading(form, resources);
            }
        }
        private static void Loading(Control control, ComponentResourceManager resources)
        {
            if (control is ListView)
            {
                resources.ApplyResources(control, control.Name);
                ListView ts = (ListView)control;
                resources.ApplyResources(ts.Columns[0], "timeStampHeader");
                resources.ApplyResources(ts.Columns[1], "infoHeader");
                //foreach (ColumnHeader c in ts.Columns)
                //{
                //    resources.ApplyResources(c, c.Name);
                //}
            }
            foreach (Control c in control.Controls)
            {
                resources.ApplyResources(c, c.Name);
                if (c.Controls != null)
                {
                    Loading(c, resources);
                }
            }

        }

        private void listViewLog_SelectedIndexChanged(object sender, EventArgs e)
        {

        }

        private void button1_Click(object sender, EventArgs e)
        {
            LoadSolutionWithUI(pathCodeRecognition);
            //MessageBox.Show("加载方案成功！     ");
            AutoClosingMessageBox.Show("方案加载成功!", "提示", 1500); // 1秒后自动关闭

            choose = 0;
        }

        private void button2_Click(object sender, EventArgs e)
        {
            LoadSolutionWithUI(pathSizeDetection);
            //MessageBox.Show("加载方案成功！");
            AutoClosingMessageBox.Show("方案加载成功!", "提示", 1500); // 1秒后自动关闭
            choose = 1;
        }

        private void button3_Click(object sender, EventArgs e)
        {
            LoadSolutionWithUI(pathClassification);
            //MessageBox.Show("加载方案成功！");
            AutoClosingMessageBox.Show("方案加载成功!", "提示", 1500); // 1秒后自动关闭
            choose = 2;
        }


        private void button4_Click(object sender, EventArgs e)
        {
            LoadSolutionWithUI(pathDefectDetection);
            //MessageBox.Show("加载方案成功！");
            AutoClosingMessageBox.Show("方案加载成功!", "提示", 1500); // 1秒后自动关闭
            choose = 3;
        }

        private void button5_Click(object sender, EventArgs e)
        {
            LoadSolutionWithUI(pathCounting);
            AutoClosingMessageBox.Show("方案加载成功!", "提示", 1500); // 1秒后自动关闭
            choose = 4;
        }

        private void button6_Click(object sender, EventArgs e)
        {
            LoadSolutionWithUI(pathAdditionalTask);
            AutoClosingMessageBox.Show("方案加载成功!", "提示", 1500);
            choose = 5;
        }

        private void groupBox2_Enter(object sender, EventArgs e)
        {

        }

        private void listBoxResult_SelectedIndexChanged(object sender, EventArgs e)
        {

        }

        private void labelResult_Click(object sender, EventArgs e)
        {

        }
    }
}
