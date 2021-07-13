using System.Drawing;

namespace CSpk_BootGUI
{
    partial class Form1
    {
        /// <summary>
        /// 必需的设计器变量。
        /// </summary>
        private System.ComponentModel.IContainer components = null;
      
        /// <summary>
        /// 清理所有正在使用的资源。
        /// </summary>
        /// <param name="disposing">如果应释放托管资源，为 true；否则为 false。</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows 窗体设计器生成的代码

        /// <summary>
        /// 设计器支持所需的方法 - 不要
        /// 使用代码编辑器修改此方法的内容。
        /// </summary>
        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(Form1));
            this.label_Serial = new System.Windows.Forms.Label();
            this.comboBox_Serial = new System.Windows.Forms.ComboBox();
            this.checkBox_McuUpdate = new System.Windows.Forms.CheckBox();
            this.checkBox_FlashUpdate = new System.Windows.Forms.CheckBox();
            this.checkBox_BluetoothUpdate = new System.Windows.Forms.CheckBox();
            this.progressBar_Update = new System.Windows.Forms.ProgressBar();
            this.button_Update = new System.Windows.Forms.Button();
            this.serialPort_Update = new System.IO.Ports.SerialPort(this.components);
            this.label_progressBar = new System.Windows.Forms.Label();
            this.label_UpdateStatus = new System.Windows.Forms.Label();
            this.label_VersionNumber = new System.Windows.Forms.Label();
            this.timer_UpdateDeviceScan = new System.Windows.Forms.Timer(this.components);
            this.folderBrowserDialog1 = new System.Windows.Forms.FolderBrowserDialog();
            this.pictureBox1 = new System.Windows.Forms.PictureBox();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).BeginInit();
            this.SuspendLayout();
            // 
            // label_Serial
            // 
            this.label_Serial.AutoSize = true;
            this.label_Serial.Location = new System.Drawing.Point(31, 19);
            this.label_Serial.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.label_Serial.Name = "label_Serial";
            this.label_Serial.Size = new System.Drawing.Size(43, 13);
            this.label_Serial.TabIndex = 0;
            this.label_Serial.Text = "串口号";
            // 
            // comboBox_Serial
            // 
            this.comboBox_Serial.FormattingEnabled = true;
            this.comboBox_Serial.Location = new System.Drawing.Point(98, 16);
            this.comboBox_Serial.Margin = new System.Windows.Forms.Padding(2);
            this.comboBox_Serial.Name = "comboBox_Serial";
            this.comboBox_Serial.Size = new System.Drawing.Size(92, 21);
            this.comboBox_Serial.TabIndex = 2;
            this.comboBox_Serial.DropDown += new System.EventHandler(this.comboBox_Serial_DropDown);
            // 
            // checkBox_McuUpdate
            // 
            this.checkBox_McuUpdate.AutoSize = true;
            this.checkBox_McuUpdate.Location = new System.Drawing.Point(34, 54);
            this.checkBox_McuUpdate.Margin = new System.Windows.Forms.Padding(2);
            this.checkBox_McuUpdate.Name = "checkBox_McuUpdate";
            this.checkBox_McuUpdate.Size = new System.Drawing.Size(85, 17);
            this.checkBox_McuUpdate.TabIndex = 3;
            this.checkBox_McuUpdate.Text = "Mcu Update";
            this.checkBox_McuUpdate.UseVisualStyleBackColor = true;
            this.checkBox_McuUpdate.CheckedChanged += new System.EventHandler(this.checkBox_McuUpdate_CheckedChanged);
            // 
            // checkBox_FlashUpdate
            // 
            this.checkBox_FlashUpdate.AutoSize = true;
            this.checkBox_FlashUpdate.Location = new System.Drawing.Point(137, 54);
            this.checkBox_FlashUpdate.Margin = new System.Windows.Forms.Padding(2);
            this.checkBox_FlashUpdate.Name = "checkBox_FlashUpdate";
            this.checkBox_FlashUpdate.Size = new System.Drawing.Size(89, 17);
            this.checkBox_FlashUpdate.TabIndex = 4;
            this.checkBox_FlashUpdate.Text = "Flash Update";
            this.checkBox_FlashUpdate.UseVisualStyleBackColor = true;
            this.checkBox_FlashUpdate.CheckedChanged += new System.EventHandler(this.checkBox_FlashUpdate_CheckedChanged);
            // 
            // checkBox_BluetoothUpdate
            // 
            this.checkBox_BluetoothUpdate.AutoSize = true;
            this.checkBox_BluetoothUpdate.Location = new System.Drawing.Point(248, 54);
            this.checkBox_BluetoothUpdate.Margin = new System.Windows.Forms.Padding(2);
            this.checkBox_BluetoothUpdate.Name = "checkBox_BluetoothUpdate";
            this.checkBox_BluetoothUpdate.Size = new System.Drawing.Size(109, 17);
            this.checkBox_BluetoothUpdate.TabIndex = 5;
            this.checkBox_BluetoothUpdate.Text = "Bluetooth Update";
            this.checkBox_BluetoothUpdate.UseVisualStyleBackColor = true;
            this.checkBox_BluetoothUpdate.CheckedChanged += new System.EventHandler(this.checkBox_BluetoothUpdate_CheckedChanged);
            // 
            // progressBar_Update
            // 
            this.progressBar_Update.Location = new System.Drawing.Point(43, 263);
            this.progressBar_Update.Margin = new System.Windows.Forms.Padding(2);
            this.progressBar_Update.Name = "progressBar_Update";
            this.progressBar_Update.Size = new System.Drawing.Size(397, 19);
            this.progressBar_Update.TabIndex = 6;
            // 
            // button_Update
            // 
            this.button_Update.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(89)))), ((int)(((byte)(89)))), ((int)(((byte)(89)))));
            this.button_Update.FlatAppearance.BorderSize = 0;
            this.button_Update.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.button_Update.Font = new System.Drawing.Font("Tahoma", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.button_Update.Location = new System.Drawing.Point(9, 361);
            this.button_Update.Margin = new System.Windows.Forms.Padding(0);
            this.button_Update.Name = "button_Update";
            this.button_Update.Size = new System.Drawing.Size(643, 51);
            this.button_Update.TabIndex = 7;
            this.button_Update.Text = "UPDATE NOW";
            this.button_Update.UseVisualStyleBackColor = false;
            this.button_Update.Click += new System.EventHandler(this.button_Update_Click);
            // 
            // serialPort_Update
            // 
            this.serialPort_Update.BaudRate = 3686400;
            this.serialPort_Update.ReadTimeout = 100;
            this.serialPort_Update.WriteTimeout = 100;
            this.serialPort_Update.ErrorReceived += new System.IO.Ports.SerialErrorReceivedEventHandler(this.serialPort_Update_ErrorReceived);
            this.serialPort_Update.DataReceived += new System.IO.Ports.SerialDataReceivedEventHandler(this.serialPort_Update_DataReceived);
            // 
            // label_progressBar
            // 
            this.label_progressBar.AutoSize = true;
            this.label_progressBar.BackColor = System.Drawing.Color.Transparent;
            this.label_progressBar.Location = new System.Drawing.Point(457, 263);
            this.label_progressBar.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.label_progressBar.Name = "label_progressBar";
            this.label_progressBar.Size = new System.Drawing.Size(21, 13);
            this.label_progressBar.TabIndex = 8;
            this.label_progressBar.Text = "0%";
            // 
            // label_UpdateStatus
            // 
            this.label_UpdateStatus.AutoSize = true;
            this.label_UpdateStatus.BackColor = System.Drawing.Color.Transparent;
            this.label_UpdateStatus.Location = new System.Drawing.Point(40, 99);
            this.label_UpdateStatus.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.label_UpdateStatus.Name = "label_UpdateStatus";
            this.label_UpdateStatus.Size = new System.Drawing.Size(0, 13);
            this.label_UpdateStatus.TabIndex = 10;
            // 
            // label_VersionNumber
            // 
            this.label_VersionNumber.AutoSize = true;
            this.label_VersionNumber.BackColor = System.Drawing.Color.Transparent;
            this.label_VersionNumber.Location = new System.Drawing.Point(34, 146);
            this.label_VersionNumber.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.label_VersionNumber.Name = "label_VersionNumber";
            this.label_VersionNumber.Size = new System.Drawing.Size(0, 13);
            this.label_VersionNumber.TabIndex = 11;
            // 
            // timer_UpdateDeviceScan
            // 
            this.timer_UpdateDeviceScan.Enabled = true;
            this.timer_UpdateDeviceScan.Interval = 3000;
            this.timer_UpdateDeviceScan.Tick += new System.EventHandler(this.timer_UpdateDeviceScan_Tick);
            // 
            // pictureBox1
            // 
            this.pictureBox1.Image = ((System.Drawing.Image)(resources.GetObject("pictureBox1.Image")));
            this.pictureBox1.Location = new System.Drawing.Point(387, 16);
            this.pictureBox1.Margin = new System.Windows.Forms.Padding(2);
            this.pictureBox1.Name = "pictureBox1";
            this.pictureBox1.Size = new System.Drawing.Size(75, 87);
            this.pictureBox1.SizeMode = System.Windows.Forms.PictureBoxSizeMode.Zoom;
            this.pictureBox1.TabIndex = 9;
            this.pictureBox1.TabStop = false;
            this.pictureBox1.Click += new System.EventHandler(this.pictureBox1_Click_1);
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(29)))), ((int)(((byte)(39)))), ((int)(((byte)(51)))));
            this.ClientSize = new System.Drawing.Size(661, 421);
            this.ControlBox = false;
            this.Controls.Add(this.label_VersionNumber);
            this.Controls.Add(this.label_UpdateStatus);
            this.Controls.Add(this.pictureBox1);
            this.Controls.Add(this.label_progressBar);
            this.Controls.Add(this.button_Update);
            this.Controls.Add(this.progressBar_Update);
            this.Controls.Add(this.checkBox_BluetoothUpdate);
            this.Controls.Add(this.checkBox_FlashUpdate);
            this.Controls.Add(this.checkBox_McuUpdate);
            this.Controls.Add(this.comboBox_Serial);
            this.Controls.Add(this.label_Serial);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Margin = new System.Windows.Forms.Padding(2);
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "Form1";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "CSpk_BootGUI";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.Form1_FormClosing);
            this.Load += new System.EventHandler(this.Form1_Load);
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label label_Serial;
        private System.Windows.Forms.ComboBox comboBox_Serial;
        private System.Windows.Forms.CheckBox checkBox_McuUpdate;
        private System.Windows.Forms.CheckBox checkBox_FlashUpdate;
        private System.Windows.Forms.CheckBox checkBox_BluetoothUpdate;
        private System.Windows.Forms.ProgressBar progressBar_Update;
        private System.Windows.Forms.Button button_Update;
        private System.IO.Ports.SerialPort serialPort_Update;
        private System.Windows.Forms.Label label_progressBar;
        private System.Windows.Forms.PictureBox pictureBox1;
        private System.Windows.Forms.Label label_UpdateStatus;
        private System.Windows.Forms.Label label_VersionNumber;
        private System.Windows.Forms.Timer timer_UpdateDeviceScan;
        private System.Windows.Forms.FolderBrowserDialog folderBrowserDialog1;
    }
}

