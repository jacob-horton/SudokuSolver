﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TrainingInterface
{
    public class InputData
    {
        public double[] targets;
        public double[] inputs;

        public InputData(double[] inputs, int target)
        {
            targets = Enumerable.Range(0, 10).Select(x => 0.01).ToArray();
            targets[target] = 0.99;

            this.inputs = inputs;
        }
    }
}