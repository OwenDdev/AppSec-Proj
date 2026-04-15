namespace SecureApp.API.Models
{
    public class AppFile
    {
        public int Id { get; set; }
        public string FileName { get; set; }
        public string FilePath { get; set; }
        public string UploadedBy { get; set; }
        public DateTime UploadDate { get; set; }
    }
}
