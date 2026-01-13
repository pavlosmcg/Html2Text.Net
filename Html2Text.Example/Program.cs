using System.Text;

namespace Html2Text.Example
{
    internal class Program
    {
        static void Main(string[] args)
        {
            using var fileStream = new FileStream("Samples/scottallen.html", FileMode.Open, FileAccess.Read);
            using var reader = new StreamReader(fileStream, Encoding.UTF8, detectEncodingFromByteOrderMarks: true, bufferSize: 1024, leaveOpen: true);
            var html = reader.ReadToEnd();

            // simply call Html2Text.Convert to get the raw text from HTML
            var rawText = Html2Text.Convert(html);
            Console.WriteLine(rawText);
        }
    }
}
