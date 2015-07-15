using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.IO.MemoryMappedFiles;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;

namespace TestSync
{
    /// <summary>
    /// Memory mapped file conroller
    /// </summary>
    class MMFileAdapter
    {
        public readonly static int MaxSize = 16392;

        private readonly string _mappedFileName;
        private readonly string _mutexName;

        private readonly Guid   _appGuid;
        private Mutex _nameMutex;
        private MemoryMappedFile _fileHandle;

        #region Initialize/Dispose
        public MMFileAdapter(Guid appGuid, string mappedFileName)
        {
            _appGuid = appGuid;
            _mappedFileName = mappedFileName;
            _mutexName = mappedFileName + "Mutex";
        }

        public void Open()
        {
            bool mutexCreated;
            _nameMutex = new Mutex(false, _mutexName, out mutexCreated);
            _fileHandle = MemoryMappedFile.CreateOrOpen(_mappedFileName, MaxSize, MemoryMappedFileAccess.ReadWrite);
        }

        public void Close()
        {
            _nameMutex.Close();
        }
        #endregion

        #region Collection operation

        public void SaveCollection(ObservableCollection<SynchronizableObject> collection)
        {
            //create file
            using (MemoryMappedViewAccessor accessor = _fileHandle.CreateViewAccessor(0, 0, MemoryMappedFileAccess.ReadWrite))
            {
                _nameMutex.WaitOne();

                int current = 0;

                try
                {
                    WriteString(accessor, ref current, _appGuid.ToString());

                    accessor.Write(current, (Int32) collection.Count);
                    current += sizeof (Int32);
                    for (int i = 0; i < collection.Count; i++)
                    {
                        // save collection element
                        string data = collection[i].Serialize();

                        WriteString(accessor, ref current, data);
                    }
                }
                catch (Exception ex)
                {
                    Debug.WriteLine(ex.Message);
                    MessageBox.Show("Too many objects!");
                    NotificationManager.CloseAllInstances();
                }
                finally
                {
                    _nameMutex.ReleaseMutex();
                }


                Debug.WriteLine("Saved data");
            }
        }



        public void SyncCollection(ObservableCollection<SynchronizableObject> collection)
        {
            //read file
            using (
                MemoryMappedViewAccessor accessor = _fileHandle.CreateViewAccessor(0, 0, MemoryMappedFileAccess.Read))
            {
                //read
                _nameMutex.WaitOne();

                int current = 0;

                var guidStr = ReadString(accessor, ref current);
                if (!string.IsNullOrEmpty(guidStr))
                {
                    var guid = new Guid(guidStr);
                    if (_appGuid != guid)
                    {
                        Int32 count = accessor.ReadInt32(current);
                        current += sizeof (Int32);

                        string[] serializedCollection = new string[count];
                        for (int i = 0; i < count; i++)
                        {
                            serializedCollection[i] = ReadString(accessor, ref current);
                        }
                        SyncCollection(serializedCollection, collection);
                    }
                }

                _nameMutex.ReleaseMutex();
            }
        }
        #endregion

        # region String operations
        private void WriteString(MemoryMappedViewAccessor accessor, ref int current, string data)
        {
            byte[] buffer = ConvertStringToByteArray(data);

            accessor.Write(current, (Int32)buffer.Length);
            current += sizeof(Int32);

            accessor.WriteArray<byte>(current, buffer, 0, buffer.Length);
            current += sizeof(byte) * buffer.Length;
        }

        private string ReadString(MemoryMappedViewAccessor accessor, ref int current)
        {
            Int32 size = accessor.ReadInt32(current);
            current += sizeof(Int32);

            byte[] buffer = new byte[size];
            accessor.ReadArray<byte>(current, buffer, 0, size);
            current += sizeof(byte) * size;

            string xmlData = ConvertByteArrayToString(buffer);
            return xmlData;
        }
        #endregion

        private void SyncCollection(string[] serializedCollection, ObservableCollection<SynchronizableObject> collection)
        {
            int i = 0;
            for (i = 0; i < collection.Count && i < serializedCollection.Length; i++)
            {
                collection[i].Synchronize(serializedCollection[i]);
            }

            // additional object
            while (i < serializedCollection.Length)
            {
                var xmlData = serializedCollection[i];
                Application.Current.Dispatcher.Invoke(() =>
                {
                    collection.Add(ObjectFactory.Instance.CreateRectangleFromXml(xmlData));
                });
                i++;
            }

            // remove last
            while (i < collection.Count)
            {
                Application.Current.Dispatcher.Invoke(() =>
                {
                    collection.RemoveAt(collection.Count - 1);
                });
                i++;
            }
        }

        # region String convetation
        private static byte[] ConvertStringToByteArray(string value)
        {
            System.Text.ASCIIEncoding encoding =
                new System.Text.ASCIIEncoding();
            return encoding.GetBytes(value);
        }

        public static string ConvertByteArrayToString(byte[] values)
        {
            System.Text.ASCIIEncoding encoding =
                new System.Text.ASCIIEncoding();
            return encoding.GetString(values);
        }
        #endregion
    }
}
