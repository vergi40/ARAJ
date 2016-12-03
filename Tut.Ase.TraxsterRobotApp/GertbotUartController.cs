// Petri Kannisto
// Tampere University of Technology
// Department of Automation Science and Engineering
// File created: 10/2016
// Last modified: 11/2016

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Windows.Devices.Enumeration;
using Windows.Devices.SerialCommunication;
using Windows.Storage.Streams;

namespace Tut.Ase.TraxsterRobotApp
{
    /// <summary>
    /// Wraps a UART channel connected to GertBot.
    /// </summary>
    class GertbotUartController : IDisposable
    {
        /// <summary>
        /// End byte in commands.
        /// </summary>
        public const byte END_BYTE = 0x50;

        private const byte START_BYTE = 0xA0;
        
        // To keep the program structure easy to understand, semaphores are used
        // in each public method (excluding Dispose) and not anywhere else.
        private System.Threading.SemaphoreSlim semaphore;

        private SerialDevice serialPort = null;
        private DataWriter dataWriterObject = null;
        private DataReader dataReaderObject = null;


        /// <summary>
        /// Constructor.
        /// </summary>
        public GertbotUartController()
        {
            this.semaphore = new System.Threading.SemaphoreSlim(
                1, // Initial count > 0: the semaphore is initially free
                1  // Max count: only one thread can enter at a time
                );
        }

        /// <summary>
        /// Disposes the object.
        /// </summary>
        public void Dispose()
        {
            if (this.dataReaderObject != null)
            {
                try
                {
                    this.dataReaderObject.DetachStream();
                    this.dataReaderObject.Dispose();
                    this.dataReaderObject = null;
                }
                catch { }
            }

            if (this.dataWriterObject != null)
            {
                try
                {
                    this.dataWriterObject.DetachStream();
                    this.dataWriterObject.Dispose();
                    this.dataWriterObject = null;
                }
                catch { }
            }

            if (this.serialPort != null)
            {
                try
                {
                    this.serialPort.Dispose();
                    this.serialPort = null;
                }
                catch { }
            }

            if (this.semaphore != null)
            {
                try
                {
                    this.semaphore.Dispose();
                    this.semaphore = null;
                }
                catch { }
            }
        }

        /// <summary>
        /// Sends a command. Start and end bytes are added by this function.
        /// </summary>
        /// <param name="input">Bytes to be sent.</param>
        /// <returns>Task.</returns>
        public async Task sendCommand(List<byte> input)
        {
            // Mutual exclusion is here to prevent concurrency problems
            await this.semaphore.WaitAsync();

            try
            {
                await sendCommand_impl(input);
            }
            finally
            {
                this.semaphore.Release();
            }
        }

        /// <summary>
        /// Requests for an item. Start and end bytes are added by this function.
        /// </summary>
        /// <param name="input">Bytes to be sent.</param>
        /// <returns>Response.</returns>
        public async Task<List<byte>> readItem(List<byte> input)
        {
            // Mutual exclusion is here to prevent concurrency problems
            await this.semaphore.WaitAsync();

            var startTime = DateTime.Now;

            try
            {
                await sendCommand_impl(input);

                var middleTime = DateTime.Now;

                // Reading response
                // TODO-later: If there is anything previously unread bytes awaiting, they will be returned first! How to remove them?
                var response = await readFromSerialPort();

                var endTime = DateTime.Now;

                var checkPoint1 = (middleTime - startTime).TotalMilliseconds;
                var checkPoint2 = (endTime - startTime).TotalMilliseconds;

                return new List<byte>(response);
            }
            finally
            {
                this.semaphore.Release();
            }
        }

        private async Task sendCommand_impl(List<byte> input)
        {
            // This method exists due to mutual exclusion. If sendCommand() were called in getValue(),
            // the same thread would call wait() on the same semaphore twice.
            
            if (this.serialPort == null)
            {
                // Lazy initialization
                await initSerialPort();
            }

            try
            {
                // Adding start and end bytes to message
                input.Insert(0, START_BYTE);
                input.Add(END_BYTE);

                // Writing to channel
                this.dataWriterObject.WriteBytes(input.ToArray());
                await this.dataWriterObject.StoreAsync();
            }
            catch (Exception e)
            {
                throw new Exception("UART send command failed: " + e.Message, e);
            }
        }

        private async System.Threading.Tasks.Task initSerialPort()
        {
            // https://developer.microsoft.com/en-us/windows/iot/samples/serialuart
            // http://stackoverflow.com/questions/36380925/how-to-write-serial-data-with-c-sharp-in-a-universal-windows-application

            // Make sure Package.appxmanifest contains the following capability.
            // This cannot be added with the graphical manifest editor!
            // Also, using the editor will make these lines disappear.
            //
            // <Capabilities>
            //   <DeviceCapability Name="serialcommunication">
            //     <Device Id="any">
            //       <Function Type="name:serialPort" />
            //     </Device>
            //   </DeviceCapability>
            // </Capabilities>

            // No exception handling required here if this function is called with the "await" keyword!

            if (this.serialPort != null)
            {
                throw new InvalidOperationException();
            }

            // Connecting to serial port
            var selector = SerialDevice.GetDeviceSelector("UART0");
            var serialPortDevices = await DeviceInformation.FindAllAsync(selector);

            if (serialPortDevices.Count != 1)
            {
                throw new Exception("Expected one serial port, got " + serialPortDevices.Count);
            }

            var deviceInfo = serialPortDevices[0];
            this.serialPort = await SerialDevice.FromIdAsync(deviceInfo.Id);

            // Configuring serial settings.
            // Timeouts do not seem to occur at all. However, too high timeout values seem to make responses slow.
            // I expect that the serial implementation in IoT Core is so crappy that timeouts actually
            // specify the maximum wait time.
            this.serialPort.WriteTimeout = TimeSpan.FromMilliseconds(5);
            this.serialPort.ReadTimeout = TimeSpan.FromMilliseconds(5);
            this.serialPort.BaudRate = 57600;
            this.serialPort.Parity = SerialParity.None;
            this.serialPort.StopBits = SerialStopBitCount.One;
            this.serialPort.DataBits = 8;
            this.serialPort.Handshake = SerialHandshake.None;
            
            // Creating data reader and writer
            this.dataReaderObject = new DataReader(this.serialPort.InputStream);
            //this.dataReaderObject.InputStreamOptions = InputStreamOptions.None; // In the example, this value was "partial"
            this.dataReaderObject.InputStreamOptions = InputStreamOptions.Partial; // In the example, this value was "partial"
            //this.dataReaderObject.InputStreamOptions = InputStreamOptions.ReadAhead; // In the example, this value was "partial"
            this.dataWriterObject = new DataWriter(this.serialPort.OutputStream);
            this.dataWriterObject.UnicodeEncoding = UnicodeEncoding.Utf8;
        }
        
        private async System.Threading.Tasks.Task<byte[]> readFromSerialPort()
        {
            // Wait for data...
            // Lowering the "count" parameter of LoadAsync did not apper to shorten serial response times
            UInt32 bytesRead = await this.dataReaderObject.LoadAsync(1024).AsTask();

            // Getting bytes from serial
            byte[] retval = new byte[bytesRead];

            if (bytesRead > 0)
            {
                this.dataReaderObject.ReadBytes(retval);
            }

            return retval;
        }
    }
}
