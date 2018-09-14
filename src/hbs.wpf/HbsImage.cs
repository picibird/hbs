using System.Windows.Media;

namespace System.Windows.Controls
{
    public class HbsImage : Image
    {
        protected override void OnRender(DrawingContext dc)
        {
            //this.VisualBitmapScalingMode = BitmapScalingMode.NearestNeighbor;
            base.OnRender(dc);
        }
    }
}
