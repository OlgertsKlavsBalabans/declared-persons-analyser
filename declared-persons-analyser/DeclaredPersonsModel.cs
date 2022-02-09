using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace declared_persons_analyser
{
    [global::System.Data.Services.Common.DataServiceKeyAttribute("id")]
    public class DeclaredPersonsModel
    {

        public int id { get; set; }
        public int year { get; set; }
        public int month { get; set; }
        public int day { get; set; }
        public decimal value { get; set; }
        public int district_id { get; set; }
        public string district_name { get; set; }
    }

    public class DeclaredPersonsExtended : DeclaredPersonsModel
    {
        public decimal change { get; set; }

        public DeclaredPersonsExtended (DeclaredPersonsModel dpm)
        {
            this.id = dpm.id;
            this.year = dpm.year;
            this.month = dpm.month;
            this.day = dpm.day;
            this.value = dpm.value;
            this.district_id = dpm.district_id;
            this.district_name = dpm.district_name;
        }
        public DeclaredPersonsExtended()
        {

        }
    }
}
