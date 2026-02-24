namespace DryLatexApp
{
    public partial class MainPage : ContentPage
    {
        int count = 0;

        public MainPage()
        {
            InitializeComponent();
        }

        private async void OnCounterClicked(object sender, EventArgs e)
        {
            try
            {
                HttpClient client = new HttpClient();

                var response = await client.PostAsync(
                    "http://192.168.1.147:5205/api/print",
                    null);

                if (response.IsSuccessStatusCode)
                {
                    await DisplayAlert("Success", "Printed!", "OK");
                }
                else
                {
                    await DisplayAlert("Error", "Print failed", "OK");
                }
            }
            catch (Exception ex)
            {
                await DisplayAlert("Error", ex.Message, "OK");
            }
        }
    }

}
