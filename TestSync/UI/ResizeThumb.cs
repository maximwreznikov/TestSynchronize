using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;

namespace TestSync.UI
{
    /// <summary>
    /// Class for resize object
    /// </summary>
    public class ResizeThumb : Thumb
    {
        public ResizeThumb()
        {
            DragDelta += new DragDeltaEventHandler(this.ResizeThumb_DragDelta);
        }

        private void ResizeThumb_DragDelta(object sender, DragDeltaEventArgs e)
        {
            SyncRectangle designerItem = this.DataContext as SyncRectangle;

            if (designerItem != null)
            {
                int deltaVertical, deltaHorizontal;

                switch (VerticalAlignment)
                {
                    case VerticalAlignment.Bottom:
                        deltaVertical = (int)Math.Min(-e.VerticalChange, designerItem.Height );
                        designerItem.Height -= deltaVertical;
                        break;
                    case VerticalAlignment.Top:
                        deltaVertical = (int)Math.Min(e.VerticalChange, designerItem.Height);
                        designerItem.Y = designerItem.Y + deltaVertical;
                        designerItem.Height -= deltaVertical;
                        break;
                    default:
                        break;
                }

                switch (HorizontalAlignment)
                {
                    case HorizontalAlignment.Left:
                        deltaHorizontal = (int)Math.Min(e.HorizontalChange, designerItem.Width);
                        designerItem.X = designerItem.X + deltaHorizontal;
                        designerItem.Width -= deltaHorizontal;
                        break;
                    case HorizontalAlignment.Right:
                        deltaHorizontal = (int)Math.Min(-e.HorizontalChange, designerItem.Width);
                        designerItem.Width -= deltaHorizontal;
                        break;
                    default:
                        break;
                }
            }

            e.Handled = true;
        }
    }
}
