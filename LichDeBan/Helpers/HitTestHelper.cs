using System.Windows;
using System.Windows.Media;

namespace LichDeBan.Helpers
{
    public static class HitTestHelper
    {
        public static object? FindDataContextCore(Visual parent, System.Windows.Point pt)
        {
            object? result = null;
            VisualTreeHelper.HitTest(parent, null, new HitTestResultCallback((HitTestResult hitResult) =>
            {
                DependencyObject current = hitResult.VisualHit;
                while (current != null)
                {
                    if (current is FrameworkElement fe && fe.DataContext is Models.DayCellModel cellModel)
                    {
                        result = cellModel;
                        return HitTestResultBehavior.Stop;
                    }
                    current = VisualTreeHelper.GetParent(current);
                }
                return HitTestResultBehavior.Continue;
            }), new PointHitTestParameters(pt));

            return result;
        }
    }
}
