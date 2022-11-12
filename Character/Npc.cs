using System;
using Windows.UI.Xaml.Media.Imaging;

namespace Character
{
    public class Npc : PCharacter
    {
        public Npc(int width = 50, int height = 50) : base(height, width)
        {
            Img.Source = new BitmapImage(new Uri(@"ms-appx:///Assets/virus.png"));
        }
    }
}
