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
    class MMFileController
    {
        public readonly static string MutexName = "TestSyncMapedFileMutex";
        public readonly static int MaxSize = 4096;

        private readonly string _mappedFileName;
        private Mutex _nameMutex;
        private MemoryMappedFile _fileHandle;

        public MMFileController(string mappedFileName)
        {
            _mappedFileName = mappedFileName;
        }

        public void Open()
        {
            bool mutexCreated;
            _nameMutex = new Mutex(false, MutexName, out mutexCreated);
            _fileHandle = MemoryMappedFile.CreateOrOpen(_mappedFileName, MaxSize, MemoryMappedFileAccess.ReadWrite);
        }

        public void Close()
        {
            _nameMutex.Close();
        }

        public void SaveCollection(ObservableCollection<SynchronizableObject> collection)
        {
            //create file
            using (MemoryMappedViewAccessor accessor = _fileHandle.CreateViewAccessor(0, 0, MemoryMappedFileAccess.ReadWrite))
            {
                _nameMutex.WaitOne();

                int current = 0;
                accessor.Write(current, (Int32)collection.Count);
                current += sizeof(Int32);
                for (int i = 0; i < collection.Count; i++)
                {
                    // save collection element
                    string data = collection[i].Serialize();
                    byte[] buffer = ConvertStringToByteArray(data);

                    accessor.Write(current, (Int32)buffer.Length);
                    current += sizeof(Int32);

                    accessor.WriteArray<byte>(current, buffer, 0, buffer.Length);
                    current += sizeof(byte) * buffer.Length;
                }

                _nameMutex.ReleaseMutex();

                Debug.WriteLine("Saved data");
            }
        }

        public void SyncCollection(ObservableCollection<SynchronizableObject> collection)
        {
            //read file
            using (
                MemoryMappedViewAccessor accessor = _fileHandle.CreateViewAccessor(0, 0, MemoryMappedFileAccess.Read))
            {
                var currentNum = collection.Count;

                //read
                _nameMutex.WaitOne();

                int current = 0;
                Int32 count = accessor.ReadInt32(0);
                current += sizeof(Int32);

                for (int i = 0; i < count; i++)
                {
                    Int32 size = accessor.ReadInt32(current);
                    current += sizeof(Int32);

                    byte[] buffer = new byte[size];
                    accessor.ReadArray<byte>(current, buffer, 0, size);
                    current += sizeof(byte) * size;

                    string xmlData = ConvertByteArrayToString(buffer);
                    try
                    {
                        if (!string.IsNullOrEmpty(xmlData))
                        {
                            if (i < currentNum)
                            {
                                collection[i].Synchronize(xmlData);
                            }
                            else
                            {
                                Application.Current.Dispatcher.Invoke(() =>
                                {
                                    collection.Add(ObjectFactory.Instance.CreateRectangleFromXml(xmlData));
                                });
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        Debug.WriteLine("Cant parse data {0}", xmlData);
                    }
                }

                _nameMutex.ReleaseMutex();
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
