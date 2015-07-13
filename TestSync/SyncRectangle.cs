using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;
using System.Xml.Serialization;

namespace TestSync
{
    [Serializable]
    public class SyncRectangle : SynchronizableObject
    {
        private int _x;
        private int _y;

        private int _width;
        private int _height;

        private Color _color;

        public SyncRectangle()
        {}

        public SyncRectangle(int x_, int y_, int width_, int height_, Color color_)
        {
            _x = x_;
            _y = y_;

            _width = width_;
            _height = height_;
            _color = color_;
        }

        # region Setters/Getters
        public int Width
        {
            get
            {
                return _width; 
            }
            set
            {
                if (_width == value) return;
                _width = value;
                InvokePropertyChanged("Width");
                InvokeSynchronizeProperty("Width");
            }
        }

        public int Height
        {
            get
            {
                return _height;
            }
            set
            {
                if (_height == value) return;
                _height = value;
                InvokePropertyChanged("Height");
                InvokeSynchronizeProperty("Height");
            }
        }

        public int X {
            get
            {
                return _x; 
            }
            set
            {
                if (_x == value) return;
                _x = value;
                InvokePropertyChanged("X");
                InvokeSynchronizeProperty("X");
            } 
        }
        public int Y
        {
            get
            {
                return _y;
            }
            set
            {
                if (_y == value) return;
                _y = value;
                InvokePropertyChanged("Y");
                InvokeSynchronizeProperty("Y");
            }
        }

        public Color Color
        {
            set
            {
                if (_color == value) return;
                _color = value;
                InvokePropertyChanged("Color");
                InvokeSynchronizeProperty("Color");
            }
            get
            {
                return _color;
            }
        }
        #endregion

        # region Implementation SynchronizableObject
        public override string Serialize()
        {
            //Add an empty namespace and empty value
            XmlSerializerNamespaces ns = new XmlSerializerNamespaces();
            ns.Add("", "");
            XmlSerializer ser = new XmlSerializer(typeof(SyncRectangle));
            using (StringWriter textWriter = new StringWriter())
            {
                ser.Serialize(textWriter, this, ns);
                return textWriter.ToString();
            }
        }

        public override void Synchronize(string xml)
        {
            XmlSerializer ser = new XmlSerializer(typeof(SyncRectangle));
            using (var reader = new StringReader(xml))
            {
                var sourceData = (SyncRectangle)ser.Deserialize(reader);
                SetX(sourceData.X);
                SetY(sourceData.Y);
                SetWidth(sourceData.Width);
                SetHeight(sourceData.Height);
                SetColor(sourceData.Color);
            }
        }
        #endregion

        # region Setters for syncronization
        private void SetX(int value)
        {
            if (_x == value) return;
            _x = value;
            InvokePropertyChanged("X");
        }

        private void SetY(int value)
        {
            if (_y == value) return;
            _y = value;
            InvokePropertyChanged("Y");
        }

        private void SetWidth(int value)
        {
            if (_width == value) return;
            _width = value;
            InvokePropertyChanged("Width");
        }

        private void SetHeight(int value)
        {
            if (_height == value) return;
            _height = value;
            InvokePropertyChanged("Height");
        }

        private void SetColor(Color value)
        {
            if (_color == value) return;
            _color = value;
            InvokePropertyChanged("Color");
        }
        #endregion
    }
}
