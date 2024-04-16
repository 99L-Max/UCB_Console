using System;
using System.Linq;

namespace UCB_Console
{
    class Bandit
    {
        private static double s_mathExp;
        private static double[] s_deviation;

        private readonly Arm[] _arms;
        private readonly double _sqrtDivDN, _sqrtMulDN;

        private double[] _regrets;

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

        public double GetRegrets(int i) => _regrets[i];

        public static double GetDeviation(int i) => s_deviation[i];

        public static void SetDeviation(double startDevition, double deltaDevision, int count)
        {
            DeltaDevition = deltaDevision;
            s_deviation = Enumerable.Range(0, count).Select(i => Math.Round(startDevition + i * deltaDevision, 1)).ToArray();
        }

        public void RunSimulation()
        {
            _regrets = new double[s_deviation.Length];

            double maxIncome;
            int sumCountData, batchSize, horizon, counter;

            for (int mainIndex = 0; mainIndex < s_deviation.Length; mainIndex++)
            {
                if (s_deviation[mainIndex] == 0d)
                {
                    PointProcessed?.Invoke();
                    continue;
                }

                for (int i = 0; i < _arms.Length; i++)
                    _arms[i] = new Arm(MathExp + (i == 0 ? 1 : -1) * s_deviation[mainIndex] * _sqrtDivDN, MaxDispersion);

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

                    _regrets[mainIndex] += maxIncome - _arms.Select(a => a.Income).Sum();
                }

                _regrets[mainIndex] /= NumberSimulations * _sqrtMulDN;

                if (MaxRegrets < _regrets[mainIndex])
                {
                    MaxRegrets = _regrets[mainIndex];
                    MaxDeviation = s_deviation[mainIndex];
                }

                PointProcessed?.Invoke();
            }

            Finished?.Invoke();
        }
    }
}
