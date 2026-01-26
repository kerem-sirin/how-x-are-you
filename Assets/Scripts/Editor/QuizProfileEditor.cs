using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Linq;
using HowX.Data;
using UnityEngine.Networking;

[CustomEditor(typeof(QuizProfile))]
public class QuizProfileEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();
        GUILayout.Space(20);

        QuizProfile profile = (QuizProfile)target;

        // ========== CSV IMPORT SECTION ==========
        EditorGUILayout.LabelField("Import from CSV (Local Files)", EditorStyles.boldLabel);
        GUILayout.Space(5);

        EditorGUILayout.BeginHorizontal();

        // --- BUTTON 1: QUESTIONS (CSV) ---
        GUI.backgroundColor = new Color(0.6f, 0.9f, 0.6f); // Light green
        if (GUILayout.Button("IMPORT QUESTIONS\n(CSV)", GUILayout.Height(50)))
        {
            if (profile.quizCsvFile == null)
            {
                EditorUtility.DisplayDialog("Missing File", "Please assign a Quiz CSV File first!", "OK");
                return;
            }
            ImportQuestions(profile, profile.quizCsvFile.text);
        }

        // --- BUTTON 2: TIERS (CSV) ---
        GUI.backgroundColor = new Color(0.6f, 0.9f, 0.9f); // Light cyan
        if (GUILayout.Button("IMPORT TIERS\n(CSV)", GUILayout.Height(50)))
        {
            if (profile.tierCsvFile == null)
            {
                EditorUtility.DisplayDialog("Missing File", "Please assign a Tier CSV File first!", "OK");
                return;
            }
            ImportTiers(profile, profile.tierCsvFile.text);
        }

        // --- BUTTON 3: UI & FLAGS (CSV) ---
        GUI.backgroundColor = new Color(0.9f, 0.9f, 0.6f); // Light yellow
        if (GUILayout.Button("IMPORT UI & FLAGS\n(CSV)", GUILayout.Height(50)))
        {
            if (profile.uiCsvFile == null)
            {
                EditorUtility.DisplayDialog("Missing File", "Please assign a UI CSV File first!", "OK");
                return;
            }
            ImportUI(profile, profile.uiCsvFile.text);
        }

        EditorGUILayout.EndHorizontal();

        GUILayout.Space(20);

        // ========== GOOGLE SHEETS IMPORT SECTION ==========
        EditorGUILayout.LabelField("Import from Google Sheets", EditorStyles.boldLabel);
        GUILayout.Space(5);

        EditorGUILayout.BeginHorizontal();

        // --- BUTTON 4: QUESTIONS (Google Sheets) ---
        GUI.backgroundColor = new Color(0.4f, 0.7f, 0.4f); // Darker green
        if (GUILayout.Button("IMPORT QUESTIONS\n(Google Sheets)", GUILayout.Height(50)))
        {
            if (string.IsNullOrEmpty(profile.quizSheetUrl))
            {
                EditorUtility.DisplayDialog("Missing URL", "Please enter a Quiz Sheet URL first!", "OK");
                return;
            }
            FetchAndImportQuestions(profile);
        }

        // --- BUTTON 5: TIERS (Google Sheets) ---
        GUI.backgroundColor = new Color(0.4f, 0.7f, 0.7f); // Darker cyan
        if (GUILayout.Button("IMPORT TIERS\n(Google Sheets)", GUILayout.Height(50)))
        {
            if (string.IsNullOrEmpty(profile.tierSheetUrl))
            {
                EditorUtility.DisplayDialog("Missing URL", "Please enter a Tier Sheet URL first!", "OK");
                return;
            }
            FetchAndImportTiers(profile);
        }

        // --- BUTTON 6: UI & FLAGS (Google Sheets) ---
        GUI.backgroundColor = new Color(0.7f, 0.7f, 0.4f); // Darker yellow
        if (GUILayout.Button("IMPORT UI & FLAGS\n(Google Sheets)", GUILayout.Height(50)))
        {
            if (string.IsNullOrEmpty(profile.uiSheetUrl))
            {
                EditorUtility.DisplayDialog("Missing URL", "Please enter a UI Sheet URL first!", "OK");
                return;
            }
            FetchAndImportUI(profile);
        }

        EditorGUILayout.EndHorizontal();

        GUILayout.Space(20);

        // ========== VALIDATE SECTION ==========
        GUI.backgroundColor = new Color(0.9f, 0.6f, 0.9f); // Light magenta
        if (GUILayout.Button("VALIDATE PROFILE", GUILayout.Height(35)))
        {
            ValidateProfile(profile);
        }

        GUI.backgroundColor = Color.white;
    }

    // =========================================================================
    // GOOGLE SHEETS FETCH METHODS
    // =========================================================================

    private void FetchAndImportQuestions(QuizProfile profile)
    {
        string exportUrl = ConvertToExportUrl(profile.quizSheetUrl);
        if (exportUrl == null)
        {
            EditorUtility.DisplayDialog("Invalid URL", "The Quiz Sheet URL format is invalid.\n\nExpected format:\nhttps://docs.google.com/spreadsheets/d/SHEET_ID/edit?gid=TAB_GID", "OK");
            return;
        }

        FetchCsvFromGoogle(exportUrl, "Questions", (csv) =>
        {
            ImportQuestions(profile, csv);
            EditorUtility.DisplayDialog("Success", $"Imported {profile.questions.Count} questions from Google Sheets!", "OK");
        });
    }

    private void FetchAndImportTiers(QuizProfile profile)
    {
        string exportUrl = ConvertToExportUrl(profile.tierSheetUrl);
        if (exportUrl == null)
        {
            EditorUtility.DisplayDialog("Invalid URL", "The Tier Sheet URL format is invalid.\n\nExpected format:\nhttps://docs.google.com/spreadsheets/d/SHEET_ID/edit?gid=TAB_GID", "OK");
            return;
        }

        FetchCsvFromGoogle(exportUrl, "Tiers", (csv) =>
        {
            ImportTiers(profile, csv);
            EditorUtility.DisplayDialog("Success", $"Imported {profile.tiers.Count} tiers from Google Sheets!", "OK");
        });
    }

    private void FetchAndImportUI(QuizProfile profile)
    {
        string exportUrl = ConvertToExportUrl(profile.uiSheetUrl);
        if (exportUrl == null)
        {
            EditorUtility.DisplayDialog("Invalid URL", "The UI Sheet URL format is invalid.\n\nExpected format:\nhttps://docs.google.com/spreadsheets/d/SHEET_ID/edit?gid=TAB_GID", "OK");
            return;
        }

        FetchCsvFromGoogle(exportUrl, "UI & Flags", (csv) =>
        {
            ImportUI(profile, csv);
            EditorUtility.DisplayDialog("Success", "Imported UI data from Google Sheets!", "OK");
        });
    }

    /// <summary>
    /// Converts a Google Sheets edit URL to a CSV export URL.
    /// Input:  https://docs.google.com/spreadsheets/d/SHEET_ID/edit?gid=TAB_GID
    /// Output: https://docs.google.com/spreadsheets/d/SHEET_ID/export?format=csv&gid=TAB_GID
    /// </summary>
    private string ConvertToExportUrl(string editUrl)
    {
        if (string.IsNullOrEmpty(editUrl)) return null;

        // Pattern to match Google Sheets URL
        // Supports: /edit?gid=123, /edit#gid=123, /edit?gid=123&other=params
        var match = Regex.Match(editUrl, @"docs\.google\.com/spreadsheets/d/([^/]+).*[?#&]gid=(\d+)");

        if (!match.Success)
        {
            // Try without gid (default to 0)
            var matchNoGid = Regex.Match(editUrl, @"docs\.google\.com/spreadsheets/d/([^/]+)");
            if (matchNoGid.Success)
            {
                string sheetId = matchNoGid.Groups[1].Value;
                return $"https://docs.google.com/spreadsheets/d/{sheetId}/export?format=csv&gid=0";
            }
            return null;
        }

        string id = match.Groups[1].Value;
        string gid = match.Groups[2].Value;

        return $"https://docs.google.com/spreadsheets/d/{id}/export?format=csv&gid={gid}";
    }

    /// <summary>
    /// Fetches CSV data from a Google Sheets export URL.
    /// </summary>
    private void FetchCsvFromGoogle(string url, string dataType, System.Action<string> onSuccess)
    {
        EditorUtility.DisplayProgressBar($"Fetching {dataType}", "Connecting to Google Sheets...", 0.2f);

        var request = UnityWebRequest.Get(url);
        var operation = request.SendWebRequest();

        // Poll until complete (Editor doesn't support async well)
        while (!operation.isDone)
        {
            EditorUtility.DisplayProgressBar($"Fetching {dataType}", "Downloading data...", 0.5f);
            System.Threading.Thread.Sleep(50);
        }

        EditorUtility.ClearProgressBar();

        if (request.result == UnityWebRequest.Result.ConnectionError ||
            request.result == UnityWebRequest.Result.ProtocolError)
        {
            string errorMsg = request.error;

            if (request.responseCode == 404)
            {
                errorMsg = "Sheet not found. Check that the URL is correct.";
            }
            else if (request.responseCode == 403 || request.responseCode == 401)
            {
                errorMsg = "Access denied. Make sure the sheet is set to:\n'Anyone with the link can view'";
            }

            EditorUtility.DisplayDialog("Fetch Failed", $"Failed to fetch {dataType}:\n\n{errorMsg}", "OK");
            Debug.LogError($"❌ Google Sheets fetch failed: {request.error}");
            return;
        }

        string csv = request.downloadHandler.text;

        if (string.IsNullOrEmpty(csv))
        {
            EditorUtility.DisplayDialog("Empty Response", $"The {dataType} sheet appears to be empty.", "OK");
            return;
        }

        Debug.Log($"✅ Fetched {dataType} from Google Sheets ({csv.Length} characters)");
        onSuccess?.Invoke(csv);
    }

    // =========================================================================
    // VALIDATION
    // =========================================================================

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

    // =========================================================================
    // CSV PARSING HELPERS
    // =========================================================================

    private List<string> ParseCsvRows(string rawText)
    {
        // Handle different line endings
        rawText = rawText.Replace("\r\n", "\n").Replace("\r", "\n");

        List<string> rows = new List<string>();
        bool inQuotes = false;
        string currentRow = "";

        for (int i = 0; i < rawText.Length; i++)
        {
            char c = rawText[i];

            if (c == '"')
            {
                inQuotes = !inQuotes;
                currentRow += c;
            }
            else if (c == '\n' && !inQuotes)
            {
                if (!string.IsNullOrWhiteSpace(currentRow))
                    rows.Add(currentRow);
                currentRow = "";
            }
            else
            {
                currentRow += c;
            }
        }

        // Don't forget the last row
        if (!string.IsNullOrWhiteSpace(currentRow))
            rows.Add(currentRow);

        return rows;
    }

    // =========================================================================
    // IMPORT METHODS (Shared by CSV and Google Sheets)
    // =========================================================================

    private void ImportQuestions(QuizProfile profile, string csvText)
    {
        List<string> lines = ParseCsvRows(csvText);

        if (profile.questions == null) profile.questions = new List<QuestionData>();
        profile.questions.Clear();

        Debug.Log($"📝 Starting Question Import for {profile.name}...");

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
        AssetDatabase.SaveAssets();

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

    private void ImportTiers(QuizProfile profile, string csvText)
    {
        List<string> lines = ParseCsvRows(csvText);

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
        AssetDatabase.SaveAssets();

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

    private void ImportUI(QuizProfile profile, string csvText)
    {
        List<string> lines = ParseCsvRows(csvText);

        if (profile.uiData == null) profile.uiData = new UIData();

        Debug.Log($"🎨 Starting UI Import for {profile.name}...");

        // Load Flag Image
        string flagPath = $"{profile.imageLibraryPath}Flag.png";
        profile.uiData.flagIcon = AssetDatabase.LoadAssetAtPath<Texture2D>(flagPath);
        if (profile.uiData.flagIcon == null)
            Debug.LogWarning($"⚠️ Missing Flag Image at: {flagPath}");

        // Track which keys were found
        HashSet<string> foundKeys = new HashSet<string>();
        HashSet<string> requiredKeys = new HashSet<string> { "btn_start", "btn_restart", "btn_lang", "title_template", "confirm_title", "btn_yes", "btn_no" };

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
                case "confirm_title":
                    profile.uiData.confirmTitle = new LocalizedText { en = en, native = nat };
                    break;
                case "btn_yes":
                    profile.uiData.btnYes = new LocalizedText { en = en, native = nat };
                    break;
                case "btn_no":
                    profile.uiData.btnNo = new LocalizedText { en = en, native = nat };
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
        AssetDatabase.SaveAssets();

        Debug.Log("✅ UI Data Imported!");
    }
}