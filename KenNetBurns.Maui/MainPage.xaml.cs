using SkiaSharp;

namespace KenNetBurns.Maui;

public partial class MainPage : ContentPage
{
	List<string> _images = new()
	{
		"https://images.unsplash.com/photo-1550684848-fac1c5b4e853",
		"https://images.unsplash.com/photo-1507525428034-b723cf961d3e",
		"https://images.unsplash.com/photo-1731617732560-32268c055254",
		"https://images.unsplash.com/photo-1498612753354-772a30629934",
		"https://images.unsplash.com/photo-1611149916119-c6c16eb89f89",
		"https://images.unsplash.com/photo-1526834492092-9d67abca4622",
		"https://images.unsplash.com/photo-1559827260-dc66d52bef19",
		"https://cdn.bsky.app/img/banner/plain/did:plc:jdc4fbwl6zrgkevkebb64hms/bafkreiadq4xvzgpluymhnh7iti5vheqjfakybeazfkbtf2x7b2qopjjv6q@jpeg",
	};
	public MainPage()
	{
		InitializeComponent();

	}

	protected override void OnAppearing()
	{
		base.OnAppearing();
		kbView.Mode = AnimationMode.ReverseAndLoop;
		kbView.StartAnimation();

	}

	private async void NewImage_Clicked(object sender, EventArgs e)
	{
		if (string.IsNullOrWhiteSpace(UrlEntry.Text))
		{
			var random = new Random();
			var randomIndex = random.Next(_images.Count);
			var randomImageUrl = _images[randomIndex];
			var stream = await GetImageStream(randomImageUrl);
			kbView.LoadImage(stream);
			UrlEntry.Text = randomImageUrl;
		}
		else
		{
			var stream = await GetImageStream(UrlEntry.Text);
			kbView.LoadImage(stream);
		}

		var keyframes = GetRandomKeyframes();



		kbView.SetKeyframes(keyframes);
		kbView.StartAnimation();
	}

	//get random keyframes
	private List<KBKeyframe> GetRandomKeyframes()
	{
		var random = new Random();
		var keyframes = new List<KBKeyframe>();
		for (int i = 0; i < random.Next(0, 6); i++)
		{
			keyframes.Add(new KBKeyframe
			{
				Scale = (float)(random.NextDouble() * 2),
				Position = new SKPoint((float)random.NextDouble(), (float)random.NextDouble()),
				Time = (float)(random.NextDouble())
			});
		}
		return keyframes;
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



		}
		catch (Exception ex)
		{
			StatusLabel.Text = ex.Message;
		}
	}

	//get image stream from url
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
}