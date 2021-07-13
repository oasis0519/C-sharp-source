using System;
using System.IO;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Threading;
using HidDfuUpdate;

using System.Text.RegularExpressions;
using System.Management;


namespace CSpk_BootGUI
{
    
    public partial class Form1 : Form
    {
        #region

        Byte MCU_UPDATE_CMD = 0;
        Byte FLASH_UPDATE_CMD = 1;
        //Byte BLUETOOTH_UPDATE_CMD = 2;

        Byte MCU_UPDATE_CMD_BEGIN = 3;
        Byte MCU_UPDATE_CMD_END = 4;

        Byte FLASH_UPDATE_CMD_BEGIN = 5;
        Byte FLASH_UPDATE_CMD_END = 6;

        Byte BLUETOOTH_UPDATE_CMD_BEGIN = 7;
        Byte BLUETOOTH_UPDATE_CMD_END = 8;

        Byte MCU_UPDATE_CMD_KEY = 9;

        Byte DEVICE_RESET_CMD = 10;

        Byte GET_DEVICE_INFO = 11;

        const Byte UpDatedeviceType = 0; //DSPK系列产品
        //const Byte UpDatedeviceType = 1; //BSPK系列产品
        //const Byte UpDatedeviceType = 2; //CSPK系列产品

        //const Byte TestMode = 1;//如果是旧boot请把这个变量设为1

        //int BOOT_JUMP_ADDR = 0x8000000;
        //int INFO_JUMP_ADDR = 0x8004000;
        int APP_JUMP_ADDR = 0x8008000;

        //int APP_LEN_INFO_ADDR = (0x8008000 + 0x1C);
        //#define FLASH_LEN_INFO_ADDR (0x8008000 + 0x20)此功能还未实现！

        int APP_FILE_MAX_SIZE = (96*1024);
        int FLASH_FILE_MAX_SIZE = (8*1024*1024);

        //int UART_BAUD_RATE = 3686400;
        // 0x7FF000 = (8*1024*1024) - 4096
        //int FLASH_UPDATE_SUCCESS_STATUS_ADDR  = 0x7FF000;

        string SerialUsbProductString;
        string UpdateFileNamePrefix = "";//升级文件名前缀
        double DeviceHWVersion = 0.0;
        double DeviceSWVersion = 0.0;

        #endregion

        HidUpdate hidFunction = new HidUpdate();
        string UpdateFileNameMcu = "";
        string UpdateFileNameFlash = "";
        string UpdateFileNameBT= "";

        int CSpk_BootGUI_UpBit = 0;
        int FileSeek = 0;
        int FileSize = 0;
        Byte[] FileBuf = null; 
        bool CSpk_BootGUI_Updating = false;//是否在升级?
        bool BTUsbDeviceOnlyExist = false;
        bool NeedGetDeviceInfo = false;
        bool SerialDeviceExist = false;
        string DeviceInfo = "";
        Byte[] DevInfoAndCrc16 = new Byte[18];

        public Form1()
        {
            InitializeComponent();
        }

        private void comboBox_Serial_DropDown(object sender, EventArgs e)
        {
            comboBox_Serial.Items.Clear();
            comboBox_Serial.Items.AddRange(System.IO.Ports.SerialPort.GetPortNames());
        }

        private void serialPort_Update_DataReceived(object sender, System.IO.Ports.SerialDataReceivedEventArgs e)
        {

        }

        public void UpdateSuccessOrFailedHandler()
        {
            hidFunction.HidDfuEnd();
            //if (serialPort_Update.IsOpen)

            //try{ serialPort_Update.Dispose(); } catch { }
            this.Invoke(new Action(() => { this.Close(); }));
            
        }

        public void CallToUpdateThread()
        {
            if ((CSpk_BootGUI_UpBit & 0x1) > 0)
            {
                //MessageBox.Show("升级Mcu");
                if (!UpdateMcu())
                {
                    UpdateSuccessOrFailedHandler();
                    return;
                }
            }
            if ((CSpk_BootGUI_UpBit & 0x2) > 0)
            {
                //MessageBox.Show("升级flash");
                if (!UpdateFlash())
                {
                    UpdateSuccessOrFailedHandler();
                    return;
                }
            }
            if ((CSpk_BootGUI_UpBit & 0x4) > 0)
            {
                //MessageBox.Show("升级bluetooth");
                if (!UpdateBluetooth())
                {
                    UpdateSuccessOrFailedHandler();
                    return;
                }
            }
            if ((CSpk_BootGUI_UpBit & 0x4) == 0)//不升级蓝牙，需要改送复位命令
            {
                if (!DeviceReset())
                {
                    UpdateSuccessOrFailedHandler();
                    
                    return;
                }
                try {
                    if (serialPort_Update.IsOpen)
                        serialPort_Update.Dispose();
                    else
                        MessageBox.Show("串口太早关闭 复位0");

                }
                catch { MessageBox.Show("串口太早关闭 复位1"); }
            }
            System.Threading.Thread.Sleep(3000); //毫秒
            MessageBox.Show("Update Success!");
            UpdateSuccessOrFailedHandler();

        }

        private void button_Update_Click(object sender, EventArgs e)
        {
            if (CSpk_BootGUI_UpBit == 0)
            {
                MessageBox.Show("Please select the software component that you want to update.");
                return;
            }
            if (SelectUpdateFileName() == false) return;
            if (CSpk_BootGUI_Updating == false)
            {
                //if (SelectUpdateFileName() == false) return;
                //if (SerialOpen() == false) return;
                ThreadStart NewThread = new ThreadStart(CallToUpdateThread);
                Thread UpdateThread = new Thread(NewThread);
                UpdateThread.Start();
                label_VersionNumber.Text = DeviceInfo;
                CSpk_BootGUI_Updating = true;
                button_Update.Enabled = false;
            }
        }

        private void checkBox_McuUpdate_CheckedChanged(object sender, EventArgs e)
        {
            if (checkBox_McuUpdate.Checked)
                CSpk_BootGUI_UpBit |= 0x01;
            else
                CSpk_BootGUI_UpBit &= ~0x01;
        }

        private void checkBox_FlashUpdate_CheckedChanged(object sender, EventArgs e)
        {
            if (checkBox_FlashUpdate.Checked)
                CSpk_BootGUI_UpBit |= 0x02;
            else
                CSpk_BootGUI_UpBit &= ~0x02;
        }

        private void checkBox_BluetoothUpdate_CheckedChanged(object sender, EventArgs e)
        {
            if (checkBox_BluetoothUpdate.Checked)
                CSpk_BootGUI_UpBit |= 0x04;
            else
                CSpk_BootGUI_UpBit &= ~0x04;
        }


        private bool UpdateMcu()
        {
            label_UpdateStatus.Invoke(new Action(() => { label_UpdateStatus.Text = "mcu Updating..."; }));
            LoadUpdateFile(UpdateFileNameMcu);
            if (!CSpk_Write(MCU_UPDATE_CMD_BEGIN, 0, 0))
            {
                MessageBox.Show("Update Mcu failed ,error code : MCU_UPDATE_CMD_BEGIN");
                return false;
            }
            if (!CSpk_SendMcuKey())
            {
                MessageBox.Show("Update Mcu failed ,error code : MCU_UPDATE_CMD_KEY");
                return false;
            }
            if (!CSpk_WriteFileToSerial(MCU_UPDATE_CMD, APP_JUMP_ADDR))
            {
                MessageBox.Show("Update Mcu failed,error code : MCU_UPDATE_CMD");
                return false;
            }
            if (!CSpk_Write(MCU_UPDATE_CMD_END, 0, 0))
            {
                MessageBox.Show("Update Mcu failed,error code : MCU_UPDATE_CMD_END");
                return false;
            }

            progressBar_Update.Invoke(new Action(() => { progressBar_Update.Value = 100; }));
            System.Threading.Thread.Sleep(1000); //毫秒
            label_progressBar.Invoke(new Action(() => { label_progressBar.Text = "100%"; }));
            System.Threading.Thread.Sleep(2000); //毫秒
            return true;
        }

        private bool UpdateFlash()
        {
            label_UpdateStatus.Invoke(new Action(() => { label_UpdateStatus.Text = "flash Updating..."; }));
            LoadUpdateFile(UpdateFileNameFlash);
            if (!CSpk_Write(FLASH_UPDATE_CMD_BEGIN, 0, 0))
            {
                MessageBox.Show("Update flash failed ,error code : FLASH_UPDATE_CMD_BEGIN");
                return false;
            }
            if (!CSpk_WriteFileToSerial(FLASH_UPDATE_CMD, 0))
            {
                MessageBox.Show("Update flash failed,error code : FLASH_UPDATE_CMD");
                return false;
            }
            if (!CSpk_Write(FLASH_UPDATE_CMD_END, 0, 0))
            {
                MessageBox.Show("Update flash failed,error code : FLASH_UPDATE_CMD_END");
                return false;
            }
            progressBar_Update.Invoke(new Action(() => { progressBar_Update.Value = 100; }));
            System.Threading.Thread.Sleep(1000); //毫秒
            label_progressBar.Invoke(new Action(() => { label_progressBar.Text = "100%"; }));
            System.Threading.Thread.Sleep(2000); //毫秒
            return true;
        }

        private bool UpdateBluetooth()
        {
            //public enum HID_PROCESS
            //{
            //    HID_PROCESS_NONE = 0,
            //    HID_PROCESS_IN_PROGRESSING,
            //    HID_PROCESS_FAILD,
            //    HID_PROCESS_SUCCESS,
            //}
            int status  = 0;
            int retry = 0;
            int  ProgressStatus = 0;

            label_UpdateStatus.Invoke(new Action(() => { label_UpdateStatus.Text = "bluetooth Updating..."; }));

            //定义一个委托
            progressBar_Update.Invoke(new Action(() => { progressBar_Update.Value = 0; }));
            //定义一个委托
            label_progressBar.Invoke(new Action(() => { label_progressBar.Text = "0%"; }));

            if (SerialDeviceExist)//存在串口设备
            {
                if (!CSpk_Write(BLUETOOTH_UPDATE_CMD_BEGIN, 0, 0))
                {
                    MessageBox.Show("Update flash Bluetooth ,error code : BLUETOOTH_UPDATE_CMD_BEGIN");
                    return false;
                }
                //关闭串口
                try
                {
                    if (serialPort_Update.IsOpen)
                        serialPort_Update.Close();
                    else
                        MessageBox.Show("串口太早关闭 蓝牙0");

                }
                catch { MessageBox.Show("串口太早关闭 蓝牙1"); }
                System.Threading.Thread.Sleep(7000); //毫秒
            }

            for (retry = 0; retry < 300; retry++)
            {
                System.Threading.Thread.Sleep(1000); //毫秒
                if (!hidFunction.deviceDetectCheck())continue;
                hidFunction.HidDfuStart();
                System.Threading.Thread.Sleep(1000); //毫秒
                hidFunction.HidDfuConfirmYes();
                status = hidFunction.HidDfuGetStatus();

                if (status == 1) break;
                hidFunction.HidDfuEnd();
            }
            if (retry == 5)
            {
                MessageBox.Show("Update Bluetooth failed,error code : Bluetooth can't enter the HID_PROCESS_IN_PROGRESSING");
                return false;
            } 
            for (int i = 0; i < 6 * 60;i++ )//6分钟
            {
                System.Threading.Thread.Sleep(1000); //毫秒
                ProgressStatus = hidFunction.HidDfuInProgressStatus();

                if (ProgressStatus <= 1) ProgressStatus = 1;

                status = hidFunction.HidDfuGetStatus();
                if (status == 2) 
                {
                    hidFunction.HidDfuEnd();
                    //MessageBox.Show(hidFunction.HidGetMoreDetails());                    
                    MessageBox.Show("Update Bluetooth failed,error code : Bluetooth into HID_PROCESS_FAILD");
                    return false;
                }
                else if (status == 4)break;

                //进度条显示
                progressBar_Update.Invoke(new Action(() => { progressBar_Update.Value = ProgressStatus ; }));
                label_progressBar.Invoke(new Action(() => { label_progressBar.Text = ProgressStatus  + "%"; }));

            }
            for (int i = 0; i < 60;i++)//60s 为了rebooting,实际只需40s
            {
                System.Threading.Thread.Sleep(1000); //毫秒
                ProgressStatus = 94 + i * 6 / 40;
                if (ProgressStatus >= 99) ProgressStatus = 99;
                status = hidFunction.HidDfuGetStatus();
                if (status == 3) break;

                //进度条显示
                progressBar_Update.Invoke(new Action(() => { progressBar_Update.Value = ProgressStatus; }));
                label_progressBar.Invoke(new Action(() => { label_progressBar.Text = ProgressStatus  + "%"; }));
            }
            if (status != 3)
            {
                MessageBox.Show("Update Bluetooth failed,error code : Bluetooth can't enter the HID_PROCESS_SUCCESS");
                return false;
            }
            //progressBar_Update.Invoke(new Action(() => { progressBar_Update.Value = 99; }));
            //label_progressBar.Invoke(new Action(() => { label_progressBar.Text = "99%"; }));

            //if (SerialDeviceExist)//存在串口设备
            //{
            //    if (SerialOpen() == false) return false;
            //    if (!CSpk_Write(BLUETOOTH_UPDATE_CMD_END, 0, 0))
            //    {
            //        MessageBox.Show("Update flash Bluetooth ,error number is 4");
            //        return false;
            //    }
            //    //关闭串口
            //    try
            //    {
            //        if (serialPort_Update.IsOpen)
            //            serialPort_Update.Close();
            //        else
            //            MessageBox.Show("串口太早关闭 蓝牙2");

            //    }
            //    catch { MessageBox.Show("串口太早关闭 蓝牙3"); }
            //}

            progressBar_Update.Invoke(new Action(() => { progressBar_Update.Value = 100; }));
            //System.Threading.Thread.Sleep(1000); //毫秒
            label_progressBar.Invoke(new Action(() => { label_progressBar.Text = "100%"; }));
            //System.Threading.Thread.Sleep(2000); //毫秒
            return true;
        }

        private bool DeviceReset()
        {
            if (!CSpk_Write(DEVICE_RESET_CMD, 0, 0))
            {
                MessageBox.Show("Reset failed ,error number is 1");
                return false;
            }
            return true;
        }

        private bool LoadUpdateFile(string FileName)
        {
            // 读取文件
            try
            {
                BinaryReader br;
                FileStream fs = new FileStream(FileName, FileMode.Open, FileAccess.Read);
                //判断文件是否正常大小
                if (FileName == UpdateFileNameMcu)
                {
                    if (fs.Length > APP_FILE_MAX_SIZE)
                    {
                        MessageBox.Show("Mcu update file size is error!");
                        return false;
                    }
                }
                else if (FileName == UpdateFileNameFlash)
                {
                    if (fs.Length > FLASH_FILE_MAX_SIZE)
                    {
                        MessageBox.Show("Flash update file size is error!");
                        return false;
                    }
                }
                FileBuf = new Byte[fs.Length + 2];
                br = new BinaryReader(fs);
                br.Read(FileBuf, 0, (int)fs.Length);
                FileSeek = 0;
                FileSize = (int)fs.Length;//最后两个字节是为了存储crc
                fs.Close();

                if (FileName == UpdateFileNameMcu)
                {
                    
                    FileBuf[0x1C] = (Byte)(FileSize & 0xFF);
                    FileBuf[0x1D] = (Byte)(FileSize >> 8);
                    FileBuf[0x1E] = (Byte)(FileSize >> 16);
                    FileBuf[0x1F] = (Byte)(FileSize >> 24);

                    ushort crc = GetCrc16(FileBuf, FileSize);
                    FileBuf[FileSize] = (Byte)(crc & 0xFF);
                    FileBuf[FileSize+1] = (Byte)(crc >> 8);
                    FileSize += 2;//2个字节为了增加CRC
                }
            }
            catch (IOException e)
            {
                MessageBox.Show(e.Message + "\n Cannot open file.");
                return false;
            }
            return true;
        }

        private bool CSpk_WriteFileToSerial(Byte cmd, int addr)
        {
            int NumByteToWrite = 4096;
            int integral = FileSize / NumByteToWrite;
            int remainder = FileSize % NumByteToWrite;

            for (int i = 0; i < integral; i++)
            {
                if (cmd == MCU_UPDATE_CMD) System.Threading.Thread.Sleep(100); //毫秒
                if (!CSpk_Write(cmd, addr, NumByteToWrite)) return false;
                addr += NumByteToWrite;
                FileSeek += NumByteToWrite;
            }
            if (remainder > 0)
            {
                if (cmd == MCU_UPDATE_CMD) System.Threading.Thread.Sleep(100); //毫秒
                if (!CSpk_Write(cmd, addr, remainder)) return false;
                addr += remainder;
                FileSeek += remainder;
            }
            return true;
        }

        private bool CSpk_SendMcuKey()
        {
           return  CSpk_Write(MCU_UPDATE_CMD_KEY, 0, 8);
        }

        private bool CSpk_Write(Byte cmd, int addr, int dataLen)
        {
            Byte[] data = DataPackage(cmd, addr, dataLen);
            if(DataSend(data, 20 + dataLen))return true;
            return false;
        }

        private bool DataSend(Byte[] data,int size)
        {
            //串口重试5次
            for (int retry = 0; retry < 5; retry++)
            {
                try 
                { 
                    serialPort_Update.Write(data, 0, size);
                    Byte[] crc = new Byte[20];
                    for (int j = 0; j < 1000;j++ )
                    {
                        if (serialPort_Update.BytesToRead != 0) break;
                        System.Threading.Thread.Sleep(1); //毫秒
                    }
                    if (serialPort_Update.BytesToRead == 0) return false;
                    if (NeedGetDeviceInfo == true)
                    {
                        int len;
                        len = serialPort_Update.BytesToRead;
                        serialPort_Update.Read(DevInfoAndCrc16, 0, serialPort_Update.BytesToRead);
                        if ((DevInfoAndCrc16[16] == data[2]) && (DevInfoAndCrc16[17] == data[3]) && (len == 18))
                        {
                            NeedGetDeviceInfo = false;//设备信息获取成功
                            return true;
                        }    
                    }
                    else
                    {
                        serialPort_Update.Read(crc, 0, serialPort_Update.BytesToRead);
                        if ((crc[0] == data[2]) && (crc[1] == data[3]))
                            return true;
                    } 
                }
                catch {
                    //MessageBox.Show("串口异常！");
                    //serialPort_Update.Close();
                    //serialPort_Update.Dispose();
                    return false;
                
                }
            }
            return false;
        }

        private Byte[] DataPackage(Byte cmd, int addr, int dataLen)
        {
            Byte[] buf = new Byte[20 + dataLen];
            #region
            /*
                typedef struct
                {
                    uint8_t  cmd;
                    uint8_t  dmc;
                    uint16_t crc;
                    uint32_t len;
                    uint32_t addr;
                    uint32_t sum;
                    uint32_t per;

                }UpdateCmd_t;
             */
            #endregion
            int per = 0;
            ushort crc = 0;
            

            buf[0] = cmd;
            buf[1] = (Byte)(cmd ^ 0xFF);
            if (dataLen > 0)
            {
                Byte[] data = new Byte[dataLen];
                int tmpSeek = FileSeek;

                for (int i = 0; i < dataLen;i++)
                    data[i] = FileBuf[tmpSeek++];

                if (cmd == MCU_UPDATE_CMD_KEY)
                {
                    //#define UNLOCK_KEY0                ((uint32_t)0x45670123U)  /*!< unlock key 0 */
                    //#define UNLOCK_KEY1                ((uint32_t)0xCDEF89ABU)  /*!< unlock key 1 */
                    //0x45670123CDEF89AB
                    Byte[] key = { 0xAB, 0x89, 0xEF, 0xCD,0x23, 0x01, 0x67, 0x45 };
                    key.CopyTo(data, 0);
                }

                crc = GetCrc16(data, dataLen);
                buf[2] = (Byte)(crc & 0xFF);
                buf[3] = (Byte)(crc >> 8);

                buf[4] = (Byte)(dataLen & 0xFF);
                buf[5] = (Byte)(dataLen >> 8);
                buf[6] = (Byte)(dataLen >> 16);
                buf[7] = (Byte)(dataLen >> 24);

                buf[8] = (Byte)(addr & 0xFF);
                buf[9] = (Byte)(addr >> 8);
                buf[10] = (Byte)(addr >> 16);
                buf[11] = (Byte)(addr >> 24);

                data.CopyTo(buf, 20);
            }
            else
            {
                buf[2] = 0;
                buf[3] = 0;

                buf[4] = 0;
                buf[5] = 0;
                buf[6] = 0;
                buf[7] = 0;

                buf[8] = 0;
                buf[9] = 0;
                buf[10] = 0;
                buf[11] = 0;

            }
            if ((cmd == MCU_UPDATE_CMD) || (cmd == FLASH_UPDATE_CMD) || (cmd == MCU_UPDATE_CMD_END) || (cmd == FLASH_UPDATE_CMD_END))
            {
                //进度条显示
                progressBar_Update.Invoke(new Action(() => { progressBar_Update.Value = FileSeek * 99 / FileSize; }));
                label_progressBar.Invoke(new Action(() => { label_progressBar.Text = (FileSeek * 99 / FileSize) + "%"; }));

                per = FileSeek * 255 / FileSize;
            }
            if (cmd == GET_DEVICE_INFO)
                NeedGetDeviceInfo = true;

           
            int sum = cmd + crc + dataLen + addr + per;
            buf[12] = (Byte)(sum & 0xFF);
            buf[13] = (Byte)(sum >> 8);
            buf[14] = (Byte)(sum >> 16);
            buf[15] = (Byte)(sum >> 24);

            buf[16] = (Byte)(per & 0xFF);
            buf[17] = (Byte)(per >> 8);
            buf[18] = (Byte)(per >> 16);
            buf[19] = (Byte)(per >> 24);

            return buf;
        }
        private ushort GetCrc16(Byte[] q, int len)
        {
            ushort crc = 0;
            for (int i = 0; i < len; i++)
                crc = (ushort)(ccitt_table[(crc >> 8 ^ q[i]) & 0xff] ^ (crc << 8));
            return crc;
        }
        #region
        ushort[] ccitt_table = 
        {
	        0x0000, 0x1021, 0x2042, 0x3063, 0x4084, 0x50A5, 0x60C6, 0x70E7,
	        0x8108, 0x9129, 0xA14A, 0xB16B, 0xC18C, 0xD1AD, 0xE1CE, 0xF1EF,
	        0x1231, 0x0210, 0x3273, 0x2252, 0x52B5, 0x4294, 0x72F7, 0x62D6,
	        0x9339, 0x8318, 0xB37B, 0xA35A, 0xD3BD, 0xC39C, 0xF3FF, 0xE3DE,
	        0x2462, 0x3443, 0x0420, 0x1401, 0x64E6, 0x74C7, 0x44A4, 0x5485,
	        0xA56A, 0xB54B, 0x8528, 0x9509, 0xE5EE, 0xF5CF, 0xC5AC, 0xD58D,
	        0x3653, 0x2672, 0x1611, 0x0630, 0x76D7, 0x66F6, 0x5695, 0x46B4,
	        0xB75B, 0xA77A, 0x9719, 0x8738, 0xF7DF, 0xE7FE, 0xD79D, 0xC7BC,
	        0x48C4, 0x58E5, 0x6886, 0x78A7, 0x0840, 0x1861, 0x2802, 0x3823,
	        0xC9CC, 0xD9ED, 0xE98E, 0xF9AF, 0x8948, 0x9969, 0xA90A, 0xB92B,
	        0x5AF5, 0x4AD4, 0x7AB7, 0x6A96, 0x1A71, 0x0A50, 0x3A33, 0x2A12,
	        0xDBFD, 0xCBDC, 0xFBBF, 0xEB9E, 0x9B79, 0x8B58, 0xBB3B, 0xAB1A,
	        0x6CA6, 0x7C87, 0x4CE4, 0x5CC5, 0x2C22, 0x3C03, 0x0C60, 0x1C41,
	        0xEDAE, 0xFD8F, 0xCDEC, 0xDDCD, 0xAD2A, 0xBD0B, 0x8D68, 0x9D49,
	        0x7E97, 0x6EB6, 0x5ED5, 0x4EF4, 0x3E13, 0x2E32, 0x1E51, 0x0E70,
	        0xFF9F, 0xEFBE, 0xDFDD, 0xCFFC, 0xBF1B, 0xAF3A, 0x9F59, 0x8F78,
	        0x9188, 0x81A9, 0xB1CA, 0xA1EB, 0xD10C, 0xC12D, 0xF14E, 0xE16F,
	        0x1080, 0x00A1, 0x30C2, 0x20E3, 0x5004, 0x4025, 0x7046, 0x6067,
	        0x83B9, 0x9398, 0xA3FB, 0xB3DA, 0xC33D, 0xD31C, 0xE37F, 0xF35E,
	        0x02B1, 0x1290, 0x22F3, 0x32D2, 0x4235, 0x5214, 0x6277, 0x7256,
	        0xB5EA, 0xA5CB, 0x95A8, 0x8589, 0xF56E, 0xE54F, 0xD52C, 0xC50D,
	        0x34E2, 0x24C3, 0x14A0, 0x0481, 0x7466, 0x6447, 0x5424, 0x4405,
	        0xA7DB, 0xB7FA, 0x8799, 0x97B8, 0xE75F, 0xF77E, 0xC71D, 0xD73C,
	        0x26D3, 0x36F2, 0x0691, 0x16B0, 0x6657, 0x7676, 0x4615, 0x5634,
	        0xD94C, 0xC96D, 0xF90E, 0xE92F, 0x99C8, 0x89E9, 0xB98A, 0xA9AB,
	        0x5844, 0x4865, 0x7806, 0x6827, 0x18C0, 0x08E1, 0x3882, 0x28A3,
	        0xCB7D, 0xDB5C, 0xEB3F, 0xFB1E, 0x8BF9, 0x9BD8, 0xABBB, 0xBB9A,
	        0x4A75, 0x5A54, 0x6A37, 0x7A16, 0x0AF1, 0x1AD0, 0x2AB3, 0x3A92,
	        0xFD2E, 0xED0F, 0xDD6C, 0xCD4D, 0xBDAA, 0xAD8B, 0x9DE8, 0x8DC9,
	        0x7C26, 0x6C07, 0x5C64, 0x4C45, 0x3CA2, 0x2C83, 0x1CE0, 0x0CC1,
	        0xEF1F, 0xFF3E, 0xCF5D, 0xDF7C, 0xAF9B, 0xBFBA, 0x8FD9, 0x9FF8,
	        0x6E17, 0x7E36, 0x4E55, 0x5E74, 0x2E93, 0x3EB2, 0x0ED1, 0x1EF0
        };
        #endregion

        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (serialPort_Update.IsOpen)
                serialPort_Update.Close();
            hidFunction.HidDfuEnd();
 
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            SerialUsbProductString = GetProductString();
            USB_Scan();
           
        }

        private bool SerialOpen()
        {
            try
            {
                comboBox_Serial.Invoke(new Action(() => { serialPort_Update.PortName = comboBox_Serial.Text; }));
                serialPort_Update.Open();
                //serialPort_Update.DiscardOutBuffer();
                //serialPort_Update.DiscardInBuffer();
            }
            catch
            {
                MessageBox.Show("open com port failed, please select the correct com port.");
                return false;
            }
             return true;
        }

        private void pictureBox1_Click_1(object sender, EventArgs e)
        {

        }

        private void serialPort_Update_ErrorReceived(object sender, System.IO.Ports.SerialErrorReceivedEventArgs e)
        {
            //serialPort_Update_Error = true;
        }

        private void timer_UpdateDeviceScan_Tick(object sender, EventArgs e)
        {
            if (CSpk_BootGUI_Updating == true) return;
            USB_Scan();
            ThreadStart NewThread = new ThreadStart(UpdateDeviceScan);
            Thread Thread1 = new Thread(NewThread);
            Thread1.Start();
        }

        private void UpdateDeviceScan()
        {
            SerialUsbProductString = GetProductString();
        }
        private string GetProductString()
        {
            string ProductString = "";
            string PNPDeviceID = "";
            try
            {
                PNPDeviceID = GetPNPDeviceID("\"USB\\\\VID_0A12&PID_1243\\\\CSPKDS10H1.1\"");
                if (PNPDeviceID == "")
                    PNPDeviceID = GetPNPDeviceID("\"USB\\\\VID_28E9&PID_018A\\\\DSUPD\"");


                switch (PNPDeviceID)
                {
                    case "USB\\VID_0A12&PID_1243\\CSPKDS24":
                        ProductString = "";
                        break;
                    case "USB\\VID_0A12&PID_1243\\CSPKDS10H1.1":
                        DeviceInfo = "CSPKDS10H1.1";
                        ProductString = "CSPKDS10";
                        break;
                    case "USB\\VID_28E9&PID_018A\\DSUPD":
                        ProductString = "DSUPD";
                        break;
                    default:
                        break;
                }
                //ProductString = Regex.Match(Regex.Match(PNPDeviceID, @"\\DSUPD").Value, @"DSUPD").Value;
                //ProductString = Regex.Match(Regex.Match(PNPDeviceID, @"\\CSPKDS10").Value, @"CSPKDS10").Value;
            }
            catch { }

            return ProductString;
        }

        // "\"USB\\\\VID_0A12&PID_1243\\\\CSPKDS24\""
        private string GetPNPDeviceID(string id)
        {
            string PNPDeviceID = "";
            ManagementObjectCollection PnPEntityCollection = new ManagementObjectSearcher("SELECT * FROM Win32_PnPEntity WHERE DeviceID=" + id).Get();
            if (PnPEntityCollection != null)
            {
                foreach (ManagementObject Entity in PnPEntityCollection)
                {
                    PNPDeviceID = Entity["PNPDeviceID"] as String;  // 设备ID
                }
            }
            return PNPDeviceID;
        }

        private void USB_Scan()
        {
            if (CSpk_BootGUI_Updating == true) return;
            /*
             读取usb 串口的ProductString，如果读取失败，说明升级设备没有串口或设备不存在
             */
            //串口 vid 0x28E9 pid 0x018A
            //SerialUsbProductString = GetProductString();

            if (SerialUsbProductString == "DSUPD")
            {
                label_UpdateStatus.Invoke(new Action(() => { label_UpdateStatus.Text = ""; }));
                SerialDeviceExist = true;

                checkBox_BluetoothUpdate.Invoke(new Action(() => { checkBox_BluetoothUpdate.Enabled = true; }));
                checkBox_McuUpdate.Invoke(new Action(() => { checkBox_McuUpdate.Enabled = true; }));
                checkBox_FlashUpdate.Invoke(new Action(() => { checkBox_FlashUpdate.Enabled = true; }));
                comboBox_Serial.Invoke(new Action(() => { comboBox_Serial.Enabled = true; }));
                button_Update.Invoke(new Action(() => { button_Update.Enabled = true; }));
                label_UpdateStatus.Invoke(new Action(() => { label_UpdateStatus.Text = "Accpeted update device is found"; }));
            }
            else
            {
                //蓝牙 vid 0x0A12 pid 0x1243
                if (SerialUsbProductString == "CSPKDS10")
                {
                    label_UpdateStatus.Invoke(new Action(() => { label_UpdateStatus.Text = ""; }));
                    BTUsbDeviceOnlyExist = true;
                    checkBox_BluetoothUpdate.Invoke(new Action(() => { checkBox_BluetoothUpdate.Enabled = true; }));
                    checkBox_McuUpdate.Invoke(new Action(() => { checkBox_McuUpdate.Enabled = false; }));
                    checkBox_FlashUpdate.Invoke(new Action(() => { checkBox_FlashUpdate.Enabled = false; }));
                    comboBox_Serial.Invoke(new Action(() => { comboBox_Serial.Enabled = false; }));
                    button_Update.Invoke(new Action(() => { button_Update.Enabled = true; }));
                    label_UpdateStatus.Invoke(new Action(() => { label_UpdateStatus.Text = "Accpeted update device is found"; }));
                }

                else
                {
                    SerialDeviceExist = false;
                    BTUsbDeviceOnlyExist = false;
                    label_UpdateStatus.Invoke(new Action(() => { label_UpdateStatus.Text = "Accpeted update device is not found"; }));
                    checkBox_BluetoothUpdate.Invoke(new Action(() => { checkBox_BluetoothUpdate.Enabled = false; }));
                    checkBox_McuUpdate.Invoke(new Action(() => { checkBox_McuUpdate.Enabled = false; }));
                    checkBox_FlashUpdate.Invoke(new Action(() => { checkBox_FlashUpdate.Enabled = false; }));
                    comboBox_Serial.Invoke(new Action(() => { comboBox_Serial.Enabled = false; }));
                    button_Update.Invoke(new Action(() => { button_Update.Enabled = false; }));

                }
            }
        }
        private bool SelectUpdateFileName()
        {

            if (SerialDeviceExist)//串口设备存在
            {
                #region
                if (SerialOpen() == false) return false;
                if (!CSpk_Write(GET_DEVICE_INFO, 0, 0))
                {
                    //DevInfoAndCrc16
                    MessageBox.Show("GET_DEVICE_INFO ,error number is 1");
                    return false;
                }
                DeviceInfo = System.Text.Encoding.Default.GetString(DevInfoAndCrc16);
                //MessageBox.Show(DeviceInfo);
                /*
                    string UpdateFileNamePrefix = "";//升级文件名前缀
                    double DeviceHWVersion = 0.0;
                    double DeviceSWVersion = 0.0;
                 
                 */
                try
                {
                    //label_VersionNumber.Text = DeviceInfo;
                    UpdateFileNamePrefix = DeviceInfo.Substring(0, 8);
                    DeviceHWVersion = Convert.ToDouble(DeviceInfo.Substring(9, 3));


                    if (UpdateFileNamePrefix == "CSPKDS24")
                    {
                        string DeviceHWVersionString = DeviceInfo.Substring(8, 4);
                        switch (DeviceHWVersionString)
                        {
                            case "H2.1":
                            case "H2.3":
                            case "H2.4":
                            case "H2.5":
                            case "H1.1":
                            case "H1.2":
                            case "H1.3":
                            case "H1.4":
                            case "H1.5":
                            case "H5.1":
                            case "H5.2":
                            case "H5.3":
                            case "H5.4":
                            case "H5.5":
                                break;
                            default:
                                MessageBox.Show("mcu hw version error");
                                return false;
                        }
                    }
                    else if (UpdateFileNamePrefix == "CSPKDS16")
                    {
                        string DeviceHWVersionString = DeviceInfo.Substring(8, 4);
                        switch (DeviceHWVersionString)
                        {
                            case "H2.2":
                            case "H3.1":
                            case "H3.2":
                            case "H3.3":
                            case "H3.4":
                            case "H3.5":
                            case "H4.1":
                            case "H4.2":
                            case "H4.3":
                            case "H4.4":
                            case "H4.5":
                                break;
                            default:
                                MessageBox.Show("mcu hw version error");
                                return false;
                        }
                    }
                    DeviceSWVersion = Convert.ToDouble(DeviceInfo.Substring(13, 3));

                    /*********************************************************************************************************************/
                    string[] UpdateFileNameMcuList = Directory.GetFiles(".", UpdateFileNamePrefix + "WriteMcu" + "*.bin");
                    if (UpdateFileNameMcuList.Length > 0)
                    {
                        UpdateFileNameMcu = UpdateFileNameMcuList[0];
                        foreach (string FileName in UpdateFileNameMcuList)
                            if (string.Compare(UpdateFileNameMcu, FileName, true) == -1)
                                UpdateFileNameMcu = FileName;
                    }
                    else
                    {
                        MessageBox.Show("not find mcu update file");
                        return false;
                    }
                    double McuSWVersion = Convert.ToDouble(Regex.Match(UpdateFileNameMcu, @"[0-9]{1}\.[0-9]{1}").Value);
                    if (DeviceSWVersion > McuSWVersion)
                    {
                        MessageBox.Show("mcu update file version is same or low");
                        return false;
                    }
                    /*********************************************************************************************************************/
                    string[] UpdateFileNameFlashList = Directory.GetFiles(".", UpdateFileNamePrefix + "WriteFlash" + "*.bin");
                    if (UpdateFileNameFlashList.Length > 0)
                    {
                        UpdateFileNameFlash = UpdateFileNameFlashList[0];
                        foreach (string FileName in UpdateFileNameFlashList)
                            if (string.Compare(UpdateFileNameFlash, FileName, true) == -1)
                                UpdateFileNameFlash = FileName;
                    }
                    else
                    {
                        MessageBox.Show("not find flash update file");
                        return false;
                    }

                    double FlashSWVersion = Convert.ToDouble(Regex.Match(UpdateFileNameFlash, @"[0-9]{1}\.[0-9]{1}").Value);
                    if (DeviceSWVersion > FlashSWVersion)
                    {
                        MessageBox.Show("flash update file version is same or low");
                        return false;
                    }
                    /*********************************************************************************************************************/
                    string[] UpdateFileNameBTList = Directory.GetFiles(".", UpdateFileNamePrefix + "WriteBT" + "*.bin");
                    if (UpdateFileNameBTList.Length > 0)
                    {
                        UpdateFileNameBT = UpdateFileNameBTList[0];
                        foreach (string FileName in UpdateFileNameBTList)
                            if (string.Compare(UpdateFileNameBT, FileName, true) == -1)
                                UpdateFileNameBT = FileName;
                    }
                    else
                    {
                        MessageBox.Show("not find bluetooth update file");
                        return false;
                    }
                    double BTSWVersion = Convert.ToDouble(Regex.Match(UpdateFileNameBT, @"[0-9]{1}\.[0-9]{1}").Value);
                    if (DeviceSWVersion > BTSWVersion)
                    {
                        MessageBox.Show("bluetooth update file version is same or low");
                        return false;
                    }

                }
                catch
                {
                    try
                    {
                        // 创建一个 StreamReader 的实例来读取文件 
                        // using 语句也能关闭 StreamReader
                        using (StreamReader sr = new StreamReader("UpdateFile.ini"))
                        {
                            UpdateFileNameMcu = sr.ReadLine();
                            UpdateFileNameFlash = sr.ReadLine();
                            UpdateFileNameBT = sr.ReadLine();
                        }
                    }
                    catch
                    {
                        // 向用户显示出错消息
                        MessageBox.Show("UpdateFile no find");
                    }
                }

                //copy to CubicSpeaker_3007_upg_signed.bin
                if (File.Exists(UpdateFileNameBT))//必须判断要复制的文件是否存在
                {
                    File.Copy(UpdateFileNameBT, "CubicSpeaker_3007_upg_signed.bin", true);//三个参数分别是源文件路径，存储路径，若存储路径有相同文件是否替换
                }
                #endregion
            }
            else
            {
                if (BTUsbDeviceOnlyExist)//只存在蓝牙设备
                {
                    if (UpDatedeviceType == 0)
                        UpdateFileNamePrefix = "CSPKDS10";
                    string[] UpdateFileNameBTList = Directory.GetFiles(".", "UpdateFileNamePrefix" + "WriteBT" + "*.bin");
                    if (UpdateFileNameBTList.Length > 0)
                    {
                        UpdateFileNameBT = UpdateFileNameBTList[0];
                        foreach (string FileName in UpdateFileNameBTList)
                            if (string.Compare(UpdateFileNameBT, FileName, true) == -1)
                                UpdateFileNameBT = FileName;
                    }
                    else
                    {
                        MessageBox.Show("not find bluetooth update file");
                        return false;
                    }
                    //copy to CubicSpeaker_3007_upg_signed.bin
                    if (File.Exists(UpdateFileNameBT))//必须判断要复制的文件是否存在
                    {
                        File.Copy(UpdateFileNameBT, "CubicSpeaker_3007_upg_signed.bin", true);//三个参数分别是源文件路径，存储路径，若存储路径有相同文件是否替换
                    }
                }
                else
                {
                    label_UpdateStatus.Text = "no find update device";
                    return false;
                }
            }

            switch (UpdateFileNamePrefix)
            {
                case "CSPKDS24":
                    pictureBox1.Image = Image.FromFile("CSPKDS24.png");
                    break;
                case "CSPKDS16":
                    pictureBox1.Image = Image.FromFile("CSPKDS16.png");
                    break;
                case "CSPKDS10":
                    pictureBox1.Image = Image.FromFile("CSPKDS10.png");
                    break;
                default:
                    pictureBox1.Image = Image.FromFile("1.png");
                    break;
            }
            return true;
        }

        private void progressBar1_Click(object sender, EventArgs e)
        {

        }

        private void groupBox1_Enter(object sender, EventArgs e)
        {

        }

        private void richTextBox1_TextChanged(object sender, EventArgs e)
        {

        }

        private void textBox1_TextChanged(object sender, EventArgs e)
        {

        }

        private void label1_Click(object sender, EventArgs e)
        {

        }

        private void pictureBox2_Click(object sender, EventArgs e)
        {
            checkBox_McuUpdate.Checked = true;
        }
    }
}
