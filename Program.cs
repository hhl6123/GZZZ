using System;
using System.Windows.Forms;
using System.Threading;
using Common.Logging;
using ForeStar.CoreUI.Plot;
using ForeStar.Core.Exceptions;
using ForeStar.CoreUI.Message;
using ForeStar.CoreUI.Plot.Start;
using ForeStar.Core.Context;
using ForeStar.Core.Context.Support;
using ForeStar.CoreUI.Plot.Logic;

namespace ForeStar.Main.Win
{
    static class Program
    {
        private static IMessage message;
        /// <summary>
        /// 应用程序的主入口点。
        /// </summary>
        [STAThread]
        static void Main(string[] para)
        {
            try
            {
                Application.ThreadException += Application_ThreadException;
                AppDomain.CurrentDomain.UnhandledException += new UnhandledExceptionEventHandler(CurrentDomain_UnhandledException);
                Application.EnableVisualStyles();
                Application.SetCompatibleTextRenderingDefault(false);

                //当从服务登陆的时候，从命令行中获取的登录信息
                if (para != null && para.Length > 0)
                {
                    string value = para[0];
                    int index = value.IndexOf("://");
                    if (index != -1)
                    {
                        string needWord = value.Substring(index + 3);
                        if (needWord.EndsWith("/"))
                        {
                            needWord = needWord.Trim(new char[] { '/' });
                        }
                        string[] arrayStrs = needWord.Split(new char[] { ' ' });
                        if (arrayStrs.Length > 1)
                        {
                            string userName = arrayStrs[0];
                            string passWord = arrayStrs[1];
                            Core.ServiceLocator.ServiceLocatorFactory.ServiceLocator.SetInstance<string>(userName, Core.ServiceLocator.ServiceLocatorKeys.LoginUserID);
                            Core.ServiceLocator.ServiceLocatorFactory.ServiceLocator.SetInstance<string>(passWord, Core.ServiceLocator.ServiceLocatorKeys.LoginPassWord);
                        }
                    }
                }

                //SystemStart systemstart = new SystemStart();
                //PlotBuild build = new PlotBuild();
                //IPluginContent content = systemstart.ShowDialog(build);
                //if (content == null)
                //    return;
                IApplicationContext pContext = ContextRegistry.GetContext();
               

                IOpenProjectLogic open = pContext.GetObject("IOpenProjectLogic") as IOpenProjectLogic;
                if (!open.ProjectExit() && !open.OpenAccess())
                    return;

                IPlotBuild plotbuild = pContext.GetObject("IPlotBuild") as IPlotBuild;
                IPluginContent content = plotbuild.Build();
                if (content == null)
                    return;
                if (content.PluginMainForm is RibbonDevWorkPlantForm)
                {
                    (content.PluginMainForm as RibbonDevWorkPlantForm).SetWorkspace(content);
                }
                Application.Run(content.PluginMainForm as Form);
            }
            catch (Exception e)
            {
                if (e is Core.SysRegist.SysRegistException)
                {
                    ForeStar.SysReg.SysRegistForm from = new SysReg.SysRegistForm();
                    DialogResult dr = from.ShowDialog();
                    if (dr == DialogResult.OK)
                        Application.Restart();
                    else
                        Application.Exit();
                }

                try
                {
                    ILog adviceLogger = LogManager.GetLogger(AppDomain.CurrentDomain.Id.ToString());
                    adviceLogger.Error(e.Message, e);
                }
                catch { }
            }
        }

        /// <summary>
        /// 捕捉系统异常
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        static void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            UnhandleException(e.ExceptionObject as Exception);
        }

        /// <summary>
        /// 捕捉系统异常
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private static void Application_ThreadException(object sender, ThreadExceptionEventArgs e)
        {
            UnhandleException(e.Exception);
        }

        private static void UnhandleException(Exception e)
        {
            // 记录日志
            IException innerException = e as IException;
            ILog adviceLogger = LogManager.GetLogger(AppDomain.CurrentDomain.Id.ToString());

            try
            {
                if (innerException != null)
                {
                    if (innerException is Core.SysRegist.SysRegistException)
                    {
                        ForeStar.SysReg.SysRegistForm from = new SysReg.SysRegistForm();
                       DialogResult dr= from.ShowDialog();
                       if (dr == DialogResult.OK)
                           Application.Restart();
                       else
                           Application.Exit();
                    }
                    else
                    {
                        if (!innerException.IsLog)
                        {
                            adviceLogger.Error(e.Message, e);
                            innerException.IsLog = true;
                        }

                        if (!innerException.IsShow)
                        {
                            innerException.IsShow = true;
                        }
                    }
                }
                else
                {
                    adviceLogger.Error(e.Message, e);
                }
                //XtraMessageBox.Show(String.Format("原因：{0}；\n堆栈信息:{1}", e.Message, e.StackTrace));
            }
            catch (Exception ex)
            {
                adviceLogger.Error(ex.Message, ex);
                //XtraMessageBox.Show(String.Format("原因：{0}；\n堆栈信息:{1}", e.Message, e.StackTrace));
            }
        }
    }
}