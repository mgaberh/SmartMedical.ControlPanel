using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO.Ports;
using System.Reflection;
using System.Threading;
using System.ComponentModel;

namespace SerialComms.Manager
{
    /// <summary>
    /// The class that does all the work by wrapping the SerialPort class from .NET.
    /// </summary>
    /// <remarks></remarks>
    public class _Serialport : IDisposable
    {
        private SerialPort _serialPortNet = null;
        private _SerialPortSettings _serialPortSettings = null;

        // Event handler delegate lists.
        private event EventHandler<_SerialDataBytesArgs> NewSerialDataBytesReceived;
        private event EventHandler<_SerialDataStringArgs> NewSerialDataStringReceived;

        private List<string> _portNamesWMI = new List<string>();
        private static string[] _portNamesAvailable_ = null;

        // Use Bindinglist for automatic update of combobox when list changes.
        private BindingList<KeyValuePair<string, int>> _settableBaud_ = new BindingList<KeyValuePair<string, int>>();
        private BindingList<KeyValuePair<string, int>> _settableData_ = new BindingList<KeyValuePair<string, int>>();
        private BindingList<KeyValuePair<string, int>> _settableStopBits_ = new BindingList<KeyValuePair<string, int>>();
        private BindingList<KeyValuePair<string, int>> _settableParity_ = new BindingList<KeyValuePair<string, int>>();
        private BindingList<KeyValuePair<string, int>> _settableHandshake_ = new BindingList<KeyValuePair<string, int>>();

        bool _disposed = false;

        #region Public properties
        public _SerialPortSettings _Settings
        {
            get
            {
                return _serialPortSettings;
            }
            set
            {
                _serialPortSettings = value;
            }
        }

        public List<string> _PortNameWMI
        {
            get
            {
                return _portNamesWMI;
            }
            set
            {
                _portNamesWMI = value;
            }
        }
        #endregion

        public _Serialport(_SerialPortSettings settings)
        {
            _serialPortNet = new SerialPort();   // This is the serial port class in .net.
            _serialPortSettings = settings;
            initialisePort();
        }

        ~_Serialport()
        {
            stop();
            Dispose();
        }

        public void Dispose()
        {
            // Start by calling Dispose(bool) with true.
            Dispose(true);
            // Suppress finalization for this object, since we've already handled our resource cleanup tasks.
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool bDisposing)
        {
            // _disposed flag to allow us to call this method multiple times safely.
            if (!this._disposed)
            {
                if (bDisposing)
                {
                    _serialPortNet.DataReceived -= new SerialDataReceivedEventHandler(dataReceivedHandler);
                }

                // Clean up unmanaged resources.
                if (_serialPortNet != null)
                {
                    if (_serialPortNet.IsOpen)
                        _serialPortNet.Close();

                    _serialPortNet.Dispose();
                }
            }

            _disposed = true;
        }

        private bool initialisePort()
        {
            if (setPortName())
            {
                //GetSerialPortNamesWMI();
                //getPortCapabilities();
                configurePort();

                return true;
            }

            return false;
        }

        #region Public_properties
        // Used for binding to controls.
        public BindingList<KeyValuePair<string, int>> _SettableBaud
        {
            get
            {
                return _settableBaud_;
            }
            set
            {
                _settableBaud_ = value;
            }
        }

        public string[] _PortNamesAvailable
        {
            get
            {
                return _portNamesAvailable_;
            }
            set
            {
                _portNamesAvailable_ = value;
            }
        }

        public BindingList<KeyValuePair<string, int>> _SettableData
        {
            get
            {
                return _settableData_;
            }
            set
            {
                _settableData_ = value;
            }
        }

        public BindingList<KeyValuePair<string, int>> _SettableStopBits
        {
            get
            {
                return _settableStopBits_;
            }
            set
            {
                _settableStopBits_ = value;
            }
        }

        public BindingList<KeyValuePair<string, int>> _SettableParity
        {
            get
            {
                return _settableParity_;
            }
            set
            {
                _settableParity_ = value;
            }
        }

        public BindingList<KeyValuePair<string, int>> _SettableHandshake
        {
            get
            {
                return _settableHandshake_;
            }
            set
            {
                _settableHandshake_ = value;
            }
        }

        #endregion

        /// <summary>
        /// Open the port to enable send and receive.
        /// </summary>
        public bool start()
        {
            stop();

            if (_serialPortNet != null)
            {
                try
                {
                    _serialPortNet.Open();
                    _serialPortNet.DiscardOutBuffer();
                    _serialPortNet.DiscardInBuffer();
                    return true;
                }
                catch (Exception e)
                {
                    throw new Exception(String.Format("Serial port start failed:{0}", e.Message));
                }
            }
            return false;
        }

        /// <summary>
        /// Close the port and disable send and receive.
        /// </summary>
        public bool stop()
        {
            // Closing serial port if it is open
            if (_serialPortNet != null && _serialPortNet.IsOpen)
            {
                _serialPortNet.DiscardOutBuffer();
                _serialPortNet.DiscardInBuffer();
                _serialPortNet.Close();
            }
            else
            {
                return false;
            }

            // MSDN The best practice for any application is to wait for some amount of time after calling the
            // Close method before attempting to call the Open method, as the port may not be closed instantly.
            Thread.Sleep(250);

            return true;
        }

        /// <summary>
        /// Send data as byte array.
        /// </summary>
        /// <remarks>Write data byte array to the port for sending.</remarks>
        public bool Send(ref _SerialDataBytesArgs Data)
        {
            if (!_serialPortNet.IsOpen)
                return false;

            _serialPortNet.Write(Data.BytesOut, 0, Data.NumBytes);
            return true;
        }

        /// <summary>
        /// Send data as string.
        /// </summary>
        /// <remarks>Write data string to the port for sending.</remarks>
        public bool Send(ref _SerialDataStringArgs Data)
        {
            if (!_serialPortNet.IsOpen)
                return false;

            _serialPortNet.WriteLine(Data._String);
            return true;
        }

        /// <summary>
        /// Get a valid port name.
        /// </summary>
        /// <remarks>
        /// Get available port names. 
        /// If the port name currently in use is in the list then all is well.
        ///  If not just select the first name in the list. 
        /// A port name must be valid before the port can be opened so this step ensures it.
        /// </remarks>
        public bool setPortName()
        {
            if (getAvailablePortNames())
            {
                if (!_portNamesAvailable_.Contains(_serialPortSettings._PortName))
                    _serialPortSettings._PortName = _portNamesAvailable_[0];

                _serialPortNet.PortName = _serialPortSettings._PortName;

                return true;
            }

            return false;
        }

        /// <summary>
        /// Get available port names using the static method of the SerialPort class.
        /// </summary>
        public static bool getAvailablePortNames()
        {
            _portNamesAvailable_ = SerialPort.GetPortNames();

            return (_portNamesAvailable_.Count() > 0);
        }

        /// <summary>
        /// Get extra information about the ports using WMI.
        /// </summary>
        //public void GetSerialPortNamesWMI()
        //{
        //    if (_serialPortSettings._useWMI)
        //    {
        //        _WMIHardware.GetNames_Win32_SerialPort(ref _portNamesWMI, true);
        //        _WMIHardware.GetNames_MSSerial_PortName(ref _portNamesWMI, false);
        //    }
        //}

        /// <summary>
        /// Configure port to the chosen settings.
        /// </summary>
        public void configurePort(bool bHandler = true)
        {
            stop();

            if (bHandler)
                _serialPortNet.DataReceived -= new SerialDataReceivedEventHandler(dataReceivedHandler);

            _serialPortNet.PortName = _serialPortSettings._PortName;

            // Set the read/write timeouts
            _serialPortNet.ReadTimeout = 500;
            _serialPortNet.WriteTimeout = 500;

            _serialPortNet.BaudRate = _serialPortSettings._BaudRate;
            _serialPortNet.Parity = _serialPortSettings._Parity;
            _serialPortNet.DataBits = _serialPortSettings._DataBits;
            _serialPortNet.StopBits = _serialPortSettings._StopBits;
            _serialPortNet.Handshake = _serialPortSettings._Handshake;

            if (bHandler)
                _serialPortNet.DataReceived += new SerialDataReceivedEventHandler(dataReceivedHandler);
        }

        #region Port capabilities
        /// <summary>
        /// Get the possible range of settings for baud rate etc for this port.
        /// </summary>
        public void getPortCapabilities()
        {
            stop();

            _serialPortNet.PortName = _serialPortSettings._PortName;

            _serialPortNet.Open();

            object commProp = _serialPortNet.BaseStream.GetType().GetField("commProp", BindingFlags.Instance | BindingFlags.NonPublic).GetValue(_serialPortNet.BaseStream);

            Int32 dwSettableBaud = (Int32)commProp.GetType().GetField("dwSettableBaud", BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public).GetValue(commProp);
            getSettableBaud(dwSettableBaud);

            UInt16 wSettableData = (UInt16)commProp.GetType().GetField("wSettableData", BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public).GetValue(commProp);
            getSettableData(wSettableData);

            UInt16 wSettableStopParity = (UInt16)commProp.GetType().GetField("wSettableStopParity", BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public).GetValue(commProp);
            getSettableStopBits(wSettableStopParity);
            getSettableParity(wSettableStopParity);

            Int32 dwSettableParams = (Int32)commProp.GetType().GetField("dwSettableParams", BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public).GetValue(commProp);
            Int32 dwProvCapabilities = (Int32)commProp.GetType().GetField("dwProvCapabilities", BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public).GetValue(commProp);

            _serialPortNet.Close();
        }

        private void getSettableData(UInt16 wsettabledata)
        {
            const UInt16 DATABITS_5 = 0x0001;
            const UInt16 DATABITS_6 = 0x0002;
            const UInt16 DATABITS_7 = 0x0004;
            const UInt16 DATABITS_8 = 0x0008;
            const UInt16 DATABITS_16 = 0x0010;
            const UInt16 DATABITS_16X = 0x0020;

            _SettableData.Clear();

            if (Convert.ToBoolean(wsettabledata & DATABITS_5))
                _SettableData.Add(new KeyValuePair<string, int>("5", 5));

            if (Convert.ToBoolean(wsettabledata & DATABITS_6))
                _SettableData.Add(new KeyValuePair<string, int>("6", 6));

            if (Convert.ToBoolean(wsettabledata & DATABITS_7))
                _SettableData.Add(new KeyValuePair<string, int>("7", 7));

            if (Convert.ToBoolean(wsettabledata & DATABITS_8))
                _SettableData.Add(new KeyValuePair<string, int>("8", 8));

            if (Convert.ToBoolean(wsettabledata & DATABITS_16))
                _SettableData.Add(new KeyValuePair<string, int>("16", 16));

            if (Convert.ToBoolean(wsettabledata & DATABITS_16X))
                _SettableData.Add(new KeyValuePair<string, int>("16X", 0));
        }


        public void getSettableBaud(Int32 dwsettablebaud)
        {
            const Int32 BAUD_075 = 0x00000001;
            const Int32 BAUD_110 = 0x00000002;
            //const Int32 BAUD_134_5 = 0x00000004;
            const Int32 BAUD_150 = 0x00000008;
            const Int32 BAUD_300 = 0x00000010;
            const Int32 BAUD_600 = 0x00000020;
            const Int32 BAUD_1200 = 0x00000040;
            const Int32 BAUD_1800 = 0x00000080;
            const Int32 BAUD_2400 = 0x00000100;
            const Int32 BAUD_4800 = 0x00000200;
            const Int32 BAUD_7200 = 0x00000400;
            const Int32 BAUD_9600 = 0x00000800;
            const Int32 BAUD_14400 = 0x00001000;
            const Int32 BAUD_19200 = 0x00002000;
            const Int32 BAUD_38400 = 0x00004000;
            const Int32 BAUD_56K = 0x00008000;
            const Int32 BAUD_57600 = 0x00040000;
            const Int32 BAUD_115200 = 0x00020000;
            const Int32 BAUD_128K = 0x00010000;

            _SettableBaud.Clear();

            if (Convert.ToBoolean(dwsettablebaud & BAUD_075))
                _SettableBaud.Add(new KeyValuePair<string, int>("75", 75));

            if (Convert.ToBoolean(dwsettablebaud & BAUD_110))
                _SettableBaud.Add(new KeyValuePair<string, int>("110", 110));

            if (Convert.ToBoolean(dwsettablebaud & BAUD_150))
                _SettableBaud.Add(new KeyValuePair<string, int>("150", 150));

            if (Convert.ToBoolean(dwsettablebaud & BAUD_300))
                _SettableBaud.Add(new KeyValuePair<string, int>("300", 300));

            if (Convert.ToBoolean(dwsettablebaud & BAUD_600))
                _SettableBaud.Add(new KeyValuePair<string, int>("600", 600));

            if (Convert.ToBoolean(dwsettablebaud & BAUD_1200))
                _SettableBaud.Add(new KeyValuePair<string, int>("1200", 1200));

            if (Convert.ToBoolean(dwsettablebaud & BAUD_1800))
                _SettableBaud.Add(new KeyValuePair<string, int>("1800", 1800));

            if (Convert.ToBoolean(dwsettablebaud & BAUD_2400))
                _SettableBaud.Add(new KeyValuePair<string, int>("2400", 2400));

            if (Convert.ToBoolean(dwsettablebaud & BAUD_4800))
                _SettableBaud.Add(new KeyValuePair<string, int>("4800", 4800));

            if (Convert.ToBoolean(dwsettablebaud & BAUD_7200))
                _SettableBaud.Add(new KeyValuePair<string, int>("7200", 7200));

            if (Convert.ToBoolean(dwsettablebaud & BAUD_9600))
                _SettableBaud.Add(new KeyValuePair<string, int>("9600", 9600));

            if (Convert.ToBoolean(dwsettablebaud & BAUD_14400))
                _SettableBaud.Add(new KeyValuePair<string, int>("14400", 14400));

            if (Convert.ToBoolean(dwsettablebaud & BAUD_19200))
                _SettableBaud.Add(new KeyValuePair<string, int>("19200", 19200));

            if (Convert.ToBoolean(dwsettablebaud & BAUD_38400))
                _SettableBaud.Add(new KeyValuePair<string, int>("38400", 38400));

            if (Convert.ToBoolean(dwsettablebaud & BAUD_56K))
                _SettableBaud.Add(new KeyValuePair<string, int>("56K", 56000));

            if (Convert.ToBoolean(dwsettablebaud & BAUD_57600))
                _SettableBaud.Add(new KeyValuePair<string, int>("57600", 57600));

            if (Convert.ToBoolean(dwsettablebaud & BAUD_115200))
                _SettableBaud.Add(new KeyValuePair<string, int>("115200", 115200));

            if (Convert.ToBoolean(dwsettablebaud & BAUD_128K))
                _SettableBaud.Add(new KeyValuePair<string, int>("128K", 128000));
        }

        public void getSettableStopBits(UInt16 wSettableStopParity)
        {
            const UInt16 STOPBITS_10 = 0x0001;
            const UInt16 STOPBITS_15 = 0x0002;
            const UInt16 STOPBITS_20 = 0x0004;

            _SettableStopBits.Clear();

            if (Convert.ToBoolean(wSettableStopParity & STOPBITS_10))
                _SettableStopBits.Add(new KeyValuePair<string, int>(StopBits.One.ToString(), (int)StopBits.One));

            if (Convert.ToBoolean(wSettableStopParity & STOPBITS_15))
                _SettableStopBits.Add(new KeyValuePair<string, int>(StopBits.OnePointFive.ToString(), (int)StopBits.OnePointFive));

            if (Convert.ToBoolean(wSettableStopParity & STOPBITS_20))
                _SettableStopBits.Add(new KeyValuePair<string, int>(StopBits.Two.ToString(), (int)StopBits.Two));
        }

        public void getSettableParity(UInt16 wSettableStopParity)
        {
            const UInt16 PARITY_NONE = 0x0100;
            const UInt16 PARITY_ODD = 0x0200;
            const UInt16 PARITY_EVEN = 0x0400;
            const UInt16 PARITY_MARK = 0x0800;
            const UInt16 PARITY_SPACE = 0x1000;

            _SettableParity.Clear();

            if (Convert.ToBoolean(wSettableStopParity & PARITY_NONE))
                _SettableParity.Add(new KeyValuePair<string, int>(Parity.None.ToString(), (int)Parity.None));

            if (Convert.ToBoolean(wSettableStopParity & PARITY_ODD))
                _SettableParity.Add(new KeyValuePair<string, int>(Parity.Odd.ToString(), (int)Parity.Odd));

            if (Convert.ToBoolean(wSettableStopParity & PARITY_EVEN))
                _SettableParity.Add(new KeyValuePair<string, int>(Parity.Even.ToString(), (int)Parity.Even));

            if (Convert.ToBoolean(wSettableStopParity & PARITY_MARK))
                _SettableParity.Add(new KeyValuePair<string, int>(Parity.Mark.ToString(), (int)Parity.Mark));

            if (Convert.ToBoolean(wSettableStopParity & PARITY_SPACE))
                _SettableParity.Add(new KeyValuePair<string, int>(Parity.Space.ToString(), (int)Parity.Space));
        }

        public void getSettableHandshake(Int32 dwProvCapabilities)
        {
            const Int32 PCF_RTSCTS = 0x0002;
            const Int32 PCF_XONXOFF = 0x0010;

            _SettableHandshake.Clear();

            _SettableHandshake.Add(new KeyValuePair<string, int>(Handshake.None.ToString(), (int)Handshake.None));

            if (Convert.ToBoolean(dwProvCapabilities & PCF_RTSCTS))
                _SettableHandshake.Add(new KeyValuePair<string, int>(Handshake.RequestToSend.ToString(), (int)Handshake.RequestToSend));

            if (Convert.ToBoolean(dwProvCapabilities & PCF_XONXOFF))
                _SettableHandshake.Add(new KeyValuePair<string, int>(Handshake.XOnXOff.ToString(), (int)Handshake.XOnXOff));
        }
        #endregion

        #region Event handlers
        /// <summary>
        /// Get incoming data from port.
        /// </summary>
        /// <remarks>Deal with port related activities within class and send data as byte array to subscribers.</remarks>
        private void dataReceivedHandler(object sender, SerialDataReceivedEventArgs args)
        {
            if (!_serialPortNet.IsOpen)
                return;

            int BytesToRead = _serialPortNet.BytesToRead;

            try
            {
                _SerialDataBytesArgs drb_args = new _SerialDataBytesArgs(BytesToRead);

                drb_args.NumBytes = _serialPortNet.Read(drb_args.BytesOut, 0, BytesToRead);

                if ((drb_args.NumBytes > 0))
                {
                    // Send byte array to subscribers.
                    if (NewSerialDataBytesReceived != null)
                    {
                        NewSerialDataBytesReceived(this, drb_args);
                    }

                    // Send string to subscribers.
                    if (NewSerialDataStringReceived != null)
                    {
                        NewSerialDataStringReceived(
                            this, new _SerialDataStringArgs(Encoding.ASCII.GetString(drb_args.BytesOut, 0, drb_args.NumBytes)));
                    }
                }
            }
            catch (Exception e)
            {
                throw new Exception("Data handler " + e.Message);
            }
        }


        /// <summary>
        /// Allow a client to subscribe to data bytes events.
        /// </summary>
        public void SetClientDataHandler(Action<object, _SerialDataBytesArgs> handler)
        {
            NewSerialDataBytesReceived += new EventHandler<_SerialDataBytesArgs>(handler);
        }

        /// <summary>
        /// Allow a client to subscribe to data string events.
        /// </summary>
        public void SetClientDataHandler(Action<object, _SerialDataStringArgs> handler)
        {
            NewSerialDataStringReceived += new EventHandler<_SerialDataStringArgs>(handler);
        }

        #endregion
    }

    #region Event handler data types
    /// <summary>
    /// Encapsulate forwarded data as byte array.
    /// </summary>
    /// <remarks>Allows us to package incoming data from port and forward it to subscribers.</remarks>
    public class _SerialDataBytesArgs : EventArgs
    {
        private byte[] _bytesHeld;
        private int _numBytesHeld;

        public _SerialDataBytesArgs(byte[] BytesIn)
        {
            _bytesHeld = BytesIn;
        }

        public _SerialDataBytesArgs(int BytesToRead)
        {
            _bytesHeld = new byte[BytesToRead];
        }

        public byte[] BytesOut
        {
            get
            {
                return _bytesHeld;
            }
            set
            {
                _bytesHeld = value;
            }
        }

        public int NumBytes
        {
            get
            {
                return _numBytesHeld;
            }
            set
            {
                _numBytesHeld = value;
            }
        }
    }

    /// <summary>
    /// Encapsulate forwarded data as string.
    /// </summary>
    public class _SerialDataStringArgs : EventArgs
    {
        private string _string;

        public _SerialDataStringArgs(string STRING)
        {
            _string = STRING;
        }

        public string _String
        {
            get
            {
                return _string;
            }
            set
            {
                _string = value;
            }
        }
    }
    #endregion
}
