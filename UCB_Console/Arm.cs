using System;

namespace UCB_Console
{
    class Arm
    {
        private readonly Random _random = new();

        public readonly double Expectation;
        public readonly double Variance;

        public int Counter { private set; get; }
        public double Income { private set; get; }
        public double UCB { private set; get; }

        public Arm(double expectation, double variance)
        {
            Expectation = expectation;
            Variance = variance;
        }

        public void Reset() =>
            Income = UCB = Counter = 0;

        public void Select(int data, ref int countSumData)
        {
            countSumData += data;
            Counter += data;

            while (data-- > 0)
                if (_random.NextDouble() < Expectation)
                    Income++;
        }

        public void SetUCB(int countData, double a) =>
            UCB = Income / Counter + a * Math.Sqrt(Variance * Math.Log(countData) / Counter);
    }
}
