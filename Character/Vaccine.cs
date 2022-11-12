using System;
using Windows.UI.Xaml.Media.Imaging;

namespace Character
{
    public class Vaccine: PCharacter
    {
        public Vaccine(int width = 50, int height = 50) : base(width, height)
        {
            Img.Source = new BitmapImage(new Uri(@"ms-appx:///Assets/vaccine.png"));
            died = true;
        }
        public void Created() { died = false; }
    }
}
