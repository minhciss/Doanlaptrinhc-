using VinhKhanhapp.Models;
using VinhKhanhapp.PageModels;

namespace VinhKhanhapp.Pages
{
    public partial class MainPage : ContentPage
    {
        public MainPage(MainPageModel model)
        {
            InitializeComponent();
            BindingContext = model;
        }
    }
}