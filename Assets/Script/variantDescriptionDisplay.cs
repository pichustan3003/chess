using System;
using System.Collections.Generic;
using System.Text;
using TMPro;
using UnityEngine;

public class variantDescriptionDisplay : MonoBehaviour
{
    public TMP_Text titleText;
    public TMP_Text descriptionText;
    public TMP_Text winConditionsText;

    readonly Dictionary<string, string[]> descriptions =
        new(StringComparer.OrdinalIgnoreCase);
    bool hasParsedDescriptions;

    void Start()
    {
        ParseDescriptions();
    }

    public void ParseDescriptions()
    {
        descriptions.Clear();
        hasParsedDescriptions = false;
        TextAsset file = Resources.Load<TextAsset>("desc");
        if (file == null)
        {
            Debug.LogError("Could not load variant descriptions from Resources/desc.txt.");
            return;
        }

        string[] records = file.text.Split('\n');
        for (int i = 0; i < records.Length; i++)
        {
            string record = records[i].TrimEnd('\r');
            if (i == 0) record = record.TrimStart('\uFEFF');
            if (string.IsNullOrWhiteSpace(record)) continue;

            if (!TryParseRecord(record, out List<string> fields, out string error))
            {
                Debug.LogError($"Invalid variant description on line {i + 1}: {error}");
                continue;
            }

            if (fields.Count != 3)
            {
                Debug.LogError($"Variant description on line {i + 1} must have exactly three fields.");
                continue;
            }

            if (fields[0].Equals("title", StringComparison.OrdinalIgnoreCase) &&
                fields[1].Equals("description", StringComparison.OrdinalIgnoreCase) &&
                fields[2].Equals("win conditions", StringComparison.OrdinalIgnoreCase))
            {
                continue;
            }

            for (int field = 0; field < fields.Count; field++)
            {
                fields[field] = fields[field].Replace("\\n", "\n");
                Debug.Log(fields[field]);
            }

            if (string.IsNullOrWhiteSpace(fields[0]))
            {
                Debug.LogError($"Variant description on line {i + 1} has an empty title.");
                continue;
            }

            if (!descriptions.TryAdd(fields[0], fields.ToArray()))
            {
                Debug.LogError($"Duplicate variant description title '{fields[0]}' on line {i + 1}.");
            }
        }

        hasParsedDescriptions = true;
    }

    public void DisplayDescription(string title)
    {
        if (!hasParsedDescriptions)
        {
            ParseDescriptions();
        }

        if (titleText == null || descriptionText == null ||
            winConditionsText == null)
        {
            Debug.LogError("Assign all three TMP text fields on variantDescriptionDisplay.");
            return;
        }

        if (string.IsNullOrWhiteSpace(title) ||
            !descriptions.TryGetValue(title.Trim(), out string[] fields))
        {
            Debug.LogError($"No variant description found for title '{title}'.");
            return;
        }

        titleText.text = fields[0];
        descriptionText.text = $"Description: {fields[1]}";
        winConditionsText.text = $"Win Conditions \n{fields[2]}";
        showMenu();
    }

    static bool TryParseRecord(string record, out List<string> fields, out string error)
    {
        fields = new List<string>();
        error = null;
        int position = 0;

        while (position < record.Length)
        {
            while (position < record.Length && char.IsWhiteSpace(record[position]))
            {
                position++;
            }

            if (position == record.Length) break;
            if (record[position] != '"')
            {
                error = "Each field must begin with a quote.";
                return false;
            }

            position++;
            StringBuilder field = new();
            bool closed = false;
            while (position < record.Length)
            {
                if (record[position] != '"')
                {
                    field.Append(record[position++]);
                    continue;
                }

                if (position + 1 < record.Length && record[position + 1] == '"')
                {
                    field.Append('"');
                    position += 2;
                    continue;
                }

                position++;
                closed = true;
                break;
            }

            if (!closed)
            {
                error = "A quoted field is not closed.";
                return false;
            }

            fields.Add(field.ToString());
            while (position < record.Length && char.IsWhiteSpace(record[position]))
            {
                position++;
            }

            if (position == record.Length) break;
            if (record[position] != ',')
            {
                error = "Expected a comma between fields.";
                return false;
            }

            position++;
        }

        return true;
    }

    void showMenu()
    {
        gameObject.SetActive(true);
    }
    public void hideMenu()
    {
        gameObject.SetActive(false);
    }
}
