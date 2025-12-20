using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using HowX.Data;

[CustomEditor(typeof(QuizProfile))]
public class QuizProfileEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();
        GUILayout.Space(20);

        QuizProfile profile = (QuizProfile)target;

        // --- BUTTON 1: QUESTIONS ---
        GUI.backgroundColor = Color.green;
        if (GUILayout.Button("IMPORT QUESTIONS (CSV)", GUILayout.Height(40)))
        {
            if (profile.quizCsvFile == null)
            {
                Debug.LogError("Please assign a Question CSV File to the profile first!");
                return;
            }
            ImportQuestions(profile);
        }

        GUILayout.Space(10);

        // --- BUTTON 2: TIERS ---
        GUI.backgroundColor = Color.cyan;
        if (GUILayout.Button("IMPORT TIERS (CSV)", GUILayout.Height(40)))
        {
            if (profile.tierCsvFile == null)
            {
                Debug.LogError("Please assign a Tier CSV File first!");
                return;
            }
            ImportTiers(profile);
        }

        GUI.backgroundColor = Color.white;
    }

    // --- HELPER: ROBUST CSV PARSER ---
    private List<string> ParseCsvRows(string rawText)
    {
        List<string> rows = new List<string>();
        bool inQuotes = false;
        System.Text.StringBuilder currentRow = new System.Text.StringBuilder();

        for (int i = 0; i < rawText.Length; i++)
        {
            char c = rawText[i];
            if (c == '\"')
            {
                inQuotes = !inQuotes;
                currentRow.Append(c);
            }
            else if ((c == '\n' || c == '\r') && !inQuotes)
            {
                if (c == '\r' && i + 1 < rawText.Length && rawText[i + 1] == '\n') i++;
                rows.Add(currentRow.ToString());
                currentRow.Clear();
            }
            else
            {
                currentRow.Append(c);
            }
        }
        if (currentRow.Length > 0) rows.Add(currentRow.ToString());
        return rows;
    }

    private void ImportQuestions(QuizProfile profile)
    {
        List<string> lines = ParseCsvRows(profile.quizCsvFile.text);

        if (profile.questions == null) profile.questions = new List<QuestionData>();
        profile.questions.Clear();

        Debug.Log($"📂 Starting Question Import for {profile.name}...");

        for (int i = 1; i < lines.Count; i++)
        {
            string line = lines[i].Trim();
            if (string.IsNullOrEmpty(line)) continue;

            string[] cols = Regex.Split(line, ",(?=(?:[^\"]*\"[^\"]*\")*[^\"]*$)");
            for (int c = 0; c < cols.Length; c++) cols[c] = cols[c].Trim('"', ' ', '\r', '\n');

            if (cols.Length < 14)
            {
                Debug.LogWarning($"⚠️ Skipping invalid row {i}: Not enough columns.");
                continue;
            }

            if (!int.TryParse(cols[0], out int idNum)) continue;
            string idString = idNum.ToString("D2");

            // Input string might be "0" or "0,1" or "0;1"
            string indexRaw = cols[1];
            List<int> correctList = new List<int>();

            // Split by comma (and trim any whitespace)
            foreach (var part in indexRaw.Split(','))
            {
                if (int.TryParse(part.Trim(), out int val))
                {
                    correctList.Add(val);
                }
            }

            string q_en = cols[2]; string q_nat = cols[3];
            bool useQImg = bool.Parse(cols[4].ToLower());
            bool useAImg = bool.Parse(cols[5].ToLower());

            QuestionData qData = new QuestionData();
            qData.id = idString;

            qData.correctIndices = correctList; // Assign the list!

            qData.useQuestionAnswer = useQImg;
            qData.useImageAnswers = useAImg;
            qData.questionText = new LocalizedText { en = q_en, native = q_nat };
            qData.answer0 = new LocalizedText { en = cols[6], native = cols[7] };
            qData.answer1 = new LocalizedText { en = cols[8], native = cols[9] };
            qData.answer2 = new LocalizedText { en = cols[10], native = cols[11] };
            qData.answer3 = new LocalizedText { en = cols[12], native = cols[13] };

            // Image loading logic (Same as before)
            if (useQImg)
            {
                string path = $"{profile.imageLibraryPath}Q_{idString}.png";
                qData.questionVisual = AssetDatabase.LoadAssetAtPath<Texture2D>(path);
                if (qData.questionVisual == null) Debug.LogWarning($"⚠️ MISSING QUESTION IMAGE (ID: {idString}): {path}");
            }

            if (useAImg)
            {
                qData.answer0_Image = LoadAnsImage(profile.imageLibraryPath, idString, 0);
                qData.answer1_Image = LoadAnsImage(profile.imageLibraryPath, idString, 1);
                qData.answer2_Image = LoadAnsImage(profile.imageLibraryPath, idString, 2);
                qData.answer3_Image = LoadAnsImage(profile.imageLibraryPath, idString, 3);
            }

            profile.questions.Add(qData);
        }
        EditorUtility.SetDirty(profile);
        Debug.Log($"✅ QUESTIONS IMPORTED: {profile.questions.Count}");
    }

    private Texture2D LoadAnsImage(string rootPath, string id, int ansIndex)
    {
        string fileName = $"Q_{id}_{ansIndex.ToString("D2")}.png";
        string fullPath = rootPath + fileName;

        Texture2D tex = AssetDatabase.LoadAssetAtPath<Texture2D>(fullPath);

        if (tex == null)
        {
            Debug.LogWarning($"⚠️ MISSING ANSWER IMAGE (ID: {id}, Ans: {ansIndex}): Expected at '{fullPath}'");
        }

        return tex;
    }

    private void ImportTiers(QuizProfile profile)
    {
        List<string> lines = ParseCsvRows(profile.tierCsvFile.text);

        if (profile.tiers == null) profile.tiers = new List<TierData>();
        profile.tiers.Clear();

        Debug.Log($"🏆 Starting Tier Import for {profile.name}...");

        for (int i = 1; i < lines.Count; i++)
        {
            string line = lines[i].Trim();
            if (string.IsNullOrEmpty(line)) continue;

            string[] cols = Regex.Split(line, ",(?=(?:[^\"]*\"[^\"]*\")*[^\"]*$)");
            for (int c = 0; c < cols.Length; c++) cols[c] = cols[c].Trim('"', ' ', '\r', '\n');

            if (cols.Length < 7) continue;

            TierData tier = new TierData();

            if (float.TryParse(cols[0], out float percent)) tier.maxPercentage = percent;

            tier.title = new LocalizedText { en = cols[1], native = cols[2] };
            tier.subtitle = new LocalizedText { en = cols[3], native = cols[4] };
            tier.description = new LocalizedText { en = cols[5], native = cols[6] };

            profile.tiers.Add(tier);
        }

        profile.tiers.Sort((a, b) => a.maxPercentage.CompareTo(b.maxPercentage));

        EditorUtility.SetDirty(profile);
        Debug.Log($"✅ TIERS IMPORTED: {profile.tiers.Count} items.");
    }
}