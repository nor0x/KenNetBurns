using SkiaSharp;
using System.Text;

namespace Kenimation.Maui;

public partial class MainPage : ContentPage
{
	List<string> _images = new()
	{
		"https://images.unsplash.com/photo-1744380623181-a675718f120c",
		"https://images.unsplash.com/photo-1507525428034-b723cf961d3e",
		"https://images.unsplash.com/photo-1731617732560-32268c055254",
		"https://images.unsplash.com/photo-1744479357124-ef43ab9d6a9f",
		"https://images.unsplash.com/photo-1611149916119-c6c16eb89f89",
		"https://images.unsplash.com/photo-1526834492092-9d67abca4622",
		"https://images.unsplash.com/photo-1559827260-dc66d52bef19",
		"https://images.unsplash.com/photo-1745428911615-eb9b017a1a8f",
		"https://images.unsplash.com/photo-1743885143645-b28cdaacf8b5",
		"https://images.unsplash.com/photo-1745044935151-961c1209f561",
		"https://images.unsplash.com/photo-1462759353907-b2ea5ebd72e7"
	};
	bool _paused;
	int _duration = 5000;

	public MainPage()
	{
		InitializeComponent();
	}

	protected override void OnDisappearing()
	{
		base.OnDisappearing();
		kbView.Dispose();
	}

	private async void NewImage_Clicked(object sender, EventArgs e)
	{
		kbView.Mode = AnimationMode.ReverseAndLoop;

		var randomIndex = Random.Shared.Next(_images.Count);
		var randomImageUrl = _images[randomIndex];
		var stream = await GetImageStream(randomImageUrl);
		kbView.LoadImage(stream);

		var randomKeyframes = KBView.GetRandomSmoothKeyframes(Random.Shared.Next(2, 20));

		StatusLabel.Text = $"Random Keyframes:\n{string.Join("\n", randomKeyframes.Select(k => k.ToString()))}";
		kbView.AnimationDuration = 20000;
		kbView.SetKeyframes(randomKeyframes);
		kbView.StartAnimation();
	}

	private void ToggleState_Clicked(object sender, EventArgs e)
	{
		if (_paused)
		{
			kbView.Resume();
			StateToggleButton.Text = "Pause";
		}
		else
		{
			kbView.Pause();
			StateToggleButton.Text = "Resume";
		}
		_paused = !_paused;
	}

	private async void LoadImage_Clicked(object sender, EventArgs e)
	{
		try
		{
			var url = UrlEntry.Text;
			if (string.IsNullOrWhiteSpace(url))
			{
				StatusLabel.Text = "Please enter a valid URL.";
				return;
			}
			var stream = await GetImageStream(url);
			if (stream == null)
			{
				StatusLabel.Text = "Failed to load image.";
				return;
			}
			kbView.LoadImage(stream);

			var randomKeyframes = KBView.GetRandomSmoothKeyframes(Random.Shared.Next(2, 20));

			StatusLabel.Text = $"Random Keyframes:\n{string.Join("\n", randomKeyframes.Select(k => k.ToString()))}";
			kbView.AnimationDuration = 20000;
			kbView.SetKeyframes(randomKeyframes);
			kbView.StartAnimation();
		}
		catch (Exception ex)
		{
			StatusLabel.Text = ex.Message;
		}
	}

	private async Task<Stream> GetImageStream(string url)
	{
		try
		{
			var httpClient = new HttpClient();
			var response = await httpClient.GetAsync(url);
			if (response.IsSuccessStatusCode)
			{
				return await response.Content.ReadAsStreamAsync();
			}
			else
			{
				throw new Exception($"Failed to load image from {url}. Status code: {response.StatusCode}");
			}
		}
		catch (Exception ex)
		{
			StatusLabel.Text = ex.Message;
			return null;
		}
	}
	private void DurationSlider_ValueChanged(object sender, ValueChangedEventArgs e)
	{
		if (e.NewValue != _duration)
		{
			_duration = (int)e.NewValue;
			kbView.AnimationDuration = _duration;
			DurationLabel.Text = $"Duration: {e.NewValue} ms";
		}
	}
}