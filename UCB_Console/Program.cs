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
            var maxCountThreads = 6;

            if (!Directory.Exists(pathSave))
                throw new Exception("Указан несуществующий путь сохранения");

            Bandit.MathExp = 0.5d;
            Bandit.MaxDispersion = 0.25d;
            Bandit.NumberSimulations = 200000;
            Bandit.SetDeviation(0d, 0.3d, 101);

            //Изменение времени обновления

            var parameter = new double[] { 0.62, 0.66, 0.67, 0.67 };
            var count = parameter.Length;
            var timeChangeBatch = Enumerable.Range(1, count).Select(x => x * 5).ToArray();

            var arms = Enumerable.Repeat(2, count).ToArray();
            var horizons = Enumerable.Repeat(5000, count).ToArray();

            var startBatchSize = Enumerable.Repeat(50, count).ToArray();
            var alpha = Enumerable.Repeat(1.5d, count).ToArray();

            var simulation = new Simulation(maxCountThreads);

            simulation.Run(arms, startBatchSize, horizons, parameter, alpha, timeChangeBatch);

            if (!Directory.Exists(pathSave))
                Directory.CreateDirectory(pathSave);

            simulation.Save(pathSave);
        }
    }
}