
namespace BiBEAT_DAQ
{
    partial class DaqUI
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(DaqUI));
            this.groupBox4 = new System.Windows.Forms.GroupBox();
            this.data1 = new System.Windows.Forms.TextBox();
            this.data2 = new System.Windows.Forms.TextBox();
            this.data8 = new System.Windows.Forms.TextBox();
            this.data3 = new System.Windows.Forms.TextBox();
            this.data7 = new System.Windows.Forms.TextBox();
            this.data4 = new System.Windows.Forms.TextBox();
            this.data5 = new System.Windows.Forms.TextBox();
            this.data6 = new System.Windows.Forms.TextBox();
            this.groupBox2 = new System.Windows.Forms.GroupBox();
            this.IntSendOutput = new System.Windows.Forms.Button();
            this.IntGetInput = new System.Windows.Forms.Button();
            this.usbDataBox = new System.Windows.Forms.TextBox();
            this.plotViewNcv = new OxyPlot.WindowsForms.PlotView();
            this.statusStrip1 = new System.Windows.Forms.StatusStrip();
            this.toolStripStatusDevice = new System.Windows.Forms.ToolStripStatusLabel();
            this.menuStrip1 = new System.Windows.Forms.MenuStrip();
            this.settingsToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.configureDeviceToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.groupBox4.SuspendLayout();
            this.groupBox2.SuspendLayout();
            this.statusStrip1.SuspendLayout();
            this.menuStrip1.SuspendLayout();
            this.SuspendLayout();
            // 
            // groupBox4
            // 
            this.groupBox4.Controls.Add(this.data1);
            this.groupBox4.Controls.Add(this.data2);
            this.groupBox4.Controls.Add(this.data8);
            this.groupBox4.Controls.Add(this.data3);
            this.groupBox4.Controls.Add(this.data7);
            this.groupBox4.Controls.Add(this.data4);
            this.groupBox4.Controls.Add(this.data5);
            this.groupBox4.Controls.Add(this.data6);
            this.groupBox4.Location = new System.Drawing.Point(12, 37);
            this.groupBox4.Name = "groupBox4";
            this.groupBox4.Size = new System.Drawing.Size(111, 287);
            this.groupBox4.TabIndex = 17;
            this.groupBox4.TabStop = false;
            this.groupBox4.Text = "Input ( in Decimal )";
            // 
            // data1
            // 
            this.data1.Location = new System.Drawing.Point(6, 19);
            this.data1.MaxLength = 5;
            this.data1.Name = "data1";
            this.data1.Size = new System.Drawing.Size(97, 20);
            this.data1.TabIndex = 5;
            this.data1.Text = "0";
            this.data1.KeyPress += new System.Windows.Forms.KeyPressEventHandler(this.data1_KeyPress);
            // 
            // data2
            // 
            this.data2.Enabled = false;
            this.data2.Location = new System.Drawing.Point(6, 53);
            this.data2.MaxLength = 5;
            this.data2.Name = "data2";
            this.data2.Size = new System.Drawing.Size(97, 20);
            this.data2.TabIndex = 8;
            this.data2.Text = "0";
            // 
            // data8
            // 
            this.data8.Enabled = false;
            this.data8.Location = new System.Drawing.Point(6, 257);
            this.data8.MaxLength = 1;
            this.data8.Name = "data8";
            this.data8.Size = new System.Drawing.Size(97, 20);
            this.data8.TabIndex = 14;
            this.data8.Text = "0";
            // 
            // data3
            // 
            this.data3.Enabled = false;
            this.data3.Location = new System.Drawing.Point(6, 87);
            this.data3.MaxLength = 1;
            this.data3.Name = "data3";
            this.data3.Size = new System.Drawing.Size(97, 20);
            this.data3.TabIndex = 9;
            this.data3.Text = "0";
            // 
            // data7
            // 
            this.data7.Enabled = false;
            this.data7.Location = new System.Drawing.Point(6, 223);
            this.data7.MaxLength = 1;
            this.data7.Name = "data7";
            this.data7.Size = new System.Drawing.Size(97, 20);
            this.data7.TabIndex = 13;
            this.data7.Text = "0";
            // 
            // data4
            // 
            this.data4.Enabled = false;
            this.data4.Location = new System.Drawing.Point(6, 121);
            this.data4.MaxLength = 1;
            this.data4.Name = "data4";
            this.data4.Size = new System.Drawing.Size(97, 20);
            this.data4.TabIndex = 10;
            this.data4.Text = "0";
            // 
            // data5
            // 
            this.data5.Enabled = false;
            this.data5.Location = new System.Drawing.Point(6, 155);
            this.data5.MaxLength = 1;
            this.data5.Name = "data5";
            this.data5.Size = new System.Drawing.Size(97, 20);
            this.data5.TabIndex = 11;
            this.data5.Text = "0";
            // 
            // data6
            // 
            this.data6.Enabled = false;
            this.data6.Location = new System.Drawing.Point(6, 189);
            this.data6.MaxLength = 1;
            this.data6.Name = "data6";
            this.data6.Size = new System.Drawing.Size(97, 20);
            this.data6.TabIndex = 12;
            this.data6.Text = "0";
            // 
            // groupBox2
            // 
            this.groupBox2.Controls.Add(this.IntSendOutput);
            this.groupBox2.Controls.Add(this.IntGetInput);
            this.groupBox2.Location = new System.Drawing.Point(12, 330);
            this.groupBox2.Name = "groupBox2";
            this.groupBox2.Size = new System.Drawing.Size(111, 110);
            this.groupBox2.TabIndex = 18;
            this.groupBox2.TabStop = false;
            this.groupBox2.Text = "Interrupt Transfer";
            // 
            // IntSendOutput
            // 
            this.IntSendOutput.Location = new System.Drawing.Point(6, 62);
            this.IntSendOutput.Name = "IntSendOutput";
            this.IntSendOutput.Size = new System.Drawing.Size(99, 35);
            this.IntSendOutput.TabIndex = 1;
            this.IntSendOutput.Text = "Send Output Report";
            this.IntSendOutput.UseVisualStyleBackColor = true;
            this.IntSendOutput.Click += new System.EventHandler(this.IntSendOutput_Click_1);
            // 
            // IntGetInput
            // 
            this.IntGetInput.Enabled = false;
            this.IntGetInput.Location = new System.Drawing.Point(6, 21);
            this.IntGetInput.Name = "IntGetInput";
            this.IntGetInput.Size = new System.Drawing.Size(99, 35);
            this.IntGetInput.TabIndex = 0;
            this.IntGetInput.Text = "Get Input Report";
            this.IntGetInput.UseVisualStyleBackColor = true;
            // 
            // usbDataBox
            // 
            this.usbDataBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.usbDataBox.BackColor = System.Drawing.SystemColors.ControlLightLight;
            this.usbDataBox.Location = new System.Drawing.Point(12, 446);
            this.usbDataBox.Multiline = true;
            this.usbDataBox.Name = "usbDataBox";
            this.usbDataBox.ReadOnly = true;
            this.usbDataBox.Size = new System.Drawing.Size(964, 162);
            this.usbDataBox.TabIndex = 19;
            this.usbDataBox.Text = "Accepted Input Values (In Decimal)\r\n---------------------------------------------" +
    "\r\n\r\n97   :  LED OFF\r\n98   :  LED ON\r\n99   :  LED BLINK\r\n100 :  Receive Data";
            // 
            // plotViewNcv
            // 
            this.plotViewNcv.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.plotViewNcv.BackColor = System.Drawing.SystemColors.Control;
            this.plotViewNcv.Location = new System.Drawing.Point(140, 23);
            this.plotViewNcv.Name = "plotViewNcv";
            this.plotViewNcv.PanCursor = System.Windows.Forms.Cursors.Hand;
            this.plotViewNcv.Size = new System.Drawing.Size(823, 392);
            this.plotViewNcv.TabIndex = 20;
            this.plotViewNcv.Text = "plotView1";
            this.plotViewNcv.ZoomHorizontalCursor = System.Windows.Forms.Cursors.SizeWE;
            this.plotViewNcv.ZoomRectangleCursor = System.Windows.Forms.Cursors.SizeNWSE;
            this.plotViewNcv.ZoomVerticalCursor = System.Windows.Forms.Cursors.SizeNS;
            // 
            // statusStrip1
            // 
            this.statusStrip1.BackColor = System.Drawing.SystemColors.Window;
            this.statusStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.toolStripStatusDevice});
            this.statusStrip1.Location = new System.Drawing.Point(0, 617);
            this.statusStrip1.Name = "statusStrip1";
            this.statusStrip1.Size = new System.Drawing.Size(988, 22);
            this.statusStrip1.TabIndex = 21;
            this.statusStrip1.Text = "statusStrip1";
            // 
            // toolStripStatusDevice
            // 
            this.toolStripStatusDevice.Image = global::BiBEAT_DAQ.Properties.Resources.offline;
            this.toolStripStatusDevice.Margin = new System.Windows.Forms.Padding(5, 3, 0, 2);
            this.toolStripStatusDevice.Name = "toolStripStatusDevice";
            this.toolStripStatusDevice.Size = new System.Drawing.Size(138, 17);
            this.toolStripStatusDevice.Text = "Device not connected";
            // 
            // menuStrip1
            // 
            this.menuStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.settingsToolStripMenuItem});
            this.menuStrip1.Location = new System.Drawing.Point(0, 0);
            this.menuStrip1.Name = "menuStrip1";
            this.menuStrip1.Size = new System.Drawing.Size(988, 24);
            this.menuStrip1.TabIndex = 22;
            this.menuStrip1.Text = "menuStrip1";
            // 
            // settingsToolStripMenuItem
            // 
            this.settingsToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.configureDeviceToolStripMenuItem});
            this.settingsToolStripMenuItem.Name = "settingsToolStripMenuItem";
            this.settingsToolStripMenuItem.Size = new System.Drawing.Size(61, 20);
            this.settingsToolStripMenuItem.Text = "Settings";
            // 
            // configureDeviceToolStripMenuItem
            // 
            this.configureDeviceToolStripMenuItem.Name = "configureDeviceToolStripMenuItem";
            this.configureDeviceToolStripMenuItem.Size = new System.Drawing.Size(180, 22);
            this.configureDeviceToolStripMenuItem.Text = "Configure Device";
            this.configureDeviceToolStripMenuItem.Click += new System.EventHandler(this.configureDeviceToolStripMenuItem_Click);
            // 
            // DaqUI
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(988, 639);
            this.Controls.Add(this.statusStrip1);
            this.Controls.Add(this.menuStrip1);
            this.Controls.Add(this.plotViewNcv);
            this.Controls.Add(this.usbDataBox);
            this.Controls.Add(this.groupBox2);
            this.Controls.Add(this.groupBox4);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.MainMenuStrip = this.menuStrip1;
            this.Name = "DaqUI";
            this.Text = "BiBEAT Data Acquisition Viewer";
            this.Load += new System.EventHandler(this.DaqUI_Load);
            this.groupBox4.ResumeLayout(false);
            this.groupBox4.PerformLayout();
            this.groupBox2.ResumeLayout(false);
            this.statusStrip1.ResumeLayout(false);
            this.statusStrip1.PerformLayout();
            this.menuStrip1.ResumeLayout(false);
            this.menuStrip1.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.GroupBox groupBox4;
        private System.Windows.Forms.TextBox data1;
        private System.Windows.Forms.TextBox data2;
        private System.Windows.Forms.TextBox data8;
        private System.Windows.Forms.TextBox data3;
        private System.Windows.Forms.TextBox data7;
        private System.Windows.Forms.TextBox data4;
        private System.Windows.Forms.TextBox data5;
        private System.Windows.Forms.TextBox data6;
        private System.Windows.Forms.GroupBox groupBox2;
        private System.Windows.Forms.Button IntSendOutput;
        private System.Windows.Forms.Button IntGetInput;
        private System.Windows.Forms.TextBox usbDataBox;
        private OxyPlot.WindowsForms.PlotView plotViewNcv;
        private System.Windows.Forms.StatusStrip statusStrip1;
        private System.Windows.Forms.ToolStripStatusLabel toolStripStatusDevice;
        private System.Windows.Forms.MenuStrip menuStrip1;
        private System.Windows.Forms.ToolStripMenuItem settingsToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem configureDeviceToolStripMenuItem;
    }
}

