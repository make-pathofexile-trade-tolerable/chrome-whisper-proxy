using System;
using System.Net;
using System.Net.Sockets;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using WindowsInput.Native;
using WindowsInput;

namespace chrome_pathofexile_trade_proxy
{
    class Program
    {
        [DllImport("user32.dll", EntryPoint = "FindWindow", SetLastError = true)]
        public static extern IntPtr FindWindow(IntPtr ZeroOnly, string lpWindowName);

        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        static extern bool SetForegroundWindow(IntPtr hWnd);

        static void Main(string[] args)
        {
            try
            {
                IPAddress localAddr = IPAddress.Parse("127.0.0.1");
                Int32 port = 11011;
                string lastMsg = "";

                TcpListener server = new TcpListener(localAddr, port);
                server.Start();

                Console.WriteLine("server listening, {0}", server.LocalEndpoint);

                while (true)
                {
                    Socket s = server.AcceptSocket();
                    Byte[] bytes = new Byte[2048];
                    int k = s.Receive(bytes);

                    string data = "";

                    for (int i = 0; i < k; i++)
                    {
                        data += Convert.ToChar(bytes[i]);
                    }

                    if (data != "")
                    {
                        // parse msg - start w/ @
                        string msg = data.Substring(data.IndexOf("@"), data.Length - data.IndexOf("@"));

                        if (lastMsg != msg)
                        {
                            SendToPoe(msg);
                            Console.WriteLine(msg);
                            lastMsg = msg;
                        }


                        s.Shutdown(SocketShutdown.Both);
                    }

                    s.Close();
                }
            }
            catch (Exception e)
            {
                Console.WriteLine("Exception: {0}", e);
            }
        }

        static void SendToPoe(string msg)
        {
            var poeHandle = FindWindow(IntPtr.Zero, "Path of Exile");

            if (poeHandle != IntPtr.Zero)
            {
                SetForegroundWindow(poeHandle);

                Task.Delay(100).ContinueWith((a) =>
                {
                    InputSimulator sim = new InputSimulator();
                    Random random = new Random();

                    sim.Keyboard.KeyPress(VirtualKeyCode.RETURN);

                    sim.Keyboard.Sleep(random.Next(25, 100));

                    sim.Keyboard.TextEntry(msg);

                    sim.Keyboard.Sleep(random.Next(25, 100));

                    sim.Keyboard.KeyPress(VirtualKeyCode.RETURN);
                });

            }
        }


    }
}
