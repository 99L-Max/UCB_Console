using System;

namespace UCB_Console
{
    class Arm
    {
        private readonly Random random = new();

        public readonly double Expectation;
        public readonly double Dispersion;

        public int Counter { private set; get; }
        public double Income { private set; get; }
        public double UCB { private set; get; }

        public Arm(double expectation, double dispersion)
        {
            Expectation = expectation;
            Dispersion = dispersion;
        }

        public void Reset()
        {
            Counter = 0;
            Income = 0;
            UCB = 0;
        }

        public void Select(int data, ref int countBatches)
        {
            countBatches++;
            Counter++;
            
            while (data-- > 0)
                if (random.NextDouble() < Expectation)
                    Income++;
        }

        public void SetUCB(int batchSize, int countBatches, double a)
        {
            UCB = Income / Counter + a * Math.Sqrt(Dispersion * batchSize * Math.Log(countBatches) / Counter);
        }
    }
}
