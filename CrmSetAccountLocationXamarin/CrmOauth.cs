using Foundation;
using Microsoft.IdentityModel.Clients.ActiveDirectory;
using System;
using System.Linq;
using System.Threading.Tasks;
using UIKit;

namespace CrmAccountEnrichmentXamarin
{
    public class CrmOauth
    {
        private static AuthenticationResult _authResult;

        async public static Task<bool> GetToken(UIViewController controller)
        {
            NSString tokenExpirationKey = new NSString("AccessTokenExpirationDate");

            //Check if token data is saved
            if (!string.IsNullOrEmpty(NSUserDefaults.StandardUserDefaults.StringForKey("AccessToken")) &&
                NSUserDefaults.StandardUserDefaults.ValueForKey(tokenExpirationKey) != null &&
                !string.IsNullOrEmpty(NSUserDefaults.StandardUserDefaults.StringForKey("RefreshToken")))
            {
                NSDate nsdate = (NSDate)NSUserDefaults.StandardUserDefaults.ValueForKey(tokenExpirationKey);
                DateTime d = new DateTime(2001, 1, 1, 0, 0, 0).AddSeconds(nsdate.SecondsSinceReferenceDate);
                DateTime tokenExpireDate = DateTime.SpecifyKind(d, DateTimeKind.Unspecified);
                DateTime currentDate = DateTime.Now.ToUniversalTime();

                //Check if Access Token is expired
                if (currentDate > tokenExpireDate)
                {
                    try
                    {
                        //Access Token is expired use Refresh Token to renew
                        var authContext = new AuthenticationContext(MasterViewController.CommonAuthority);
                        if (authContext.TokenCache.ReadItems().Any())
                            authContext = new AuthenticationContext(authContext.TokenCache.ReadItems().First().Authority);

                        _authResult =
                            await
                                authContext.AcquireTokenByRefreshTokenAsync(
                                    NSUserDefaults.StandardUserDefaults.StringForKey("RefreshToken"),
                                    MasterViewController.ClientId);

                        SaveTokens();
                        return true;
                    }
                    catch (Exception)
                    {
                        //Refresh failed - go to standard login
                    }
                }
                else //Access Token is still valid
                    return true;
            }

            //Standard login prompt
            try
            {
                var authContext = new AuthenticationContext(MasterViewController.CommonAuthority);
                if (authContext.TokenCache.ReadItems().Any())
                    authContext = new AuthenticationContext(authContext.TokenCache.ReadItems().First().Authority);
                _authResult =
                    await
                        authContext.AcquireTokenAsync(MasterViewController.CrmUrl, MasterViewController.ClientId, 
                        MasterViewController.ReturnUri, new AuthorizationParameters(controller));

                SaveTokens();
                return true;
            }
            catch (AdalServiceException ex)
            {
                if (ex.StatusCode == 0)
                {
                    //Access Denied and/or hit Cancel button on login screen                  
                }
                return false;
            }
        }

        private static void SaveTokens()
        {
            NSUserDefaults.StandardUserDefaults.SetString(_authResult.AccessToken, "AccessToken");
            NSDate tokenExpiration = (NSDate)_authResult.ExpiresOn.UtcDateTime;
            NSString tokenExpirationKey = new NSString("AccessTokenExpirationDate");
            NSUserDefaults.StandardUserDefaults.SetValueForKey(tokenExpiration, tokenExpirationKey);
            NSUserDefaults.StandardUserDefaults.SetString(_authResult.RefreshToken, "RefreshToken");
        }
    }
}