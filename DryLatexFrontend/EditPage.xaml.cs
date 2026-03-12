using System.Text.Json;
using System.Text;
namespace DryLatexApp;

public partial class EditPage : ContentPage
{
	Bill bill;
    int id;
	public EditPage(Bill selectedBill)
	{
        
		InitializeComponent();
        bill = selectedBill;
        id = bill.Id;
        TestDisplay();
    }
    private async void TestDisplay()
    {
        
        NameInput.Text = bill.Name;
        WeightInput.Text = bill.TotalWeight;
        BucketInput.Text = bill.Bucket;
        DeductInput.Text = bill.Deduct;
        PriceInput.Text = bill.Price;
        TotalInput.Text = bill.Total;
       
    }

    private async void saveprintbtn_Clicked(object sender, EventArgs e)
    {
        
        string name = NameInput.Text;
        string weightText = WeightInput.Text;
        string bucketText = BucketInput.Text;
        string deductText = DeductInput.Text;
        string priceText = PriceInput.Text;
        TotalInput.Text = "";
        // Default name if empty
       

        if (string.IsNullOrWhiteSpace(bucketText))
        {
            bucketText = "0";

        }

        if (string.IsNullOrWhiteSpace(weightText))
        {
            await DisplayAlert("Error", "??????????????????????????", "OK");
            return;
        }

        // Validate deduct



        if (string.IsNullOrWhiteSpace(deductText))
        {
            await DisplayAlert("Error", "?????????????????????????", "OK");
            return;
        }


        if (string.IsNullOrWhiteSpace(priceText))
        {
            await DisplayAlert("Error", "???????????????????????", "OK");
            return;
        }

        //after data confirmation 
        var data = new
        {
            Id = id,
            Name = name,
            Totalweight = weightText,
            Bucket = bucketText,
            Deduct = deductText,
            Price = priceText,
            Total = ""//recalculate in the backend
           
       
        };

        try
        {
            HttpClient client = new HttpClient();

            string json = JsonSerializer.Serialize(data);

            var content = new StringContent(json, Encoding.UTF8, "application/json");

            var response = await client.PostAsync(
                "http://192.168.1.147:5205/api/Print/EditDataAndPrint",
                content);

            string result = await response.Content.ReadAsStringAsync();

            if (response.IsSuccessStatusCode)
            {

                await DisplayAlert("Server Response","??????????????????????????", "OK");
                TotalInput.Text = result;
            }
            else
            {
                await DisplayAlert("Error", $"Status: {response.StatusCode}\n\n{result}", "OK");
            }
        }
        catch (Exception ex)
        {
            await DisplayAlert("Error", ex.ToString(), "OK");
        }
    }

    private async void saveeditbtn_Clicked(object sender, EventArgs e)
    {
        string name = NameInput.Text;
        string weightText = WeightInput.Text;
        string bucketText = BucketInput.Text;
        string deductText = DeductInput.Text;
        string priceText = PriceInput.Text;
        TotalInput.Text = "";
        // Default name if empty


        if (string.IsNullOrWhiteSpace(bucketText))
        {
            bucketText = "0";

        }

        if (string.IsNullOrWhiteSpace(weightText))
        {
            await DisplayAlert("Error", "??????????????????????????", "OK");
            return;
        }

        // Validate deduct



        if (string.IsNullOrWhiteSpace(deductText))
        {
            await DisplayAlert("Error", "?????????????????????????", "OK");
            return;
        }


        if (string.IsNullOrWhiteSpace(priceText))
        {
            await DisplayAlert("Error", "???????????????????????", "OK");
            return;
        }

        //after data confirmation 
        var data = new
        {
            Id = id,
            Name = name,
            Totalweight = weightText,
            Bucket = bucketText,
            Deduct = deductText,
            Price = priceText,
            Total = ""//recalculate in the backend


        };

        try
        {
            HttpClient client = new HttpClient();

            string json = JsonSerializer.Serialize(data);

            var content = new StringContent(json, Encoding.UTF8, "application/json");

            var response = await client.PostAsync(
                "http://192.168.1.147:5205/api/Print/EditData",
                content);

            string result = await response.Content.ReadAsStringAsync();

            if (response.IsSuccessStatusCode)
            {

                await DisplayAlert("Server Response", "??????????????????????????", "OK");
                TotalInput.Text = result;
            }
            else
            {
                await DisplayAlert("Error", $"Status: {response.StatusCode}\n\n{result}", "OK");
            }
        }
        catch (Exception ex)
        {
            await DisplayAlert("Error", ex.ToString(), "OK");
        }
    }
}