using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Text.RegularExpressions;

namespace LanguageManager
{
    class Program
    {
        static void Main()
        {
            Console.BackgroundColor = ConsoleColor.Yellow;
            Console.Clear();
            Console.ForegroundColor = ConsoleColor.Black;     //зміна кольору
            string filePath = "languages.txt";
            List<string> languages = new List<string>();

            if (File.Exists(filePath))
            {
                languages = File.ReadAllLines(filePath).ToList();    //заповнення масиву
            }

            while (true)
            {
               
                Console.WriteLine("\nМовний менеджер:");
                Console.WriteLine("1. Переглянути мови");
                Console.WriteLine("2. Додати мову");
                Console.WriteLine("3. Видалити мову");
                Console.WriteLine("4. Переглянути переклади");
                Console.WriteLine("5. Додати переклад");
                Console.WriteLine("6. Видалити переклад");
                Console.WriteLine("7. Шукати переклад за ключем");
                Console.WriteLine("8. Шукати переклад за регулярним виразом");
                Console.WriteLine("9. Вийти");

                Console.Write("Введіть ваш вибір: ");
                if (!int.TryParse(Console.ReadLine(), out int choice))
                {
                    Console.WriteLine("Не правильний вибір. Спробуйте знову.");
                    continue;
                }

                switch (choice)
                {
                    case 1:
                        ViewLanguages(languages);
                        break;
                    case 2:
                        languages = AddLanguage(languages);
                        break;
                    case 3:
                        languages = RemoveLanguage(languages);
                        break;
                    case 4:
                        ViewTranslations(languages);
                        break;
                    case 5:
                        AddTranslation(languages);
                        break;
                    case 6:
                        RemoveTranslation(languages);
                        break;
                    case 7:
                        SearchTranslationByKey(languages);
                        break;
                    case 8:
                        SearchTranslationByRegex(languages);
                        break;
                    case 9:
                        SaveLanguages(languages, filePath);
                        LogAction("Exit the program");
                        Console.WriteLine("Бувай!");
                        Environment.Exit(0); //закриття консолі 
                        break;
                    default:
                        Console.WriteLine("Не правильний вибір. Спробуйте знову.");
                        break;
                }
            }
        }

        static void ViewLanguages(List<string> languages)
        {
            Console.WriteLine("\nМови:");
            if (languages.Count == 0)    //перевірка мови
            {
                Console.WriteLine("Мови не знайдено.");
                LogAction("Viewed languages: None found");
            }
            else
            {
                for (int i = 0; i < languages.Count; i++)
                {
                    Console.WriteLine($"{i + 1}. {languages[i]}");
                }
                LogAction("Viewed languages");
            }
        }

        static List<string> AddLanguage(List<string> languages)     //додавання мови
        {
            Console.Write("\nВведіть мову щоб додати: ");
            string language = Console.ReadLine();

            if (!languages.Contains(language))
            {
                languages.Add(language);
                Console.WriteLine($"{language} мова успішно додана.");
                LogAction($"Added language: {language}");                         
            }
            else
            {
                Console.WriteLine($"{language} мова вже існує.");
                LogAction($"Tried to add existing language: {language}");
            }

            return languages;
        }

        static List<string> RemoveLanguage(List<string> languages)              //видалення мови
        {
            ViewLanguages(languages);

            Console.Write("\nВведіть номер мови для видалення: ");
            if (!int.TryParse(Console.ReadLine(), out int index) || index <= 0 || index > languages.Count)
            {
                Console.WriteLine("Не правильний вибір. Спробуйте знову.");
                LogAction("Failed to remove language: Invalid selection");
                return languages;
            }

            index--;
            string removedLanguage = languages[index];
            languages.RemoveAt(index);
            Console.WriteLine($"{removedLanguage} мова успішно видалена.");
            DeleteTranslationsFile(removedLanguage);
            LogAction($"Removed language: {removedLanguage}");

            return languages;
        }

        static void ViewTranslations(List<string> languages)             //перегляд перекладів
        {
            Console.WriteLine("\nВиберіть мову для перегляду перекладів:");
            ViewLanguages(languages);

            Console.Write("Введіть номер мови: ");
            if (!int.TryParse(Console.ReadLine(), out int index) || index <= 0 || index > languages.Count)
            {
                Console.WriteLine("Не правильний вибір. Спробуйте знову.");
                LogAction("Failed to view translations: Invalid selection");
                return;
            }

            index--; 
            string language = languages[index];
            string filePath = $"{language}.json";

            if (File.Exists(filePath))
            {
                string json = File.ReadAllText(filePath);
                Dictionary<string, string> translations = JsonSerializer.Deserialize<Dictionary<string, string>>(json);

                Console.WriteLine("\nПереклади:");
                foreach (KeyValuePair<string, string> entry in translations)
                {
                    Console.WriteLine($"{entry.Key}: {entry.Value}");
                }
                LogAction($"Viewed translations for language: {language}");
            }
            else
            {
                Console.WriteLine("\nПереклади для цієї мови не знайдено.");
                LogAction($"No translations found for language: {language}");
            }
        }

        static void AddTranslation(List<string> languages)         //додавання перекладу 
        {
            Console.WriteLine("\nВиберіть мову для додавання перекладу:");
            ViewLanguages(languages);

            Console.Write("Введіть номер мови: ");
            if (!int.TryParse(Console.ReadLine(), out int index) || index <= 0 || index > languages.Count)
            {
                Console.WriteLine("Не правильний вибір. Спробуйте знову.");
                LogAction("Failed to add translation: Invalid selection");
                return;
            }

            index--; 
            string language = languages[index];
            string filePath = $"{language}.json";

            Console.Write("\nВведіть ключ для перекладу: ");
            string key = Console.ReadLine();

            Console.Write("Введіть переклад: ");
            string value = Console.ReadLine();

            Dictionary<string, string> translations;

            if (File.Exists(filePath))
            {
                string json = File.ReadAllText(filePath);
                translations = JsonSerializer.Deserialize<Dictionary<string, string>>(json);
            }
            else
            {
                translations = new Dictionary<string, string>();
            }

            if (!translations.ContainsKey(key))
            {
                translations[key] = value;
                SaveJson(filePath, translations);
                Console.WriteLine($"Переклад '{key}' успішно додано.");
                LogAction($"Added translation for {language}: {key} -> {value}");
            }
            else
            {
                Console.WriteLine($"Переклад '{key}' вже існує.");
                LogAction($"Tried to add existing translation for {language}: {key}");
            }
        }

        static void RemoveTranslation(List<string> languages)
        {
            Console.WriteLine("\nВиберіть мову для видалення перекладу:");
            ViewLanguages(languages);

            Console.Write("Введіть номер мови: ");
            if (!int.TryParse(Console.ReadLine(), out int index) || index <= 0 || index > languages.Count)
            {
                Console.WriteLine("Не правильний вибір. Спробуйте знову.");
                LogAction("Failed to remove translation: Invalid selection");
                return;
            }

            index--; 
            string language = languages[index];
            string filePath = $"{language}.json";

            if (File.Exists(filePath))
            {
                Console.Write("\nВведіть ключ для видалення перекладу: ");
                string key = Console.ReadLine();

                string json = File.ReadAllText(filePath);
                Dictionary<string, string> translations = JsonSerializer.Deserialize<Dictionary<string, string>>(json);

                if (translations.ContainsKey(key))
                {
                    translations.Remove(key);
                    SaveJson(filePath, translations);
                    Console.WriteLine($"Переклад '{key}' успішно видалено.");
                    LogAction($"Removed translation for {language}: {key}");
                }
                else
                {
                    Console.WriteLine($"Переклад '{key}' не знайдено.");
                    LogAction($"Failed to remove translation for {language}: {key} not found");
                }
            }
            else
            {
                Console.WriteLine("\nПереклади для цієї мови не знайдено.");
                LogAction($"No translations found for language: {language}");
            }
        }

        static void SearchTranslationByKey(List<string> languages)
        {
            Console.Write("\nВведіть ключ для пошуку перекладу: ");
            string key = Console.ReadLine();

            bool found = false;
            foreach (string language in languages)
            {
                string filePath = $"{language}.json";

                if (File.Exists(filePath))
                {
                    string json = File.ReadAllText(filePath);
                    Dictionary<string, string> translations = JsonSerializer.Deserialize<Dictionary<string, string>>(json);

                    if (translations.ContainsKey(key))
                    {
                        Console.WriteLine($"{language}: {translations[key]}");
                        found = true;
                    }
                    else
                    {
                        Console.WriteLine($"{language}:");
                    }
                }
            }

            if (!found)
            {
                Console.WriteLine("Переклад не знайдено.");
            }

            LogAction($"Searched translation by key: {key}");
        }

        static void SearchTranslationByRegex(List<string> languages)
        {
            Console.Write("\nВведіть регулярний вираз для пошуку перекладу: ");
            string pattern = Console.ReadLine();
            Regex regex = new Regex(pattern);

            bool found = false;
            foreach (string language in languages)
            {
                string filePath = $"{language}.json";

                if (File.Exists(filePath))
                {
                    string json = File.ReadAllText(filePath);
                    Dictionary<string, string> translations = JsonSerializer.Deserialize<Dictionary<string, string>>(json);

                    foreach (var entry in translations)
                    {
                        if (regex.IsMatch(entry.Key)) 
                        {
                            Console.WriteLine($"\nПереклад для '{entry.Key}' у мові '{language}': {entry.Value}");
                            found = true;
                        }
                    }
                }
            }

            if (!found)
            {
                Console.WriteLine("Переклад не знайдено.");
            }

            LogAction($"Searched translation by regex: {pattern}");
        }

        static void SaveLanguages(List<string> languages, string filePath)
        {
            File.WriteAllLines(filePath, languages);
        }

        static void SaveJson(string filePath, Dictionary<string, string> translations)
        {
            string json = JsonSerializer.Serialize(translations);
            File.WriteAllText(filePath, json);
        }

        static void DeleteTranslationsFile(string language)
        {
            string filePath = $"{language}.json";
            if (File.Exists(filePath))
            {
                File.Delete(filePath);
            }
        }

        static void LogAction(string action)
        {
            string logFilePath = "logs.txt";
            string logEntry = $"{DateTime.Now}: {action}";
            File.AppendAllText(logFilePath, logEntry + Environment.NewLine);
        }
    }
}