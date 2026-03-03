using Microsoft.AspNetCore.Http;
using System.ComponentModel.DataAnnotations;

namespace Shared.Validations
{
    public class ValidateFileListAttribute : ValidationAttribute
    {
        private readonly long _maxFileSize;
        private readonly string[] _allowedExtensions;
        private readonly int? _minFiles;
        private readonly int? _maxFiles;

        // Constructor without count validation
        public ValidateFileListAttribute(long maxFileSize, string[] allowedExtensions)
        {
            _maxFileSize = maxFileSize;
            _allowedExtensions = allowedExtensions;
            _minFiles = null;
            _maxFiles = null;
            ErrorMessage = "من فضلك تأكد من صحة الملفات المرفوعة";
        }

        // Constructor with only max files validation
        public ValidateFileListAttribute(long maxFileSize, string[] allowedExtensions, int maxFiles)
        {
            _maxFileSize = maxFileSize;
            _allowedExtensions = allowedExtensions;
            _minFiles = null;
            _maxFiles = maxFiles;
        }

        // Constructor with min and max files validation
        public ValidateFileListAttribute(long maxFileSize, string[] allowedExtensions, int minFiles, int maxFiles)
        {
            _maxFileSize = maxFileSize;
            _allowedExtensions = allowedExtensions;
            _minFiles = minFiles;
            _maxFiles = maxFiles;
        }

        protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
        {
            // Get display name for better error messages
            var displayName = validationContext.DisplayName;

            // Handle null (optional field)
            if (value == null)
            {
                // If minFiles is specified and > 0, null is invalid
                if (_minFiles.HasValue && _minFiles > 0)
                {
                    return new ValidationResult($"الرجاء رفع {_minFiles} صور على الأقل لـ {displayName}");
                }
                return ValidationResult.Success;
            }

            if (value is List<IFormFile> files)
            {
                // Check minimum files
                if (_minFiles.HasValue && files.Count < _minFiles.Value)
                {
                    if (_minFiles == _maxFiles)
                    {
                        return new ValidationResult($"الرجاء رفع {_minFiles} صور بالضبط لـ {displayName}");
                    }
                    return new ValidationResult($"الرجاء رفع {_minFiles} صور على الأقل لـ {displayName}");
                }

                // Check maximum files
                if (_maxFiles.HasValue && files.Count > _maxFiles.Value)
                {
                    if (_minFiles == _maxFiles)
                    {
                        return new ValidationResult($"الرجاء رفع {_maxFiles} صور بالضبط لـ {displayName}");
                    }
                    return new ValidationResult($"عذراً، يمكنك رفع {_maxFiles} صور كحد أقصى لـ {displayName}");
                }

                // Validate each file
                for (int i = 0; i < files.Count; i++)
                {
                    var file = files[i];

                    // Skip null files
                    if (file == null)
                        continue;

                    // Check if file is empty
                    if (file.Length == 0)
                    {
                        return new ValidationResult($"الصورة رقم {i + 1} لا تحتوي على بيانات. الرجاء اختيار صورة صالحة");
                    }

                    // Check file size
                    if (file.Length > _maxFileSize)
                    {
                        return new ValidationResult($"الصورة رقم {i + 1} حجمها كبير جداً. الحد الأقصى المسموح هو {_maxFileSize / 1024 / 1024} ميجابايت");
                    }

                    // Check file extension
                    var fileExtension = Path.GetExtension(file.FileName).ToLowerInvariant();
                    if (string.IsNullOrEmpty(fileExtension) || !_allowedExtensions.Contains(fileExtension))
                    {
                        return new ValidationResult($"الصورة رقم {i + 1} من نوع غير مدعوم. الأنواع المسموح بها: {string.Join("، ", _allowedExtensions)}");
                    }
                }

                return ValidationResult.Success;
            }

            return new ValidationResult($"نوع البيانات لـ {displayName} غير صالح");
        }
    }
}