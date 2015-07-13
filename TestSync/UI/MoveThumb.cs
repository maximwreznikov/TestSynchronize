using System.Windows.Controls;
using System.Windows.Controls.Primitives;

namespace TestSync.UI
{
    /// <summary>
    /// Class for move object
    /// </summary>
    public class MoveThumb : Thumb
    {
        public MoveThumb()
        {
            DragDelta += new DragDeltaEventHandler(this.MoveThumb_DragDelta);
        }

        private void MoveThumb_DragDelta(object sender, DragDeltaEventArgs e)
        {
            SyncRectangle designerItem = this.DataContext as SyncRectangle;

            if (designerItem != null)
            {
                designerItem.X += (int)e.HorizontalChange;
                designerItem.Y += (int)e.VerticalChange;
            }
        }
    }
}
