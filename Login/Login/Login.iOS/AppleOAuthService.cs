using System;
using System.Net;
using System.Threading.Tasks;
using Foundation;
using Login.iOS;
using Newtonsoft.Json.Linq;
using UIKit;
using Xamarin.Auth;
using Xamarin.Forms;

[assembly: Dependency(typeof(AppleOAuthService))]
namespace Login.iOS
{
    public class AppleOAuthService : NSObject, IOAuthService
    {
        TaskCompletionSource<LoginResult> _completionSource;

        public Task<LoginResult> Login()
        {
            _completionSource = new TaskCompletionSource<LoginResult>();
            var auth = new OAuth2Authenticator
            (
                clientId: "8a509979-094e-4f72-9007-9af5e7bd6d88",
                scope: "wl.basic, wl.emails, wl.photos",
                authorizeUrl: new Uri("https://login.live.com/oauth20_authorize.srf"),
                redirectUrl: new Uri("https://login.live.com/oauth20_desktop.srf"),
                clientSecret: null,
                accessTokenUrl: new Uri("https://login.live.com/oauth20_token.srf")
            )
            {
                AllowCancel = true
            };

            auth.Completed += AuthOnCompleted;
            auth.ClearCookiesBeforeLogin = true;
            auth.Title = "Microsoft";

            UIApplication.SharedApplication.KeyWindow.RootViewController.PresentViewController(auth.GetUI(), true, null);
            
            return _completionSource.Task;
        }

        async void AuthOnCompleted(object sender, AuthenticatorCompletedEventArgs authCompletedArgs)
        {
			UIApplication.SharedApplication.KeyWindow.RootViewController.DismissViewController (true, null);
            
			if (!authCompletedArgs.IsAuthenticated || authCompletedArgs.Account == null)
                SetResult(new LoginResult { LoginState = LoginState.Canceled });
            else
            {
                var token = authCompletedArgs.Account.Properties.ContainsKey("access_token")
                    ? authCompletedArgs.Account.Properties["access_token"]
                    : null;
                var expInString = authCompletedArgs.Account.Properties.ContainsKey("expires_in")
                    ? authCompletedArgs.Account.Properties["expires_in"]
                    : null;
                
                var expireIn = Convert.ToInt32(expInString);
                var expireAt = DateTimeOffset.Now.AddSeconds(expireIn);
                
                await GetUserProfile(authCompletedArgs.Account, token, expireAt);
            }
        }

        public void Logout()
        {
            _completionSource = null;
        }

        void SetResult(LoginResult result)
        {
            _completionSource?.TrySetResult(result);
            _completionSource = null;
        }

        async Task GetUserProfile(Account account, string token, DateTimeOffset expireAt)
        {
            var result = new LoginResult
            {
                Token = token,
                ExpireAt = expireAt
            };

            var request = new OAuth2Request("GET", new Uri("https://apis.live.net/v5.0/me"),
                null, account);
            var response = await request.GetResponseAsync();
            if (response != null && response.StatusCode == HttpStatusCode.OK)
            {
                var userJson = response.GetResponseText();
                var jobject = JObject.Parse(userJson);
                result.LoginState = LoginState.Success;
                result.Email = jobject["emails"]?["preferred"].ToString();
                result.FirstName = jobject["first_name"]?.ToString();
                result.LastName = jobject["last_name"]?.ToString();
                result.ImageUrl = jobject["picture"]?["data"]?["url"]?.ToString();
                var userId = jobject["id"]?.ToString();
                result.UserId = userId;
                result.ImageUrl = $"https://apis.live.net/v5.0/{userId}/picture";
            }
            else
            {
                result.LoginState = LoginState.Failed;
                result.ErrorString = $"Error: Responce={response}, StatusCode = {response?.StatusCode}";
            }

            SetResult(result);
        }
    }
}
