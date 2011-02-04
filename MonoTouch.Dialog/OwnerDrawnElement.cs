using System;
using System.Drawing;
using MonoTouch.UIKit;
using MonoTouch.CoreFoundation;
using MonoTouch.CoreGraphics;
using MonoTouch.Foundation;

namespace MonoTouch.Dialog
{
	public abstract class OwnerDrawnElement : Element, IElementSizing
	{		
		public string CellReuseIdentifier
		{
			get;set;	
		}
		
		public UITableViewCellStyle Style
		{
			get;set;	
		}
		
		public OwnerDrawnElement (UITableViewCellStyle style, string cellIdentifier) : base(null)
		{
			this.CellReuseIdentifier = cellIdentifier;
			this.Style = style;
		}
		
		public float GetHeight (UITableView tableView, NSIndexPath indexPath)
		{
			return Height(tableView.Bounds);
		}
		
		public override UITableViewCell GetCell (DialogViewController dvc,UITableView tv)
		{
			OwnerDrawnCell cell = tv.DequeueReusableCell(this.CellReuseIdentifier) as OwnerDrawnCell;
			
			if (cell == null)
			{
				cell = new OwnerDrawnCell(this, this.Style, this.CellReuseIdentifier);
				cell.Update();
			}

			return cell;
		}	
		
		public abstract void Draw(RectangleF bounds, CGContext context, UIView view);
		
		public abstract float Height(RectangleF bounds);
		
		class OwnerDrawnCell : UITableViewCell
		{
			OwnerDrawnCellView view;
			
			public OwnerDrawnCell(OwnerDrawnElement element, UITableViewCellStyle style, string cellReuseIdentifier) : base(style, cellReuseIdentifier)
			{
				view = new OwnerDrawnCellView(element);
				ContentView.Add(view);
			}

			public void Update()
			{
				SetNeedsDisplay();
			}		
	
			public override void LayoutSubviews ()
			{
				base.LayoutSubviews ();
				
				view.Frame = ContentView.Bounds;
			}
		}
		
		class OwnerDrawnCellView : UIView
		{				
			OwnerDrawnElement element;
			
			public OwnerDrawnCellView(OwnerDrawnElement element)
			{
				this.element = element;	
			}
			
			public void Update()
			{
				SetNeedsDisplay();
			
			}
			
			public override void Draw (RectangleF rect)
			{
				CGContext context = UIGraphics.GetCurrentContext();
				element.Draw(rect, context, this);
			}
		}
	}
}

