using System;
using AHBottomNavigation.AHBottomNavigation;
using Android.Animation;
using Android.Content;
using Android.Content.Res;
using Android.OS;
using Android.Support.Design.Widget;
using Android.Support.V4.View;
using Android.Support.V4.View.Animation;
using Android.Util;
using Android.Views;
using Android.Views.Animations;

namespace AHBottomNavigation.AHBottomNavigation
{
	public class CustomViewPropertyAnimatorUpdateListener : Java.Lang.Object, IViewPropertyAnimatorUpdateListener
	{
		Action<View> _onAnimationUpdate;

		public CustomViewPropertyAnimatorUpdateListener(Action<View> onAnimationUpdate)
		{
			_onAnimationUpdate = onAnimationUpdate;
		}

		public void OnAnimationUpdate(View view)
		{
			_onAnimationUpdate?.Invoke(view);
		}
	}

	public class CustomViewPropertyIAnimatorUpdateListener : Java.Lang.Object, ValueAnimator.IAnimatorUpdateListener
	{
		Action<ValueAnimator> _onAnimationUpdate;

		public CustomViewPropertyIAnimatorUpdateListener(Action<ValueAnimator> onAnimationUpdate)
		{
			_onAnimationUpdate = onAnimationUpdate;
		}

		public void OnAnimationUpdate(ValueAnimator animation)
		{
			_onAnimationUpdate?.Invoke(animation);
		}
	}

	public class CustomIOnLayoutChangeListener : Java.Lang.Object, View.IOnLayoutChangeListener
	{
		Action<View, int, int, int, int, int, int, int, int> _onLayoutChange;

		public CustomIOnLayoutChangeListener(Action<View, int, int, int, int, int, int, int, int> onLayoutChange)
		{
			_onLayoutChange = onLayoutChange;
		}

		public void OnLayoutChange(View v, int left, int top, int right, int bottom, int oldLeft, int oldTop, int oldRight, int oldBottom)
		{
			_onLayoutChange?.Invoke(v, left, top, right, bottom, oldLeft, oldTop, oldRight, oldBottom);
		}
	}

	public class AHBottomNavigationBehavior<T> : VerticalScrollingBehavior<T> where T : View
	{
		private static IInterpolator INTERPOLATOR = new LinearOutSlowInInterpolator();
		private static int ANIM_DURATION = 300;

		private int mTabLayoutId;
		private bool hidden = false;
		private ViewPropertyAnimatorCompat translationAnimator;
		private ObjectAnimator translationObjectAnimator;
		private TabLayout mTabLayout;
		private Snackbar.SnackbarLayout snackbarLayout;
		private FloatingActionButton floatingActionButton;
		private int mSnackbarHeight = -1;
		private bool fabBottomMarginInitialized = false;
		private float targetOffset = 0, fabTargetOffset = 0, fabDefaultBottomMargin = 0, snackBarY = 0;
		private bool behaviorTranslationEnabled = true;
		private AHBottomNavigation.IOnNavigationPositionListener navigationPositionListener;

		/**
	 	* Constructor
	 	*/
		public AHBottomNavigationBehavior():base()
		{
		}

		public AHBottomNavigationBehavior(bool behaviorTranslationEnabled):base()
		{
			this.behaviorTranslationEnabled = behaviorTranslationEnabled;
		}

		public AHBottomNavigationBehavior(Context context, IAttributeSet attrs):base(context, attrs)
		{
			TypedArray a = context.ObtainStyledAttributes(attrs, Resource.Styleable.AHBottomNavigationBehavior_Params);
			mTabLayoutId = a.GetResourceId(Resource.Styleable.AHBottomNavigationBehavior_Params_tabLayoutId, View.NoId);
			a.Recycle();
		}

		public override bool OnLayoutChild(Android.Support.Design.Widget.CoordinatorLayout parent, Java.Lang.Object child, int layoutDirection)
		{
			//return base.OnLayoutChild(parent, child, layoutDirection);
			bool layoutChild = base.OnLayoutChild(parent, child, layoutDirection);
			if (mTabLayout == null && mTabLayoutId != View.NoId)
			{
				mTabLayout = findTabLayout((View)child);
			}
			return layoutChild;
		}

		private TabLayout findTabLayout(View child)
		{
			if (mTabLayoutId == 0)
				return null;
			return (TabLayout)child.FindViewById(mTabLayoutId);
		}

		public override bool OnDependentViewChanged(CoordinatorLayout parent, Java.Lang.Object child, View dependency)
		{
			return base.OnDependentViewChanged(parent, child, dependency);
		}

		public override void OnDependentViewRemoved(CoordinatorLayout parent, Java.Lang.Object child, View dependency)
		{
			base.OnDependentViewRemoved(parent, child, dependency);
		}

		public override bool LayoutDependsOn(CoordinatorLayout parent, Java.Lang.Object child, View dependency)
		{
			updateSnackbar((Android.Views.View)child, dependency);
			updateFloatingActionButton(dependency);
			return base.LayoutDependsOn(parent, child, dependency);
		}

		public override void onNestedVerticalOverScroll(CoordinatorLayout coordinatorLayout, T child, ScrollDirection direction, int currentOverScroll, int totalOverScroll)
		{
			
		}

		public override void onDirectionNestedPreScroll(CoordinatorLayout coordinatorLayout, T child, View target, int dx, int dy, int[] consumed, ScrollDirection scrollDirection)
		{
			
		}

		protected override bool onNestedDirectionFling(CoordinatorLayout coordinatorLayout, T child, View target, float velocityX, float velocityY, ScrollDirection scrollDirection)
		{
			return false;
		}

		public override bool OnStartNestedScroll(CoordinatorLayout coordinatorLayout, Java.Lang.Object child, View directTargetChild, View target, int nestedScrollAxes)
		{
			//return base.OnStartNestedScroll(coordinatorLayout, child, directTargetChild, target, nestedScrollAxes);
			return nestedScrollAxes == ViewCompat.ScrollAxisVertical || base.OnStartNestedScroll(coordinatorLayout, child, directTargetChild, target, nestedScrollAxes);
		}

		/**
	 	* Handle scroll direction
	 	* @param child
	 	* @param scrollDirection
	 	*/
		private void handleDirection(T child, ScrollDirection scrollDirection)
		{
			if (!behaviorTranslationEnabled)
			{
				return;
			}
			if (scrollDirection == ScrollDirection.SCROLL_DIRECTION_DOWN && hidden)
			{
				hidden = false;
				animateOffset(child, 0, false, true);
			}
			else if (scrollDirection == ScrollDirection.SCROLL_DIRECTION_UP && !hidden)
			{
				hidden = true;
				animateOffset(child, child.Height, false, true);
			}
		}

		/**
	 	* Animate offset
	 	*
	 	* @param child
	 	* @param offset
	 	*/
		private void animateOffset(T child, int offset, bool forceAnimation, bool withAnimation)
		{
			if (!behaviorTranslationEnabled && !forceAnimation)
			{
				return;
			}
			if (Build.VERSION.SdkInt < BuildVersionCodes.Kitkat)
			{
				ensureOrCancelObjectAnimation(child, offset, withAnimation);
				translationObjectAnimator.Start();
			}
			else {
				ensureOrCancelAnimator(child, withAnimation);
				translationAnimator.TranslationY(offset).Start();
			}
		}

		/**
		 * Manage animation for Android >= KITKAT
	 	*
	 	* @param child
	 	*/
		private void ensureOrCancelAnimator(T child, bool withAnimation)
		{
			if (translationAnimator == null)
			{
				translationAnimator = ViewCompat.Animate(child);
				translationAnimator.SetDuration(withAnimation ? ANIM_DURATION : 0);
				translationAnimator.SetUpdateListener(
					new CustomViewPropertyAnimatorUpdateListener((view) =>
					{
						// Animate snackbar
						if (snackbarLayout != null && snackbarLayout.LayoutParameters is ViewGroup.MarginLayoutParams)
						{
							targetOffset = view.MeasuredHeight - view.TranslationY;
							ViewGroup.MarginLayoutParams p = (ViewGroup.MarginLayoutParams)snackbarLayout.LayoutParameters;
							p.SetMargins(p.LeftMargin, p.TopMargin, p.RightMargin, (int)targetOffset);
							snackbarLayout.RequestLayout();
						}
						// Animate Floating Action Button
						if (floatingActionButton != null && floatingActionButton.LayoutParameters is ViewGroup.MarginLayoutParams)
						{
							ViewGroup.MarginLayoutParams p = (ViewGroup.MarginLayoutParams)floatingActionButton.LayoutParameters;
							fabTargetOffset = fabDefaultBottomMargin - view.TranslationY + snackBarY;
							p.SetMargins(p.LeftMargin, p.TopMargin, p.RightMargin, (int)fabTargetOffset);
							floatingActionButton.RequestLayout();
						}
						// Pass navigation height to listener
						if (navigationPositionListener != null)
						{
							navigationPositionListener.onPositionChange((int)(view.MeasuredHeight - view.TranslationY + snackBarY));
						}
					})
				);
				translationAnimator.SetInterpolator(INTERPOLATOR);
			}
			else {
				translationAnimator.SetDuration(withAnimation ? ANIM_DURATION : 0);
				translationAnimator.Cancel();
			}
		}

		/**
	 	* Manage animation for Android < KITKAT
	 	*
	 	* @param child
	 	*/
		private void ensureOrCancelObjectAnimation(T child, int offset, bool withAnimation)
		{

			if (translationObjectAnimator != null)
			{
				translationObjectAnimator.Cancel();
			}

			translationObjectAnimator = ObjectAnimator.OfFloat(child, /*View.TRANSLATION_Y*/"translationY", offset);
			translationObjectAnimator.SetDuration(withAnimation ? ANIM_DURATION : 0);
			translationObjectAnimator.SetInterpolator(INTERPOLATOR);
			translationObjectAnimator.AddUpdateListener(
				new CustomViewPropertyIAnimatorUpdateListener((animation) =>
				{
					if (snackbarLayout != null && snackbarLayout.LayoutParameters is ViewGroup.MarginLayoutParams)
					{
						targetOffset = child.MeasuredHeight - child.TranslationY;
						ViewGroup.MarginLayoutParams p = (ViewGroup.MarginLayoutParams)snackbarLayout.LayoutParameters;
						p.SetMargins(p.LeftMargin, p.TopMargin, p.RightMargin, (int)targetOffset);
						snackbarLayout.RequestLayout();
					}
					// Animate Floating Action Button
					if (floatingActionButton != null && floatingActionButton.LayoutParameters is ViewGroup.MarginLayoutParams)
					{
						ViewGroup.MarginLayoutParams p = (ViewGroup.MarginLayoutParams)floatingActionButton.LayoutParameters;
						fabTargetOffset = fabDefaultBottomMargin - child.TranslationY + snackBarY;
						p.SetMargins(p.LeftMargin, p.TopMargin, p.RightMargin, (int)fabTargetOffset);
						floatingActionButton.RequestLayout();
					}
					// Pass navigation height to listener
					if (navigationPositionListener != null)
					{
						navigationPositionListener.onPositionChange((int)(child.MeasuredHeight - child.TranslationY + snackBarY));
					}
				})
			);
		}

		public static AHBottomNavigationBehavior<T> from(T view)
		{
			ViewGroup.LayoutParams param = view.LayoutParameters;
			if (!(param is CoordinatorLayout.LayoutParams)) {
				throw new ArgumentException("The view is not a child of CoordinatorLayout");
			}
			CoordinatorLayout.Behavior behavior = ((CoordinatorLayout.LayoutParams)param)
				.Behavior;
			if (!(behavior is AHBottomNavigationBehavior<T>)) {
				throw new ArgumentException(
						"The view is not associated with AHBottomNavigationBehavior");
			}
			return (AHBottomNavigationBehavior<T>)behavior;
		}

		public void setTabLayoutId(int tabId)
		{
			this.mTabLayoutId = tabId;
		}

		/**
		 * Enable or not the behavior translation
		 * @param behaviorTranslationEnabled
		 */
		public void setBehaviorTranslationEnabled(bool behaviorTranslationEnabled)
		{
			this.behaviorTranslationEnabled = behaviorTranslationEnabled;
		}

		/**
		 * Set OnNavigationPositionListener
		 */
		public void setOnNavigationPositionListener(AHBottomNavigation.IOnNavigationPositionListener navigationHeightListener)
		{
			this.navigationPositionListener = navigationHeightListener;
		}

		/**
		 * Remove OnNavigationPositionListener()
		 */
		public void removeOnNavigationPositionListener()
		{
			this.navigationPositionListener = null;
		}

		/**
		 * Hide AHBottomNavigation with animation
		 * @param view
		 * @param offset
		 */
		public void hideView(T view, int offset, bool withAnimation)
		{
			if (!hidden)
			{
				hidden = true;
				animateOffset(view, offset, true, withAnimation);
			}
		}

		/**
		 * Reset AHBottomNavigation position with animation
		 * @param view
		 */
		public void resetOffset(T view, bool withAnimation)
		{
			if (hidden)
			{
				hidden = false;
				animateOffset(view, 0, true, withAnimation);
			}
		}

		/**
	 	* Update Snackbar bottom margin
	 	*/
		public void updateSnackbar(View child, View dependency)
		{

			if (dependency != null && dependency is Snackbar.SnackbarLayout)
			{

				snackbarLayout = (Snackbar.SnackbarLayout)dependency;
				if (Build.VERSION.SdkInt >= BuildVersionCodes.Kitkat)
				{
					snackbarLayout.AddOnLayoutChangeListener(
						new CustomIOnLayoutChangeListener(
							(View v, int left, int top, int right, int bottom, int oldLeft, int oldTop, int oldRight, int oldBottom) =>
							{
								if (floatingActionButton != null &&
									floatingActionButton.LayoutParameters is ViewGroup.MarginLayoutParams)
								{
									ViewGroup.MarginLayoutParams p = (ViewGroup.MarginLayoutParams)floatingActionButton.LayoutParameters;
									snackBarY = bottom - v.GetY();
									fabTargetOffset = fabDefaultBottomMargin - child.TranslationY + snackBarY;
									p.SetMargins(p.LeftMargin, p.TopMargin, p.RightMargin, (int)fabTargetOffset);
									floatingActionButton.RequestLayout();
								}
								// Pass navigation height to listener
								if (navigationPositionListener != null)
								{
									navigationPositionListener.onPositionChange((int)(child.MeasuredHeight - child.TranslationY + snackBarY));
								}
							}
						)
					);
				}

				if (mSnackbarHeight == -1)
				{
					mSnackbarHeight = dependency.Height;
				}

				int targetMargin = (int)(child.MeasuredHeight - child.TranslationY);
				if (Build.VERSION.SdkInt < BuildVersionCodes.Lollipop)
				{
					child.BringToFront();
				}

				if (dependency.LayoutParameters is ViewGroup.MarginLayoutParams)
				{
					ViewGroup.MarginLayoutParams p = (ViewGroup.MarginLayoutParams)dependency.LayoutParameters;
					p.SetMargins(p.LeftMargin, p.TopMargin, p.RightMargin, targetMargin);
					dependency.RequestLayout();
				}
			}
		}

		/**
	 	* Update floating action button bottom margin
	 	*/
		public void updateFloatingActionButton(View dependency)
		{
			if (dependency != null && dependency is FloatingActionButton) {
				floatingActionButton = (FloatingActionButton)dependency;
				if (!fabBottomMarginInitialized && dependency.LayoutParameters is ViewGroup.MarginLayoutParams) {
					fabBottomMarginInitialized = true;
					ViewGroup.MarginLayoutParams p = (ViewGroup.MarginLayoutParams)dependency.LayoutParameters;
					fabDefaultBottomMargin = p.BottomMargin;
				}
			}
		}
	}
}

