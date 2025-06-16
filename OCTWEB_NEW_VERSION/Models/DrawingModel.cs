using PagedList;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace OCTWEB_NET45.Models
{
    public class DrawingModel
    {

    }

    public class FileShareModel
    {
        public int id { get; set; }

        [Required]
        [MaxLength(80)]
        [RegularExpression(@"^[a-zA-Z0-9-_\s]*$", ErrorMessage = "Special Characters are not allowed.")]
        [Display(Name = "File Name")]
        public string name { get; set; }

        [Display(Name = "File Name")]
        public string file { get; set; }

        [Range(0, int.MaxValue, ErrorMessage = "Only positive number allowed")]
        [Display(Name = "Revision")]
        public int rev { get; set; }

        public DateTime date { get; set; }
        public string date_str { get; set; }

        [MaxLength(220)]
        [Display(Name = "File Name")]
        public string note { get; set; }
    }

    public class FileShareListModel
    {
        public int? Page { get; set; }
        public string sortOrder { get; set; }
        public int? file_name_id { get; set; }
        public List<SelectListItem> select_name { get; set; }
        //public IPagedList<FileShareModel> filesharePagedList { get; set; }
        public List<FileShareModel> fileshareList { get; set; }
    }

    public class PartNameModel
    {
        public int id { get; set; }

        [Required]
        [MaxLength(50)]
        [Display(Name = "Part Name")]
        public string name { get; set; }
    }
    public class PartNameListModel
    {
        public int? Page { get; set; }
        public string sortOrder { get; set; }
        public string partname { get; set; }
        //public IPagedList<PartNameModel> PartNamePagedList { get; set; }
        public List<PartNameModel> PartNameList { get; set; }
    }

    public class ProcessModel
    {
        public int id { get; set; }

        [Required]
        [MaxLength(40)]
        [Display(Name = "Process Code")]
        public string processCode { get; set; }

        [Required]
        [Range(0, int.MaxValue, ErrorMessage = "Only positive number allowed.")]
        [Display(Name = "Processs No")]
        public int processNo { get; set; }

        [Required]
        [Display(Name = "Group Line Name")]
        public int partName_id { get; set; }

        public string partName { get; set; }
        public List<SelectListItem> SelectPartName { get; set; }
    }

    public class ProcessListModel
    {
        public int? Page { get; set; }
        public string sortOrder { get; set; }
        public string search_processCode { get; set; }
        public string search_processNo { get; set; }
        public int? search_partName_id { get; set; }

        public List<SelectListItem> SelectPartName { get; set; }
        //public IPagedList<ProcessModel> ProcessPagedList { get; set; }
        public List<ProcessModel> ProcessList { get; set; }

    }


    public class ProcessDrawingModel
    {
        public int id { get; set; }

        [Required]
        [MaxLength(15)]
        [RegularExpression(@"^[a-zA-Z0-9-_()]*$", ErrorMessage = "Special characters are not allowed.")]
        [Display(Name = "Drawing No.")]
        public string drawingNumber { get; set; }

        [Required]
        [MaxLength(2)]
        [RegularExpression(@"^[0-9]*$", ErrorMessage = "Only positive number allowed.")]
        [Display(Name = "Revision")]
        public string rev { get; set; }

        [Required]
        [MaxLength(50)]
        [RegularExpression(@"^[a-zA-Z0-9-_()]*$", ErrorMessage = "Special charecters are not allowed.")]
        [Display(Name = "Model No.")]
        public string modelNo { get; set; } //MOP_Name

        public string file { get; set; }

        [MaxLength(200)]
        [Display(Name = "Remark")]
        public string remark { get; set; } //MOP_Note1

        public int? _hide { get; set; }
        public int? _lock { get; set; }

        [Required]
        [Display(Name = "Line Name")]
        public int? process_id { get; set; } //get SelectLineName
        [Required]
        [Display(Name = "Part Name")]
        public int? partName_id { get; set; } //get SelectPartName
        public int? groupLine_id { get; set; } //get SelectGroupName
        public string groupLine { get; set; } //Process/groupLineName(PartName)
        public string lineName { get; set; } //Process/ProcessCode
        public string partName { get; set; } //PartName/PartName

        public List<SelectListItem> SelectGroupLine { get; set; }
        public List<SelectListItem> SelectLineName { get; set; }
        public List<SelectListItem> SelectPartName { get; set; }


    }

    public class ProcessDrawingListModel
    {
        public int? Page { get; set; }
        public string sortOrder { get; set; }
        public int? groupLine_id { get; set; } //Process/groupLineName(PartName)
        public string partName { get; set; }  //PartName/PartName
        public string lineName { get; set; } //Process/ProcessCode
        public string drawingNo { get; set; }
        public string modelNo { get; set; }
        public List<SelectListItem> SelectGroupLine { get; set; }
        //public IPagedList<ProcessDrawingModel> ProcessDrawingPagedList { get; set; }
        public List<ProcessDrawingModel> ProcessDrawingList { get; set; }
    }

    public class EngineerDrawingModel
    {
        public int id { get; set; }

        [Required]
        [MaxLength(20)]
        [Display(Name = "Level")]
        public string level { get; set; }

        [Required]
        [MaxLength(15)]
        [RegularExpression(@"^[a-zA-Z0-9-_()]*$", ErrorMessage = "Special characters are not allowed.")]
        [Display(Name = "Drawing No.")]
        public string drawingNo { get; set; }

        [Required]
        [MaxLength(2)]
        [RegularExpression(@"^[0-9]*$", ErrorMessage = "Only positive number allowed.")]
        [Display(Name = "Revision")]
        public string rev { get; set; }

        [Required]
        [MaxLength(50)]
        [RegularExpression(@"^[a-zA-Z0-9-_()@.+*]*$", ErrorMessage = "Special characters are not allowed.")]
        [Display(Name = "Model No.")]
        public string modelNo { get; set; }


        [MaxLength(150)]
        [RegularExpression(@"^[a-zA-Z0-9-_()@.+*]*$", ErrorMessage = "Special charecters are not allowed.")]
        [Display(Name = "Semi Assembly No.")]
        public string semiAssemblyNo { get; set; }

        [MaxLength(150)]
        [RegularExpression(@"^[a-zA-Z0-9-_()@.+*]*$", ErrorMessage = "Special charecters are not allowed.")]
        [Display(Name = "Customer No.")]
        public string customerNo { get; set; }

        public string drawing_file { get; set; }
        public string partList_file { get; set; }

        [MaxLength(200)]
        [Display(Name = "Note")]
        public string note { get; set; }
        public bool _hide { get; set; }
        public bool _lock { get; set; }

        [Required]
        [Display(Name = "Part Name")]
        public int? partName_id { get; set; }

        public string tempPartName { get; set; }
        public string partName { get; set; }
        public int formulas_count { get; set; }

        public List<SelectListItem> selectPartName { get; set; }
    }

    public class EngineerDrawingListModel
    {
        public int? Page { get; set; }
        public string sortOrder { get; set; }

        public string partName { get; set; }
        public string drawingNo { get; set; }
        public string modelNo { get; set; }
        public int sortby_id { get; set; }

        public List<SelectListItem> SelectSortBy { get; set; }
        //public IPagedList<EngineerDrawingModel> engineerDrawingPagedList { get; set; }
        public List<EngineerDrawingModel> engineerDrawingList { get; set; }
    }

    public class FormulasModel
    {
        public int id_parent { get; set; }
        public string drawing_parent { get; set; }
        public string model_parent { get; set; }
        public string partName_parent { get; set; }

        public string searchDrawingNo_child { get; set; }
        public string searchModelNo_child { get; set; }
        public string searchPartName_child { get; set; }

        public int id_child { get; set; }
        public string model_child { get; set; }
        public string drawingNo_child { get; set; }
        public string partName_child { get; set; }
        public string level_child { get; set; }


        [Display(Name = "Quantity")]
        public float quantity_child { get; set; }


        [MaxLength(15)]
        [Display(Name = "Unit Name")]
        [DataType(DataType.Text)]
        [RegularExpression(@"^[a-zA-Z-]*$", ErrorMessage = "Special characters, numeric, Thai characters and spaces are not allowed.")]
        public string unit_child { get; set; }

        [MaxLength(200)]
        [Display(Name = "Note")]
        public string note_child { get; set; }

        public List<FormulasChildModel> ChildModelList { get; set; }
        public List<FormulasSelectChildModel> SelectChildModelList { get; set; }

    }

    public class FormulasChildModel
    {
        public bool check { get; set; }
        public int id { get; set; }
        public string model { get; set; }
        public string drawingNo { get; set; }
        public string partName { get; set; }
        public int? partName_id { get; set; }
        public string level { get; set; }

        public float quantity { get; set; }
        public string unit { get; set; }
        public string note { get; set; }

    }
    public class FormulasSelectChildModel
    {
        public int id_child { get; set; }
        public string model_child { get; set; }
        public string drawingNo_child { get; set; }
        public string partName_child { get; set; }
        public string level_child { get; set; }

        public string quantity_child { get; set; }
        public string unit_child { get; set; }
        public string note_child { get; set; }

        public int check_select { get; set; }

    }

    public class JsTreeModel
    {
        public string id { get; set; }
        public string parent { get; set; }
        public string text { get; set; }
        public StateModel state { get; set; }
        public string type { get; set; }
        //public bool children { get; set; } // if node has sub-nodes set true or not set false
    }
    public class StateModel
    {
        public bool opened { get; set; }
        public bool selected { get; set; }

        public StateModel()
        {
            opened = true;
            selected = false;
        }

    }

    public class ChildFormulasModel
    {
        public int id { get; set; }
        public string drawing_file { get; set; }
        public string partlist_file { get; set; }
        public string level { get; set; }
        public string drawingNo { get; set; }
        public string revision { get; set; }
        public string modelNo { get; set; }
        public string semiAssembly { get; set; }
        public string customerNo { get; set; }
        public string partName { get; set; }
        public float quantity { get; set; }
        public string unitname { get; set; }
        public string usage { get; set; }// qty unitname
        public string note { get; set; }
    }

    public class ViewFormulasModel
    {
        public int id { get; set; }
        public string drawingNo { get; set; }
        public string modelNo { get; set; }
        public string partName { get; set; }
        public List<JsTreeModel> JsTreeModelList { get; set; }
        public List<ChildFormulasModel> ChildFormulasModelList { get; set; }
    }

    public class CommonModel
    {
        public int id { get; set; }
        public string drawingNo { get; set; }
        public string modelNo { get; set; }
        public string partName { get; set; }
        public List<ParentFormulasModel> ParentFormulasModelList { get; set; }
    }

    public class ParentFormulasModel
    {
        public int id { get; set; }
        public string level { get; set; }
        public string drawingNo { get; set; }
        public string revision { get; set; }
        public string modelNo { get; set; }
        public string semiAssembly { get; set; }
        public string customerNo { get; set; }
        public string partName { get; set; }
        public string note { get; set; }
    }

    public class DrawingReportModel
    {
        public int? Page { get; set; }
        public string sortOrder { get; set; }
        public int drawingType_id { get; set; }
        //For Process
        public int? groupLine_id_pr { get; set; } //Process/groupLineName(PartName)
        public string partName_pr { get; set; }  //PartName/PartName
        public string lineName_pr { get; set; } //Process/ProcessCode
        public string drawingNo_pr { get; set; }
        public string modelNo_pr { get; set; }
        public List<SelectListItem> SelectGroupLine { get; set; }
        //For Engineer
        public string partName_en { get; set; }
        public string drawingNo_en { get; set; }
        public string modelNo_en { get; set; }
        public string level_en { get; set; } // for engineer
        public string semiAssemblyNo_en { get; set; }
        public string customerNo_en { get; set; }

        public List<SelectListItem> SelectDrawingType { get; set; }
        //public IPagedList<EngineerDrawingModel> engineerDrawingPagedList { get; set; }
        //public IPagedList<ProcessDrawingModel> ProcessDrawingPagedList { get; set; }
        public List<EngineerDrawingModel> EngineerDrawingList { get; set; }
        public List<ProcessDrawingModel> ProcessDrawingList { get; set; }
    }

    public class ErrorDeleteDrawingModel
    {
        public int main_id { get; set; }
        public string main_drawing_no { get; set; }
        public string main_model_no { get; set; }
        public string main_part_name { get; set; }
        public List<ErrorDeleteModel> Parent_used { get; set; }
        public List<ErrorDeleteModel> Child_used { get; set; }
    }
    public class ErrorDeleteModel
    {
        public int id { get; set; }
        public string level { get; set; }
        public string drawing_no { get; set; }
        public string model_no { get; set; }
        public string part_name { get; set; }
        public int formulas_count { get; set; }
    }
}