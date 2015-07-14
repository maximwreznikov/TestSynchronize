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
using System.Threading.Tasks;
using System.Timers;

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
        public static readonly string MappedFileName = "TestSyncMapedFileName";

        private readonly List<SynchronizableObject>         _syncObjects = new List<SynchronizableObject>();
        private ObservableCollection<SynchronizableObject>  _objectCollection = null;

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

        #region Memory mapped file iteraction

        private MMFileAdapter       _mmFile;
        private bool                _needSave;
        private Timer               _saveTimer;
        private NotificationManager _syncManager;

        public void Startup(NotificationManager syncManager)
        {
            _syncManager = syncManager;
            _syncManager.SyncState += OnSyncState;
            
            var appGuid = Guid.NewGuid();
            _mmFile = new MMFileAdapter(appGuid, MappedFileName);
            _mmFile.Open();

            _needSave = false;
            // Create a timer for save collection state
            _saveTimer = new Timer();
            _saveTimer.Elapsed += SaveObjects;
            _saveTimer.Interval = 50;
            _saveTimer.Enabled = true;
            _saveTimer.Start();
        }

        private void OnSyncState()
        {
            SyncObject();
        }

        private void SaveObjects(object source, ElapsedEventArgs e)
        {
            SaveObject();
        }

        private void SaveObject()
        {
            if (_objectCollection == null) return;
            if (!_needSave) return;

            _saveTimer.Stop();
            _needSave = false;
            _mmFile.SaveCollection(_objectCollection);
            _syncManager.UpdateAll();
            _saveTimer.Start();
        }

        private void SyncObject()
        {
            if (_objectCollection == null) return;

            _mmFile.SyncCollection(_objectCollection);
        }

        public void Destroy()
        {
            _mmFile.Close();
            _syncManager.SyncState -= OnSyncState;
            _saveTimer.Elapsed -= SaveObjects;
            _saveTimer.Enabled = false;
        }
        
        #endregion

        public void AttachCollection(ObservableCollection<SynchronizableObject> objectCollection)
        {
            _objectCollection = objectCollection;
            _objectCollection.CollectionChanged += OnObservableCollectionChanged;

            // sync with existing collection
            _mmFile.SyncCollection(_objectCollection);
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
            _needSave = true;
        }

        public void DetachObject(SynchronizableObject obj)
        {
            if (obj == null) return;
            if (_syncObjects.Remove(obj))
            {
                obj.SynchronizeProperty -= OnObjectSynchronizeProperty;
                _needSave = true;
            }
        }

        private void OnObjectSynchronizeProperty(object sender, PropertyChangedEventArgs propertyChangedEventArgs)
        {
            _needSave = true;
        }
    }
}
