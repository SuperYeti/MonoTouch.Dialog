using System;
using MonoTouch.Foundation;
using MonoTouch.UIKit;
using System.Drawing;

namespace MonoTouch.Dialog
{
	public class DialogViewController : UITableViewController
	{
		public UITableViewStyle Style = UITableViewStyle.Grouped;
		UITableView tableView;
		RootElement root;
		bool pushing;
		bool dirty;
		
		private bool rotateUIEnabled = NSUserDefaults.StandardUserDefaults.BoolForKey("UIInterfaceRotateEnabled");
		
		public bool RotateUIEnabled
		{
			get{return rotateUIEnabled;}
			set{rotateUIEnabled = value;}
		}
		
		public RootElement Root {
			get {
				return root;
			}
			set {
				if (root == value)
					return;
				root = value;
				root.TableView = tableView;					
				ReloadData ();
			}
		} 
		
		class Source : UITableViewSource {
			protected DialogViewController container;
			protected RootElement root;
			
			public Source (DialogViewController container)
			{
				this.container = container;
				root = container.root;
			}
			
			public override int RowsInSection (UITableView tableview, int section)
			{
				var s = root.Sections [section];
				return s.Elements.Count;
			}

			public override int NumberOfSections (UITableView tableView)
			{
				return root.Sections.Count;
			}

			public override string TitleForHeader (UITableView tableView, int section)
			{
				return root.Sections [section].Caption;
			}

			public override string TitleForFooter (UITableView tableView, int section)
			{
				return root.Sections [section].Footer;
			}

			public override UITableViewCell GetCell (UITableView tableView, MonoTouch.Foundation.NSIndexPath indexPath)
			{
				var section = root.Sections [indexPath.Section];
				var element = section.Elements [indexPath.Row];
				
				return element.GetCell (tableView);
			}
			
			public override void RowSelected (UITableView tableView, MonoTouch.Foundation.NSIndexPath indexPath)
			{
				var section = root.Sections [indexPath.Section];
				var element = section.Elements [indexPath.Row];

				element.Selected (container, tableView, indexPath);

			}			
			
			public override UIView GetViewForHeader (UITableView tableView, int sectionIdx)
			{
				var section = root.Sections [sectionIdx];
				return section.HeaderView;
			}

			public override float GetHeightForHeader (UITableView tableView, int sectionIdx)
			{
				var section = root.Sections [sectionIdx];
				if (section.HeaderView == null)
					return -1;
				return section.HeaderView.Frame.Height;
			}

			public override UIView GetViewForFooter (UITableView tableView, int sectionIdx)
			{
				var section = root.Sections [sectionIdx];
				return section.FooterView;
			}
			
			public override float GetHeightForFooter (UITableView tableView, int sectionIdx)
			{
				var section = root.Sections [sectionIdx];
				if (section.FooterView == null)
					return -1;
				return section.FooterView.Frame.Height;
			}
		}

		//
		// Performance trick, if we expose GetHeightForRow, the UITableView will
		// probe *every* row for its size;   Avoid this by creating a separate
		// model that is used only when we have items that require resizing
		//
		class SizingSource : Source {
			public SizingSource (DialogViewController controller) : base (controller) {}
			
			public override float GetHeightForRow (UITableView tableView, MonoTouch.Foundation.NSIndexPath indexPath)
			{
				var section = root.Sections [indexPath.Section];
				var element = section.Elements [indexPath.Row];
				
				var sizable = element as IElementSizing;
				if (sizable == null)
					return tableView.RowHeight;
				return sizable.GetHeight (tableView, indexPath);
			}
		}
			
		public void ActivateController (UIViewController controller)
		{
			dirty = true;
			
			var parent = ParentViewController;
			var nav = parent as UINavigationController;
			
			// We can not push a nav controller into a nav controller
			if (nav != null && !(controller is UINavigationController))
				nav.PushViewController (controller, true);
			else
				PresentModalViewController (controller, true);
		}

		public override void LoadView ()
		{
			tableView = new UITableView (UIScreen.MainScreen.Bounds, Style) {
				AutoresizingMask = UIViewAutoresizing.FlexibleHeight | UIViewAutoresizing.FlexibleWidth | UIViewAutoresizing.FlexibleTopMargin,
				AutosizesSubviews = true
			};

			UpdateSource ();
			View = tableView;
			if (root == null)
				return;
			
			root.TableView = tableView;
		}
		
		public override bool ShouldAutorotateToInterfaceOrientation (UIInterfaceOrientation toInterfaceOrientation)
		{	
			return true;
			//return rotateUIEnabled;
		
		}

		public override void ViewWillAppear (bool animated)
		{
			base.ViewWillAppear (animated);
			root.Prepare ();
			
			NavigationItem.HidesBackButton = !pushing;
			if (root.Caption != null)
				NavigationItem.Title = root.Caption;
			if (dirty){
				tableView.ReloadData ();
				dirty = false;
			}
		}

		Source TableSource;
		
		void UpdateSource ()
		{
			if (root == null)
				return;
			
			TableSource = root.UnevenRows ? new SizingSource (this) : new Source (this);
			tableView.Source = TableSource;
		}

		public void ReloadData ()
		{
			root.Prepare ();
			if (tableView != null){
				UpdateSource ();
				tableView.ReloadData ();
			}
			dirty = false;
		}
		
		public event EventHandler ViewDissapearing;
		
		public override void ViewWillDisappear (bool animated)
		{
			base.ViewWillDisappear (animated);
			if (ViewDissapearing != null)
				ViewDissapearing (this, EventArgs.Empty);
		}

		void PrepareRoot (RootElement root)
		{
			this.root = root;
			if (root != null)
				root.Prepare ();
		}
		
		public DialogViewController (RootElement root)
		{
			PrepareRoot (root);
		}
		
		/// <summary>
		///     Creates a new DialogViewController from a RootElement and sets the push status
		/// </summary>
		/// <param name="root">
		/// The <see cref="RootElement"/> containing the information to render.
		/// </param>
		/// <param name="pushing">
		/// A <see cref="System.Boolean"/> describing whether this is being pushed 
		/// (NavigationControllers) or not.   If pushing is true, then the back button 
		/// will be shown, allowing the user to go back to the previous controller
		/// </param>
		public DialogViewController (RootElement root, bool pushing)
		{
			this.pushing = pushing;
			PrepareRoot (root);
		}
		
		public DialogViewController (RootElement root, bool pushing, bool AutoRotateUI)
		{
			this.pushing = pushing;
			this.rotateUIEnabled = AutoRotateUI;
			PrepareRoot(root);
		}
			
	}
	
}
