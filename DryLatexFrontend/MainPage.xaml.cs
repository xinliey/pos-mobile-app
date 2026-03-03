using System.Text.Json;
using System.Text;

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

        protected override async void OnAppearing()
        {
            base.OnAppearing();

            await Navigation.PushModalAsync(new SetPricePage());
        }

        private async void OnCounterClicked(object sender, EventArgs e)
        {
            string name = NameInput.Text;
            string weightText = WeightInput.Text;
            string bucketText = BucketInput.Text;
            string deductText = DeductInput.Text;

            // Default name if empty
            if (string.IsNullOrWhiteSpace(name))
            {
                name = "ลูกค้า";
            }

            // Validate weight
            if (!double.TryParse(weightText, out double weight))
            {
                await DisplayAlert("Error", "กรุณากรอกน้ำหนักให้ถูกต้อง", "OK");
                return;
            }

            // Validate deduct
            if (!double.TryParse(deductText, out double deduct))
            {
                await DisplayAlert("Error", "กรุณากรอกค่าหักให้ถูกต้อง", "OK");
                return;
            }

            // Validate category
            if (string.IsNullOrWhiteSpace(selectedCategory))
            {
                await DisplayAlert("Error", "กรุณาเลือกชนิดขี้ยาง", "OK");
                return;
            }

            // Validate color
            if (string.IsNullOrWhiteSpace(color))
            {
                await DisplayAlert("Error", "กรุณาเลือกระดับความแห้ง", "OK");
                return;
            }
           //after data confirmation 
            var data = new
            {
                Name = name,
                Weight = weight,
                Bucket = bucketText,
                Deduct = deduct,
                Category = selectedCategory,
                Color = color
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
        private void OnCategoryChanged(object sender, EventArgs e)
        {
            if (TypePicker.SelectedIndex == -1)
                return;
             selectedCategory= TypePicker.SelectedItem?.ToString();
           
            ColorPicker.Items.Clear();
            ColorPicker.SelectedIndex = -1;
            if (selectedCategory == "จอก" || selectedCategory == "ก้อน")
            {
                ColorPicker.Items.Add("ขาว");
                ColorPicker.Items.Add("แห้ง");
                ColorPicker.Items.Add("เหลือง");
            }
            else if (selectedCategory == "เส้น")
            {
                ColorPicker.Items.Add("ขาว");
                ColorPicker.Items.Add("แห้ง");
            }
        }

        private void ColorPicker_SelectedIndexChanged(object sender, EventArgs e)
        {
            color = ColorPicker.SelectedItem?.ToString();
        }
    }

}
