﻿using System;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using TemperatureStation.Web.Data;
using TemperatureStation.Web.Extensions;
using TemperatureStation.Web.Models;

namespace TemperatureStation.Web.Controllers
{
    public class HomeController : Controller
    {
        private readonly ApplicationDbContext _dataContext;
        private readonly ICalculatorProvider _calcProvider;

        public HomeController(ApplicationDbContext dataContext, ICalculatorProvider calcProvider)
        {
            _dataContext = dataContext;
            _calcProvider = calcProvider;
        }

        public async Task<IActionResult> Index(int? measurementId)
        {
            if(!User.Identity.IsAuthenticated)
            {
                return View("IndexPublic");
            }

            _dataContext.ChangeTracker.QueryTrackingBehavior = QueryTrackingBehavior.NoTracking;
            
            var model = new HomeViewModel();
            model.Measurement = await _dataContext.Measurements
                                                  .Include(m => m.SensorRoles)
                                                  .Include(m => m.Calculators)
                                                  .Where(m => measurementId == null || m.Id == measurementId)
                                                  .OrderByDescending(m => m.IsActive)
                                                  .FirstOrDefaultAsync();
            if(model.Measurement == null)
            {
                return View("IndexEmpty");
            }

            model.Readings = _dataContext.GetReadings(model.Measurement.Id, null, 10)
                                         .OrderByDescending(r => r.Key)
                                         .ToList();

            //model.ChartData = _dataContext.GetReadings(model.Measurement.Id, DateTime.Now.AddHours(-24), 10000);
            model.ChartData = _dataContext.GetReadings(model.Measurement.Id, null, 10000);
            var showOnChart = _calcProvider.GetTypes()
                                            .Where(t => t.GetTypeInfo().GetCustomAttribute<CalculatorAttribute>() != null)
                                            .Where(t => t.GetTypeInfo().GetCustomAttribute<CalculatorAttribute>().ShowOnChart)
                                            .Select(t => t.GetTypeInfo().GetCustomAttribute<CalculatorAttribute>().Name)
                                            .ToList();
            showOnChart.AddRange(model.Measurement.SensorRoles.Select(r => r.RoleName));
            model.CalculatorsOnChart = showOnChart.ToArray();

            var labels = _calcProvider.GetTypes()
                                            .Where(t => t.GetTypeInfo().GetCustomAttribute<CalculatorAttribute>() != null)
                                            .Select(t => new
                                            {
                                                Label = t.GetTypeInfo().GetCustomAttribute<CalculatorAttribute>().DisplayLabel,
                                                Name = t.GetTypeInfo().GetCustomAttribute<CalculatorAttribute>().Name
                                            })
                                            .ToDictionary(k => k.Name, e => e.Label);

            foreach (var key in labels.Keys.ToList())
            {
                if(string.IsNullOrEmpty(labels[key]))
                {
                    labels[key] = key;
                }
            }
            model.Labels = labels;
            model.Calculators = _calcProvider.GetCalculators();
            model.Statistics = _dataContext.GetMeasurementStats(model.Measurement.Id);
            model.Measurements = _dataContext.Measurements
                                             .OrderByDescending(m => m.IsActive)
                                             .ThenBy(m => m.Name)
                                             .Select(m => new SelectListItem
                                             {
                                                 Text = m.Name,
                                                 Value = m.Id.ToString(),
                                                 Selected = (m.Id == model.Measurement.Id)
                                             })
                                             .ToList();

            return View(model); 
        }

        public IActionResult Error()
        {
            return View();
        }
    }
}