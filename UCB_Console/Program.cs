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
            Bandit.SetDeviation(0d, 0.3d, 101);

            var parameter = new double[] { 0.70, 0.65, 0.61, 0.56, 0.53, 0.50, 0.47, 0.44 };
            var count = parameter.Length;

            var arms = Enumerable.Repeat(2, count).ToArray();
            var horizons = Enumerable.Repeat(5000, count).ToArray();

            var startBatchSize = Enumerable.Range(1, count).Select(x => x * 25).ToArray();
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