using System;
using System.IO;
using System.Linq;

namespace UCB_Console
{
    class Program
    {
        static void Main(string[] args)
        {
            string pathSave = @"E:\НовГУ\2) Магистратура\1 курс\Научная деятельность\Результаты\10) Переменный размер пакета\TXT";

            if (!Directory.Exists(pathSave))
                throw new Exception("Указан несуществующий путь сохранения");

            Bandit.MathExp = 0.5d;
            Bandit.MaxDispersion = 0.25d;
            Bandit.NumberSimulations = 400000;
            Bandit.SetDeviation(1.2d, 0.3d, 7);

            var a0 = 0.75d;
            var da = 0.01d;
            var count = 5;

            var arms = Enumerable.Repeat(2, count).ToArray();
            var startBatchSize = Enumerable.Repeat(100, count).ToArray();
            var horizons = Enumerable.Repeat(5000, count).ToArray();
            var parameters = Enumerable.Range(0, count).Select(i => Math.Round(a0 + i * da, 2)).ToArray();
            var alphas = Enumerable.Repeat(1.5, count).ToArray();
            var timeChangeBatch = Enumerable.Repeat(10, count).ToArray();

            Simulation simulation = new Simulation(5);
            simulation.Run(arms, startBatchSize, horizons, parameters, alphas, timeChangeBatch);

            if (!Directory.Exists(pathSave))
                Directory.CreateDirectory(pathSave);

            simulation.Save(pathSave);
        }
    }
}