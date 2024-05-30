using System;
using System.Collections.Generic;
using System.Linq;

namespace UCB_Console
{
    enum RuleChangeBatch
    {
        Const,
        Alpha,
        Log
    }

    class Bandit
    {
        private readonly Arm[] _arms;
        private readonly int[] _batches;
        private readonly double _sqrtDivDN, _sqrtMulDN;

        private Dictionary<double, double> _regrets;

        public readonly RuleChangeBatch RuleChangeBatch;
        public readonly double Expectation;
        public readonly double Variance;
        public readonly int NumberBatches;
        public readonly int StartBatchSize;
        public readonly double Alpha;
        public readonly int TimeChangeBatch;
        public readonly int Horizon;
        public readonly double Parameter;

        public Action PointProcessed;
        public Action<Bandit> GameOver;

        public Bandit(RuleChangeBatch rule, double expectation, double variance, int countArms, int numberBatches, int startBatchSize, double parameter, double alpha = 1.5, int timeChangeBatch = 10)
        {
            if (expectation < 0d || expectation > 1d)
                throw new ArgumentException("Incorrect mathematical expectation. The mathematical expectation must be greater than or equal to 0 and less than or equal to 1.");

            if (variance < 0d || variance > 0.25d)
                throw new ArgumentException("Incorrect variance. The variance must be greater than or equal to 0 and less than or equal to 0.25.");

            _arms = new Arm[countArms];
            _batches = new int[NumberBatches];

            _batches = rule switch
            {
                RuleChangeBatch.Alpha => Enumerable.Range(0, numberBatches - countArms).Select(i => (int)(Math.Pow(alpha, i / timeChangeBatch) * startBatchSize)).ToArray(),
                RuleChangeBatch.Log => Enumerable.Range(0, numberBatches - countArms).Select(x => startBatchSize * (int)Math.Exp(x)).ToArray(),
                _ => Enumerable.Repeat(startBatchSize, numberBatches - countArms).ToArray(),
            };

            Horizon = startBatchSize * countArms + _batches.Sum();
            RuleChangeBatch = rule;
            Expectation = expectation;
            Variance = variance;
            NumberBatches = numberBatches;
            StartBatchSize = startBatchSize;
            Parameter = parameter;
            Alpha = alpha;
            TimeChangeBatch = timeChangeBatch;

            _sqrtDivDN = Math.Sqrt(Variance / Horizon);
            _sqrtMulDN = Math.Sqrt(Variance * Horizon);
        }

        public double MaxDeviation { private set; get; }

        public double MaxRegrets { private set; get; }

        public Dictionary<double, double> Regrets => 
            _regrets.ToDictionary(k => k.Key, v => v.Value);

        public void Play(double[] deviations, int gamesCount)
        {
            _regrets = deviations.ToDictionary(k => k, v => 0d);

            double maxIncome;
            int sumCountData;

            foreach (var dev in deviations)
            {
                if (dev == 0d)
                {
                    PointProcessed?.Invoke();
                    continue;
                }

                for (int i = 0; i < _arms.Length; i++)
                    _arms[i] = new Arm(Expectation + (i == 0 ? dev : -dev) * _sqrtDivDN, Variance);

                maxIncome = _arms.Select(a => a.Expectation).Max() * Horizon;

                for (int num = 0; num < gamesCount; num++)
                {
                    sumCountData = 0;

                    foreach (var arm in _arms)
                    {
                        arm.Reset();
                        arm.Select(StartBatchSize, ref sumCountData);
                    }

                    foreach (var batch in _batches)
                    {
                        foreach (var arm in _arms)
                            arm.SetUCB(sumCountData, Parameter);

                        _arms.OrderByDescending(a => a.UCB).First().Select(batch, ref sumCountData);
                    }

                    _regrets[dev] += maxIncome - _arms.Select(a => a.Income).Sum();
                }

                _regrets[dev] /= gamesCount * _sqrtMulDN;
                PointProcessed?.Invoke();
            }

            (MaxDeviation, MaxRegrets) = _regrets.Aggregate((max, next) => next.Value > max.Value ? next : max);
            GameOver?.Invoke(this);
        }
    }
}
