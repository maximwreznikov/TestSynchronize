using System;
using System.IO.Pipes;
using System.Threading;
using System.Windows.Media;

namespace TestSync
{

    /// <summary>
    /// Factory for objects
    /// </summary>
    internal class ObjectFactory
    {
        private readonly Color [] ColorPalette =
        {
            Colors.Red, Colors.Green, Colors.Blue, Colors.Yellow, Colors.Cyan, Colors.BlueViolet, Colors.Chocolate
        };

        #region Singleton
        private static readonly Lazy<ObjectFactory> _instance = new Lazy<ObjectFactory>(() => new ObjectFactory());
        public static ObjectFactory Instance { get { return _instance.Value; } }
        private ObjectFactory()
        { }
        #endregion
        
        public SyncRectangle CreateRandomRectangle()
        {
            Random r = new Random();
            var width = r.Next(10, 100);
            var height = r.Next(10, 100);

            var x = r.Next(10, 300);
            var y = r.Next(5, 150);

            int index = r.Next(0, ColorPalette.Length - 1);

            var newRect = new SyncRectangle(x, y, width, height, ColorPalette[index]);
            return newRect;
        }

        public SyncRectangle CreateRectangleFromXml(string xml)
        {
            var newRect = new SyncRectangle();
            newRect.Synchronize(xml);
            return newRect;
        }
    }
}
