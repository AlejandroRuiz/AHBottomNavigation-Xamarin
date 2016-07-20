using System;
using Android.Content;
using Android.Support.V4.View;
using Android.Util;

namespace AHBottomNavigation.AHBottomNavigation
{
	public class AHBottomNavigationViewPager : ViewPager
	{
		private bool enabled;

		public AHBottomNavigationViewPager(Context context, IAttributeSet attrs) : base(context, attrs)
		{
			this.enabled = false;
		}

		public override bool OnTouchEvent(Android.Views.MotionEvent e)
		{
			if (this.enabled)
			{
				return base.OnTouchEvent(e);
			}
			return false;
		}

		public override bool OnInterceptTouchEvent(Android.Views.MotionEvent ev)
		{
			if (this.enabled)
			{
				return base.OnInterceptTouchEvent(ev);
			}
			return false;
		}

		/**
	 	* Enable or disable the swipe navigation
	 	* @param enabled
	 	*/
		public void setPagingEnabled(bool enabled)
		{
			this.enabled = enabled;
		}
	}
}

