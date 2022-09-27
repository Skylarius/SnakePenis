using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Localization;
using UnityEngine.Localization.Settings;

public class LocalizedStringUser : MonoBehaviour, IDataPersistence
{
    public static LocalizedStringUser Instance { get; private set; }
    public LocalizedStringTable UITextStringTable;
    public LocalizedStringTable SpecialsStringTable;
    public LocalizedStringTable QuotesTable;

    [Header("Locales")]
    public Locale EN, FR, IT;
    private Locale selectedLocale = null;

    private void Awake()
    {
        if (Instance != null)
        {
            Debug.LogError("Found more than one LocalizedStringUser in the scene.");
        }
        Instance = this;
    }

    private void Start()
    {
        if (selectedLocale != null)
        {
            return;
        }
        if (Application.systemLanguage == SystemLanguage.French)
        {
            Debug.Log("This system is in French. ");
            SetLanguage(FR);
        }
        else if (Application.systemLanguage == SystemLanguage.English)
        {
            Debug.Log("This system is in English. ");
            SetLanguage(EN);
        }
        else if (Application.systemLanguage == SystemLanguage.Italian)
        {
            Debug.Log("This system is in Italian. ");
            SetLanguage(IT);
        }
    }

    public static void SetLanguage(Locale l)
    {
        LocalizationSettings.SelectedLocale = l;
        Instance.selectedLocale = l;
    }

    public static string GetLocalizedStringWithPlayerName(string s)
    {
        Dictionary<string, object> dict = new Dictionary<string, object>();
        dict["player-name"] = ScoreManager.CurrentScoreName;
        return LocalizationSettings.StringDatabase.GetLocalizedString(Instance.UITextStringTable.TableReference, s, new List<object> { dict });
    }

    public static string GetLocalizedStringWithArgs(string s, params KeyValuePair<string, object>[] args)
    {
        Dictionary<object, object> dict = new Dictionary<object, object>();
        for (int i=0; i< args.Length; i++)
        {
            dict[args[i].Key] = args[i].Value;
        }
        return LocalizationSettings.StringDatabase.GetLocalizedString(Instance.UITextStringTable.TableReference, s, new List<object> { dict });
    }

    internal static string GetRandomLocalizedQuote()
    {
        UnityEngine.Localization.Tables.StringTable table = Instance.QuotesTable.GetTable();
        string s = table.SharedData.Entries[UnityEngine.Random.Range(0, table.Count)].Key;
        return GetLocalizedString(s, Instance.QuotesTable);
    }

    public static string GetLocalizedStringWithArray(string s, params string[] args)
    {
        return LocalizationSettings.StringDatabase.GetLocalizedString(
            Instance.UITextStringTable.TableReference, s,
            args);
    }

    public static string GetLocalizedString(string s, LocalizedStringTable table)
    {
        return LocalizationSettings.StringDatabase.GetLocalizedString(table.TableReference, s);
    }

    public static string GetLocalizedBonusString(string s)
    {
        return GetLocalizedString(s, Instance.SpecialsStringTable);
    }

    public static string GetLocalizedUIString(string s)
    {
        return GetLocalizedString(s, Instance.UITextStringTable);
    }

    public static string GetLocalizedQuote(string s)
    {
        return GetLocalizedString(s, Instance.QuotesTable);
    }

    public void LoadData(GameData data)
    {
        if (data.Language == "EN")
        {
            SetLanguage(EN);
        }
        if (data.Language == "FR")
        {
            SetLanguage(FR);
        }
        if (data.Language == "IT")
        {
            SetLanguage(IT);
        }
    }

    public void SaveData(ref GameData data)
    {
        if (selectedLocale == EN)
        {
            data.Language = "EN";
        }
        if (selectedLocale == FR)
        {
            data.Language = "FR";
        }
        if (selectedLocale == IT)
        {
            data.Language = "IT";
        }
    }
}
