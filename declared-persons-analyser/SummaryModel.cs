using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace declared_persons_analyser
{
    class SummaryModel
    {
        public decimal min { get; set; }
        public decimal max { get; set; }
        public decimal avarage { get; set; }

        public MaxChange maxDrop { get; set; }
        public MaxChange maxIncrease { get; set; }

        public SummaryModel ()
        {
            this.maxDrop = new MaxChange();
            this.maxIncrease = new MaxChange();
        }
    }

    public class MaxChange
    {
        public decimal value { get; set; }
        public string group { get; set; }
        public MaxChange ()
        {
            this.value = 0;
            this.group = "";
        }
    }
}
