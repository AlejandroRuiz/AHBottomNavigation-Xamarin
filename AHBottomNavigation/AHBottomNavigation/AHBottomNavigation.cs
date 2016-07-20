using System;
using System.Collections.Generic;
using Android.Animation;
using Android.Content;
using Android.Content.Res;
using Android.Graphics;
using Android.Graphics.Drawables;
using Android.OS;
using Android.Support.Design.Widget;
using Android.Support.V4.Content;
using Android.Support.V4.View;
using Android.Support.V4.View.Animation;
using Android.Util;
using Android.Views;
using Android.Views.Animations;
using Android.Widget;

namespace AHBottomNavigation.AHBottomNavigation
{
	public class CustomAnimator : Java.Lang.Object, Animator.IAnimatorListener
	{
		Action<Animator> _onAnimationCancel;

		Action<Animator> _onAnimationEnd;

		Action<Animator> _onAnimationRepeat;

		Action<Animator> _onAnimationStart;

		public CustomAnimator(Action<Animator> onAnimationCancel, Action<Animator> onAnimationEnd, Action<Animator> onAnimationRepeat, Action<Animator> onAnimationStart)
		{
			_onAnimationCancel = onAnimationCancel;
			_onAnimationEnd = onAnimationEnd;
			_onAnimationRepeat = onAnimationRepeat;
			_onAnimationStart = onAnimationStart;
		}

		public void OnAnimationCancel(Animator animation)
		{
			_onAnimationCancel?.Invoke(animation);
		}

		public void OnAnimationEnd(Animator animation)
		{
			_onAnimationEnd?.Invoke(animation);
		}

		public void OnAnimationRepeat(Animator animation)
		{
			_onAnimationRepeat?.Invoke(animation);
		}

		public void OnAnimationStart(Animator animation)
		{
			_onAnimationStart?.Invoke(animation);
		}
	}

	public class AHBottomNavigation : FrameLayout
	{
		// Constant
		public const int CURRENT_ITEM_NONE = -1;
		public const int UPDATE_ALL_NOTIFICATIONS = -1;

		// Static
		private static String TAG = "AHBottomNavigation";
		private const int MIN_ITEMS = 3;
		private const int MAX_ITEMS = 5;

		// Listener
		private IOnTabSelectedListener tabSelectedListener;
		private IOnNavigationPositionListener navigationPositionListener;

		// Variables
		private Context context;
		private Resources resources;
		private List<AHBottomNavigationItem> items = new List<AHBottomNavigationItem>();
		private List<View> views = new List<View>();
		private AHBottomNavigationBehavior<AHBottomNavigation> bottomNavigationBehavior;
		private View backgroundColorView;
		private Animator circleRevealAnim;
		private bool colored = false;
		private String[] notifications = { "", "", "", "", "" };
		private bool isBehaviorTranslationSet = false;
		private int currentItem = 0;
		private int currentColor = 0;
		private bool behaviorTranslationEnabled = true;
		private bool needHideBottomNavigation = false;
		private bool hideBottomNavigationWithAnimation = false;

		// Variables (Styles)
		private Typeface titleTypeface;
		private int defaultBackgroundColor = Color.White.ToArgb();
		private int itemActiveColor;
		private int itemInactiveColor;
		private int titleColorActive;
		private int titleColorInactive;
		private int coloredTitleColorActive;
		private int coloredTitleColorInactive;
		private float titleActiveTextSize, titleInactiveTextSize;
		private int bottomNavigationHeight;
		private float selectedItemWidth, notSelectedItemWidth;
		private bool forceTint = false;
		private bool forceTitlesDisplay = false;

		// Notifications
		private int notificationTextColor;
		private int notificationBackgroundColor;
		private Drawable notificationBackgroundDrawable;
		private Typeface notificationTypeface;
		private int notificationActiveMarginLeft, notificationInactiveMarginLeft;
		private int notificationActiveMarginTop, notificationInactiveMarginTop;

		public AHBottomNavigation(Context context) : base(context)
		{
			init(context);
		}

		public AHBottomNavigation(Context context, IAttributeSet attrs) : base(context, attrs)
		{
			init(context);
		}

		public AHBottomNavigation(Context context, IAttributeSet attrs, int defStyleAttr) : base(context, attrs, defStyleAttr)
		{
			init(context);
		}

		protected override void OnSizeChanged(int w, int h, int oldw, int oldh)
		{
			base.OnSizeChanged(w, h, oldw, oldh);
			createItems();
		}

		protected override void OnMeasure(int widthMeasureSpec, int heightMeasureSpec)
		{
			base.OnMeasure(widthMeasureSpec, heightMeasureSpec);
			if (!isBehaviorTranslationSet)
			{
				//The translation behavior has to be set up after the super.onMeasure has been called.
				setBehaviorTranslationEnabled(behaviorTranslationEnabled);
				isBehaviorTranslationSet = true;
			}
		}

		protected override Android.OS.IParcelable OnSaveInstanceState()
		{
			//return base.OnSaveInstanceState();
			Bundle bundle = new Bundle();
			bundle.PutParcelable("superState", base.OnSaveInstanceState());
			bundle.PutInt("current_item", currentItem);
			bundle.PutStringArray("notifications", notifications);
			return bundle;
		}

		protected override void OnRestoreInstanceState(IParcelable state)
		{
			if (state is Bundle)
			{
				Bundle bundle = (Bundle)state;
				currentItem = bundle.GetInt("current_item");
				notifications = bundle.GetStringArray("notifications");
				state = (Android.OS.IParcelable)bundle.GetParcelable("superState");
			}
			base.OnRestoreInstanceState(state);
		}

		/////////////
		// PRIVATE //
		/////////////

		/**
		 * Init
		 *
		 * @param context
		 */
		private void init(Context context)
		{
			this.context = context;
			resources = this.context.Resources;

			notificationTextColor = ContextCompat.GetColor(context, Android.Resource.Color.White);
			bottomNavigationHeight = (int)resources.GetDimension(Resource.Dimension.bottom_navigation_height);

			// Item colors
			titleColorActive = ContextCompat.GetColor(context, Resource.Color.colorBottomNavigationAccent);
			titleColorInactive = ContextCompat.GetColor(context, Resource.Color.colorBottomNavigationInactive);
			// Colors for colored bottom navigation
			coloredTitleColorActive = ContextCompat.GetColor(context, Resource.Color.colorBottomNavigationActiveColored);
			coloredTitleColorInactive = ContextCompat.GetColor(context, Resource.Color.colorBottomNavigationInactiveColored);

			itemActiveColor = titleColorActive;
			itemInactiveColor = titleColorInactive;

			// Notifications
			notificationActiveMarginLeft = (int)resources.GetDimension(Resource.Dimension.bottom_navigation_notification_margin_left_active);
			notificationInactiveMarginLeft = (int)resources.GetDimension(Resource.Dimension.bottom_navigation_notification_margin_left);
			notificationActiveMarginTop = (int)resources.GetDimension(Resource.Dimension.bottom_navigation_notification_margin_top_active);
			notificationInactiveMarginTop = (int)resources.GetDimension(Resource.Dimension.bottom_navigation_notification_margin_top);

			ViewCompat.SetElevation(this, resources.GetDimension(Resource.Dimension.bottom_navigation_elevation));
			SetClipToPadding(false);

			ViewGroup.LayoutParams param = new ViewGroup.LayoutParams(ViewGroup.LayoutParams.MatchParent, bottomNavigationHeight);
			LayoutParameters = param;
		}

		/**
	 	* Create the items in the bottom navigation
	 	*/
		private void createItems()
		{
			if (items.Count < MIN_ITEMS)
			{
				Log.Warn(TAG, "The items list should have at least 3 items");
			}
			else if (items.Count > MAX_ITEMS)
			{
				Log.Warn(TAG, "The items list should not have more than 5 items");
			}

			int layoutHeight = (int)resources.GetDimension(Resource.Dimension.bottom_navigation_height);

			RemoveAllViews();
			views.Clear();
			backgroundColorView = new View(context);
			if (Build.VERSION.SdkInt >= Android.OS.BuildVersionCodes.Lollipop)
			{
				LayoutParams backgroundLayoutParams = new LayoutParams(
					ViewGroup.LayoutParams.MatchParent, layoutHeight);
				AddView(backgroundColorView, backgroundLayoutParams);
			}

			LinearLayout linearLayout = new LinearLayout(context);
			linearLayout.Orientation = Android.Widget.Orientation.Horizontal;
			linearLayout.SetGravity(GravityFlags.Center);

			LayoutParams layoutParams = new LayoutParams(ViewGroup.LayoutParams.MatchParent, layoutHeight);
			AddView(linearLayout, layoutParams);

			if (items.Count == MIN_ITEMS || forceTitlesDisplay)
			{
				createClassicItems(linearLayout);
			}
			else {
				createSmallItems(linearLayout);
			}

			// Force a request layout after all the items have been created
			Post(() =>
			{
				RequestLayout();
			});
		}

		/**
	 	* Create classic items (only 3 items in the bottom navigation)
	 	*
	 	* @param linearLayout The layout where the items are added
	 	*/
		private void createClassicItems(LinearLayout linearLayout)
		{

			LayoutInflater inflater = (LayoutInflater)context.GetSystemService(Context.LayoutInflaterService);

			float height = resources.GetDimension(Resource.Dimension.bottom_navigation_height);
			float minWidth = resources.GetDimension(Resource.Dimension.bottom_navigation_min_width);
			float maxWidth = resources.GetDimension(Resource.Dimension.bottom_navigation_max_width);

			if (forceTitlesDisplay && items.Count > MIN_ITEMS)
			{
				minWidth = resources.GetDimension(Resource.Dimension.bottom_navigation_small_inactive_min_width);
				maxWidth = resources.GetDimension(Resource.Dimension.bottom_navigation_small_inactive_max_width);
			}

			int layoutWidth = Width;
			if (layoutWidth == 0 || items.Count == 0)
			{
				return;
			}

			float itemWidth = layoutWidth / items.Count;
			if (itemWidth < minWidth)
			{
				itemWidth = minWidth;
			}
			else if (itemWidth > maxWidth)
			{
				itemWidth = maxWidth;
			}

			float activeSize = resources.GetDimension(Resource.Dimension.bottom_navigation_text_size_active);
			float inactiveSize = resources.GetDimension(Resource.Dimension.bottom_navigation_text_size_inactive);
			int activePaddingTop = (int)resources.GetDimension(Resource.Dimension.bottom_navigation_margin_top_active);

			if (titleActiveTextSize != 0 && titleInactiveTextSize != 0)
			{
				activeSize = titleActiveTextSize;
				inactiveSize = titleInactiveTextSize;
			}
			else if (forceTitlesDisplay && items.Count > MIN_ITEMS)
			{
				activeSize = resources.GetDimension(Resource.Dimension.bottom_navigation_text_size_forced_active);
				inactiveSize = resources.GetDimension(Resource.Dimension.bottom_navigation_text_size_forced_inactive);
			}

			for (int i = 0; i < items.Count; i++)
			{

				int itemIndex = i;
				AHBottomNavigationItem item = items[itemIndex];

				View view = inflater.Inflate(Resource.Layout.bottom_navigation_item, this, false);
				FrameLayout container = (FrameLayout)view.FindViewById(Resource.Id.bottom_navigation_container);
				ImageView icon = (ImageView)view.FindViewById(Resource.Id.bottom_navigation_item_icon);
				TextView title = (TextView)view.FindViewById(Resource.Id.bottom_navigation_item_title);
				TextView notification = (TextView)view.FindViewById(Resource.Id.bottom_navigation_notification);

				icon.SetImageDrawable(item.getDrawable(context));
				title.Text = item.getTitle(context);

				if (titleTypeface != null)
				{
					title.Typeface = titleTypeface;
				}

				if (forceTitlesDisplay && items.Count > MIN_ITEMS)
				{
					container.SetPadding(0, container.PaddingTop, 0, container.PaddingBottom);
				}

				if (i == currentItem)
				{
					icon.Selected = true;
					// Update margins (icon & notification)
					if (view.LayoutParameters is ViewGroup.MarginLayoutParams)
					{
						ViewGroup.MarginLayoutParams p = (ViewGroup.MarginLayoutParams)icon.LayoutParameters;
						p.SetMargins(p.LeftMargin, activePaddingTop, p.RightMargin, p.BottomMargin);

						ViewGroup.MarginLayoutParams paramsNotification = (ViewGroup.MarginLayoutParams)
							notification.LayoutParameters;
						paramsNotification.SetMargins(notificationActiveMarginLeft, paramsNotification.TopMargin,
						paramsNotification.RightMargin, paramsNotification.BottomMargin);
						view.RequestLayout();
					}
				}
				else {
					icon.Selected = false;
					ViewGroup.MarginLayoutParams paramsNotification = (ViewGroup.MarginLayoutParams)
						notification.LayoutParameters;
					paramsNotification.SetMargins(notificationInactiveMarginLeft, paramsNotification.TopMargin,
						paramsNotification.RightMargin, paramsNotification.BottomMargin);
				}

				if (colored)
				{
					if (i == currentItem)
					{
						SetBackgroundColor(new Color(item.getColor(context)));
						currentColor = item.getColor(context);
					}
				}
				else {
					SetBackgroundColor(new Color(defaultBackgroundColor));
				}
				icon.SetImageDrawable(AHHelper.getTintDrawable(items[i].getDrawable(context),
															   currentItem == i ? itemActiveColor : itemInactiveColor, forceTint));
				title.SetTextColor(new Color(currentItem == i ? itemActiveColor : itemInactiveColor));
				title.SetTextSize(ComplexUnitType.Px, currentItem == i ? activeSize : inactiveSize);
				view.Click += (sender, e) =>
				{
					updateItems(itemIndex, true);
				};
				LayoutParams param = new LayoutParams((int)itemWidth, (int)height);
				linearLayout.AddView(view, param);
				views.Add(view);
			}
			updateNotifications(true, UPDATE_ALL_NOTIFICATIONS);
		}

		/**
	 	* Create small items (more than 3 items in the bottom navigation)
	 	*
	 	* @param linearLayout The layout where the items are added
	 	*/
		private void createSmallItems(LinearLayout linearLayout)
		{

			LayoutInflater inflater = (LayoutInflater)context.GetSystemService(Context.LayoutInflaterService);

			float height = resources.GetDimension(Resource.Dimension.bottom_navigation_height);
			float minWidth = resources.GetDimension(Resource.Dimension.bottom_navigation_small_inactive_min_width);
			float maxWidth = resources.GetDimension(Resource.Dimension.bottom_navigation_small_inactive_max_width);

			int layoutWidth = Width;
			if (layoutWidth == 0 || items.Count == 0)
			{
				return;
			}

			float itemWidth = layoutWidth / items.Count;

			if (itemWidth < minWidth)
			{
				itemWidth = minWidth;
			}
			else if (itemWidth > maxWidth)
			{
				itemWidth = maxWidth;
			}

			int activeMarginTop = (int)resources.GetDimension(Resource.Dimension.bottom_navigation_small_margin_top_active);
			float difference = resources.GetDimension(Resource.Dimension.bottom_navigation_small_selected_width_difference);

			selectedItemWidth = itemWidth + items.Count * difference;
			itemWidth -= difference;
			notSelectedItemWidth = itemWidth;


			for (int i = 0; i < items.Count; i++)
			{

				int itemIndex = i;
				AHBottomNavigationItem item = items[itemIndex];

				View view = inflater.Inflate(Resource.Layout.bottom_navigation_small_item, this, false);
				ImageView icon = (ImageView)view.FindViewById(Resource.Id.bottom_navigation_small_item_icon);
				TextView title = (TextView)view.FindViewById(Resource.Id.bottom_navigation_small_item_title);
				TextView notification = (TextView)view.FindViewById(Resource.Id.bottom_navigation_notification);
				icon.SetImageDrawable(item.getDrawable(context));
				title.Text = item.getTitle(context);

				if (titleActiveTextSize != 0)
				{
					title.SetTextSize(ComplexUnitType.Px, titleActiveTextSize);
				}

				if (titleTypeface != null)
				{
					title.Typeface = titleTypeface;
				}

				if (i == currentItem)
				{
					icon.Selected = true;
					// Update margins (icon & notification)
					if (view.LayoutParameters is ViewGroup.MarginLayoutParams)
					{
						ViewGroup.MarginLayoutParams p = (ViewGroup.MarginLayoutParams)icon.LayoutParameters;
						p.SetMargins(p.LeftMargin, activeMarginTop, p.RightMargin, p.BottomMargin);

						ViewGroup.MarginLayoutParams paramsNotification = (ViewGroup.MarginLayoutParams)
						notification.LayoutParameters;
						paramsNotification.SetMargins(notificationActiveMarginLeft, notificationActiveMarginTop,
								paramsNotification.RightMargin, paramsNotification.BottomMargin);

						view.RequestLayout();
					}
				}
				else {
					icon.Selected = false;
					ViewGroup.MarginLayoutParams paramsNotification = (ViewGroup.MarginLayoutParams)
							notification.LayoutParameters;
					paramsNotification.SetMargins(notificationInactiveMarginLeft, notificationInactiveMarginTop,
												  paramsNotification.RightMargin, paramsNotification.BottomMargin);
				}

				if (colored)
				{
					if (i == currentItem)
					{
						SetBackgroundColor(new Color(item.getColor(context)));
						currentColor = item.getColor(context);
					}
				}
				else {
					SetBackgroundColor(new Color(defaultBackgroundColor));
				}

				icon.SetImageDrawable(AHHelper.getTintDrawable(items[i].getDrawable(context),
						currentItem == i ? itemActiveColor : itemInactiveColor, forceTint));
				title.SetTextColor(new Color(currentItem == i ? itemActiveColor : itemInactiveColor));
				title.Alpha = (currentItem == i ? 1 : 0);
				view.Click += (sender, e) =>
				{
					updateSmallItems(itemIndex, true);
				};

				LayoutParams param = new LayoutParams(i == currentItem ? (int)selectedItemWidth :
						(int)itemWidth, (int)height);
				linearLayout.AddView(view, param);
				views.Add(view);
			}

			updateNotifications(true, UPDATE_ALL_NOTIFICATIONS);
		}

		/**
	 	* Update Items UI
	 	*
	 	* @param itemIndex   int: Selected item position
	 	* @param useCallback boolean: Use or not the callback
	 	*/
		private void updateItems(int itemIndex, bool useCallback)
		{

			if (currentItem == itemIndex)
			{
				if (tabSelectedListener != null && useCallback)
				{
					tabSelectedListener.onTabSelected(itemIndex, true);
				}
				return;
			}

			if (tabSelectedListener != null && useCallback)
			{
				bool selectionAllowed = tabSelectedListener.onTabSelected(itemIndex, false);
				if (!selectionAllowed) return;
			}

			int activeMarginTop = (int)resources.GetDimension(Resource.Dimension.bottom_navigation_margin_top_active);
			int inactiveMarginTop = (int)resources.GetDimension(Resource.Dimension.bottom_navigation_margin_top_inactive);
			float activeSize = resources.GetDimension(Resource.Dimension.bottom_navigation_text_size_active);
			float inactiveSize = resources.GetDimension(Resource.Dimension.bottom_navigation_text_size_inactive);

			if (titleActiveTextSize != 0 && titleInactiveTextSize != 0)
			{
				activeSize = titleActiveTextSize;
				inactiveSize = titleInactiveTextSize;
			}
			else if (forceTitlesDisplay && items.Count > MIN_ITEMS)
			{
				activeSize = resources.GetDimension(Resource.Dimension.bottom_navigation_text_size_forced_active);
				inactiveSize = resources.GetDimension(Resource.Dimension.bottom_navigation_text_size_forced_inactive);
			}

			for (int i = 0; i < views.Count; i++)
			{

				if (i == itemIndex)
				{

					TextView title = (TextView)views[itemIndex].FindViewById(Resource.Id.bottom_navigation_item_title);
					ImageView icon = (ImageView)views[itemIndex].FindViewById(Resource.Id.bottom_navigation_item_icon);
					TextView notification = (TextView)views[itemIndex].FindViewById(Resource.Id.bottom_navigation_notification);

					icon.Selected = true;
					AHHelper.updateTopMargin(icon, inactiveMarginTop, activeMarginTop);
					AHHelper.updateLeftMargin(notification, notificationInactiveMarginLeft, notificationActiveMarginLeft);
					AHHelper.updateTextColor(title, itemInactiveColor, itemActiveColor);
					AHHelper.updateTextSize(title, inactiveSize, activeSize);
					AHHelper.updateDrawableColor(context, items[itemIndex].getDrawable(context), icon,
												 itemInactiveColor, itemActiveColor, forceTint);

					if (Build.VERSION.SdkInt >= BuildVersionCodes.Lollipop && colored)
					{

						int finalRadius = Math.Max(Width, Height);
						int cx = (int)views[itemIndex].GetX() + views[itemIndex].Width / 2;
						int cy = views[itemIndex].Height / 2;

						if (circleRevealAnim != null && circleRevealAnim.IsRunning)
						{
							circleRevealAnim.Cancel();
							SetBackgroundColor(new Color(items[itemIndex].getColor(context)));
							backgroundColorView.SetBackgroundColor(Color.Transparent);
						}

						circleRevealAnim = ViewAnimationUtils.CreateCircularReveal(backgroundColorView, cx, cy, 0, finalRadius);
						circleRevealAnim.StartDelay = 5;
						circleRevealAnim.AddListener(new CustomAnimator(
							null,
							(obj) =>
							{
								backgroundColorView.SetBackgroundColor(new Color(items[itemIndex].getColor(context)));
							},
							null,
							(obj) =>
							{
								SetBackgroundColor(new Color(items[itemIndex].getColor(context)));
								backgroundColorView.SetBackgroundColor(Color.Transparent);
							}
						));
						circleRevealAnim.Start();
					}
					else if (colored)
					{
						AHHelper.updateViewBackgroundColor(this, currentColor, items[itemIndex].getColor(context));
					}
					else {
						SetBackgroundColor(new Color(defaultBackgroundColor));
						backgroundColorView.SetBackgroundColor(Color.Transparent);
					}
				}
				else if (i == currentItem)
				{
					TextView title = (TextView)views[currentItem].FindViewById(Resource.Id.bottom_navigation_item_title);
					ImageView icon = (ImageView)views[currentItem].FindViewById(Resource.Id.bottom_navigation_item_icon);
					TextView notification = (TextView)views[currentItem].FindViewById(Resource.Id.bottom_navigation_notification);
					icon.Selected = false;
					AHHelper.updateTopMargin(icon, activeMarginTop, inactiveMarginTop);
					AHHelper.updateLeftMargin(notification, notificationActiveMarginLeft, notificationInactiveMarginLeft);
					AHHelper.updateTextColor(title, itemActiveColor, itemInactiveColor);
					AHHelper.updateTextSize(title, activeSize, inactiveSize);
					AHHelper.updateDrawableColor(context, items[currentItem].getDrawable(context), icon,
							itemActiveColor, itemInactiveColor, forceTint);
				}
			}
			currentItem = itemIndex;
			if (currentItem > 0 && currentItem < items.Count)
			{
				currentColor = items[currentItem].getColor(context);
			}
			else if (currentItem == CURRENT_ITEM_NONE)
			{
				SetBackgroundColor(new Color(defaultBackgroundColor));
				backgroundColorView.SetBackgroundColor(Color.Transparent);
			}

			/*
			if (tabSelectedListener != null && useCallback) {
				tabSelectedListener.onTabSelected(itemIndex, false);
			}
			*/
		}

		/**
	 	* Update Small items UI
	 	*
	 	* @param itemIndex   int: Selected item position
	 	* @param useCallback boolean: Use or not the callback
	 	*/
		private void updateSmallItems(int itemIndex, bool useCallback)
		{

			if (currentItem == itemIndex)
			{
				if (tabSelectedListener != null && useCallback)
				{
					tabSelectedListener.onTabSelected(itemIndex, true);
				}
				return;
			}

			if (tabSelectedListener != null && useCallback)
			{
				bool selectionAllowed = tabSelectedListener.onTabSelected(itemIndex, false);
				if (!selectionAllowed) return;
			}

			int activeMarginTop = (int)resources.GetDimension(Resource.Dimension.bottom_navigation_small_margin_top_active);
			int inactiveMargin = (int)resources.GetDimension(Resource.Dimension.bottom_navigation_small_margin_top);

			for (int i = 0; i < views.Count; i++)
			{

				if (i == itemIndex)
				{

					FrameLayout container = (FrameLayout)views[itemIndex].FindViewById(Resource.Id.bottom_navigation_small_container);
					TextView title = (TextView)views[itemIndex].FindViewById(Resource.Id.bottom_navigation_small_item_title);
					ImageView icon = (ImageView)views[itemIndex].FindViewById(Resource.Id.bottom_navigation_small_item_icon);
					TextView notification = (TextView)views[itemIndex].FindViewById(Resource.Id.bottom_navigation_notification);

					icon.Selected = (true);
					AHHelper.updateTopMargin(icon, inactiveMargin, activeMarginTop);
					AHHelper.updateLeftMargin(notification, notificationInactiveMarginLeft, notificationActiveMarginLeft);
					AHHelper.updateTopMargin(notification, notificationInactiveMarginTop, notificationActiveMarginTop);
					AHHelper.updateTextColor(title, itemInactiveColor, itemActiveColor);
					AHHelper.updateAlpha(title, 0, 1);
					AHHelper.updateWidth(container, notSelectedItemWidth, selectedItemWidth);
					AHHelper.updateDrawableColor(context, items[itemIndex].getDrawable(context), icon,
							itemInactiveColor, itemActiveColor, forceTint);

					if (Build.VERSION.SdkInt >= BuildVersionCodes.Lollipop && colored)
					{
						int finalRadius = Math.Max(Width, Height);
						int cx = (int)views[itemIndex].GetX() + views[itemIndex].Width / 2;
						int cy = views[itemIndex].Height / 2;

						if (circleRevealAnim != null && circleRevealAnim.IsRunning)
						{
							circleRevealAnim.Cancel();
							SetBackgroundColor(new Color(items[itemIndex].getColor(context)));
							backgroundColorView.SetBackgroundColor(Color.Transparent);
						}

						circleRevealAnim = ViewAnimationUtils.CreateCircularReveal(backgroundColorView, cx, cy, 0, finalRadius);
						circleRevealAnim.StartDelay = 5;
						circleRevealAnim.AddListener(new CustomAnimator(
							null,
							(obj) =>
							{
								SetBackgroundColor(new Color(items[itemIndex].getColor(context)));
								backgroundColorView.SetBackgroundColor(Color.Transparent);
							},
							null,
							(obj) =>
							{
								backgroundColorView.SetBackgroundColor(new Color(items[itemIndex].getColor(context)));
							}
						));
						circleRevealAnim.Start();
					}
					else if (colored)
					{
						AHHelper.updateViewBackgroundColor(this, currentColor,
														   items[itemIndex].getColor(context));
					}
					else {
						SetBackgroundColor(new Color(defaultBackgroundColor));
						backgroundColorView.SetBackgroundColor(Color.Transparent);
					}
				}
				else if (i == currentItem)
				{
					View container = views[currentItem].FindViewById(Resource.Id.bottom_navigation_small_container);
					TextView title = (TextView)views[currentItem].FindViewById(Resource.Id.bottom_navigation_small_item_title);
					ImageView icon = (ImageView)views[currentItem].FindViewById(Resource.Id.bottom_navigation_small_item_icon);
					TextView notification = (TextView)views[currentItem].FindViewById(Resource.Id.bottom_navigation_notification);
					icon.Selected = false;
					AHHelper.updateTopMargin(icon, activeMarginTop, inactiveMargin);
					AHHelper.updateLeftMargin(notification, notificationActiveMarginLeft, notificationInactiveMarginLeft);
					AHHelper.updateTopMargin(notification, notificationActiveMarginTop, notificationInactiveMarginTop);
					AHHelper.updateTextColor(title, itemActiveColor, itemInactiveColor);
					AHHelper.updateAlpha(title, 1, 0);
					AHHelper.updateWidth(container, selectedItemWidth, notSelectedItemWidth);
					AHHelper.updateDrawableColor(context, items[currentItem].getDrawable(context), icon,
												 itemActiveColor, itemInactiveColor, forceTint);

				}
			}

			currentItem = itemIndex;
			if (currentItem > 0 && currentItem < items.Count)
			{
				currentColor = items[currentItem].getColor(context);
			}
			else if (currentItem == CURRENT_ITEM_NONE)
			{
				SetBackgroundColor(new Color(defaultBackgroundColor));
				backgroundColorView.SetBackgroundColor(Color.Transparent);
			}

			/*
			if (tabSelectedListener != null && useCallback) {
				tabSelectedListener.onTabSelected(itemIndex, false);
			}
			*/
		}

		/**
	 	* Update notifications
		*/
		private void updateNotifications(bool updateStyle, int itemPosition)
		{

			for (int i = 0; i < views.Count; i++)
			{

				if (itemPosition != UPDATE_ALL_NOTIFICATIONS && itemPosition != i)
				{
					continue;
				}

				TextView notification = (TextView)views[i].FindViewById(Resource.Id.bottom_navigation_notification);

				string currentValue = notification.Text;
				bool animate = !(currentValue == notifications[i]);

				if (updateStyle)
				{
					notification.SetTextColor(new Color(notificationTextColor));
					if (notificationTypeface != null)
					{
						notification.Typeface = notificationTypeface;
					}
					else {
						notification.SetTypeface(null, TypefaceStyle.Bold);
					}

					if (notificationBackgroundDrawable != null)
					{
						if (Build.VERSION.SdkInt >= BuildVersionCodes.JellyBean)
						{
							Drawable drawable = notificationBackgroundDrawable.GetConstantState().NewDrawable();
							notification.Background = drawable;
						}
						else {
							notification.SetBackgroundDrawable(notificationBackgroundDrawable);
						}
					}
					else if (notificationBackgroundColor != 0)
					{
						Drawable defautlDrawable = ContextCompat.GetDrawable(context, Resource.Drawable.notification_background);
						if (Build.VERSION.SdkInt >= BuildVersionCodes.JellyBean)
						{
							notification.Background = (AHHelper.getTintDrawable(defautlDrawable,
																				notificationBackgroundColor, forceTint));
						}
						else {
							notification.SetBackgroundDrawable(AHHelper.getTintDrawable(defautlDrawable,
																						notificationBackgroundColor, forceTint));
						}
					}
				}

				if (notifications[i].Length == 0 && notification.Text.Length > 0)
				{
					notification.Text = "";
					if (animate)
					{
						notification.Animate()
								.ScaleX(0)
								.ScaleY(0)
								.Alpha(0)
								.SetInterpolator(new AccelerateInterpolator())
								.SetDuration(150)
								.Start();
					}
				}
				else if (notifications[i].Length > 0)
				{
					notification.Text = notifications[i];
					if (animate)
					{
						notification.ScaleX = 0;
						notification.ScaleY = 0;
						notification.Animate()
								.ScaleX(1)
								.ScaleY(1)
								.Alpha(1)
								.SetInterpolator(new OvershootInterpolator())
								.SetDuration(150)
								.Start();
					}
				}
			}
		}

		////////////
		// PUBLIC //
		////////////

		/**
		 * Add an item
		 */
		public void addItem(AHBottomNavigationItem item)
		{
			if (this.items.Count > MAX_ITEMS)
			{
				Log.Warn(TAG, "The items list should not have more than 5 items");
			}
			items.Add(item);
			createItems();
		}

		/**
		 * Add all items
		 */
		public void addItems(List<AHBottomNavigationItem> items)
		{
			if (items.Count > MAX_ITEMS || (this.items.Count + items.Count) > MAX_ITEMS)
			{
				Log.Warn(TAG, "The items list should not have more than 5 items");
			}
			this.items.AddRange(items);
			createItems();
		}

		/**
		 * Remove an item at the given index
		 */
		public void removeItemAtIndex(int index)
		{
			if (index < items.Count)
			{
				this.items.RemoveAt(index);
				createItems();
			}
		}

		/**
		 * Remove all items
		 */
		public void removeAllItems()
		{
			this.items.Clear();
			createItems();
		}

		/**
		 * Refresh the AHBottomView
		 */
		public void refresh()
		{
			createItems();
		}

		/**
		 * Return the number of items
		 *
		 * @return int
		 */
		public int getItemsCount()
		{
			return items.Count;
		}

		/**
		 * Return if the Bottom Navigation is colored
		 */
		public bool isColored()
		{
			return colored;
		}

		/**
		 * Set if the Bottom Navigation is colored
		 */
		public void setColored(bool colored)
		{
			this.colored = colored;
			this.itemActiveColor = colored ? coloredTitleColorActive : titleColorActive;
			this.itemInactiveColor = colored ? coloredTitleColorInactive : titleColorInactive;
			createItems();
		}

		/**
		 * Return the bottom navigation background color
		 *
		 * @return The bottom navigation background color
		 */
		public int getDefaultBackgroundColor()
		{
			return defaultBackgroundColor;
		}

		/**
		 * Set the bottom navigation background color
		 *
		 * @param defaultBackgroundColor The bottom navigation background color
		 */
		public void setDefaultBackgroundColor(int defaultBackgroundColor)
		{
			this.defaultBackgroundColor = defaultBackgroundColor;
			createItems();
		}

		/**
		 * Get the accent color (used when the view contains 3 items)
		 *
		 * @return The default accent color
		 */
		public int getAccentColor()
		{
			return itemActiveColor;
		}

		/**
		 * Set the accent color (used when the view contains 3 items)
		 *
		 * @param accentColor The new accent color
		 */
		public void setAccentColor(int accentColor)
		{
			this.titleColorActive = accentColor;
			this.itemActiveColor = accentColor;
			createItems();
		}

		/**
		 * Get the inactive color (used when the view contains 3 items)
		 *
		 * @return The inactive color
		 */
		public int getInactiveColor()
		{
			return itemInactiveColor;
		}

		/**
		 * Set the inactive color (used when the view contains 3 items)
		 *
		 * @param inactiveColor The inactive color
		 */
		public void setInactiveColor(int inactiveColor)
		{
			this.titleColorInactive = inactiveColor;
			this.itemInactiveColor = inactiveColor;
			createItems();
		}

		/**
		 * Set the colors used when the bottom bar uses the colored mode
		 *
		 * @param colorActive   The active color
		 * @param colorInactive The inactive color
		 */
		public void setColoredModeColors(int colorActive, int colorInactive)
		{
			this.coloredTitleColorActive = colorActive;
			this.coloredTitleColorInactive = colorInactive;
			createItems();
		}

		/**
		 * Set notification typeface
		 *
		 * @param typeface Typeface
		 */
		public void setTitleTypeface(Typeface typeface)
		{
			this.titleTypeface = typeface;
			createItems();
		}

		/**
		 * Set title text size
		 *
		 * @param activeSize
		 * @param inactiveSize
		 */
		public void setTitleTextSize(float activeSize, float inactiveSize)
		{
			this.titleActiveTextSize = activeSize;
			this.titleInactiveTextSize = inactiveSize;
			createItems();
		}

		/**
		 * Get item at the given index
		 *
		 * @param position int: item position
		 * @return The item at the given position
		 */
		public AHBottomNavigationItem getItem(int position)
		{
			if (position < 0 || position > items.Count - 1)
			{
				Log.Warn(TAG, "The position is out of bounds of the items (" + items.Count + " elements)");
			}
			return items[position];
		}

		/**
		 * Get the current item
		 *
		 * @return The current item position
		 */
		public int getCurrentItem()
		{
			return currentItem;
		}

		/**
		 * Set the current item
		 *
		 * @param position int: position
		 */
		public void setCurrentItem(int position)
		{
			setCurrentItem(position, true);
		}

		/**
		 * Set the current item
		 *
		 * @param position    int: item position
		 * @param useCallback boolean: use or not the callback
		 */
		public void setCurrentItem(int position, bool useCallback)
		{
			if (position >= items.Count)
			{
				Log.Warn(TAG, "The position is out of bounds of the items (" + items.Count + " elements)");
				return;
			}

			if (items.Count == MIN_ITEMS || forceTitlesDisplay)
			{
				updateItems(position, useCallback);
			}
			else {
				updateSmallItems(position, useCallback);
			}
		}

		/**
		 * Return if the behavior translation is enabled
		 *
		 * @return a boolean value
		 */
		public bool isBehaviorTranslationEnabled()
		{
			return behaviorTranslationEnabled;
		}

		/**
		 * Set the behavior translation value
		 *
		 * @param behaviorTranslationEnabled boolean for the state
		 */
		public void setBehaviorTranslationEnabled(bool behaviorTranslationEnabled)
		{
			this.behaviorTranslationEnabled = behaviorTranslationEnabled;
			if (Parent is CoordinatorLayout)
			{
				ViewGroup.LayoutParams param = LayoutParameters;
				if (bottomNavigationBehavior == null)
				{
					bottomNavigationBehavior = new AHBottomNavigationBehavior<AHBottomNavigation>(behaviorTranslationEnabled);
				}
				else {
					bottomNavigationBehavior.setBehaviorTranslationEnabled(behaviorTranslationEnabled);
				}
				if (navigationPositionListener != null)
				{
					bottomNavigationBehavior.setOnNavigationPositionListener(navigationPositionListener);
				}
				((CoordinatorLayout.LayoutParams)param).Behavior = bottomNavigationBehavior;
				if (needHideBottomNavigation)
				{
					needHideBottomNavigation = false;
					bottomNavigationBehavior.hideView(this, bottomNavigationHeight, hideBottomNavigationWithAnimation);
				}
			}
		}

		/**
		 * Hide Bottom Navigation with animation
		 */
		public void hideBottomNavigation()
		{
			hideBottomNavigation(true);
		}

		/**
		 * Hide Bottom Navigation with or without animation
		 *
		 * @param withAnimation Boolean
		 */
		public void hideBottomNavigation(bool withAnimation)
		{
			if (bottomNavigationBehavior != null)
			{
				bottomNavigationBehavior.hideView(this, bottomNavigationHeight, withAnimation);
			}
			else if (Parent is CoordinatorLayout)
			{
				needHideBottomNavigation = true;
				hideBottomNavigationWithAnimation = withAnimation;
			}
			else {
				// Hide bottom navigation
				ViewCompat.Animate(this)
						.TranslationY(bottomNavigationHeight)
						.SetInterpolator(new LinearOutSlowInInterpolator())
						.SetDuration(withAnimation ? 300 : 0)
						.Start();
			}
		}

		/**
		 * Restore Bottom Navigation with animation
		 */
		public void restoreBottomNavigation()
		{
			restoreBottomNavigation(true);
		}

		/**
		 * Restore Bottom Navigation with or without animation
		 *
		 * @param withAnimation Boolean
		 */
		public void restoreBottomNavigation(bool withAnimation)
		{
			if (bottomNavigationBehavior != null)
			{
				bottomNavigationBehavior.resetOffset(this, withAnimation);
			}
			else {
				// Show bottom navigation
				ViewCompat.Animate(this)
						.TranslationY(0)
						.SetInterpolator(new LinearOutSlowInInterpolator())
						.SetDuration(withAnimation ? 300 : 0)
						.Start();
			}
		}

		/**
		 * Return if the tint should be forced (with setColorFilter)
		 *
		 * @return Boolean
		 */
		public bool isForceTint()
		{
			return forceTint;
		}

		/**
		 * Set the force tint value
		 * If forceTint = true, the tint is made with drawable.setColorFilter(color, PorterDuff.Mode.SRC_IN);
		 *
		 * @param forceTint Boolean
		 */
		public void setForceTint(bool forceTint)
		{
			this.forceTint = forceTint;
			createItems();
		}

		/**
		 * Return if we force the titles to be displayed
		 *
		 * @return Boolean
		 */
		public bool isForceTitlesDisplay()
		{
			return forceTitlesDisplay;
		}

		/**
		 * Force the titles to be displayed (or used the classic behavior)
		 * Note: Against Material Design guidelines
		 *
		 * @param forceTitlesDisplay Boolean
		 */
		public void setForceTitlesDisplay(bool forceTitlesDisplay)
		{
			this.forceTitlesDisplay = forceTitlesDisplay;
			createItems();
		}

		/**
		 * Set AHOnTabSelectedListener
		 */
		public void setOnTabSelectedListener(IOnTabSelectedListener tabSelectedListener)
		{
			this.tabSelectedListener = tabSelectedListener;
		}

		/**
		 * Remove AHOnTabSelectedListener
		 */
		public void removeOnTabSelectedListener()
		{
			this.tabSelectedListener = null;
		}

		/**
		 * Set OnNavigationPositionListener
		 */
		public void setOnNavigationPositionListener(IOnNavigationPositionListener navigationPositionListener)
		{
			this.navigationPositionListener = navigationPositionListener;
			if (bottomNavigationBehavior != null)
			{
				bottomNavigationBehavior.setOnNavigationPositionListener(navigationPositionListener);
			}
		}

		/**
		 * Remove OnNavigationPositionListener()
		 */
		public void removeOnNavigationPositionListener()
		{
			this.navigationPositionListener = null;
			if (bottomNavigationBehavior != null)
			{
				bottomNavigationBehavior.removeOnNavigationPositionListener();
			}
		}

		/**
		 * Set the notification number
		 *
		 * @param nbNotification int
		 * @param itemPosition   int
		 */
		[Obsolete]
		public void setNotification(int nbNotification, int itemPosition)
		{
			if (itemPosition < 0 || itemPosition > items.Count - 1)
			{
				Log.Warn(TAG, "The position is out of bounds of the items (" + items.Count + " elements)");
				return;
			}
			notifications[itemPosition] = nbNotification == 0 ? "" : nbNotification.ToString();
			updateNotifications(false, itemPosition);
		}

		/**
		 * Set Notification content
		 *
		 * @param title        String
		 * @param itemPosition int
		 */
		public void setNotification(String title, int itemPosition)
		{
			notifications[itemPosition] = title;
			updateNotifications(false, itemPosition);
		}

		/**
		 * Set notification text color
		 *
		 * @param textColor int
		 */
		public void setNotificationTextColor(int textColor)
		{
			this.notificationTextColor = textColor;
			updateNotifications(true, UPDATE_ALL_NOTIFICATIONS);
		}

		/**
		 * Set notification text color
		 *
		 * @param textColor int
		 */
		public void setNotificationTextColorResource(int textColor)
		{
			this.notificationTextColor = ContextCompat.GetColor(context, textColor);
			updateNotifications(true, UPDATE_ALL_NOTIFICATIONS);
		}

		/**
		 * Set notification background resource
		 *
		 * @param drawable Drawable
		 */
		public void setNotificationBackground(Drawable drawable)
		{
			this.notificationBackgroundDrawable = drawable;
			updateNotifications(true, UPDATE_ALL_NOTIFICATIONS);
		}

		/**
		 * Set notification background color
		 *
		 * @param color int
		 */
		public void setNotificationBackgroundColor(int color)
		{
			this.notificationBackgroundColor = color;
			updateNotifications(true, UPDATE_ALL_NOTIFICATIONS);
		}

		/**
		 * Set notification background color
		 *
		 * @param color int
		 */
		public void setNotificationBackgroundColorResource(int color)
		{
			this.notificationBackgroundColor = ContextCompat.GetColor(context, color);
			updateNotifications(true, UPDATE_ALL_NOTIFICATIONS);
		}

		/**
		 * Set notification typeface
		 *
		 * @param typeface Typeface
		 */
		public void setNotificationTypeface(Typeface typeface)
		{
			this.notificationTypeface = typeface;
			updateNotifications(true, UPDATE_ALL_NOTIFICATIONS);
		}

		/**
		 * Set the notification margin left
		 *
		 * @param activeMargin
		 * @param inactiveMargin
		 */
		public void setNotificationMarginLeft(int activeMargin, int inactiveMargin)
		{
			this.notificationActiveMarginLeft = activeMargin;
			this.notificationInactiveMarginLeft = inactiveMargin;
			createItems();
		}

		/**
		 * Activate or not the elevation
		 *
		 * @param useElevation boolean
		 */
		public void setUseElevation(bool useElevation)
		{
			ViewCompat.SetElevation(this, useElevation ?
									resources.GetDimension(Resource.Dimension.bottom_navigation_elevation) : 0);
			SetClipToPadding(false);
		}

		/**
		 * Activate or not the elevation, and set the value
		 *
		 * @param useElevation boolean
		 * @param elevation    float
		 */
		public void setUseElevation(bool useElevation, float elevation)
		{
			ViewCompat.SetElevation(this, useElevation ? elevation : 0);
			SetClipToPadding(false);
		}

		////////////////
		// INTERFACES //
		////////////////

		/**
		 *
		 */
		public interface IOnTabSelectedListener
		{
			/**
			 * Called when a tab has been selected (clicked)
			 *
			 * @param position    int: Position of the selected tab
			 * @param wasSelected boolean: true if the tab was already selected
			 * @return boolean: true for updating the tab UI, false otherwise
			 */
			bool onTabSelected(int position, bool wasSelected);
		}

		public interface IOnNavigationPositionListener
		{
			/**
			 * Called when the bottom navigation position is changed
			 *
			 * @param y int: y translation of bottom navigation
			 */
			void onPositionChange(int y);
		}

	}
}

