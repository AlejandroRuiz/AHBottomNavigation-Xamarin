using System;
using Android.Content;
using Android.Support.Design.Widget;
using Android.Util;
using Android.Views;

namespace AHBottomNavigation.AHBottomNavigation
{
	public abstract class VerticalScrollingBehavior<T> : CoordinatorLayout.Behavior where T : View
	{

		private int mTotalDyUnconsumed = 0;
		private int mTotalDy = 0;
		private ScrollDirection mOverScrollDirection = ScrollDirection.SCROLL_NONE;
		private ScrollDirection mScrollDirection = ScrollDirection.SCROLL_NONE;

		public VerticalScrollingBehavior(Context context, IAttributeSet attrs):base(context, attrs)
		{
		}

		public VerticalScrollingBehavior():base()
		{
		}

		public enum ScrollDirection : int
		{
			SCROLL_DIRECTION_UP = 1,
			SCROLL_DIRECTION_DOWN = -1,
			SCROLL_NONE = 0
		}

		/*
	   	@return Overscroll direction: SCROLL_DIRECTION_UP, CROLL_DIRECTION_DOWN, SCROLL_NONE
   		*/
		public ScrollDirection getOverScrollDirection()
		{
			return mOverScrollDirection;
		}


		/**
		 * @return Scroll direction: SCROLL_DIRECTION_UP, SCROLL_DIRECTION_DOWN, SCROLL_NONE
		 */
		public ScrollDirection getScrollDirection()
		{
			return mScrollDirection;
		}

		/**
	 	* @param coordinatorLayout
	 	* @param child
	 	* @param direction         Direction of the overscroll: SCROLL_DIRECTION_UP, SCROLL_DIRECTION_DOWN
	 	* @param currentOverScroll Unconsumed value, negative or positive based on the direction;
	 	* @param totalOverScroll   Cumulative value for current direction
	 	*/
		public abstract void onNestedVerticalOverScroll(CoordinatorLayout coordinatorLayout, T child, ScrollDirection direction, int currentOverScroll, int totalOverScroll);

		/**
		 * @param scrollDirection Direction of the overscroll: SCROLL_DIRECTION_UP, SCROLL_DIRECTION_DOWN
		 */
		public abstract void onDirectionNestedPreScroll(CoordinatorLayout coordinatorLayout, T child, View target, int dx, int dy, int[] consumed, ScrollDirection scrollDirection);

		public override bool OnStartNestedScroll(CoordinatorLayout coordinatorLayout, Java.Lang.Object child, View directTargetChild, View target, int nestedScrollAxes)
		{
			//return base.OnStartNestedScroll(coordinatorLayout, child, directTargetChild, target, nestedScrollAxes);
			return (nestedScrollAxes & (int)View.ScrollAxisVertical) != 0;
		}

		public override void OnNestedScrollAccepted(CoordinatorLayout coordinatorLayout, Java.Lang.Object child, View directTargetChild, View target, int nestedScrollAxes)
		{
			base.OnNestedScrollAccepted(coordinatorLayout, child, directTargetChild, target, nestedScrollAxes);
		}

		public override void OnStopNestedScroll(CoordinatorLayout coordinatorLayout, Java.Lang.Object child, View target)
		{
			base.OnStopNestedScroll(coordinatorLayout, child, target);
		}

		public override void OnNestedScroll(CoordinatorLayout coordinatorLayout, Java.Lang.Object child, View target, int dxConsumed, int dyConsumed, int dxUnconsumed, int dyUnconsumed)
		{
			base.OnNestedScroll(coordinatorLayout, child, target, dxConsumed, dyConsumed, dxUnconsumed, dyUnconsumed);
			if (dyUnconsumed > 0 && mTotalDyUnconsumed < 0)
			{
				mTotalDyUnconsumed = 0;
				mOverScrollDirection = ScrollDirection.SCROLL_DIRECTION_UP;
			}
			else if (dyUnconsumed < 0 && mTotalDyUnconsumed > 0)
			{
				mTotalDyUnconsumed = 0;
				mOverScrollDirection = ScrollDirection.SCROLL_DIRECTION_DOWN;
			}
			mTotalDyUnconsumed += dyUnconsumed;
			onNestedVerticalOverScroll(coordinatorLayout, (T)child, mOverScrollDirection, dyConsumed, mTotalDyUnconsumed);
		}

		public override void OnNestedPreScroll(CoordinatorLayout coordinatorLayout, Java.Lang.Object child, View target, int dx, int dy, int[] consumed)
		{
			base.OnNestedPreScroll(coordinatorLayout, child, target, dx, dy, consumed);
			if (dy > 0 && mTotalDy < 0)
			{
				mTotalDy = 0;
				mScrollDirection = ScrollDirection.SCROLL_DIRECTION_UP;
			}
			else if (dy < 0 && mTotalDy > 0)
			{
				mTotalDy = 0;
				mScrollDirection = ScrollDirection.SCROLL_DIRECTION_DOWN;
			}
			mTotalDy += dy;
			onDirectionNestedPreScroll(coordinatorLayout, (T)child, target, dx, dy, consumed, mScrollDirection);
		}

		public override bool OnNestedFling(CoordinatorLayout coordinatorLayout, Java.Lang.Object child, View target, float velocityX, float velocityY, bool consumed)
		{
			base.OnNestedFling(coordinatorLayout, child, target, velocityX, velocityY, consumed);
			mScrollDirection = velocityY > 0 ? ScrollDirection.SCROLL_DIRECTION_UP : ScrollDirection.SCROLL_DIRECTION_DOWN;
			return onNestedDirectionFling(coordinatorLayout, (T)child, target, velocityX, velocityY, mScrollDirection);
		}

		protected abstract bool onNestedDirectionFling(CoordinatorLayout coordinatorLayout, T child, View target, float velocityX, float velocityY, ScrollDirection scrollDirection);

		public override bool OnNestedPreFling(CoordinatorLayout coordinatorLayout, Java.Lang.Object child, View target, float velocityX, float velocityY)
		{
			return base.OnNestedPreFling(coordinatorLayout, child, target, velocityX, velocityY);
		}

		public override Android.Support.V4.View.WindowInsetsCompat OnApplyWindowInsets(CoordinatorLayout coordinatorLayout, Java.Lang.Object child, Android.Support.V4.View.WindowInsetsCompat insets)
		{
			return base.OnApplyWindowInsets(coordinatorLayout, child, insets);
		}

		public override Android.OS.IParcelable OnSaveInstanceState(CoordinatorLayout parent, Java.Lang.Object child)
		{
			return base.OnSaveInstanceState(parent, child);
		}
	}
}

