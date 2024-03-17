using System;
using System.Linq;

namespace UCB_Console
{
    class Bandit
    {
        private static double mathExp;
        private static double[] deviation;

        private readonly Arm[] arms;
        private readonly double sqrtDivDN, sqrtMulDN;

        private double[] regrets;

        public static int NumberSimulations;
        public static double MaxDispersion;

        public readonly int BatchSize;
        public readonly int NumberBatches;
        public readonly int Horizon;
        public readonly double Parameter;

        public delegate void EventUpdateData();
        public event EventUpdateData PointProcessed;
        public event EventUpdateData Finished;

        public Bandit(int countArms, int batchSize, int numberBatches, double parameter)
        {
            arms = new Arm[countArms];

            BatchSize = batchSize;
            NumberBatches = numberBatches;
            Parameter = parameter;
            Horizon = BatchSize * NumberBatches;

            sqrtDivDN = Math.Sqrt(MaxDispersion / Horizon);
            sqrtMulDN = Math.Sqrt(MaxDispersion * Horizon);
        }

        public double MaxDeviation { private set; get; }

        public double MaxRegrets { private set; get; } = 0d;

        public static int NumberDeviations => deviation.Length;

        public static double MathExp
        {
            set
            {
                if (value > 1d || value < 0d)
                    throw new ArgumentException("For the Bernoulli distribution expectation of p must be between 0 and 1 inclusive.");

                mathExp = value;
            }
            get => mathExp;
        }

        public static double DeltaDevition { private set; get; }

        public double GetRegrets(int i) => regrets[i];

        public static double GetDeviation(int i) => deviation[i];

        public static void SetDeviation(double startDevition, double deltaDevision, int count)
        {
            DeltaDevition = deltaDevision;
            deviation = Enumerable.Range(0, count).Select(i => Math.Round(startDevition + i * deltaDevision, 1)).ToArray();
        }

        public void RunSimulation()
        {
            regrets = new double[deviation.Length];

            IOrderedEnumerable<Arm> sortArms;
            double maxIncome;
            int sumCountBatches;

            for (int mainIndex = 0; mainIndex < deviation.Length; mainIndex++)
            {
                if (deviation[mainIndex] == 0d)
                {
                    PointProcessed.Invoke();
                    continue;
                }

                for (int i = 0; i < arms.Length; i++)
                    arms[i] = new Arm(MathExp + (i == 0 ? 1 : -1) * deviation[mainIndex] * sqrtDivDN, MaxDispersion);

                maxIncome = arms.Select(a => a.Expectation).Max() * Horizon;

                for (int num = 0; num < NumberSimulations; num++)
                {
                    sumCountBatches = 0;

                    foreach (var arm in arms)
                    {
                        arm.Reset();
                        arm.Select(BatchSize, ref sumCountBatches);
                    }

                    for (int batch = arms.Length; batch < NumberBatches; batch++)
                    {
                        foreach (var arm in arms)
                            arm.SetUCB(BatchSize, sumCountBatches, Parameter);

                        sortArms = arms.OrderByDescending(a => a.UCB);
                        sortArms.First().Select(BatchSize, ref sumCountBatches);
                    }

                    regrets[mainIndex] += maxIncome - arms.Select(a => a.Income).Sum();
                }

                regrets[mainIndex] /= NumberSimulations * sqrtMulDN;

                if (MaxRegrets < regrets[mainIndex])
                {
                    MaxRegrets = regrets[mainIndex];
                    MaxDeviation = deviation[mainIndex];
                }

                PointProcessed.Invoke();
            }

            Finished.Invoke();
        }
    }
}
