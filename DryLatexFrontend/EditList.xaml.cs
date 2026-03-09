using System.Text.Json;

namespace DryLatexApp;

public partial class EditList : ContentPage
{
	List<Bill> bills;
	public EditList()
	{
		InitializeComponent();
        LoadBills();
	}
    private async void LoadBills()
    {
        HttpClient client = new HttpClient();
   
        var response = await client.GetAsync("http://192.168.1.147:5205/api/Print/GetBillList");
        
        string json = await response.Content.ReadAsStringAsync();

        bills = JsonSerializer.Deserialize<List<Bill>>(json, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        });
   
    
        BillCollection.ItemsSource = bills;
    }
    private async void BillCollection_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        var selectedBill = e.CurrentSelection.FirstOrDefault() as Bill;

        if (selectedBill == null)
            return;

        await Navigation.PushAsync(new EditPage(selectedBill));
    }
}