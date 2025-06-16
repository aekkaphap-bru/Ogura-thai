using OCTWEB_NET45.Context;
using OCTWEB_NET45.Models;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace OCTWEB_NET45.Controllers.Production
{
    public class FccAssyController : Controller
    {
        private Thomas_Ogura_TESTDBEntities tb = new Thomas_Ogura_TESTDBEntities();
        private List<int> locationCode = new List<int> { 72, 112, 115, 116, 129 }; //This is Location not used.   
        private List<string> color_list = new List<string> {
                                                             "#52006A"
                                                            ,"#FBCB0A"
                                                            ,"#37E2D5"
                                                            ,"#379237"
                                                            ,"#3f3e4b"
                                                            ,"#eea9c1"
                                                            ,"#c959b9"
                                                            ,"#8656d2"
                                                            ,"#b2435f"
                                                            ,"#677b62"
                                                            ,"#43a75e"
                                                            ,"#255355"
                                                            ,"#f2c061"
                                                            ,"#e58055"
                                                            ,"#481665"};
        private List<string> color_list2 = new List<string> {
                                                             "#10A19D"
                                                            ,"#FF7000"
                                                            ,"#FF5D9E"
                                                            ,"#FFBF00"
                                                            ,"#8F71FF"
                                                            ,"#8BFFFF"
                                                            ,"#540375"
                                                            ,"#456672"
                                                            ,"#A2C11C"
                                                            ,"#FF847C"
                                                            ,"#FFDC34"
                                                            ,"#A55540"
                                                            ,"#1B435D"
                                                            ,"#66A96B"
                                                            ,"#414141"};
        private Dictionary<string,decimal> target_stock = new Dictionary<string, decimal>()
                            {
                                {"CORE BLANK/DISC",37500},
                                {"Fc Shaving",7500},
                                {"Fc Bonderizing",7500},
                                {"Fc Forging",7500},
                                {"Fc L",7500},
                                {"Fc L(Supplier)",30000},
                                {"Fc Assy AC (No Zinc Plating)",0},
                                {"FLANGE",0},
                                {"Fc Zinc Plating",7500},
                                {"Flange Zinc Plating",7500},
                                {"Fc Assy WP Zinc Plating",7500},
                                {"Fc Assy WP",37500},
                                {"Fcc Semi Assy",7500},
                                {"Fcc Casting",15000},
                                {"Fcc Assy",37500},
                                {"Coil Winding",0},
                                {"MAGNET WIRE",0},
                            };

        //
        // GET: /FccAssy/
        public ActionResult HBarChart()
        {
            StockItemModel model = new StockItemModel();

            string date_str = String.Format("({0})", DateTime.Now.ToString("dd MMMM yyyy, HH:mm"));
            model.date_str = date_str;

            List<StockBookModel> stock = tb.StockBook_Structure_FccAssy_View2023.Select(s => new StockBookModel //.Where(x => x.StockItemQty != 0)
            {
                MasterItemId = s.MasterChildItemId,
                ItemNo = s.ChildItemNo,
                ItemShortName = s.ChildItemShortName,
                ItemName = s.ItemName,
                ItemGroup = s.ItemGroup,
                ModelNo = s.ChildModelNo,
                MasterLocationId = s.MasterLocationId,
                LocationCode = s.LocationCode,
                LocationName = s.LocationName,
                StockItemQty = s.StockItemQty,
                EnumLocationTypeId = s.EnumLocationTypeId,
            }).ToList();

            var item_short_name_list = new List<string>{"CORE BLANK/DISC"
                                                        ,"Fc Shaving"
                                                        ,"Fc Bonderizing"
                                                        ,"Fc Forging"

                                                        ,"Fc L"
                                                        ,"Fc Assy AC (No Zinc Plating)"
                                                        ,"FLANGE"

                                                        ,"Fc Zinc Plating"
                                                        ,"Flange Zinc Plating"
                                                        ,"Fc Assy WP Zinc Plating"

                                                        ,"Fc Assy WP"
                                                        ,"FIELD CORE ASSY"

                                                        ,"Fcc Semi Assy"
                                                        ,"Fcc Casting"

                                                        ,"Coil Winding"};
            var color_list = new List<string> {
                     "#362222"
                    ,"#282A3A"
                    ,"#423F3E"
                    ,"#735F32"

                    ,"#002B5B"
                    ,"#2B4865"
                    ,"#256D85"

                    ,"#285430"
                    ,"#395144"
                    ,"#A4BE7B"

                    ,"#F9B208"
                    ,"#FC5404"

                    ,"#FF5858"
                    ,"#E0144C"

                    ,"#460C68"};

            var color_list2 = new List<string> {
                     "#472D2D"
                    ,"#553939"
                    ,"#704F4F"
                    ,"#A77979"

                    ,"#B1B2FF"
                    ,"#AAC4FF"
                    ,"#D2DAFF"

                    ,"#A4BE7B"
                    ,"#5F8D4E"
                    ,"#82CD47"

                    ,"#F0FF42"
                    ,"#F98404"

                    ,"#FF5858"
                    ,"#FF5858"

                    ,"#FF97C1"};

            /*var color_list = new List<string> {
                     "#f45a3e"
                    ,"#fdb66e"
                    ,"#817141"
                    ,"#52635d"

                    ,"#3f3e4b"
                    ,"#eea9c1"
                    ,"#c959b9"

                    ,"#8656d2"
                    ,"#b2435f"
                    ,"#677b62"

                    ,"#43a75e"
                    ,"#255355"

                    ,"#f2c061"
                    ,"#e58055"

                    ,"#481665"};*/
            /*
            var color_list2 = new List<string> {
                    "#906644"
                    ,"#605654"
                    ,"#304664"
                    ,"#50b6a9"
                    ,"#003674"
                    ,"#eea9c1"
                    ,"#c959b9"
                    ,"#8656d2"
                    ,"#406cb6"
                    ,"#43a75e"
                    ,"#83ba4f"
                    ,"#f2c061"
                    ,"#e58055"
                    ,"#d84b4b"
                    ,"#c07634"};
            */

            List<HBarChartGroupModel> bar_chart_list = new List<HBarChartGroupModel>();
           
            foreach (var item_short_name in item_short_name_list.Select((value, i) => new { i, value }))
            {

                //Supplier
                var item_group_supplier = stock.Where(x => x.ItemShortName == item_short_name.value && x.EnumLocationTypeId == 3)
                    .GroupBy(s => new { s.ModelNo, s.ItemNo, s.ItemShortName })
                    .Select(g => new
                    {
                        ModelNo = g.Key.ModelNo,
                        ItemNo = g.Key.ItemNo,
                        ItemShortName = g.Key.ItemShortName,
                        StockItemQty = g.Sum(x => Math.Round(Convert.ToDecimal(x.StockItemQty), 2))
                    }).OrderBy(o=>o.ModelNo).ToList();
                //Local OCT
                var item_group_local = stock.Where(x => x.ItemShortName == item_short_name.value && x.EnumLocationTypeId != 3)
                   .GroupBy(s => new { s.ModelNo, s.ItemNo, s.ItemShortName })
                   .Select(g => new
                   {
                       ModelNo = g.Key.ModelNo,
                       ItemNo = g.Key.ItemNo,
                       ItemShortName = g.Key.ItemShortName,
                       StockItemQty = g.Sum(x => Math.Round(Convert.ToDecimal(x.StockItemQty), 2))
                   }).OrderBy(o=>o.ModelNo).ToList();

                if (item_group_local.Any())
                {
                    HBarChartGroupModel bar_chart = new HBarChartGroupModel();
                    var x_values = new List<decimal?>();
                    var y_values = new List<List<string>>();
                    var y_value_1 = new List<string>();
                    var y_value_2 = new List<string>();
                    var text_list = new List<string>();

                    foreach (var i in item_group_local)
                    {
                        x_values.Add(i.StockItemQty);
                        y_value_1.Add(item_short_name.value);
                        y_value_2.Add(i.ModelNo);
                        text_list.Add(i.StockItemQty.ToString("N0"));
                    }
                    y_values.Add(y_value_1);
                    y_values.Add(y_value_2);

                    bar_chart.x = x_values;
                    bar_chart.y = y_values;
                    bar_chart.name = item_short_name.value;
                    bar_chart.type = "bar";
                    PlotMarker plotmarker = new PlotMarker();
                    plotmarker.width = 0;
                    plotmarker.color = color_list[item_short_name.i];
                    bar_chart.marker = plotmarker;
                    bar_chart.textposition = "right";
                    bar_chart.orientation = "h";
                    // bar_chart.width = 1;
                    bar_chart.text = text_list;
                    bar_chart.barmode = "stack";
                    bar_chart_list.Add(bar_chart);
                }

                if (item_group_supplier.Any())
                {
                    HBarChartGroupModel bar_chart = new HBarChartGroupModel();
                    var x_values = new List<decimal?>();
                    var y_values = new List<List<string>>();
                    var y_value_1 = new List<string>();
                    var y_value_2 = new List<string>();
                    var text_list = new List<string>();

                    foreach (var i in item_group_supplier)
                    {
                        x_values.Add(i.StockItemQty);
                        y_value_1.Add(String.Concat(item_short_name.value, "-Supplier"));
                        y_value_2.Add(i.ModelNo);
                        text_list.Add(i.StockItemQty.ToString("N0"));
                    }
                    y_values.Add(y_value_1);
                    y_values.Add(y_value_2);

                    bar_chart.x = x_values;
                    bar_chart.y = y_values;
                    bar_chart.name = String.Concat(item_short_name.value, "-Supplier");
                    bar_chart.type = "bar";
                    PlotMarker plotmarker = new PlotMarker();
                    plotmarker.width = 0;
                    plotmarker.color = color_list2[item_short_name.i];
                    bar_chart.marker = plotmarker;
                    bar_chart.textposition = "right";
                    bar_chart.orientation = "h";
                    // bar_chart.width = 1;
                    bar_chart.text = text_list;
                    bar_chart.barmode = "stack";
                    bar_chart_list.Add(bar_chart);

                }
            }

            model.HBarChartGroupData = bar_chart_list;

            model.StockBookList = stock;

            return View(model);
        }

        //
        //GET: StockTimeChart
        public ActionResult StockMovementChart()
        {
            try
            {                              
                var itemShortName = "CORE BLANK/DISC";
                StockTimeListModel model = new StockTimeListModel();

                List<StockTimeChartModel> stc_list = new List<StockTimeChartModel>();
               
                //Set Date
                DateTime to_date = DateTime.Today;
                DateTime first_day_months = new DateTime(to_date.Year, to_date.Month, 1);
                DateTime first_day_months_3 = first_day_months.AddMonths(-3);
                DateTime end_day_months = first_day_months_3.AddDays(-1);

                //Get Data List
                stc_list = DataLocal(stc_list, itemShortName, first_day_months_3, end_day_months);
                stc_list = stc_list.OrderBy(o => o.name).ToList();

                //Target in stock plot
                StockTimeChartModel stc_target = new StockTimeChartModel();
                List<decimal> y_axis_target = new List<decimal>() { target_stock[itemShortName], target_stock[itemShortName] };
                List<string> x_axis_target = new List<string>() {end_day_months.ToString("yyyy-MM-dd"),to_date.ToString("yyyy-MM-dd") };
                List<string> text_target = new List<string>() 
                    { 
                        String.Format("Target in stock: {0}",target_stock[itemShortName].ToString("N0"))
                        , String.Format("Target in stock: {0}",target_stock[itemShortName].ToString("N0")) 
                    };
                stc_target.y = y_axis_target;
                stc_target.x = x_axis_target;
                stc_target.text = text_target;
                stc_target.type = "scatter";
                stc_target.name = "Target Stock";
                PlotMarker plot_target = new PlotMarker();
                plot_target.color = "rgb(219, 64, 82)";
                plot_target.width = 4;
                stc_target.line = plot_target;
                stc_target.mode = "lines";

                stc_list.Add(stc_target);

                model.first_date_str = end_day_months.ToString("yyyy-MM-dd");
                model.StockTimeChartData = stc_list;

                return View(model);
            }
            catch(Exception ex)
            {
                ModelState.AddModelError("", ex.Message);
                ViewBag.errorMessage = ex.ToString();
                return View("Error");
            }
        }

        //
        //GET: GetDataChart
        public JsonResult GetDataChart(string itemShortName)
        {
            try
            {
                List<StockTimeChartModel> stc_list = new List<StockTimeChartModel>();
               
                //Set Date
                DateTime to_date = DateTime.Today;
                DateTime first_day_months = new DateTime(to_date.Year, to_date.Month, 1);
                DateTime first_day_months_3 = first_day_months.AddMonths(-3);
                DateTime end_day_months = first_day_months_3.AddDays(-1);

                //Get Data List
                stc_list = DataLocal(stc_list, itemShortName, first_day_months_3, end_day_months);
                stc_list = stc_list.OrderBy(o => o.name).ToList();

                //Target in stock plot
                StockTimeChartModel stc_target = new StockTimeChartModel();
                List<decimal> y_axis_target = new List<decimal>() { target_stock[itemShortName], target_stock[itemShortName] };
                List<string> x_axis_target = new List<string>() { end_day_months.ToString("yyyy-MM-dd"), to_date.ToString("yyyy-MM-dd") };
                List<string> text_target = new List<string>() 
                    { 
                        String.Format("Target in stock: {0}",target_stock[itemShortName].ToString("N0"))
                        , String.Format("Target in stock: {0}",target_stock[itemShortName].ToString("N0")) 
                    };
                stc_target.y = y_axis_target;
                stc_target.x = x_axis_target;
                stc_target.text = text_target;
                stc_target.type = "scatter";
                stc_target.name = "Target Stock";
                PlotMarker plot_target = new PlotMarker();
                plot_target.color = "rgb(219, 64, 82)";
                plot_target.width = 4;
                stc_target.line = plot_target;
                stc_target.mode = "lines";

                stc_list.Add(stc_target);

                return Json(stc_list,JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        //
        //GET: GetDataChartSupplier
        public JsonResult GetDataChartSupplier(string itemShortName)
        {
            try
            {
                var itemShortNameSupplier = "Fc L";

                List<StockTimeChartModel> stc_list = new List<StockTimeChartModel>();
               
                //Set Date
                DateTime to_date = DateTime.Today;
                DateTime first_day_months = new DateTime(to_date.Year, to_date.Month, 1);
                DateTime first_day_months_3 = first_day_months.AddMonths(-3);
                DateTime end_day_months = first_day_months_3.AddDays(-1);

                //Get Data List
                stc_list = DataSupplier(stc_list, itemShortNameSupplier, first_day_months_3, end_day_months);
                stc_list = stc_list.OrderBy(o => o.name).ToList();

                //Target in stock plot
                StockTimeChartModel stc_target = new StockTimeChartModel();
                List<decimal> y_axis_target = new List<decimal>() { target_stock[itemShortName], target_stock[itemShortName] };
                List<string> x_axis_target = new List<string>() { end_day_months.ToString("yyyy-MM-dd"), to_date.ToString("yyyy-MM-dd") };
                List<string> text_target = new List<string>() 
                    { 
                        String.Format("Target in stock: {0}",target_stock[itemShortName].ToString("N0"))
                        , String.Format("Target in stock: {0}",target_stock[itemShortName].ToString("N0")) 
                    };
                stc_target.y = y_axis_target;
                stc_target.x = x_axis_target;
                stc_target.text = text_target;
                stc_target.type = "scatter";
                stc_target.name = "Target Stock";
                PlotMarker plot_target = new PlotMarker();
                plot_target.color = "rgb(219, 64, 82)";
                plot_target.width = 4;
                stc_target.line = plot_target;
                stc_target.mode = "lines";

                stc_list.Add(stc_target);

                return Json(stc_list, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        //
        //GET: GetDataChartAll
        public JsonResult GetDataChartAll(string itemShortName)
        {
            try
            {
                List<StockTimeChartModel> stc_list = new List<StockTimeChartModel>();
                //Set Date
                DateTime to_date = DateTime.Today;
                DateTime first_day_months = new DateTime(to_date.Year, to_date.Month, 1);
                DateTime first_day_months_3 = first_day_months.AddMonths(-3);
                DateTime end_day_months = first_day_months_3.AddDays(-1);

                //Get Data List
                stc_list = DataLocal(stc_list,itemShortName, first_day_months_3, end_day_months);
                stc_list = DataSupplier(stc_list, itemShortName, first_day_months_3, end_day_months);
                stc_list = stc_list.OrderBy(o => o.name).ToList();

                //Target in stock plot
                StockTimeChartModel stc_target = new StockTimeChartModel();
                List<decimal> y_axis_target = new List<decimal>() { target_stock[itemShortName], target_stock[itemShortName] };
                List<string> x_axis_target = new List<string>() { end_day_months.ToString("yyyy-MM-dd"), to_date.ToString("yyyy-MM-dd") };
                List<string> text_target = new List<string>() 
                    { 
                        String.Format("Target in stock: {0}",target_stock[itemShortName].ToString("N0"))
                        , String.Format("Target in stock: {0}",target_stock[itemShortName].ToString("N0")) 
                    };
                stc_target.y = y_axis_target;
                stc_target.x = x_axis_target;
                stc_target.text = text_target;
                stc_target.type = "scatter";
                stc_target.name = "Target Stock";
                PlotMarker plot_target = new PlotMarker();
                plot_target.color = "rgb(219, 64, 82)";
                plot_target.width = 4;
                stc_target.line = plot_target;
                stc_target.mode = "lines";

                stc_list.Add(stc_target);

                return Json(stc_list, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        //get data in local 
        public List<StockTimeChartModel> DataLocal(List<StockTimeChartModel> stc_list, string itemShortName,DateTime first_day_months_3,DateTime end_day_months)
        {
            try
            {
                List<int> item_id_list = tb.MasterItems.Where(x => x.IsActive == true && x.ItemShortName == itemShortName)
                     .Select(s => s.Id).ToList();
              
                //Inventory Records Query
                IEnumerable<InventoryRecord> ir_query = tb.InventoryRecords.Where(x => x.TransactionDate >= first_day_months_3
                    && x.ItemQty != 0
                    && !locationCode.Contains(x.MasterLocationId)
                    && item_id_list.Contains(x.MasterItemId)
                    && x.MasterLocation.EnumLocationTypeId != 3);

                var stock_time = ir_query.GroupBy(s => new { s.MasterItemId, s.TransactionDate })
                    .Select(g => new StockTimeModel
                    {
                        MasterItemId = g.Key.MasterItemId,
                        TransectionDate = g.Key.TransactionDate,
                        StockQty = g.Sum(s => s.ItemQty)
                    }).OrderBy(o => o.TransectionDate).ToList();
                //Select Item
                List<int> item_id_list_selected = stock_time.Select(s => s.MasterItemId).Distinct().ToList();
               
                foreach (var item_id in item_id_list_selected.Select((value, i) => new { i, value }))
                {
                    StockTimeChartModel stc = new StockTimeChartModel();
                    List<decimal> y_axis = new List<decimal>();
                    List<string> x_axis = new List<string>();
                    List<string> text = new List<string>();

                    var st = stock_time.Where(x => x.MasterItemId == item_id.value).ToList();
                    //Item detail
                    var item_detail = tb.MasterItems.Where(x => x.Id == item_id.value).FirstOrDefault();

                    //Stocktaking Query
                    List<Decimal> stocktaking_query = tb.StocktakingActualDetails
                        .Join(tb.StocktakingActualHeaders, sad => sad.StocktakingActualHeaderId, sah => sah.Id, (sad, sah) => new { sad = sad, sah = sah })
                        .Where(x => x.sad.MasterItemId == item_id.value
                            && !locationCode.Contains(x.sad.MasterLocationId)
                            && x.sah.StocktakingDate == end_day_months
                            && x.sad.MasterLocation.EnumLocationTypeId != 3)
                        .Select(s => s.sad.ActualStockItemQty).ToList();
                    Decimal stocktaking_sum = stocktaking_query.Any() ? stocktaking_query.Sum() : 0;

                    //Add first Qty/Date
                    y_axis.Add(stocktaking_sum);
                    x_axis.Add(end_day_months.ToString("yyyy-MM-dd"));
                    text.Add(String.Format("Beginning:{0}", stocktaking_sum.ToString("N0")));

                    Decimal CumulativeSum = stocktaking_sum;
                    foreach (var stock in st)
                    {
                        CumulativeSum = CumulativeSum + stock.StockQty;
                        y_axis.Add(CumulativeSum);
                        x_axis.Add(stock.TransectionDate.ToString("yyyy-MM-dd"));
                        text.Add(String.Format("Change: {0}", stock.StockQty.ToString("N0")));
                    }

                    stc.x = x_axis;
                    stc.y = y_axis;
                    stc.text = text;
                    stc.type = "scatter";
                    stc.name = item_detail.ModelNo;
                    PlotMarker plot = new PlotMarker();
                    plot.color = color_list[item_id.i % 15];
                    plot.width = 2;
                    stc.line = plot;
                    stc.mode = "lines+markers";
                    stc.hovertemplate = "<i>Date: %{x}</i>" +
                                        "<br><b>%{text}</b>" +
                                        "<br><b>Current Qty: %{y:,.0f}</b>";

                    stc_list.Add(stc);
                }

                return stc_list;
            }
            catch(Exception ex)
            {
                throw ex;
            }
        }

        //get data in supplier 
        public List<StockTimeChartModel> DataSupplier(List<StockTimeChartModel> stc_list, string itemShortName, DateTime first_day_months_3, DateTime end_day_months)
        {
            try
            {
                List<int> item_id_list = tb.MasterItems.Where(x => x.IsActive == true && x.ItemShortName == itemShortName)
                     .Select(s => s.Id).ToList();
               
                //Inventory Records Query
                IEnumerable<InventoryRecord> ir_query = tb.InventoryRecords.Where(x => x.TransactionDate >= first_day_months_3
                    && x.ItemQty != 0
                    && !locationCode.Contains(x.MasterLocationId)
                    && item_id_list.Contains(x.MasterItemId)
                    && x.MasterLocation.EnumLocationTypeId == 3);

                var stock_time = ir_query.GroupBy(s => new { s.MasterItemId, s.TransactionDate })
                    .Select(g => new StockTimeModel
                    {
                        MasterItemId = g.Key.MasterItemId,
                        TransectionDate = g.Key.TransactionDate,
                        StockQty = g.Sum(s => s.ItemQty)
                    }).OrderBy(o => o.TransectionDate).ToList();
                //Select Item
                List<int> item_id_list_selected = stock_time.Select(s => s.MasterItemId).Distinct().ToList();
               
                foreach (var item_id in item_id_list_selected.Select((value, i) => new { i, value }))
                {
                    StockTimeChartModel stc = new StockTimeChartModel();
                    List<decimal> y_axis = new List<decimal>();
                    List<string> x_axis = new List<string>();
                    List<string> text = new List<string>();

                    var st = stock_time.Where(x => x.MasterItemId == item_id.value).ToList();
                    //Item detail
                    var item_detail = tb.MasterItems.Where(x => x.Id == item_id.value).FirstOrDefault();

                    //Stocktaking Query
                    List<Decimal> stocktaking_query = tb.StocktakingActualDetails
                        .Join(tb.StocktakingActualHeaders, sad => sad.StocktakingActualHeaderId, sah => sah.Id, (sad, sah) => new { sad = sad, sah = sah })
                        .Where(x => x.sad.MasterItemId == item_id.value
                            && !locationCode.Contains(x.sad.MasterLocationId)
                            && x.sah.StocktakingDate == end_day_months
                            && x.sad.MasterLocation.EnumLocationTypeId == 3)
                        .Select(s => s.sad.ActualStockItemQty).ToList();
                    Decimal stocktaking_sum = stocktaking_query.Any() ? stocktaking_query.Sum() : 0;

                    //Add first Qty/Date
                    y_axis.Add(stocktaking_sum);
                    x_axis.Add(end_day_months.ToString("yyyy-MM-dd"));
                    text.Add(String.Format("Beginning:{0}", stocktaking_sum.ToString("N0")));

                    Decimal CumulativeSum = stocktaking_sum;
                    foreach (var stock in st)
                    {
                        CumulativeSum = CumulativeSum + stock.StockQty;
                        y_axis.Add(CumulativeSum);
                        x_axis.Add(stock.TransectionDate.ToString("yyyy-MM-dd"));
                        text.Add(String.Format("Change: {0}", stock.StockQty.ToString("N0")));
                    }
                    stc.x = x_axis;
                    stc.y = y_axis;
                    stc.text = text;
                    stc.type = "scatter";
                    stc.name = String.Concat(item_detail.ModelNo,"-Supplier");
                    PlotMarker plot = new PlotMarker();
                    plot.color = color_list2[item_id.i % 15];
                    plot.width = 2;
                    stc.line = plot;
                    stc.mode = "lines+markers";
                    stc.hovertemplate = "<i>Date: %{x}</i>" +
                                        "<br><b>%{text}</b>" +
                                        "<br><b>Current Qty: %{y:,.0f}</b>";
                    stc_list.Add(stc);
                }

                return stc_list;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }


        protected override void Dispose(bool disposing)
        {
            tb.Dispose();
            base.Dispose(disposing);
        }
       

	}
}