using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO.Ports;

namespace SerialComms.Manager
{
    public class _SerialCommsManager
    {
        /// <summary>
        /// The SerialComms.Manager _Serialport which contains a .net SerialPort
        /// </summary>
        /// <remarks></remarks>
        private _Serialport _serialPort = null;
        private readonly string _XMLsettingsPath = null;

        public _SerialCommsManager(Action<object, _SerialDataBytesArgs> handler, string XMLsettingsPath = "")
        {
            _XMLsettingsPath = XMLsettingsPath;
            getSerialPort();
            setHandler(handler);
        }

        public _SerialCommsManager(Action<object, _SerialDataStringArgs> handler, string XMLsettingsPath = "")
        {
            _XMLsettingsPath = XMLsettingsPath;
            getSerialPort();
            setHandler(handler);
        }

        ~_SerialCommsManager()
        {
            SCM_Stop();
            saveSettingsXML();
        }

        /// <summary>
        /// Bring up the settings dialog.
        /// </summary>
       
        public bool SCM_Start()
        {
            return _serialPort.start();
        }

        public bool SCM_Stop()
        {
            return _serialPort.stop();
        }

        /// <summary>
        /// Send data as byte array.
        /// </summary>
        /// <remarks>Write data byte array to the port for sending.</remarks>
        public bool SCM_Send(ref _SerialDataBytesArgs Data)
        {
            try
            {

                if (_serialPort.Send(ref Data))
                    return true;
            }
            catch (Exception)
            {
                throw new Exception("No open Serial (COM) port.");
            }
            return false;
           
        }

        /// <summary>
        /// Send data as string.
        /// </summary>
        /// <remarks>Write data string to the port for sending.</remarks>
        public bool SCM_Send(ref _SerialDataStringArgs Data)
        {
            try
            {
                if (_serialPort.Send(ref Data))
                    return true;
            }
            catch (Exception)
            {

                throw new Exception("No open Serial (COM) port.");
            }
            return false;
        }

        /// <summary>
        /// Set the callback for _SerialDataBytesArgs.
        /// </summary>
        /// <remarks>This is the first method to call after instantiating the manager.</remarks>
        private void setHandler(Action<object, _SerialDataBytesArgs> handler)
        {
            _serialPort.SetClientDataHandler(handler);
        }

        /// <summary>
        /// Set the callback for _SerialDataStringArgs.
        /// </summary>
        /// <remarks>This is the first method to call after instantiating the manager.</remarks>
        private void setHandler(Action<object, _SerialDataStringArgs> handler)
        {
            _serialPort.SetClientDataHandler(handler);
        }

        /// <summary>
        /// Get settings from XML file and create new serialport.
        /// </summary>
        private void getSerialPort()
        {
            try
            {
                // Get saved settings OR default settings.
                _SerialPortSettingsXML spsXML = new _SerialPortSettingsXML();
                _SerialPortSettings settings = spsXML.getSettings(_XMLsettingsPath);

                if (settings == null)
                    throw new Exception("Get XML failed");

                _serialPort = new _Serialport(settings);
            }
            catch (Exception e)
            {
                throw new Exception("Get serial port failed " + e.ToString());
            }
        }

        /// <summary>
        /// Save current settings to XML
        /// </summary>
        private void saveSettingsXML()
        {
            // Save settings.
            if (_serialPort != null)
            {
                if (_serialPort._Settings != null)
                {
                    _SerialPortSettingsXML spsXML = new _SerialPortSettingsXML();
                    spsXML.saveSettings(_serialPort._Settings, _XMLsettingsPath);
                }
            }
        }
    }
}
