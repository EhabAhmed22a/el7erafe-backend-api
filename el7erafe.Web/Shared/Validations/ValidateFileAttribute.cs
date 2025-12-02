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
                return null;
            }

            if (value is IFormFile file)
            {
                if (file.Length == 0)
                {
                    return new ValidationResult("الملف لا يمكن أن يكون فارغًا.");
                }

                if (file.Length > _maxFileSize)
                {
                    return new ValidationResult($"حجم الملف يجب ألا يتجاوز {_maxFileSize / 1024 / 1024} ميجابايت.");
                }

                var fileExtension = Path.GetExtension(file.FileName).ToLowerInvariant();
                if (string.IsNullOrEmpty(fileExtension) || !_allowedExtensions.Contains(fileExtension))
                {
                    return new ValidationResult($"يُسمح فقط بملفات من الأنواع التالية: {string.Join(", ", _allowedExtensions)}");
                }
            }
            else
            {
                return new ValidationResult("نوع الملف غير صالح.");
            }
            
            return ValidationResult.Success; 
        }
    }
}
