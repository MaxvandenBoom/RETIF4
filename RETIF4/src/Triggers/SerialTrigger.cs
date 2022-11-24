/**
 * SerialTrigger class
 * 
 * Copyright (C) 2022  Max van den Boom (Nick Ramsey Lab, University Medical Center Utrecht, The Netherlands)
 *
 * This program is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software
 * Foundation, either version 3 of the License, or (at your option) any later version. This program is distributed in the hope that it will be useful, but
 * WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for
 * more details. You should have received a copy of the GNU General Public License along with this program. If not, see <https://www.gnu.org/licenses/>.
 */
using System;
using RETIF4.Events;
using RETIF4.Helpers;
using NLog;
using System.IO.Ports;
using System.Collections.Specialized;
using System.Configuration;
using System.Windows.Forms;

namespace RETIF4.Triggers {

    public class SerialTrigger : ITrigger {

        private static Logger logger = LogManager.GetLogger("SerialTrigger");

        private string configPort = "COM1";
        private int configBaudrate = 9600;
        private Parity configParity = Parity.None;
        private int configDataBits = 8;
        private StopBits configStopBits = StopBits.One;

        private Object lockSerialPort = new Object();
        private SerialPort serialPort = null;
        private bool listening = false;

        ////
        //
        ////


        // A 'trigger' event(handler). An EventHandler delegate is associated with the event.
        // methods should be subscribed to this object
        public event EventHandler<TriggerEventArgs> triggerHandler;

        public void simulateTriggerEvent(int value) {

            // message
    	    Console.WriteLine("trigger --> simulate fire event");
            
            // raise the event
            raiseTriggerEvent(value);

        }

        private void raiseTriggerEvent(int value) {

            // fire event
            TriggerEventArgs args = new TriggerEventArgs();
            args.value = value;
            args.datetime = DateHelper.getDateTimeWithMs();
            EventHandler<TriggerEventArgs> handler = triggerHandler;
            if (handler != null)    handler(this, args);

        }

        ////
        //
        ////

        public SerialTrigger() {

            //
            // retrieve serial settings from the app.config
            // 

            NameValueCollection appSettings = ConfigurationManager.AppSettings;
            
            string strPort = appSettings["SerialPort"] ?? "";
            if (!string.IsNullOrEmpty(strPort))                 configPort = strPort;

            string strBaudrate = appSettings["SerialBaudrate"] ?? "";
            if (!string.IsNullOrEmpty(strBaudrate))             int.TryParse(strBaudrate, out configBaudrate);

            string strParity = appSettings["SerialParity"] ?? "";
            if (string.Compare(strParity, "even", true) == 0)   configParity = Parity.Even;
            if (string.Compare(strParity, "mark", true) == 0)   configParity = Parity.Mark;
            if (string.Compare(strParity, "none", true) == 0)   configParity = Parity.None;
            if (string.Compare(strParity, "odd", true) == 0)    configParity = Parity.Odd;
            if (string.Compare(strParity, "space", true) == 0)  configParity = Parity.Space;
            
            string strDataBits = appSettings["SerialDataBits"] ?? "";
            if (!string.IsNullOrEmpty(strDataBits)) int.TryParse(strDataBits, out configDataBits);

            string strStopBits = appSettings["SerialStopBits"] ?? "";
            if (string.Compare(strStopBits, "None", true) == 0 || string.Compare(strStopBits, "0", true) == 0)              configStopBits = StopBits.None;
            if (string.Compare(strStopBits, "One", true) == 0 || string.Compare(strStopBits, "1", true) == 0)               configStopBits = StopBits.One;
            if (string.Compare(strStopBits, "OnePointFive", true) == 0 || string.Compare(strStopBits, "1.5", true) == 0)    configStopBits = StopBits.OnePointFive;
            if (string.Compare(strStopBits, "Two", true) == 0 || string.Compare(strStopBits, "2", true) == 0)               configStopBits = StopBits.Two;

            // check if the serial port exists
            bool portFound = false;
            string[] portsAvailable = SerialPort.GetPortNames();
            foreach (string s in portsAvailable) {
                if (string.Compare(s, strPort, true) == 0) {
                    portFound = true;
                    break;
                }
            }
            if (!portFound && portsAvailable.Length > 0) {

                // allow the user to choose a port by popup
                configPort = ListMessageBox.ShowSingle("Select serial port", SerialPort.GetPortNames());

            }

            // thread safety
            lock (lockSerialPort) { 

                try {

                    // create the serialport object with the given settings
                    serialPort = new SerialPort(configPort, configBaudrate, configParity, configDataBits, configStopBits);
                    //serialPort.ReadBufferSize = 40960;	// Set read buffer == 10K
                    //serialPort.WriteBufferSize = 4096;	// Set write buffer == 4K
                    //serialPort.DtrEnable = false;   // no hardware handshake (DSR/DTR handshake)
                    //serialPort.RtsEnable = false;   // no hardware handshake (RTS/CTS handshake)
                    //serialPort.ReadTimeout = 500;
                    //serialPort.WriteTimeout = 500;

                    // Attach a method to be called when there is data waiting in the port's buffer
                    serialPort.DataReceived += new SerialDataReceivedEventHandler(serialPort_DataReceived);

                    // try to open the serialport
                    serialPort.Open();
                    
                    // message
                    logger.Info("Opened serial port (port: " + configPort + "; baudrate: " + configBaudrate + "; parity: " + configParity.ToString() + "; databits: " + configDataBits + "; stopbits: " + configStopBits.ToString() + ")");

                } catch (Exception) {

                    // message
                    logger.Error("Could not open serial port (port: " + configPort + "; baudrate: " + configBaudrate + "; parity: " + configParity.ToString() + "; databits: " + configDataBits + "; stopbits: " + configStopBits.ToString() + "), not receiving triggers");

                    // set serialport to null
                    serialPort = null;

                }

            }

        }


        public string getComPort() {
            return configPort;
        }

        public bool start() {

            // thread safety
            lock (lockSerialPort) {

                // discard anything in the buffers
                if (serialPort != null) {
                    serialPort.DiscardInBuffer();
                    serialPort.DiscardOutBuffer();
                }

                // flag to listen
                listening = true;

                // return success
                return true;

            }

        }

        public void stop() {

            // thread safety
            lock (lockSerialPort) {

                // flag to stop listening
                listening = false;

            }

        }
        
        public bool isListening() {

            lock (lockSerialPort) {
                return listening;
            }

        }

        public void destroy() {

            // stop listening
            stop();

            // thread safety
            lock (lockSerialPort) {

                // close the serial port
                if (serialPort != null)     serialPort.Close();

            }
            
        }

        private void serialPort_DataReceived(object s, SerialDataReceivedEventArgs e) {

            // thread safety
            lock (lockSerialPort) {

                // check if we are listening
                if (listening) {

                    if (serialPort != null) {

                        // read the data
                        byte[] data = new byte[serialPort.BytesToRead];
                        serialPort.Read(data, 0, data.Length);

                        // send every byte as an trigger event
                        if (data.Length == 1)
                            raiseTriggerEvent(data[0]);
                        else {
                            for (int i = 0; i < data.Length; i++)   raiseTriggerEvent(data[i]);
                        }

                    }

                }
                
            } // end lock

        } // end function


    }

}
