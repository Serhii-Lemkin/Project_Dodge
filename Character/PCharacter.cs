using Windows.UI.Xaml.Controls;

namespace Character
{
    public class PCharacter
    {
        protected bool died;
        double locX, locY;
        private Image _img;
        public PCharacter(int width = 50, int height = 50)
        {
            _img = new Image();
            _img.Height = height;
            _img.Width = width;
            died = false;
        }
        public Image Img { get { return _img; } set { _img = value; } }
        public void ChDied() { died = true; }
        public double LocX { get { return locX; } set { locX = value; } }
        public double LocY { get { return locY; } set { locY = value; } }
        public bool Died { get { return died; } }

    }
}
