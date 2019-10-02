using System;
using System.Threading.Tasks;
using MobileTemplate.BL.ViewModels;
using Xamarin.Forms;

namespace MobileTemplate.UI.Pages
{
	public class BasePage : ContentPage, IDisposable {
		protected BaseViewModel ViewModel => BindingContext as BaseViewModel;

		~BasePage() {
			Dispose();
		}

		public void Dispose() {
			ViewModel?.Dispose();
		}

		public void SetViewModel(BaseViewModel viewModel) {
			BindingContext = viewModel;
		}

		protected override void OnAppearing() {
			base.OnAppearing();
			Task.Run(async () => {
				await Task.Delay(50); // Allow UI to handle events loop
				if (ViewModel != null)
					await ViewModel.OnPageAppearing();
			});
		}

		protected override void OnDisappearing() {
			base.OnDisappearing();
			Task.Run(async () => {
				await Task.Delay(50); // Allow UI to handle events loop
				if (ViewModel != null)
					await ViewModel.OnPageDissapearing();
			});
		}
	}
}
