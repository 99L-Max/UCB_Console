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
            var alphas = Enumerable.Range(10, 11).Select(x => x * 0.1d).ToArray();

            foreach (var alp in alphas)
            {
                Console.Clear();

                var a0 = 0.45d;
                var da = 0.01d;
                var count = 30;

                var arms = Enumerable.Repeat(2, count).ToArray();
                var horizons = Enumerable.Repeat(5000, count).ToArray();
                var parameter = Enumerable.Range(0, count).Select(x => Math.Round(a0 + x * da, 2)).ToArray();

                var startBatchSize = Enumerable.Repeat(100, count).ToArray();
                var timeChangeBatch = Enumerable.Repeat(10, count).ToArray();
                var alpha = Enumerable.Repeat(alp, count).ToArray();

                var simulation = new Simulation(maxCountThreads);

                simulation.Run(arms, startBatchSize, horizons, parameter, alpha, timeChangeBatch);

                if (!Directory.Exists(pathSave))
                    Directory.CreateDirectory(pathSave);

                simulation.Save(pathSave);
            }
        }
    }
}
