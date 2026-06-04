using System.ComponentModel.DataAnnotations;

namespace EventHub.ViewModels;

public class DatabaseSetupViewData
{
    [Required(ErrorMessage = "Sunucu/IP zorunludur.")]
    [Display(Name = "SQL Server IP veya host")]
    public string Server { get; set; } = "localhost";

    [Range(1, 65535, ErrorMessage = "Port 1 ile 65535 arasinda olmalidir.")]
    public int Port { get; set; } = 1433;

    [Required(ErrorMessage = "Veritabani adi zorunludur.")]
    [Display(Name = "Veritabani")]
    public string Database { get; set; } = "EventHubDb";

    [Required(ErrorMessage = "Kullanici adi zorunludur.")]
    [Display(Name = "Kullanici adi")]
    public string UserId { get; set; } = "sa";

    [Required(ErrorMessage = "Sifre zorunludur.")]
    [DataType(DataType.Password)]
    [Display(Name = "Sifre")]
    public string Password { get; set; } = string.Empty;
}
