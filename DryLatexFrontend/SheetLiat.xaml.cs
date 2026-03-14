using System.Text.Json;

namespace DryLatexApp;

public partial class SheetLiat : ContentPage
{
    List<Bill> bills;
    public SheetLiat()
	{
		InitializeComponent();
        LoadBills();
	}
    protected override void OnAppearing()
    {
        base.OnAppearing();
        LoadBills();
    }
    private async void LoadBills()
    {
        HttpClient client = new HttpClient();

        var response = await client.GetAsync("http://192.168.1.147:5205/api/Print/GetSheetList");

        string json = await response.Content.ReadAsStringAsync();

        bills = JsonSerializer.Deserialize<List<Bill>>(json, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        });
        //await DisplayAlert("Backend Response", json, "OK");

        SheetCollection.ItemsSource = bills;
    }
  

    private async void BillCollection_SelectionChanged_1(object sender, SelectionChangedEventArgs e)
    {
   var selectedBill = e.CurrentSelection.FirstOrDefault() as Bill;

        if (selectedBill == null)
            return;

        await Navigation.PushAsync(new EditPage(selectedBill));
       SheetCollection.SelectedItem = null; //reset the index so the same index can be selected 
    }
}