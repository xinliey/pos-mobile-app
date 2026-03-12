using System.Text.Json;
using System.Text;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace DryLatexApp
{
    public partial class MainPage : ContentPage
    {
        int count;
        string selectedCategory;
        string color;
        public MainPage()
        {
            InitializeComponent();
        }


        private async void OnCounterClicked(object sender, EventArgs e)
        {
            string name = NameInput.Text;
            string weightText = WeightInput.Text;
            string bucketText = BucketInput.Text;
            string deductText = DeductInput.Text;
            string priceText = PriceInput.Text;
            // Default name if empty
            if (string.IsNullOrWhiteSpace(name))
            {
                name = "ลูกค้า";
            }
            

            if (string.IsNullOrWhiteSpace(bucketText))
            {
                bucketText = "0";
               
            }

            if (!double.TryParse(weightText, out double weight))
            {
                await DisplayAlert("Error", "กรุณากรอกน้ำหนักให้ถูกต้อง", "OK");
                return;
            }

            // Validate deduct



            if (!double.TryParse(deductText, out double deduct))
            {
                await DisplayAlert("Error", "กรุณากรอกหักน้ำให้ถูกต้อง", "OK");
                return;
            }


            if (!double.TryParse(priceText, out double price))
            {
                await DisplayAlert("Error", "กรุณากรอกราคาให้ถูกต้อง", "OK");
                return;
            }

            //after data confirmation 
            var data = new
            {
                Name = name,
                TotalWeight = weight.ToString(),
                Bucket = bucketText,
                Deduct = deduct.ToString(),
                Price = price.ToString(),
                Total = ""

            };

            try
            {
                HttpClient client = new HttpClient();

                string json = JsonSerializer.Serialize(data);

                var content = new StringContent(json, Encoding.UTF8, "application/json");

                var response = await client.PostAsync(
                    "http://192.168.1.147:5205/api/print",
                    content);

                string result = await response.Content.ReadAsStringAsync();

                if (response.IsSuccessStatusCode)
                {

                    await DisplayAlert("Server Response", result, "OK");
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
            ResetPage(); //reset the whole page blank after save data 

            /*try
            {
                HttpClient client = new HttpClient();

                var response = await client.PostAsync(
                    "http://192.168.1.147:5205/api/print",
                    null);
                string result = await response.Content.ReadAsStringAsync();

                if (response.IsSuccessStatusCode)
                {
                    await DisplayAlert("Success", "success", "OK");
                }
                else
                {
                    await DisplayAlert("Error", $"Status: {response.StatusCode}\n\n{result}", "OK");
                }
            }
            catch (Exception ex)
            {
                await DisplayAlert("Error", ex.ToString(), "OK");
            }*/
        }

        private async void SumBtn_Clicked(object sender, EventArgs e)
        {
            try
            {
                HttpClient client = new HttpClient();

                var response = await client.PostAsync(
                    "http://192.168.1.147:5205/api/Print/end-day",
                    null);
                string result = await response.Content.ReadAsStringAsync(); //receiving response from backend 

                if (response.IsSuccessStatusCode)
                {
                    await DisplayAlert("Result", result, "OK"); //display response 
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

        private async void EditBtn_Clicked(object sender, EventArgs e)
        {
            await Navigation.PushAsync(new EditList());
        }
        private void ResetPage()
        {
            NameInput.Text = null;
            WeightInput.Text = null;
            BucketInput.Text = null;
            DeductInput.Text = null;
            PriceInput.Text = null;

        }
    }
    

}
