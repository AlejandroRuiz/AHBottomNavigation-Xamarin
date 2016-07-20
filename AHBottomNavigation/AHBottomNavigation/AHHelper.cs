using System;
using Android.Animation;
using Android.App;
using Android.Content;
using Android.Graphics;
using Android.Graphics.Drawables;
using Android.OS;
using Android.Support.V4.Graphics.Drawable;
using Android.Util;
using Android.Views;
using Android.Widget;

namespace AHBottomNavigation.AHBottomNavigation
{

	public class AHHelper
	{
		/**
	 	* Return a tint drawable
	 	*
	 	* @param drawable
	 	* @param color
	 	* @param forceTint
	 	* @return
	 	*/
		public static Drawable getTintDrawable(Drawable drawable, int color, bool forceTint)
		{
			if (forceTint)
			{
				drawable.ClearColorFilter();
				drawable.SetColorFilter(new Color(color), PorterDuff.Mode.SrcIn);
				drawable.InvalidateSelf();
				return drawable;
			}
			Drawable wrapDrawable = DrawableCompat.Wrap(drawable).Mutate();
			DrawableCompat.SetTint(wrapDrawable, color);
			return wrapDrawable;
		}

		/**
		* Update top margin with animation
	 	*/
		public static void updateTopMargin(View view, int fromMargin, int toMargin)
		{
			ValueAnimator animator = ValueAnimator.OfFloat(fromMargin, toMargin);
			animator.SetDuration(150);
			animator.AddUpdateListener(
				new CustomViewPropertyIAnimatorUpdateListener((valueAnimator) =>
				{
					float animatedValue = (float)valueAnimator.AnimatedValue;
					if (view.LayoutParameters is ViewGroup.MarginLayoutParams)
					{
						ViewGroup.MarginLayoutParams p = (ViewGroup.MarginLayoutParams)view.LayoutParameters;
						p.SetMargins(p.LeftMargin, (int)animatedValue, p.RightMargin, p.BottomMargin);
						view.RequestLayout();
					}
				})
			);
			animator.Start();
		}

		/**
	 	* Update bottom margin with animation
	 	*/
		public static void updateBottomMargin(View view, int fromMargin, int toMargin, int duration)
		{
			ValueAnimator animator = ValueAnimator.OfFloat(fromMargin, toMargin);
			animator.SetDuration(duration);
			animator.AddUpdateListener(
				new CustomViewPropertyIAnimatorUpdateListener(
					(valueAnimator) =>
					{
						float animatedValue = (float)valueAnimator.AnimatedValue;
						if (view.LayoutParameters is ViewGroup.MarginLayoutParams)
						{
							ViewGroup.MarginLayoutParams p = (ViewGroup.MarginLayoutParams)view.LayoutParameters;
							p.SetMargins(p.LeftMargin, p.TopMargin, p.RightMargin, (int)animatedValue);
							view.RequestLayout();
						}
					}
				)
			);
			animator.Start();
		}

		/**
	 	* Update left margin with animation
	 	*/
		public static void updateLeftMargin(View view, int fromMargin, int toMargin)
		{
			ValueAnimator animator = ValueAnimator.OfFloat(fromMargin, toMargin);
			animator.SetDuration(150);
			animator.AddUpdateListener(
				new CustomViewPropertyIAnimatorUpdateListener(
					(valueAnimator) =>
					{
						float animatedValue = (float)valueAnimator.AnimatedValue;
						if (view.LayoutParameters is ViewGroup.MarginLayoutParams)
						{
							ViewGroup.MarginLayoutParams p = (ViewGroup.MarginLayoutParams)view.LayoutParameters;
							p.SetMargins((int)animatedValue, p.TopMargin, p.RightMargin, p.BottomMargin);
							view.RequestLayout();
						}
					}
				)
			);
			animator.Start();
		}

		/**
	 	* Update text size with animation
	 	*/
		public static void updateTextSize(TextView textView, float fromSize, float toSize)
		{
			ValueAnimator animator = ValueAnimator.OfFloat(fromSize, toSize);
			animator.SetDuration(150);
			animator.AddUpdateListener(
				new CustomViewPropertyIAnimatorUpdateListener(
					(valueAnimator) =>
					{
						float animatedValue = (float)valueAnimator.AnimatedValue;
						textView.SetTextSize(ComplexUnitType.Px, animatedValue);
					}
				)
			);
			animator.Start();
		}

		/**
		 * Update alpha
		 */
		public static void updateAlpha(View view, float fromValue, float toValue)
		{
			ValueAnimator animator = ValueAnimator.OfFloat(fromValue, toValue);
			animator.SetDuration(150);
			animator.AddUpdateListener(
				new CustomViewPropertyIAnimatorUpdateListener(
					(valueAnimator) =>
					{
						float animatedValue = (float)valueAnimator.AnimatedValue;
						view.Alpha = (animatedValue);
					}
				)
			);
			animator.Start();
		}

		/**
	 	* Update text color with animation
	 	*/
		public static void updateTextColor(TextView textView, int fromColor, int toColor)
		{
			ValueAnimator colorAnimation = ValueAnimator.OfObject(new ArgbEvaluator(), fromColor, toColor);
			colorAnimation.SetDuration(150);
			colorAnimation.AddUpdateListener(
				new CustomViewPropertyIAnimatorUpdateListener(
					(animator) =>
					{
						textView.SetTextColor(new Color((int)animator.AnimatedValue));
					}
			)
			);
			colorAnimation.Start();
		}


		/**
	 	* Update text color with animation
	 	*/
		public static void updateViewBackgroundColor(View view, int fromColor, int toColor)
		{
			ValueAnimator colorAnimation = ValueAnimator.OfObject(new ArgbEvaluator(), fromColor, toColor);
			colorAnimation.SetDuration(150);
			colorAnimation.AddUpdateListener(
				new CustomViewPropertyIAnimatorUpdateListener(
					(animator) =>
					{
						view.SetBackgroundColor(new Color((int)animator.AnimatedValue));
					}
				)
			);
			colorAnimation.Start();
		}

		/**
	 	* Update image view color with animation
	 	*/
		public static void updateDrawableColor(Context context, Drawable drawable,
											   ImageView imageView, int fromColor,
		                                       int toColor, bool forceTint)
		{
			ValueAnimator colorAnimation = ValueAnimator.OfObject(new ArgbEvaluator(), fromColor, toColor);
			colorAnimation.SetDuration(150);
			colorAnimation.AddUpdateListener(
				new CustomViewPropertyIAnimatorUpdateListener(
					(animator) =>
					{
						imageView.SetImageDrawable(AHHelper.getTintDrawable(drawable,
																			(int)animator.AnimatedValue, forceTint));
					}
				)
			);
			colorAnimation.Start();
		}

		/**
	 	* Update width
	 	*/
		public static void updateWidth(View view, float fromWidth, float toWidth)
		{
			ValueAnimator animator = ValueAnimator.OfFloat(fromWidth, toWidth);
			animator.SetDuration(150);
			animator.AddUpdateListener(
				new CustomViewPropertyIAnimatorUpdateListener(
					(anim) =>
					{
						ViewGroup.LayoutParams param = view.LayoutParameters;
						param.Width = (int)Math.Round((float)anim.AnimatedValue);
						view.LayoutParameters = param;
					}
				)
			);
			animator.Start();
		}

		/**
	 	* Check if the status bar is translucent
	 	*
	 	* @param context Context
	 	* @return
	 	*/
		public static bool isTranslucentStatusBar(Context context)
		{
			Window w = unwrap(context).Window;
			WindowManagerLayoutParams lp = w.Attributes;
			var flags = lp.Flags;
			if ((flags & WindowManagerFlags.TranslucentNavigation) == WindowManagerFlags.TranslucentNavigation)
			{
				return true;
			}

			return false;
		}

		/**
	 	* Get the height of the buttons bar
	 	*
	 	* @param context Context
	 	* @return
	 	*/
		public static int getSoftButtonsBarSizePort(Context context)
		{
			if (Build.VERSION.SdkInt >= BuildVersionCodes.JellyBeanMr1)
			{
				DisplayMetrics metrics = new DisplayMetrics();
				Window window = unwrap(context).Window;
				window.WindowManager.DefaultDisplay.GetMetrics(metrics);
				int usableHeight = metrics.HeightPixels;
				window.WindowManager.DefaultDisplay.GetRealMetrics(metrics);
				int realHeight = metrics.HeightPixels;
				if (realHeight > usableHeight)
					return realHeight - usableHeight;
				else
					return 0;
			}
			return 0;
		}

		/**
	 	* Unwrap wactivity
	 	*
	 	* @param context Context
	 	* @return Activity
	 	*/
		public static Activity unwrap(Context context)
		{
			while (!(context is Activity))
			{
				ContextWrapper wrapper = (ContextWrapper)context;
				context = wrapper.BaseContext;
			}
			return (Activity)context;
		}

	}
}

