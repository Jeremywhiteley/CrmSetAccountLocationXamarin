using CoreLocation;
using Foundation;
using MapKit;
using Microsoft.Xrm.Sdk.Samples;
using System;
using System.Globalization;
using UIKit;

namespace CrmAccountEnrichmentXamarin
{
    public partial class DetailViewController : UIViewController
    {
        object _detailItem;
        CLLocationManager _iPhoneLocationManager;

        public DetailViewController(IntPtr handle)
            : base(handle)
        {
        }

        public void SetDetailItem(object newDetailItem)
        {
            if (_detailItem != newDetailItem)
            {
                _detailItem = newDetailItem;

                // Update the view
                ConfigureView();
            }
        }

        void ConfigureView()
        {
            // Update the user interface for the detail item
            if (IsViewLoaded && _detailItem != null)
            {
                if (((Entity)_detailItem).Contains("name"))
                    AccountName.Text = ((Entity)_detailItem).GetAttributeValue<string>("name");
                if (((Entity)_detailItem).Contains("address1_latitude"))
                    AccountLatitude.Text = ((Entity)_detailItem).GetAttributeValue<double>("address1_latitude").ToString();
                if (((Entity)_detailItem).Contains("address1_longitude"))
                    AccountLongitude.Text = ((Entity)_detailItem).GetAttributeValue<double>("address1_longitude").ToString();
            }
        }

        public override void DidReceiveMemoryWarning()
        {
            // Releases the view if it doesn't have a superview.
            base.DidReceiveMemoryWarning();

            // Release any cached data, images, etc that aren't in use.
        }

        public override void ViewDidLoad()
        {
            base.ViewDidLoad();

            // Perform any additional setup after loading the view, typically from a nib.
            ConfigureView();

            _iPhoneLocationManager = new CLLocationManager();
            if (UIDevice.CurrentDevice.CheckSystemVersion(8, 0))
            {
                _iPhoneLocationManager.RequestWhenInUseAuthorization();
            }

            mapView.ShowsUserLocation = true;

            if (mapView.UserLocationVisible)
            {
                UpdateUiCoords();
            }

            _iPhoneLocationManager.DesiredAccuracy = 1000; // 1000 meters/1 kilometer

            mapView.DidUpdateUserLocation += (sender, e) =>
            {
                if (mapView.UserLocation != null)
                {
                    CLLocationCoordinate2D coords = mapView.UserLocation.Coordinate;
                    MKCoordinateSpan span = new MKCoordinateSpan(MilesToLatitudeDegrees(2), MilesToLongitudeDegrees(2, coords.Latitude));
                    mapView.Region = new MKCoordinateRegion(coords, span);
                    UpdateUiCoords();
                }
            };
        }

        private void UpdateUiCoords()
        {
            CurrentLatitude.Text = _iPhoneLocationManager.Location.Coordinate.Latitude.ToString(CultureInfo.InvariantCulture);
            CurrentLongitude.Text = _iPhoneLocationManager.Location.Coordinate.Longitude.ToString(CultureInfo.InvariantCulture);
            var mkPointAnnotation = new MKPointAnnotation
            {
                Title = "You Are Here",
            };
            mkPointAnnotation.SetCoordinate(new CLLocationCoordinate2D(_iPhoneLocationManager.Location.Coordinate.Latitude, _iPhoneLocationManager.Location.Coordinate.Longitude));
            mapView.AddAnnotation(mkPointAnnotation);
        }

        async partial void SetLatLong(UIBarButtonItem sender)
        {
            OrganizationDataWebServiceProxy orgService = new OrganizationDataWebServiceProxy
            {
                ServiceUrl = MasterViewController.CrmUrl,
                AccessToken = NSUserDefaults.StandardUserDefaults.StringForKey("AccessToken")
            };

            Entity account = (Entity)_detailItem;
            account["address1_latitude"] = _iPhoneLocationManager.Location.Coordinate.Latitude;
            account["address1_longitude"] = _iPhoneLocationManager.Location.Coordinate.Longitude;

            AccountLatitude.Text = _iPhoneLocationManager.Location.Coordinate.Latitude.ToString(CultureInfo.InvariantCulture);
            AccountLongitude.Text = _iPhoneLocationManager.Location.Coordinate.Longitude.ToString(CultureInfo.InvariantCulture);

            await orgService.Update(account);
        }

        /// <summary>
        /// Converts miles to latitude degrees
        /// </summary>
        public double MilesToLatitudeDegrees(double miles)
        {
            double earthRadius = 3960.0;
            double radiansToDegrees = 180.0 / Math.PI;
            return (miles / earthRadius) * radiansToDegrees;
        }

        /// <summary>
        /// Converts miles to longitudinal degrees at a specified latitude
        /// </summary>
        public double MilesToLongitudeDegrees(double miles, double atLatitude)
        {
            double earthRadius = 3960.0;
            double degreesToRadians = Math.PI / 180.0;
            double radiansToDegrees = 180.0 / Math.PI;

            // derive the earth's radius at that point in latitude
            double radiusAtLatitude = earthRadius * Math.Cos(atLatitude * degreesToRadians);
            return (miles / radiusAtLatitude) * radiansToDegrees;
        }
    }
}
