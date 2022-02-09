using System;
using System.Collections.Generic;
using System.Data.Services.Client;
using System.Linq;
using System.Data;
using System.Text.Json;
using System.Text.Encodings.Web;
using System.IO;
using System.Dynamic;
namespace declared_persons_analyser
{
    class Program
    {
        static void Main(string[] args)
        {
            Args inputArgs = getInputArgs(args);
            validateArgs(inputArgs);
            var declaredPersonsQuerry = getQuerryFromUrl(inputArgs);
            List<DeclaredPersonsExtended> declaredPersons = calculateDeclaredPersons(declaredPersonsQuerry);
            SummaryModel summary = calculateSummary(declaredPersons, inputArgs.group.groupProps);
            printDeclaredPersons(declaredPersons, inputArgs.group.groupProps, getGroupStringForamt(inputArgs.group.value));
            printSummary(summary);
            createJsonFile(declaredPersons,summary,inputArgs.group.groupProps,inputArgs.output.value);
        }
        static Args getInputArgs(string[] args)
        {
            Args inputArgs = new Args();
            string chosenParameter = "";
            foreach (string arg in args)
            {
                if (arg.Substring(0, 1) == "-")
                {
                    chosenParameter = arg;
                }
                else
                {
                    switch (chosenParameter)
                    {
                        case "-source":
                            inputArgs.source.value = arg;
                            break;
                        case "-district":
                            inputArgs.district.value = int.Parse(arg);
                            break;
                        case "-year":
                            inputArgs.year.value = int.Parse(arg);
                            break;
                        case "-month":
                            inputArgs.month.value = int.Parse(arg);
                            break;
                        case "-day":
                            inputArgs.day.value = int.Parse(arg);
                            break;
                        case "-limit":
                            inputArgs.limit.value = int.Parse(arg);
                            break;
                        case "-group":
                            inputArgs.group.value = arg;
                            break;
                        case "-out":
                            inputArgs.output.value = arg;
                            break;
                        default:
                            break;
                    }
                }

            }

            return inputArgs;
        }
        static void validateArgs(Args inputArgs)
        {
            if (inputArgs.district.initialized == false)
            {
                throw new ArgumentNullException("Arg -district is not initialized, and is obligatory. Add arg -district 000 to specify district!");
            }
        }
        static string getGroupStringForamt (string groupValue)
        {
            string groupStringForamt = "";
            switch (groupValue)
            {
                case "y":
                    groupStringForamt = "{3,-10}";
                    break;
                case "m":
                    groupStringForamt = "{3,-10}";
                    break;
                case "d":
                    groupStringForamt = "{3,-10}";
                    break;
                case "ym":
                    groupStringForamt = "{3,-10}";
                    groupStringForamt += " {4,-10}";
                    break;
                case "yd":
                    groupStringForamt = "{3,-10}";
                    groupStringForamt += " {4,-10}";
                    break;
                case "md":
                    groupStringForamt = "{3,-10}";
                    groupStringForamt += " {4,-10}";
                    break;
                default:
                    break;
            };
            return groupStringForamt;
        }
        static IQueryable<DeclaredPersonsModel> getQuerryFromUrl (Args inputArgs)
        {
            DataServiceContext dpContext = new DataServiceContext(new Uri(inputArgs.source.value));

            var predicate = PredicateBuilder.True<DeclaredPersonsModel>();

            predicate = predicate.And(p => p.district_id == inputArgs.district.value);

            if (inputArgs.year.initialized)
            {
                predicate = predicate.And(p => p.year == inputArgs.year.value);
            }
            if (inputArgs.month.initialized)
            {
                predicate = predicate.And(p => p.month == inputArgs.month.value);
            }
            if (inputArgs.day.initialized)
            {
                predicate = predicate.And(p => p.day == inputArgs.day.value);
            }


            var dpQuerry = dpContext.CreateQuery<DeclaredPersonsModel>(" ")
                .Where(predicate)
                .OrderByDescending(p => p.year).ThenBy(p => p.month).ThenBy(p => p.day)
                .Take(inputArgs.limit.value);

            return dpQuerry;
        }
        static List<DeclaredPersonsExtended> calculateDeclaredPersons (IQueryable<DeclaredPersonsModel> declaredPersonsQuerry)
        {

            List<DeclaredPersonsExtended> declaredPersons = new List<DeclaredPersonsExtended>();
            decimal lastPersonValue = 0;
            bool firstCycle = true;
            foreach (var p in declaredPersonsQuerry)
            {
                var declaredPerson = new DeclaredPersonsExtended(p);

                if (firstCycle)
                {
                    firstCycle = false;
                    lastPersonValue = p.value;
                    declaredPerson.change = 0;
                }
                else
                {
                    declaredPerson.change = p.value - lastPersonValue;
                    lastPersonValue = p.value;
                }
                declaredPersons.Add(declaredPerson);
            }
            return declaredPersons;
        }
        static SummaryModel calculateSummary (List<DeclaredPersonsExtended> declaredPersons, string[] groupProps)
        {
            SummaryModel summary = new SummaryModel();

            bool firstCycle = true;
            decimal changeSum = 0;
            foreach (var p in declaredPersons)
            {
                if (firstCycle)
                {
                    firstCycle = false;
                    summary.max = p.value;
                    summary.min = p.value;
                    summary.maxDrop.group = getSummaryGroup(p);
                    summary.maxIncrease.group = summary.maxDrop.group;
                }
                else
                {
                    changeSum += p.value;
                    if (p.value > summary.max)
                    {
                        summary.max = p.value;
                    }
                    if (p.value < summary.min)
                    {
                        summary.min = p.value;
                    }
                    if (p.change > summary.maxIncrease.value)
                    {
                        summary.maxIncrease.value = p.change;
                        summary.maxIncrease.group = getSummaryGroup(p);
                    }
                    if (p.change < summary.maxDrop.value)
                    {
                        summary.maxDrop.value = p.change;
                        summary.maxDrop.group = getSummaryGroup(p);
                    }
                }
            }
            if (declaredPersons.Count > 1)
            {
                summary.avarage = Decimal.Round(changeSum / declaredPersons.Count);
            }
            return summary;

            string getSummaryGroup (DeclaredPersonsExtended declaredPerson)
            {
                string summaryGroup = "";
                foreach (var groupString in groupProps)
                {
                    if (groupString != "")
                    {
                        summaryGroup += declaredPerson.GetType().GetProperty(groupString).GetValue(declaredPerson, null);
                        summaryGroup += ".";
                    }
                }
                if (summaryGroup.EndsWith("."))
                {
                    summaryGroup  = summaryGroup.Remove(summaryGroup.Length - 1);
                }
                return summaryGroup;
            }
        }
        static void printDeclaredPersons (List<DeclaredPersonsExtended> declaredPersons, string[] groupProps, string groupStringForamt)
        {
            string declaredPersonsFormat = "{0,-20} " + groupStringForamt + " {1,-10} {2,-10}";
            string formatedString = String.Format(declaredPersonsFormat, "district_name", "value", "change", groupProps[0], groupProps[1]);
            Console.WriteLine(formatedString);

            if (groupProps[0] == "")
            {
                foreach (var p in declaredPersons)
                {
                    Console.WriteLine(declaredPersonsFormat, p.district_name, p.value, p.change);
                }
            }
            else if (groupProps[1] == "")
            {
                foreach (var p in declaredPersons)
                {
                    Console.WriteLine(declaredPersonsFormat, p.district_name, p.value, p.change,
                        p.GetType().GetProperty(groupProps[0]).GetValue(p, null));
                }
            }
            else
            {
                foreach (var p in declaredPersons)
                {
                    Console.WriteLine(declaredPersonsFormat, p.district_name, p.value, p.change,
                        p.GetType().GetProperty(groupProps[0]).GetValue(p, null),
                        p.GetType().GetProperty(groupProps[1]).GetValue(p, null));
                }
            }
        }
        static void printSummary (SummaryModel summary)
        {
            string summaryStringFormat = "{0,-15} {1,-25}";
            string summaryChangeStringFormat = "{0,-15} {1,-7} {2,-7}";
            Console.WriteLine();
            Console.WriteLine(summaryStringFormat, "Max:", summary.max);
            Console.WriteLine(summaryStringFormat, "Min:", summary.min);
            Console.WriteLine(summaryStringFormat, "Avarage:", summary.avarage);
            Console.WriteLine();
            Console.WriteLine(summaryChangeStringFormat, "Max drop:", summary.maxDrop.value, summary.maxDrop.group);
            Console.WriteLine(summaryChangeStringFormat, "Max increase:", summary.maxIncrease.value, summary.maxIncrease.group);

        }
        static void createJsonFile (List<DeclaredPersonsExtended> declaredPersons, SummaryModel summary, string[] groupProps,string output)
        {
            var data = new List<ExpandoObject>();
            foreach (var p in declaredPersons)
            {
                dynamic dataPoint = new ExpandoObject();
                dataPoint.districtName = p.district_name;
                dataPoint.value = p.value;
                dataPoint.change = p.change;
                if (groupProps[0] != "")
                {
                    ((IDictionary<string, object>)dataPoint)[groupProps[0]] = p.GetType().GetProperty(groupProps[0]).GetValue(p, null);
                }
                if (groupProps[1] != "")
                {
                    ((IDictionary<string, object>)dataPoint)[groupProps[1]] = p.GetType().GetProperty(groupProps[1]).GetValue(p, null);
                }
                data.Add(dataPoint);
            }
            var jsonObject = new { data, summary };
            var jsonSerializerSettings = new JsonSerializerOptions
            {
                WriteIndented = true,
                Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
            };
            File.WriteAllText(output, JsonSerializer.Serialize(jsonObject, jsonSerializerSettings));
        }

    }


}
