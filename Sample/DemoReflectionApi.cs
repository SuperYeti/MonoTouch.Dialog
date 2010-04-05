//
// Sample showing the core Element-based API to create a dialog
//
using System;
using System.Collections.Generic;
using MonoTouch.Dialog;
using MonoTouch.UIKit;
using MonoTouch.Foundation;

namespace Sample
{
	// Use the preserve attribute to inform the linker that even if I do not
	// use the fields, to not try to optimize them away.
	
	[Preserve (AllMembers=true)]
	class Settings {
	[Section]
		public bool AccountEnabled;
		[Skip] public bool Hidden;
				
	[Section ("Account", "Your credentials")]
		
		[Entry ("Enter your login name")]
		public string Login;
		
		[Password ("Enter your password")]
		public string Password;
		
	[Section ("Time Editing")]
		
		public TimeSettings TimeSamples;
		
	[Section ("Enumerations")]
		
		[Caption ("Favorite CLR type")]
		public TypeCode FavoriteType;
		
	[Section ("Checkboxes")]
		[Checkbox]
		bool English = true;
		
		[Checkbox]
		bool Spanish;
		
	[Section ("Image Selection")]
		public UIImage Top;
		public UIImage Middle;
		public UIImage Bottom;
		
	[Section ("Multiline")]
		[Caption ("This is a\nmultiline string\nall you need is the\n[Multiline] attribute")]
		[Multiline]
		public string multi;
		
	[Section ("IEnumerable")]
		[RadioSelection ("ListOfString")] 
		public int selected = 1;
		public IList<string> ListOfString;
		
	[Section ("Alignment")]
		[Alignment("Left")]
		public string LeftAlign;
		
		[Alignment("Center")]
		public string CenterAlign;
		
		[Alignment("Right")]
		public string RightAlign;
		
	}

	public class TimeSettings {
		public DateTime Appointment;
		
		[Date]
		public DateTime Birthday;
		
		[Time]
		public DateTime Alarm;
	}
	
	public partial class AppDelegate 
	{
		Settings settings;
		
		public void DemoReflectionApi ()
		{	
			if (settings == null){
				var image = UIImage.FromFile ("monodevelop-32.png");
				
				settings = new Settings () {
					AccountEnabled = true,
					Login = "postmater@localhost.com",
					TimeSamples = new TimeSettings () {
						Appointment = DateTime.Now,
						Birthday = new DateTime (1980, 6, 24),
						Alarm = new DateTime (2000, 1, 1, 7, 30, 0, 0)
					},
					FavoriteType = TypeCode.Int32,
					Top = image,
					Middle = image,
					Bottom = image,
					ListOfString = new List<string> () { "One", "Two", "Three" }
				};
			}
			var bc = new BindingContext (null, settings, "Settings");
			
			var dv = new DialogViewController (bc.Root, true);
			
			// When the view goes out of screen, we fetch the data.
			dv.ViewDissapearing += delegate {
				// This reflects the data back to the object instance
				bc.Fetch ();
				
				// Manly way of dumping the data.
				Console.WriteLine ("Current status:");
				Console.WriteLine (
				    "AccountEnabled:  {0}\n" +
				    "Login:           {1}\n" +
				    "Password:        {2}\n" +
				    "Appointment:     {3}\n" +
				    "Birthday:        {4}\n" +
				    "Alarm:           {5}\n" +
				    "Favorite Type:   {6}\n" + 
				    "IEnumerable idx: {7}", 
				    settings.AccountEnabled, settings.Login, settings.Password, 
				    settings.TimeSamples.Appointment, settings.TimeSamples.Birthday, 
				    settings.TimeSamples.Alarm, settings.FavoriteType,
				    settings.selected);
			};
			navigation.PushViewController (dv, true);	
		}
	}
}