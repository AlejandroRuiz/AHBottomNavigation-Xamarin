using System;
using Android.Content;
using Android.Content.Res;
using Android.Graphics;
using Android.Graphics.Drawables;
using Android.Support.Graphics.Drawable;
using Android.Support.V4.Content;

namespace AHBottomNavigation.AHBottomNavigation
{
	public class AHBottomNavigationItem
	{
		private String title = "";
		private Drawable drawable;
		private int color = Color.Gray.ToArgb();

		private int titleRes = 0;
		private int drawableRes = 0;
		private int colorRes = 0;

		/**
		 * Constructor
		 *
		 * @param title    Title
		 * @param resource Drawable resource
		 */
		public AHBottomNavigationItem(String title, int resource)
		{
			this.title = title;
			this.drawableRes = resource;
		}

		/**
		 * @param title    Title
		 * @param resource Drawable resource
		 * @param color    Background color
		 */
		[Obsolete]
		public AHBottomNavigationItem(String title, int resource, int color)
		{
			this.title = title;
			this.drawableRes = resource;
			this.color = color;
		}

		/**
		 * Constructor
		 *
		 * @param titleRes    String resource
		 * @param drawableRes Drawable resource
		 * @param colorRes    Color resource
		 */
		public AHBottomNavigationItem(int titleRes, int drawableRes, int colorRes)
		{
			this.titleRes = titleRes;
			this.drawableRes = drawableRes;
			this.colorRes = colorRes;
		}

		/**
		 * Constructor
		 *
		 * @param title    String
		 * @param drawable Drawable
		 */
		public AHBottomNavigationItem(String title, Drawable drawable)
		{
			this.title = title;
			this.drawable = drawable;
		}

		/**
		 * Constructor
		 *
		 * @param title    String
		 * @param drawable Drawable
		 * @param color    Color
		 */
		public AHBottomNavigationItem(String title, Drawable drawable, int color)
		{
			this.title = title;
			this.drawable = drawable;
			this.color = color;
		}

		public String getTitle(Context context)
		{
			if (titleRes != 0)
			{
				return context.GetString(titleRes);
			}
			return title;
		}

		public void setTitle(String title)
		{
			this.title = title;
			this.titleRes = 0;
		}

		public void setTitle(int titleRes)
		{
			this.titleRes = titleRes;
			this.title = "";
		}

		public int getColor(Context context)
		{
			if (colorRes != 0)
			{
				return ContextCompat.GetColor(context, colorRes);
			}
			return color;
		}

		public void setColor(int color)
		{
			this.color = color;
			this.colorRes = 0;
		}

		public void setColorRes(int colorRes)
		{
			this.colorRes = colorRes;
			this.color = 0;
		}

		public Drawable getDrawable(Context context)
		{
			if (drawableRes != 0)
			{
				try
				{
					return VectorDrawableCompat.Create(context.Resources, drawableRes, null);
				}
				catch (Resources.NotFoundException e)
				{
					return ContextCompat.GetDrawable(context, drawableRes);
				}
			}
			return drawable;
		}

		public void setDrawable(int drawableRes)
		{
			this.drawableRes = drawableRes;
			this.drawable = null;
		}

		public void setDrawable(Drawable drawable)
		{
			this.drawable = drawable;
			this.drawableRes = 0;
		}
	}
}

