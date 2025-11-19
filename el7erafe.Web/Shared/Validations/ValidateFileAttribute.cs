using Microsoft.AspNetCore.Http;
using System.ComponentModel.DataAnnotations;

namespace Shared.Validations
{
    public class ValidateFileAttribute : ValidationAttribute
    {
        private readonly long _maxFileSize;
        private readonly string[] _allowedExtensions;

        public ValidateFileAttribute(long maxFileSize, string[] allowedExtensions)
        {
            _maxFileSize = maxFileSize;
            _allowedExtensions = allowedExtensions;
        }

        protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
        {
            if (value == null)
            {
                return new ValidationResult("File is required.");
            }

            if (value is IFormFile file)
            {
                if (file.Length == 0)
                {
                    return new ValidationResult("File cannot be empty.");
                }

                if (file.Length > _maxFileSize)
                {
                    return new ValidationResult($"File size cannot exceed {_maxFileSize / 1024 / 1024}MB.");
                }

                var fileExtension = Path.GetExtension(file.FileName).ToLowerInvariant();
                if (string.IsNullOrEmpty(fileExtension) || !_allowedExtensions.Contains(fileExtension))
                {
                    return new ValidationResult($"Only {string.Join(", ", _allowedExtensions)} files are allowed.");
                }
            }
            else
            {
                return new ValidationResult("Invalid file type.");
            }
            
            return ValidationResult.Success; 
        }
    }
}
