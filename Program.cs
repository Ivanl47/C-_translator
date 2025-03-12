using System;
using System.IO;
using System.Text.Json;
using Newtonsoft.Json;
using System.Collections.Generic;

class Program
{
    static string settingsFile = "settings.json";
    static string dictionaryPath;
    static string savePath;

    static void Main()
    {
        LoadSettings();

        while (true)
        {
            Console.WriteLine("Enter word (or type 'change save directory' to update settings):");
            string inputWord = Console.ReadLine();

            if (inputWord.ToLower() == "change save directory")
            {
                ConfigureSettings();
                continue;
            }

            string translation = Translator(inputWord);
            Console.WriteLine(translation);
        }
    }

    static void LoadSettings()
    {
        if (File.Exists(settingsFile))
        {
            try
            {
                string json = File.ReadAllText(settingsFile);
                var settings = JsonConvert.DeserializeObject<Dictionary<string, string>>(json);

                if (settings != null && settings.ContainsKey("dictionaryPath") && settings.ContainsKey("savePath"))
                {
                    dictionaryPath = settings["dictionaryPath"];
                    savePath = settings["savePath"];
                    return;
                }
            }
            catch { }
        }
        ConfigureSettings();
    }

    static void ConfigureSettings()
    {
        Console.WriteLine("Enter path to dictionary file:");
        dictionaryPath = Console.ReadLine();
        Console.WriteLine("Enter path to save translations:");
        savePath = Console.ReadLine();

        var settings = new Dictionary<string, string>
        {
            { "dictionaryPath", dictionaryPath },
            { "savePath", savePath }
        };

        File.WriteAllText(settingsFile, JsonConvert.SerializeObject(settings, Formatting.Indented));
    }

    static string Translator(string word)
    {
        if (!File.Exists(dictionaryPath))
        {
            return "Error: Dictionary file not found!";
        }

        string jsonContent = File.ReadAllText(dictionaryPath);
        List<Dictionary<string, string>> dictionary;

        try
        {
            dictionary = JsonConvert.DeserializeObject<List<Dictionary<string, string>>>(jsonContent);
            if (dictionary == null) return "Error: Invalid dictionary format!";
        }
        catch
        {
            return "Error: Could not parse dictionary!";
        }

        foreach (var entry in dictionary)
        {
            if (entry.ContainsKey("ukr") && entry.ContainsKey("eng"))
            {
                if (entry["ukr"] == word)
                {
                    AskToSave(word, entry["eng"]);
                    return entry["eng"];
                }
                else if (entry["eng"] == word)
                {
                    AskToSave(word, entry["ukr"]);
                    return entry["ukr"];
                }
            }
        }

        return "Word not found";
    }

    static void AskToSave(string original, string translated)
    {
        Console.WriteLine($"Save translation: \"{original}\" -> \"{translated}\"? (y/n)");
        string userResponse = Console.ReadLine()?.ToLower();

        if (userResponse == "y")
        {
            SaveTranslation(original, translated);
            Console.WriteLine("Translation saved.");
        }
    }

    static void SaveTranslation(string original, string translated)
    {
        List<Dictionary<string, string>> translations = new List<Dictionary<string, string>>();

        if (File.Exists(savePath))
        {
            string existingJson = File.ReadAllText(savePath);
            try
            {
                translations = JsonConvert.DeserializeObject<List<Dictionary<string, string>>>(existingJson) ?? new List<Dictionary<string, string>>();
            }
            catch
            {
                translations = new List<Dictionary<string, string>>();
            }
        }

        translations.Add(new Dictionary<string, string> { { "original", original }, { "translated", translated } });

        string newJson = JsonConvert.SerializeObject(translations, Formatting.Indented);
        File.WriteAllText(savePath, newJson);
    }
}
