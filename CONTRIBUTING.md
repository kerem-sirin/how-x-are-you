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
├── Images/
│   └── Flag.png          # Country flag (any size, will be scaled)
├── How X Are You - XX Quiz.csv
├── How X Are You - XX Tier.csv
└── How X Are You - XX UI.csv
```

#### 3. Use the Google Sheet Template

> **[📄 Google Sheet Template](https://docs.google.com/spreadsheets/d/1ct-ShL0a72dP0yXP25wbkmHo9trunVviVYG2tVhin0E/edit?usp=sharing)**

1. Open the template (contains Polish example data)
2. **File → Make a copy** to your own Google Drive
3. Rename tabs from `PL` to your country code (e.g., `JP Quiz`, `JP Tier`, `JP UI`)
4. Replace content with your country's data
5. Export each tab: **File → Download → Comma-separated values (.csv)**

#### 4. Questions CSV Format

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

#### 5. Prepare Tiers CSV

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

#### 6. Prepare UI CSV

```csv
Key,EN,Native
btn_start,Start Game,[Native translation]
btn_restart,Try Again,[Native translation]
btn_lang,English,[Native language name]
title_template,How {0} Are You?,[Native equivalent with {0} placeholder]
```

#### 7. Add Images

**Naming Convention:**
- Question images: `Q_00.png`, `Q_01.png`, etc.
- Answer images: `Q_00_00.png`, `Q_00_01.png`, etc.
- Flag: `Flag.png`

**Image Guidelines:**
- Format: PNG (transparency supported)
- Size: No strict requirement (scaled in UI)
- Content: Family-friendly, no copyrighted material

#### 8. Create ScriptableObject in Unity

1. Right-click → **Create → HowX → Country Profile**
2. Configure all fields
3. Assign CSV files
4. Click **IMPORT QUESTIONS**, **IMPORT TIERS**, **IMPORT UI**
5. Click **VALIDATE PROFILE** — fix any errors

#### 9. Test Thoroughly

- [ ] Play full game in English
- [ ] Play full game in Native language
- [ ] Toggle language mid-game
- [ ] Verify all images load
- [ ] Check all tier results display correctly
- [ ] Test on different screen sizes (if possible)

#### 10. Submit Pull Request

```bash
git checkout -b country/XX   # e.g., country/JP
git add Assets/Data/XX/
git commit -m "Add [Country Name] country profile"
git push origin country/XX
```

Then open a PR with:
- Country name and code
- Number of questions
- Any special notes
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

Open an issue with the `question` label or reach out to the maintainers.

Thank you for helping make "How X Are You?" more fun for everyone! 🎉