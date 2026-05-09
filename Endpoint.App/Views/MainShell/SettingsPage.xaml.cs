using Endpoint.App.ViewModels.Settings;

namespace Endpoint.App.Views.MainShell;

public partial class SettingsPage : ContentPage
{
	public SettingsPage(SettingViewModel settingViewModel)
	{
		InitializeComponent();
		BindingContext=settingViewModel;
	}
}