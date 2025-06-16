using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace OCTWEB_NET45.Models
{
    public class StockBookModel
    {
       
        public int? MasterItemId { get; set; }
        public string ItemNo { get; set; }
        public string ItemShortName { get; set; }
        public string ItemName { get; set; }
        public string ItemGroup { get; set; }
        public string ModelNo { get; set; }
        public int MasterLocationId { get; set; }
        public string LocationCode { get; set; }
        public string LocationName { get; set; }
        public decimal? StockItemQty { get; set; }
        public int EnumLocationTypeId { get; set; }
 
    }

    public class StockItemModel
    {
        public string item_short_name { get; set; }
        public string item_no { get; set; }
        public string date_str { get; set; }
        public List<StockBookModel> StockBookList { get; set; }
        public List<BarChartGroupModel> BarChartGroupData { get; set; }
        public List<HBarChartGroupModel> HBarChartGroupData { get; set; }
    }

    public class BarChartGroupModel
    {
        public List<List<string>> x { get; set; }
        public List<decimal?> y { get; set; }
        public string name { get; set; }
        public string type { get; set; }
        public PlotMarker marker { get; set; }
        public string textposition { get; set; }
        public string orientation { get; set; }
       // public decimal width { get; set; }
    }

    public class PlotMarker
    {
        public string color { get; set; }
        public decimal width { get; set; }
    }

    public class HBarChartGroupModel
    {
        public List<decimal?> x { get; set; }
        public List<List<string>> y { get; set; }
        public string name { get; set; }
        public string type { get; set; }
        public PlotMarker marker { get; set; }
        public string textposition { get; set; }
        public string orientation { get; set; }
        public List<string> text { get; set; }
        public string barmode { get; set; }
        // public decimal width { get; set; }
    }


    public class FGItemModel
    {
        public int id { get; set; }
        public string ItemNo { get; set; }
        public string ItemShortName { get; set; }
        public string ItemName { get; set; }
        public string ModelNo { get; set; }
        public string UnitCode { get; set; }
        public decimal StockQty { get; set; }
        public string StockQty_str { get; set; }
        public decimal Ratio { get; set; }
        public string Ratio_str { get; set; }
        public string LocationRemark { get; set; }
          
    }

    public class FGItemListModel
    {
        public string item_short_name { get; set; }
        public string item_no { get; set; }
        public string model_no { get; set; }
        public string item_name { get; set; }

        public List<FGItemModel> ItemList { get; set; }
    }

    public class MasterStructureItemModel
    {
        public int Id { get; set; }
        public string ItemNo { get; set; }
        public string ModelNo { get; set; }
        public string ItemShortName { get; set; }
        public string ItemName { get; set; }
        public string UnitCode { get; set; }
        public decimal StockQty { get; set; }
        public string StockQty_str { get; set; }
        public decimal Ratio { get; set; }
        public string Ratio_str { get; set; }
        public decimal StockQty_Ratio { get; set; }
        public string StockQty_Ratio_str { get; set; }
        public string LocationRemark { get; set; }
        public int status { get; set; }
        public string VendorCode { get; set; }
        public string BusinessPartnerShortName { get; set; }
        public decimal qty_max { get; set; }
        public string qty_max_str { get; set; }
        public decimal qty_min { get; set; }
        public string qty_min_str { get; set; }
        public decimal qty_avg { get; set; }
        public string qty_avg_str { get; set; }
      
    }

    public class MasterStructureItemListModel
    {
        public int id_parent { get; set; }
        public List<MasterStructureItemModel> ItemList { get; set; }
        public List<ItemStructureJsTreeModel> ItemStructureJsTreeModelList { get; set; }
    }


    public class CommonItemModel
    {
        public int id { get; set; }
        public string ItemNo { get; set; }
        public string ItemShortName { get; set; }
        public string ItemName { get; set; }
        public string ModelNo { get; set; }
        public string UnitCode { get; set; }
        public decimal StockQty { get; set; }
        public string StockQty_str { get; set; }
        public string LocationRemark { get; set; }
        public int status { get; set; }
        public string VendorCode { get; set; }
        public string BusinessPartnerShortName { get; set; }
        public decimal qty_max { get; set; }
        public string qty_max_str { get; set; }
        public decimal qty_min { get; set; }
        public string qty_min_str { get; set; }
        public decimal qty_avg { get; set; }
        public string qty_avg_str { get; set; }

    }

    public class CommonItemListModel
    {
        public int child_id { get; set; }
        public string item_short_name { get; set; }
        public string item_no { get; set; }
        public string model_no { get; set; }
        public string ItemName { get; set; }
        public string UnitCode { get; set; }
        public decimal StockQty { get; set; }
        public string StockQty_str { get; set; }
        public string LocationRemark { get; set; }
        public int status { get; set; }
        public string VendorCode { get; set; }
        public string BusinessPartnerShortName { get; set; }
        public decimal qty_max { get; set; }
        public string qty_max_str { get; set; }
        public decimal qty_min { get; set; }
        public string qty_min_str { get; set; }
        public decimal qty_avg { get; set; }
        public string qty_avg_str { get; set; }

        public List<CommonItemModel> ItemList { get; set; }
    }

    public class StockTimeChartModel
    {
        public List<decimal> y { get; set; }
        public List<string> x { get; set; }
        public string name { get; set; }
        public string type { get; set; }
        public PlotMarker line { get; set; }
        public string textposition { get; set; }
        public string orientation { get; set; }
        public List<string> text { get; set; }
        public string mode { get; set; }
        public string hovertemplate { get; set; }

    }

    public class StockTimeModel
    {
        public int MasterItemId { get; set; }
        public DateTime TransectionDate { get; set; }
        public string TransectionDate_str { get; set; }
        public decimal StockQty { get; set; }
        public string StockQty_str { get; set; }
        public decimal Stock_CumulativeSum { get; set; }
        public string Stock_CumulativeSum_str { get; set; }
        public string ItemNo { get; set; }
        public string ItemShortName { get; set; }
        public string ModelNo { get; set; }
        
    }


    public class StockTimeListModel
    {
        public string first_date_str { get; set; }
        public List<StockTimeChartModel> StockTimeChartData { get; set; }
    }


    public class ItemStructureJsTreeModel
    {
        public string id { get; set; }
        public string text { get; set; }
        public MasterStructureItemModel data { get; set; }
        public string parent { get; set; }
        public ItemStateModel state { get; set; }
        public string type { get; set; }
        //public string type { get; set; }
        //public bool children { get; set; } // if node has sub-nodes set true or not set false
    }

    public class ItemStateModel
    {
        public bool opened { get; set; }
        public bool selected { get; set; }

        public ItemStateModel()
        {
            opened = true;
            selected = false;
        }

    }
}