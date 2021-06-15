using System;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading;
using System.Windows.Forms;
using OxyPlot;
using OxyPlot.Axes;
using OxyPlot.Series;

namespace BiBEAT_DAQ
{
    public partial class DaqUI : Form
    {
        private LineSeries _ls;
        private PlotModel _plotModel;
        private static bool _deviceConnected = false;
        private Thread hostDeviceComm;

        private delegate void SafeCallDelegate(string text);

        private DeviceCommunication _deviceComm;
        

        private enum FormActions
        {
            EnableSendDataButton,
            DisableSendDataButton
        };

        internal enum DeviceActions
        {
            GetInterruptReport = 0,
            SendInterruptReport = 1,
            StayOnHold = 2
        };

        private DeviceActions deviceAction = DeviceActions.StayOnHold;


        //  This delegate has the same parameters as AccessForm.
        //  Used in accessing the application's form from a different thread.

        private delegate void MarshalDataToForm(FormActions action, String textToAdd);

        public DaqUI()
        {
            InitializeComponent();
            Startup();
        }

        ///  <summary>
        ///  Performs various application-specific functions that
        ///  involve accessing the application's form.
        ///  </summary>
        ///  
        ///  <param name="action"> a FormActions member that names the action to perform on the form</param>
        ///  <param name="formText"> text that the form displays or the code uses for 
        ///  another purpose. Actions that don't use text ignore this parameter. </param>
        private void AccessForm(FormActions action, String formText)
        {
            try
            {
                //  Select an action to perform on the form:

                switch (action)
                {
                    case FormActions.EnableSendDataButton:

                        IntSendOutput.Enabled = true;
                        break;

                    case FormActions.DisableSendDataButton:

                        IntSendOutput.Enabled = false;
                        break;
                }
            }
            catch (Exception ex)
            {
                DisplayException(Name, ex);
                throw;
            }
        }


        private void StartListeningToDevice()
        {
            hostDeviceComm = new Thread(ExecuteInBackgroundAsync)
            {
                IsBackground = true
            };
            hostDeviceComm.Start();
            //Thread.Sleep(1000);
        }

        private async void ExecuteInBackgroundAsync()
        {
            while (true)
            {
                if (_deviceConnected)
                {
                    try
                    {
                        bool t;
                        //Array.Clear(_outBuffer, 0, 16);
                        switch (deviceAction)
                        {
                            case DeviceActions.GetInterruptReport:
                                // not implemented yet

                                break;

                            case DeviceActions.SendInterruptReport:

                                var userInput = GetDataFromTextInput();

                                ResetPlotting();
                                ResetUsbDataBox();

                                t = await _deviceComm.RequestToSendInterruptOutputReport(userInput);
                                if (t)
                                {
                                    switch (userInput[1])
                                    {
                                        case 97: // Command to turn the light ON
                                            UpdateUsbDataBox("LED Light OFF");
                                            break;
                                        case 98: // Command to turn the light ON
                                            UpdateUsbDataBox("LED Light ON");
                                            break;
                                        case 99: // Command to turn the light ON
                                            UpdateUsbDataBox("LED Light BLINK");
                                            break;
                                        case 100: //If command is sent to receive data from the device
                                            await _deviceComm.RequestToGetInterruptInputReport();
                                            break;
                                        default:
                                            String dataText =
                                                "Please Enter a valid Input" + System.Environment.NewLine +
                                                "------------------------------------------------" + System.Environment.NewLine +
                                                "97   : LED OFF" + System.Environment.NewLine +
                                                "98   : LED ON" + System.Environment.NewLine +
                                                "99   : LED BLINK" + System.Environment.NewLine +
                                                "100 : Receive Data";
                                            UpdateUsbDataBox(dataText);
                                            break;
                                    }
                                }

                                deviceAction = DeviceActions.StayOnHold;
                                break;
                        }

                        Thread.Sleep(300);
                    }
                    catch (Exception ex)
                    {
                        DisplayException(Name, ex);
                        _deviceConnected = false;
                        //throw;
                    }
                }
                else
                {
                    Thread.Sleep(2000);
                }
            }
        }


        /// <summary>
        ///     Perform actions that must execute when the program starts.
        /// </summary>
        private void Startup()
        {
            InitializePlotArea();

            try
            {
                Shown += InitializeDeviceCommunication;
            }
            catch (Exception ex)
            {
                DisplayException(Name, ex);
                throw;
            }

            StartListeningToDevice();
        }
        
        
        //
        // Event Handlers
        //

        /// <summary>
        /// Event handler for finding NCV hid and obtain the handle for further reading/writing of device data
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="eventArgs"></param>
        private void InitializeDeviceCommunication(Object sender, EventArgs eventArgs)
        {
            _deviceComm = new DeviceCommunication();
            _deviceComm.OnInterruptDataReceived += IntReceivedDataHandler;
            _deviceComm.OnHidDeviceConnected += DeviceConnectedHandler;
            _deviceComm.OnHidDeviceDisconnected += DeviceDisconnectedHandler;
            _deviceComm.CheckForBibeatHidDevice();
        }

        /// <summary>
        /// Shows the device is connected in the status bar
        /// when hid handle is successfully obtained and the device is ready to use 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="eventArgs"></param>
        private void DeviceConnectedHandler(object sender, EventArgs eventArgs)
        {
            toolStripStatusDevice.Text = @"Device is connected";
            toolStripStatusDevice.Image = Properties.Resources.online;
            MyMarshalDataToForm(FormActions.EnableSendDataButton, "");
            
            _deviceConnected = true;
        }

        /// <summary>
        /// Shows device disconnected status in the status bar
        /// when the NCV device is disconnected from USB port
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="eventArgs"></param>
        private void DeviceDisconnectedHandler(object sender, EventArgs eventArgs)
        {
            _deviceConnected = false;
            deviceAction = DeviceActions.SendInterruptReport;
            toolStripStatusDevice.Text = @"Device is disconnected";
            toolStripStatusDevice.Image = Properties.Resources.offline;
            MyMarshalDataToForm(FormActions.DisableSendDataButton, "");
            ResetPlotting();
            ResetUsbDataBox();
            //HostDeviceComm.Abort();
        }

        /// <summary>
        /// Initializes the plotting area using Oxyplot library for drawing ncv data received from USB
        /// </summary>
        private void InitializePlotArea()
        {
            _plotModel = new PlotModel

            {
                //PlotMargins = new OxyThickness(20, 20, 20, 20),
                IsLegendVisible = false,
                //Background = OxyColors.White
            };

            var linearAxis1 = new LinearAxis
            {
                Title = "X axis",
                MajorGridlineStyle = LineStyle.Solid,
                MinorGridlineStyle = LineStyle.Dot,
                Position = AxisPosition.Bottom,
                AbsoluteMinimum = 0,
                AbsoluteMaximum = 100,
                StartPosition = 0,
                EndPosition = 1
            };

            var linearAxis2 = new LinearAxis
            {
                Title = "Y axis",
                MajorGridlineStyle = LineStyle.Solid,
                MinorGridlineStyle = LineStyle.Dot,
                Minimum = 0,
                Maximum = 255,
                IsPanEnabled = false,
                IsZoomEnabled = false,
                StartPosition = 0,
                EndPosition = 1,
                Key = "y1",
                Position = AxisPosition.Left
            };
            var linearAxis3 = new LinearAxis
            {
                Position = AxisPosition.Left,
                MajorGridlineStyle = LineStyle.Solid,
                MinorGridlineStyle = LineStyle.Dot,
                Minimum = 0,
                Maximum = 5,
                IsPanEnabled = false,
                IsZoomEnabled = false,
                StartPosition = .5,
                EndPosition = 1,
                Key = "y2"
            };

            _plotModel.Axes.Add(linearAxis1);
            _plotModel.Axes.Add(linearAxis2);
            //_plotModel.Axes.Add(linearAxis3);

            //_plotModel.Series.Add(new FunctionSeries(Math.Sin, 0, 10, 1000) { YAxisKey = linearAxis2.Key });
            //_plotModel.Series.Add(new FunctionSeries(Math.Sin, 0, 10, 1000) { YAxisKey = linearAxis3.Key });

            _ls = new LineSeries();


            //Console.WriteLine("Step size: " + step);

            //for (var i = 0; i < _data.Length; i++) _ls.Points.Add(new DataPoint(i * stepX, stepY * _data[i]));

            var ls2 = new LineSeries
            {
                Color = OxyColors.Red,
                StrokeThickness = 2
            };

            ls2.Points.Add(new DataPoint(10, 0));
            ls2.Points.Add(new DataPoint(10, 5));

            
            var isDown = false;

            _ls.MouseDown += (s, e) =>
            {
                if (e.ChangedButton == OxyMouseButton.Left) isDown = true;
            };

            _plotModel.TrackerChanged += (s, e) =>
            {
                if (!isDown) return;

                if (e.HitResult == null) return;

                Console.WriteLine(e.HitResult.DataPoint.X);

                ls2.Points.Clear();
                ls2.Points.Add(new DataPoint(e.HitResult.DataPoint.X, 0));
                ls2.Points.Add(new DataPoint(e.HitResult.DataPoint.X, 5));
                _plotModel.InvalidatePlot(true);

                isDown = false;
            };

            // attaches the data to plotview chart. uncomment to enable plotview
            plotViewNcv.ActualController.UnbindMouseDown(OxyMouseButton.Left);
            plotViewNcv.Model = _plotModel;
        }

        /// <summary>
        /// Event handler for data received via USB Interrupt Transfer 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        ///
        private void IntReceivedDataHandler(object sender, IntDataReceivedArgs e)
        {
            Debug.WriteLine("Interrupt received data handler..");
            String dataText = "";
            byte[] intData = e.InterruptData.Skip(1).ToArray(); //remove the report ID from array[0] position

            for (var i = 0; i < intData.Length; i++) //e.PlotData.Length
            {
                _ls.Points.Add(new DataPoint(i, intData[i]));
                dataText += intData[i] + " ";
            }

            _plotModel.Series.Add(_ls);
            _plotModel.InvalidatePlot(true);

            UpdateUsbDataBox(dataText);
        }

        /// <summary>
        /// Print USB Data on UI Text box
        /// </summary>
        /// <param name="text"></param>
        private void UpdateUsbDataBox(String text)
        {
            if (usbDataBox.InvokeRequired)
            {
                var d = new SafeCallDelegate(UpdateUsbDataBox);
                usbDataBox.Invoke(d, new object[] {text});
            }
            else
            {
                usbDataBox.Text = text;
            }
        }

        private void ResetUsbDataBox()
        {
            UpdateUsbDataBox("");
        }


        ///  <summary>
        ///  Enables accessing a form's controls from another thread 
        ///  </summary>
        ///  
        ///  <param name="action"> a FormActions member that names the action to perform on the form </param>
        ///  <param name="textToDisplay"> text that the form displays or the code uses for 
        ///  another purpose. Actions that don't use text ignore this parameter.  </param>
        private void MyMarshalDataToForm(FormActions action, String textToDisplay)
        {
            try
            {
                object[] args = {action, textToDisplay};

                //  The AccessForm routine contains the code that accesses the form.

                MarshalDataToForm marshalDataToFormDelegate = AccessForm;

                //  Execute AccessForm, passing the parameters in args.

                Invoke(marshalDataToFormDelegate, args);
            }
            catch (Exception ex)
            {
                DisplayException(Name, ex);
                throw;
            }
        }


        /// <summary>
        ///     Provides a central mechanism for exception handling.
        ///     Displays a message box that describes the exception.
        /// </summary>
        /// <param name="moduleName"> the module where the exception occurred. </param>
        /// <param name="e"> the exception </param>
        internal static void DisplayException(string moduleName, Exception e)
        {
            //  Create an error message.

            var message = "Exception: " + e.Message + Environment.NewLine + "Module: " + moduleName +
                          Environment.NewLine + "Method: " + e.TargetSite.Name;

            const string caption = "Unexpected Exception";

            //MessageBox.Show(message, caption, MessageBoxButtons.OK);
            Debug.WriteLine(message);

            // Get the last error and display it. 

            var error = Marshal.GetLastWin32Error();

            Debug.WriteLine("The last Win32 Error was: " + error);
        }


        private void ResetPlotting()
        {
            _plotModel.InvalidatePlot(true);
            _plotModel.Series.Clear();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        private byte[] GetDataFromTextInput()
        {
            byte[] outputReportBuffer = new byte[65];

            outputReportBuffer[0] = 0; //assuming repot ID is 0

            outputReportBuffer[1] = (byte) (byte.TryParse(data1.Text, out byte n) ? n : 0);
            outputReportBuffer[2] = Convert.ToByte(data2.Text);
            outputReportBuffer[3] = Convert.ToByte(data3.Text);
            outputReportBuffer[4] = Convert.ToByte(data4.Text);
            outputReportBuffer[5] = Convert.ToByte(data5.Text);
            outputReportBuffer[6] = Convert.ToByte(data6.Text);
            outputReportBuffer[7] = Convert.ToByte(data7.Text);
            outputReportBuffer[8] = Convert.ToByte(data8.Text);

            return outputReportBuffer;
        }


        ///  <summary>
        ///  Display a message if the user clicks a button when a transfer is in progress.
        ///  </summary>
        /// 
        private void DisplayTransferInProgressMessage()
        {
            //  Create an error message.

            String message = "An interrupt transfer is in progress. Try again after sometime.";

            const String caption = "Wait a moment..";
            MessageBox.Show(message, caption, MessageBoxButtons.OK);
            Debug.WriteLine(message);
        }

        private void IntSendOutput_Click_1(object sender, EventArgs e)
        {
            try
            {
                if (_deviceComm._transferInProgress)
                {
                    DisplayTransferInProgressMessage();
                }
                else
                {
                    deviceAction = DeviceActions.SendInterruptReport;
                }
            }
            catch (Exception ex)
            {
                DisplayException(Name, ex);
                //throw;
            }
        }

        private void data1_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (!char.IsControl(e.KeyChar) && !char.IsDigit(e.KeyChar))
            {
                e.Handled = true;
            }
        }

        private void DaqUI_Load(object sender, EventArgs e)
        {

        }

        private void configureDeviceToolStripMenuItem_Click(object sender, EventArgs e)
        {
            DeviceConfigUI deviceConfigUi = new DeviceConfigUI(_deviceComm);
            deviceConfigUi.ShowDialog();
        }
    }
}