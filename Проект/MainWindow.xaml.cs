using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Threading.Tasks;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;
using System.Windows.Media.Imaging;

namespace Cell_automat_project
{
    public partial class MainWindow : Window
    {
        string selIt;//выбранный для расстановки объект в выпадающем списке
        private void Invite(object sender, RoutedEventArgs e)
        {
            InvitationWindow invWindow = new InvitationWindow();
            invWindow.ShowDialog();

        }//сообщение перед началом

        Dictionary<Point,Jedi> jedis = new Dictionary<Point, Jedi>();//будем заполнять бойцами
        Dictionary<Point, Droid> droids = new Dictionary<Point, Droid>();
        List<Shot> shots = new List<Shot>();//выстрелы

        public static Random rnd1 = new Random(DateTime.Now.Millisecond);//рандом для выстрелов дроидов

        private void Placing(object sender, MouseButtonEventArgs e)
        {   
            Point p = e.GetPosition(this);
            switch (selIt)
            {
                case "Jedi": 
                    Jedi a1 = new Jedi(p, ref canvas);
                    if (!jedis.ContainsKey(new Point(a1.position.X, a1.position.Y)))//два бойца не могут стоять на одной клетке
                    {
                        jedis.Add(new Point(a1.position.X, a1.position.Y), a1);
                    } else { Jedi.amount--; }
                    break;
                case "Droid": 
                    Droid a2 = new Droid(p, ref canvas);
                    if (!droids.ContainsKey(new Point(a2.position.X, a2.position.Y)))
                    {
                        droids.Add(new Point(a2.position.X, a2.position.Y), a2);
                    } else { Droid.amount--; }
                    break;
            }
        }//добавить клетку

        private void Removing(object sender, MouseButtonEventArgs e)
        {
            Point p = e.GetPosition(this);
            p.X = (int)(((int)p.X) / 10);
            p.Y = (((int)p.Y - 40) / 10);
            if (jedis.ContainsKey(p))
            {
                CellObject.killCell(ref canvas, jedis[p].id);
                jedis.Remove(p);
                Jedi.amount--;
            }
            if (droids.ContainsKey(p))
            {
                CellObject.killCell(ref canvas, droids[p].id);
                droids.Remove(p);
                Droid.amount--;
            }

        }//убрать клетку

        private void combobox1_selected(object sender, RoutedEventArgs e)
        {
            ComboBox comboBox = (ComboBox)sender;
            ComboBoxItem selectedItem = (ComboBoxItem)comboBox.SelectedItem;
            selIt = selectedItem.Content.ToString();
        }//выбор расставляемого типа бойцов

        private void makeStep()//действия за один шаг симуляции
        {
            Droid.findComponents(ref droids);//находим центры групп дроидов
            //фаза передвижения
            foreach (KeyValuePair<Point, Droid> d in droids)
            {
                d.Value.process(ref canvas, ref droids, ref jedis);
            }
            foreach (KeyValuePair<Point, Jedi> j in jedis)
            {
                j.Value.process(ref canvas, ref droids, ref jedis);
            }
            CellObject.posUpdate(ref canvas, ref droids, ref jedis, ref shots);//обновляем позиции для всех объектов
            //фаза борьбы
            foreach (KeyValuePair<Point, Droid> d in droids)
            {
                d.Value.attack(ref canvas, ref droids, ref jedis, ref shots);
            }
            foreach (KeyValuePair<Point, Jedi> j in jedis)
            {
                j.Value.attack(ref canvas, ref droids);
            }

            //фаза уборки
            CellObject.lifeUpdate(ref canvas, ref droids, ref jedis, ref shots);
        }

        private void start_click(object sender, RoutedEventArgs e)
        {
            makeStep();
        }

        private void clear_click(object sender, RoutedEventArgs e)
        {
            canvas.Children.Clear();
            droids.Clear();
            jedis.Clear();
            shots.Clear();
        }

        public MainWindow()
        {
            InitializeComponent();

        }
    }

    public partial class InvitationWindow : Window
    {
        public InvitationWindow()
        {
            InitializeComponent();
        }

        private void okbutton(object sender, RoutedEventArgs e)
        {
            this.DialogResult = true;
        }
    }
}
