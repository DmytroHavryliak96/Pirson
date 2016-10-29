using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace lab6
{
    class Program
    {
        static void Main(string[] args)
        {
            
            double[] array2 = new double[28];
            for (int i = 0; i < array2.Length; i++)
            {
                bool result;
                do
                {
                    Console.WriteLine("Enter the integer value: ");
                    string value = Console.ReadLine();
                    int number;
                    result = Int32.TryParse(value, out number);
                    if (result)
                    {
                        array2[i] = Convert.ToDouble(number);
                    }
                    else
                    {
                        Console.WriteLine("Not integer! Retry please.");
                    }
                } while (!result);

            }

            Console.WriteLine("-----");
            Console.WriteLine("Створена вибiрка:");
            Console.WriteLine();
            for (int i = 0; i < array2.Length; i++)
            {
                Console.Write(array2[i] + " ");
            }

            Console.WriteLine();

            Distribution type = Distribution.Normal;

            int r = 5;// кількість інтервалів для норм. розподілу
            Criteria cr = new Criteria(array2, type, r, "Laplas.txt");

            cr.Verify();
            Console.WriteLine("------");

            type = Distribution.Uniform;
            Criteria cr2 = new Criteria(array2, type);
            cr2.Verify();

            Console.ReadLine();
        }
    }
}
