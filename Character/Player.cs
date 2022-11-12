using System;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Imaging;

namespace Character
{
    public class Player: PCharacter
    {
        public bool HasLife { get; set; }
        public Player(int width = 50, int height = 50) : base(width, height)
        {
            HasLife = false;
            Img.Source = new BitmapImage(new Uri(@"ms-appx:///Assets/man.png"));
            Img.Stretch = Stretch.Uniform;
        }
        
    }
}
