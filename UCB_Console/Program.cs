using System;
using System.IO;
using System.Linq;

namespace UCB_Console
{
    class Program
    {
        static void Main(string[] args)
        {
            var pathSave = @"E:\НовГУ\2) Магистратура\1 курс\Научная деятельность\Результаты\10) Переменный размер пакета\TXT";

            if (!Directory.Exists(pathSave))
                throw new Exception("Указан несуществующий путь сохранения");

            Bandit.MathExp = 0.5d;
            Bandit.MaxDispersion = 0.25d;
            Bandit.NumberSimulations = 400000;
            Bandit.SetDeviation(0.9d, 0.3d, 7);

            var maxCountThreads = 5;

            var a0 = 0.45d;
            var da = 0.01d;
            var count = 30;

            var arrays = new (int[], int[], double[])[]
            {
                (
                    Enumerable.Range(1, 10).Select(x => x * 10).ToArray(),
                    Enumerable.Repeat(10, 10).ToArray(),
                    Enumerable.Repeat(1.5d, 10).ToArray()
                ),

                (
                    Enumerable.Repeat(50, 5).ToArray(),
                    Enumerable.Range(1, 5).Select(x => x * 5).ToArray(),
                    Enumerable.Repeat(1.5d, 5).ToArray()
                ),

                (
                    Enumerable.Repeat(50, 11).ToArray(),
                    Enumerable.Repeat(10, 11).ToArray(),
                    Enumerable.Range(10, 11).Select(x => x * 0.1d).ToArray()
                ),
            };

            foreach (var arr in arrays)
            {
                var (size, time, alpha) = arr;

                for (int i = 0; i < size.Length; i++)
                    RunSimulations(maxCountThreads, count, 2, 5000, a0, da, size[i], time[i], alpha[i], pathSave);
            }
        }

        static void RunSimulations(int maxCountThreads, int countObjects, int arm, int horizon, double a0, double da, int startBatchSize, int timeChangeBatch, double alpha, string pathSave)
        {
            Console.Clear();

            var arms = Enumerable.Repeat(arm, countObjects).ToArray();
            var horizons = Enumerable.Repeat(horizon, countObjects).ToArray();
            var parameter = Enumerable.Range(0, countObjects).Select(x => Math.Round(a0 + x * da, 2)).ToArray();

            var startBatchSizes = Enumerable.Repeat(startBatchSize, countObjects).ToArray();
            var timeChangeBatches = Enumerable.Repeat(timeChangeBatch, countObjects).ToArray();
            var alphas = Enumerable.Repeat(alpha, countObjects).ToArray();

            var simulation = new Simulation(maxCountThreads);

            simulation.Run(arms, startBatchSizes, horizons, parameter, alphas, timeChangeBatches);

            if (!Directory.Exists(pathSave))
                Directory.CreateDirectory(pathSave);

            simulation.Save(pathSave);
        }
    }
}
