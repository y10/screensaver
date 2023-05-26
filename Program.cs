using System.IO.Pipes;

namespace Screensaver
{
    internal static class Program
    {
        static bool firstInstance;

        static Form1 form;
        /// <summary>
        ///  The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            using (Mutex mutex = new Mutex(true, $"{typeof(Program)}", out firstInstance))
            {
                if (firstInstance)
                {
                    try
                    {
                        ThreadPool.QueueUserWorkItem((s) =>
                        {
                            WaitForConnection();
                        });

                        RunInstance();

                    }
                    finally
                    {
                        mutex.ReleaseMutex();
                    }
                }
                else
                {
                    ConnectMainForm();
                }
            }
        }

        private static void ConnectMainForm()
        {
            using (var clientPipe = new NamedPipeClientStream(".", $"{typeof(Program)}"))
            {
                clientPipe.Connect();
                clientPipe.Close();
            }
        }

        private static void WaitForConnection()
        {
            using (var serverPipe = new NamedPipeServerStream($"{typeof(Program)}"))
            {
                using (var reader = new StreamReader(serverPipe))
                {
                    while (true)
                    {
                        serverPipe.WaitForConnection();
                        try
                        {
                            form.EnableRandomMoves();
                        }
                        finally
                        {
                            serverPipe.Disconnect();
                        }
                    }
                }
            }
        }

        private static void RunInstance()
        {
            // To customize application configuration such as set high DPI settings or default font,
            // see https://aka.ms/applicationconfiguration.
            ApplicationConfiguration.Initialize();
            form = new Form1();
            Application.Run(form);
        }
    }
}
