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

            //Общие параметры
            var rule = RuleChangeBatch.Alpha;
            var expectation = 0.5d;
            var variance = 0.25d;

            //Отклонения d
            var deviationStart = 0.0d;
            var deviationDelta = 0.3d;
            var deviationCount = 101;

            var deviations = Enumerable.Range(0, deviationCount).Select(x => Math.Round(deviationStart + x * deviationDelta, 1)).ToArray();

            //Параметр стратегии a
            var parameterCount = 4;

            //Настройки симуляций
            var countGames = 200000;
            var countThreads = 5;

            //Данные бандита
            var countArms = 2;
            var numberBatches = 50;
            var timeChangeBatch = 10;
            var alpha = 1.5d;

            //Массивы для бандитов
            var countsArms = Enumerable.Repeat(countArms, parameterCount).ToArray();
            var numbersBatches = Enumerable.Repeat(numberBatches, parameterCount).ToArray();
            var startsBatchSize = new int[] { 50, 100, 150, 200 };
            var parameters = new double[] { 0.67, 0.64, 0.63, 0.63 };
            var alphas = Enumerable.Repeat(alpha, parameterCount).ToArray();
            var timesChangeBatch = Enumerable.Repeat(timeChangeBatch, parameterCount).ToArray();

            var player = new Player(rule, expectation, variance, countsArms, numbersBatches, startsBatchSize, parameters, alphas, timesChangeBatch);

            player.Play(deviations, countGames, countThreads);

            Console.Clear();
            Console.WriteLine(player.GameResult);

            if (!Directory.Exists(pathSave))
                Directory.CreateDirectory(pathSave);

            player.Save(pathSave);
        }
    }
}