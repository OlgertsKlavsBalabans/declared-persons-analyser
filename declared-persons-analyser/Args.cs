using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace declared_persons_analyser
{
    class Args
    {
        //Properties 
        public StringArg source { get; set; }
        public IntArg district { get; set; }
        public IntArg year { get; set; }
        public IntArg month { get; set; }
        public IntArg day { get; set; }
        public IntArg limit { get; set; }
        public StringArg output { get; set; } //represents argument "out"
        public GroupArg group { get; set; }

        public Args()
        {
            this.district = new IntArg();
            this.year = new IntArg();
            this.month = new IntArg();
            this.day = new IntArg();
            this.limit = new IntArg(100);
            this.group = new GroupArg();
            this.output = new StringArg();
            this.source = new StringArg("https://www.epakalpojumi.lv/odata/service/DeclaredPersons");
        }
    }

    class IntArg
    {
        private int _value;
        public int value
        {
            get { return _value; }
            set
            {
                _value = value;
                this.initialized = true;
            }
        }

        public bool initialized { get; set; }

        public IntArg(int v)
        {
            this.value = v;
            this.initialized = false;
        }
        public IntArg()
        {
            this.initialized = false;
        }

    }
    class StringArg
    {
        private string _value;
        public string value
        {
            get { return _value; }
            set
            {
                _value = value;
                this.initialized = true;
            }
        }
        public bool initialized { get; set; }

        public StringArg(string v)
        {
            this.value = v;
            this.initialized = false;
        }
        public StringArg()
        {
            this.initialized = false;
        }
    }

    class GroupArg : StringArg
    {

        private string _value;
        public new string value
        {
            get { return _value; }
            set
            {
                _value = value;
                this.initialized = true;
                this.groupProps = getGroupProps(value);
            }
        }

        public string[] groupProps { get; set; }

        public GroupArg() : base()
        {
            this.groupProps = new string[] { "", "" };
        }
        public GroupArg(string value) : base(value)
        {
            this.groupProps = getGroupProps(value);
        }

        static string[] getGroupProps(string group)
        {
            var obj = new DeclaredPersonsModel();
            string[] groupProps = { "", "" };
            switch (group)
            {
                case "y":
                    groupProps[0] = nameof(obj.year);
                    break;
                case "m":
                    groupProps[0] = nameof(obj.month);
                    break;
                case "d":
                    groupProps[0] = nameof(obj.day);
                    break;
                case "ym":
                    groupProps[0] = nameof(obj.year);
                    groupProps[1] = nameof(obj.month);
                    break;
                case "yd":
                    groupProps[0] = nameof(obj.year);
                    groupProps[1] = nameof(obj.day);
                    break;
                case "md":
                    groupProps[0] = nameof(obj.month);
                    groupProps[1] = nameof(obj.day);
                    break;
                default:
                    break;
            };
            return groupProps;
        }
    }
}
