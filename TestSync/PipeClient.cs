using System;
using System.Collections.Generic;
using System.IO.Pipes;
using System.Linq;
using System.Security.Principal;
using System.Text;
using System.Threading.Tasks;

namespace TestSync
{
    class PipeClient
    {
        public string PipeName { get; private set; }

        private readonly Guid               _clientGuid;
        private NamedPipeClientStream       _clientPipe = null;

        public PipeClient(Guid clientGuid, string pipeName)
        {
            _clientGuid = clientGuid;
            PipeName = pipeName;
        }

        public void Run()
        {
//            _clientPipe = new NamedPipeClientStream(".", PipeName, PipeDirection.Out, PipeOptions.None,
//                TokenImpersonationLevel.Impersonation);
        }

        public void Stop()
        {
        }

        public void ProcessMessage(string message)
        {
//            _clientPipe.Connect();
//
//            var ss = new StreamString(_clientPipe);
//            ss.WriteString(_clientGuid.ToString());
//            ss.WriteString(message);
//
//            _clientPipe.Close();

            using (var clientPipe = new NamedPipeClientStream(".", PipeName, PipeDirection.Out, PipeOptions.None, TokenImpersonationLevel.Impersonation))
            {
                clientPipe.Connect();
                var ss = new StreamString(clientPipe);
                ss.WriteString(_clientGuid.ToString());
                ss.WriteString(message);
                clientPipe.Close();
            }
        }
    }
}
