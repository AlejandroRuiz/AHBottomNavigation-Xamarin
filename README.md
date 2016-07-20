# AHBottomNavigation-Xamarin
AHBottomNavigation Control - Bottom Navigation for Xamarin.Android

#Usage

AXML

```
<?xml version="1.0" encoding="utf-8"?>
<RelativeLayout xmlns:android="http://schemas.android.com/apk/res/android"
    android:orientation="vertical"
    android:layout_width="match_parent"
    android:layout_height="match_parent">
    <Button
        android:id="@+id/myButton"
        android:layout_width="match_parent"
        android:layout_height="wrap_content"
        android:text="@string/hello" />
    <AHBottomNavigation.AHBottomNavigation.AHBottomNavigation
        android:layout_alignParentBottom="true"
        android:id="@+id/bottom_navigation"
        android:layout_width="match_parent"
        android:layout_height="wrap_content"
        android:layout_gravity="bottom" />
</RelativeLayout>
```

CODE BEHIND

```
public class MainActivity : AppCompatActivity, AHBottomNavigation.AHBottomNavigation.IOnTabSelectedListener
	{
		int count = 1;

		public bool onTabSelected(int position, bool wasSelected)
		{
			var bottomNavigation = (AHBottomNavigation.AHBottomNavigation)FindViewById(Resource.Id.bottom_navigation);
			if (position == 1) 
			{
				
				bottomNavigation.setNotification("", 1);
			}

			var item = bottomNavigation.getItem(position);
			Toast.MakeText(this, item.getTitle(this), ToastLength.Short).Show();

			return true;
		}

		protected override void OnCreate(Bundle savedInstanceState)
		{
			base.OnCreate(savedInstanceState);

			// Set our view from the "main" layout resource
			SetContentView(Resource.Layout.Main);

			// Get our button from the layout resource,
			// and attach an event to it
			Button button = FindViewById<Button>(Resource.Id.myButton);

			button.Click += delegate { button.Text = string.Format("{0} clicks!", count++); };


			var bottomNavigation = (AHBottomNavigation.AHBottomNavigation)FindViewById(Resource.Id.bottom_navigation);

			// Create items
			var item1 = new AHBottomNavigationItem("Tab 1", Resource.Mipmap.ic_maps_place, Resource.Color.color_tab_1);
			var item2 = new AHBottomNavigationItem("Tab 2", Resource.Mipmap.ic_maps_local_bar, Resource.Color.color_tab_2);
			var item3 = new AHBottomNavigationItem("Tab 3", Resource.Mipmap.ic_maps_local_restaurant, Resource.Color.color_tab_3);

			// Add items
			bottomNavigation.addItem(item1);
			bottomNavigation.addItem(item2);
			bottomNavigation.addItem(item3);

			// Set background color
			bottomNavigation.setDefaultBackgroundColor(Color.ParseColor("#FEFEFE"));

			// Disable the translation inside the CoordinatorLayout
			bottomNavigation.setBehaviorTranslationEnabled(false);

			// Change colors
			bottomNavigation.setAccentColor(Color.ParseColor("#F63D2B"));
			bottomNavigation.setInactiveColor(Color.ParseColor("#747474"));

			// Force to tint the drawable (useful for font with icon for example)
			bottomNavigation.setForceTint(true);

			// Force the titles to be displayed (against Material Design guidelines!)
			bottomNavigation.setForceTitlesDisplay(true);

			// Use colored navigation with circle reveal effect
			bottomNavigation.setColored(true);

			// Set current item programmatically
			bottomNavigation.setCurrentItem(0);

			// Customize notification (title, background, typeface)
			bottomNavigation.setNotificationBackgroundColor(Color.ParseColor("#F63D2B"));

			// Add or remove notification for each item
			bottomNavigation.setNotification("4", 1);
			//bottomNavigation.setNotification("", 1);

			bottomNavigation.setOnTabSelectedListener(this);
		}
	}
```

BASED ON: https://github.com/aurelhubert/ahbottomnavigation
