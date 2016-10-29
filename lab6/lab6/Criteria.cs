using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace lab6
{
    public enum Distribution { Normal, Uniform }

    public class Criteria
    {
        public List<Intervals> list = new List<Intervals>(); // інтервали для норм. розподілу
        public Dictionary<double, int> unique = new Dictionary<double, int>(); // унікальні значення та їхні частоти для рівном. розподілу
        public Distribution type = new Distribution(); // тип розподілу
        public double dispertion; // дисперсія
        public double average; // середнє значення
        public double[] array; // вибірка
        public Dictionary<double, double> Laplas = new Dictionary<double, double>();
        public string filename;

        public int r; // кількість інтервалів для норм. розподілу
        public double[] z1; // змінна для функції Лапласа по лівій межі інтервалу
        public double[] z2; // змінна для функції Лапласа по правій межі інтервалу
        public double[] Laplace_z1; /*= new double[5] { -0.5, -0.3264, -0.0753, 0.2123, 0.4049 };*/ // лаплас для лівих меж
        public double[] Laplace_z2; /* = new double[5] { -0.3264, -0.0753, 0.2123, 0.4049, 0.5 };*/ // лаплас для правих меж
        public double[] p; // теоретична ймовірність для інтервалу
        public double[] ni; // теоретичні частоти для інтервалів

        public double criteriaValue = 0; // емпіричний критерій Пірсона
        public double criticalNormal; // критичний критерій Пірсона для норм. розп., альфа = 0,01 (0,99 в таблиці)
        public double criticalUniform; // критичний критерій Пірсона для рівном. розп., альфа = 0,1 (0,9 в таблиці)


        // Конструктор для норм. розподілу
        public Criteria(double[] array, Distribution type, int r, string filename)
        {
            Array.Sort(array);
            this.array = new double[array.Length];
            for (int i = 0; i < array.Length; i++)
            {
                this.array[i] = array[i];
            }
            this.r = r;
            this.dispertion = Dispertion(this.array);
            this.average = this.array.Average();
            this.type = type;
            this.filename = filename;
            
        }

        // Конструктор для рівномірного розподілу
        public Criteria(double[] array, Distribution type)
        {
            Array.Sort(array);
            this.array = new double[array.Length];
            for (int i = 0; i < array.Length; i++)
            {
                this.array[i] = array[i];
            }
            this.type = type;
        }

        // Перевірка гіпотези про нормальний розподіл
        public void Verify()
        {
            if (this.type == Distribution.Normal)
            {
                LoadFromFile(filename);
                CreateIntervals();
                Evaluate_Mi();
                CorrectIntervals();
                Evaluate_Mi();
                Evaluate_z1_2();
                Evaluate_Laplace();
                Evaluate_p();
                Evaluate_ni();
                Evaluate_Criteria();
                ShowStupin();
                Console.WriteLine("Введiть критичний критерiй для обрахованої ступенi свободи: ");
                setCritical(Convert.ToDouble(Console.ReadLine()));

                Console.WriteLine("Критерiй емпiричний: " + criteriaValue + ", критерiй критичний: " + criticalNormal);

                if (criteriaValue > criticalNormal)
                    Console.WriteLine("Розподiл далекий вiд нормального");
                else
                    Console.WriteLine("Розподiл близький до нормального");
            }

            if (this.type == Distribution.Uniform)
            {
                UniqueValues();
                Evaluate_p();
                Evaluate_ni();
                Evaluate_Criteria();
                ShowStupin();
                Console.WriteLine("Введiть критичний критерiй для обрахованої ступенi свободи: ");
                setCritical(Convert.ToDouble(Console.ReadLine()));

                Console.WriteLine("Критерiй емпiричний: " + criteriaValue + ", критерiй критичний: " + criticalUniform );
                if (criteriaValue > criticalUniform)
                    Console.WriteLine("Розподiл далекий вiд рiвномiрного");
                
                else
                    Console.WriteLine("Розподiл близький до рiвномiрного");     
            }

        }

        // Визначення інтервалів
        public void CreateIntervals()
        {
            double minValue = array.Min();
            double maxValue = array.Max();
            double interval = (maxValue - minValue) / (double)r; // величина інтервалу
            for (double i = minValue; i < maxValue; i += interval)
            {
                if (i == maxValue) break;
                
                Intervals obj = new Intervals();
                obj.start = i;
                obj.end = i + interval;
                list.Add(obj);
            }
        }

        // Коригування інтервалів
        public void CorrectIntervals()
        {
            int permission = 0;
            do
            {
                Evaluate_Mi();
               
                for (int i = 0; i < list.Count; i++)
                {


                    if (list.ElementAt(i).mi < 5)
                    {
                        if (i == list.Count - 1)
                        { 
                            list.ElementAt(i - 1).end = list.ElementAt(i).end;
                            list.RemoveAt(i);
                        }
                        else
                        {
                            list.ElementAt(i+1).start = list.ElementAt(i).start;
                            list.RemoveAt(i);
                        }
                        permission = 0;
                        break;
                    }
                    else
                        permission = 1;
                }
            } while (permission == 0);
        }

        // Обчислення частот вибіркових елементів
        public void Evaluate_Mi()
        {
            for(int i =0; i < list.Count; i++)
            {
                list.ElementAt(i).mi = 0;
            }
            for (int i = 0; i < list.Count; i++)
            {
                    for (int j = 0; j < array.Length; j++)
                    {
                        if (list.ElementAt(i).start <= array[j] && array[j] <= list.ElementAt(i).end)
                            list.ElementAt(i).mi++;
                    }
            }
            
        }

        // Обчислення дисперсії
        public double Dispertion(double[] array)
        {
            double average = array.Average();
            double sum = 0.0;
            for (int i = 0; i < array.Length; i++)
                sum += Math.Pow((array[i] - average), 2);
            return sum / array.Length;
        }

        // Обчислення вхідних значень для ф-цій Лапласа
        public void Evaluate_z1_2()
        {
            z1 = new double[list.Count];
            z2 = new double[list.Count];

            for (int i = 0; i < list.Count; i++)
            {
                z1[i] = Math.Round((list.ElementAt(i).start - average) / Math.Sqrt(dispertion), 2);
                z2[i] = Math.Round((list.ElementAt(i).end - average) / Math.Sqrt(dispertion), 2);
            }
        }

        // Обчислення значень функції Лапласа
        public void Evaluate_Laplace()
        {
            Laplace_z1 = new double[z1.Length];
            Laplace_z2 = new double[z2.Length];

            for(int i = 0; i < list.Count; i++)
            {
                if(z1[i] < 0)
                    Laplace_z1[i] = -1*Laplas[-z1[i]];
                else
                    Laplace_z1[i] = Laplas[z1[i]];
                if (z2[i] < 0)
                    Laplace_z2[i] = -1 * Laplas[-z2[i]];
                else
                    Laplace_z2[i] = Laplas[z2[i]];
            }

            Laplace_z1[0] = -0.5;
            Laplace_z2[z2.Length-1] = 0.5;
        }

        // Обчилення теоретичних ймовірностей для норм. розподілу
        public void Evaluate_p()
        {
            if (this.type == Distribution.Normal)
            {
                p = new double[list.Count];
                for (int i = 0; i < p.Length; i++)
                {
                    p[i] = Laplace_z2[i] - Laplace_z1[i];
                }
            }

            if (this.type == Distribution.Uniform)
            {
                p = new double[unique.Count];
                for (int i = 0; i < p.Length; i++)
                {
                    p[i] = 1.0 / unique.Count;
                }
            }
        }

        // Обчислення теоретичних частот
        public void Evaluate_ni()
        {
            ni = new double[p.Length];
            for (int i = 0; i < p.Length; i++)
            {
                ni[i] = array.Length * p[i];
            }
        }

        // Обчислення критерію Пірсона
        public void Evaluate_Criteria()
        {
            if (this.type == Distribution.Normal)
            {
                for (int i = 0; i < list.Count; i++)
                {
                    criteriaValue += Math.Pow((list.ElementAt(i).mi - ni[i]), 2) / ni[i];
                }
            }

            if(this.type == Distribution.Uniform)
            {
                for(int i = 0; i < unique.Count; i++)
                {
                    criteriaValue += Math.Pow(unique.ElementAt(i).Value - ni[i], 2) / ni[i];
                }
            }
        }

        // Пошук унікальних значень для рівном. розподілу та їхні частоти
        public void UniqueValues()
        {
            int step = 0;
            for (int i = 0; i < array.Length; i += step)
            {
                step = 0;
                for (int j = 0; j < array.Length; j++)
                {
                    if (array[i] == array[j])
                        step++;
                }
                unique.Add(array[i], step);
            }
        }

        public void LoadFromFile(string filename)
        {
            StreamReader reader = File.OpenText(filename);
            double x;
            double y;
            while (!reader.EndOfStream)
            {
                string[] line = reader.ReadLine().Split('\t');
                x = Convert.ToDouble(line[0]);
                y = Convert.ToDouble(line[1]);
                Laplas.Add(x, y);
            }
        }

        public void setCritical(double k)
        {
            if(this.type == Distribution.Normal)
                this.criticalNormal = k;
            if (this.type == Distribution.Uniform)
                this.criticalUniform = k;
        }

        public void ShowStupin()
        {

        Console.WriteLine("Обрахований ступiнь свободи: ");

            if (this.type == Distribution.Normal)
                Console.WriteLine(list.Count - 2 - 1);
            if (this.type == Distribution.Uniform)
                Console.WriteLine(unique.Count - 0 - 1);
        }

    }
}
