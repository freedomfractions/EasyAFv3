namespace EasyAF.Import
{
    public class ImportOptions
    {
        public bool StrictMissingRequiredHeaders { get; set; } = false; // if true, any Required+Error header missing aborts import
    }
}
