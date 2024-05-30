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
            var deviationStart = 0.9d;
            var deviationDelta = 0.3d;
            var deviationCount = 7;

            var deviations = Enumerable.Range(0, deviationCount).Select(x => Math.Round(deviationStart + x * deviationDelta, 1)).ToArray();

            //Параметр стратегии a
            var parameterStart = 0.45d;
            var parameterDelta = 0.01d;
            var parameterCount = 30;

            //Настройки симуляций
            var countGames = 200000;
            var countThreads = 6;

            //Данные бандита
            var countArms = 2;
            var startBatchSize = 50;
            var numberBatches = 50;
            var timeChangeBatch = 10;
            var alpha = 1.0d;

            //Массивы для бандитов
            var countsArms = Enumerable.Repeat(countArms, parameterCount).ToArray();
            var numbersBatches = Enumerable.Repeat(numberBatches, parameterCount).ToArray();
            var startsBatchSize = Enumerable.Repeat(startBatchSize, parameterCount).ToArray();
            var parameters = Enumerable.Range(0, parameterCount).Select(i => Math.Round(parameterStart + i * parameterDelta, 2)).ToArray();
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