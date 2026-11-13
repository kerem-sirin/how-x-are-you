# How X Are You?

A data-driven, expandable quiz game framework built in Unity. Test players' knowledge about any country or topic with localized questions, tiered results, and a charming Boho Matte visual style.

![Unity](https://img.shields.io/badge/Unity-6.3_LTS-black?logo=unity)
![License](https://img.shields.io/badge/License-MIT-green)
![Platform](https://img.shields.io/badge/Platform-WebGL-blue)

<!-- Add your screenshots here -->
<p align="center">
  <img src="Screenshots/PL/menu.png" alt="Menu Screenshot" width="140">
  <img src="Screenshots/PL/QuizTabletPortrait.png" alt="Quiz Screenshot" width="200">
  <img src="Screenshots/PL/result.png" alt="Result Screenshot" width="140">
  <img src="Screenshots/PL/QuizMobileLandscape.png" alt="Quiz Screenshot" width="240">
</p>

---

## ✨ Features

- **Multi-Country Support** — Create unlimited country profiles with unique questions, tiers, and localization
- **Randomized Gameplay** — Questions and answers shuffle every playthrough
- **Bilingual** — Toggle between English and native language instantly, from the menu or mid-quiz via Settings
- **Tiered Results** — 8 result tiers with unique titles, subtitles, and descriptions
- **Responsive Design** — Supports both Portrait and Landscape orientations
- **Boho Matte Theme** — Warm, inviting visual style with custom assets
- **Google Sheets Integration** — Import data directly from Google Sheets URLs
- **CSV Support** — Alternative local CSV import for offline workflows
- **Dynamic Audio** — Randomized SFX pitch for satisfying feedback

---

## 🎮 Play Now

| Polish 🇵🇱 | Turkish 🇹🇷 |
|:----------:|:----------:|
| [How Polish Are You?](https://bluebuckgames.itch.io/how-polish-are-you) | [How Turkish Are You?](https://bluebuckgames.itch.io/how-turkish-are-you) |
---

## 🚀 Quick Start

### Requirements

- Unity **6.3 LTS** (project uses `6000.3.10f1`)
- UI Toolkit (included in Unity)

### Setup

1. **Clone the repository**
   ```bash
   git clone https://github.com/kerem-sirin/how-x-are-you
   ```

2. **Open in Unity**
   - Open Unity Hub
   - Click "Add" → Navigate to cloned folder
   - Open project with Unity 6.3 LTS

3. **Open the main scene**
   ```
   Assets/Scenes/MainGame.unity
   ```

4. **Press Play** : The Polish profile is pre-configured

5. **Switch country (optional)** : Select the `GameController` object in the scene and drag another profile (e.g. `Assets/Data/TR/Turkish Profile.asset`) into **Current Profile**

---

## 📁 Project Structure

```
Assets/
├── Audio/                       # Music & sound effects
│
├── Data/
│   ├── PL/                      # Country profile folder
│   │   ├── Poland Profile.asset # QuizProfile ScriptableObject
│   │   ├── How X Are You - PL Quiz.csv   # Optional local CSVs
│   │   ├── How X Are You - PL Tier.csv
│   │   ├── How X Are You - PL UI.csv
│   │   └── Images/              # Question & answer images
│   │       ├── Q_00.png         # Question image (ID: 00)
│   │       ├── Q_01_00.png      # Answer image (Q01, Answer 0)
│   │       └── Flag.png         # Country flag
│   └── TR/                      # Turkish profile (imported from Google Sheets)
│
├── Scripts/
│   ├── Core/
│   │   ├── GameController.cs    # Main game loop, UI & language toggle
│   │   ├── AudioController.cs   # Music & SFX management
│   │   ├── SettingsController.cs # Settings panel: mute & language toggles, return to menu
│   │   ├── GameEvents.cs        # Event system
│   │   ├── MediaQuery.cs        # Orientation detection
│   │   ├── MediaQueryEvents.cs  # Orientation events
│   │   └── ResponsiveUI.cs      # PanelSettings swapping
│   ├── Data/
│   │   ├── QuizProfile.cs       # ScriptableObject definition
│   │   ├── QuestionData.cs      # Question structure
│   │   ├── TierData.cs          # Result tier structure
│   │   ├── UIData.cs            # UI strings structure
│   │   └── LocalizedText.cs     # Bilingual text wrapper
│   └── Editor/
│       └── QuizProfileEditor.cs # CSV/Google Sheets import & validation
│
├── UI/
│   ├── Themes/
│   │   ├── BaseTheme.uss        # Main stylesheet
│   │   ├── BaseTheme-Portrait.uss   # Portrait overrides
│   │   ├── BaseTheme-Landscape.uss  # Landscape overrides
│   │   ├── RuntimeTheme-Portrait.tss   # Portrait theme
│   │   └── RuntimeTheme-Landscape.tss  # Landscape theme
│   ├── Layouts/
│   │   └── MainGame.uxml        # UI structure
│   ├── PanelSettings-Portrait.asset   # 1080x1920 reference resolution
│   ├── PanelSettings-Landscape.asset  # 1920x1080 reference resolution
│   └── Textures/                # UI assets (backgrounds, buttons, frames, icons)
│
└── Scenes/
    └── MainGame.unity           # Main scene
```

---

## 📝 Creating a New Country Profile

### Step 1: Create Folder Structure

```
Assets/Data/XX/                  # XX = country code (e.g., DE, JP)
└── Images/
    └── Flag.png
```

### Step 2: Create ScriptableObject

1. Right-click in Project window
2. **Create → HowX → Country Profile**
3. Name it (e.g., `Profile_Poland`)

### Step 3: Configure Profile

| Field | Example | Description |
|-------|---------|-------------|
| Folder Name | `PL` | Must match folder in Assets/Data/ |

### Step 4: Import Data

You can import data from **Google Sheets** (recommended) or **local CSV files**.

#### Option A: Google Sheets Import 🌐

1. Copy the [Google Sheet Template](https://docs.google.com/spreadsheets/d/1ct-ShL0a72dP0yXP25wbkmHo9trunVviVYG2tVhin0E/)
2. Fill in your country's data
3. Make the sheet public (Share → Anyone with link → Viewer)
4. Copy each tab's URL and paste into the profile
5. Click the **Import (Google Sheets)** buttons

#### Option B: CSV Import 📁

1. Export CSVs from Google Sheets
2. Assign CSV files to the profile
3. Click the **Import (CSV)** buttons

### Step 5: Validate

Click **VALIDATE PROFILE** and check the Console. It reports:

- **Errors:** duplicate question IDs, no questions, no tiers, missing English or native text (questions, answers, tiers, UI)
- **Warnings:** fewer than 50 questions, not 8 tiers, no 100% tier, missing question/answer images, missing flag

Fix every error before submitting. Validation can't judge the content itself, so still playtest both languages (see the [requirements checklist](CONTRIBUTING.md#requirements-checklist)).

---

## 📋 Google Sheet Template

> **[📄 View Template on Google Sheets](https://docs.google.com/spreadsheets/d/1ct-ShL0a72dP0yXP25wbkmHo9trunVviVYG2tVhin0E/edit?usp=sharing)**
> 
> This template contains 3 tabs: **Quiz**, **Tier**, and **UI**. You can:
> - 📝 **Comment** on the sheet to suggest improvements
> - 📋 **Make a copy** (File → Make a copy) to create your own country profile
> - 🌐 **Import directly** using tab URLs in Unity

---

## 🖼️ Image Naming Convention

| Type | Pattern | Example |
|------|---------|---------|
| Question Image | `Q_{ID}.png` | `Q_05.png` |
| Answer Image | `Q_{ID}_{AnswerIndex}.png` | `Q_05_00.png`, `Q_05_01.png` |
| Flag | `Flag.png` | `Flag.png` |

- IDs are **zero-padded** to 2 digits: `00`, `01`, `02`...
- Answer indices: `00`, `01`, `02`, `03`

---

## 🎨 Customizing the Theme

The visual style is defined in `Assets/UI/Themes/BaseTheme.uss`:

```css
:root {
    --theme-bg: rgb(231, 158, 133);       /* Terra Cotta */
    --card-bg: rgba(255, 248, 245, 0.95); /* Warm White */
    --theme-accent: rgb(56, 115, 115);    /* Deep Teal */
    --hover-bg: rgb(240, 213, 154);       /* Maize Yellow */
    --text-main: rgb(56, 115, 115);       /* Deep Teal */
    --text-body: rgb(166, 112, 93);       /* Muted Clay */
    --radius-lg: 24px;
    --radius-md: 16px;
    --overlay-bg: rgba(0, 0, 0, 0.5);
}
```

Custom assets are in `Assets/UI/Textures/`:
- `Backgrounds/bg_pattern.png` : Tileable background
- `Buttons/button_square_gradient.png` : 9-slice button/panel frame
- `Frames/frame_round_gradient.png` : 9-slice rounded frame
- `Icons/` : Settings, close, music and SFX on/off icons

---

## 📱 Responsive Design

The game supports both **Portrait** and **Landscape** orientations:

| Orientation | Layout |
|-------------|--------|
| Portrait | Question on top, answers on bottom |
| Landscape | Question on left, answers on right |

Orientation is detected automatically: a screen counts as landscape when width ÷ height ≥ 1.2. The UI adapts by swapping PanelSettings. Each orientation has its own PanelSettings and theme (`RuntimeTheme-*.tss`), which loads `BaseTheme.uss` plus the matching `BaseTheme-Portrait.uss` / `BaseTheme-Landscape.uss` overrides.

---

## 🏗️ Building for WebGL

1. Assign the country you want to build to **Current Profile** on the `GameController` object in `MainGame` (one build per country)
2. **File → Build Profiles**
3. Select **Web** platform
4. Click **Switch Platform**
5. **Player Settings:**
   - Resolution: **960 x 540**
   - Compression Format: **Brotli** (for itch.io)
   - Memory Size: **256 MB** (adjust if needed)
   - Enable **Decompression Fallback**
6. Click **Build**

### Deploying to itch.io

1. Create a new project on itch.io
2. Set **Kind of project** to **HTML**
3. Upload your WebGL build as a `.zip`
4. Check **This file will be played in the browser**
5. Set viewport dimensions: **960 x 540**
6. Enable **Mobile friendly** and **Fullscreen button**

---

## 🤝 Contributing

Contributions are welcome! See [CONTRIBUTING.md](CONTRIBUTING.md) for detailed guidelines.

### Quick Start for Contributors

1. Fork the repository
2. Create your country folder in `Assets/Data/`
3. Use the [Google Sheet Template](https://docs.google.com/spreadsheets/d/1ct-ShL0a72dP0yXP25wbkmHo9trunVviVYG2tVhin0E/)
4. Import and test with **VALIDATE PROFILE**
5. Submit a Pull Request

### Guidelines

- Follow existing code style
- Test on both Portrait and Landscape orientations
- Ensure all text has both EN and Native translations
- Keep questions family-friendly and culturally respectful

---

## 📄 License

This project is licensed under the **MIT License** : See the [LICENSE](LICENSE) file for details.

---

## 🙏 Acknowledgments

- Built with [Unity UI Toolkit](https://docs.unity3d.com/Manual/UIElements.html)
- Inspired by BuzzFeed-style personality quizzes
- Boho Matte color palette

---

## 🎵 Music Credits

- Music by [Kevin MacLeod](https://incompetech.com)  
- Licensed under [Creative Commons: By Attribution 4.0](http://creativecommons.org/licenses/by/4.0/)

| Track | Usage |
|-------|-------|
| "Cloud Dancer" | Menu Music |
| "Journey To Ascend" | Quiz Music |
| "Sergio's Magic Dustbin" | Result Music |

---

## 🔊 SFX Credits

- Sound effects by [Universfield](https://pixabay.com/users/universfield-28281460/) on Pixabay
- Licensed under the [Pixabay Content License](https://pixabay.com/service/license-summary/)

| SFX | Usage |
|-------|-------|
| New Notification 07 | Correct Answer |
| Error 04 | Wrong Answer |

---

## 📬 Contact

- **itch.io:** [bluebuckgames](https://bluebuckgames.itch.io)
- **GitHub:** [@kerem-sirin](https://github.com/kerem-sirin)

---

<p align="center">
  Made with ❤️ and way too many pierogi references
</p>
