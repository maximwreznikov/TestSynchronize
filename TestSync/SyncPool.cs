using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.IO.Pipes;
using System.Linq;
using System.Security.Principal;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace TestSync
{
    public class StreamString
    {
        private Stream ioStream;
        private UnicodeEncoding streamEncoding;

        public StreamString(Stream ioStream)
        {
            this.ioStream = ioStream;
            streamEncoding = new UnicodeEncoding();
        }

        public string ReadString()
        {
            byte[] strSizeArr = new byte[sizeof(int)];
            ioStream.Read(strSizeArr, 0, sizeof(int));
            int strSize = BitConverter.ToInt32(strSizeArr, 0);
            byte[] inBuffer = new byte[strSize];
            ioStream.Read(inBuffer, 0, strSize);
            return streamEncoding.GetString(inBuffer);
        }

        public int WriteString(string outString)
        {
            byte[] outBuffer = streamEncoding.GetBytes(outString);
            byte[] strSize = BitConverter.GetBytes(outBuffer.Length);
            ioStream.Write(strSize, 0, strSize.Length);
            ioStream.Write(outBuffer, 0, outBuffer.Length);
            ioStream.Flush();
            return outBuffer.Length + 2;
        }
    }

    /// <summary>
    /// Pool for synchronize object between processes
    /// 
    /// </summary>
    class SyncPool
    {
        public static readonly string PipeName = "PipesEnroll";

        private readonly List<SynchronizableObject> _syncObjects = new List<SynchronizableObject>();
        private ObservableCollection<SynchronizableObject> _objectCollection = null;

        #region Singleton
        private static readonly Lazy<SyncPool> _instance = new Lazy<SyncPool>(() => new SyncPool());
        public static SyncPool Instance { get { return _instance.Value; } }
        private SyncPool()
        { }

        ~SyncPool()
        {
            Destroy();
        }
        #endregion

        #region Pipes iteraction

        private PipeServer _pipeServer;
        private PipeClient _pipeClient;
        public void Startup()
        {
            var appGuid = Guid.NewGuid();
            _pipeServer = new PipeServer(appGuid, PipeName);
            _pipeServer.ReceiveMessage += OnReceiveMessage;
            _pipeServer.Run();

            _pipeClient = new PipeClient(appGuid, PipeName);
            _pipeClient.Run();
        }

        private void OnReceiveMessage(string message)
        {
            _syncObjects[0].Synchronize(message);
        }

        public void Destroy()
        {
            _pipeServer.Stop();
            _pipeClient.Stop();
        }
        
        #endregion

        public void AttachCollection(ObservableCollection<SynchronizableObject> objectCollection)
        {
            _objectCollection = objectCollection;

            _objectCollection.CollectionChanged += OnObservableCollectionChanged;
        }

        private void OnObservableCollectionChanged(object sender, NotifyCollectionChangedEventArgs args)
        {
            switch (args.Action)
            {
                case NotifyCollectionChangedAction.Add:
                    foreach (var p in args.NewItems) AttachObject(p as SynchronizableObject);
                    break;
                case NotifyCollectionChangedAction.Remove:
                    foreach (var p in args.OldItems) DetachObject(p as SynchronizableObject);
                    break;
                default:
                    throw new Exception("Collection сhanged фction is not supported!");
                    break;
            }
        }

        public void AttachObject(SynchronizableObject obj)
        {
            if (obj == null) return;
            obj.SynchronizeProperty += OnObjectSynchronizeProperty;
            _syncObjects.Add(obj);
        }

        public void DetachObject(SynchronizableObject obj)
        {
            if (obj == null) return;
            if(_syncObjects.Remove(obj))
                obj.SynchronizeProperty -= OnObjectSynchronizeProperty;
            
        }

        private void OnObjectSynchronizeProperty(object sender, PropertyChangedEventArgs propertyChangedEventArgs)
        {
            var obj = sender as SynchronizableObject;
            var objectData = obj.Serialize();
            _pipeClient.ProcessMessage(objectData);
        }
    }
}
