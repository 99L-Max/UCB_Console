using System;
using System.IO;
using System.Linq;

namespace UCB_Console
{
    class Program
    {
        static void Main(string[] args)
        {
            string pathSave = @"E:\НовГУ\2) Магистратура\1 курс\Научная деятельность\Результаты\10) Переменный размер пакета";

            if (!Directory.Exists(pathSave))
                throw new Exception("Указан несуществующий путь сохранения");

            Bandit.MathExp = 0.5d;
            Bandit.MaxDispersion = 0.25d;
            Bandit.NumberSimulations = 100000;
            Bandit.SetDeviation(1.2d, 0.3d, 5);

            double a0 = 0.75d;
            double da = 0.01d;
            int count = 30;

            int[] arms = Enumerable.Repeat(2, count).ToArray();
            int[] numberBatches = Enumerable.Repeat(50, count).ToArray();
            int[] batchSize = Enumerable.Repeat(100, count).ToArray();
            double[] a = Enumerable.Range(0, count).Select(i => Math.Round(a0 + i * da, 2)).ToArray();

            Simulation simulation = new Simulation(5);
            simulation.Run(arms, batchSize, numberBatches, a);

            if (!Directory.Exists(pathSave))
                Directory.CreateDirectory(pathSave);

            simulation.Save(pathSave);
        }
    }
}
