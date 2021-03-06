﻿using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Mvc.Rendering;
using TemperatureStation.Web.Data;
using TemperatureStation.Web.Extensions;

namespace TemperatureStation.Web.Models
{
    public class HomeViewModel
    {
        public Measurement Measurement { get; set; }
        public IEnumerable<IGrouping<DateTime, ReadingViewModel>> Readings { get; set; }
        public IEnumerable<IGrouping<DateTime, ReadingViewModel>> ChartData { get; set; }
        public string[] CalculatorsOnChart { get; set; }
        public IDictionary<string,string> Labels { get; set; }
        public IDictionary<string, ICalculator> Calculators { get; set; }
        public IDictionary<string, Tuple<double, double>> Statistics { get; set; }
        public IEnumerable<SelectListItem> Measurements { get; set; }

        public HomeViewModel()
        {
            Readings = new List<IGrouping<DateTime, ReadingViewModel>>();
            CalculatorsOnChart = new string[] { };
            Labels = new Dictionary<string, string>();
            Measurements = new List<SelectListItem>();
        }
    }
}
