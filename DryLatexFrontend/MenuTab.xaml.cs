namespace DryLatexApp;

public partial class MenuTab : TabbedPage
{
	public MenuTab()
	{
		InitializeComponent();
	}
    private async void ReportPage_Appearing(object sender, EventArgs e)
    {
        await MainPage.ShowSummary(this);
    }
}