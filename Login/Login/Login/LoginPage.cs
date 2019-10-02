using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Xamarin.Forms;

namespace Login
{
    public class LoginPage : ContentPage
    {
        readonly Label _hintLabel;
        readonly List<Button> _loginButtons = new List<Button>();
        bool _isAuthenticated;

        public LoginPage()
        {
            Title = "Binwell Login";

            _hintLabel = new Label
            {
                HorizontalTextAlignment = TextAlignment.Center,
                Text = "Unauthenticated"
            };

            var stackLayout = new StackLayout
            {
                VerticalOptions = LayoutOptions.Center,
                Children = { _hintLabel }
            };

            var providers = new[] { "Facebook", "VK", "Microsoft"};
            foreach (var provider in providers)
            {
                var loginButton = new Button
                {
                    HorizontalOptions = LayoutOptions.Center,
                    Text = $"Login {provider}",
                    AutomationId = provider
                };

                loginButton.Clicked += LoginButtonOnClicked;

                _loginButtons.Add(loginButton);
                stackLayout.Children.Add(loginButton);
            }

            Content = stackLayout;
        }

        async void LoginButtonOnClicked(object sender, EventArgs e)
        {
            if (_isAuthenticated)
            {
                _hintLabel.Text = "Unauthenticated";

                var senderBtn = sender as Button;
                if (senderBtn == null) return;

                Logout(senderBtn.AutomationId);

                _isAuthenticated = false;
                foreach (var btn in _loginButtons)
                {
                    btn.IsEnabled = true;
                    btn.Text = $"Login {btn.AutomationId}";
                }
            }
            else
            {
                var senderBtn = sender as Button;
                if (senderBtn == null) return;
                
                _hintLabel.Text = "Login. Please wait";
                var loginResult = await Login(senderBtn.AutomationId);

                foreach (var btn in _loginButtons.Where(b => b != senderBtn))
                    btn.IsEnabled = false;

                switch (loginResult.LoginState)
                {
                    case LoginState.Canceled:
                        _hintLabel.Text = "Canceled";
                        foreach (var btn in _loginButtons.Where(b => b != senderBtn))
                            btn.IsEnabled = true;
                        break;
                    case LoginState.Success:
                        _hintLabel.Text = $"Hi {loginResult.FirstName}! Your email is {loginResult.Email}";
                        senderBtn.Text = $"Logout {senderBtn.AutomationId}";
                        
                        _isAuthenticated = true;
                        break;
                    default:
                        _hintLabel.Text = "Failed: " + loginResult.ErrorString;
                        foreach(var btn in _loginButtons.Where(b => b != senderBtn))
                            btn.IsEnabled = true;
                        break;
                }
            }
        }

        Task<LoginResult> Login(string providerName)
        {
            switch (providerName.ToLower())
            {
                case "vk":
                    return DependencyService.Get<IVkService>().Login();
                case "facebook":
                    return DependencyService.Get<IFacebookService>().Login();
                default:
                    return DependencyService.Get<IOAuthService>().Login();
            }
        }

        void Logout(string providerName)
        {
            switch (providerName.ToLower())
            {
                case "vk":
                    DependencyService.Get<IVkService>().Logout();
                    return;
                case "facebook":
                    DependencyService.Get<IFacebookService>().Logout();
                    return;
                default:
                    DependencyService.Get<IOAuthService>().Logout();
                    return;
            }
        }
    }
}