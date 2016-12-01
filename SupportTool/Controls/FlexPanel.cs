using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace SupportTool.Controls
{
	public class FlexPanel : Panel
	{
		public static bool GetFlex(DependencyObject obj) => (bool)obj.GetValue(FlexProperty);
		public static void SetFlex(DependencyObject obj, bool value) => obj.SetValue(FlexProperty, value);
		public static readonly DependencyProperty FlexProperty = DependencyProperty.RegisterAttached("Flex", typeof(bool), typeof(FlexPanel), new PropertyMetadata(false));

		public static int GetFlexWeight(DependencyObject obj) => (int)obj.GetValue(FlexWeightProperty);
		public static void SetFlexWeight(DependencyObject obj, int value) => obj.SetValue(FlexWeightProperty, value);
		public static readonly DependencyProperty FlexWeightProperty = DependencyProperty.RegisterAttached("FlexWeight", typeof(int), typeof(FlexPanel), new PropertyMetadata(0));

		public Orientation Orientation
		{
			get => (Orientation)GetValue(OrientationProperty);
			set => SetValue(OrientationProperty, value);
		}
		public static readonly DependencyProperty OrientationProperty = DependencyProperty.Register(nameof(Orientation), typeof(Orientation), typeof(FlexPanel), new PropertyMetadata(Orientation.Vertical));
	}
}
