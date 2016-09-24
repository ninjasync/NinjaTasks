using System;
using System.ComponentModel;
using System.Windows;

namespace CustomChromeLibrary
{
	public class CustomChromeWindow: Window, INotifyPropertyChanged
	{

		protected override void OnStateChanged(EventArgs e)
		{
			base.OnStateChanged(e);
            OnPropertyChanged("WindowMargin");
		}

		public Thickness WindowMargin
		{
			get
			{
			    if (WindowState == WindowState.Maximized)
			        return SystemParameters.WindowResizeBorderThickness;
				else
					return new Thickness(0, 0, 0, 0);
			}
		}

		#region INotifyPropertyChanged
		private void OnPropertyChanged(String info)
		{
			if (PropertyChanged != null)
			{
				PropertyChanged(this, new PropertyChangedEventArgs(info));
			}
		}

		public event PropertyChangedEventHandler PropertyChanged;
		#endregion
	}
}
