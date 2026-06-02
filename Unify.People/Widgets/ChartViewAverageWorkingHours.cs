using AppStudio.Framework.Core;
using AppStudio.Model;
using DevExtreme.AspNet.Mvc;
using DevExtreme.AspNet.Mvc.Builders;
using Unify.Data;
using Unify.People.Models;

namespace Unify.People.Widgets
{
    public class ChartViewAverageWorkingHours : ChartView
    {
        public ChartViewAverageWorkingHours()
        {
            ChartType = ChartType.Line;
            Title = "This Week's Avg Working Hours";
            KeyCaption = "Avg Working Hours";
            DataCaption = "Daily Avg Working Hours";

            OnLoadData += ChartViewAverageWorkingHours_OnLoadData;
            OnBeforeRender += ChartViewAverageWorkingHours_OnBeforeRender;
        }

        private async Task<List<ChartData>> ChartViewAverageWorkingHours_OnLoadData()
        {
            var cardView = this.Page.FindControl<CardView<DashboardFilterData>>();
            var TenantId = AppStudio.Framework.Core.CoreServices.CurrentUser.GetTenantId();
            DateTime fromDate = 
                (cardView.Data.FromDate == DateOnly.MinValue)
                ? DateTime.Parse(DateTime.Now.ToString("yyyy-MM-dd"))
                : DateTime.Parse(cardView.Data.FromDate.ToString("yyyy-MM-dd"));
            DateTime toDate =
                (cardView.Data.ToDate == DateOnly.MinValue)
                ? DateTime.Parse(DateTime.Now.ToString("yyyy-MM-dd"))
                : DateTime.Parse(cardView.Data.ToDate.ToString("yyyy-MM-dd"));
            string? departmentName = (cardView.Data.DepartmentName == null) ? null : cardView.Data.DepartmentName;


            //string sql =
            //       $"SELECT CAST(dateTime AS Date) AS DateOnly" +
            //       $" , ROUND(DATE_DIFF('second', MIN(timeStamp), MAX(timeStamp))/3600.00,2) AS Average" +
            //       // $"     , ROUND(((SUM(CAST(time AS DECIMAL)) / COUNT(DISTINCT employeeId)) / 60), 2) AS Average" +
            //       $"  FROM attendanceindex" +
            //       $" WHERE TenantId = '{TenantId}'";
            //if (!string.IsNullOrEmpty(departmentName)) sql += $"   AND department = '{departmentName}'";
            //sql += $"   AND (dateTime >= '{fromDate.ToString("yyyy-MM-dd")}' AND dateTime <= '{toDate.ToString("yyyy-MM-dd")}')";
            //sql += $" GROUP BY CAST(dateTime AS Date)";


            var peopleDb = CoreServices.GetService<PeopleDb>();
            var query = peopleDb.AttendanceLogs
                .Where(m =>  m.DateTime.Date >= fromDate && m.DateTime.Date <= toDate);
            if (!string.IsNullOrEmpty(departmentName)) query = query.Where(m => m.Department == departmentName);
            var results = query
                .GroupBy(m => m.DateTime.Date)
                .Select(m => new AverageData
                {
                    DateOnly = DateOnly.FromDateTime(m.Key),
                    Average = Math.Round(
                        (decimal)(m.Max(x => x.TimeStamp) - m.Min(x => x.TimeStamp)).TotalSeconds / 3600m, 2)
                })
                .ToList();


            List<AppStudio.Model.ChartData> chartDatas = new();
            foreach (var value in results)
            {
                AppStudio.Model.ChartData chartData = new();

                chartData.Key = value.DateOnly.Day.ToString() + "/" + value.DateOnly.Month.ToString();
                chartData.Data = value.Average;
                chartDatas.Add(chartData);
            }

            return chartDatas;
        }

        private async void ChartViewAverageWorkingHours_OnBeforeRender(WidgetBuilder widget)
        {
            string jsFunction = @"
                    function onChartPointClick(s, e) {
                        e.processOnServer = false;
                    }";

            ChartBuilder chartBuilder = widget as ChartBuilder;
            chartBuilder
                .Palette(VizPalette.DarkMoon)
                .Margin(m => m.Bottom(20))
                .ArgumentAxis(a => a
                    .ValueMarginsEnabled(true)
                    .DiscreteAxisDivisionMode(DiscreteAxisDivisionMode.BetweenLabels)
                    .Grid(g => g.Visible(false))
                     .Label(c => c.Format(Format.Decimal))       // Set label format to fixed-point with 2 decimal places
                )
                .CommonAxisSettings(c => c.AllowDecimals(true).ValueMarginsEnabled(true))
                //.ValueAxis(c => c.Add().Label(c => c.Format(Format.Decimal)))
                //.CommonSeriesSettings(c => c.Label(c => c.Format(Format.Decimal)))
                .Legend(l => l
                    .VerticalAlignment(VerticalEdge.Bottom)
                    .HorizontalAlignment(HorizontalAlignment.Center)
                    .ItemTextPosition(DevExtreme.AspNet.Mvc.Position.Bottom)
                )
                .Crosshair(t => t.Enabled(true))
                .Export(e => e.Enabled(true))
                .Tooltip(t => t.Enabled(true))
                .ValueAxis(m => m.Add()
                    .AggregatedPointsPosition(AggregatedPointsPosition.CrossTicks)
                    .AllowDecimals(true)
                    .ConstantLines(c => c.Add()
                        .Color("#fc3535")
                        .DashStyle(DevExtreme.AspNet.Mvc.DashStyle.Dash)
                        .Label(l => l
                            .Visible(true)
                            .HorizontalAlignment(DevExtreme.AspNet.Mvc.HorizontalAlignment.Right)
                            .Font(f => f
                                .Color("#fc3535")
                                .Size(15)
                                .Weight(500)
                            )
                            .Text("9 hours")
                        )
                        .Value(9)
                        .Width(2)
                    )
                )
                .OnPointClick(jsFunction);
        }
    }
}

//private async Task<List<ChartData>> ChartViewAverageWorkingHours_OnLoadData()
//{
//    var cardView = this.Page.GetPart<CardView<DashboardFilterData>>();
//    var TenantId = AppBuilder.Framework.Core.CoreServices.GetUser().GetTenantId();
//    string fromDate = (cardView.Data.FromDate == DateTime.MinValue) ? DateTime.Now.ToString("yyyy-MM-dd") : cardView.Data.FromDate.ToString("yyyy-MM-dd");
//    string toDate = (cardView.Data.ToDate == DateTime.MinValue) ? DateTime.Now.ToString("yyyy-MM-dd") : cardView.Data.ToDate.ToString("yyyy-MM-dd");
//    string? departmentName = (cardView.Data.DepartmentName == null) ? null : cardView.Data.DepartmentName;

//    string sql =
//        $"SELECT " +
//        $"    CAST(timeStamp AS DATE) AS DateOnly, " +
//        $"    ROUND(AVG(DATE_DIFF('second', MIN(timeStamp), MAX(timeStamp)) / 3600.00), 2) AS AverageDailyWorkingHours " +
//        $"FROM attendanceindex " +
//        $"WHERE TenantId = '{TenantId}'";

//    if (departmentName != null)
//        sql += $" AND department = '{departmentName}'";

//    sql += $" AND (timeStamp >= '{fromDate}' AND timeStamp <= '{toDate}') ";
//    sql += $"GROUP BY CAST(timeStamp AS DATE)";

//    var elasticDb = AppBuilder.Framework.Core.CoreServices.GetService<ElasticDb>();
//    List<AverageData> results = await elasticDb.GetAllAsync<AverageData>(sql);
//    results = results.OrderBy(m => m.DateOnly).ToList();

//    List<AppBuilder.Model.ChartData> chartDatas = new();
//    foreach (var data in results)
//    {
//        AppBuilder.Model.ChartData chartData = new();

//        chartData.Key = data.DateOnly.Day.ToString() + "/" + data.DateOnly.Month.ToString();
//        chartData.Data = data.Average;
//        chartDatas.Add(chartData);
//    }

//    return chartDatas;
//}
//    private void ChartViewAverageWorkingHours_OnBeforeRender(DevExtreme.AspNet.Mvc.Builders.WidgetBuilder widget)
//    {
//        ChartBuilder chartBuilder = widget as ChartBuilder;

//        chartBuilder
//            .Palette(VizPalette.DarkMoon)
//            //.ArgumentAxis(c => c.Tick(c=> c.Visible(false)).Label(c => c.Visible(false)))
//            .Export(e => e.Enabled(true))
//            .AutoHidePointMarkers(true)
////.Legend(c => c.Visible(false))

//            // Hides the labels (green square blocks)
//            //.CommonAnnotationSettings(c => c.CustomizeTooltip(Name).TooltipEnabled(true))
//            .CommonAxisSettings(c => c.AllowDecimals(true).EndOnTick(true))
//            .ValueAxis(m => m.Add()
//                .ConstantLines(c => c.Add()
//                    .Color("#fc3535")
//                    .DashStyle(DevExtreme.AspNet.Mvc.DashStyle.Dash)
//                    .Label(l => l
//                        .Visible(true)
//                        .HorizontalAlignment(DevExtreme.AspNet.Mvc.HorizontalAlignment.Right)
//                        .Font(f => f
//                            .Color("#fc3535")
//                            .Size(15)
//                            .Weight(500)
//                        )
//                        .Text("9 hours")
//                    )
//                    .Value(9)
//                    .Width(2)
//                )
//            );
//    }


//        private void ChartViewAverageWorkingHours_OnBeforeRender(DevExtreme.AspNet.Mvc.Builders.WidgetBuilder widget)
//        {
//            ChartBuilder chartBuilder = widget as ChartBuilder;

//            chartBuilder
//    .Palette(VizPalette.DarkMoon)
//    .Export(e => e.Enabled(true))
//    .AutoHidePointMarkers(true)
//    .StickyHovering(false)

//    // Argument Axis
//    .ArgumentAxis(a => a
//        .AggregatedPointsPosition(AggregatedPointsPosition.CrossTicks)
//        .Label(l => l
//            .Visible(true)
//            .Position(DevExtreme.AspNet.Mvc.Position.Bottom)
//            .TextOverflow(VizTextOverflow.Ellipsis)
//            .RotationAngle(0)
//        )
//        .DiscreteAxisDivisionMode(DiscreteAxisDivisionMode.BetweenLabels)
//    )

//    // Series Configuration
//    //.CommonSeriesSettings(s => s
//    //    .Type(SeriesType.Line)
//    //    .ArgumentField("Key")
//    //    .ValueField("Data")
//    //    .DashStyle(DashStyle.Solid)
//    //    .Visible(true)
//    //    .Width(2)
//    //    .Point(p => p
//    //        .Visible(true)
//    //        .Size(5)
//    //        .Symbol(PointSymbol.Circle)
//    //    )
//    //)

//    // Value Axis
//    .CommonAxisSettings(c => c
//        .AllowDecimals(true)
//        .EndOnTick(true)
//    )
//    .ValueAxis(m => m.Add()
//        .AggregatedPointsPosition(AggregatedPointsPosition.CrossTicks)
//        .AllowDecimals(true)
//        .ConstantLines(c => c.Add()
//            .Color("#fc3535")
//            .DashStyle(DevExtreme.AspNet.Mvc.DashStyle.Dash)
//            .Label(l => l
//                .Visible(true)
//                .HorizontalAlignment(DevExtreme.AspNet.Mvc.HorizontalAlignment.Right)
//                .Font(f => f
//                    .Color("#fc3535")
//                    .Size(15)
//                    .Weight(500)
//                )
//                .Text("9 hours")
//            )
//            .Value(9)
//            .Width(2)
//        )
//    )

//    // Crosshair
//    .Crosshair(c => c
//        .Label(l => l.Visible(true))
//    );



//        }
//    }
//}

//        private void ChartViewAverageWorkingHours_OnBeforeRender(DevExtreme.AspNet.Mvc.Builders.WidgetBuilder widget)
//        {
//            ChartBuilder chartBuilder = widget as ChartBuilder;

//            chartBuilder
//                .Palette(VizPalette.DarkMoon)
//                .Export(e => e.Enabled(true))
//                .AutoHidePointMarkers(true)
//                .StickyHovering(false) // Matches JavaScript "stickyHovering": false

//                // Argument Axis Settings
//                .ArgumentAxis(a => a
//                    .AggregatedPointsPosition(AggregatedPointsPosition.CrossTicks)
//                    .Label(l => l
//                        .Position(DevExtreme.AspNet.Mvc.Position.Bottom)  // Matches "position": "bottom"
//                        .TextOverflow(VizTextOverflow.Ellipsis // Matches "textOverflow": "ellipsis"
//                    )
//                ))
//                .SeriesSelectionMode(ChartElementSelectionMode.Single)
//                .CommonSeriesSettings(c => c.DashStyle(DashStyle.Solid).Visible(true).Width(2))
//                // Value Axis Settings

//                .CommonAxisSettings(c => c
//                    .AllowDecimals(true)
//                    .EndOnTick(true)
//                )
//                .ValueAxis(m => m.Add()
//                    .AggregatedPointsPosition(AggregatedPointsPosition.CrossTicks)
//                    .AllowDecimals(true)
//                    .ConstantLines(c => c.Add()
//                        .Color("#fc3535")
//                        .DashStyle(DevExtreme.AspNet.Mvc.DashStyle.Dash)
//                        .Label(l => l
//                            .Visible(true)
//                            .HorizontalAlignment(DevExtreme.AspNet.Mvc.HorizontalAlignment.Right)
//                            .Font(f => f
//                                .Color("#fc3535")
//                                .Size(15)
//                                .Weight(500)
//                            )
//                            .Text("9 hours")
//                        )
//                        .Value(9)
//                        .Width(2)
//                    )
//                )

//                // Crosshair Settings
//                .Crosshair(c => c
//                    .Label(l => l.Visible(true)) // Matches JavaScript "crosshair": { "label": { "visible": true } }
//                )

//                // Series Settings
//                //.CommonSeriesSettings(c => c
//                //    .Type(SeriesType.Line)
//                //    .ArgumentField("Key") // Maps to "arg"
//                //    .ValueField("Data") // Maps to "val"
//                //    .Line(c => c.Point(p => p
//                //        .SelectionMode(SelectionMode.AllSeriesPoints) // Matches "selectionMode": "allSeriesPoints"
//                //        .Symbol(PointSymbol.Polygon) // Matches "symbol": "polygon"
//                //        .Visible(false) // Matches "visible": false
//                //    ))
//                //)

//                // Tooltip Settings
//                .Tooltip(t => t
//                    .Enabled(true)
//                // Matches "location": "edge"
//                );

//                // Legend Settings
//                //.Legend(l => l
//                //    .HorizontalAlignment(HorizontalAlignment.Center) // Matches "horizontalAlignment": "center"
//                //    .ItemTextPosition(DevExtreme.AspNet.Mvc.Position.Right) // Matches "itemTextPosition": "right"
//                //    .VerticalAlignment(VerticalEdge.Bottom) // Matches "verticalAlignment": "bottom"
//                //);
//        }

//    }
//}
//        private void ChartViewAverageWorkingHours_OnBeforeRender(DevExtreme.AspNet.Mvc.Builders.WidgetBuilder widget)
//        {
//            ChartBuilder chartBuilder = widget as ChartBuilder;

//            chartBuilder
//               .Palette(VizPalette.DarkMoon)
//           //    .CommonSeriesSettings(s => s
//          // .ShowInLegend(false)
//       //)
//         //                .Legend(l => l
//         //    .VerticalAlignment(VerticalEdge.Bottom)
//         //    .HorizontalAlignment(HorizontalAlignment.Center)
//         //    .ItemTextPosition(DevExtreme.AspNet.Mvc.Position.Bottom)
//         //)
//         .Export(e => e.Enabled(true))

//        //.Margin(m => m.Bottom(20))
//            .AutoHidePointMarkers(true)
//            .CommonAxisSettings(c => c.AllowDecimals(true).EndOnTick(true))
//            // Specify the series type
//            // Disable data labels
//            // Configure other common series settings as needed


//            //chartBuilder.Margin(m => m.Bottom(20));
//            //chartBuilder.AutoHidePointMarkers(new JS("10"));
//            //chartBuilder.CommonAxisSettings(c => c.DiscreteAxisDivisionMode(DiscreteAxisDivisionMode.BetweenLabels).Color("#FF00FF"));
//            // chartBuilder.ContainerBackgroundColor(new JS(@"#FF0000"));
//            //chartBuilder.CommonSeriesSettings(c => c.ValueField(Name).Color("blue"));
//            //    chartBuilder.ArgumentAxis(a => a
//            //    .ValueMarginsEnabled(false)
//            //    //.DiscreteAxisDivisionMode(DiscreteAxisDivisionMode.CrossLabels)
//            //    .Grid(g => g.Visible(false))
//            //);
//            //.Tooltip(t => t.Enabled(true))
//            //.Crosshair(c => c.Enabled(true))
//            //        .CommonSeriesSettings(s => s

//            //    .Point(p => p.Visible(true))
//            //)
//            //.ArgumentAxis(a => a
//            //            .ValueMarginsEnabled(false)
//            //            .DiscreteAxisDivisionMode(DiscreteAxisDivisionMode.CrossLabels)
//            //            .Grid(g => g.Visible(true))

//            //        )
//            //    )
//            //    .AllowDecimals(true)
//            //)


//            .ValueAxis(m => m.Add()
//                .ConstantLines(c => c.Add()
//                    .Color("#fc3535")
//                    .DashStyle(DevExtreme.AspNet.Mvc.DashStyle.Dash)
//                    .Label(l => l
//                        .Visible(true)
//                        .HorizontalAlignment(DevExtreme.AspNet.Mvc.HorizontalAlignment.Right)
//                        .Font(f => f
//                            .Color("#fc3535")
//                            .Size(15)
//                            .Weight(500)
//                        )
//                        .Text("9 hours")
//                    )
//                    .Value(9)
//                    .Width(2)
//                )
//            );
//        }
//    }
//}
