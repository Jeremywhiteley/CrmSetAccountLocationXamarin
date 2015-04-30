// WARNING
//
// This file has been generated automatically by Xamarin Studio from the outlets and
// actions declared in your storyboard file.
// Manual changes to this file will not be maintained.
//
using Foundation;
using System;
using System.CodeDom.Compiler;
using UIKit;

namespace CrmAccountEnrichmentXamarin
{
	[Register ("DetailViewController")]
	partial class DetailViewController
	{
		[Outlet]
		[GeneratedCode ("iOS Designer", "1.0")]
		UILabel AccountLatitude { get; set; }

		[Outlet]
		[GeneratedCode ("iOS Designer", "1.0")]
		UILabel AccountLongitude { get; set; }

		[Outlet]
		[GeneratedCode ("iOS Designer", "1.0")]
		UILabel AccountName { get; set; }

		[Outlet]
		[GeneratedCode ("iOS Designer", "1.0")]
		UILabel CurrentLatitude { get; set; }

		[Outlet]
		[GeneratedCode ("iOS Designer", "1.0")]
		UILabel CurrentLongitude { get; set; }

		[Outlet]
		[GeneratedCode ("iOS Designer", "1.0")]
		MapKit.MKMapView mapView { get; set; }

		[Action ("SetLatLong:")]
		[GeneratedCode ("iOS Designer", "1.0")]
		partial void SetLatLong (UIBarButtonItem sender);

		void ReleaseDesignerOutlets ()
		{
			if (AccountLatitude != null) {
				AccountLatitude.Dispose ();
				AccountLatitude = null;
			}
			if (AccountLongitude != null) {
				AccountLongitude.Dispose ();
				AccountLongitude = null;
			}
			if (AccountName != null) {
				AccountName.Dispose ();
				AccountName = null;
			}
			if (CurrentLatitude != null) {
				CurrentLatitude.Dispose ();
				CurrentLatitude = null;
			}
			if (CurrentLongitude != null) {
				CurrentLongitude.Dispose ();
				CurrentLongitude = null;
			}
			if (mapView != null) {
				mapView.Dispose ();
				mapView = null;
			}
		}
	}
}
