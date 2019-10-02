using System.Threading.Tasks;
using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Login.Droid;
using Org.Json;
using Xamarin.Facebook;
using Xamarin.Facebook.Login;
using Xamarin.Forms;

[assembly: Dependency(typeof(AndroidFacebookService))]
namespace Login.Droid
{
    public class AndroidFacebookService: Java.Lang.Object, IFacebookService, GraphRequest.IGraphJSONObjectCallback, GraphRequest.ICallback, IFacebookCallback
    {
        public static AndroidFacebookService Instance => DependencyService.Get<IFacebookService>() as AndroidFacebookService;

        readonly ICallbackManager _callbackManager = CallbackManagerFactory.Create();
        readonly string[] _permissions = { @"public_profile", @"email", @"user_about_me" };

        LoginResult _loginResult;
        TaskCompletionSource<LoginResult> _completionSource;

        public AndroidFacebookService()
        {
            LoginManager.Instance.RegisterCallback(_callbackManager, this);
        }

        public Task<LoginResult> Login()
        {
            _completionSource = new TaskCompletionSource<LoginResult>();
            LoginManager.Instance.LogInWithReadPermissions(Forms.Context as Activity, _permissions);
            return _completionSource.Task;
        }

        public void Logout()
        {
            LoginManager.Instance.LogOut();
        }

        public void OnActivityResult(int requestCode, int resultCode, Intent data)
        {
            _callbackManager?.OnActivityResult(requestCode, resultCode, data);
        }

        public void OnCompleted(JSONObject data, GraphResponse response)
        {
            OnCompleted(response);
        }

        public void OnCompleted(GraphResponse response)
        {
            if (response?.JSONObject == null)
                _completionSource?.TrySetResult(new LoginResult {LoginState = LoginState.Canceled});
            else
            {
                _loginResult = new LoginResult
                {
                    FirstName = Profile.CurrentProfile.FirstName,
                    LastName = Profile.CurrentProfile.LastName,
                    Email = response.JSONObject.Has("email") ? response.JSONObject.GetString("email") : string.Empty,
                    ImageUrl = response.JSONObject.GetJSONObject("picture")?.GetJSONObject("data")?.GetString("url"),
                    Token = AccessToken.CurrentAccessToken.Token,
                    UserId = AccessToken.CurrentAccessToken.UserId,
                    ExpireAt = Utils.FromMsDateTime(AccessToken.CurrentAccessToken?.Expires?.Time),
                    LoginState = LoginState.Success
                };

                _completionSource?.TrySetResult(_loginResult);
            }
        }

        public void OnCancel()
        {
            _completionSource?.TrySetResult(new LoginResult { LoginState = LoginState.Canceled });
        }

        public void OnError(FacebookException exception)
        {
            _completionSource?.TrySetResult(new LoginResult
            {
                LoginState = LoginState.Failed,
                ErrorString = exception?.Message
            });
        }

        public void OnSuccess(Java.Lang.Object result)
        {
            var facebookLoginResult = result.JavaCast<Xamarin.Facebook.Login.LoginResult>();
            if (facebookLoginResult == null) return;

            var parameters = new Bundle();
            parameters.PutString("fields", "id,email,picture.width(1000).height(1000)");
            var request = GraphRequest.NewMeRequest(facebookLoginResult.AccessToken, this);
            request.Parameters = parameters;
            request.ExecuteAsync();
        }
    }
}