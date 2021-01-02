using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO.Ports;
using System.Xml;
using System.Xml.Serialization;
using System.IO;

namespace SerialComms.Manager
{
    /// <summary>
    /// Class to hold current settings for serial port with public properties for binding and serialising.
    /// </summary>
    public class _SerialPortSettings
    {
        #region Private_stores
        // Binding stores for public properties.
        private string _portName = "COM3";
        private int _baudRate = 9600;
        private int _dataBits = 8;
        private Parity _parity = Parity.None;
        private StopBits _stopBits = StopBits.One;
        private Handshake _handshake = Handshake.XOnXOff;
        public bool _useWMI = false;
        #endregion

        #region Public_properties
        // Properties for binding and XML serializer.
        public int _BaudRate
        {
            get
            {
                return _baudRate;
            }
            set
            {
                _baudRate = value;
            }
        }

        public Handshake _Handshake
        {
            get
            {
                return _handshake;
            }
            set
            {
                _handshake = value;
            }
        }

        public Parity _Parity
        {
            get
            {
                return _parity;
            }
            set
            {
                _parity = value;
            }
        }

        public string _PortName
        {
            get
            {
                return _portName;
            }
            set
            {
                _portName = value;
            }
        }

        public StopBits _StopBits
        {
            get
            {
                return _stopBits;
            }
            set
            {
                _stopBits = value;
            }
        }

        public int _DataBits
        {
            get
            {
                return _dataBits;
            }
            set
            {
                _dataBits = value;
            }
        }
        #endregion
    }

    #region XML
    /// <summary>
    /// Class to serialise settings to XML
    /// </summary>
    /// <remarks>
    /// The default directory is the location of the dll.
    /// No path selection is avaiable within this namespace and any alternate path must be supplied when entering via _SerialCommsManager
    /// </remarks>
    public class _SerialPortSettingsXML
    {
        /// <summary>
        /// The default XML file _name
        /// </summary>
        private const string DefaultXMLFileName = "SerialPortSettings.xml";

        /// <returns>An instance of _SerialPortSettings with either default values or values read from the XML file</returns>
        public _SerialPortSettings getSettings(string filePath, string fileName = DefaultXMLFileName)
        {
            string fileNamePath = getXMLFilePath(filePath, fileName);
            XmlSerializer XmlSerial = new XmlSerializer(typeof(_SerialPortSettings));
            _SerialPortSettings sdefault = new _SerialPortSettings(); // Create default settings.
 
            if (!File.Exists(fileNamePath)) // Create settings file with default values
            {
                using (FileStream fs = new FileStream(fileNamePath, FileMode.Create))
                {
                    XmlSerial.Serialize(fs, sdefault);
                }
                return sdefault;
            }
            else // Read settings from file
            {
                _SerialPortSettings sc = null;

                try
                {
                    using (FileStream fs = new FileStream(fileNamePath, FileMode.Open))
                    {
                        XmlReader reader = new XmlTextReader(fs);

                        if (XmlSerial.CanDeserialize(reader))
                            sc = (_SerialPortSettings)XmlSerial.Deserialize(reader);
                    }
                }
                catch (Exception ex)
                {
                    // File likely corrupt - delete and allow defaults.
                    File.Delete(fileNamePath);
                }

                // If problem has occurred sc is still null.
                return sc != null ? sc : sdefault;
            }
        }

        /// <summary>
        /// Updates an instance of _SerialPortSettings to the existing XML file
        /// </summary>
        public void saveSettings(_SerialPortSettings settings, string filePath, string fileName = DefaultXMLFileName)
        {
            string fileNamePath = getXMLFilePath(filePath, fileName);

            if (!File.Exists(fileName))
                return; // Don't do anything if file doesn't exist.

            using (FileStream fs = new FileStream(fileName, FileMode.Open))
            {
                XmlSerializer XmlSerial = new XmlSerializer(typeof(_SerialPortSettings));
                XmlSerial.Serialize(fs, settings);
            }
        }

        private string getXMLFilePath(string strPath, string strName)
        {
            string fileNamePath = null;

            if (!String.IsNullOrEmpty(strPath))
            {
                fileNamePath = strPath;
                fileNamePath += "\\";
                fileNamePath += strName;
            }
            else
                fileNamePath = strName;

            return fileNamePath;
        }
    }
}
    #endregion
