using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;
using System.Windows.Media.Animation;

namespace Cell_automat_project
{
    enum Types { Jedi, Droid };//добавлять все виды объектов

    public class CellObject
    {
        public Point position;
        public bool isMovable;
        public Color color;
        public int size;
        public int id;

        public void createCell(ref Canvas canv)
        {
            Rectangle cell = new Rectangle();
            cell.Width = size;
            cell.Height = size;
            canv.Children.Add(cell);
            position.X = (int)(((int)position.X) / 10);//операции с 5 чтобы попадать в клеточки
            position.Y = (((int)position.Y - 40) / 10);// -40 потому что нужна позиция относительно Canvas, а GetPosition выдает относительно окна
            Canvas.SetLeft(cell, position.X*10);
            Canvas.SetTop(cell, position.Y*10);
            SolidColorBrush myBrush1 = new SolidColorBrush(color);
            cell.Fill = myBrush1;
            id = canv.Children.IndexOf(cell);
        }

        public static void killCell(ref Canvas canv, int i)
        {
            ((Rectangle)canv.Children[i]).Opacity = 0;
        }

        public void move(Point p, ref Canvas canv)
        {
            //передвижение анимацией
            DoubleAnimation moveAnimation = new DoubleAnimation(position.X * 10, p.X * 10, TimeSpan.FromMilliseconds(500));
            canv.Children[id].BeginAnimation(Canvas.LeftProperty, moveAnimation);
            moveAnimation = new DoubleAnimation(position.Y * 10, p.Y * 10, TimeSpan.FromMilliseconds(500));
            canv.Children[id].BeginAnimation(Canvas.TopProperty, moveAnimation);
            position.X = p.X;
            position.Y = p.Y;

            //передвижение скачком
            //position.X = p.X;
            //Canvas.SetLeft(canv.Children[id], p.X * 10);
            //position.Y = p.Y;
            //Canvas.SetTop(canv.Children[id], p.Y * 10);
        }

        public void moveToTarget(Point targ, ref Canvas canv, ref Dictionary<Point, Droid> droids, ref Dictionary<Point, Jedi> jedis, bool bypass)//TODO сделать kwargs для того, с чем можно столкнуться
        {   
            int newX = (int)position.X; int newY = (int)position.Y;
            double deltaX = (int)targ.X - (int)position.X; double deltaY = (int)targ.Y - (int)position.Y;
            var scope = (double)deltaY / deltaX;

            if ((deltaX == 0) || (deltaY == 0))
            {
                newX = (int)position.X + (deltaX == 0 ? 0 : (int)(deltaX / Math.Abs(deltaX)));
                newY = (int)position.Y + (deltaY == 0 ? 0 : (int)(deltaY / Math.Abs(deltaY)));
            }
            else
            {
                if (Math.Abs(scope) >= 1)
                {
                    newY = (int)position.Y + (int)(deltaY / Math.Abs(deltaY));
                    newX = (int)position.X + (int)Math.Round((newY - (int)position.Y) / (scope));
                }
                else
                {
                    newX = (int)position.X + (int)(deltaX / Math.Abs(deltaX));
                    newY = (int)position.Y + (int)Math.Round((newX - (int)position.X) * (scope));
                }
            }

            if (!(droids.ContainsKey(new Point(newX, newY)) || jedis.ContainsKey(new Point(newX, newY))) || !bypass)//чтобы не шел на занятые клетки
            {
                this.move(new Point(newX, newY), ref canv);
                //switch (((object)this).GetType().Name)
                //{
                //    case "Droid":
                //        droids.Remove(this.position);
                //        droids.Add(new Point(newX, newY), (Droid)this);
                //        break;
                //    case "Jedi":
                //        jedis.Remove(this.position);
                //        jedis.Add(new Point(newX, newY), (Jedi)this);
                //        break;
                //}
                //position = new Point(newX, newY);
            }
        }

        public static void posUpdate(ref Canvas canv, ref Dictionary<Point, Droid> droids, ref Dictionary<Point, Jedi> jedis, ref List<Shot> shots)//TODO сделать kwargs для всех типов
        {
            Dictionary<Point, Droid> tempdroids = new Dictionary<Point, Droid>();
            Dictionary<Point, Jedi> tempjedis = new Dictionary<Point, Jedi>();
            foreach (KeyValuePair<Point, Droid> d in droids)
            {
                if (tempdroids.ContainsKey(new Point(d.Value.position.X, d.Value.position.Y)))//два бойца пришли на одну клетку одновременно - решение проблемы
                {
                    droids[d.Key].move(d.Key, ref canv);
                }
                tempdroids.Add(new Point(d.Value.position.X, d.Value.position.Y), d.Value);
            }
            droids.Clear();
            foreach (KeyValuePair<Point, Droid> d in tempdroids)
            {
                droids.Add(d.Key, d.Value);
            }
            foreach (KeyValuePair<Point, Jedi> j in jedis)
            {   
                if (tempjedis.ContainsKey(new Point(j.Value.position.X, j.Value.position.Y))){//два бойца пришли на одну клетку одновременно - решение проблемы
                    jedis[j.Key].move(j.Key, ref canv);
                }
                tempjedis.Add(new Point(j.Value.position.X, j.Value.position.Y), j.Value);
            }
            jedis.Clear();
            foreach (KeyValuePair<Point, Jedi> j in tempjedis)
            {
                j.Value.health = Jedi.maxHealth;
                jedis.Add(j.Key, j.Value);
            }
            foreach (Shot sh in shots)
            {
                sh.process(ref canv, ref droids, ref jedis);
            }
        }

        public static void lifeUpdate(ref Canvas canv, ref Dictionary<Point, Droid> droids, ref Dictionary<Point, Jedi> jedis, ref List<Shot> shots)
        {
            List<Point> dk = droids.Keys.ToList();
            List<Point> jk = jedis.Keys.ToList();
            foreach (Point d in dk)
            {
                if (droids[d].health <= 0)
                {
                    killCell(ref canv, droids[d].id);
                    droids.Remove(d);
                    Droid.amount--;
                }
            }
            foreach (Point j in jk)
            {
                if (jedis[j].health <= 0)
                {
                    killCell(ref canv, jedis[j].id);
                    jedis.Remove(j);
                    Jedi.amount--;
                }
            }
            for (int i = 0; i < shots.Count; i++)
            {
                if (shots[i].age >= Shot.maxAge || shots[i].position.Y<0)
                {
                    killCell(ref canv, shots[i].id);
                    shots.RemoveAt(i--);
                }
            }
        }

        public static double distance(Point p1, Point p2)
        {
            return Math.Sqrt(Math.Pow(p1.X - p2.X, 2) + Math.Pow(p1.Y - p2.Y, 2));
        }
    }

    public class Fighter : CellObject
    {
        public int power;
        public int health;
        public int weight;

    }

    public class Jedi : Fighter
    {
        static public int amount;
        public static int maxHealth = 4;

        public Jedi(Point pos, ref Canvas canv)
        {
            isMovable = true;
            position = pos;
            health = maxHealth;
            color = Colors.Aqua;
            size = 10;
            amount++;
            createCell(ref canv);
        }

        public void process(ref Canvas canv, ref Dictionary<Point, Droid> droids, ref Dictionary<Point, Jedi> jedis)
        {
            double minDist = 10000000;
            List<Point> variants = new List<Point>();
            foreach (Point po in Droid.components)
            {
                if (po.X != this.position.X && po.Y != this.position.Y)
                {
                    var dist = distance(this.position, po);
                    if (dist < minDist)
                    {
                        variants.Clear();
                        variants.Add(po);
                        minDist = dist;
                    }
                    else if (dist == minDist)
                    {
                        variants.Add(po);
                    }
                }
            }
            if (variants.Count != 0)
            {
                Random rnd = new Random();
                int po = rnd.Next(0, variants.Count);
                moveToTarget(variants[po], ref canv, ref droids, ref jedis, true);
            }
        }

        public void attack(ref Canvas canv, ref Dictionary<Point, Droid> droids)
        {
            int x = (int)position.X; int y = (int)position.Y;
            Point[] n = { new Point(x - 1, y - 1), new Point(x - 1, y), new Point(x - 1, y + 1), new Point(x, y - 1), new Point(x, y + 1), new Point(x + 1, y - 1), new Point(x + 1, y), new Point(x + 1, y + 1) };//соседи
            int weight1 = 0;
            Point optimum = new Point();
            foreach (Point p in n)//поиск наиболее выгодного передвижения
            {
                if (droids.ContainsKey(p))
                {
                    Point[] wn = { new Point(p.X - 1, p.Y - 1), new Point(p.X - 1, p.Y), new Point(p.X - 1, p.Y + 1), new Point(p.X, p.Y - 1), new Point(p.X, p.Y), new Point(p.X, p.Y + 1), new Point(p.X + 1, p.Y - 1), new Point(p.X + 1, p.Y), new Point(p.X + 1, p.Y + 1) };
                    int weight2 = 0;
                    foreach (Point w in wn)
                    {
                        if (droids.ContainsKey(w)) { weight2++; }
                    }
                    if (weight2 > weight1)
                    {
                        weight1 = weight2;
                        optimum = p;
                    }
                }
            }
            if (weight1 != 0)
            {
                move(optimum, ref canv);
                droids[optimum].health--;
            }
        }
    }

    public class Droid : Fighter
    {
        static public int amount;
        static public List<Point> components = new List<Point>();//группы дроидов
        public Droid(Point pos, ref Canvas canv)
        {
            isMovable = true;
            position = pos;
            health = 1;
            color = Colors.Wheat;
            size = 10;
            amount++;
            createCell(ref canv);
        }

        public static (int,int,int) dfs(List<Point> dots,List<int> colors, int v)//поиск в глубину
        {
            colors[v] = 1;
            var x = (int)dots[v].X;
            var y = (int)dots[v].Y;
            var k = 1;
            Point[] n = {new Point(x - 1, y - 1), new Point(x - 1, y), new Point(x - 1, y + 1), new Point(x, y - 1), new Point(x, y + 1), new Point(x + 1, y - 1), new Point(x + 1, y), new Point(x + 1, y + 1) };//соседи
            for (var i = 0; i < 8; i++)
            {
                if (dots.Contains(n[i]) && colors[dots.IndexOf(n[i])] == 0)
                {
                    var newPos = dfs(dots, colors, dots.IndexOf(n[i]));
                    x += newPos.Item1;
                    y += newPos.Item2;
                    k += newPos.Item3;
                }
            }
            colors[v] = 2;
            return ((int)x, (int)y, k);
        }

        public static void findComponents(ref Dictionary<Point, Droid> droids)
        {
            components.Clear();
            var dots = droids.Keys.ToList();
            List<int> colors = new List<int>(amount);
            for (int i = 0; i < amount; i++)
            {
                colors.Add(0);
            }
            for (int i = 0; i < amount; i++)
            {
                if (colors[i] == 0)
                {
                    var center = dfs(dots, colors, i);
                    components.Add(new Point(Math.Round((double)center.Item1 / center.Item3), Math.Round((double)center.Item2 / center.Item3)));
                    //droidsAmount += center.k;
                }
            }
        }

        public void process(ref Canvas canv, ref Dictionary<Point, Droid> droids, ref Dictionary<Point, Jedi> jedis)
        {
            int x = (int)this.position.X; int y = (int)this.position.Y;
            Point[] n = { new Point(x - 1, y - 1), new Point(x - 1, y), new Point(x - 1, y + 1), new Point(x, y - 1), new Point(x, y + 1), new Point(x + 1, y - 1), new Point(x + 1, y), new Point(x + 1, y + 1) };//соседи
            int ncount = 0;//число соседей
            for (var i = 0; i < 8; i++)
            {
                if (droids.ContainsKey(n[i]))
                {
                    ncount++;
                }
            }
            if (ncount < 3)
            {
                double minDist = 10000000;
                List<Point> variants = new List<Point>();
                foreach (Point po in components)
                {
                    if (po.X != this.position.X && po.Y != this.position.Y) {
				        var dist = distance(this.position, po);
				        if (dist < minDist) {
                            variants.Clear();
					        variants.Add(po);
					        minDist = dist;
				        } else if (dist == minDist){
					        variants.Add(po);}
			        }
                }
                if (variants.Count != 0)
                {
                    Random rnd = new Random();
                    int po = rnd.Next(0,variants.Count);
                    moveToTarget(variants[po], ref canv, ref droids, ref jedis, true);
                }
            }
        }

        public void attack(ref Canvas canv, ref Dictionary<Point, Droid> droids, ref Dictionary<Point, Jedi> jedis, ref List<Shot> shots)
        {
            int view = 28;//дальность обнаружения врага
            List<Point> variants = new List<Point>();
            double minDist = 10000000;
            foreach (KeyValuePair<Point, Jedi> j in jedis)
            {
               var dist = distance(this.position, j.Value.position);
               if ((dist < minDist)  && (dist <= view))
               {
                    bool flag = true;//не должно быть дроидов на расстоянии 5 клеток
                    int newx = (int)position.X, newy = (int)position.Y;
                    for (int i = 0; i < 5; i++)
                    {
                        double deltaX = (int)j.Value.position.X - newx;
                        double deltaY = (int)j.Value.position.Y - newy;
                        var scope = (double)deltaY / deltaX;

                        if ((deltaX == 0) || (deltaY == 0))
                        {
                            newx = newx + (deltaX == 0 ? 0 : (int)(deltaX / Math.Abs(deltaX)));
                            newy = newy + (deltaY == 0 ? 0 : (int)(deltaY / Math.Abs(deltaY)));
                        }
                        else
                        {
                            if (Math.Abs(scope) >= 1)
                            {
                                newy = newy + (int)(deltaY / Math.Abs(deltaY));
                                newx = newy + (int)Math.Round((newy - (int)newy) / (scope));
                            }
                            else
                            {
                                newx = newx + (int)(deltaX / Math.Abs(deltaX));
                                newy = newy + (int)Math.Round((newx - newx) * (scope));
                            }
                        }
                        if (droids.ContainsKey(new Point(newx, newy)))//есть ли дроид?
                        {
                            flag = false;
                            break;
                        }

                    }
                    if (flag)
                    {
                        variants.Clear();
                        variants.Add(j.Value.position);
                        minDist = dist;
                    }
               }
               else if (dist == minDist)
               {
                    bool flag = true;//не должно быть дроидов на расстоянии 5 клеток
                    int newx = (int)position.X, newy = (int)position.Y;
                    for (int i = 0; i < 5; i++)
                    {
                        double deltaX = (int)j.Value.position.X - newx;
                        double deltaY = (int)j.Value.position.Y - newy;
                        var scope = (double)deltaY / deltaX;

                        if ((deltaX == 0) || (deltaY == 0))
                        {
                            newx = newx + (deltaX == 0 ? 0 : (int)(deltaX / Math.Abs(deltaX)));
                            newy = newy + (deltaY == 0 ? 0 : (int)(deltaY / Math.Abs(deltaY)));
                        }
                        else
                        {
                            if (Math.Abs(scope) >= 1)
                            {
                                newy = newy + (int)(deltaY / Math.Abs(deltaY));
                                newx = newy + (int)Math.Round((newy - (int)newy) / (scope));
                            }
                            else
                            {
                                newx = newx + (int)(deltaX / Math.Abs(deltaX));
                                newy = newy + (int)Math.Round((newx - newx) * (scope));
                            }
                        }
                        if (droids.ContainsKey(new Point(newx, newy)))//есть ли дроид?
                        {
                            flag = false;
                            break;
                        }

                    }
                    if (flag)
                    {
                        variants.Add(j.Value.position);
                    }
               }
                
            }
            int ver = MainWindow.rnd1.Next(0, 10);//вероятность совершения выстрела
            if (variants.Count != 0 && ver > 5)
            {
                Random rnd2 = new Random();
                int po = rnd2.Next(0, variants.Count);
                shots.Add(new Shot(position, variants[po], ref canv));
            }


        }
    }

    public class Shot : CellObject
    {   
        public int age;
        public static int maxAge = 32;
        public int velocity = 2;
        public Point target;
        public Shot(Point pos, Point targ, ref Canvas canv)
        {
            isMovable = true;
            position.X = pos.X * 10;
            position.Y = (pos.Y+4) * 10;
            target = targ;
            color = Colors.Red;
            size = 10;
            createCell(ref canv);
            age = 0;
            while (distance(target, position) <= maxAge)//чтобы снаряд не останавливался долетев до цели
            {
                target.X += target.X - position.X;
                target.Y += target.Y - position.Y;
            }
        }

        public void process(ref Canvas canv, ref Dictionary<Point, Droid> droids, ref Dictionary<Point, Jedi> jedis)
        {
            for (int i = 0; i<velocity; i++)
            {
                this.moveToTarget(target, ref canv, ref droids, ref jedis, false);
                if (droids.ContainsKey(position))
                {
                    droids[position].health --;
                    age = maxAge;
                }
                int x = (int)position.X; int y = (int)position.Y;
                Point[] n = { new Point(x - 1, y - 1), new Point(x - 1, y), new Point(x - 1, y + 1), new Point(x, y - 1), new Point(x, y), new Point(x, y + 1), new Point(x + 1, y - 1), new Point(x + 1, y), new Point(x + 1, y + 1) };//соседи
                foreach (Point q in n)
                {
                    if (jedis.ContainsKey(q))
                    {
                        jedis[q].health--;
                        age = maxAge;
                    }
                }
                age++;
            }
        }
    }
}
