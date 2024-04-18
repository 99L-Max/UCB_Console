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
            var maxCountThreads = 5;

            if (!Directory.Exists(pathSave))
                throw new Exception("Указан несуществующий путь сохранения");

            Bandit.MathExp = 0.5d;
            Bandit.MaxDispersion = 0.25d;
            Bandit.NumberSimulations = 200000;
            Bandit.SetDeviation(0.9d, 0.3d, 7);

            //Измение альфы
            var batchSizes = Enumerable.Range(1, 8).Select(x => x * 25).ToArray();

            foreach (var size in batchSizes)
            {
                Console.Clear();

                var a0 = Math.Round(-0.2d / 175 * size + (0.25d - 200 * (-0.2d / 175)), 2);
                var da = 0.01d;
                var count = 30;

                var arms = Enumerable.Repeat(2, count).ToArray();
                var horizons = Enumerable.Repeat(5000, count).ToArray();
                var parameter = Enumerable.Range(0, count).Select(x => Math.Round(a0 + x * da, 2)).ToArray();

                var startBatchSize = Enumerable.Repeat(size, count).ToArray();
                var timeChangeBatch = Enumerable.Repeat(10, count).ToArray();
                var alpha = Enumerable.Repeat(1.5d, count).ToArray();

                var simulation = new Simulation(maxCountThreads);

                simulation.Run(arms, startBatchSize, horizons, parameter, alpha, timeChangeBatch);

                if (!Directory.Exists(pathSave))
                    Directory.CreateDirectory(pathSave);

                simulation.Save(pathSave);
            }
        }
    }
}