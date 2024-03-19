namespace KSiS2
{
    public partial class MainPage : ContentPage
    {
        int count = 0;

        public MainPage()
        {
            InitializeComponent();
        }

        private void OnButtonReleased(object sender, EventArgs e)
        {
            (sender as Button)!.BackgroundColor = Color.Parse("Blue");
        }

        private void OnButtonFocused(object sender, EventArgs e)
        {
            (sender as Button)!.BackgroundColor = Color.Parse("Orange");
        }

        //настройка сервера
        private void OnSettingsClicked(object sender, EventArgs e)
        {

        }

    }

}
