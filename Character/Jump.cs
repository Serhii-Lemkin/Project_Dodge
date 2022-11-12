using System;
using Windows.UI.Xaml.Media.Imaging;

namespace Character
{
    public class Jump: PCharacter
    {
        public Jump(int width = 50, int height = 50) : base(width, height)
        {
            Img.Source = new BitmapImage(new Uri(@"ms-appx:///Assets/Jump.png"));
            died = true;
        }
        public void Created() { died = false; }
    }
}
