using Microsoft.Maui.Animations;

namespace KenNetBurns.Maui;

public partial class MainPage : ContentPage
{
	List<string> _images = new()
	{
		//"https://images.unsplash.com/photo-1550684848-fac1c5b4e853",
		//"https://images.unsplash.com/photo-1507525428034-b723cf961d3e",
		//"https://images.unsplash.com/photo-1731617732560-32268c055254",
		//"https://images.unsplash.com/photo-1498612753354-772a30629934",
		//"https://images.unsplash.com/photo-1611149916119-c6c16eb89f89",
		//"https://images.unsplash.com/photo-1526834492092-9d67abca4622",
		//"https://images.unsplash.com/photo-1559827260-dc66d52bef19",
		"https://cdn.bsky.app/img/banner/plain/did:plc:jdc4fbwl6zrgkevkebb64hms/bafkreiadq4xvzgpluymhnh7iti5vheqjfakybeazfkbtf2x7b2qopjjv6q@jpeg",
	};
	public MainPage()
	{
		InitializeComponent();
		kbView.ImageSource = _images[new Random().Next(0, _images.Count)];
		kbView.TransitionGenerator = new RectTransitionGenerator(15000, Interpolators.Linear)
		{
			Scale = 0.62f
		};
		kbView.TransitionStart += (s, e) =>
		{
			MainThread.BeginInvokeOnMainThread(() => StatusLabel.Text = $"Transition started: {e}");
		};
		kbView.TransitionEnd += (s, e) =>
		{
			MainThread.BeginInvokeOnMainThread(() => StatusLabel.Text = $"Transition ended: {e}");
		};

	}

	protected override void OnAppearing()
	{
		base.OnAppearing();
		kbView.StartAnimation();
	}

	private void NewImage_Clicked(object sender, EventArgs e)
	{
		if (string.IsNullOrWhiteSpace(UrlEntry.Text))
		{
			kbView.ImageSource = _images[new Random().Next(0, _images.Count)];
		}
		else
		{
			kbView.ImageSource = UrlEntry.Text;
		}
		kbView.TransitionGenerator = new RandomTransitionGenerator();
		kbView.TransitionGenerator.SetTransitionDuration(Random.Shared.Next(3000, 25000));
		kbView.TransitionGenerator.SetTransitionInterpolator(Interpolators.AccelerateDecelerate);
	}


	bool paused;
	private void ToggleState_Clicked(object sender, EventArgs e)
	{
		if (paused)
		{
			kbView.Resume();
			StateToggleButton.Text = "Pause";
		}
		else
		{
			kbView.Pause();
			StateToggleButton.Text = "Resume";
		}
		paused = !paused;
	}

	private void LoadImage_Clicked(object sender, EventArgs e)
	{
		try
		{
			var url = UrlEntry.Text;
			kbView.ImageSource = url;

		}
		catch (Exception ex)
		{
			StatusLabel.Text = ex.Message;
		}


		}
}

