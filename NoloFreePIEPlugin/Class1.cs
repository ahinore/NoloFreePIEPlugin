using FreePIE.Core.Contracts;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Net;
using System.Net.Sockets;

using System.Reactive.Linq;
using System.Reactive.Disposables;


namespace NoloFreePIEPlugin
{
    [GlobalType(Type = typeof(NoloDataTrackerFreePIEPluginGlobal))]
    public class MyNoloDataTrackerFreePIEPlugin : IPlugin
    {
        Boolean continueFlag;
        object dummy = new object();
        byte[] rawdata;
        NoloDataTrackerFreePIEPluginGlobal global;

        DateTime hapticStart = DateTime.MinValue;

        UdpClient remoteClient;

        int localPort;

        public object CreateGlobal()
        {
            return global = new NoloDataTrackerFreePIEPluginGlobal(this);
        }

        public Action Start()
        {
            Observable.Create<string>(async o =>
            {
                continueFlag = true;

                var local = new IPEndPoint(IPAddress.Any, localPort);
                var client = new UdpClient(local);
                client.Client.ReceiveTimeout = 3000;

                try
                {
                    while (continueFlag)
                    {

                        try
                        {
                            var result = await Task.Run(() =>
                            {
                                var task = client.ReceiveAsync();
                                task.Wait(3000);
                                if (task.IsCompleted)
                                { return task.Result; }
                                throw new TimeoutException();
                            });

                            bool taken = false;
                            try
                            {
                                Monitor.Enter(dummy, ref taken);
                                rawdata = result.Buffer;
                            }
                            finally
                            {
                                if (taken) Monitor.Exit(dummy);
                            }

                            if (remoteClient == null)
                            {
                                remoteClient = new UdpClient(localPort + 1);
                                var remote = new IPEndPoint(result.RemoteEndPoint.Address, localPort);
                                remoteClient.Connect(remote);
                            }
                        }
                        catch (SocketException e)
                        {
                            o.OnNext("Time Out");
                        }
                    }
                }
                finally
                {
                    client.Close();
                    if (remoteClient != null)
                    {
                        remoteClient.Close();
                        remoteClient = null;
                    }

                    o.OnNext("Thread Stopped");
                }
                return Disposable.Empty;

            }).Subscribe(x =>
            {
                Console.WriteLine(x);
            });

            return null;
        }

        public void Stop()
        {
            continueFlag = false;
        }

        private void send_message(String data)
        {

            if (remoteClient == null)
            {
                return;
            }

            var message = Encoding.UTF8.GetBytes(data);
            remoteClient.Send(message, message.Length);
        }


        public event EventHandler Started;

        public string FriendlyName
        {
            get { return "Nolo Data Tracker FreePIE Plugin"; }
        }

        public bool GetProperty(int index, IPluginProperty property)
        {
            if (index == 0)
            {
                property.Name = "localPort";
                property.Caption = "Local port number";
                property.DefaultValue = 5678;
                property.Value = 5678;
                property.HelpText = "Local port number";
                return true;
            }
            return false;
        }

        public bool SetProperties(Dictionary<string, object> properties)
        {
            localPort = (int)properties["localPort"];
            return true;
        }

        public void DoBeforeNextExecute()
        {

            if (rawdata == null)
            {
                return;
            }

            String sdata;

            bool taken = false;
            try
            {
                Monitor.Enter(dummy, ref taken);
                sdata = Encoding.UTF8.GetString(rawdata);
            }
            finally
            {
                if (taken) Monitor.Exit(dummy);
            }

            String[] data = sdata.Split(',');

            global.poseHmd[0] = Convert.ToDouble(data[0]);
            global.poseHmd[1] = Convert.ToDouble(data[1]);
            global.poseHmd[2] = Convert.ToDouble(data[2]);

            global.quaternionHmd[0] = Convert.ToDouble(data[3]);
            global.quaternionHmd[1] = Convert.ToDouble(data[4]);
            global.quaternionHmd[2] = Convert.ToDouble(data[5]);
            global.quaternionHmd[3] = Convert.ToDouble(data[6]);
            global.vecAngularVelocityHmd[0] = Convert.ToDouble(data[7]);
            global.vecAngularVelocityHmd[1] = Convert.ToDouble(data[8]);
            global.vecAngularVelocityHmd[2] = Convert.ToDouble(data[9]);
            global.vecVelocityHmd[0] = Convert.ToDouble(data[10]);
            global.vecVelocityHmd[1] = Convert.ToDouble(data[11]);
            global.vecVelocityHmd[2] = Convert.ToDouble(data[12]);


            global.poseLeft[0] = Convert.ToDouble(data[13]);
            global.poseLeft[1] = Convert.ToDouble(data[14]);
            global.poseLeft[2] = Convert.ToDouble(data[15]);
            global.quaternionLeft[0] = Convert.ToDouble(data[16]);
            global.quaternionLeft[1] = Convert.ToDouble(data[17]);
            global.quaternionLeft[2] = Convert.ToDouble(data[18]);
            global.quaternionLeft[3] = Convert.ToDouble(data[19]);
            global.vecAngularVelocityLeft[0] = Convert.ToDouble(data[20]);
            global.vecAngularVelocityLeft[1] = Convert.ToDouble(data[21]);
            global.vecAngularVelocityLeft[2] = Convert.ToDouble(data[22]);
            global.vecVelocityLeft[0] = Convert.ToDouble(data[23]);
            global.vecVelocityLeft[1] = Convert.ToDouble(data[24]);
            global.vecVelocityLeft[2] = Convert.ToDouble(data[25]);
            global.controllerLeftButtons = Convert.ToInt32(data[26]);
            global.controllerLeftTouches = Convert.ToInt32(data[27]);
            global.controllerLeftAxisX = Convert.ToDouble(data[28]);
            global.controllerLeftAxisY = Convert.ToDouble(data[29]);

            global.poseRight[0] = Convert.ToDouble(data[30]);
            global.poseRight[1] = Convert.ToDouble(data[31]);
            global.poseRight[2] = Convert.ToDouble(data[32]);
            global.quaternionRight[0] = Convert.ToDouble(data[33]);
            global.quaternionRight[1] = Convert.ToDouble(data[34]);
            global.quaternionRight[2] = Convert.ToDouble(data[35]);
            global.quaternionRight[3] = Convert.ToDouble(data[36]);
            global.vecAngularVelocityRight[0] = Convert.ToDouble(data[37]);
            global.vecAngularVelocityRight[1] = Convert.ToDouble(data[38]);
            global.vecAngularVelocityRight[2] = Convert.ToDouble(data[39]);
            global.vecVelocityRight[0] = Convert.ToDouble(data[40]);
            global.vecVelocityRight[1] = Convert.ToDouble(data[41]);
            global.vecVelocityRight[2] = Convert.ToDouble(data[42]);
            global.controllerRightButtons = Convert.ToInt32(data[43]);
            global.controllerRightTouches = Convert.ToInt32(data[44]);
            global.controllerRightAxisX = Convert.ToDouble(data[45]);
            global.controllerRightAxisY = Convert.ToDouble(data[46]);

            if ((global.controllerLeftHapticPuls != 0 || global.controllerRightHapticPuls != 0) && hapticStart == DateTime.MinValue)
            {
                hapticStart = DateTime.Now;
                send_message(global.controllerLeftHapticPuls + "," + global.controllerRightHapticPuls);
            }

            if (hapticStart != DateTime.MinValue)
            {
                TimeSpan ts = DateTime.Now - hapticStart;
                if (ts.Milliseconds > 100)
                {
                    hapticStart = DateTime.MinValue;
                }
            }
        }
    }

    [Global(Name = "noloDataTracker")]
    public class NoloDataTrackerFreePIEPluginGlobal
    {
        private readonly MyNoloDataTrackerFreePIEPlugin plugin;

        public NoloDataTrackerFreePIEPluginGlobal(MyNoloDataTrackerFreePIEPlugin plugin)
        {
            this.plugin = plugin;
        }

        public double[] poseHmd { get; set; } = new double[3];
        public double[] quaternionHmd { get; set; } = new double[4];
        public double[] vecAngularVelocityHmd { get; set; } = new double[3];
        public double[] vecVelocityHmd { get; set; } = new double[3];

        public double[] poseLeft { get; set; } = new double[3];
        public double[] quaternionLeft { get; set; } = new double[4];
        public double[] vecAngularVelocityLeft { get; set; } = new double[3];
        public double[] vecVelocityLeft { get; set; } = new double[3];
        public int controllerLeftButtons { get; set; }
        public int controllerLeftTouches { get; set; }
        public double controllerLeftAxisX { get; set; }
        public double controllerLeftAxisY { get; set; }

        public double[] poseRight { get; set; } = new double[3];
        public double[] quaternionRight { get; set; } = new double[4];
        public double[] vecAngularVelocityRight { get; set; } = new double[3];
        public double[] vecVelocityRight { get; set; } = new double[3];
        public int controllerRightButtons { get; set; }
        public int controllerRightTouches { get; set; }
        public double controllerRightAxisX { get; set; }
        public double controllerRightAxisY { get; set; }

        public int controllerRightHapticPuls { get; set; }
        public int controllerLeftHapticPuls { get; set; }
    }
}
