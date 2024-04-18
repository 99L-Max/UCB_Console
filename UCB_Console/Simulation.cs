using System;
using System.IO;
using System.Linq;
using System.Threading;

namespace UCB_Console
{
    class Simulation
    {
        private Thread[] _threads;
        private Bandit[] _bandits;
        private int _indexBandit = -1;
        private int _countProcessedPoints = -1;
        private int _countPoints;

        public readonly int MaxCountThreads;

        public Simulation(int maxCountThreads)
        {
            MaxCountThreads = maxCountThreads;
        }

        private void StartNextThread()
        {
            _indexBandit++;

            if (_indexBandit < _bandits.Length)
                _threads[_indexBandit].Start();
        }

        private void UpdateProgress()
        {
            _countProcessedPoints++;

            Console.Write($"\rВыполнено {_countProcessedPoints} / {_countPoints} ({_countProcessedPoints * 100 / _countPoints}%)");
        }

        private static bool CheckArraysLength(params Array[] arrays) =>
            arrays.All(arr => arr.Length == arrays[0].Length);

        public void Run(int[] countArms, int[] startBatchSize, int[] horizon, double[] parameter, double[] alpha, int[] timeChangeBatch)
        {
            if (!CheckArraysLength(countArms, startBatchSize, horizon, parameter, alpha, timeChangeBatch))
                throw new ArgumentException("Несовпадение размером массивов.");

            _bandits = new Bandit[countArms.Length];
            _threads = new Thread[countArms.Length];

            for (int i = 0; i < _bandits.Length; i++)
            {
                _bandits[i] = new Bandit(countArms[i], startBatchSize[i], horizon[i], parameter[i], alpha[i], timeChangeBatch[i]);

                _bandits[i].PointProcessed += UpdateProgress;
                _bandits[i].Finished += StartNextThread;

                _threads[i] = new Thread(_bandits[i].RunSimulation);
            }

            _countPoints = _bandits.Length * Bandit.NumberDeviations;

            int countThreads = Math.Min(MaxCountThreads, _bandits.Length);

            while (countThreads-- > 0)
                StartNextThread();

            UpdateProgress();

            foreach (var th in _threads)
                th.Join();
        }

        public void Save(string path)
        {
            var name = $"M - {_bandits[0].StartBatchSize}, T - {_bandits[0].TimeChangeBatch}, A - {_bandits[0].Alpha}";
            var time = $"{DateTime.Now:d} {DateTime.Now.Hour:d2}.{DateTime.Now.Minute:d2}.{DateTime.Now.Second:d2}";

            using StreamWriter writer = new(@$"{path}\{name} ({time}).txt");

            writer.Write("d");

            foreach (var b in _bandits)
                writer.Write(" " + b.Parameter);

            writer.WriteLine();

            for (int d = 0; d < Bandit.NumberDeviations; d++)
            {
                writer.Write(Bandit.GetDeviation(d));

                foreach (var b in _bandits)
                    writer.Write(" " + b.GetRegrets(d));

                writer.WriteLine();
            }
        }
    }
}
