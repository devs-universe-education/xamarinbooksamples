using System.Threading.Tasks;
using Facebook.CoreKit;
using Facebook.LoginKit;
using Foundation;
using Login.iOS;
using Xamarin.Forms;
using Xamarin.Forms.Platform.iOS;

[assembly: Dependency(typeof(AppleFacebookService))]
namespace Login.iOS
{
    public class AppleFacebookService: IFacebookService
    {
        readonly LoginManager _loginManager = new LoginManager();
        readonly string[] _permissions = { @"public_profile", @"email", @"user_about_me" };

        LoginResult _loginResult;
        TaskCompletionSource<LoginResult> _completionSource;

        public Task<LoginResult> Login()
        {
            _completionSource = new TaskCompletionSource<LoginResult>();
            _loginManager.LogInWithReadPermissions(_permissions, Utils.GetCurrentViewController(), LoginManagerLoginHandler);
            return _completionSource.Task;
        }

        public void Logout()
        {
            _loginResult = null;
            _completionSource = null;
            _loginManager.LogOut();
        }

        void LoginManagerLoginHandler(LoginManagerLoginResult result, NSError error)
        {
            if (result.IsCancelled)
                _completionSource?.TrySetResult(new LoginResult {LoginState = LoginState.Canceled});
            else if (error != null)
                _completionSource?.TrySetResult(new LoginResult { LoginState = LoginState.Failed, ErrorString = error.LocalizedDescription });
            else
            {
                _loginResult = new LoginResult
                {
                    Token = result.Token.TokenString,
                    UserId = result.Token.UserID,
                    ExpireAt = result.Token.ExpirationDate.ToDateTime()
                };

                var request = new GraphRequest(@"me", new NSDictionary(@"fields", @"email, first_name, last_name, picture.width(1000).height(1000)"));
                request.Start(GetEmailRequestHandler);
            }
        }

        void GetEmailRequestHandler(GraphRequestConnection connection, NSObject result, NSError error)
        {
            if (error != null || _loginResult == null)
                _completionSource?.TrySetResult(new LoginResult { LoginState = LoginState.Failed, ErrorString = _loginResult == null ? "Invalid login sequence": error?.LocalizedDescription });
            else
            {
                var dict = result as NSDictionary;

                if (dict != null)
                {
                    var key = new NSString(@"email");
                    if (dict.ContainsKey(key))
                        _loginResult.Email = dict[key]?.ToString();

                    key = new NSString(@"first_name");
                    if (dict.ContainsKey(key))
                        _loginResult.FirstName = dict[key]?.ToString();

                    key = new NSString(@"last_name");
                    if (dict.ContainsKey(key))
                        _loginResult.LastName = dict[key]?.ToString();


                    key = new NSString(@"picture");
                    if (dict.ContainsKey(key))
                        _loginResult.ImageUrl = dict[key]?.ValueForKeyPath(new NSString("data"))?.ValueForKey(new NSString("url"))?.ToString();
                } 

                _loginResult.LoginState = LoginState.Success;
                _completionSource?.TrySetResult(_loginResult);
            }
        }
    }
}