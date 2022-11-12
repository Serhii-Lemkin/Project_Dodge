using Character;
using System;
using Windows.System;
using Windows.UI;
using Windows.UI.Core;
using Windows.UI.Popups;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Imaging;

namespace Logic
{
    public class GLogic
    {
        UInt32 score, prBest;
        const int arrLng = 10;
        const int step = 8;
        int countDeadVir;
        int chosenDifficulty;
        int enemystep;
        bool gamePaused;
        bool jumpAllowed;
        bool mRight, mLeft, mUp, mDown;
        bool endlessChosen;
        double fieldHeight;
        double fieldWidth;
        DispatcherTimer timer = new DispatcherTimer();
        TimeSpan speed = new TimeSpan(0, 0, 0, 0, 15);
        Player pl;
        Npc[] viruses;
        Vaccine vaccine;
        Jump jump;
        Random random = new Random();
        //ui
        Canvas cnv;
        Image pause, backG;
        Button startStop, jumpBtn, restart, info;
        ComboBox difficulty;
        StackPanel ui;
        CheckBox endless;
        TextBlock showScore, showPrScore;
        public GLogic(Canvas cnv)
        {
            CoreWindow.GetForCurrentThread().KeyDown += GLogic_KeyDown;
            CoreWindow.GetForCurrentThread().KeyUp += GLogic_KeyUp;
            CoreWindow.GetForCurrentThread().SizeChanged += GLogic_SizeChanged;
            fieldHeight = CoreWindow.GetForCurrentThread().Bounds.Height;
            fieldWidth = CoreWindow.GetForCurrentThread().Bounds.Width;
            viruses = new Npc[arrLng];
            gamePaused = true;
            endlessChosen = false;
            timer.Tick += Timer_Tick;
            timer.Interval = speed;
            this.cnv = cnv;
            InitGame();
            chosenDifficulty = difficulty.SelectedIndex;
            prBest = 0;
            //InfoDialog();
        }
        void GLogic_SizeChanged(CoreWindow sender, WindowSizeChangedEventArgs args)
        {
            backG.Width = fieldWidth = CoreWindow.GetForCurrentThread().Bounds.Width;
            backG.Height = fieldHeight = CoreWindow.GetForCurrentThread().Bounds.Height;
            Canvas.SetLeft(pause, fieldWidth - 50);
            ui.Width = fieldWidth;
        }
        void GLogic_KeyUp(CoreWindow sender, KeyEventArgs args)
        {
            if (args.VirtualKey == VirtualKey.S || args.VirtualKey == VirtualKey.Down) mDown = false;
            if (args.VirtualKey == VirtualKey.W || args.VirtualKey == VirtualKey.Up) mUp = false;
            if (args.VirtualKey == VirtualKey.A || args.VirtualKey == VirtualKey.Left) mLeft = false;
            if (args.VirtualKey == VirtualKey.D || args.VirtualKey == VirtualKey.Right) mRight = false;
        }
        void GLogic_KeyDown(CoreWindow sender, KeyEventArgs args)
        {
            if (args.VirtualKey == VirtualKey.P)
            {
                if (gamePaused) StartGame();
                else PauseGame();
            }
            if (args.VirtualKey == VirtualKey.R)
            {
                if (!gamePaused && jumpAllowed)
                {
                    PLaceInValidPosition(pl);
                    jumpAllowed = false;
                    jumpBtn.IsEnabled = false;
                }
            }
            if (args.VirtualKey == VirtualKey.N) InitGame();
            if (args.VirtualKey == VirtualKey.I) InfoDialog();

            if (args.VirtualKey == VirtualKey.S || args.VirtualKey == VirtualKey.Down) mDown = true;
            if (args.VirtualKey == VirtualKey.W || args.VirtualKey == VirtualKey.Up) mUp = true;
            if (args.VirtualKey == VirtualKey.A || args.VirtualKey == VirtualKey.Left) mLeft = true;
            if (args.VirtualKey == VirtualKey.D || args.VirtualKey == VirtualKey.Right) mRight = true;
        }
        public void InitGame()
        {
            mRight = mLeft = mUp = mDown = false;
            jumpAllowed = true;
            score = 0;
            countDeadVir = 0;
            cnv.Children.Clear();
            CreateBackGroundImage();
            PlaceCharacters();
            InitUI();
            endless.IsChecked = endlessChosen;
            ChangeDifficulty();
            PauseGame();
            jump = new Jump();
            vaccine = new Vaccine();
            vaccine.ChDied();
            jump.ChDied();
        }
        void PlaceCharacters()
        {
            pl = new Player(50, 80);
            cnv.Children.Add(pl.Img);
            UpdateLoc(pl, GetRandomX(), GetRandomY());
            for (int i = 0; i < viruses.Length; i++)
            {
                viruses[i] = new Npc();
                cnv.Children.Add(viruses[i].Img);
                PLaceInValidPosition(viruses[i], i);
            }
        }
        void PLaceInValidPosition(PCharacter ch, int index = arrLng)
        {
            double x, y;
            bool valid;
            while (true)
            {
                valid = true;
                x = GetRandomX();
                y = GetRandomY();
                if (ch is Npc || ch is Vaccine || ch is Jump)
                    if (!CheckColisionByScale(pl, x, y, 5))
                    {
                        valid = false;
                        continue;
                    }
                for (int i = 0; i < index; i++)
                {
                    if (viruses[i].Died || i == index) continue;
                    if (!CheckColisionByScale(viruses[i], x, y, 2))
                    {
                        valid = false;
                        break;
                    }
                }
                if (!valid) continue;
                UpdateLoc(ch, x, y);
                break;
            }
        }
        bool CheckColisionByScale(PCharacter ch, double x, double y, double scale = 0.5)
        {
            if (Math.Abs(ch.LocX - x) <= ch.Img.Width * scale && Math.Abs(ch.LocY - y) <= ch.Img.Height * scale)
                return false;
            return true;
        }
        void NpcMover()
        {
            foreach (var virus in viruses)
            {
                if (virus.Died == true) continue;
                if (Math.Abs(virus.LocX - pl.LocX) >= enemystep)
                {
                    if (virus.LocX > pl.LocX) UpdateLoc(virus, virus.LocX - enemystep, virus.LocY);
                    else UpdateLoc(virus, virus.LocX + enemystep, virus.LocY);
                }
                if (Math.Abs(virus.LocY - pl.LocY) >= enemystep)
                {
                    if (virus.LocY > pl.LocY) UpdateLoc(virus, virus.LocX, virus.LocY - enemystep);
                    else UpdateLoc(virus, virus.LocX, virus.LocY + enemystep);
                }
            }
        }
        void Timer_Tick(object sender, object e)
        {
            EndGameConditions();
            PlayerMover();
            NpcMover();
            ColisionChecker();
            if (endless.IsChecked.Value)
            {
                if (random.Next(300) == 1 && vaccine.Died && !pl.HasLife)
                {
                    vaccine.Created();
                    cnv.Children.Add(vaccine.Img);
                    PLaceInValidPosition(vaccine);
                }
                if (random.Next(300) == 2 && jump.Died)
                {
                    jump.Created();
                    cnv.Children.Add(jump.Img);
                    PLaceInValidPosition(jump);
                }
                score++;
                showScore.Text = $"Score:{score}";
            }
        }
        void EndGameConditions()
        {
            string s = "";

            if (countDeadVir == viruses.Length - 1 && endless.IsChecked == false)
            {
                s = "You didn't get sick! Keep a good work Staying Safe!";
                EndGameMessage(s);
            }
            if (pl.Died)
            {
                if (pl.Died && countDeadVir == 0)
                    s = "You got sick... Assigned to home quarantine!";
                else if (pl.Died && countDeadVir == 1)
                    s = $"You got sick... But you avoided 1 virus. Assigned to home quarantine!";
                else if (pl.Died && countDeadVir != 1 && countDeadVir != 0)
                    s = $"You got sick... But you avoided {countDeadVir} viruses. Assigned to home quarantine!";
                if (endless.IsChecked.Value)
                {
                    s += $"\nYour score is {score}.";
                    if (score > prBest) prBest = score;
                    score = 0;
                }
                EndGameMessage(s);
            }
        }
        async void EndGameMessage(string s)
        {
            difficulty.IsEnabled = true;
            endless.IsEnabled = true;
            PauseGame();
            MessageDialog dlg = new MessageDialog(s, "Game Over!");
            UICommand yesCommand = new UICommand("Restart", RestartGameCommand);
            UICommand noCommand = new UICommand("Quit", CloseGameCommand);
            dlg.Commands.Add(yesCommand);
            dlg.Commands.Add(noCommand);
            await dlg.ShowAsync();
        }
        void CloseGameCommand(IUICommand command) => Application.Current.Exit();
        void RestartGameCommand(IUICommand command)
        {
            gamePaused = true;
            InitGame();
        }
        void ColisionChecker()
        {
            if (endless.IsChecked.Value)
            {
                if (!vaccine.Died)
                {
                    if (!CheckColisionByScale(pl, vaccine.LocX, vaccine.LocY))
                    {
                        vaccine.ChDied();
                        pl.HasLife = true;
                        cnv.Children.Remove(vaccine.Img);
                        ui.Children.Add(vaccine.Img);
                    }
                }
                if (!jump.Died)
                {
                    if (!CheckColisionByScale(pl, jump.LocX, jump.LocY))
                    {
                        jumpAllowed = true;
                        jumpBtn.IsEnabled = true;
                        jump.ChDied();
                        cnv.Children.Remove(jump.Img);
                        score += 500;
                    }
                }
            }
            for (int i = 0; i < viruses.Length; i++)
            {
                if (viruses[i].Died) continue;
                if (!CheckColisionByScale(pl, viruses[i].LocX, viruses[i].LocY))
                {
                    if (pl.HasLife)
                    {
                        pl.HasLife = false;
                        ui.Children.Remove(vaccine.Img);
                        if (endless.IsChecked.Value) PLaceInValidPosition(viruses[i]);
                    }
                    else pl.ChDied();
                }
                for (int j = 0; j < viruses.Length; j++)
                {
                    if (viruses[j].Died || i == j) continue;
                    if (!CheckColisionByScale(viruses[i], viruses[j].LocX, viruses[j].LocY))
                    {
                        if (endless.IsChecked.Value)
                        {
                            PLaceInValidPosition(viruses[j]);
                            countDeadVir++;
                        }
                        else
                        {
                            viruses[j].ChDied();
                            cnv.Children.Remove(viruses[j].Img);
                            countDeadVir++;
                        }
                    }
                }
            }
        }
        void PlayerMover()
        {
            if (mUp) UpdateLoc(pl, pl.LocX, pl.LocY - step);
            if (mDown) UpdateLoc(pl, pl.LocX, pl.LocY + step);
            if (mRight) UpdateLoc(pl, pl.LocX + step, pl.LocY);
            if (mLeft) UpdateLoc(pl, pl.LocX - step, pl.LocY);
        }
        void UpdateLoc(PCharacter ch, double x, double y)
        {
            if (x > 0 && x < fieldWidth - ch.Img.Width)
                ch.LocX = x;
            if (y > 50 && y < fieldHeight - ch.Img.Height)
                ch.LocY = y;
            Canvas.SetLeft(ch.Img, ch.LocX);
            Canvas.SetTop(ch.Img, ch.LocY);
        }
        void CreateBackGroundImage()
        {
            backG = new Image
            {
                Height = fieldHeight,
                Width = fieldWidth,
                Source = new BitmapImage(new Uri(@"ms-appx:///Assets/back.jpeg")),
                Stretch = Stretch.UniformToFill
            };
            cnv.Children.Add(backG);
        }
        void InitUI()
        {
            int heightBtn = 50;
            int widthBtn = 100;
            TextBlock dif = new TextBlock()
            {
                TextWrapping = TextWrapping.NoWrap,
                Width = widthBtn,
                Text = "Difficulty:",
                TextAlignment = TextAlignment.Center,
                VerticalAlignment = VerticalAlignment.Center,
                Foreground = new SolidColorBrush(Colors.Black)

            };
            info = new Button
            {
                Height = heightBtn,
                Width = widthBtn,
                Content = "Info",
                Foreground = new SolidColorBrush(Colors.Black)
            };
            info.Click += Info_Click;
            startStop = new Button
            {
                Height = heightBtn,
                Width = widthBtn,
                Content = "Start",
                Foreground = new SolidColorBrush(Colors.Black)
            };
            startStop.Click += StartStop_Click;
            restart = new Button
            {
                Height = heightBtn,
                Width = widthBtn,
                Content = "Restart",
                Foreground = new SolidColorBrush(Colors.Black)
            };
            restart.Click += Restart_Click;
            jumpBtn = new Button
            {
                Height = heightBtn,
                Width = widthBtn,
                Content = "Jump",
                Foreground = new SolidColorBrush(Colors.Black)
            };
            jumpBtn.Click += Jump_Click;
            difficulty = new ComboBox
            {
                Height = heightBtn,
                Width = widthBtn,
                Foreground = new SolidColorBrush(Colors.Black),
                Background = new SolidColorBrush(Color.FromArgb(50, 255, 255, 255))
            };
            difficulty.Items.Add("Easy");
            difficulty.Items.Add("Normal");
            difficulty.Items.Add("Hard");
            difficulty.SelectedIndex = chosenDifficulty;
            difficulty.SelectionChanged += Difficulty_SelectionChanged;
            endless = new CheckBox
            {
                Width = widthBtn,
                Content = "Endless",
                HorizontalAlignment = HorizontalAlignment.Right,
                VerticalAlignment = VerticalAlignment.Center
            };
            endless.Checked += Endless_Checked;
            endless.Unchecked += Endless_Unchecked;
            StackPanel endlessBack = new StackPanel
            {
                Height = heightBtn,
                Width = widthBtn + 30,
                Background = new SolidColorBrush(Colors.LightBlue)
            };
            endlessBack.Children.Add(endless);
            showScore = new TextBlock
            {
                TextWrapping = TextWrapping.NoWrap,
                Width = widthBtn * 2,
                Text = $"Score:",
                TextAlignment = TextAlignment.Center,
                VerticalAlignment = VerticalAlignment.Center,
                Foreground = new SolidColorBrush(Colors.Black)
            };
            showPrScore = new TextBlock
            {
                TextWrapping = TextWrapping.NoWrap,
                Width = widthBtn * 2,
                Text = $"Best:{prBest}",
                TextAlignment = TextAlignment.Center,
                VerticalAlignment = VerticalAlignment.Center,
                Foreground = new SolidColorBrush(Colors.Black)
            };
            ui = new StackPanel
            {
                Orientation = Orientation.Horizontal,
                Height = heightBtn,
                Width = fieldWidth,
                Background = new SolidColorBrush(Color.FromArgb(50, 255, 255, 255))
            };
            ui.Children.Add(info);
            ui.Children.Add(startStop);
            ui.Children.Add(restart);
            ui.Children.Add(jumpBtn);
            ui.Children.Add(dif);
            ui.Children.Add(difficulty);
            ui.Children.Add(endlessBack);
            cnv.Children.Add(ui);

            pause = new Image();
            pause.Width = pause.Height = 50;
            pause.Source = new BitmapImage(new Uri(@"ms-appx:///Assets/pause.png"));
            cnv.Children.Add(pause);
            Canvas.SetLeft(pause, fieldWidth - 50);
        }
        void Endless_Unchecked(object sender, RoutedEventArgs e)
        {
            endlessChosen = endless.IsChecked.Value;
            ui.Children.Remove(showScore);
            ui.Children.Remove(showPrScore);
        }
        void Endless_Checked(object sender, RoutedEventArgs e)
        {
            endlessChosen = endless.IsChecked.Value;
            ui.Children.Add(showScore);
            ui.Children.Add(showPrScore);
        }
        void Difficulty_SelectionChanged(object sender, SelectionChangedEventArgs e) => ChangeDifficulty();
        void StartStop_Click(object sender, RoutedEventArgs e)
        {
            if (startStop.IsPointerOver)
            {
                if (gamePaused)
                {
                    gamePaused = false;
                    StartGame();
                    startStop.Content = "Pause";
                }
                else
                {
                    gamePaused = true;
                    PauseGame();
                    startStop.Content = "Start";
                }
            }
        }
        void Jump_Click(object sender, RoutedEventArgs e)
        {
            if (jumpAllowed && !gamePaused && jumpBtn.IsPointerOver)
            {
                PLaceInValidPosition(pl);
                jumpAllowed = false;
                jumpBtn.IsEnabled = false;
            }
        }
        void Info_Click(object sender, RoutedEventArgs e) { if (info.IsPointerOver) InfoDialog(); }
        void Restart_Click(object sender, RoutedEventArgs e) { if (restart.IsPointerOver) InitGame(); }
        double GetRandomX() => (fieldWidth - pl.Img.Width) * random.NextDouble(); 
        double GetRandomY() => (fieldHeight - pl.Img.Height - 50) * random.NextDouble() + 50; 
        async void InfoDialog()
        {
            string s = $"This is a game where you are trying to stay safe, while viruses all arround you start coming. You have to run, Forest, run! " +
                $"If a virus catches you, you lose. If two viruses collide one of them dies. You win when there is only one virus left. Understood? Great!\n\n" +
                $"   Moving Your Character:\n" +
                $"1. To move up press W or Up key on your keyboard.\n" +
                $"2. To move down press S or Down key on your keyboard.\n" +
                $"3. To move left press A or Left key on your keyboard.\n" +
                $"4. To move right press D or Right key on your keyboard.\n" +
                $"   Rules:\n" +
                $"1. When you open the game, your game is paused and ready to be played.\n" +
                $"2. To start the game press the Start button or press P on your keyboard. When you do, the Start button will change to be a Pause button you can use.\n" +
                $"3. To pause press the Pause button or press P on your keyboard. When you do the Pause button will change to be a Start button. To unpause press the Start button or press P again.\n" +
                $"4. To make a jump to random location press the Jump button, or press R on your keyboard. Notice that you have only one jump per game, so use it as your last resort." +
                $"Notice also that the location you will appear at after jump is semirandom. You will be place in some distance from your enemies, but you can still be surrounded.\nGame of chance, game of luck.\n" +
                $"5. To initialize reload press the Reload Button, or press N on your keyboard.\n" +
                $"6. There is a dropdown menu for difficulty. The default is Easy, But if the game is too slow for you you can change the difficulty. Simply Click the menu and chose whatever you prefer.\n" +
                $"7. To enter the endless mode check the Endless checkbox. If it checked you enter the mode, when instead of dying viruses will find a new place on the screen and will chase you again. The score of the viruses you avoided will be displayed in the end.\n" +
                $"The endless mode opens a few new features I'll let you figure out for yourself." +
                $"8. On the top right side you have a picture that indicates if the game is paused. If you dont see the picture your game is running.\n\n" +
                $"Notice: If you change the resolution of the Window it is greatly recomended to reload the game (as described at Rules.5) to benefit from full screen generation.\n\n" +
                $"Good Luck, Have Fun! And thank you for choosing the DodgeGame by Sergey Lemkin\n" +
                $"2022, Sergey Lemkin, the CEO of SHCPI (Sergey's Home Coutch Potato Industries). All rights reserved.";
            MessageDialog dlg = new MessageDialog(s, "Welcome To Dodge Game!");
            await dlg.ShowAsync();
        }
        void StartGame()
        {
            timer.Start();
            gamePaused = false;
            endless.IsEnabled = false;
            difficulty.IsEnabled = false;
            mRight = mLeft = mUp = mDown = false;
            pause.Visibility = Visibility.Collapsed;
            startStop.Content = "Pause";
            gamePaused = false;
        }
        void PauseGame()
        {
            timer.Stop();
            gamePaused = true;
            pause.Visibility = Visibility.Visible;
            startStop.Content = "Start";
            gamePaused = true;
        }
        private void ChangeDifficulty()
        {
            if (difficulty.SelectedIndex == 0) enemystep = 4;
            if (difficulty.SelectedIndex == 1) enemystep = 5;
            if (difficulty.SelectedIndex == 2) enemystep = 6;
            chosenDifficulty = difficulty.SelectedIndex;
        }
    }
}
