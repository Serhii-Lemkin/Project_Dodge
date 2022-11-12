using Logic;
using Windows.UI.Xaml.Controls;

namespace Project_Dodge_1
{
    public sealed partial class MainPage : Page
    {
        public MainPage()
        {
            this.InitializeComponent();
            GLogic logic = new GLogic(cnv);
        }
    }
}
