using System;

namespace Lab1
{
    class Program
    {
        static double Discr(double a, double b, double c)
        {
            return (b*b - 4*a*c);
        }
        static void Main(string[] args)
        {
            Console.Write("Кузьмин Роман Александрович, ИУ5-34Б\n\n");
            double a, b, c;
            string a_s, b_s, c_s;//коэф-ы считываются в строки, а затем проверяется их корректность
            if (args.Length != 0) { a_s = args[0]; b_s = args[1]; c_s = args[2]; }//аргументы из командной строки (если есть)
            else
            {
                Console.Write("Введите коэффициент A: ");
                a_s = Console.ReadLine();
                Console.Write("Введите коэффициент B: ");
                b_s = Console.ReadLine();
                Console.Write("Введите коэффициент C: ");
                c_s = Console.ReadLine();
            }
            //============ проверка корректности ================
            while (!Double.TryParse(a_s, out a) || Double.TryParse(a_s, out a) && Convert.ToDouble(a_s)==0)
            {
                Console.Write("Недопустимый ввод.\nВведите коэффициент A: ");
                a_s = Console.ReadLine();
            }
            a = Convert.ToDouble(a_s);

            while (!Double.TryParse(b_s, out b))
            {
                Console.Write("Недопустимый ввод.\nВведите коэффициент B: ");
                b_s = Console.ReadLine();
            }
            b = Convert.ToDouble(b_s);

            while (!Double.TryParse(c_s, out c))
            {
                Console.Write("Недопустимый ввод.\nВведите коэффициент C: ");
                c_s = Console.ReadLine();
            }
            c = Convert.ToDouble(c_s);
            //======================== вычисления =============================
            double d = Discr(a, b, c);//дискриминант

            if ((d < 0) || (d>0) && ((-b - Math.Sqrt(d)) / (2 * a)<0) && ((-b + Math.Sqrt(d)) / (2 * a) < 0) || (d==0) && ((-b / (2 * a))<0))
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("Корней нет");
                Console.ResetColor();
            } else if (d == 0 && ((-b / (2 * a)) == 0) || (d > 0) && ((-b - Math.Sqrt(d)) / (2 * a) < 0) && ((-b + Math.Sqrt(d)) / (2 * a) == 0))
            {
                Console.Write("Уравнение имеет один корень: ");
                Console.ForegroundColor = ConsoleColor.Green;
                Console.Write(0);
                Console.ResetColor();
            } else if (d == 0)
            {
                Console.Write("Уравнение имеет два корня: ");
                Console.ForegroundColor = ConsoleColor.Green;
                Console.Write("{0} {1}", -Math.Sqrt(-b / (2 * a)), Math.Sqrt(-b / (2 * a)));
                Console.ResetColor();
            }
            else if ((d > 0) && ((-b - Math.Sqrt(d)) / (2 * a) < 0) && ((-b + Math.Sqrt(d)) / (2 * a) > 0))
            {
                Console.Write("Уравнение имеет два корня: ");
                Console.ForegroundColor = ConsoleColor.Green;
                Console.Write("{0} {1}", -Math.Sqrt((-b + Math.Sqrt(d)) / (2 * a)), Math.Sqrt((-b + Math.Sqrt(d)) / (2 * a)));
                Console.ResetColor();
            } else if ((d > 0) && ((-b - Math.Sqrt(d)) / (2 * a) == 0) && ((-b + Math.Sqrt(d)) / (2 * a) > 0))
            {
                Console.Write("Уравнение имеет три корня: ");
                Console.ForegroundColor = ConsoleColor.Green;
                Console.Write("{0} 0 {1}", -Math.Sqrt((-b + Math.Sqrt(d)) / (2 * a)), Math.Sqrt((-b + Math.Sqrt(d)) / (2 * a)));
                Console.ResetColor();
            } else
            {
                Console.Write("Уравнение имеет четыре корня: ");
                Console.ForegroundColor = ConsoleColor.Green;
                Console.Write("{0} {1} {2} {3}", -Math.Sqrt((-b - Math.Sqrt(d)) / (2 * a)), Math.Sqrt((-b - Math.Sqrt(d)) / (2 * a)), -Math.Sqrt((-b + Math.Sqrt(d)) / (2 * a)), Math.Sqrt((-b + Math.Sqrt(d)) / (2 * a)));
                Console.ResetColor();
            }
        }
    }
}
