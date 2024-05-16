using System;
using System.Collections.Generic;
using System.Linq;

namespace UCB_Console
{
    class Bandit
    {
        private static double s_mathExp;
        private static double[] s_deviation;

        private readonly Arm[] _arms;
        private readonly double _sqrtDivDN, _sqrtMulDN;

        private Dictionary<double, double> _regrets;

        public static int NumberSimulations;
        public static double MaxDispersion;

        public readonly double Alpha;
        public readonly int TimeChangeBatch;

        public readonly int StartBatchSize;
        public readonly int Horizon;
        public readonly double Parameter;

        public delegate void EventUpdateData();
        public event EventUpdateData PointProcessed;
        public event EventUpdateData Finished;

        public Bandit(int countArms, int startBatchSize, int horizon, double parameter, double alpha, int timeChangeBatch)
        {
            _arms = new Arm[countArms];

            StartBatchSize = startBatchSize;
            Horizon = horizon;
            Parameter = parameter;

            Alpha = alpha;
            TimeChangeBatch = timeChangeBatch;

            _sqrtDivDN = Math.Sqrt(MaxDispersion / Horizon);
            _sqrtMulDN = Math.Sqrt(MaxDispersion * Horizon);
        }

        public double MaxDeviation { private set; get; }

        public double MaxRegrets { private set; get; } = 0d;

        public static int NumberDeviations => s_deviation.Length;

        public static double[] Deviations => (double[])s_deviation.Clone();

        public static double MathExp
        {
            set
            {
                if (value > 1d || value < 0d)
                    throw new ArgumentException("For the Bernoulli distribution expectation of p must be between 0 and 1 inclusive.");

                s_mathExp = value;
            }
            get => s_mathExp;
        }

        public static double DeltaDevition { private set; get; }

        public double GetRegrets(double dev) => _regrets[dev];

        public static void SetDeviation(double startDevition, double deltaDevision, int count)
        {
            DeltaDevition = deltaDevision;
            s_deviation = Enumerable.Range(0, count).Select(i => Math.Round(startDevition + i * deltaDevision, 1)).ToArray();
        }

        public void RunSimulation()
        {
            _regrets = s_deviation.ToDictionary(k => k, v => 0d);

            double maxIncome;
            int sumCountData, batchSize, horizon, counter;

            foreach(var dev in s_deviation)
            {
                if (dev == 0d)
                {
                    PointProcessed?.Invoke();
                    continue;
                }

                for (int i = 0; i < _arms.Length; i++)
                    _arms[i] = new Arm(MathExp + (i == 0 ? 1 : -1) * dev * _sqrtDivDN, MaxDispersion);

                maxIncome = _arms.Select(a => a.Expectation).Max() * Horizon;

                for (int num = 0; num < NumberSimulations; num++)
                {
                    batchSize = StartBatchSize;
                    horizon = Horizon;
                    counter = sumCountData = 0;

                    foreach (var arm in _arms)
                    {
                        arm.Reset();
                        arm.Select(batchSize, ref sumCountData, ref horizon);

                        counter++;
                    }

                    while (horizon > 0)
                    {
                        foreach (var arm in _arms)
                            arm.SetUCB(sumCountData, Parameter);

                        if (counter >= TimeChangeBatch)
                        {
                            counter = 0;
                            batchSize = (int)(Alpha * batchSize);
                        }

                        _arms.OrderByDescending(a => a.UCB).First().Select(batchSize, ref sumCountData, ref horizon);
                        counter++;
                    }

                    _regrets[dev] += maxIncome - _arms.Select(a => a.Income).Sum();
                }

                _regrets[dev] /= NumberSimulations * _sqrtMulDN;
                PointProcessed?.Invoke();
            }

            (MaxDeviation, MaxRegrets) = _regrets.Aggregate((max, next) => next.Value > max.Value ? next : max);
            Finished?.Invoke();
        }
    }
}
