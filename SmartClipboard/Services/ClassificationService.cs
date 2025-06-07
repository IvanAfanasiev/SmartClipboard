using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Metadata;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace SmartClipboard.Services
{
    internal class ClassificationService
    {
        public static ContentType Classify(string text)
        {
            if (Regex.IsMatch(text, @"^(http|https)://")) return ContentType.Link;
            if (Regex.IsMatch(text, @"^[\w\.-]+@[\w\.-]+\.\w+$")) return ContentType.Email;
            if (text.Contains("{") && text.Contains("}") && text.Contains(";")) return ContentType.Code;
            return ContentType.Text;
        }
    }
    public enum ContentType
    {
        Text,
        Link,
        Email,
        Code,
        Unknown
    }
}
