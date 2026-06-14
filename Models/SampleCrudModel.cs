using System.ComponentModel.DataAnnotations;

namespace Career_Guidance_Platform.Models;

public class SampleCrudModel
{
    public int Id { get; set; }

    [Required(ErrorMessage = "Tên không được để trống")]
    [StringLength(100)]
    public string Name { get; set; } = string.Empty;

    [Required(ErrorMessage = "Mô tả không được để trống")]
    public string Description { get; set; } = string.Empty;

    [Display(Name = "Loại")]
    public string Category { get; set; } = "Default";

    [Display(Name = "Trạng thái")]
    public bool IsActive { get; set; } = true;

    [Display(Name = "Nội dung chi tiết")]
    public string Content { get; set; } = string.Empty;

    [Display(Name = "Ngày tạo")]
    [DataType(DataType.DateTime)]
    public DateTime CreatedDate { get; set; } = DateTime.Now;

    [Display(Name = "Ngày cập nhật")]
    [DataType(DataType.DateTime)]
    public DateTime? UpdatedDate { get; set; }

    [Display(Name = "File đính kèm")]
    public string? AttachmentPath { get; set; }

    [Display(Name = "Hình ảnh")]
    public string? ImagePath { get; set; }
}