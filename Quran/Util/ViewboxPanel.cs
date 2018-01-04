using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace Quran.Util
{
    public class ViewboxPanel : Panel
    {
        private double scale;

        protected override Size MeasureOverride(Size availableSize)
        {
            double width = 0;
            Size unlimitedSize = new Size(double.PositiveInfinity, double.PositiveInfinity);
            foreach (UIElement child in Children)
            {
                child.Measure(unlimitedSize);
                width += child.DesiredSize.Width;
            }
            scale = availableSize.Width / width;

            return availableSize;
        }

        protected override Size ArrangeOverride(Size finalSize)
        {
            Transform scaleTransform = new ScaleTransform(scale, scale);
            double width = 0;
            foreach (UIElement child in Children)
            {
                child.RenderTransform = scaleTransform;
                child.Arrange(new Rect(new Point(scale * width, 0), new Size(child.DesiredSize.Width, finalSize.Height / scale)));
                width += child.DesiredSize.Height;
            }

            return finalSize;
        }
    }
}
