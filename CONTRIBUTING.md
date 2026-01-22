# Contributing to "How X Are You?"

Thank you for your interest in contributing! This guide will help you add new country profiles or improve the codebase.

---

## 🌍 Adding a New Country Profile

This is the most impactful way to contribute! Each new country expands the game for players worldwide.

### Requirements Checklist

Before submitting, ensure you have:

- [ ] **50 questions minimum** (game selects 20 randomly per session)
- [ ] **8 result tiers** covering 0-100% score range
- [ ] **Full bilingual support** (English + native language)
- [ ] **All referenced images** included
- [ ] **Passed validation** (no errors in Unity console)
- [ ] **Playtested** both languages

### Step-by-Step Guide

#### 1. Fork & Clone

```bash
git fork https://github.com/ORIGINAL_OWNER/how-x-are-you
git clone https://github.com/YOUR_USERNAME/how-x-are-you
```

#### 2. Create Country Folder

```
Assets/Data/XX/           # Use ISO 3166-1 alpha-2 code (JP, DE, BR, etc.)
└── Images/
    └── Flag.png          # Country flag (any size, will be scaled)
```

#### 3. Use the Google Sheet Template

> **[📄 Google Sheet Template](https://docs.google.com/spreadsheets/d/1ct-ShL0a72dP0yXP25wbkmHo9trunVviVYG2tVhin0E/edit?usp=sharing)**

1. Open the template (contains Polish example data)
2. **File → Make a copy** to your own Google Drive
3. Rename your copy to match your country (e.g., "How X Are You - Japan")
4. Replace content with your country's data in all 3 tabs: **Quiz**, **Tier**, **UI**

---

## 📊 Importing Data: Two Methods

You can import data either via **Google Sheets URLs** (recommended) or **local CSV files**.

### Method 1: Google Sheets Import (Recommended) 🌐

This method imports directly from your Google Sheet — no need to download CSV files!

#### Step 1: Make Your Sheet Public

1. Open your Google Sheet
2. Click **Share** (top-right)
3. Change access to **"Anyone with the link"** → **Viewer**
4. Click **Done**

#### Step 2: Get Tab URLs with GIDs

Each tab in Google Sheets has a unique **GID** (tab identifier). You need the URL for each tab.

**How to find the GID:**

1. Click on the tab you want (Quiz, Tier, or UI)
2. Look at the URL in your browser:
   ```
   https://docs.google.com/spreadsheets/d/1abc123xyz/edit#gid=0
                                                           ↑↑↑
                                                        This is the GID
   ```
3. Copy the **full URL** including the `gid=` parameter

**Example URLs:**
| Tab | Example URL |
|-----|-------------|
| Quiz | `https://docs.google.com/spreadsheets/d/1abc.../edit#gid=0` |
| Tier | `https://docs.google.com/spreadsheets/d/1abc.../edit#gid=123456789` |
| UI | `https://docs.google.com/spreadsheets/d/1abc.../edit#gid=987654321` |

> 💡 **Tip:** The first tab usually has `gid=0`. Other tabs have longer numeric GIDs.

#### Step 3: Import in Unity

1. Open your **QuizProfile** ScriptableObject
2. Paste the URLs into the **Google Sheets URLs** section:
   - Quiz Sheet URL
   - Tier Sheet URL
   - UI Sheet URL
3. Click the corresponding **Import (Google Sheets)** buttons:
   - **IMPORT QUESTIONS (Google Sheets)** — Green button
   - **IMPORT TIERS (Google Sheets)** — Cyan button
   - **IMPORT UI & FLAGS (Google Sheets)** — Yellow button
4. Click **VALIDATE PROFILE** to check for errors

---

### Method 2: Local CSV Import 📁

If you prefer working with local files:

1. Export each tab from Google Sheets: **File → Download → Comma-separated values (.csv)**
2. Save CSV files to your country folder:
   ```
   Assets/Data/XX/
   ├── How X Are You - XX Quiz.csv
   ├── How X Are You - XX Tier.csv
   └── How X Are You - XX UI.csv
   ```
3. In Unity, assign CSV files to the **Import from CSV** section
4. Click the corresponding **Import (CSV)** buttons

---

## 📋 Data Format Reference

### Questions (Quiz Tab)

| Column | Required | Notes |
|--------|----------|-------|
| ID | ✅ | Sequential integers starting from 0 |
| CorrectIndex | ✅ | `0`, `1`, `2`, `3`, or comma-separated like `0,1` |
| Q_EN | ✅ | English question |
| Q_Native | ✅ | Native language question |
| Use Question Image | ✅ | `TRUE` or `FALSE` |
| Use Answer Image | ✅ | `TRUE` or `FALSE` |
| A0-A3_EN | ✅ | English answers |
| A0-A3_Native | ✅ | Native answers |

**Tips for Great Questions:**

- Mix difficulty levels (easy, medium, hard)
- Include cultural references locals would know
- Add humor where appropriate
- Avoid controversial political topics
- Use images for visual variety

### Tiers (Tier Tab)

Create 8 tiers with increasing `MaxPercent` values:

| MaxPercent | Suggested Theme |
|------------|-----------------|
| 12-15% | Outsider / Tourist |
| 20-25% | Beginner |
| 35-40% | Learning |
| 50-60% | Average Citizen |
| 70-75% | Enthusiast |
| 80-85% | Expert |
| 90-95% | Nearly Perfect |
| 100% | Ultimate / Transcended |

**Tips:**
- Make tier titles humorous and culturally relevant
- Subtitles should be short catchphrases
- Descriptions can be longer and playful

### UI Strings (UI Tab)

| Key | EN | Native |
|-----|-----|--------|
| btn_start | Start Game | [Native translation] |
| btn_restart | Try Again | [Native translation] |
| btn_lang | English | [Native language name] |
| title_template | How Polish Are You? | [Full title in native language] |

> 💡 **Note:** The `title_template` should be the complete title, not a template with placeholders.

---

## 🖼️ Image Guidelines

### Naming Convention

| Type | Pattern | Example |
|------|---------|---------|
| Question Image | `Q_{ID}.png` | `Q_05.png` |
| Answer Image | `Q_{ID}_{AnswerIndex}.png` | `Q_05_00.png`, `Q_05_01.png` |
| Flag | `Flag.png` | `Flag.png` |

- IDs are **zero-padded** to 2 digits: `00`, `01`, `02`...
- Answer indices: `00`, `01`, `02`, `03`

### Image Requirements

- **Format:** PNG (transparency supported)
- **Size:** No strict requirement (scaled in UI)
- **Content:** Family-friendly, no copyrighted material

---

## 🎮 Create ScriptableObject in Unity

1. Right-click in Project window → **Create → HowX → Country Profile**
2. Name it (e.g., `Profile_Japan`)
3. Configure the profile:

| Field | Example | Description |
|-------|---------|-------------|
| Folder Name | `JP` | Must match folder in `Assets/Data/` |

4. Add your Google Sheets URLs or CSV files
5. Click the **Import** buttons
6. Click **VALIDATE PROFILE** — fix any errors

---

## ✅ Testing Checklist

Before submitting, verify:

- [ ] Play full game in English
- [ ] Play full game in Native language
- [ ] Toggle language mid-game
- [ ] Verify all images load
- [ ] Check all tier results display correctly
- [ ] Test in **Portrait** orientation
- [ ] Test in **Landscape** orientation
- [ ] Run **VALIDATE PROFILE** with no errors

---

## 📤 Submit Pull Request

```bash
git checkout -b country/XX   # e.g., country/JP
git add Assets/Data/XX/
git commit -m "Add [Country Name] country profile"
git push origin country/XX
```

Then open a PR with:
- Country name and code
- Number of questions
- Link to your Google Sheet (for review)
- Screenshot of result screen

---

## 🐛 Bug Reports

Found a bug? Open an issue with:

1. **Description** — What happened?
2. **Steps to Reproduce** — How can we trigger it?
3. **Expected Behavior** — What should happen?
4. **Screenshots** — If applicable
5. **Platform** — Browser, OS, device

---

## 💻 Code Contributions

### Areas for Improvement

- Additional animations/transitions
- Accessibility features
- Performance optimizations
- New question types (multiple select, etc.)

### Code Style Guidelines

- Use meaningful variable names
- Comment complex logic
- Follow existing namespace structure (`HowX.Core`, `HowX.Data`)
- Store Action delegates as fields for event subscriptions (avoid memory leaks)
- Use UI Toolkit exclusively (no UGUI)
- Test thoroughly before submitting

### Pull Request Process

1. Fork the repository
2. Create a feature branch (`feature/your-feature`)
3. Make changes with clear commits
4. Test in Unity Editor
5. Submit PR with description of changes

---

## 📜 Content Guidelines

### Do ✅

- Celebrate cultural uniqueness
- Use self-deprecating national humor
- Include well-known cultural references
- Make questions educational AND fun

### Don't ❌

- Use copyrighted images without permission
- Include offensive stereotypes
- Reference controversial political events
- Add inappropriate or adult content

---

## 🙋 Questions?

Open an issue with the `question` label or start a Discussion.

Thank you for helping make "How X Are You?" more fun for everyone! 🎉
