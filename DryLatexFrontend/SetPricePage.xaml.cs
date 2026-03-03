using System.Text.Json;
using System.Text;

namespace DryLatexApp;

public partial class SetPricePage : ContentPage
{
	public SetPricePage()
	{
		InitializeComponent();
	}

    private async void SavePriceBtn_Clicked(object sender, EventArgs e)
    {
        var priceRequest = new PriceRequest
        {
            Category = TypePicker.SelectedItem?.ToString(),
            Color = ColorPicker.SelectedItem?.ToString(),
            Price = decimal.Parse(PriceInput.Text)
        };
        if (TypePicker.SelectedIndex == -1 ||
        ColorPicker.SelectedIndex == -1 ||
        string.IsNullOrWhiteSpace(PriceInput.Text))
        {
            await DisplayAlert("Error", "Please fill all fields.", "OK");
            return;
        }
        var json = JsonSerializer.Serialize(priceRequest);

        var content = new StringContent(json, Encoding.UTF8, "application/json");

        HttpClient client = new HttpClient();

        var response = await client.PostAsync(
            "http://192.168.1.147:5205/api/print/set-price",
            content);

        if (response.IsSuccessStatusCode)
        {
            await DisplayAlert("Success", "Price updated!", "OK");
        }
        else
        {
            await DisplayAlert("Error", "Failed to update price.", "OK");
        }
    }
}