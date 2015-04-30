using Foundation;
using Microsoft.Xrm.Sdk.Query.Samples;
using Microsoft.Xrm.Sdk.Samples;
using System;
using System.Collections.Generic;
using UIKit;

namespace CrmSetAccountLocationXamarin
{
    public partial class MasterViewController : UITableViewController
    {
        //Requires: https://code.msdn.microsoft.com/Mobile-Development-Helper-3213e2e6

        DataSource _dataSource;
        public static string ClientId = "[GUID_HERE]";
        public static string CommonAuthority = "https://login.windows.net/[GUID_HERE]/oauth2/authorize?api-version=1.0";
        public static Uri ReturnUri = new Uri("http://yourapp-redirect");
        public static string CrmUrl = "https://org.crm.dynamics.com";

        public MasterViewController(IntPtr handle)
            : base(handle)
        {
            Title = NSBundle.MainBundle.LocalizedString("Accounts", "Accounts");

            // Custom initialization
        }

        public override void DidReceiveMemoryWarning()
        {
            // Releases the view if it doesn't have a superview.
            base.DidReceiveMemoryWarning();

            // Release any cached data, images, etc that aren't in use.
        }

        async public override void ViewDidLoad()
        {
            base.ViewDidLoad();

            TableView.Source = _dataSource = new DataSource(this);

            var gotToken = await CrmOauth.GetToken(this);
            if (!gotToken) return;

            Login.Title = "Logout";
            GetAccounts(NSUserDefaults.StandardUserDefaults.StringForKey("AccessToken"));
        }

        class DataSource : UITableViewSource
        {
            static readonly NSString CellIdentifier = new NSString("Cell");
            readonly List<object> _objects = new List<object>();
            readonly MasterViewController _controller;

            public DataSource(MasterViewController controller)
            {
                _controller = controller;
            }

            public IList<object> Objects
            {
                get { return _objects; }
            }

            // Customize the number of sections in the table view.
            public override nint NumberOfSections(UITableView tableView)
            {
                return 1;
            }

            public override nint RowsInSection(UITableView tableview, nint section)
            {
                return _objects.Count;
            }

            // Customize the appearance of table view cells.
            public override UITableViewCell GetCell(UITableView tableView, NSIndexPath indexPath)
            {
                var cell = tableView.DequeueReusableCell(CellIdentifier, indexPath);

                cell.TextLabel.Text = ((Entity)_objects[indexPath.Row]).GetAttributeValue<string>("name");

                return cell;
            }

            public override bool CanEditRow(UITableView tableView, NSIndexPath indexPath)
            {
                // Return false if you do not want the specified item to be editable.
                return true;
            }

            public override void CommitEditingStyle(UITableView tableView, UITableViewCellEditingStyle editingStyle, NSIndexPath indexPath)
            {
                if (editingStyle == UITableViewCellEditingStyle.Delete)
                {
                    // Delete the row from the data source.
                    _objects.RemoveAt(indexPath.Row);
                    _controller.TableView.DeleteRows(new[] { indexPath }, UITableViewRowAnimation.Fade);
                }
                else if (editingStyle == UITableViewCellEditingStyle.Insert)
                {
                    // Create a new instance of the appropriate class, insert it into the array, and add a new row to the table view.
                }
            }
            /*
            // Override to support rearranging the table view.
            public override void MoveRow (UITableView tableView, NSIndexPath sourceIndexPath, NSIndexPath destinationIndexPath)
            {
            }
            */
            /*
            // Override to support conditional rearranging of the table view.
            public override bool CanMoveRow (UITableView tableView, NSIndexPath indexPath)
            {
                // Return false if you do not want the item to be re-orderable.
                return true;
            }
            */
        }

        public override void PrepareForSegue(UIStoryboardSegue segue, NSObject sender)
        {
            if (segue.Identifier != "showDetail") return;

            var indexPath = TableView.IndexPathForSelectedRow;
            var item = _dataSource.Objects[indexPath.Row];

            ((DetailViewController)segue.DestinationViewController).SetDetailItem(item);
        }

        async private void GetAccounts(string accessToken)
        {
            OrganizationDataWebServiceProxy orgService = new OrganizationDataWebServiceProxy
            {
                ServiceUrl = CrmUrl,
                AccessToken = accessToken
            };

            //Get "Sample" Accounts - make this more meaningful & add paging
            QueryExpression query = new QueryExpression
            {
                EntityName = "account",
                ColumnSet = new ColumnSet("name", "address1_latitude", "address1_longitude"),
                TopCount = 10,
                Orders = new DataCollection<OrderExpression>
                    {
                        new OrderExpression
                        {
                            AttributeName = "name",
                            OrderType = OrderType.Descending
                        }
                    },
                Criteria = new FilterExpression
                {
                    Conditions =
                        {
                            new ConditionExpression
                            {
                                EntityName = "account",
                                AttributeName = "name",
                                Operator = ConditionOperator.Like,
                                Values = {"%sample%"}
                            }
                        }
                }
            };

            EntityCollection response = await orgService.RetrieveMultiple(query);

            foreach (Entity account in response.Entities)
            {
                _dataSource.Objects.Insert(0, account);

                using (var indexPath = NSIndexPath.FromRowSection(0, 0))
                    TableView.InsertRows(new[] { indexPath }, UITableViewRowAnimation.Automatic);
            }
        }

        async partial void Login_Activated(UIBarButtonItem sender)
        {
            if (Login.Title == "Logout")
            {
                NSUserDefaults.StandardUserDefaults.RemoveObject("AccessToken");
                NSUserDefaults.StandardUserDefaults.RemoveObject("RefreshToken");
                NSUserDefaults.StandardUserDefaults.RemoveObject("AccessTokenExpirationDate");
                Login.Title = "Login";
                _dataSource.Objects.Clear();
                TableView.Source = _dataSource = new DataSource(this);
                TableView.ReloadData();
                return;
            }

            var gotToken = await CrmOauth.GetToken(this);
            if (!gotToken) return;

            Login.Title = "Logout";
            GetAccounts(NSUserDefaults.StandardUserDefaults.StringForKey("AccessToken"));
        }
    }
}
