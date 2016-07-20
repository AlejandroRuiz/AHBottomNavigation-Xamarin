using System;
using System.Collections.Generic;
using Android.App;
using Android.Support.V7.Widget;
using Android.Views;

namespace AHBottomNavigation.AHBottomNavigation
{
	public class AHBottomNavigationAdapter
	{
		private IMenu mMenu;
		private List<AHBottomNavigationItem> navigationItems;

		/**
		 * Constructor
		 *
		 * @param activity
		 * @param menuRes
		 */
		public AHBottomNavigationAdapter(Activity activity, int menuRes)
		{
			PopupMenu popupMenu = new PopupMenu(activity, null);
			mMenu = popupMenu.Menu;
			activity.MenuInflater.Inflate(menuRes, mMenu);
		}

		/**
		 * Setup bottom navigation
		 *
		 * @param ahBottomNavigation AHBottomNavigation: Bottom navigation
		 */
		public void setupWithBottomNavigation(AHBottomNavigation ahBottomNavigation)
		{
			setupWithBottomNavigation(ahBottomNavigation, null);
		}

		/**
		 * Setup bottom navigation (with colors)
		 *
		 * @param ahBottomNavigation AHBottomNavigation: Bottom navigation
		 * @param colors             int[]: Colors of the item
		 */
		public void setupWithBottomNavigation(AHBottomNavigation ahBottomNavigation, int[] colors)
		{
			if (navigationItems == null)
			{
				navigationItems = new List<AHBottomNavigationItem>();
			}
			else {
				navigationItems.Clear();
			}

			if (mMenu != null)
			{
				for (int i = 0; i < mMenu.Size(); i++)
				{
					IMenuItem item = mMenu.GetItem(i);
					if (colors != null && colors.Length >= mMenu.Size() && colors[i] != 0)
					{
						AHBottomNavigationItem navigationItem = new AHBottomNavigationItem(item.TitleFormatted.ToString(), item.Icon, colors[i]);
						navigationItems.Add(navigationItem);
					}
					else {
						AHBottomNavigationItem navigationItem = new AHBottomNavigationItem(item.TitleFormatted.ToString(), item.Icon);
						navigationItems.Add(navigationItem);
					}
				}
				ahBottomNavigation.removeAllItems();
				ahBottomNavigation.addItems(navigationItems);
			}
		}

		/**
		 * Get Menu Item
		 *
		 * @param index
		 * @return
		 */
		public IMenuItem getMenuItem(int index)
		{
			return mMenu.GetItem(index);
		}

		/**
		 * Get Navigation Item
		 *
		 * @param index
		 * @return
		 */
		public AHBottomNavigationItem getNavigationItem(int index)
		{
			return navigationItems[index];
		}

		/**
		 * Get position by menu id
		 *
		 * @param menuId
		 * @return
		 */
		public int getPositionByMenuId(int menuId)
		{
			for (int i = 0; i < mMenu.Size(); i++)
			{
				if (mMenu.GetItem(i).ItemId == menuId)
					return i;
			}
			return -1;
		}
	}
}