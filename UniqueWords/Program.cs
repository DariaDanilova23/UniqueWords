using System;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

class Program
{
    static void Main()
    {
        Console.OutputEncoding = Encoding.Default;

        string projectDirectory = Directory.GetParent(Directory.GetCurrentDirectory()).Parent.Parent.FullName;
        string filePath = Path.Combine(projectDirectory, "War_and_Peace.txt");

        ConcurrentDictionary<string, int> wordCount = new ConcurrentDictionary<string, int>();

        //Чтение файла и разделение по словам
        Parallel.ForEach(File.ReadLines(filePath), line =>
        {
            var cleanedWords = line
                .Split(new[] { ' ', '\t', '\n', '\r', '.', ',', ';', ':', '!', '?', (char)160 }, StringSplitOptions.RemoveEmptyEntries)
                .Select(word => RemoveBrackets(word.ToLowerInvariant().Trim()));

            //Добавление в словарь
            foreach (var cleanedWord in cleanedWords)
            {
                wordCount.AddOrUpdate(cleanedWord, 1, (_, count) => count + 1);
            }
        });

        // Сортировка по количеству повторений
        var sortedWordCount = wordCount.OrderByDescending(pair => pair.Value).ToList();

        // Постраничный вывод отсортированных слов
        const int pageSize = 25;
        int pageIndex = 0;

        do
        {
            Console.Clear(); 

            Console.WriteLine($"Отсортировано по количеству повторений (Страница {pageIndex + 1}):");
            Console.WriteLine("Слово\t\t\tКоличество");

            for (int i = pageIndex * pageSize; i < (pageIndex + 1) * pageSize && i < sortedWordCount.Count; i++)
            {
                var pair = sortedWordCount[i];
                Console.WriteLine($"{pair.Key.PadRight(20)}\t{pair.Value}");
            }

            Console.WriteLine("\nВыберите действие: 'стрелка влево' - предыдущая страница, 'стрелка вправо' - следующая страница, 'Q' - выход");

            var key = Console.ReadKey().Key;

            switch (key)
            {
                case ConsoleKey.RightArrow:
                    pageIndex = Math.Min(pageIndex + 1, (sortedWordCount.Count - 1) / pageSize);
                    break;
                case ConsoleKey.LeftArrow:
                    pageIndex = Math.Max(pageIndex - 1, 0);
                    break;
                case ConsoleKey.Q:
                    return; 
            }

        } while (true);
        
    }

    static string RemoveBrackets(string input)
    {
        return Regex.Replace(input, "[{}()«».]", string.Empty);
    }
}