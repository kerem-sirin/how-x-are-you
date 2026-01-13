using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Linq;
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
                Debug.LogError("❌ Please assign a Question CSV File to the profile first!");
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
                Debug.LogError("❌ Please assign a Tier CSV File first!");
                return;
            }
            ImportTiers(profile);
        }

        GUILayout.Space(10);

        // --- BUTTON 3: UI & FLAGS ---
        GUI.backgroundColor = Color.yellow;
        if (GUILayout.Button("IMPORT UI & FLAGS (CSV)", GUILayout.Height(40)))
        {
            if (profile.uiCsvFile == null)
            {
                Debug.LogError("❌ Please assign a UI CSV File first!");
                return;
            }
            ImportUI(profile);
        }

        GUILayout.Space(20);

        // --- BUTTON 4: VALIDATE ---
        GUI.backgroundColor = Color.magenta;
        if (GUILayout.Button("VALIDATE PROFILE", GUILayout.Height(30)))
        {
            ValidateProfile(profile);
        }

        GUI.backgroundColor = Color.white;
    }

    // --- VALIDATION ---
    private void ValidateProfile(QuizProfile profile)
    {
        Debug.Log($"🔍 Validating Profile: {profile.name}...");

        int errorCount = 0;
        int warningCount = 0;

        // 1. Check for duplicate question IDs
        if (profile.questions != null && profile.questions.Count > 0)
        {
            var duplicates = profile.questions
                .GroupBy(q => q.id)
                .Where(g => g.Count() > 1)
                .Select(g => g.Key)
                .ToList();

            if (duplicates.Count > 0)
            {
                Debug.LogError($"❌ DUPLICATE QUESTION IDS FOUND: {string.Join(", ", duplicates)}");
                errorCount++;
            }
        }

        // 2. Check for missing question images
        if (profile.questions != null)
        {
            List<string> missingQuestionImages = new List<string>();
            List<string> missingAnswerImages = new List<string>();

            foreach (var q in profile.questions)
            {
                if (q.useQuestionAnswer && q.questionVisual == null)
                    missingQuestionImages.Add(q.id);

                if (q.useImageAnswers)
                {
                    if (q.answer0_Image == null) missingAnswerImages.Add($"{q.id}_00");
                    if (q.answer1_Image == null) missingAnswerImages.Add($"{q.id}_01");
                    if (q.answer2_Image == null) missingAnswerImages.Add($"{q.id}_02");
                    if (q.answer3_Image == null) missingAnswerImages.Add($"{q.id}_03");
                }
            }

            if (missingQuestionImages.Count > 0)
            {
                Debug.LogWarning($"⚠️ MISSING QUESTION IMAGES ({missingQuestionImages.Count}): Q_{string.Join(", Q_", missingQuestionImages)}");
                warningCount++;
            }

            if (missingAnswerImages.Count > 0)
            {
                Debug.LogWarning($"⚠️ MISSING ANSWER IMAGES ({missingAnswerImages.Count}): Q_{string.Join(", Q_", missingAnswerImages)}");
                warningCount++;
            }
        }

        // 3. Check for empty questions list
        if (profile.questions == null || profile.questions.Count == 0)
        {
            Debug.LogError("❌ NO QUESTIONS LOADED! Import questions first.");
            errorCount++;
        }
        else
        {
            Debug.Log($"   ✓ Questions: {profile.questions.Count}");
        }

        // 4. Check for empty/missing tiers
        if (profile.tiers == null || profile.tiers.Count == 0)
        {
            Debug.LogError("❌ NO TIERS LOADED! Import tiers first.");
            errorCount++;
        }
        else
        {
            // Check tier coverage (should have 100% tier)
            float maxTier = profile.tiers.Max(t => t.maxPercentage);
            if (maxTier < 100f)
            {
                Debug.LogWarning($"⚠️ Highest tier is {maxTier}%. Consider adding a 100% tier for perfect scores.");
                warningCount++;
            }
            Debug.Log($"   ✓ Tiers: {profile.tiers.Count} (max: {maxTier}%)");
        }

        // 5. Check UI Data
        if (profile.uiData == null)
        {
            Debug.LogError("❌ UI DATA IS NULL!");
            errorCount++;
        }
        else
        {
            List<string> missingUI = new List<string>();

            if (profile.uiData.btnStart == null || string.IsNullOrEmpty(profile.uiData.btnStart.en))
                missingUI.Add("btnStart");
            if (profile.uiData.btnRestart == null || string.IsNullOrEmpty(profile.uiData.btnRestart.en))
                missingUI.Add("btnRestart");
            if (profile.uiData.btnLang == null || string.IsNullOrEmpty(profile.uiData.btnLang.en))
                missingUI.Add("btnLang");
            if (profile.uiData.titleTemplate == null || string.IsNullOrEmpty(profile.uiData.titleTemplate.en))
                missingUI.Add("titleTemplate");
            if (profile.uiData.flagIcon == null)
                missingUI.Add("flagIcon");

            if (missingUI.Count > 0)
            {
                Debug.LogWarning($"⚠️ MISSING UI DATA: {string.Join(", ", missingUI)}");
                warningCount++;
            }
            else
            {
                Debug.Log("   ✓ UI Data: Complete");
            }
        }

        // 6. Check identity fields
        if (string.IsNullOrEmpty(profile.countryName))
        {
            Debug.LogWarning("⚠️ Country Name is empty!");
            warningCount++;
        }
        if (string.IsNullOrEmpty(profile.countryAdjective))
        {
            Debug.LogWarning("⚠️ Country Adjective is empty (used in title)!");
            warningCount++;
        }
        if (string.IsNullOrEmpty(profile.folderName))
        {
            Debug.LogError("❌ Folder Name is empty! Images won't load.");
            errorCount++;
        }

        // Summary
        Debug.Log("────────────────────────────────");
        if (errorCount == 0 && warningCount == 0)
        {
            Debug.Log($"✅ VALIDATION PASSED! Profile '{profile.name}' is ready.");
        }
        else
        {
            Debug.Log($"📊 VALIDATION COMPLETE: {errorCount} errors, {warningCount} warnings");
        }
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

        HashSet<string> seenIds = new HashSet<string>();
        List<string> duplicateIds = new List<string>();
        int missingImageCount = 0;

        for (int i = 1; i < lines.Count; i++)
        {
            string line = lines[i].Trim();
            if (string.IsNullOrEmpty(line)) continue;

            string[] cols = Regex.Split(line, ",(?=(?:[^\"]*\"[^\"]*\")*[^\"]*$)");
            for (int c = 0; c < cols.Length; c++) cols[c] = cols[c].Trim('"', ' ', '\r', '\n');

            if (cols.Length < 14)
            {
                Debug.LogWarning($"⚠️ Skipping invalid row {i}: Not enough columns ({cols.Length}/14).");
                continue;
            }

            if (!int.TryParse(cols[0], out int idNum)) continue;
            string idString = idNum.ToString("D2");

            // Check for duplicate IDs
            if (seenIds.Contains(idString))
            {
                duplicateIds.Add(idString);
                Debug.LogError($"❌ DUPLICATE ID DETECTED: {idString} at row {i}. Skipping!");
                continue;
            }
            seenIds.Add(idString);

            // Parse correct indices (e.g., "0" or "0,1")
            string indexRaw = cols[1];
            List<int> correctList = new List<int>();
            foreach (var part in indexRaw.Split(','))
            {
                if (int.TryParse(part.Trim(), out int val))
                {
                    correctList.Add(val);
                }
            }

            string q_en = cols[2];
            string q_nat = cols[3];
            bool useQImg = bool.Parse(cols[4].ToLower());
            bool useAImg = bool.Parse(cols[5].ToLower());

            QuestionData qData = new QuestionData
            {
                id = idString,
                correctIndices = correctList,
                useQuestionAnswer = useQImg,
                useImageAnswers = useAImg,
                questionText = new LocalizedText { en = q_en, native = q_nat },
                answer0 = new LocalizedText { en = cols[6], native = cols[7] },
                answer1 = new LocalizedText { en = cols[8], native = cols[9] },
                answer2 = new LocalizedText { en = cols[10], native = cols[11] },
                answer3 = new LocalizedText { en = cols[12], native = cols[13] }
            };

            // Load question image
            if (useQImg)
            {
                string path = $"{profile.imageLibraryPath}Q_{idString}.png";
                qData.questionVisual = AssetDatabase.LoadAssetAtPath<Texture2D>(path);
                if (qData.questionVisual == null)
                {
                    Debug.LogWarning($"⚠️ MISSING: {path}");
                    missingImageCount++;
                }
            }

            // Load answer images
            if (useAImg)
            {
                qData.answer0_Image = LoadAnsImage(profile.imageLibraryPath, idString, 0, ref missingImageCount);
                qData.answer1_Image = LoadAnsImage(profile.imageLibraryPath, idString, 1, ref missingImageCount);
                qData.answer2_Image = LoadAnsImage(profile.imageLibraryPath, idString, 2, ref missingImageCount);
                qData.answer3_Image = LoadAnsImage(profile.imageLibraryPath, idString, 3, ref missingImageCount);
            }

            profile.questions.Add(qData);
        }

        EditorUtility.SetDirty(profile);

        // Summary
        Debug.Log("────────────────────────────────");
        Debug.Log($"✅ QUESTIONS IMPORTED: {profile.questions.Count}");

        if (duplicateIds.Count > 0)
            Debug.LogError($"❌ Skipped {duplicateIds.Count} duplicates: {string.Join(", ", duplicateIds)}");

        if (missingImageCount > 0)
            Debug.LogWarning($"⚠️ {missingImageCount} missing images detected. Check warnings above.");
    }

    private Texture2D LoadAnsImage(string rootPath, string id, int ansIndex, ref int missingCount)
    {
        string fileName = $"Q_{id}_{ansIndex.ToString("D2")}.png";
        string fullPath = rootPath + fileName;

        Texture2D tex = AssetDatabase.LoadAssetAtPath<Texture2D>(fullPath);

        if (tex == null)
        {
            Debug.LogWarning($"⚠️ MISSING: {fullPath}");
            missingCount++;
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

            if (cols.Length < 7)
            {
                Debug.LogWarning($"⚠️ Skipping tier row {i}: Not enough columns ({cols.Length}/7).");
                continue;
            }

            TierData tier = new TierData();

            if (float.TryParse(cols[0], out float percent))
                tier.maxPercentage = percent;

            tier.title = new LocalizedText { en = cols[1], native = cols[2] };
            tier.subtitle = new LocalizedText { en = cols[3], native = cols[4] };
            tier.description = new LocalizedText { en = cols[5], native = cols[6] };

            profile.tiers.Add(tier);
        }

        // Sort by percentage (ascending)
        profile.tiers.Sort((a, b) => a.maxPercentage.CompareTo(b.maxPercentage));

        EditorUtility.SetDirty(profile);
        Debug.Log($"✅ TIERS IMPORTED: {profile.tiers.Count} items.");

        // Warn if no 100% tier
        if (profile.tiers.Count > 0)
        {
            float maxTier = profile.tiers[profile.tiers.Count - 1].maxPercentage;
            if (maxTier < 100f)
            {
                Debug.LogWarning($"⚠️ Highest tier is {maxTier}%. Players scoring above this will get this tier.");
            }
        }
    }

    private void ImportUI(QuizProfile profile)
    {
        string rawText = profile.uiCsvFile.text;
        List<string> lines = ParseCsvRows(rawText);

        if (profile.uiData == null) profile.uiData = new UIData();

        Debug.Log($"🎨 Starting UI Import for {profile.name}...");

        // Load Flag Image
        string flagPath = $"{profile.imageLibraryPath}Flag.png";
        profile.uiData.flagIcon = AssetDatabase.LoadAssetAtPath<Texture2D>(flagPath);
        if (profile.uiData.flagIcon == null)
            Debug.LogWarning($"⚠️ Missing Flag Image at: {flagPath}");

        // Track which keys were found
        HashSet<string> foundKeys = new HashSet<string>();
        HashSet<string> requiredKeys = new HashSet<string> { "btn_start", "btn_restart", "btn_lang", "title_template" };

        // Parse CSV Rows (Format: Key, EN, Native)
        for (int i = 1; i < lines.Count; i++)
        {
            string line = lines[i].Trim();
            if (string.IsNullOrEmpty(line)) continue;

            string[] cols = Regex.Split(line, ",(?=(?:[^\"]*\"[^\"]*\")*[^\"]*$)");
            for (int c = 0; c < cols.Length; c++) cols[c] = cols[c].Trim('"', ' ', '\r', '\n');

            if (cols.Length < 3) continue;

            string key = cols[0].ToLower();
            string en = cols[1];
            string nat = cols[2];

            foundKeys.Add(key);

            switch (key)
            {
                case "btn_start":
                    profile.uiData.btnStart = new LocalizedText { en = en, native = nat };
                    break;
                case "btn_restart":
                    profile.uiData.btnRestart = new LocalizedText { en = en, native = nat };
                    break;
                case "btn_lang":
                    profile.uiData.btnLang = new LocalizedText { en = en, native = nat };
                    break;
                case "title_template":
                    profile.uiData.titleTemplate = new LocalizedText { en = en, native = nat };
                    break;
            }
        }

        // Check for missing required keys
        var missingKeys = requiredKeys.Except(foundKeys).ToList();
        if (missingKeys.Count > 0)
        {
            Debug.LogWarning($"⚠️ Missing UI keys in CSV: {string.Join(", ", missingKeys)}");
        }

        EditorUtility.SetDirty(profile);
        Debug.Log("✅ UI Data Imported!");
    }
}