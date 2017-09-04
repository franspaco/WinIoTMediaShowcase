using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net.Http;
using Windows.ApplicationModel.Background;
using Windows.Devices.SerialCommunication;
using Windows.Storage.Streams;
using System.Diagnostics;
using Windows.Devices.Enumeration;
using System.Threading.Tasks;
using System.Threading;

// The Background Application template is documented at http://go.microsoft.com/fwlink/?LinkID=533884&clcid=0x409

namespace SerialReadTest {
    public sealed class StartupTask : IBackgroundTask {

        SerialDevice serialPort;
        DataReader dataReaderObject;
        DataWriter dataWriterObject;
        CancellationTokenSource cts;

        public async void Run(IBackgroundTaskInstance taskInstance)
 {
            this.
            // 
            // TODO: Insert code to perform background work
            //
            // If you start any asynchronous methods here, prevent the task
            // from closing prematurely by using BackgroundTaskDeferral as
            // described in http://aka.ms/backgroundtaskdeferral
            //

            cts = new CancellationTokenSource();

            string aqs = SerialDevice.GetDeviceSelector();
            Debug.WriteLine(aqs);
            var dev = await DeviceInformation.FindAllAsync(aqs);

            if (dev.Any()) {
                string deviceId = dev.First().Id;
                Debug.WriteLine($"ID: {deviceId}");
                await OpenPort(deviceId);

                Debug.WriteLine("Starting listening task!");

                for(int i = 0; i < 10; i++) {
                    await Listen();
                }

                Debug.WriteLine("Pre Dispose");
                serialPort?.Dispose();
                Debug.WriteLine("Pos Dispose");
            }

        }

        private async Task OpenPort(string deviceId) {
            serialPort = await SerialDevice.FromIdAsync(deviceId);
            Debug.WriteLine("Serial port received.");
            if (serialPort != null) {
                serialPort.WriteTimeout = TimeSpan.FromMilliseconds(1000);
                serialPort.ReadTimeout = TimeSpan.FromMilliseconds(1000);
                serialPort.BaudRate = 9600;
                serialPort.Parity = SerialParity.None;
                serialPort.StopBits = SerialStopBitCount.One;
                serialPort.DataBits = 8;
                serialPort.Handshake = SerialHandshake.None;
                Debug.WriteLine("PORT OPEN");
            }
            else {
                Debug.WriteLine("Binding failed!");
            }
        }

        private async Task Listen() {
            if (serialPort != null) {
                dataReaderObject = new DataReader(serialPort.InputStream);
                await ReadAsync(cts.Token);
            }
        }

        private async Task ReadAsync(CancellationToken cancellationToken) {
            Task<UInt32> loadAsyncTask;

            uint ReadBufferLength = 1;  // only when this buffer would be full next code would be executed

            byte[] tagId = new byte[ReadBufferLength];
            //dataReaderObject.InputStreamOptions = InputStreamOptions.Partial;
            loadAsyncTask = dataReaderObject.LoadAsync(ReadBufferLength).AsTask(cancellationToken);   // Create a task object

            UInt32 bytesRead = await loadAsyncTask;    // Launch the task and wait until buffer would be full

            if (bytesRead > 0) {
                Debug.WriteLine("READING!");
                dataReaderObject.ReadBytes(tagId);
                //string strFromPort = dataReaderObject.ReadString(bytesRead);
                foreach(byte b in tagId) {
                    Debug.Write(b.ToString("X"));
                }
                Debug.WriteLine("");
            }
        }
    }
}
