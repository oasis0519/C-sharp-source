using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
using System.Runtime.InteropServices;
using System.Diagnostics;
using HidDfuAPI;

namespace HidDfuUpdate
{
    public class HidUpdate
    {
        public Process process = null;
        string outputDetail = null;
        string outLastLine = null;
        bool ResultForStart = false;
        byte ResultOfDfu = (byte)HID_PROCESS.HID_PROCESS_NONE;
        bool ResultRefreshLock = false;
        byte InProgressStatus = 0;

        private static Thread hidBurnProcess;
        public bool deviceDetectCheck()
        {
            ushort count = 0;
            //bool isDetected = false;

            int retVal = HidDfu.hidDfuConnect((ushort)0x0a12, (ushort)0x1243, 0, (ushort)0xff00, out count);


            if (retVal == (int)HID_STATUS.HIDDFU_ERROR_NONE)
            {
                //MessageBox.Show(count.ToString());
                //stbDisplay.Append("Found " + count.ToString() + " device.\r\n");
                //refreshTheDisplay();
            }
            //Disconnect
            int disconnectResult = HidDfu.hidDfuDisconnect();
            if (retVal == (int)HID_STATUS.HIDDFU_ERROR_NONE && disconnectResult != (int)HID_STATUS.HIDDFU_ERROR_NONE)
            {
                retVal = disconnectResult;
            }

            return (retVal == (int)HID_STATUS.HIDDFU_ERROR_NONE && disconnectResult == (int)HID_STATUS.HIDDFU_ERROR_NONE);
        }

        private void burnProcessTask(object obj)
        {
            process = new Process();

            //process.StartInfo.FileName = "cmd.exe";
            string str = System.IO.Directory.GetCurrentDirectory() + "\\HidDfuCmd.exe";
            process.StartInfo.FileName = @str;
            process.StartInfo.WorkingDirectory = ".";
            process.StartInfo.UseShellExecute = false;
            process.StartInfo.RedirectStandardInput = true;
            process.StartInfo.RedirectStandardOutput = true;
            process.StartInfo.CreateNoWindow = true;
            //Process.Start("cmd.exe");
            process.OutputDataReceived += new DataReceivedEventHandler(OutputHandler);

            string fileText = "upgradebin 0A12 1243 0 FF00 CubicSpeaker_3007_upg_signed.bin";
            process.StartInfo.Arguments = fileText;

            process.Start();

            //process.StandardInput.WriteLine("HidDfuCmd.exe upgradebin 0A12 1243 0 FF00 CubicSpeaker_3007_upg_signed.bin");

            //process.StandardInput.WriteLine("exit");

            process.BeginOutputReadLine();
            //using (StreamWriter sw = new StreamWriter("output.log"))
            //{
            // SW.WriteLine(process.StandardOutput.ReadToEnd());
            //}
        }

        private void OutputHandler(object sendingProcess, DataReceivedEventArgs outLine)
        {
            if (!String.IsNullOrEmpty(outLine.Data))
            {
                StringBuilder sb = new StringBuilder(outputDetail);
                outputDetail = sb.AppendLine(outLine.Data).ToString();
                outLastLine = outLine.Data;

                //this.textBoxTest.SelectionStart = this.textBoxTest.Text.Length;
                //this.textBoxTest.ScrollToCaret();
                if (outLastLine.Contains("Error"))
                {
                    ResultForStart = false;
                    ResultOfDfu = (byte)HID_PROCESS.HID_PROCESS_FAILD;
                }
                else
                {
                    ResultForStart = true;

                    if (ResultRefreshLock == false)
                        ResultOfDfu = (byte)HID_PROCESS.HID_PROCESS_IN_PROGRESSING;
                }

                if (outLastLine.Contains("upgradebin succeeded"))
                {
                    ResultOfDfu = (byte)HID_PROCESS.HID_PROCESS_SUCCESS;
                    ResultRefreshLock = true;
                }

                if (outLastLine.Contains("rebooting"))
                {
                    ResultOfDfu = (byte)HID_PROCESS.HID_PROCESS_REBOOTING;
                }

                if (outLastLine.Contains("%"))
                {
                    //InProgressStatus
                    string temp = outLastLine.Substring(0, outLastLine.IndexOf("%"));
                    InProgressStatus = Convert.ToByte(temp,10);
                }
            }
        }

        public void HidDfuStart()
        {
            hidBurnProcess = new Thread(new ParameterizedThreadStart(burnProcessTask));
            hidBurnProcess.Name = "hidBurn";
            hidBurnProcess.Start();
            ResultOfDfu = (byte)HID_PROCESS.HID_PROCESS_NONE;
            ResultRefreshLock = false;
            outputDetail = " ";
            InProgressStatus = 0;
        }

        public void HidDfuConfirmYes()
        {
            if (HidDfuCouldStart())
            {
                process.StandardInput.WriteLine("y");

                if(ResultRefreshLock == false)
                    ResultOfDfu = (byte)HID_PROCESS.HID_PROCESS_IN_PROGRESSING;
            }
            else
            {
                outLastLine = "Can't be start";
                ResultOfDfu = (byte)HID_PROCESS.HID_PROCESS_FAILD;
            }
        }

        public void HidDfuConfirmNo()
        {
            process.StandardInput.WriteLine("n");
        }

        public string HidGetLastOutput()
        {
            //if (outLastLine.Contains("Error"))
            //{
            //    ResultForStart = false;
            //    ResultOfDfu = (byte)HID_PROCESS.HID_PROCESS_FAILD;
            //}
            //else
            //{
            //    ResultForStart = true;
            //    ResultOfDfu = (byte)HID_PROCESS.HID_PROCESS_NONE;
            //}

            //if (outLastLine.Contains("upgradebin succeed"))
            //{
            //    ResultOfDfu = (byte)HID_PROCESS.HID_PROCESS_SUCCESS;
            //}

            //if (outLastLine.Contains("%"))
            //{

            //}
            return outLastLine;
        }

        public string HidGetMoreDetails()
        {
            return outputDetail;
        }

        public bool HidDfuCouldStart()
        {
            return ResultForStart;
        }

        public byte HidDfuGetStatus()
        {
            return ResultOfDfu;
        }

        public byte HidDfuInProgressStatus()
        {
            return InProgressStatus;
        }

        public void HidDfuEnd()
        {
            if (process != null)
            {
                if (!process.HasExited)
                {
                    process.Kill();
                }
            }

            outputDetail = " ";
        }
    }
}
