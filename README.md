# VR Detective: Case Files

> A first-person 3D mystery investigation game built in Unity 6 (URP)

![Unity](https://img.shields.io/badge/Unity-6-000000?logo=unity)
![C#](https://img.shields.io/badge/C%23-10.0-239120?logo=csharp)
![URP](https://img.shields.io/badge/Pipeline-URP-blue)
![Platform](https://img.shields.io/badge/Platform-Desktop%20PC-green)

---

## Table of Contents

- [Overview](#overview)
- [Storyline — The 11:47 Mystery](#storyline--the-1147-mystery)
- [Game Flow](#game-flow)
- [Scenes](#scenes)
  - [Scene 1: Main Menu (00_MainMenu)](#scene-1-main-menu-00_mainmenu)
  - [Scene 2: Case Room (01_CaseRoom)](#scene-2-case-room-01_caseroom)
  - [Scene 3: Research Archive (02_ResearchArchive)](#scene-3-research-archive-02_researcharchive)
  - [Scene 4: Detective Board (03_DetectiveBoard)](#scene-4-detective-board-03_detectiveboard)
  - [Scene 5: Case Conclusion (04_CaseConclusion)](#scene-5-case-conclusion-04_caseconclusion)
- [Controls](#controls)
- [Technical Architecture](#technical-architecture)
  - [Rendering Pipeline](#rendering-pipeline)
  - [Singleton Architecture](#singleton-architecture)
  - [Scene Management & Transitions](#scene-management--transitions)
  - [Player Controller Stack](#player-controller-stack)
  - [Evidence System Architecture](#evidence-system-architecture)
  - [Audio System](#audio-system)
  - [UI Framework](#ui-framework)
- [C# Script Reference](#c-script-reference)
- [Asset Inventory](#asset-inventory)
  - [Third-Party Asset Packs](#third-party-asset-packs)
  - [Custom Materials](#custom-materials)
  - [Audio Assets](#audio-assets)
  - [UI Assets](#ui-assets)
- [Project Directory Structure](#project-directory-structure)
- [Build Settings](#build-settings)
- [How to Build](#how-to-build)
- [Credits](#credits)

---

## Overview

**VR Detective: Case Files** is a first-person 3D detective mystery game where the player investigates the mysterious disappearance of Dr. Vale from his office. Despite the "VR" in its name, the game is designed as a **desktop PC experience** using standard keyboard-and-mouse controls to create an immersive investigation.

The player explores two 3D environments — a crime-scene office and a research archive — collecting evidence by examining interactable objects. All discovered clues are pinned to a procedurally generated Detective Board, and the game concludes with a multi-question deduction quiz that tests whether the player has correctly interpreted the evidence.

| Detail | Value |
|--------|-------|
| **Genre** | Mystery, Detective, Investigation, First-Person Exploration |
| **Engine** | Unity 6 |
| **Rendering** | Universal Render Pipeline (URP) |
| **Language** | C# (.NET Standard 2.1) |
| **Platform** | Desktop PC (Windows) |
| **Project Type** | College game development project (IIIT Sri City — Semester 7, VAR) |

---

## Storyline — The 11:47 Mystery

Dr. Vale has vanished from his office under unusual circumstances. When the detective arrives, several details suggest something was carefully orchestrated:

- A **wall clock** has stopped at exactly **11:47**
- A **cup of coffee** sits half-finished on the desk
- A **computer** may hold logs of Vale's final activities
- **Research documents** hint at what Vale was studying before the incident
- A **hidden USB drive** — the critical breakthrough — contains a farewell letter, offshore bank records (\$2.4M transferred to Account #4471-B in the Cayman Islands), and a passport scan under a new identity: **R. Calloway**

The adjacent Research Archive holds three corroborating records:

- A **research log** documenting Vale's recent work and sudden gaps
- An **access record** showing Vale used the service lift at 11:42 PM — five minutes before the clock stopped
- A **financial ledger** with transaction entries that match the USB's offshore account

The investigation ultimately reveals that **Dr. Vale staged his own disappearance** — methodically preparing funds, a new identity, and a cover story before walking out through the service exit.

---

## Game Flow

```
┌──────────────────────────────────────────────────────────────────┐
│                        GAME FLOW DIAGRAM                        │
│                                                                  │
│   00_MainMenu                                                    │
│       │ [Play]                                                   │
│       ▼                                                          │
│   01_CaseRoom  ─────────────────────── Explore Dr. Vale's office │
│       │ Examine: Clock, Coffee,        5 interactable clues      │
│       │ Computer, Book, USB Drive      + Detective Journal       │
│       │ [F key when 5/5 found]                                   │
│       ▼                                                          │
│   02_ResearchArchive ──────────────── Explore the archive room   │
│       │ Examine: Research Log,         3 archive records         │
│       │ Access Record, Financial       First-person 3D scene     │
│       │ Ledger                                                   │
│       │ [F key when 3/3 found]                                   │
│       ▼                                                          │
│   03_DetectiveBoard ───────────────── Review all evidence        │
│       │ Procedural cork board          Clickable evidence cards   │
│       │ Red thread connections         Detail overlay modal      │
│       │ [Continue to Deduction]                                  │
│       ▼                                                          │
│   04_CaseConclusion ───────────────── 4-question deduction quiz  │
│       │ Evidence-based questions       Score + rating system     │
│       │ Correct/incorrect feedback     Final case report         │
│       │ [Return to Main Menu]                                    │
│       ▼                                                          │
│   00_MainMenu ─────────────────────── Cycle complete             │
└──────────────────────────────────────────────────────────────────┘
```

---

## Scenes

### Scene 1: Main Menu (`00_MainMenu`)

The entry point of the game. Establishes the noir detective atmosphere with a dark background and gold-accented typography.

| Component | Description |
|-----------|-------------|
| **Background** | Dark detective-office themed image (`Noir Detective Office Investigation Board.png`) |
| **Title** | "VR DETECTIVE: CASE FILES" rendered in gold TextMeshPro |
| **Play Button** | Triggers `MainMenuController.PlayGame()` → loads `01_CaseRoom` via `SceneFader` |
| **How to Play Button** | Toggles an instructions overlay panel |
| **Quit Button** | Calls `Application.Quit()` (functional in builds only) |
| **Persistent Singletons** | `SceneFader`, `GameStateManager`, `AudioManager` instantiated here |

**Key Script:** `MainMenuController.cs`

---

### Scene 2: Case Room (`01_CaseRoom`)

The primary investigation environment — Dr. Vale's office rendered as a fully explorable first-person 3D scene.

| System | Implementation |
|--------|----------------|
| **Environment** | Office furniture, desk, chair, monitor, bookshelf built from `nappin/OfficeEssentialsPack` prefabs |
| **Player Controller** | `CharacterController` + `PlayerMovement.cs` (WASD, sprint, crouch, jump) + `MouseLook.cs` (mouse-driven camera) |
| **Interaction System** | `PlayerInteraction.cs` raycasts from the player camera; `ObjectHighlight.cs` tints interactable objects; `InteractionCursorUI.cs` changes cursor color |
| **Evidence Objects** | 5 interactable GameObjects, each with `ObjectHighlight` + `BoxCollider`. Named to match `GetClueDescription()` string matching: `*clock*`, `*coffee*`/`*mug*`, `*monitor*`/`*computer*`, `*book*`, `*usb*`/`*drive*` |
| **USB Drive** | Custom 3-material model: `USB_Anodized_Teal`, `USB_Brushed_Metal`, `USB_Status_Glow` |
| **Examination UI** | `ExaminationUI.cs` displays a timed panel (auto-hides after 5s) with the clue description |
| **Detective Journal** | `DetectiveJournal.cs` — dictionary-backed evidence log, toggled with Tab, shows progress counter |
| **HUD Badge** | `HUDBadge.cs` — top-right evidence counter "🔍 X / 5" with pulse animation on update |
| **Objective Tracker** | Dynamic `TMP_Text` driven by `PlayerInteraction.UpdateObjective()` — 4 distinct states based on evidence progress |
| **Scene Transition** | When all 5 clues collected (including USB), `[F]` key fades to `02_ResearchArchive` |
| **Audio** | `ambient_office` loop plays via `AudioManager`, `sfx_clue_found` one-shot on each discovery, `sfx_usb_found` for the USB, footstep system via `FootstepAudio.cs` |

**Key Scripts:** `PlayerInteraction.cs`, `PlayerMovement.cs`, `MouseLook.cs`, `DetectiveJournal.cs`, `ExaminationUI.cs`, `ObjectHighlight.cs`, `HUDBadge.cs`, `FootstepAudio.cs`

---

### Scene 3: Research Archive (`02_ResearchArchive`)

A second explorable 3D environment — a corporate archive room with filing cabinets and terminals. Uses the same first-person controller stack as the Case Room.

| System | Implementation |
|--------|----------------|
| **Environment** | Archive room built with custom materials: `Archive_Walnut` (wood panels), `Archive_DeepBlue` (walls), `Archive_Floor`, `Archive_Brass` (accents), `Archive_Paper` (documents), `Archive_Terminal` (screens) |
| **Clue System** | `ArchiveClue.cs` component (data-only MonoBehaviour with `clueId`, `clueTitle`, `description` fields) + `ObjectHighlight.cs` for visual feedback |
| **Controller** | `ArchiveSceneController.cs` — dedicated interaction controller with its own raycast loop, clue panel, objective tracker, and `HashSet<string>` duplicate guard |
| **3 Records** | Research Log, Access Record, Financial Ledger — each registered via `GameStateManager.RegisterCollectedEvidence()` with source/category metadata |
| **Scene Transition** | `[F]` key when 3/3 records found → fades to `03_DetectiveBoard` |
| **Audio** | `ambient_archive` loop, `sfx_clue_found` on first examination, `sfx_page_turn` on re-examination |

**Key Scripts:** `ArchiveSceneController.cs`, `ArchiveClue.cs`

---

### Scene 4: Detective Board (`03_DetectiveBoard`)

A procedurally generated evidence wall that visualises all collected clues in a classic "detective cork board" style — entirely built at runtime via code (no scene-authored UI).

| System | Implementation |
|--------|----------------|
| **Board Generation** | `DetectiveBoardController.cs` creates the entire UI hierarchy in `Start()`: Canvas → background → walnut frame → cork board → evidence cards → red threads → buttons |
| **Evidence Cards** | One card per `EvidenceRecord` from `GameStateManager.EvidenceRecords`. Each card includes: procedurally generated pixel-art icon (`MakeEvidenceIcon()`), title, source label, description, red push-pin, shadow, and a click-to-inspect `Button` |
| **Red Thread Links** | Connecting lines drawn between corroborating evidence pairs (e.g., `case_stopped_clock` ↔ `archive_access_record`). Implemented as rotated `Image` RectTransforms |
| **Detail Overlay** | Modal popup on card click showing full evidence text. Closed via `[Escape]` or close button |
| **Procedural Icons** | `MakeEvidenceIcon()` generates 96×96 pixel-art sprites using Bresenham line, circle, and rectangle primitives drawn directly to `Texture2D` pixel buffers |
| **Layout Algorithm** | Cards positioned in a 4-column, 2-row grid. Sorted by narrative priority via `EvidenceRank()` |
| **Navigation** | "MAIN MENU" button (resets state) and "REVIEW DEDUCTION →" button (loads `04_CaseConclusion`) |

**Key Script:** `DetectiveBoardController.cs` (425 lines, self-contained)

---

### Scene 5: Case Conclusion (`04_CaseConclusion`)

A four-question deduction quiz where the player's understanding of the evidence is tested. Features dynamic visual polish applied at runtime.

| System | Implementation |
|--------|----------------|
| **Quiz Engine** | `CaseConclusionController.cs` — 4 questions with 3 choices each. Correct answers pre-defined in `correctChoices[]`. Each submission reveals a detailed evidence-based explanation |
| **Visual Polish** | `ApplyVisualPolish()` overrides scene UI colors at runtime to enforce a consistent dossier palette (navy ink, gold accents, paper-white text) |
| **Button Feedback** | Selected answer: gold highlight + 1.05× scale. Submit button disabled until selection. All buttons locked after submission |
| **Scoring System** | Cumulative score across 4 questions. Ratings: LEAD DETECTIVE (4/4), CASE ANALYST (3/4), FIELD INVESTIGATOR (2/4), CASE REVIEW REQUIRED (0-1/4). Stored via `GameStateManager.SetDeductionResult()` |
| **Final Report** | After all 4 questions, displays final score, rating, and a narrative summary of the complete case |
| **Audio Feedback** | `sfx_clue_found` on correct answer, `sting_unresolved` on incorrect, `sting_case_solved` on perfect score |
| **Navigation** | "Return to Main Menu" calls `GameStateManager.ResetState()` then fades to `00_MainMenu` |

**Key Script:** `CaseConclusionController.cs` (443 lines)

---

## Controls

| Key | Action | Available In |
|-----|--------|-------------|
| `W A S D` | Move (forward, left, back, right) | CaseRoom, Archive |
| `Mouse` | Look around (first-person camera) | CaseRoom, Archive |
| `Left Shift` | Sprint (1.7× speed multiplier) | CaseRoom, Archive |
| `Left Ctrl` | Crouch (0.5× speed, reduced height) | CaseRoom, Archive |
| `Space` | Jump | CaseRoom, Archive |
| `E` | Examine interactable object | CaseRoom, Archive |
| `Tab` | Toggle Detective Journal | CaseRoom |
| `Escape` | Close open UI panel | CaseRoom, Archive, Board |
| `F` | File case report / proceed to next scene | CaseRoom, Archive |
| `Mouse Click` | Select answer / inspect evidence card | Board, Conclusion |

---

## Technical Architecture

### Rendering Pipeline

The project uses **Universal Render Pipeline (URP)** configured via:

| Asset | Purpose |
|-------|---------|
| `PC_RPAsset.asset` | URP Pipeline Asset — defines rendering quality, shadow settings, and post-processing for desktop |
| `PC_Renderer.asset` | Forward Renderer — handles the rendering pass order and renderer features |
| `Mobile_RPAsset.asset` / `Mobile_Renderer.asset` | Fallback mobile-quality settings (not actively used) |
| `DefaultVolumeProfile.asset` | Global Volume profile for post-processing (bloom, color grading, ambient occlusion) |
| `SampleSceneProfile.asset` | Per-scene Volume override profile |

### Singleton Architecture

Three persistent `DontDestroyOnLoad` singletons survive all scene transitions:

```
┌─────────────────────┐    ┌──────────────────┐    ┌──────────────────┐
│   GameStateManager  │    │    SceneFader     │    │   AudioManager   │
│─────────────────────│    │──────────────────│    │──────────────────│
│ EvidenceCollected   │    │ fadeCanvasGroup   │    │ sounds[]         │
│ EvidenceRecords[]   │    │ fadeDuration      │    │ audioSources{}   │
│ USBFound            │    │                  │    │                  │
│ DeductionScore      │    │ FadeToScene()    │    │ Play()           │
│ AllEvidenceFound    │    │ FadeAlpha()      │    │ Stop()           │
│ OnEvidenceUpdated   │    │                  │    │ PlayOneShot()    │
│ RegisterEvidence()  │    └──────────────────┘    │ StopAll()        │
│ RegisterUSB()       │                            └──────────────────┘
│ ResetState()        │
└─────────────────────┘
```

- **`GameStateManager`** — Central state store. Tracks evidence via `HashSet<string>` (duplicate-proof) and `List<EvidenceRecord>` (ordered for the board). Fires `OnEvidenceUpdated` C# event for reactive UI updates.
- **`SceneFader`** — Full-screen `CanvasGroup` (sort order 999) that lerps alpha 0→1→load→1→0 using coroutines. Blocks raycasts during transitions.
- **`AudioManager`** — Builds one `AudioSource` per `SoundEntry` at runtime as child GameObjects. Dictionary-based lookup. Loop-guard prevents restarting active ambient tracks.

### Scene Management & Transitions

All scene loads route through `SceneFader.Instance.FadeToScene(sceneName)` with a direct `SceneManager.LoadScene()` fallback. Scene names are string-based constants matching the Build Settings list.

### Player Controller Stack

The first-person controller is composed of three independent components on the Player GameObject:

| Component | Script | Responsibility |
|-----------|--------|---------------|
| `CharacterController` | Unity built-in | Capsule-based collision and `Move()` API |
| `PlayerMovement` | `PlayerMovement.cs` | WASD input → `CharacterController.Move()`, sprint/crouch height lerp, jump via `sqrt(jumpHeight * -2 * gravity)` |
| `MouseLook` | `MouseLook.cs` | `Input.GetAxis("Mouse X/Y")` → local X rotation (clamped ±80°) + player body Y rotation. Locks cursor on Start |

### Evidence System Architecture

```
   Player examines object
         │
         ▼
   PlayerInteraction.TryInteract()
         │ Raycast → ObjectHighlight match
         │ GetClueDescription() → narrative text
         │
    ┌────┴────┐
    │         │
    ▼         ▼
 Journal    GameStateManager
 .AddEvidence()  .RegisterCollectedEvidence()
 (UI storage)    (global tracking + EvidenceRecord)
    │                    │
    ▼                    ▼
 UpdateEvidenceList()  OnEvidenceUpdated event
 (StringBuilder)           │
                      ┌────┴────┐
                      ▼         ▼
                  HUDBadge   Objective
                  (pulse)    text update
```

The `EvidenceRecord` class stores structured evidence metadata:
```csharp
[Serializable]
public class EvidenceRecord {
    public string evidenceId;    // Unique key (e.g., "case_stopped_clock")
    public string title;         // Display name
    public string description;   // Full narrative text
    public string source;        // "Case Room" or "Research Archive"
    public string category;      // "clock", "coffee", "usb", "research", etc.
}
```

### Audio System

The `AudioManager` uses a `SoundEntry[]` array configured in the Inspector. At runtime, it builds a `Dictionary<string, AudioSource>` keyed by sound name, with each `AudioSource` on a dedicated child GameObject.

| Sound Name | Type | Trigger |
|------------|------|---------|
| `ambient_office` | Loop | Auto-plays in `01_CaseRoom` |
| `ambient_archive` | Loop | Plays in `02_ResearchArchive` |
| `suspense_bgm` | Loop | Background music |
| `sfx_clue_found` | One-shot | First examination of any clue |
| `sfx_usb_found` | One-shot | USB drive discovery |
| `sfx_page_turn` | One-shot | Journal toggle, re-examination |
| `sfx_ui_click` | One-shot | Button interactions |
| `sting_case_solved` | One-shot | Perfect deduction score |
| `sting_unresolved` | One-shot | Incorrect deduction answer |
| `sfx_footstep_walk_a/b/c` | One-shot | Walking footsteps (random selection) |
| `sfx_footstep_sprint_a/b` | One-shot | Sprint footsteps (random selection) |

`FootstepAudio.cs` plays random footstep clips at a configurable `stepInterval` when the `CharacterController` is grounded and moving (input magnitude > 0.1).

### UI Framework

All UI is built with **Unity UI (uGUI)** and **TextMeshPro (TMP)**:

- **Screen Space Overlay** canvases for HUD, menus, and panels
- **`CanvasScaler.ScaleWithScreenSize`** at 1920×1080 reference resolution for responsive layout
- **`GraphicRaycaster`** on all interactive canvases
- The Detective Board (`DetectiveBoardController`) generates its entire UI hierarchy procedurally at runtime — no pre-authored canvas objects

---

## C# Script Reference

### Core Game Systems

| Script | Lines | Purpose |
|--------|-------|---------|
| [`GameStateManager.cs`](Assets/Scripts/GameStateManager.cs) | 183 | Persistent singleton. Tracks evidence count, USB flag, evidence records list, deduction score. Fires `OnEvidenceUpdated` event. `DontDestroyOnLoad`. |
| [`SceneFader.cs`](Assets/Scripts/SceneFader.cs) | 110 | Persistent singleton. Black fade-out/fade-in scene transitions via `CanvasGroup` alpha lerp. Blocks raycasts during fade. |
| [`AudioManager.cs`](Assets/Scripts/AudioManager.cs) | 200 | Persistent singleton. Builds `AudioSource` per `SoundEntry` at runtime. Dictionary lookup. Loop-guard for ambient tracks. |

### Player Controller

| Script | Lines | Purpose |
|--------|-------|---------|
| [`PlayerMovement.cs`](Assets/Scripts/PlayerMovement.cs) | 56 | WASD movement via `CharacterController.Move()`. Sprint (Shift, 1.7×), crouch (Ctrl, 0.5×, height lerp), jump (Space). Gravity simulation. |
| [`MouseLook.cs`](Assets/Scripts/MouseLook.cs) | 35 | Mouse-driven first-person camera. Vertical rotation clamped ±80°. Locks and hides cursor on Start. |
| [`FootstepAudio.cs`](Assets/Scripts/FootstepAudio.cs) | 82 | Plays random footstep clips at configurable interval when grounded and moving. Requires `CharacterController`. |

### Investigation System

| Script | Lines | Purpose |
|--------|-------|---------|
| [`PlayerInteraction.cs`](Assets/Scripts/PlayerInteraction.cs) | 383 | Central interaction handler. Raycasts from camera, matches object names to clue descriptions, registers evidence with `GameStateManager`, manages USB detection, drives objective text, handles `[F]` key scene transition. |
| [`ObjectHighlight.cs`](Assets/Scripts/ObjectHighlight.cs) | 44 | Tints object renderers to `highlightColor` when aimed at. Stores and restores original material colors. |
| [`InteractionCursorUI.cs`](Assets/Scripts/InteractionCursorUI.cs) | 20 | Changes cursor `Image.color` between `normalColor` and `interactColor` based on whether a clue is in range. |
| [`ExaminationUI.cs`](Assets/Scripts/ExaminationUI.cs) | 65 | Shows a timed info panel with clue description. Auto-hides after `displayDuration` (default 5s). Exposes `IsPanelOpen` property and `HidePanel()`. |
| [`DetectiveJournal.cs`](Assets/Scripts/DetectiveJournal.cs) | 175 | `Dictionary<string, string>` evidence log. `StringBuilder`-based UI rendering. Toggle/close via Tab/Escape. Fires `OnEvidenceAdded` event. `IsJournalOpen` property. |
| [`ArchiveClue.cs`](Assets/Scripts/ArchiveClue.cs) | 14 | Data-only MonoBehaviour. Holds `clueId`, `clueTitle`, and `description` (TextArea) for archive objects. |
| [`ArchiveSceneController.cs`](Assets/Scripts/ArchiveSceneController.cs) | 212 | Interaction controller for the Research Archive scene. Own raycast loop, `HashSet<string>` duplicate guard, clue panel, objective tracker. Registers evidence with source/category metadata. |

### UI Systems

| Script | Lines | Purpose |
|--------|-------|---------|
| [`HUDBadge.cs`](Assets/Scripts/HUDBadge.cs) | 144 | Live evidence counter badge. Subscribes to `GameStateManager.OnEvidenceUpdated`. Coroutine-based pulse animation on update. |
| [`UIInputHandler.cs`](Assets/Scripts/UIInputHandler.cs) | 82 | Global Escape key handler with priority: Journal → Examination Panel. |
| [`DetectiveBoardController.cs`](Assets/Scripts/DetectiveBoardController.cs) | 425 | Procedurally generates the entire detective cork board at runtime. Evidence cards, red thread links, pixel-art icons, detail overlay, navigation buttons. |
| [`CaseConclusionController.cs`](Assets/Scripts/CaseConclusionController.cs) | 443 | Four-question deduction quiz. Runtime visual polish. Score tracking. Evidence-based explanations. Final case report generation. |

### Scene Management

| Script | Lines | Purpose |
|--------|-------|---------|
| [`MainMenuController.cs`](Assets/Scripts/MainMenuController.cs) | 70 | Play/HowToPlay/Quit button handlers. Routes Play through `SceneFader`. |
| [`LoadingScreenController.cs`](Assets/Scripts/LoadingScreenController.cs) | 145 | Async scene loading with animated crime-scene tape and staggered text fade-ins. Minimum display time enforced. |
| [`USBRevealController.cs`](Assets/Scripts/USBRevealController.cs) | 151 | Terminal-style typewriter reveal of USB contents (3 file fragments). Navigates to conclusion. |
| [`VerdictRevealController.cs`](Assets/Scripts/VerdictRevealController.cs) | 175 | Cinematic epilogue with slide-in evidence cards, punch-scale "CASE SOLVED" stamp, and return-to-menu button. |

### Data Models

| Class | Defined In | Purpose |
|-------|-----------|---------|
| `EvidenceRecord` | `GameStateManager.cs` | Serializable evidence metadata: `evidenceId`, `title`, `description`, `source`, `category` |
| `AudioManager.SoundEntry` | `AudioManager.cs` | Serializable audio clip definition: `name`, `clip`, `volume`, `loop` |

---

## Asset Inventory

### Third-Party Asset Packs

| Asset | Publisher | Location | Used For |
|-------|-----------|----------|----------|
| **Office Essentials Pack** | nappin | `Assets/nappin/OfficeEssentialsPack/` | Case Room environment — desk, chair, monitor, bookshelf, keyboard, coffee mug, office props. Contains `Models/`, `Materials/`, `Prefabs/`, and `Textures/` subdirectories. |
| **TextMesh Pro** | Unity Technologies | `Assets/TextMesh Pro/` | All in-game text rendering. Includes default font assets (`Fonts/`), materials, shaders, example sprites, and style sheets. |

### Custom Materials

| Material | Shader | Used In | Purpose |
|----------|--------|---------|---------|
| `Mat_Floor.mat` | URP/Lit | Case Room | Office floor surface |
| `Mat_Wall.mat` | URP/Lit | Case Room | Office wall surface |
| `USB_Anodized_Teal.mat` | URP/Lit | Case Room | USB drive body — teal anodized metal look |
| `USB_Brushed_Metal.mat` | URP/Lit | Case Room | USB drive connector — brushed steel finish |
| `USB_Status_Glow.mat` | URP/Lit (Emission) | Case Room | USB drive LED indicator — emissive green glow |
| `Archive_Walnut.mat` | URP/Lit | Research Archive | Wooden archive shelving and panels |
| `Archive_DeepBlue.mat` | URP/Lit | Research Archive | Archive room wall paint |
| `Archive_Floor.mat` | URP/Lit | Research Archive | Archive room flooring |
| `Archive_Brass.mat` | URP/Lit | Research Archive | Metal filing cabinet handles and accents |
| `Archive_Paper.mat` | URP/Lit | Research Archive | Document and file folder surfaces |
| `Archive_Terminal.mat` | URP/Lit | Research Archive | Terminal screen surfaces |

### Audio Assets

All audio files are `.wav` format stored in `Assets/Audio/`:

| File | Duration | Type | Used For |
|------|----------|------|----------|
| `ambient_office.wav` | ~6s loop | Ambient | Background atmosphere in Case Room — subtle HVAC hum |
| `ambient_archive.wav` | ~6s loop | Ambient | Background atmosphere in Research Archive |
| `suspense_bgm.wav` | ~18s loop | Music | Suspenseful background music |
| `sfx_clue_found.wav` | ~0.5s | SFX | Discovery sound when examining a new clue |
| `sfx_usb_found.wav` | ~1s | SFX | Tension sting when finding the USB drive |
| `sfx_page_turn.wav` | ~0.5s | SFX | Journal open/close, re-examining known evidence |
| `sfx_ui_click.wav` | ~0.5s | SFX | UI button click feedback |
| `sting_case_solved.wav` | ~1s | SFX | Victory sting on perfect deduction score |
| `sting_unresolved.wav` | ~0.5s | SFX | Incorrect answer feedback |
| `sfx_footstep_walk_a.wav` | ~0.1s | SFX | Walking footstep variant A |
| `sfx_footstep_walk_b.wav` | ~0.1s | SFX | Walking footstep variant B |
| `sfx_footstep_walk_c.wav` | ~0.1s | SFX | Walking footstep variant C |
| `sfx_footstep_sprint_a.wav` | ~0.1s | SFX | Sprint footstep variant A |
| `sfx_footstep_sprint_b.wav` | ~0.1s | SFX | Sprint footstep variant B |

### UI Assets

| File | Location | Purpose |
|------|----------|---------|
| `Noir Detective Office Investigation Board.png` | `Assets/MainMenu/` | Main menu background image — noir-styled detective office |

---

## Project Directory Structure

```
VR Detective - Case Files/
├── Assets/
│   ├── Audio/                          # All .wav sound effects and ambient loops
│   ├── Editor/                         # Editor-only scripts (VRDetectiveSetup.cs)
│   ├── MainMenu/                       # Main menu background image
│   ├── Materials/                      # Custom URP/Lit materials (12 .mat files)
│   ├── Models/                         # Custom 3D models (USB drive)
│   ├── nappin/
│   │   └── OfficeEssentialsPack/       # Third-party office furniture asset pack
│   │       ├── Materials/
│   │       ├── Models/
│   │       ├── Prefabs/
│   │       └── Textures/
│   ├── Prefabs/                        # Reusable prefabs
│   ├── Scenes/
│   │   ├── 00_MainMenu.unity           # Main menu scene
│   │   ├── 01_CaseRoom.unity           # Investigation office scene
│   │   ├── 02_ResearchArchive.unity    # Archive exploration scene
│   │   ├── 03_DetectiveBoard.unity     # Evidence board scene
│   │   └── 04_CaseConclusion.unity     # Deduction quiz scene
│   ├── Scripts/                        # All C# gameplay scripts (21 files)
│   ├── Settings/                       # URP pipeline, renderer, and volume assets
│   ├── TextMesh Pro/                   # TMP package (fonts, shaders, materials)
│   ├── Tests/EditMode/                 # Edit-mode test stubs
│   ├── TutorialInfo/                   # Unity tutorial metadata
│   ├── UI/                             # UI sprites and assets
│   └── URPDefaultResources/            # URP default shader resources
├── Packages/                           # Unity Package Manager manifest
├── ProjectSettings/                    # Unity project configuration
└── README.md                           # This file
```

---

## Build Settings

Scenes must be added to **File → Build Settings** in this exact order:

| Index | Scene | Role |
|-------|-------|------|
| 0 | `00_MainMenu` | Entry point |
| 1 | `01_CaseRoom` | Investigation (office) |
| 2 | `02_ResearchArchive` | Investigation (archive) |
| 3 | `03_DetectiveBoard` | Evidence review |
| 4 | `04_CaseConclusion` | Final deduction quiz |

---

## How to Build

### Prerequisites
- **Unity 6** (6000.x) with the **Universal RP** package
- **TextMeshPro** (installed via Package Manager → Unity Registry)
- Windows 10/11 PC

### Steps
1. Open the project in Unity Hub → select Unity 6
2. Wait for full script compilation (Console should show 0 errors)
3. Verify all 5 scenes are in **File → Build Settings** in the correct order
4. **File → Build Settings → Build** → choose output folder
5. Run the generated `.exe`

### Play in Editor
1. Open `Assets/Scenes/00_MainMenu.unity`
2. Press **Play** ▶
3. The `DontDestroyOnLoad` singletons initialize from whichever scene you enter — individual scene testing always works

---

## Credits

| Role | Detail |
|------|--------|
| **Game Design & Development** | Student project — IIIT Sri City, Semester 7 (VAR Course) |
| **Game Engine** | Unity 6 by Unity Technologies |
| **Office Assets** | Office Essentials Pack by nappin (Unity Asset Store) |
| **Text Rendering** | TextMesh Pro by Unity Technologies |
| **Audio** | Procedurally generated and royalty-free .wav clips |

---

*VR Detective: Case Files — A first-person 3D detective mystery where players investigate Dr. Vale's disappearance, examine evidence across two crime scenes, build a detective wall, and prove the truth through logical deduction.*

