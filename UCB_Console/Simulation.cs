using System;
using System.IO;
using System.Linq;
using System.Threading;

namespace UCB_Console
{
    class Simulation
    {
        private Timer timer;
        private Thread[] threads;
        private Bandit[] bandits;
        private int seconds;
        private int indexBandit = -1;
        private int countProcessedPoints = -1;
        private int countPoints;

        public readonly int MaxCountThreads;

        public Simulation(int maxCountThreads)
        {
            MaxCountThreads = maxCountThreads;

            TimerCallback tm = new(TimerTick);
            timer = new(tm, 0, 0, 1000);
        }

        private void TimerTick(object sender)
        {
            seconds++;

            Console.SetCursorPosition(0, 0);
            Console.WriteLine($"{seconds / 3600:d2}:{seconds / 60:d2}:{seconds % 60:d2}");
            Console.Write($"Выполнено {countProcessedPoints} / {countPoints} ({countProcessedPoints * 100 / countPoints}%)");
        }

        private void StartNextThread()
        {
            indexBandit++;

            if (indexBandit < bandits.Length)
                threads[indexBandit].Start();
        }

        private void UpdateProgress() => countProcessedPoints++;

        private bool CheckArraysLength(params Array[] arrays)
        {
            int length = arrays[0].Length;
            return arrays.All(arr => arr.Length == length);
        }

        public void Run(int[] countArms, int[] batchSize, int[] numberBatches, double[] parameter)
        {
            if (!CheckArraysLength(countArms, batchSize, numberBatches, parameter))
                throw new ArgumentException("The size of the arrays does not match.");

            bandits = new Bandit[countArms.Length];
            threads = new Thread[countArms.Length];

            for (int i = 0; i < bandits.Length; i++)
            {
                bandits[i] = new Bandit(countArms[i], batchSize[i], numberBatches[i], parameter[i]);

                bandits[i].PointProcessed += UpdateProgress;
                bandits[i].Finished += StartNextThread;

                threads[i] = new Thread(bandits[i].RunSimulation);
            }

            countPoints = bandits.Length * Bandit.NumberDeviations;

            int countThreads = Math.Min(MaxCountThreads, bandits.Length);

            while (countThreads-- > 0)
                StartNextThread();

            UpdateProgress();

            foreach (var th in threads)
                th.Join();
        }

        public void Save(string path)
        {
            string time = $"{DateTime.Now:d}_{DateTime.Now.Hour:d2}.{DateTime.Now.Minute:d2}.{DateTime.Now.Second:d2}";

            using StreamWriter writer = new(@$"{path}\result_({time}).txt");

            writer.Write("d");

            foreach (var b in bandits)
                writer.Write(" " + b.Parameter);

            writer.WriteLine();

            for (int d = 0; d < Bandit.NumberDeviations; d++)
            {
                writer.Write(Bandit.GetDeviation(d));

                foreach (var b in bandits)
                    writer.Write(" " + b.GetRegrets(d));

                writer.WriteLine();
            }
        }
    }
}
