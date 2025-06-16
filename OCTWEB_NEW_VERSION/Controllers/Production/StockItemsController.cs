using OCTWEB_NET45.Context;
using OCTWEB_NET45.Models;
using OfficeOpenXml;
using OfficeOpenXml.Style;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace OCTWEB_NET45.Controllers.Production
{
    public class StockItemsController : Controller
    {

        private Thomas_Ogura_TESTDBEntities tb = new Thomas_Ogura_TESTDBEntities();
        private List<int> locationCode = new List<int> { 72, 112, 115, 116, 129 };
        private Dictionary<string, int> itemOrder = new Dictionary<string, int>()
                            {
                                {"CORE BLANK/DISC",1},
                                {"Fc Shaving",2},
                                {"Fc Bonderizing",3},
                                {"Fc Forging",4},
                                {"Fc L",5},
                                {"Fc Assy AC (No Zinc Plating)",6},
                                {"FLANGE",7},
                                {"Fc Zinc Plating",8},
                                {"Flange Zinc Plating",9},
                                {"Fc Assy WP Zinc Plating",10},
                                {"Fc Assy WP",11},
                                {"FIELD CORE ASSY",12},
                                {"Fcc Semi Assy",13},
                                {"Fcc Casting",14},
                                {"Coil Winding",15},
                                {"Fcc Assy",16}                                
                            };
        private List<string> selectedItemShortName = new List<string>()
                                {
                                    "CORE BLANK/DISC",
                                    "Fc Shaving",
                                    "Fc Bonderizing",
                                    "Fc Forging",
                                    "Fc L",
                                    "Fc Assy AC (No Zinc Plating)",
                                    "FLANGE",
                                    "Fc Zinc Plating",
                                    "Flange Zinc Plating",
                                    "Fc Assy WP Zinc Plating",
                                    "Fc Assy WP",
                                    "FIELD CORE ASSY",
                                    "Fcc Semi Assy",
                                    "Fcc Casting",
                                    "Coil Winding",
                                    "Fcc Assy",
                                };

        //
        //GET: StockItems/ItemList
        public ActionResult ItemList()
        {
            try
            {
                FGItemListModel model = new FGItemListModel();
                IEnumerable<MasterItem> query = tb.MasterItems.Where(x => x.IsActive == true).Take(0).ToList();
                if (!String.IsNullOrEmpty(model.item_short_name))
                {
                    query = query.Where(x => x.ItemShortName == model.item_short_name).ToList();
                }
                if (!String.IsNullOrEmpty(model.item_no))
                {
                    query = query.Where(x => x.ItemNo == model.item_no).ToList();
                }
                if (!String.IsNullOrEmpty(model.model_no))
                {
                    query = query.Where(x => x.ModelNo == model.model_no).ToList();
                }
                if (!String.IsNullOrEmpty(model.item_name))
                {
                    query = query.Where(x => x.ItemName == model.item_name).ToList();
                }
                var item_list = query.Select(s => new FGItemModel
                {
                    id = s.Id,
                    ItemNo = s.ItemNo,
                    ItemName = s.ItemName,
                    ItemShortName = s.ItemShortName,
                    ModelNo = s.ModelNo,
                }).ToList();

                model.ItemList = item_list;

                return View(model);
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", ex.Message);
                ViewBag.errorMessage = ex.ToString();
                return View("Error");
            }
        }
        //
        // POST: StockItems/ItemList
        [HttpPost]
        public ActionResult ItemList(FGItemListModel model)
        {
            try
            {
                if (String.IsNullOrEmpty(model.item_short_name)
                    && String.IsNullOrEmpty(model.item_no)
                    && String.IsNullOrEmpty(model.model_no)
                    && String.IsNullOrEmpty(model.item_name))
                {
                    return View(model);
                }

                IEnumerable<MasterItem> query = tb.MasterItems.Where(x => x.IsActive == true).ToList();
                if (!String.IsNullOrEmpty(model.item_short_name))
                {
                    query = query.Where(x => x.ItemShortName == model.item_short_name).ToList();
                }
                if (!String.IsNullOrEmpty(model.item_no))
                {
                    query = query.Where(x => x.ItemNo == model.item_no).ToList();
                }
                if (!String.IsNullOrEmpty(model.model_no))
                {
                    query = query.Where(x => x.ModelNo == model.model_no).ToList();
                }
                if (!String.IsNullOrEmpty(model.item_name))
                {
                    query = query.Where(x => x.ItemName == model.item_name).ToList();
                }
                var item_list = query.Select(s => new FGItemModel
                {
                    id = s.Id,
                    ItemNo = s.ItemNo,
                    ItemName = s.ItemName,
                    ItemShortName = s.ItemShortName,
                    ModelNo = s.ModelNo,
                    UnitCode = s.MasterUnit.UnitCode,

                }).ToList();

                foreach (var i in item_list)
                {
                    var stockbook = tb.StockBooks.Where(x => x.MasterItemId == i.id && x.StockItemQty != 0 && !locationCode.Contains(x.MasterLocationId)).ToList();
                    var stock_qty = stockbook.Select(s => s.StockItemQty).Sum();
                    string remark_location = "";
                    if (stockbook.Any())
                    {
                        List<string> stockbook_grouped = stockbook.GroupBy(g => g.MasterLocationId)
                            .Select(s => String.Concat((from ml in tb.MasterLocations where ml.Id == s.Key select ml.LocationCode).FirstOrDefault()
                                    , "("
                                    , Convert.ToDecimal(s.Sum(j => j.StockItemQty)).ToString("N")
                                    , ")")
                                    ).ToList();
                        remark_location = String.Join(", ", stockbook_grouped);

                    }
                    i.LocationRemark = remark_location;
                    i.StockQty = stock_qty;
                    i.StockQty_str = stock_qty.ToString("N");              
                    
                }

                model.ItemList = item_list;

                return View(model);
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", ex.Message);
                ViewBag.errorMessage = ex.ToString();
                return View("Error");
            }
        }

        //
        //GET: /StockItems/ItemDetail
        public ActionResult ItemDetail(int id)
        {
            try
            {
                MasterStructureItemListModel model = new MasterStructureItemListModel();

                var item_parent = tb.MasterItems.Where(x => x.Id == id)
                    .Select(s=> new {itemNo = s.ItemNo, modelNo = s.ModelNo, itemShortName = s.ItemShortName, unitCode = s.MasterUnit.UnitCode})
                    .FirstOrDefault();
                if(item_parent == null)
                {
                    ViewBag.errorMessage = "Item parent is null.";
                    return View("Error");
                }
                
                List<MasterStructureItemModel> itemChild = new List<MasterStructureItemModel>();
                itemChild = GetChild(id, itemChild);

                //Stock Qty Parent
                var stockbook = tb.StockBooks.Where(x => x.MasterItemId == id && x.StockItemQty != 0 && !locationCode.Contains(x.MasterLocationId)).ToList();
                var stock_qty = stockbook.Select(s => s.StockItemQty).Sum();
                string remark_location = "";
                if (stockbook.Any())
                {
                    List<string> stockbook_grouped = stockbook.GroupBy(g => g.MasterLocationId)
                        .Select(s => String.Concat((from ml in tb.MasterLocations where ml.Id == s.Key select ml.LocationCode).FirstOrDefault()
                                , "("
                                , Convert.ToDecimal(s.Sum(j => j.StockItemQty)).ToString("N")
                                , ")")
                                ).ToList();
                    remark_location = String.Join(", ", stockbook_grouped);

                }
                //Add Parent
                MasterStructureItemModel parent_first = new MasterStructureItemModel();
                parent_first.Id = id;
                parent_first.ItemNo = item_parent.itemNo;
                parent_first.ItemShortName = item_parent.itemShortName;
                parent_first.ModelNo = item_parent.modelNo;
                parent_first.UnitCode = item_parent.unitCode;
                parent_first.StockQty = stock_qty;
                parent_first.StockQty_str = stock_qty.ToString("N");
                parent_first.LocationRemark = remark_location;
                parent_first.status = 20;
                //Set Vendor
                var vendor_query = tb.ReceivingActualDetails.Where(x => x.MasterItemId == id).OrderByDescending(o => o.Id).Take(1)
                    .Select(s => new
                    {
                        vendorCode = s.ReceivingActualHeader.MasterVendor.VendorCode,
                        businessPartnerShortName = s.ReceivingActualHeader.MasterVendor.MasterBusinessPartner.BusinessPartnerShortName,
                    }).FirstOrDefault();
                if(vendor_query != null)
                {
                    parent_first.VendorCode = vendor_query.vendorCode;
                    parent_first.BusinessPartnerShortName = vendor_query.businessPartnerShortName;
                }
                else
                {
                    parent_first.VendorCode = "";
                    parent_first.BusinessPartnerShortName = "";
                }

                //Set Max, Min, Avg Qty
                DateTime todate = DateTime.Today;
                DateTime first_date = new DateTime(todate.Year,todate.Month,1);
                DateTime first_date_1 = first_date.AddMonths(-1);
                DateTime end_date_1 = new DateTime(first_date_1.Year,first_date_1.Month,DateTime.DaysInMonth(first_date_1.Year,first_date_1.Month));
            
                var mfg_qurey = tb.MfgMaterialsIssueDetails.Where(x => x.MasterItemId == id && x.IssueDate >= first_date_1 && x.IssueDate <= end_date_1)
                    .GroupBy(g => g.IssueDate)
                    .Select(s => new { IssueDate = s.Key, ItemQty = s.Sum(j=>j.ItemQty) }).ToList();

                List<decimal> mfg_list = mfg_qurey.Select(s => s.ItemQty).ToList();
                if(mfg_list.Any())
                {
                    decimal qty_max = mfg_list.Max();
                    decimal qty_min = mfg_list.Min();
                    decimal qty_avg = mfg_list.Average();

                    parent_first.qty_max = qty_max;
                    parent_first.qty_max_str = qty_max.ToString("N");
                    parent_first.qty_min = qty_min;
                    parent_first.qty_min_str = qty_min.ToString("N");
                    parent_first.qty_avg = qty_avg;
                    parent_first.qty_avg_str = qty_avg.ToString("N");
                }
                else
                {
                    parent_first.qty_max_str = "";
                    parent_first.qty_min_str = "";
                    parent_first.qty_avg_str = "";
                }


                itemChild.Add(parent_first);

                model.ItemList = itemChild.OrderByDescending(o => o.status).ToList();
                model.id_parent = id;

                /*
                //Get tree child Recursive
                List<ItemStructureJsTreeModel> tree_node = new List<ItemStructureJsTreeModel>();
                //Add Main Root
                tree_node.Add(new ItemStructureJsTreeModel
                {
                    id = id.ToString(),
                    parent = "#",
                    text = String.Concat(item_parent.itemShortName, "_", item_parent.modelNo),
                    state = new ItemStateModel() { selected = false },
                    type = "root",
                });
                tree_node = GetTree(id, tree_node);
                model.ItemStructureJsTreeModelList = tree_node;
                */
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
        //GET: /StockItems/GetDatail
        public JsonResult GetDatail(int id)
        {
            try
            {
                List<MasterStructureItemModel> result = new List<MasterStructureItemModel>();
                result = GetChild(id, result);

                return Json(result.OrderByDescending(o=>o.status).ToList(), JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                throw ex;
            }

        }

        //
        //GET: /StockItems/ExportStructure
        public void ExportStructure(int id)
        {
            try
            {
                var item_parent = tb.MasterItems.Where(x => x.Id == id)
                    .Select(s => new { itemNo = s.ItemNo, modelNo = s.ModelNo, itemShortName = s.ItemShortName, unitCode = s.MasterUnit.UnitCode })
                    .FirstOrDefault();

                List<MasterStructureItemModel> itemChild = new List<MasterStructureItemModel>();
                itemChild = GetChild(id, itemChild);

                //Stock Qty Parent
                var stockbook = tb.StockBooks.Where(x => x.MasterItemId == id && x.StockItemQty != 0 && !locationCode.Contains(x.MasterLocationId)).ToList();
                var stock_qty = stockbook.Select(s => s.StockItemQty).Sum();
                string remark_location = "";
                if (stockbook.Any())
                {
                    List<string> stockbook_grouped = stockbook.GroupBy(g => g.MasterLocationId)
                        .Select(s => String.Concat((from ml in tb.MasterLocations where ml.Id == s.Key select ml.LocationCode).FirstOrDefault()
                                , "("
                                , Convert.ToDecimal(s.Sum(j => j.StockItemQty)).ToString("N")
                                , ")")
                                ).ToList();
                    remark_location = String.Join(", ", stockbook_grouped);

                }
                //Add Parent
                MasterStructureItemModel parent_first = new MasterStructureItemModel();
                parent_first.Id = id;
                parent_first.ItemNo = item_parent.itemNo;
                parent_first.ItemShortName = item_parent.itemShortName;
                parent_first.ModelNo = item_parent.modelNo;
                parent_first.UnitCode = item_parent.unitCode;
                parent_first.StockQty = stock_qty;
                parent_first.StockQty_str = stock_qty.ToString("N");
                parent_first.LocationRemark = remark_location;
                parent_first.status = 20;
                //Set Vendor
                var vendor_query = tb.ReceivingActualDetails.Where(x => x.MasterItemId == id).OrderByDescending(o => o.Id).Take(1)
                    .Select(s => new
                    {
                        vendorCode = s.ReceivingActualHeader.MasterVendor.VendorCode,
                        businessPartnerShortName = s.ReceivingActualHeader.MasterVendor.MasterBusinessPartner.BusinessPartnerShortName,
                    }).FirstOrDefault();
                if (vendor_query != null)
                {
                    parent_first.VendorCode = vendor_query.vendorCode;
                    parent_first.BusinessPartnerShortName = vendor_query.businessPartnerShortName;
                }
                else
                {
                    parent_first.VendorCode = "";
                    parent_first.BusinessPartnerShortName = "";
                }
                //Set Max, Min, Avg Qty
                DateTime todate = DateTime.Today;
                DateTime first_date = new DateTime(todate.Year, todate.Month, 1);
                DateTime first_date_1 = first_date.AddMonths(-1);
                DateTime end_date_1 = new DateTime(first_date_1.Year, first_date_1.Month, DateTime.DaysInMonth(first_date_1.Year, first_date_1.Month));

                var mfg_qurey = tb.MfgMaterialsIssueDetails.Where(x => x.MasterItemId == id && x.IssueDate >= first_date_1 && x.IssueDate <= end_date_1)
                    .GroupBy(g => g.IssueDate)
                    .Select(s => new { IssueDate = s.Key, ItemQty = s.Sum(j => j.ItemQty) }).ToList();

                List<decimal> mfg_list = mfg_qurey.Select(s => s.ItemQty).ToList();
                if (mfg_list.Any())
                {
                    decimal qty_max = mfg_list.Max();
                    decimal qty_min = mfg_list.Min();
                    decimal qty_avg = mfg_list.Average();

                    parent_first.qty_max = qty_max;
                    parent_first.qty_max_str = qty_max.ToString("N");
                    parent_first.qty_min = qty_min;
                    parent_first.qty_min_str = qty_min.ToString("N");
                    parent_first.qty_avg = qty_avg;
                    parent_first.qty_avg_str = qty_avg.ToString("N");
                }
                else
                {
                    parent_first.qty_max_str = "";
                    parent_first.qty_min_str = "";
                    parent_first.qty_avg_str = "";
                }

                itemChild.Add(parent_first);

                ExportToExcel(itemChild.OrderByDescending(o => o.status).ToList());

            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", ex.Message);
                throw;
            }
        }

        //
        //GET: /StockItems/GetCommon
        public ActionResult GetCommon(int id)
        {
            try
            {
                DateTime todate = DateTime.Today;
                DateTime first_date = new DateTime(todate.Year, todate.Month, 1);
                DateTime first_date_1 = first_date.AddMonths(-1);
                DateTime end_date_1 = new DateTime(first_date_1.Year, first_date_1.Month, DateTime.DaysInMonth(first_date_1.Year, first_date_1.Month));

                CommonItemListModel model = new CommonItemListModel();
                var child = tb.MasterItems.Where(x => x.Id == id).FirstOrDefault();
                model.child_id = id;
                model.item_no = child.ItemNo;
                model.item_short_name = child.ItemShortName;
                model.model_no = child.ModelNo;
                model.UnitCode = child.MasterUnit.UnitCode;

                var stockbook_chi = tb.StockBooks.Where(x => x.MasterItemId == id && x.StockItemQty != 0 && !locationCode.Contains(x.MasterLocationId)).ToList();
                var stock_qty_chi = stockbook_chi.Select(s => s.StockItemQty).Sum();
                string remark_location_chi = "";
                if (stockbook_chi.Any())
                {
                    List<string> stockbook_grouped = stockbook_chi.GroupBy(g => g.MasterLocationId)
                        .Select(s => String.Concat((from ml in tb.MasterLocations where ml.Id == s.Key select ml.LocationCode).FirstOrDefault()
                                , "("
                                , Convert.ToDecimal(s.Sum(j => j.StockItemQty)).ToString("N")
                                , ")")
                                ).ToList();
                    remark_location_chi = String.Join(", ", stockbook_grouped);

                }
                model.LocationRemark = remark_location_chi;
                model.StockQty = stock_qty_chi;
                model.StockQty_str = stock_qty_chi.ToString("N");
                //Set Vendor
                var vendor_query = tb.ReceivingActualDetails.Where(x => x.MasterItemId == id).OrderByDescending(o => o.Id).Take(1)
                    .Select(s => new
                    {
                        vendorCode = s.ReceivingActualHeader.MasterVendor.VendorCode,
                        businessPartnerShortName = s.ReceivingActualHeader.MasterVendor.MasterBusinessPartner.BusinessPartnerShortName,
                    }).FirstOrDefault();
                if (vendor_query != null)
                {
                    model.VendorCode = vendor_query.vendorCode;
                    model.BusinessPartnerShortName = vendor_query.businessPartnerShortName;
                }
                else
                {
                    model.VendorCode = "";
                    model.BusinessPartnerShortName = "";
                }
                //Set Max, Min, Avg Qty
                var mfg_qurey = tb.MfgMaterialsIssueDetails.Where(x => x.MasterItemId == id && x.IssueDate >= first_date_1 && x.IssueDate <= end_date_1)
                    .GroupBy(g => g.IssueDate)
                    .Select(s => new { IssueDate = s.Key, ItemQty = s.Sum(j => j.ItemQty) }).ToList();

                List<decimal> mfg_list = mfg_qurey.Select(s => s.ItemQty).ToList();
                if (mfg_list.Any())
                {
                    decimal qty_max = mfg_list.Max();
                    decimal qty_min = mfg_list.Min();
                    decimal qty_avg = mfg_list.Average();

                    model.qty_max = qty_max;
                    model.qty_max_str = qty_max.ToString("N");
                    model.qty_min = qty_min;
                    model.qty_min_str = qty_min.ToString("N");
                    model.qty_avg = qty_avg;
                    model.qty_avg_str = qty_avg.ToString("N");
                }
                else
                {
                    model.qty_max_str = "";
                    model.qty_min_str = "";
                    model.qty_avg_str = "";
                }


                //Get
                List<CommonItemModel> common_list = new List<CommonItemModel>();
                common_list = CommonForwardModel(id);

                /*
                //Add Child
                CommonItemModel chi = new CommonItemModel();
                chi.id = id;
                chi.ItemNo = child.ItemNo;
                chi.ItemShortName = child.ItemShortName;
                chi.ItemName = child.ItemName;
                chi.ModelNo = child.ModelNo;
                chi.UnitCode = child.MasterUnit.UnitCode;

                
               
                List<CommonItemModel> common_order_list = new List<CommonItemModel>();
                common_order_list.Add(chi);
                common_order_list.AddRange(common_list);
                 */ 

                model.ItemList = common_list;

                return PartialView("GetCommon", model);
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", ex.Message);
                ViewBag.errorMessage = ex.ToString();
                return View("Error");
            }
        }

        //
        //GET: /StockItems/GetCommonBack
        public ActionResult GetCommonBack(int id)
        {
            try
            {
                DateTime todate = DateTime.Today;
                DateTime first_date = new DateTime(todate.Year, todate.Month, 1);
                DateTime first_date_1 = first_date.AddMonths(-1);
                DateTime end_date_1 = new DateTime(first_date_1.Year, first_date_1.Month, DateTime.DaysInMonth(first_date_1.Year, first_date_1.Month));

                CommonItemListModel model = new CommonItemListModel();
                var child = tb.MasterItems.Where(x => x.Id == id).FirstOrDefault();
                model.child_id = id;
                model.item_no = child.ItemNo;
                model.item_short_name = child.ItemShortName;
                model.model_no = child.ModelNo; 
                model.UnitCode = child.MasterUnit.UnitCode;

                var stockbook_chi = tb.StockBooks.Where(x => x.MasterItemId == id && x.StockItemQty != 0 && !locationCode.Contains(x.MasterLocationId)).ToList();
                var stock_qty_chi = stockbook_chi.Select(s => s.StockItemQty).Sum();
                string remark_location_chi = "";
                if (stockbook_chi.Any())
                {
                    List<string> stockbook_grouped = stockbook_chi.GroupBy(g => g.MasterLocationId)
                        .Select(s => String.Concat((from ml in tb.MasterLocations where ml.Id == s.Key select ml.LocationCode).FirstOrDefault()
                                , "("
                                , Convert.ToDecimal(s.Sum(j => j.StockItemQty)).ToString("N")
                                , ")")
                                ).ToList();
                    remark_location_chi = String.Join(", ", stockbook_grouped);

                }
                model.LocationRemark = remark_location_chi;
                model.StockQty = stock_qty_chi;
                model.StockQty_str = stock_qty_chi.ToString("N");

                //Set Vendor
                var vendor_query = tb.ReceivingActualDetails.Where(x => x.MasterItemId == id).OrderByDescending(o => o.Id).Take(1)
                    .Select(s => new
                    {
                        vendorCode = s.ReceivingActualHeader.MasterVendor.VendorCode,
                        businessPartnerShortName = s.ReceivingActualHeader.MasterVendor.MasterBusinessPartner.BusinessPartnerShortName,
                    }).FirstOrDefault();
                if (vendor_query != null)
                {
                    model.VendorCode = vendor_query.vendorCode;
                    model.BusinessPartnerShortName = vendor_query.businessPartnerShortName;
                }
                else
                {
                    model.VendorCode = "";
                    model.BusinessPartnerShortName = "";
                }
                //Set Max, Min, Avg Qty
                var mfg_qurey = tb.MfgMaterialsIssueDetails.Where(x => x.MasterItemId == id && x.IssueDate >= first_date_1 && x.IssueDate <= end_date_1)
                    .GroupBy(g => g.IssueDate)
                    .Select(s => new { IssueDate = s.Key, ItemQty = s.Sum(j => j.ItemQty) }).ToList();

                List<decimal> mfg_list = mfg_qurey.Select(s => s.ItemQty).ToList();
                if (mfg_list.Any())
                {
                    decimal qty_max = mfg_list.Max();
                    decimal qty_min = mfg_list.Min();
                    decimal qty_avg = mfg_list.Average();

                    model.qty_max = qty_max;
                    model.qty_max_str = qty_max.ToString("N");
                    model.qty_min = qty_min;
                    model.qty_min_str = qty_min.ToString("N");
                    model.qty_avg = qty_avg;
                    model.qty_avg_str = qty_avg.ToString("N");
                }
                else
                {
                    model.qty_max_str = "";
                    model.qty_min_str = "";
                    model.qty_avg_str = "";
                }

                //Get
                List<CommonItemModel> common_list = new List<CommonItemModel>();
                common_list = CommonBackwardModel(id);

                /*
                //Add Parent
                CommonItemModel chi = new CommonItemModel();
                chi.id = id;
                chi.ItemNo = child.ItemNo;
                chi.ItemShortName = child.ItemShortName;
                chi.ItemName = child.ItemName;
                

                List<CommonItemModel> common_order_list = new List<CommonItemModel>();
                common_order_list.Add(chi);
                common_order_list.AddRange(common_list);
                */
                model.ItemList = common_list;

                return PartialView("GetCommonBack", model);
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", ex.Message);
                ViewBag.errorMessage = ex.ToString();
                return View("Error");
            }
        }

        //
        //CommonForwardModel
        public List<CommonItemModel> CommonForwardModel(int id)
        {
            try
            {
                
                DateTime todate = DateTime.Today;
                DateTime first_date = new DateTime(todate.Year, todate.Month, 1);
                DateTime first_date_1 = first_date.AddMonths(-1);
                DateTime end_date_1 = new DateTime(first_date_1.Year, first_date_1.Month, DateTime.DaysInMonth(first_date_1.Year, first_date_1.Month));

                var query = tb.MasterStructure2.Where(x => x.MasterChildItemId == id && x.MasterParentItemId != id).ToList();
                List<CommonItemModel> common_list = query.Join(tb.MasterItems, c => c.MasterParentItemId, m => m.Id, (c, m) => new { c = c, m = m })
                    .Select(s => new CommonItemModel
                    {
                        id = s.c.MasterParentItemId,
                        ItemNo = s.m.ItemNo,
                        ItemShortName = s.m.ItemShortName,
                        ItemName = s.m.ItemName,
                        ModelNo = s.m.ModelNo,
                        UnitCode = s.m.MasterUnit.UnitCode,

                    }).OrderBy(o => o.ModelNo).ToList();

                foreach (var i in common_list)
                {
                    var stockbook = tb.StockBooks.Where(x => x.MasterItemId == i.id && x.StockItemQty != 0 && !locationCode.Contains(x.MasterLocationId)).ToList();
                    var stock_qty = stockbook.Select(s => s.StockItemQty).Sum();
                    string remark_location = "";
                    if (stockbook.Any())
                    {
                        List<string> stockbook_grouped = stockbook.GroupBy(g => g.MasterLocationId)
                            .Select(s => String.Concat((from ml in tb.MasterLocations where ml.Id == s.Key select ml.LocationCode).FirstOrDefault()
                                    , "("
                                    , Convert.ToDecimal(s.Sum(j => j.StockItemQty)).ToString("N")
                                    , ")")
                                    ).ToList();
                        remark_location = String.Join(", ", stockbook_grouped);

                    }
                    i.LocationRemark = remark_location;
                    i.StockQty = stock_qty;
                    i.StockQty_str = stock_qty.ToString("N");

                    //Set Vendor
                    var vendor_query = tb.ReceivingActualDetails.Where(x => x.MasterItemId == i.id).OrderByDescending(o => o.Id).Take(1)
                        .Select(s => new
                        {
                            vendorCode = s.ReceivingActualHeader.MasterVendor.VendorCode,
                            businessPartnerShortName = s.ReceivingActualHeader.MasterVendor.MasterBusinessPartner.BusinessPartnerShortName,
                        }).FirstOrDefault();
                    if (vendor_query != null)
                    {
                        i.VendorCode = vendor_query.vendorCode;
                        i.BusinessPartnerShortName = vendor_query.businessPartnerShortName;
                    }
                    else
                    {
                        i.VendorCode = "";
                        i.BusinessPartnerShortName = "";
                    }
                    //Set Max, Min, Avg Qty
                    var mfg_qurey = tb.MfgMaterialsIssueDetails.Where(x => x.MasterItemId == i.id && x.IssueDate >= first_date_1 && x.IssueDate <= end_date_1)
                        .GroupBy(g => g.IssueDate)
                        .Select(s => new { IssueDate = s.Key, ItemQty = s.Sum(j => j.ItemQty) }).ToList();

                    List<decimal> mfg_list = mfg_qurey.Select(s => s.ItemQty).ToList();
                    if (mfg_list.Any())
                    {
                        decimal qty_max = mfg_list.Max();
                        decimal qty_min = mfg_list.Min();
                        decimal qty_avg = mfg_list.Average();

                        i.qty_max = qty_max;
                        i.qty_max_str = qty_max.ToString("N");
                        i.qty_min = qty_min;
                        i.qty_min_str = qty_min.ToString("N");
                        i.qty_avg = qty_avg;
                        i.qty_avg_str = qty_avg.ToString("N");
                    }
                    else
                    {
                        i.qty_max_str = "";
                        i.qty_min_str = "";
                        i.qty_avg_str = "";
                    }
                }

                return common_list;
            }
            catch(Exception ex)
            {
                throw ex;
            }
        }

        //
        //CommonBackwardModel
        public List<CommonItemModel> CommonBackwardModel(int id)
        {
            try
            {
                DateTime todate = DateTime.Today;
                DateTime first_date = new DateTime(todate.Year, todate.Month, 1);
                DateTime first_date_1 = first_date.AddMonths(-1);
                DateTime end_date_1 = new DateTime(first_date_1.Year, first_date_1.Month, DateTime.DaysInMonth(first_date_1.Year, first_date_1.Month));

                var query = tb.MasterStructure2.Where(x => x.MasterParentItemId == id && x.MasterChildItemId != id).ToList();
                List<CommonItemModel> common_list = query.Join(tb.MasterItems, c => c.MasterChildItemId, m => m.Id, (c, m) => new { c = c, m = m })
                    .Select(s => new CommonItemModel
                    {
                        id = s.c.MasterChildItemId,
                        ItemNo = s.m.ItemNo,
                        ItemShortName = s.m.ItemShortName,
                        ItemName = s.m.ItemName,
                        ModelNo = s.m.ModelNo,
                        UnitCode = s.m.MasterUnit.UnitCode,

                    }).OrderBy(o => o.ModelNo).ToList();

                foreach (var i in common_list)
                {
                    var stockbook = tb.StockBooks.Where(x => x.MasterItemId == i.id && x.StockItemQty != 0 && !locationCode.Contains(x.MasterLocationId)).ToList();
                    var stock_qty = stockbook.Select(s => s.StockItemQty).Sum();
                    string remark_location = "";
                    if (stockbook.Any())
                    {
                        List<string> stockbook_grouped = stockbook.GroupBy(g => g.MasterLocationId)
                            .Select(s => String.Concat((from ml in tb.MasterLocations where ml.Id == s.Key select ml.LocationCode).FirstOrDefault()
                                    , "("
                                    , Convert.ToDecimal(s.Sum(j => j.StockItemQty)).ToString("N")
                                    , ")")
                                    ).ToList();
                        remark_location = String.Join(", ", stockbook_grouped);

                    }
                    i.LocationRemark = remark_location;
                    i.StockQty = stock_qty;
                    i.StockQty_str = stock_qty.ToString("N");

                    //Set Vendor
                    var vendor_query = tb.ReceivingActualDetails.Where(x => x.MasterItemId == i.id).OrderByDescending(o => o.Id).Take(1)
                        .Select(s => new
                        {
                            vendorCode = s.ReceivingActualHeader.MasterVendor.VendorCode,
                            businessPartnerShortName = s.ReceivingActualHeader.MasterVendor.MasterBusinessPartner.BusinessPartnerShortName,
                        }).FirstOrDefault();
                    if (vendor_query != null)
                    {
                        i.VendorCode = vendor_query.vendorCode;
                        i.BusinessPartnerShortName = vendor_query.businessPartnerShortName;
                    }
                    else
                    {
                        i.VendorCode = "";
                        i.BusinessPartnerShortName = "";
                    }
                    //Set Max, Min, Avg Qty
                    var mfg_qurey = tb.MfgMaterialsIssueDetails.Where(x => x.MasterItemId == i.id && x.IssueDate >= first_date_1 && x.IssueDate <= end_date_1)
                        .GroupBy(g => g.IssueDate)
                        .Select(s => new { IssueDate = s.Key, ItemQty = s.Sum(j => j.ItemQty) }).ToList();

                    List<decimal> mfg_list = mfg_qurey.Select(s => s.ItemQty).ToList();
                    if (mfg_list.Any())
                    {
                        decimal qty_max = mfg_list.Max();
                        decimal qty_min = mfg_list.Min();
                        decimal qty_avg = mfg_list.Average();

                        i.qty_max = qty_max;
                        i.qty_max_str = qty_max.ToString("N");
                        i.qty_min = qty_min;
                        i.qty_min_str = qty_min.ToString("N");
                        i.qty_avg = qty_avg;
                        i.qty_avg_str = qty_avg.ToString("N");
                    }
                    else
                    {
                        i.qty_max_str = "";
                        i.qty_min_str = "";
                        i.qty_avg_str = "";
                    }
                }

                return common_list; 
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        //
        //GET: /StockItems/GetCommonForwardJson
        public JsonResult GetCommonForwardJson(int id)
        {
            try
            {
                List<CommonItemModel> result = new List<CommonItemModel>();
                result = CommonForwardModel(id);

                return Json(result, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        //
        //GET: /StockItems/GetCommonBackwardJson
        public JsonResult GetCommonBackwardJson(int id)
        {
            try
            {
                List<CommonItemModel> result = new List<CommonItemModel>();
                result = CommonBackwardModel(id);

                return Json(result, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                throw ex;
            }

        }

        //Get Child Recursive
        public List<MasterStructureItemModel> GetChild(int id, List<MasterStructureItemModel> children)
        {
            DateTime todate = DateTime.Today;
            DateTime first_date = new DateTime(todate.Year, todate.Month, 1);
            DateTime first_date_1 = first_date.AddMonths(-1);
            DateTime end_date_1 = new DateTime(first_date_1.Year, first_date_1.Month, DateTime.DaysInMonth(first_date_1.Year, first_date_1.Month));

            List<int> id_child_list = tb.MasterStructure2.Where(x => x.MasterParentItemId == id && x.MasterParentItemId != x.MasterChildItemId)
                .Select(s => s.MasterChildItemId).ToList();
            if (id_child_list.Any())
            {
                foreach (var i in id_child_list)
                {
                    MasterStructureItemModel child = tb.MasterStructure2.Join(tb.MasterItems, f => f.MasterChildItemId, m => m.Id, (f, m) => new { f = f, m = m })
                        .Where(x => x.f.MasterParentItemId == id && x.f.MasterChildItemId == i && x.f.MasterParentItemId != x.f.MasterChildItemId)
                        .Select(s => new MasterStructureItemModel
                        {
                            Id = s.f.MasterChildItemId,
                            ItemNo = s.m.ItemNo,
                            ItemShortName = s.m.ItemShortName,
                            ModelNo = s.m.ModelNo,
                            ItemName = s.m.ItemName,
                            UnitCode = s.m.MasterUnit.UnitCode,
                            Ratio = s.f.NumeratorQty / s.f.DenominatorQty,

                        }).FirstOrDefault();

                    var stockbook = tb.StockBooks.Where(x => x.MasterItemId == child.Id && x.StockItemQty != 0 && !locationCode.Contains(x.MasterLocationId)).ToList();
                    var stock_qty = stockbook.Select(s => s.StockItemQty).Sum();
                    string remark_location = "";
                    if (stockbook.Any())
                    {
                        List<string> stockbook_grouped = stockbook.GroupBy(g => g.MasterLocationId)
                            .Select(s => String.Concat((from ml in tb.MasterLocations where ml.Id == s.Key select ml.LocationCode).FirstOrDefault()
                                    , "("
                                    , Convert.ToDecimal(s.Sum(j => j.StockItemQty)).ToString("N")
                                    , ")")
                                    ).ToList();
                        remark_location = String.Join(", ", stockbook_grouped);

                    }
                    child.LocationRemark = remark_location;
                    child.StockQty = stock_qty;
                    child.StockQty_str = stock_qty.ToString("N");
                    child.Ratio_str = child.Ratio.ToString("N");
                    child.StockQty_Ratio = stock_qty / child.Ratio;
                    child.StockQty_Ratio_str = (stock_qty / child.Ratio).ToString("N");
                    //Set Order item
                    int status_value;
                    if(itemOrder.TryGetValue(child.ItemShortName, out status_value))
                    {
                        child.status = itemOrder[child.ItemShortName];
                    }
                    else
                    {
                        child.status = 0;
                    }
                    //Set Vendor
                    var vendor_query = tb.ReceivingActualDetails.Where(x => x.MasterItemId == child.Id).OrderByDescending(o => o.Id).Take(1)
                           .Select(s => new
                           {
                               vendorCode = s.ReceivingActualHeader.MasterVendor.VendorCode,
                               businessPartnerShortName = s.ReceivingActualHeader.MasterVendor.MasterBusinessPartner.BusinessPartnerShortName,
                           }).FirstOrDefault();
                    if (vendor_query != null)
                    {
                        child.VendorCode = vendor_query.vendorCode;
                        child.BusinessPartnerShortName = vendor_query.businessPartnerShortName;
                    }
                    else
                    {
                        child.VendorCode = "";
                        child.BusinessPartnerShortName = "";
                    }
                    //Set Max, Min, Avg Qty                   
                    var mfg_qurey = tb.MfgMaterialsIssueDetails.Where(x => x.MasterItemId == child.Id && x.IssueDate >= first_date_1 && x.IssueDate <= end_date_1)
                        .GroupBy(g => g.IssueDate)
                        .Select(s => new { IssueDate = s.Key, ItemQty = s.Sum(j => j.ItemQty) }).ToList();

                    List<decimal> mfg_list = mfg_qurey.Select(s => s.ItemQty).ToList();
                    if (mfg_list.Any())
                    {
                        decimal qty_max = mfg_list.Max();
                        decimal qty_min = mfg_list.Min();
                        decimal qty_avg = mfg_list.Average();

                        child.qty_max = qty_max;
                        child.qty_max_str = qty_max.ToString("N");
                        child.qty_min = qty_min;
                        child.qty_min_str = qty_min.ToString("N");
                        child.qty_avg = qty_avg;
                        child.qty_avg_str = qty_avg.ToString("N");
                    }
                    else
                    {
                        child.qty_max_str = "";
                        child.qty_min_str = "";
                        child.qty_avg_str = "";
                    }
                   

                    children.Add(child);
                    children = GetChild(i, children);
                }
            }
            return children;
        }

        //
        //GET: /StockItems/ItemTreeDetail
        public ActionResult ItemTreeDetail(int id)
        {
            try
            {
                DateTime todate = DateTime.Today;
                DateTime first_date = new DateTime(todate.Year, todate.Month, 1);
                DateTime first_date_1 = first_date.AddMonths(-1);
                DateTime end_date_1 = new DateTime(first_date_1.Year, first_date_1.Month, DateTime.DaysInMonth(first_date_1.Year, first_date_1.Month));

                MasterStructureItemListModel model = new MasterStructureItemListModel();
                model.id_parent = id;
                List<ItemStructureJsTreeModel> tree_node = new List<ItemStructureJsTreeModel>();

                var parent = tb.MasterItems.Where(x => x.Id == id).FirstOrDefault();

                //Stock and Location
                var stockbook = tb.StockBooks.Where(x => x.MasterItemId == id && x.StockItemQty != 0 && !locationCode.Contains(x.MasterLocationId)).ToList();
                var stock_qty = stockbook.Select(s => s.StockItemQty).Sum();
                string remark_location = "";
                if (stockbook.Any())
                {
                    List<string> stockbook_grouped = stockbook.GroupBy(g => g.MasterLocationId)
                        .Select(s => String.Concat((from ml in tb.MasterLocations where ml.Id == s.Key select ml.LocationCode).FirstOrDefault()
                                , "("
                                , Convert.ToDecimal(s.Sum(j => j.StockItemQty)).ToString("N")
                                , ")")
                                ).ToList();
                    remark_location = String.Join(", ", stockbook_grouped);

                }
            
                //Parent Node
                ItemStructureJsTreeModel first_node = new ItemStructureJsTreeModel();
                first_node.id = id.ToString();
                first_node.parent = "#";
                first_node.text = String.Concat(parent.ItemNo," ",parent.ModelNo);
                first_node.state = new ItemStateModel() {selected= true,opened=true };
                first_node.type = "root";
                MasterStructureItemModel data = new MasterStructureItemModel();
                data.Id = parent.Id;
                data.ItemNo = parent.ItemNo;
                data.ModelNo = parent.ModelNo;
                data.ItemShortName = parent.ItemShortName;
                data.ItemName = parent.ItemName;
                data.UnitCode = parent.MasterUnit.UnitCode;
                data.StockQty = stock_qty;
                data.StockQty_str = stock_qty.ToString("N");
                data.LocationRemark = remark_location;

                //Set Vendor
                var vendor_query = tb.ReceivingActualDetails.Where(x => x.MasterItemId == id).OrderByDescending(o => o.Id).Take(1)
                       .Select(s => new
                       {
                           vendorCode = s.ReceivingActualHeader.MasterVendor.VendorCode,
                           businessPartnerShortName = s.ReceivingActualHeader.MasterVendor.MasterBusinessPartner.BusinessPartnerShortName,
                       }).FirstOrDefault();
                if (vendor_query != null)
                {
                    data.VendorCode = vendor_query.vendorCode;
                    data.BusinessPartnerShortName = vendor_query.businessPartnerShortName;
                }
                else
                {
                    data.VendorCode = "";
                    data.BusinessPartnerShortName = "";
                }
                //Set Max, Min, Avg Qty                   
                var mfg_qurey = tb.MfgMaterialsIssueDetails.Where(x => x.MasterItemId == id && x.IssueDate >= first_date_1 && x.IssueDate <= end_date_1)
                    .GroupBy(g => g.IssueDate)
                    .Select(s => new { IssueDate = s.Key, ItemQty = s.Sum(j => j.ItemQty) }).ToList();

                List<decimal> mfg_list = mfg_qurey.Select(s => s.ItemQty).ToList();
                if (mfg_list.Any())
                {
                    decimal qty_max = mfg_list.Max();
                    decimal qty_min = mfg_list.Min();
                    decimal qty_avg = mfg_list.Average();

                    data.qty_max = qty_max;
                    data.qty_max_str = qty_max.ToString("N");
                    data.qty_min = qty_min;
                    data.qty_min_str = qty_min.ToString("N");
                    data.qty_avg = qty_avg;
                    data.qty_avg_str = qty_avg.ToString("N");
                }
                else
                {
                    data.qty_max_str = "";
                    data.qty_min_str = "";
                    data.qty_avg_str = "";
                }

                first_node.data = data;

                tree_node.Add(first_node);

                model.ItemStructureJsTreeModelList = GetTree(id, tree_node);

                return View(model);
            }
            catch(Exception ex)
            {
                ViewBag.errorMessage = ex.ToString();
                return View("Error");
            }
        }

        //Get Json Child Recursive
        public List<ItemStructureJsTreeModel> GetTree(int id, List<ItemStructureJsTreeModel> tree_node)
        {
            DateTime todate = DateTime.Today;
            DateTime first_date = new DateTime(todate.Year, todate.Month, 1);
            DateTime first_date_1 = first_date.AddMonths(-1);
            DateTime end_date_1 = new DateTime(first_date_1.Year, first_date_1.Month, DateTime.DaysInMonth(first_date_1.Year, first_date_1.Month));
            bool check_selected = false;

            List<int> id_child_list = tb.MasterStructure2.Where(x => x.MasterParentItemId == id && x.MasterParentItemId != x.MasterChildItemId)
                .Select(s => s.MasterChildItemId).ToList();
            if (id_child_list.Any())
            {
                foreach (var i in id_child_list)
                {
                    var child_query = tb.MasterStructure2.Join(tb.MasterItems, f => f.MasterChildItemId, m => m.Id, (f, m) => new { f = f, m = m })
                        .Where(x => x.f.MasterParentItemId == id && x.f.MasterChildItemId == i && x.f.MasterParentItemId != x.f.MasterChildItemId).FirstOrDefault();
                    
                    //Stock and Location
                    var stockbook = tb.StockBooks.Where(x => x.MasterItemId == i && x.StockItemQty != 0 && !locationCode.Contains(x.MasterLocationId)).ToList();
                    var stock_qty = stockbook.Select(s => s.StockItemQty).Sum();
                    string remark_location = "";
                    if (stockbook.Any())
                    {
                        List<string> stockbook_grouped = stockbook.GroupBy(g => g.MasterLocationId)
                            .Select(s => String.Concat((from ml in tb.MasterLocations where ml.Id == s.Key select ml.LocationCode).FirstOrDefault()
                                    , "("
                                    , Convert.ToDecimal(s.Sum(j => j.StockItemQty)).ToString("N")
                                    , ")")
                                    ).ToList();
                        remark_location = String.Join(", ", stockbook_grouped);

                    }
                    //Set Selected ItemShortName
                    check_selected = selectedItemShortName.Contains(child_query.m.ItemShortName);

                    //Set child
                    ItemStructureJsTreeModel child = new ItemStructureJsTreeModel();                   
                    child.id = child_query.f.MasterChildItemId.ToString();
                    child.text = String.Concat(child_query.m.ItemNo," ",child_query.m.ModelNo);
                    child.state = new ItemStateModel() { selected = check_selected, };
                    child.parent = id.ToString();
                    if(check_selected){
                        child.type = child_query.m.ItemShortName;
                    }
                    else
                    {
                        child.type = "default";
                    }

                    MasterStructureItemModel data = new MasterStructureItemModel();
                    data.Id = child_query.f.MasterChildItemId;
                    data.ItemNo = child_query.m.ItemNo;
                    data.ModelNo = child_query.m.ModelNo;
                    data.ItemShortName = child_query.m.ItemShortName;
                    data.ItemName = child_query.m.ItemName;
                    data.UnitCode = child_query.m.MasterUnit.UnitCode;
                    data.StockQty = stock_qty;
                    data.StockQty_str = stock_qty.ToString("N");
                    data.LocationRemark = remark_location;
                    //Set Vendor
                    var vendor_query = tb.ReceivingActualDetails.Where(x => x.MasterItemId == i).OrderByDescending(o => o.Id).Take(1)
                           .Select(s => new
                           {
                               vendorCode = s.ReceivingActualHeader.MasterVendor.VendorCode,
                               businessPartnerShortName = s.ReceivingActualHeader.MasterVendor.MasterBusinessPartner.BusinessPartnerShortName,
                           }).FirstOrDefault();
                    if (vendor_query != null)
                    {
                        data.VendorCode = vendor_query.vendorCode;
                        data.BusinessPartnerShortName = vendor_query.businessPartnerShortName;
                    }
                    else
                    {
                        data.VendorCode = "";
                        data.BusinessPartnerShortName = "";
                    }
                    //Set Max, Min, Avg Qty                   
                    var mfg_qurey = tb.MfgMaterialsIssueDetails.Where(x => x.MasterItemId == i && x.IssueDate >= first_date_1 && x.IssueDate <= end_date_1)
                        .GroupBy(g => g.IssueDate)
                        .Select(s => new { IssueDate = s.Key, ItemQty = s.Sum(j => j.ItemQty) }).ToList();

                    List<decimal> mfg_list = mfg_qurey.Select(s => s.ItemQty).ToList();
                    if (mfg_list.Any())
                    {
                        decimal qty_max = mfg_list.Max();
                        decimal qty_min = mfg_list.Min();
                        decimal qty_avg = mfg_list.Average();

                        data.qty_max = qty_max;
                        data.qty_max_str = qty_max.ToString("N");
                        data.qty_min = qty_min;
                        data.qty_min_str = qty_min.ToString("N");
                        data.qty_avg = qty_avg;
                        data.qty_avg_str = qty_avg.ToString("N");
                    }
                    else
                    {
                        data.qty_max_str = "";
                        data.qty_min_str = "";
                        data.qty_avg_str = "";
                    }
                   
                    child.data = data;
                  
                    /*
                    ItemStructureJsTreeModel child = tb.MasterStructure2.Join(tb.MasterItems, f => f.MasterChildItemId, m => m.Id, (f, m) => new { f = f, m = m })
                        .Where(x => x.f.MasterParentItemId == id && x.f.MasterChildItemId == i && x.f.MasterParentItemId != x.f.MasterChildItemId)
                        .Select(s => new ItemStructureJsTreeModel
                        {
                            id = s.f.MasterChildItemId.ToString(),
                            parent = id.ToString(),
                            text = String.Concat(s.m.ItemShortName, "_", s.m.ModelNo, "_____(",
                                    tb.StockBooks.Where(x => x.MasterItemId == s.f.MasterChildItemId
                                        && x.StockItemQty != 0 && !locationCode.Contains(x.MasterLocationId))
                                        .Select(c => c.StockItemQty)
                                        .Sum().ToString("N"), ")"
                                  ),
                            state = new ItemStateModel(),
                            // children = (from df in db.DW_Formulas where df.FOR_Parent == s.f.FOR_Child select df.FOR_Child).Any() ? true : false,

                        }).FirstOrDefault();
                    */
                    tree_node.Add(child);
                    tree_node = GetTree(i, tree_node);
                }
            }
            return tree_node;
        }
       

        //
        //GET: /StockItems/ExportCommon
        public void ExportCommon(int id)
        {
            try
            {
                var query = tb.MasterStructure2.Where(x => x.MasterChildItemId == id && x.MasterParentItemId != id).ToList();
                List<CommonItemModel> common_list = query.Join(tb.MasterItems, c => c.MasterParentItemId, m => m.Id, (c, m) => new { c = c, m = m })
                    .Select(s => new CommonItemModel
                    {
                        id = s.c.MasterParentItemId,
                        ItemNo = s.m.ItemNo,
                        ItemShortName = s.m.ItemShortName,
                        ItemName = s.m.ItemName,
                        ModelNo = s.m.ModelNo,
                        UnitCode = s.m.MasterUnit.UnitCode,

                    }).OrderBy(o=>o.ModelNo).ToList();

                foreach (var i in common_list)
                {
                    var stockbook = tb.StockBooks.Where(x => x.MasterItemId == i.id && x.StockItemQty != 0 && !locationCode.Contains(x.MasterLocationId)).ToList();
                    var stock_qty = stockbook.Select(s => s.StockItemQty).Sum();
                    string remark_location = "";
                    if (stockbook.Any())
                    {
                        List<string> stockbook_grouped = stockbook.GroupBy(g => g.MasterLocationId)
                            .Select(s => String.Concat((from ml in tb.MasterLocations where ml.Id == s.Key select ml.LocationCode).FirstOrDefault()
                                    , "("
                                    , Convert.ToDecimal(s.Sum(j => j.StockItemQty)).ToString("N")
                                    , ")")
                                    ).ToList();
                        remark_location = String.Join(", ", stockbook_grouped);

                    }
                    i.LocationRemark = remark_location;
                    i.StockQty = stock_qty;
                    i.StockQty_str = stock_qty.ToString("N");
                }

                //Add Child
                var child = tb.MasterItems.Where(x => x.Id == id).FirstOrDefault();
                CommonItemModel chi = new CommonItemModel();
                chi.id = id;
                chi.ItemNo = child.ItemNo;
                chi.ItemShortName = child.ItemShortName;
                chi.ItemName = child.ItemName;
                chi.ModelNo = child.ModelNo;
                chi.UnitCode = child.MasterUnit.UnitCode;

                var stockbook_chi = tb.StockBooks.Where(x => x.MasterItemId == id && x.StockItemQty != 0 && !locationCode.Contains(x.MasterLocationId)).ToList();
                var stock_qty_chi = stockbook_chi.Select(s => s.StockItemQty).Sum();
                string remark_location_chi = "";
                if (stockbook_chi.Any())
                {
                    List<string> stockbook_grouped = stockbook_chi.GroupBy(g => g.MasterLocationId)
                        .Select(s => String.Concat((from ml in tb.MasterLocations where ml.Id == s.Key select ml.LocationCode).FirstOrDefault()
                                , "("
                                , Convert.ToDecimal(s.Sum(j => j.StockItemQty)).ToString("N")
                                , ")")
                                ).ToList();
                    remark_location_chi = String.Join(", ", stockbook_grouped);

                }
                chi.LocationRemark = remark_location_chi;
                chi.StockQty = stock_qty_chi;
                chi.StockQty_str = stock_qty_chi.ToString("N");

                List<CommonItemModel> common_order_list = new List<CommonItemModel>();
                common_order_list.Add(chi);
                common_order_list.AddRange(common_list);

                ExportToExcelCommon(common_order_list);
                
            }
            catch (Exception ex)
            {
                ViewBag.errorMessage = ex.ToString();
                throw ex;
            }
        }

        //
        //GET: /StockItems/ExportCommonBack
        public void ExportCommonBack(int id)
        {
            try
            {
                var query = tb.MasterStructure2.Where(x => x.MasterParentItemId == id && x.MasterChildItemId != id).ToList();
                List<CommonItemModel> common_list = query.Join(tb.MasterItems, c => c.MasterChildItemId, m => m.Id, (c, m) => new { c = c, m = m })
                    .Select(s => new CommonItemModel
                    {
                        id = s.c.MasterChildItemId,
                        ItemNo = s.m.ItemNo,
                        ItemShortName = s.m.ItemShortName,
                        ItemName = s.m.ItemName,
                        ModelNo = s.m.ModelNo,
                        UnitCode = s.m.MasterUnit.UnitCode,

                    }).OrderBy(o => o.ModelNo).ToList();

                foreach (var i in common_list)
                {
                    var stockbook = tb.StockBooks.Where(x => x.MasterItemId == i.id && x.StockItemQty != 0 && !locationCode.Contains(x.MasterLocationId)).ToList();
                    var stock_qty = stockbook.Select(s => s.StockItemQty).Sum();
                    string remark_location = "";
                    if (stockbook.Any())
                    {
                        List<string> stockbook_grouped = stockbook.GroupBy(g => g.MasterLocationId)
                            .Select(s => String.Concat((from ml in tb.MasterLocations where ml.Id == s.Key select ml.LocationCode).FirstOrDefault()
                                    , "("
                                    , Convert.ToDecimal(s.Sum(j => j.StockItemQty)).ToString("N")
                                    , ")")
                                    ).ToList();
                        remark_location = String.Join(", ", stockbook_grouped);

                    }
                    i.LocationRemark = remark_location;
                    i.StockQty = stock_qty;
                    i.StockQty_str = stock_qty.ToString("N");
                }

                //Add Parent
                var child = tb.MasterItems.Where(x => x.Id == id).FirstOrDefault();
                CommonItemModel chi = new CommonItemModel();
                chi.id = id;
                chi.ItemNo = child.ItemNo;
                chi.ItemShortName = child.ItemShortName;
                chi.ItemName = child.ItemName;
                chi.ModelNo = child.ModelNo;
                chi.UnitCode = child.MasterUnit.UnitCode;

                var stockbook_chi = tb.StockBooks.Where(x => x.MasterItemId == id && x.StockItemQty != 0 && !locationCode.Contains(x.MasterLocationId)).ToList();
                var stock_qty_chi = stockbook_chi.Select(s => s.StockItemQty).Sum();
                string remark_location_chi = "";
                if (stockbook_chi.Any())
                {
                    List<string> stockbook_grouped = stockbook_chi.GroupBy(g => g.MasterLocationId)
                        .Select(s => String.Concat((from ml in tb.MasterLocations where ml.Id == s.Key select ml.LocationCode).FirstOrDefault()
                                , "("
                                , Convert.ToDecimal(s.Sum(j => j.StockItemQty)).ToString("N")
                                , ")")
                                ).ToList();
                    remark_location_chi = String.Join(", ", stockbook_grouped);

                }
                chi.LocationRemark = remark_location_chi;
                chi.StockQty = stock_qty_chi;
                chi.StockQty_str = stock_qty_chi.ToString("N");

                List<CommonItemModel> common_order_list = new List<CommonItemModel>();
                common_order_list.Add(chi);
                common_order_list.AddRange(common_list);

                ExportToExcelCommon(common_order_list);

            }
            catch (Exception ex)
            {
                ViewBag.errorMessage = ex.ToString();
                throw ex;
            }
        }

        //
        //Export Excel
        public void ExportToExcel(List<MasterStructureItemModel> model)
        {
            try
            {
                List<int> g1 = new List<int> { 1, 2, 3, 4 };
                List<int> g2 = new List<int> { 5, 6, 7 };
                List<int> g3 = new List<int> { 8, 9, 10 };
                List<int> g4 = new List<int> { 11, 12, 15 };
                List<int> g5 = new List<int> { 13, 14 };

                Color color_column_name = System.Drawing.ColorTranslator.FromHtml("#C0C0C0");
                Color color_parent = System.Drawing.ColorTranslator.FromHtml("#40E0D0");
                Color color_g1 = System.Drawing.ColorTranslator.FromHtml("#FAEDCD");
                Color color_g2 = System.Drawing.ColorTranslator.FromHtml("#B9F3E4");
                Color color_g3 = System.Drawing.ColorTranslator.FromHtml("#C9F4AA");
                Color color_g4 = System.Drawing.ColorTranslator.FromHtml("#F1F7B5");
                Color color_g5 = System.Drawing.ColorTranslator.FromHtml("#FFD1D1");
                Color color_bottom_line = System.Drawing.ColorTranslator.FromHtml("#ECE8DD");

                ExcelPackage excel = new ExcelPackage();
                var workSheet = excel.Workbook.Worksheets.Add("Sheet1");

                //Excution Time
                workSheet.Cells["A1:F1"].Merge = true;
                workSheet.Cells[1, 1].Value = String.Format("Execute Time: {0}", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));

                //Column name
                workSheet.Cells[2, 1].Value = "No.";
                workSheet.Cells[2, 2].Value = "Item No";
                workSheet.Cells[2, 3].Value = "Model No";
                workSheet.Cells[2, 4].Value = "Item Short Name";
                workSheet.Cells[2, 5].Value = "Vendor Code";
                workSheet.Cells[2, 6].Value = "Vendor Short Name";
                workSheet.Cells[2, 7].Value = "Unit Code";
                workSheet.Cells[2, 8].Value = "Max";
                workSheet.Cells[2, 9].Value = "Min";
                workSheet.Cells[2, 10].Value = "Avg";
                workSheet.Cells[2, 11].Value = "Stock Qty";
                workSheet.Cells[2, 12].Value = "Ratio";
                workSheet.Cells[2, 13].Value = "StockQty / Ratio";
                workSheet.Cells[2, 14].Value = "Remark";

                workSheet.Cells["A2:N2"].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
               
                workSheet.Cells["A2:N2"].Style.Fill.PatternType = ExcelFillStyle.Solid;
                workSheet.Cells["A2:N2"].Style.Fill.BackgroundColor.SetColor(color_column_name);
                workSheet.Cells["A2:N2"].Style.Font.SetFromFont(new Font("Segoe UI", 10));
                workSheet.Cells["A2:N2"].Style.Font.Bold = true;
                workSheet.Cells["A2:N2"].Style.Font.Color.SetColor(Color.Black);
                workSheet.Cells["A2:N2"].Style.Border.Right.Style = ExcelBorderStyle.Thin;
                workSheet.Cells["A2:N2"].Style.Border.Bottom.Style = ExcelBorderStyle.Thin;
                int row = 0;
                foreach (var dd in model.Select((val, i) => new { val = val, i = i }))
                {
                    row = dd.i + 3;
                    //workSheet.Row(row).Height = 80;
                    workSheet.Row(row).Style.VerticalAlignment = OfficeOpenXml.Style.ExcelVerticalAlignment.Center;

                    string row_str = String.Format("A{0}:N{0}", row.ToString());
                    workSheet.Cells[row_str].Style.Border.Right.Style = ExcelBorderStyle.Thin;
                    workSheet.Cells[row_str].Style.Border.Bottom.Style = ExcelBorderStyle.Thin;
                    workSheet.Cells[row_str].Style.Border.Bottom.Color.SetColor(color_bottom_line);
                    workSheet.Cells[row_str].Style.Font.SetFromFont(new Font("Segoe UI", 10));

                    workSheet.Cells[row, 1].Value = dd.i ;
                    workSheet.Cells[row, 2].Value = dd.val.ItemNo;
                    workSheet.Cells[row, 3].Value = dd.val.ModelNo;
                    workSheet.Cells[row, 4].Value = dd.val.ItemShortName;
                    workSheet.Cells[row, 5].Value = dd.val.VendorCode;
                    workSheet.Cells[row, 6].Value = dd.val.BusinessPartnerShortName;
                    workSheet.Cells[row, 7].Value = dd.val.UnitCode;
                    workSheet.Cells[row, 8].Value = dd.val.qty_max;
                    workSheet.Cells[row, 9].Value = dd.val.qty_min;
                    workSheet.Cells[row, 10].Value = dd.val.qty_avg;
                    workSheet.Cells[row, 11].Value = dd.val.StockQty;
                    workSheet.Cells[row, 12].Value = dd.val.Ratio;
                    workSheet.Cells[row, 13].Value = dd.val.StockQty_Ratio;
                    workSheet.Cells[row, 14].Value = dd.val.LocationRemark;

                    workSheet.Cells[row, 1].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                    workSheet.Cells[row, 2].Style.HorizontalAlignment = ExcelHorizontalAlignment.Left;
                    workSheet.Cells[row, 3].Style.HorizontalAlignment = ExcelHorizontalAlignment.Left;
                    workSheet.Cells[row, 4].Style.HorizontalAlignment = ExcelHorizontalAlignment.Left;
                    workSheet.Cells[row, 5].Style.HorizontalAlignment = ExcelHorizontalAlignment.Left;
                    workSheet.Cells[row, 6].Style.HorizontalAlignment = ExcelHorizontalAlignment.Left;
                    workSheet.Cells[row, 7].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                    workSheet.Cells[row, 8].Style.HorizontalAlignment = ExcelHorizontalAlignment.Right;
                    workSheet.Cells[row, 9].Style.HorizontalAlignment = ExcelHorizontalAlignment.Right;
                    workSheet.Cells[row, 10].Style.HorizontalAlignment = ExcelHorizontalAlignment.Right;
                    
                    workSheet.Cells[row, 11].Style.HorizontalAlignment = ExcelHorizontalAlignment.Right;
                    if (dd.val.qty_avg >= dd.val.StockQty)
                    {
                        workSheet.Cells[row, 11].Style.Font.Bold = true;
                        //workSheet.Cells[row, 11].Style.Font.Color.SetColor(Color.Red);
                        workSheet.Cells[row_str].Style.Font.Color.SetColor(Color.Red);
                    }
                    else
                    {
                        workSheet.Cells[row, 11].Style.Font.Bold = true;
                        //workSheet.Cells[row, 11].Style.Font.Color.SetColor(Color.Black);
                        workSheet.Cells[row_str].Style.Font.Color.SetColor(Color.Black);
                    }

                    workSheet.Cells[row, 12].Style.HorizontalAlignment = ExcelHorizontalAlignment.Right;
                    workSheet.Cells[row, 13].Style.HorizontalAlignment = ExcelHorizontalAlignment.Right;
                    workSheet.Cells[row, 14].Style.HorizontalAlignment = ExcelHorizontalAlignment.Left;
                  
                   

                   
                    if (dd.val.status == 20 )
                    {
                        workSheet.Cells[row_str].Style.Fill.PatternType = ExcelFillStyle.Solid;
                        workSheet.Cells[row_str].Style.Font.SetFromFont(new Font("Segoe UI", 10));
                        workSheet.Cells[row_str].Style.Font.Bold = true;
                        workSheet.Cells[row_str].Style.Fill.BackgroundColor.SetColor(color_parent);
                    }
                    else if(g1.Contains(dd.val.status))
                    {
                        workSheet.Cells[row_str].Style.Fill.PatternType = ExcelFillStyle.Solid;
                       // workSheet.Cells[row_str].Style.Font.SetFromFont(new Font("Segoe UI", 10));
                        //workSheet.Cells[row_str].Style.Font.Bold = false;
                        workSheet.Cells[row_str].Style.Fill.BackgroundColor.SetColor(color_g1);
                    }
                    else if (g2.Contains(dd.val.status))
                    {
                        workSheet.Cells[row_str].Style.Fill.PatternType = ExcelFillStyle.Solid;
                       // workSheet.Cells[row_str].Style.Font.SetFromFont(new Font("Segoe UI", 10));
                        //workSheet.Cells[row_str].Style.Font.Bold = false;
                        workSheet.Cells[row_str].Style.Fill.BackgroundColor.SetColor(color_g2);
                    }
                    else if (g3.Contains(dd.val.status))
                    {
                        workSheet.Cells[row_str].Style.Fill.PatternType = ExcelFillStyle.Solid;
                        //workSheet.Cells[row_str].Style.Font.SetFromFont(new Font("Segoe UI", 10));
                        //workSheet.Cells[row_str].Style.Font.Bold = false;
                        workSheet.Cells[row_str].Style.Fill.BackgroundColor.SetColor(color_g3);
                    }
                    else if (g4.Contains(dd.val.status))
                    {
                        workSheet.Cells[row_str].Style.Fill.PatternType = ExcelFillStyle.Solid;
                        //workSheet.Cells[row_str].Style.Font.SetFromFont(new Font("Segoe UI", 10));
                        //workSheet.Cells[row_str].Style.Font.Bold = false;
                        workSheet.Cells[row_str].Style.Fill.BackgroundColor.SetColor(color_g4);
                    }
                    else if (g5.Contains(dd.val.status))
                    {
                        workSheet.Cells[row_str].Style.Fill.PatternType = ExcelFillStyle.Solid;
                        //workSheet.Cells[row_str].Style.Font.SetFromFont(new Font("Segoe UI", 10));
                        //workSheet.Cells[row_str].Style.Font.Bold = false;
                        workSheet.Cells[row_str].Style.Fill.BackgroundColor.SetColor(color_g5);
                    }
                    else 
                    {
                        //workSheet.Cells[row_str].Style.Font.SetFromFont(new Font("Segoe UI", 10));
                        //workSheet.Cells[row_str].Style.Font.Bold = false;
                       
                    }

                }
                workSheet.Cells[String.Format("A{0}:N{0}",row)].Style.Border.Bottom.Style = ExcelBorderStyle.Thin;
                workSheet.Cells[String.Format("A{0}:N{0}", row)].Style.Border.Bottom.Color.SetColor(Color.Black);

                workSheet.Protection.IsProtected = false;
                workSheet.Protection.AllowSelectLockedCells = false;
                workSheet.Cells[workSheet.Dimension.Address].AutoFitColumns();
                //workSheet.Column(1).Width = 15;
                using (var memoryStream = new MemoryStream())
                {
                    Response.ContentType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";
                    Response.AddHeader("content-disposition", "attachment;  filename=ItemStock.xlsx");
                    excel.SaveAs(memoryStream);
                    memoryStream.WriteTo(Response.OutputStream);
                    Response.Flush();
                    Response.End();
                }
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", ex.Message);
                throw;
            }
        }

        //
        //Export Excel Common
        public void ExportToExcelCommon(List<CommonItemModel> model)
        {
            try
            {
                List<int> g1 = new List<int> { 1, 2, 3, 4 };
                List<int> g2 = new List<int> { 5, 6, 7 };
                List<int> g3 = new List<int> { 8, 9, 10 };
                List<int> g4 = new List<int> { 11, 12, 15 };
                List<int> g5 = new List<int> { 13, 14 };

                Color color_column_name = System.Drawing.ColorTranslator.FromHtml("#C0C0C0");
                Color color_parent = System.Drawing.ColorTranslator.FromHtml("#40E0D0");
                Color color_g1 = System.Drawing.ColorTranslator.FromHtml("#FAEDCD");
                Color color_g2 = System.Drawing.ColorTranslator.FromHtml("#B9F3E4");
                Color color_g3 = System.Drawing.ColorTranslator.FromHtml("#C9F4AA");
                Color color_g4 = System.Drawing.ColorTranslator.FromHtml("#F1F7B5");
                Color color_g5 = System.Drawing.ColorTranslator.FromHtml("#FFD1D1");
                Color color_bottom_line = System.Drawing.ColorTranslator.FromHtml("#ECE8DD");

                ExcelPackage excel = new ExcelPackage();
                var workSheet = excel.Workbook.Worksheets.Add("Common");

                //Excution Time
                workSheet.Cells["A1:F1"].Merge = true;
                workSheet.Cells[1, 1].Value = String.Format("Execute Time: {0}", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));

                //Column name
                workSheet.Cells[2, 1].Value = "No.";
                workSheet.Cells[2, 2].Value = "Item No";
                workSheet.Cells[2, 3].Value = "Model No";
                workSheet.Cells[2, 4].Value = "Item Short Name";
                workSheet.Cells[2, 5].Value = "Unit Code";
                workSheet.Cells[2, 6].Value = "Stock Qty";
                workSheet.Cells[2, 7].Value = "Remark";

                workSheet.Cells["A2:G2"].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;

                workSheet.Cells["A2:G2"].Style.Fill.PatternType = ExcelFillStyle.Solid;
                workSheet.Cells["A2:G2"].Style.Fill.BackgroundColor.SetColor(color_column_name);
                workSheet.Cells["A2:G2"].Style.Font.SetFromFont(new Font("Segoe UI", 10));
                workSheet.Cells["A2:G2"].Style.Font.Bold = true;
                workSheet.Cells["A2:G2"].Style.Font.Color.SetColor(Color.Black);
                workSheet.Cells["A2:G2"].Style.Border.Right.Style = ExcelBorderStyle.Thin;
                workSheet.Cells["A2:G2"].Style.Border.Bottom.Style = ExcelBorderStyle.Thin;
                int row = 0;
                foreach (var dd in model.Select((val, i) => new { val = val, i = i }))
                {
                    row = dd.i + 3;
                    //workSheet.Row(row).Height = 80;
                    workSheet.Row(row).Style.VerticalAlignment = OfficeOpenXml.Style.ExcelVerticalAlignment.Center;

                    workSheet.Cells[row, 1].Value = dd.i;
                    workSheet.Cells[row, 2].Value = dd.val.ItemNo;
                    workSheet.Cells[row, 3].Value = dd.val.ModelNo;
                    workSheet.Cells[row, 4].Value = dd.val.ItemShortName;
                    workSheet.Cells[row, 5].Value = dd.val.UnitCode;
                    workSheet.Cells[row, 6].Value = dd.val.StockQty;
                    workSheet.Cells[row, 7].Value = dd.val.LocationRemark;

                    workSheet.Cells[row, 1].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                    workSheet.Cells[row, 2].Style.HorizontalAlignment = ExcelHorizontalAlignment.Left;
                    workSheet.Cells[row, 3].Style.HorizontalAlignment = ExcelHorizontalAlignment.Left;
                    workSheet.Cells[row, 4].Style.HorizontalAlignment = ExcelHorizontalAlignment.Left;
                    workSheet.Cells[row, 5].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                    workSheet.Cells[row, 6].Style.HorizontalAlignment = ExcelHorizontalAlignment.Right;
                    workSheet.Cells[row, 7].Style.HorizontalAlignment = ExcelHorizontalAlignment.Left;


                    string row_str = String.Format("A{0}:G{0}", row.ToString());
                    workSheet.Cells[row_str].Style.Border.Right.Style = ExcelBorderStyle.Thin;
                    workSheet.Cells[row_str].Style.Border.Bottom.Style = ExcelBorderStyle.Thin;
                    workSheet.Cells[row_str].Style.Border.Bottom.Color.SetColor(color_bottom_line);
                    if (dd.i == 0)
                    {
                        workSheet.Cells[row_str].Style.Fill.PatternType = ExcelFillStyle.Solid;
                        workSheet.Cells[row_str].Style.Font.SetFromFont(new Font("Segoe UI", 10));
                        workSheet.Cells[row_str].Style.Font.Bold = true;
                        workSheet.Cells[row_str].Style.Fill.BackgroundColor.SetColor(color_parent);
                    }
                    else
                    {
                        workSheet.Cells[row_str].Style.Font.SetFromFont(new Font("Segoe UI", 10));
                        workSheet.Cells[row_str].Style.Font.Bold = false;

                    }

                }
                workSheet.Cells[String.Format("A{0}:G{0}", row)].Style.Border.Bottom.Style = ExcelBorderStyle.Thin;
                workSheet.Cells[String.Format("A{0}:G{0}", row)].Style.Border.Bottom.Color.SetColor(Color.Black);

                workSheet.Protection.IsProtected = false;
                workSheet.Protection.AllowSelectLockedCells = false;
                workSheet.Cells[workSheet.Dimension.Address].AutoFitColumns();
                //workSheet.Column(1).Width = 15;
                using (var memoryStream = new MemoryStream())
                {
                    Response.ContentType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";
                    Response.AddHeader("content-disposition", "attachment;  filename=ItemCommon.xlsx");
                    excel.SaveAs(memoryStream);
                    memoryStream.WriteTo(Response.OutputStream);
                    Response.Flush();
                    Response.End();
                }
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", ex.Message);
                throw;
            }
        }

        // POST: /StockItems/GetItemShortName
        [HttpPost]
        public JsonResult GetItemShortName(string Prefix)
        {
            var itemShortName = tb.MasterItems
                            .Where(x => x.ItemShortName.StartsWith(Prefix) && x.IsActive == true)
                            .Select(s => s.ItemShortName)
                            .Distinct()
                            .Take(50)
                            .Select(s => new { label = s, val = s }).ToList();
            return Json(itemShortName);
        }

        // POST: /StockItems/GetItemName
        [HttpPost]
        public JsonResult GetItemName(string Prefix)
        {
            var itemName = tb.MasterItems
                            .Where(x => x.ItemName.Contains(Prefix) && x.IsActive == true)
                            .Select(s => s.ItemName)
                            .Distinct()
                            .Take(50)
                            .Select(s => new { label = s, val = s }).ToList();
            return Json(itemName);
        }

        // POST: /StockItems/GetModelNo
        [HttpPost]
        public JsonResult GetModelNo(string Prefix)
        {
            var modelNo = tb.MasterItems
                            .Where(x => x.ModelNo.Contains(Prefix) && x.IsActive == true)
                            .Select(s => s.ModelNo)
                            .Distinct()
                            .Take(50)
                            .Select(s => new { label = s, val = s }).ToList();
            return Json(modelNo);
        }

        // POST: /StockItems/GetItemNo
        [HttpPost]
        public JsonResult GetItemNo(string Prefix)
        {
            var modelNo = tb.MasterItems
                            .Where(x => x.ItemNo.Contains(Prefix) && x.IsActive == true)
                            .Select(s => s.ItemNo)
                            .Distinct()
                            .Take(50)
                            .Select(s => new { label = s, val = s }).ToList();
            return Json(modelNo);
        }


        protected override void Dispose(bool disposing)
        {
            tb.Dispose();
            base.Dispose(disposing);
        }
       

        
	}
}