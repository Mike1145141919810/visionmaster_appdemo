using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace DeepLearningDemoCs
{
    static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            try
            {
                Application.EnableVisualStyles();
                Application.SetCompatibleTextRenderingDefault(false);
                Application.Run(new MainForm());
            }
            catch (Exception ex)
            {
                VM.PlatformSDKCS.VmException vmEx = VM.Core.VmSolution.GetVmException(ex);
                if (null != vmEx)
                {
                    string strMsg = "InitControl failed. Error Code: " + Convert.ToString(vmEx.errorCode, 16);
                    MessageBox.Show(strMsg);
                }
                else
                {
                    return;
                }
            }
        }
    }
}
