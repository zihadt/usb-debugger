using System;
using System.Windows.Forms;

namespace BiBEAT_DAQ
{
    public partial class DeviceConfigUI : Form
    {
        private DeviceCommunication dc;

        public event EventHandler OnHidDeviceConfigChanged;

        public DeviceConfigUI(DeviceCommunication deviceComm)
        {
            InitializeComponent();
            dc = deviceComm;
            OnHidDeviceConfigChanged += dc.DeviceHasChangedHandler;
        }

        private void Settings_Load(object sender, EventArgs e)
        {
            // Fill center info text boxes with persistent data 
            textBoxVid.Text = Properties.Settings.Default.device_vid.Trim();
            textBoxPid.Text = Properties.Settings.Default.device_pid.Trim();
            
        }

        private void buttonSaveSettings_Click(object sender, EventArgs e)
        {
            // Fill center info text boxes with persistent data 
            Properties.Settings.Default.device_vid = textBoxVid.Text;
            Properties.Settings.Default.device_pid = textBoxPid.Text;

            //save the settings
            Properties.Settings.Default.Save();
            // Update VID & PID in Device Management
            dc.Vid = Convert.ToInt32(textBoxVid.Text);
            dc.Pid = Convert.ToInt32(textBoxPid.Text);
            //Invoke Device config change event
            OnHidDeviceConfigChanged?.Invoke(this, EventArgs.Empty);

            //close the windows
            this.Close();
        }
    }
}