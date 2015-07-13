using System;
using System.Collections.Generic;
using System.IO.Pipes;
using System.Linq;
using System.Security.Principal;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace TestSync
{
    class PipeServer
    {
        public const int MaxPipes = 254;

        public event Action<string> ReceiveMessage;

        private volatile bool       _running;
        private Thread              _runningThread;
        private EventWaitHandle     _terminateHandle = new EventWaitHandle(false, EventResetMode.AutoReset);

        private readonly Guid _serverGuid;
        public string PipeName { get; private set; }

        public PipeServer(Guid serverGuid, string pipeName)
        {
            _serverGuid = serverGuid;
            PipeName = pipeName;
        }

        void ServerLoop()
        {
            while (_running)
            {
                ProcessNextClient();
            }
            _terminateHandle.Set();
        }

        public void Run()
        {
            _running = true;
            _runningThread = new Thread(ServerLoop);
            _runningThread.Start();
        }

        public void Stop()
        {
            _running = false;

            // fake connect to realese WaitForConnection
            using (NamedPipeClientStream npcs = new NamedPipeClientStream(".", PipeName, PipeDirection.Out, PipeOptions.None,
                    TokenImpersonationLevel.Impersonation))
            {
                npcs.Connect();
            }

            _terminateHandle.WaitOne();
            _runningThread.Join();
        }

        public virtual string ProcessRequest(string message)
        {
            return "";
        }

        public void ProcessClientThread(object o)
        {
            NamedPipeServerStream pipeStream = (NamedPipeServerStream)o;

            //read data from pipe 
            var ss = new StreamString(pipeStream);

            string handshake = ss.ReadString();
            if (!string.IsNullOrEmpty(handshake))
            {
                var guid = new Guid(handshake);

                string xmlReceive = ss.ReadString();
                if (!string.IsNullOrEmpty(handshake) && _serverGuid != guid)
                    InvokeReceiveMessage(xmlReceive);

            }

            pipeStream.Close();
            pipeStream.Dispose();
        }

        public void ProcessNextClient()
        {
            try
            {
                NamedPipeServerStream pipeStream = new NamedPipeServerStream(PipeName, PipeDirection.In, 254);
                pipeStream.WaitForConnection();

                //Spawn a new thread for each request and continue waiting
                Thread t = new Thread(ProcessClientThread);
                t.Start(pipeStream);
            }
            catch (Exception e)
            {
                //If there are no more avail connections (254 is in use already) then just keep looping until one is avail
            }
        }

        public void InvokeReceiveMessage(string message)
        {
            if (string.IsNullOrEmpty(message)) return;
                    
            if (ReceiveMessage != null)
                ReceiveMessage(message);
        }
    }
}
