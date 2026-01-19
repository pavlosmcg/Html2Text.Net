using System.Text;

namespace Html2Text.Example
{
    internal class Program
    {
        static void Main(string[] args)
        {
            if (args.Length == 0)
            {
                Console.WriteLine("Usage: Html2Text.Example <path-to-html-file>");
                return;
            }

            var filePath = args[0];
            
            if (!File.Exists(filePath))
            {
                Console.WriteLine($"Error: File not found: {filePath}");
                return;
            }

            using var fileStream = new FileStream(filePath, FileMode.Open, FileAccess.Read);
            using var reader = new StreamReader(fileStream, detectEncodingFromByteOrderMarks: true, bufferSize: 1024, leaveOpen: true);
            var html = reader.ReadToEnd();

            // simply call Html2Text.Convert to get the raw text from HTML
            var rawText = Html2Text.Convert(html);
            Console.WriteLine(rawText);
        }
    }
}
