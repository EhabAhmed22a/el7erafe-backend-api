using DomainLayer.Contracts;
using DomainLayer.Exceptions;

namespace Service.Helpers
{
    public class BlobFileHelper(IBlobStorageRepository blobStorageRepository)
    {
        public async Task<Dictionary<string, string>> GetMultipleSequenceSASURLs(
            string containerName,
            string lastFileName,
            int expiryHours = 1,
            Func<string, (string prefix, string sequence)> fileNameParser = null)
        {
            if (fileNameParser is null)
                fileNameParser = DefaultFileNameParser;

            var (prefix, sequence) = fileNameParser(lastFileName);

            if (!int.TryParse(sequence, out var lastSeqNum))
                throw new TechnicalException();

            List<string> fileNames = new List<string>();
            for (int i = 1; i <= lastSeqNum; i++)
                fileNames.Add($"{prefix}{i}");

            var sasURLs = await blobStorageRepository.GetMultipleBlobsUrlWithSasTokenAsync(containerName, fileNames, expiryHours);
            return sasURLs;
        }

        private (string prefix, string sequence) DefaultFileNameParser(string fileName)
        {
            if (string.IsNullOrEmpty(fileName))
                return (fileName, fileName);

            var parts = fileName.Split('_');

            return parts.Length > 1
                ? (string.Join("_", parts[0..^1]) + "_", parts[^1])
                : ("", fileName);
        }
    }
}