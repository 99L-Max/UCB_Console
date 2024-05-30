using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;

namespace UCB_Console
{
    class Player
    {
        private readonly Bandit[] _bandits;
        private readonly Dictionary<Bandit, Thread> _threads = new();

        private double[] _deviations;
        private int _countProcessedBandits;
        private int _countProcessedPoints;
        private int _totalCountPoints;
        private Queue<(Bandit, Thread)> _waitingBandits;

        public readonly double Expectation;
        public readonly double MaxVariance;

        public RegretTable RegretTable { get; private set; }

        public int PercentProgress { get; private set; }

        public string GameResult { get; private set; }

        public double[] Deviations =>
            (double[])_deviations.Clone();

        public int BanditsCount =>
            _bandits.Length;

        public string GameInformation =>
            $"Обработано точек: {_countProcessedPoints} / {_totalCountPoints}\n" +
            $"Обработано бандитов: {_countProcessedBandits} / {_bandits.Length}\n" +
            $"Выполнено: {PercentProgress}%";

        public Player(RuleChangeBatch rule, double expectation, double maxVariance, int[] countArms, int[] numberBatches, int[] startBatchSize, double[] parameters, double[] alphas, int[] timesChangeBatch)
        {
            if (!CheckArraysLength(countArms, numberBatches, startBatchSize, parameters, alphas, timesChangeBatch))
                throw new ArgumentException("Mismatch of array sizes.");

            Expectation = expectation;
            MaxVariance = maxVariance;

            _bandits = new Bandit[countArms.Length];

            for (int i = 0; i < _bandits.Length; i++)
                _bandits[i] = new Bandit(rule, expectation, maxVariance, countArms[i], numberBatches[i], startBatchSize[i], parameters[i], alphas[i], timesChangeBatch[i]);
        }

        private static bool CheckArraysLength(params Array[] arrays) =>
            arrays.All(arr => arr.Length == arrays[0].Length);

        private void UpdateProgress()
        { 
            PercentProgress = ++_countProcessedPoints * 100 / _totalCountPoints;
            Console.Clear();
            Console.WriteLine(GameInformation);
        }

        private void StartThread()
        {
            if (_waitingBandits.Count > 0)
            {
                var (bandit, thread) = _waitingBandits.Dequeue();

                bandit.PointProcessed += UpdateProgress;
                bandit.GameOver += FinishThread;

                _threads.Add(bandit, thread);
                thread.Start();
            }
        }

        private void FinishThread(Bandit sender)
        {
            _countProcessedBandits++;

            sender.PointProcessed -= UpdateProgress;
            sender.GameOver -= FinishThread;

            _threads.Remove(sender);

            if (_threads.Count > 0)
            {
                StartThread();
            }
            else
            {
                RegretTable = new RegretTable(_deviations, _bandits);

                GameResult =
                    $"a = {_bandits[RegretTable.IndexMinMax].Parameter:f2}\n" +
                    $"l_max = {_bandits[RegretTable.IndexMinMax].MaxRegrets:f2}\n" +
                    $"d_max = {_bandits[RegretTable.IndexMinMax].MaxDeviation:f1}";
            }
        }

        public void Play(double[] deviations, int countGames, int countThreads)
        {
            PercentProgress = 0;
            GameResult = string.Empty;

            var threads = _bandits.Select(b => new Thread(() => b.Play(deviations, countGames))).ToArray();

            _deviations = (double[])deviations.Clone();
            _countProcessedBandits = _countProcessedPoints = 0;
            _totalCountPoints = deviations.Length * _bandits.Length;
            _waitingBandits = new Queue<(Bandit, Thread)>();

            for (int i = 0; i < _bandits.Length; i++)
                _waitingBandits.Enqueue((_bandits[i], threads[i]));

            int maxCountThreads = Math.Min(countThreads, _waitingBandits.Count);

            while (maxCountThreads-- > 0)
                StartThread();

            Console.WriteLine(GameInformation);

            foreach (var th in threads)
                th.Join();
        }

        public void Save(string path)
        {
            var name = $"N - {_bandits[0].Horizon}, M0 - {_bandits[0].StartBatchSize}, T - {_bandits[0].TimeChangeBatch}, A - {_bandits[0].Alpha}";
            var time = $"{DateTime.Now:d} {DateTime.Now.Hour:d2}.{DateTime.Now.Minute:d2}.{DateTime.Now.Second:d2}";

            using StreamWriter writer = new(@$"{path}\{name} ({time}).txt");
            writer.Write(RegretTable.ToString());
        }
    }
}
