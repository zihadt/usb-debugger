using System;
using System.Diagnostics;
using System.IO;
using System.Management;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Win32.SafeHandles;

namespace BiBEAT_DAQ
{
    public class DeviceCommunication
    {
        private const String ModuleName = "Device Communication";
        private static int _vid = 4617; //0x1209
        private static int _pid = 1; //0x0001


        private readonly DeviceManagement _myDeviceManagement = new DeviceManagement();
        private ManagementEventWatcher _deviceArrivedWatcher;
        private FileStream _deviceData;
        internal bool DeviceDetected;
        internal bool DeviceHandleObtained;
        private ManagementEventWatcher _deviceRemovedWatcher;
        private SafeFileHandle _hidHandle;

        private Hid _myHid = new Hid();
        private SendOrGet _sendOrGet;
        public bool _transferInProgress;

        public event EventHandler<IntDataReceivedArgs> OnInterruptDataReceived;
        public event EventHandler<CtrlDataReceivedArgs> OnCtrlDataReceived;
        public event EventHandler OnHidDeviceConnected;
        public event EventHandler OnHidDeviceDisconnected;

        public int Vid
        {
            get => _vid;
            set => _vid = value;
        }

        public int Pid
        {
            get => _pid;
            set => _pid = value;
        }

        public DeviceCommunication()
        {
            this.Vid = Convert.ToInt32(Properties.Settings.Default.device_vid.Trim());
            this.Pid = Convert.ToInt32(Properties.Settings.Default.device_pid.Trim());
            DeviceNotificationsStart();
            //CheckForNcvDevice();
        }

        internal void CheckForBibeatHidDevice()
        {
            try
            {
                DeviceDetected = FindTheHid();
                if (DeviceDetected)
                {
                    DeviceHandleObtained = true;
                    OnHidDeviceConnected?.Invoke(this, EventArgs.Empty); // Invoke NCV device connected Event
                    Debug.WriteLine("NCV device is attached to the Host");
                }
            }
            catch (Exception ex)
            {
                DisplayException(ModuleName, ex);
                throw;
            }
        }

        ///  <summary>
        ///  Called if the user changes the Vendor ID or Product ID in the text box.
        ///  </summary>
        internal void DeviceHasChangedHandler(object sender, EventArgs eventArgs)
        {
            try
            {
                //  If a device was previously detected, stop receiving notifications about it.

                if (DeviceHandleObtained)
                {
                    DeviceNotificationsStop();
                    //CloseCommunications();
                }
                // Look for a device that matches the Vendor ID and Product ID in the text boxes.

                if (FindTheHid())
                {
                    DeviceNotificationsStart();
                    OnHidDeviceConnected?.Invoke(this, EventArgs.Empty);
                }
                else
                {
                    OnHidDeviceDisconnected?.Invoke(this, EventArgs.Empty);
                }
            }
            catch (Exception ex)
            {
                DisplayException(ModuleName, ex);
                throw;
            }
        }

        /// <summary>
        ///     Add handlers to detect device arrival and removal.
        /// </summary>
        internal void DeviceNotificationsStart()
        {
            AddDeviceArrivedHandler();
            AddDeviceRemovedHandler();
        }

        ///  <summary>
        ///  Stop receiving notifications about device arrival and removal
        ///  </summary>
        private void DeviceNotificationsStop()
        {
            try
            {
                if (_deviceArrivedWatcher != null)
                    _deviceArrivedWatcher.Stop();
                if (_deviceRemovedWatcher != null)
                    _deviceRemovedWatcher.Stop();
            }
            catch (Exception ex)
            {
                DisplayException(ModuleName, ex);
                //throw;
            }
        }

        /// <summary>
        ///     Add a handler to detect arrival of devices using WMI.
        /// </summary>
        private void AddDeviceArrivedHandler()
        {
            const int pollingIntervalSeconds = 2;
            var scope = new ManagementScope("root\\CIMV2");
            scope.Options.EnablePrivileges = true;

            try
            {
                var q = new WqlEventQuery();
                q.EventClassName = "__InstanceCreationEvent";
                q.WithinInterval = new TimeSpan(0, 0, pollingIntervalSeconds);
                q.Condition = @"TargetInstance ISA 'Win32_USBControllerdevice'";
                _deviceArrivedWatcher = new ManagementEventWatcher(scope, q);
                _deviceArrivedWatcher.EventArrived += DeviceAdded;

                _deviceArrivedWatcher.Start();
            }
            catch (Exception e)
            {
                Debug.WriteLine(e.Message);
                if (_deviceArrivedWatcher != null)
                    _deviceArrivedWatcher.Stop();
            }
        }

        /// <summary>
        ///     Add a handler to detect removal of devices using WMI.
        /// </summary>
        private void AddDeviceRemovedHandler()
        {
            const int pollingIntervalSeconds = 2;
            var scope = new ManagementScope("root\\CIMV2");
            scope.Options.EnablePrivileges = true;

            try
            {
                var q = new WqlEventQuery();
                q.EventClassName = "__InstanceDeletionEvent";
                q.WithinInterval = new TimeSpan(0, 0, pollingIntervalSeconds);
                q.Condition = @"TargetInstance ISA 'Win32_USBControllerdevice'";
                _deviceRemovedWatcher = new ManagementEventWatcher(scope, q);
                _deviceRemovedWatcher.EventArrived += DeviceRemoved;
                _deviceRemovedWatcher.Start();
            }
            catch (Exception e)
            {
                Debug.WriteLine(e.Message);
                if (_deviceRemovedWatcher != null)
                    _deviceRemovedWatcher.Stop();
            }
        }

        /// <summary>
        ///     Called on arrival of any device.
        ///     Calls a routine that searches to see if the desired device is present.
        /// </summary>
        internal void DeviceAdded(object sender, EventArrivedEventArgs e)
        {
            try
            {
                Debug.WriteLine("A USB device has been inserted");

                DeviceDetected = FindTheHid();
                if (DeviceDetected)
                {
                    DeviceHandleObtained = true;
                    OnHidDeviceConnected?.Invoke(this, EventArgs.Empty); // Invoke NCV device connected Event
                }
            }
            catch (Exception ex)
            {
                DisplayException(ModuleName, ex);
                throw;
            }
        }

        /// <summary>
        ///     Called on removal of any device.
        ///     Calls a routine that searches to see if the desired device is still present.
        /// </summary>
        private void DeviceRemoved(object sender, EventArgs e)
        {
            try
            {
                Debug.WriteLine("A USB device has been removed");

                DeviceDetected = FindDeviceUsingWmi();

                if (!DeviceDetected)
                {
                    DeviceHandleObtained = false;
                    CloseCommunications();
                    OnHidDeviceDisconnected?.Invoke(this, EventArgs.Empty);
                }
            }
            catch (Exception ex)
            {
                DisplayException(ModuleName, ex);
                //throw;
            }
        }

        /// <summary>
        ///     Use the System.Management class to find a device by Vendor ID and Product ID using WMI. If found, display device
        ///     properties.
        /// </summary>
        /// <remarks>
        ///     During debugging, if you stop the firmware but leave the device attached, the device may still be detected as
        ///     present
        ///     but will be unable to communicate. The device will show up in Windows Device Manager as well.
        ///     This situation is unlikely to occur with a final product.
        /// </remarks>
        private bool FindDeviceUsingWmi()
        {
            try
            {
                // Prepend "@" to string below to treat backslash as a normal character (not escape character):

                var deviceIdString = @"USB\VID_" + Vid.ToString("X4") + "&PID_" + Pid.ToString("X4");

                DeviceDetected = false;
                var searcher = new ManagementObjectSearcher("root\\CIMV2", "SELECT * FROM Win32_PnPEntity");

                foreach (var o in searcher.Get())
                {
                    var queryObj = (ManagementObject) o;
                    if (queryObj["PNPDeviceID"].ToString().Contains(deviceIdString))
                    {
                        DeviceDetected = true;

                        // Display device properties.

                        foreach (WmiDeviceProperties wmiDeviceProperty in Enum.GetValues(typeof(WmiDeviceProperties)))
                            Debug.WriteLine(wmiDeviceProperty + ": {0}", queryObj[wmiDeviceProperty.ToString()]);
                    }
                }

                if (!DeviceDetected) Debug.WriteLine("BMPT NCV Device not attached to any USB port");
                return DeviceDetected;
            }
            catch (Exception ex)
            {
                DisplayException(ModuleName, ex);
                throw;
            }
        }


        /// <summary>
        ///     Close the handle and FileStreams for a device.
        /// </summary>
        private void CloseCommunications()
        {
            if (_deviceData != null) _deviceData.Close();

            if (_hidHandle != null && !_hidHandle.IsInvalid) _hidHandle.Close();

            // The next attempt to communicate will get a new handle and FileStreams.

            DeviceHandleObtained = false;
        }

        /// <summary>
        ///     Call HID functions that use Win32 API functions to locate a HID-class device
        ///     by its Vendor ID and Product ID. Open a handle to the device.
        /// </summary>
        /// <returns>
        ///     True if the device is detected, False if not detected.
        /// </returns>
        internal bool FindTheHid()
        {
            //Debug.WriteLine("called here");
            var devicePathName = new string[128];
            var myDevicePathName = "";

            try
            {
                DeviceHandleObtained = false;
                CloseCommunications();


                // Get the HID-class GUID.

                var hidGuid = _myHid.GetHidGuid();

                //String functionName = "GetHidGuid";
                //Debug.WriteLine(_myDebugging.ResultOfApiCall(functionName));
                //Debug.WriteLine("  GUID for system HIDs: " + hidGuid.ToString());

                //  Fill an array with the device path names of all attached HIDs.

                var availableHids = _myDeviceManagement.FindDeviceFromGuid(hidGuid, ref devicePathName);

                //  If there is at least one HID, attempt to read the Vendor ID and Product ID
                //  of each device until there is a match or all devices have been examined.

                if (availableHids)
                {
                    var memberIndex = 0;

                    do
                    {
                        // Open the handle without read/write access to enable getting information about any HID, even system keyboards and mice.

                        _hidHandle = _myHid.OpenHandle(devicePathName[memberIndex], false);

                        //functionName = "CreateFile";


                        if (!_hidHandle.IsInvalid)
                        {
                            // The returned handle is valid, 
                            // so find out if this is the device we're looking for.

                            _myHid.DeviceAttributes.Size = Marshal.SizeOf(_myHid.DeviceAttributes);

                            var success = _myHid.GetAttributes(_hidHandle, ref _myHid.DeviceAttributes);

                            if (success)
                            {
                                if (_myHid.DeviceAttributes.VendorID == this.Vid &&
                                    _myHid.DeviceAttributes.ProductID == this.Pid)
                                {
                                    //Debug.WriteLine(_myDebugging.ResultOfApiCall(functionName));
                                    //Debug.WriteLine("  Returned handle: " + _hidHandle);

                                    Debug.WriteLine("  Handle obtained to my device");

                                    Debug.WriteLine("  HIDD_ATTRIBUTES structure filled without error.");
                                    Debug.WriteLine("  Structure size: " + _myHid.DeviceAttributes.Size);
                                    Debug.WriteLine("  Vendor ID: " +
                                                    Convert.ToString(_myHid.DeviceAttributes.VendorID, 16));
                                    Debug.WriteLine("  Product ID: " +
                                                    Convert.ToString(_myHid.DeviceAttributes.ProductID, 16));
                                    Debug.WriteLine("  Version Number: " +
                                                    Convert.ToString(_myHid.DeviceAttributes.VersionNumber, 16));


                                    DeviceHandleObtained = true;

                                    myDevicePathName = devicePathName[memberIndex];
                                }
                                else
                                {
                                    //  It's not a match, so close the handle.
                                    Debug.WriteLine("I am here:" + Vid);
                                    DeviceHandleObtained = false;
                                    _hidHandle.Close();
                                }
                            }
                            else
                            {
                                //  There was a problem retrieving the information.

                                Debug.WriteLine("  Error in filling HIDD_ATTRIBUTES structure.");
                                DeviceHandleObtained = false;
                                _hidHandle.Close();
                            }
                        }

                        //  Keep looking until we find the device or there are no devices left to examine.

                        memberIndex = memberIndex + 1;
                    } while (!(DeviceHandleObtained || memberIndex == devicePathName.Length));
                }

                if (DeviceHandleObtained)
                {
                    //  The device was detected.
                    //OnHidDeviceConnected?.Invoke(this, EventArgs.Empty);

                    //  Learn the capabilities of the device.

                    _myHid.Capabilities = _myHid.GetDeviceCapabilities(_hidHandle);

                    //  Find out if the device is a system mouse or keyboard.

                    _myHid.GetHidUsage(_myHid.Capabilities);

                    //  Get the Input report buffer size.

                    GetInputReportBufferSize();
                    //MyMarshalDataToForm(FormActions.EnableInputReportBufferSize, "");

                    //Close the handle and reopen it with read/write access.

                    _hidHandle.Close();

                    _hidHandle = _myHid.OpenHandle(myDevicePathName, true);

                    if (_hidHandle.IsInvalid)
                    {
                        //MyMarshalDataToForm(FormActions.AddItemToListBox, "The device is a system " + _hidUsage + ".");
                        //MyMarshalDataToForm(FormActions.AddItemToListBox, "Windows 2000 and later obtain exclusive access to Input and Output reports for this devices.");
                        //MyMarshalDataToForm(FormActions.AddItemToListBox, "Windows 8 also obtains exclusive access to Feature reports.");
                        //MyMarshalDataToForm(FormActions.ScrollToBottomOfListBox, "");
                    }
                    else
                    {
                        if (_myHid.Capabilities.InputReportByteLength > 0)
                        {
                            //  Set the size of the Input report buffer. 

                            var inputReportBuffer = new byte[_myHid.Capabilities.InputReportByteLength];

                            _deviceData = new FileStream(_hidHandle, FileAccess.Read | FileAccess.Write,
                                inputReportBuffer.Length, false);
                        }

                        //if (_myHid.Capabilities.OutputReportByteLength > 0)
                        //{
                        //    Byte[] outputReportBuffer = null;
                        //}
                        //  Flush any waiting reports in the input buffer. (optional)

                        _myHid.FlushQueue(_hidHandle);
                    }
                }
                else
                {
                    Debug.WriteLine("NCV Device Not found");
                }

                return DeviceHandleObtained;
            }
            catch (Exception ex)
            {
                DisplayException(ModuleName, ex);
                throw;
            }
        }

        /// <summary>
        ///     Find and display the number of Input buffers
        ///     (the number of Input reports the HID driver will store).
        /// </summary>
        private void GetInputReportBufferSize()
        {
            var numberOfInputBuffers = 0;

            try
            {
                //  Get the number of input buffers.

                _myHid.GetNumberOfInputBuffers(_hidHandle, ref numberOfInputBuffers);
            }
            catch (Exception ex)
            {
                DisplayException(ModuleName, ex);
                throw;
            }
        }

        /// <summary>
        ///     Request an Input report via Control Transfer
        ///     Assumes report ID = 0.
        /// </summary>
        internal void RequestToGetControlInputReport()
        {
            const int readTimeout = 500;

            byte[] inputReportBuffer = null;

            try
            {
                var success = false;

                //  If the device hasn't been detected, was removed, or timed out on a previous attempt
                //  to access it, look for the device.

                if (!DeviceHandleObtained) DeviceHandleObtained = FindTheHid();

                if (DeviceHandleObtained)
                {
                    //  Don't attempt to exchange reports if valid handles aren't available
                    //  (as for a mouse or keyboard under Windows 2000 and later.)

                    if (!_hidHandle.IsInvalid)
                    {
                        //  Read an Input report.

                        //  Don't attempt to send an Input report if the HID has no Input report.
                        //  (The HID spec requires all HIDs to have an interrupt IN endpoint,
                        //  which suggests that all HIDs must support Input reports.)

                        if (_myHid.Capabilities.InputReportByteLength > 0)
                        {
                            //  Set the size of the Input report buffer. 

                            inputReportBuffer = new byte[_myHid.Capabilities.InputReportByteLength];


                            _transferInProgress = true;

                            //  Read a report using a control transfer.

                            success = _myHid.GetInputReportViaControlTransfer(_hidHandle, ref inputReportBuffer);
                            _transferInProgress = false;
                            if (success)
                            {
                                OnCtrlDataReceived?.Invoke(this,
                                    new CtrlDataReceivedArgs(
                                        inputReportBuffer)); // Control data received event called
                            }
                        }
                        else
                        {
                            Debug.WriteLine(
                                "No attempt to read an Input report was made. The HID doesn't have an Input report. ");
                        }
                    }
                    else
                    {
                        Debug.WriteLine(
                            "Invalid handle. No attempt to write an Output report or read an Input report was made.");
                    }

                    if (success)
                    {
                        //Debug.WriteLine("length: "+inputReportBuffer.Length);
                        //string test = "";
                        //for (int k = 0; k < inputReportBuffer.Length; k++)
                        //{
                        //    test += inputReportBuffer[k];
                        //}
                        //Debug.WriteLine(test);

                        DisplayReportData(inputReportBuffer, ReportTypes.Input, ReportReadOrWritten.Read);
                    }
                    else
                    {
                        CloseCommunications();
                        Debug.WriteLine("The attempt to read an Input report has failed.");
                    }
                }
            }

            catch (Exception ex)
            {
                DisplayException(ModuleName, ex);
                throw;
            }
        }

        ///  <summary>
        ///  Request a Feature report.
        ///  Assumes report ID = 0.
        ///  </summary>
        internal void RequestToGetFeatureReport()
        {
            String byteValue = null;

            try
            {
                //  If the device hasn't been detected, was removed, or timed out on a previous attempt
                //  to access it, look for the device.

                if (!DeviceHandleObtained)
                {
                    DeviceHandleObtained = FindTheHid();
                }

                if (DeviceHandleObtained)
                {
                    Byte[] inFeatureReportBuffer = null;

                    if ((_myHid.Capabilities.FeatureReportByteLength > 0))
                    {
                        //  The HID has a Feature report.	
                        //  Read a report from the device.

                        //  Set the size of the Feature report buffer. 

                        if ((_myHid.Capabilities.FeatureReportByteLength > 0))
                        {
                            inFeatureReportBuffer = new Byte[_myHid.Capabilities.FeatureReportByteLength];
                        }

                        //  Read a report.

                        Boolean success = _myHid.GetFeatureReport(_hidHandle, ref inFeatureReportBuffer);

                        if (success)
                        {
                            OnCtrlDataReceived?.Invoke(this,
                                new CtrlDataReceivedArgs(
                                    inFeatureReportBuffer)); // Control data received event called
                            // DisplayReportData(inFeatureReportBuffer, ReportTypes.Feature, ReportReadOrWritten.Read);
                        }
                        else
                        {
                            CloseCommunications();
                        }
                    }
                    else
                    {
                        Debug.WriteLine("The HID doesn't have a Feature report.");
                    }
                }

                _transferInProgress = false;
            }
            catch (Exception ex)
            {
                DisplayException(ModuleName, ex);
                throw;
                // return false;
            }
        }

        /// <summary>
        ///     Request an Input report via Control Transfer
        ///     Assumes report ID = 0.
        /// </summary>
        internal async Task<bool> RequestToGetInterruptInputReport()
        {
            const int readTimeout = 20000;

            byte[] inputReportBuffer = null;

            try
            {
                var success = false;

                //  If the device hasn't been detected, was removed, or timed out on a previous attempt
                //  to access it, look for the device.

                if (!DeviceHandleObtained) DeviceHandleObtained = FindTheHid();

                if (DeviceHandleObtained)
                {
                    //  Don't attempt to exchange reports if valid handles aren't available
                    //  (as for a mouse or keyboard under Windows 2000 and later.)

                    if (!_hidHandle.IsInvalid)
                    {
                        //  Read an Input report.

                        //  Don't attempt to send an Input report if the HID has no Input report.
                        //  (The HID spec requires all HIDs to have an interrupt IN endpoint,
                        //  which suggests that all HIDs must support Input reports.)

                        if (_myHid.Capabilities.InputReportByteLength > 0)
                        {
                            //  Set the size of the Input report buffer. 

                            inputReportBuffer = new byte[_myHid.Capabilities.InputReportByteLength];


                            _transferInProgress = true;

                            //  Read a report using interrupt transfers. 
                            //  Timeout if no report available.
                            //  To enable reading a report without blocking the calling thread, uses Filestream's ReadAsync method.                                               

                            // Create a delegate to execute on a timeout.

                            Action onReadTimeoutAction = OnReadTimeout;

                            // The CancellationTokenSource specifies the timeout value and the action to take on a timeout.

                            var cts = new CancellationTokenSource();

                            // Cancel the read if it hasn't completed after a timeout.

                            cts.CancelAfter(readTimeout);

                            // Specify the function to call on a timeout.

                            cts.Token.Register(onReadTimeoutAction);

                            // Stops waiting when data is available or on timeout:
                            Debug.WriteLine(" Waiting for data via Interrupt In..");

                            var bytesRead =
                                await _myHid.GetInputReportViaInterruptTransfer(_deviceData, inputReportBuffer,
                                    cts);

                            // Arrive here only if the operation completed.

                            // Dispose to stop the timeout timer. 

                            cts.Dispose();

                            _transferInProgress = false;
                            //cmdGetInputReportInterrupt.Enabled = true;

                            if (bytesRead > 0)
                            {
                                success = true;
                                OnInterruptDataReceived?.Invoke(this,
                                    new IntDataReceivedArgs(
                                        inputReportBuffer)); // Interrupt data received event called
                                Debug.Print("bytes read (includes report ID) = " + Convert.ToString(bytesRead));
                                //return success;
                            }
                        }
                        else
                        {
                            Debug.WriteLine(
                                "No attempt to read an Input report was made. The HID doesn't have an Input report. ");
                            return false;
                        }
                    }
                    else
                    {
                        Debug.WriteLine(
                            "Invalid handle. No attempt to write an Output report or read an Input report was made.");
                        return false;
                    }

                    if (success)
                    {
                        //Debug.WriteLine("length: "+inputReportBuffer.Length);
                        //string test = "";
                        //for (int k = 0; k < inputReportBuffer.Length; k++)
                        //{
                        //    test += inputReportBuffer[k];
                        //}
                        //Debug.WriteLine(test);

                        DisplayReportData(inputReportBuffer, ReportTypes.Input, ReportReadOrWritten.Read);
                        return success;
                    }
                    else
                    {
                        CloseCommunications();
                        Debug.WriteLine("The attempt to read an Input report has failed.");
                        return false;
                    }
                }

                return false;
            }

            catch (Exception ex)
            {
                DisplayException(ModuleName, ex);
                throw;
                //return false;
            }
        }

        ///  <summary>
        ///  Sends an Output report.
        ///  Assumes report ID = 0.
        ///  </summary>
        internal bool RequestToSendControlOutputReport(byte[] outputReportBuffer)
        {
            try
            {
                //  If the device hasn't been detected, was removed, or timed out on a previous attempt
                //  to access it, look for the device.

                if (!DeviceHandleObtained)
                {
                    DeviceHandleObtained = FindTheHid();
                }

                if (DeviceHandleObtained)
                {
                    //GetBytesToSend();
                }
                //  Don't attempt to exchange reports if valid handles aren't available
                //  (as for a mouse or keyboard.)

                if (!_hidHandle.IsInvalid)
                {
                    //  Don't attempt to send an Output report if the HID has no Output report.

                    if (_myHid.Capabilities.OutputReportByteLength > 0)
                    {
                        //  Write a report.

                        Boolean success;

                        _transferInProgress = true;

                        //  Use a control transfer to send the report,
                        //  even if the HID has an interrupt OUT endpoint.

                        success = _myHid.SendOutputReportViaControlTransfer(_hidHandle, outputReportBuffer);

                        _transferInProgress = false;
                        //cmdSendOutputReportControl.Enabled = true;


                        if (success)
                        {
                            DisplayReportData(outputReportBuffer, ReportTypes.Output, ReportReadOrWritten.Written);
                            return true;
                        }
                        else
                        {
                            CloseCommunications();
                            Debug.WriteLine("The attempt to write an Output report failed.");
                            return false;
                        }
                    }
                }
                else
                {
                    Debug.WriteLine("The HID doesn't have an Output report.");
                    return false;
                }
            }

            catch (Exception ex)
            {
                DisplayException(ModuleName, ex);
                throw;
            }

            return false;
        }

        ///  <summary>
        ///  Sends an Interrupt Output report.
        ///  Assumes report ID = 0.
        ///  </summary>
        internal async Task<bool> RequestToSendInterruptOutputReport(byte[] outputReportBuffer)
        {
            const Int32 writeTimeout = 5000;
            String byteValue = null;

            try
            {
                //  If the device hasn't been detected, was removed, or timed out on a previous attempt
                //  to access it, look for the device.

                if (!DeviceHandleObtained)
                {
                    DeviceHandleObtained = FindTheHid();
                }


                //  Don't attempt to exchange reports if valid handles aren't available
                //  (as for a mouse or keyboard.)

                if (!_hidHandle.IsInvalid)
                {
                    //  Don't attempt to send an Output report if the HID has no Output report.

                    if (_myHid.Capabilities.OutputReportByteLength > 0)
                    {
                        //  Write a report.

                        Boolean success;


                        Debug.WriteLine("Interrupt Out");
                        _transferInProgress = true;

                        // The CancellationTokenSource specifies the timeout value and the action to take on a timeout.

                        var cts = new CancellationTokenSource();

                        // Create a delegate to execute on a timeout.

                        Action onWriteTimeoutAction = OnWriteTimeout;

                        // Cancel the read if it hasn't completed after a timeout.

                        cts.CancelAfter(writeTimeout);

                        // Specify the function to call on a timeout.

                        cts.Token.Register(onWriteTimeoutAction);

                        // Send an Output report and wait for completion or timeout.

                        success = await _myHid.SendOutputReportViaInterruptTransfer(_deviceData, _hidHandle,
                            outputReportBuffer, cts);

                        // Get here only if the operation completes without a timeout.

                        _transferInProgress = false;

                        // Dispose to stop the timeout timer.

                        cts.Dispose();


                        if (success)
                        {
                            DisplayReportData(outputReportBuffer, ReportTypes.Output, ReportReadOrWritten.Written);
                            return true;
                        }
                        else
                        {
                            CloseCommunications();
                            Debug.WriteLine("The attempt to write an Output report failed.");
                            return false;
                        }
                    }
                }
                else
                {
                    Debug.WriteLine("The HID doesn't have an Output report.");
                    return false;
                }
            }

            catch (Exception ex)
            {
                DisplayException(ModuleName, ex);
                throw;
            }

            return false;
        }

        /// <summary>
        ///     Displays received or written report data.
        /// </summary>
        /// <param name="buffer"> contains the report data. </param>
        /// <param name="currentReportType"> "Input", "Output", or "Feature"</param>
        /// <param name="currentReadOrWritten">
        ///     "read" for Input and IN Feature reports, "written" for Output and OUT Feature
        ///     reports.
        /// </param>
        private void DisplayReportData(byte[] buffer, ReportTypes currentReportType,
            ReportReadOrWritten currentReadOrWritten)
        {
            try
            {
                int count;

                Debug.WriteLine(currentReportType + " report has been " + currentReadOrWritten.ToString().ToLower() +
                                ".");

                //  Display the report data received in the form's list box.

                //Debug.WriteLine(" Report ID: " + string.Format("{0:X2} ", buffer[0]));
                //Debug.WriteLine(" Report Data:");


                //for (count = 1; count <= buffer.Length - 1; count++)
                //{
                //    //  Display bytes as 2-character Hex strings.

                //    var byteValue = string.Format("{0:X2} ", buffer[count]);

                //    Debug.Write(byteValue + " ");
                //}
            }
            catch (Exception ex)
            {
                DisplayException(ModuleName, ex);
                throw;
            }
        }

        /// <summary>
        ///     Timeout if read via interrupt transfer doesn't return.
        /// </summary>
        private void OnReadTimeout()
        {
            try
            {
                Debug.WriteLine("The attempt to read a report timed out");
                CloseCommunications();

                _transferInProgress = false;
                //_sendOrGet = SendOrGet.Send; //was commented before. just to test what happens when it
            }
            catch (Exception ex)
            {
                DisplayException(ModuleName, ex);
                //throw;
            }
        }

        /// <summary>
        /// Timeout if write via interrupt transfer doesn't return.
        /// </summary>
        private void OnWriteTimeout()
        {
            try
            {
                Debug.WriteLine("The attempt to write a report timed out.");
                CloseCommunications();
                _transferInProgress = false;
                _sendOrGet = SendOrGet.Get;
            }
            catch (Exception ex)
            {
                DisplayException(ModuleName, ex);
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

            var LineNumber = GetLineNumber(e);
            Debug.WriteLine("line number:" + LineNumber);
        }

        public static int GetLineNumber(Exception ex)
        {
            var lineNumber = 0;
            const string lineSearch = ":line ";
            var index = ex.StackTrace.LastIndexOf(lineSearch);
            if (index != -1)
            {
                var lineNumberText = ex.StackTrace.Substring(index + lineSearch.Length);
                if (int.TryParse(lineNumberText, out lineNumber))
                {
                }
            }

            return lineNumber;
        }

        private enum WmiDeviceProperties
        {
            Name,
            Caption,
            Description,
            Manufacturer,
            PNPDeviceID,
            DeviceID,
            ClassGUID
        }

        private enum SendOrGet
        {
            Send,
            Get
        }

        public enum TransferTypes
        {
            Control,
            Interrupt
        }

        private enum ReportReadOrWritten
        {
            Read,
            Written
        }

        private enum ReportTypes
        {
            Input,
            Output,
            Feature
        }
    }

    public class CtrlDataReceivedArgs : EventArgs
    {
        public CtrlDataReceivedArgs(byte[] data)
        {
            CtrlData = data;
        }

        public byte[] CtrlData { get; set; }
    }

    public class IntDataReceivedArgs : EventArgs
    {
        public IntDataReceivedArgs(byte[] data)
        {
            InterruptData = data;
        }

        public byte[] InterruptData { get; set; }
    }
}