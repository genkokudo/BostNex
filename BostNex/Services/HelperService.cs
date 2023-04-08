using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Options;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Microsoft.Extensions.Options;
using System.Diagnostics;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;

namespace BostNex.Services
{
    /// <summary>
    /// 分類できないヘルパー
    /// </summary>
    public interface IHelperService
    {
        /// <summary>
        /// C#コメントを削除する
        /// </summary>
        /// <param name="text"></param>
        /// <returns></returns>
        public string CutComments(string text);
    }

    public class HelperService : IHelperService
    {
        public string CutComments(string text)
        {
            var re = @"(@(?:""[^""]*"")+|""(?:[^""\n\\]+|\\.)*""|'(?:[^'\n\\]+|\\.)*')|//.*|/\*(?s:.*?)\*/";
            return Regex.Replace(text, re, "$1");
        }
    }
}
