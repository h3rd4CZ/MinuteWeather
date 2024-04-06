namespace MauiNotifications.Services
{
    
    public record struct LogItem(DateTime date, string log);
    public class UserFileDataService
    {
        string filePath = $"{FileSystem.AppDataDirectory}/logDebug.txt";

        public const string LOG_DELIMITER = "$$";

        public async Task<IList<string>> ReadAllData()
        {
            EnsureFile();

            return await File.ReadAllLinesAsync(filePath);
        }

        public void WriteData(string line)
        {
            EnsureFile();

            var time = DateTime.Now;

            File.AppendAllText(filePath, $"{time}{LOG_DELIMITER}{line}{Environment.NewLine}");
        }

        public async Task WriteDataAsync(string line)
        {
            EnsureFile();

            var time = DateTime.Now;
                        
            await File.AppendAllTextAsync(filePath, $"{time}{LOG_DELIMITER}{line}{Environment.NewLine}");
        }

        public void DeleteAllData() => File.Delete(filePath);

        private void EnsureFile()
        {
            if(!File.Exists(filePath))
            {
                using (File.Create(filePath)) { }
            }
        }
    }
}
