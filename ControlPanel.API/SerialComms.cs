using System;
using SerialComms.Manager;
using System.Text;
using System.Threading;

namespace ControlPanel.API
{
   public interface ISerialComms
    {
        bool Send(string data);
        string GetStatus();
    }

    public class SerialComms : ISerialComms
    {
        private _SerialCommsManager serialCommsManager = null;
        private StringBuilder _status ;
        public SerialComms()
        {
            try
            {
                serialCommsManager = new _SerialCommsManager(DataBytesReceived);
                serialCommsManager.SCM_Start();
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }

        public bool Send (string data)
        {
            _SerialDataStringArgs myArgs = new _SerialDataStringArgs(data);
            //empty _status
            _status = new StringBuilder();
            serialCommsManager.SCM_Send(ref myArgs);
            return true;
        }
        void DataBytesReceived(object sender, _SerialDataBytesArgs data)
        {
            // Convert bytes to ASCII text.
            string _str = Encoding.ASCII.GetString(data.BytesOut, 0, data.NumBytes);
            if (!string.IsNullOrEmpty( _str))
            {
                if (_str.Contains('#'))
                {
                    _status.Append(_str.Substring(_str.IndexOf('#') + 1, 8));
                }
            }
            
            // textBoxData.AppendText(str + @"\r\n");

        }
        public string GetStatus ()
        {
            return _status.ToString();
        }
    }
}
