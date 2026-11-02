using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

/// <summary>Builds the case evidence wall from the clues collected during this playthrough.</summary>
public class DetectiveBoardController : MonoBehaviour
{
    private const string ConclusionScene = "04_CaseConclusion";
    private readonly List<Sprite> generatedSprites = new List<Sprite>();
    private Canvas canvas;
    private TMP_Text detailTitle;
    private TMP_Text detailSource;
    private TMP_Text detailBody;
    private GameObject detailOverlay;

    private void Start()
    {
        EnsureEventSystem();
        BuildBoard();
    }

    private void EnsureEventSystem()
    {
        if (EventSystem.current != null) return;
        new GameObject("EventSystem", typeof(EventSystem), typeof(StandaloneInputModule));
    }

    private void BuildBoard()
    {
        GameObject canvasObject = new GameObject("DetectiveBoardCanvas", typeof(RectTransform), typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
        canvas = canvasObject.GetComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        CanvasScaler scaler = canvasObject.GetComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920, 1080);
        scaler.matchWidthOrHeight = 0.5f;

        RectTransform screen = canvasObject.GetComponent<RectTransform>();
        AddPanel("Night Background", screen, Vector2.zero, new Vector2(1920, 1080), new Color(0.035f, 0.052f, 0.073f));
        RectTransform frame = AddPanel("Walnut Frame", screen, Vector2.zero, new Vector2(1830, 1010), new Color(0.20f, 0.13f, 0.09f));
        RectTransform cork = AddPanel("Cork Evidence Board", frame, Vector2.zero, new Vector2(1760, 940), new Color(0.39f, 0.27f, 0.18f));

        AddText("Board Header", cork, "CASE 001  /  THE 11:47 MYSTERY", new Vector2(0, 420), new Vector2(1500, 56), 38, new Color(0.96f, 0.77f, 0.42f), FontStyles.Bold, TextAlignmentOptions.Center);

        List<EvidenceRecord> records = new List<EvidenceRecord>();
        if (GameStateManager.Instance != null)
        {
            foreach (EvidenceRecord record in GameStateManager.Instance.EvidenceRecords)
                if (record != null) records.Add(record);
        }
        records.Sort((a, b) => EvidenceRank(a).CompareTo(EvidenceRank(b)));

        int linkCount = CountAvailableLinks(records);
        string status = records.Count == 0
            ? "NO EVIDENCE FILED — Complete the Case Room and Research Archive first."
            : $"EVIDENCE WALL     •     {records.Count} ITEMS     •     {linkCount} CORROBORATING CONNECTIONS";
        AddText("Board Status", cork, status, new Vector2(0, 374), new Vector2(1500, 30), 18, new Color(0.94f, 0.85f, 0.68f), FontStyles.Bold, TextAlignmentOptions.Center);

        Dictionary<int, Vector2> positions = GetCardPositions(records.Count);
        DrawEvidenceLinks(cork, records, positions);
        for (int i = 0; i < records.Count; i++)
            CreateEvidenceCard(cork, records[i], positions[i]);
        if (records.Count == 0)
        {
            AddText("Empty Board Message", cork,
                "Complete the Case Room and Research Archive to pin your discoveries here.",
                new Vector2(0, 40), new Vector2(1050, 120), 28, new Color(0.96f, 0.87f, 0.72f), FontStyles.Bold, TextAlignmentOptions.Center);
        }

        AddText("Board Instructions", cork,
            "Select any pinned item to inspect the full note. Red thread marks evidence that supports the same timeline or motive.",
            new Vector2(0, -396), new Vector2(1100, 34), 17, new Color(0.95f, 0.88f, 0.74f), FontStyles.Normal, TextAlignmentOptions.Center);

        CreateButton("Return to Main Menu", cork, "MAIN MENU", new Vector2(-660, -445), new Vector2(210, 52),
            new Color(0.10f, 0.13f, 0.17f), new Color(0.85f, 0.88f, 0.87f), ReturnToMenu);
        CreateButton("Continue to Deduction", cork, "REVIEW DEDUCTION  →", new Vector2(660, -445), new Vector2(310, 58),
            new Color(0.78f, 0.57f, 0.26f), new Color(0.10f, 0.08f, 0.06f), ContinueToConclusion);

        BuildDetailOverlay(screen);
    }

    private static int EvidenceRank(EvidenceRecord item)
    {
        string id = item.evidenceId.ToLowerInvariant();
        if (id.Contains("clock")) return 0;
        if (id.Contains("coffee")) return 1;
        if (id.Contains("computer")) return 2;
        if (id.Contains("research_book")) return 3;
        if (id.Contains("usb")) return 4;
        if (id.Contains("research_log")) return 5;
        if (id.Contains("access")) return 6;
        if (id.Contains("financial")) return 7;
        return 20;
    }

    private static Dictionary<int, Vector2> GetCardPositions(int count)
    {
        Dictionary<int, Vector2> result = new Dictionary<int, Vector2>();
        float[] x = { -625f, -208f, 208f, 625f };
        for (int i = 0; i < count; i++)
        {
            int row = i / 4;
            int column = i % 4;
            result[i] = new Vector2(x[column], row == 0 ? 160f : -150f);
        }
        return result;
    }

    private void CreateEvidenceCard(RectTransform parent, EvidenceRecord item, Vector2 position)
    {
        RectTransform card = AddPanel("Evidence Card - " + item.title, parent, position, new Vector2(386, 275), new Color(0.87f, 0.82f, 0.70f));
        card.gameObject.AddComponent<Shadow>().effectColor = new Color(0.05f, 0.025f, 0.01f, 0.55f);
        Image cardImage = card.GetComponent<Image>();
        cardImage.raycastTarget = true;
        Button inspect = card.gameObject.AddComponent<Button>();
        inspect.targetGraphic = cardImage;
        ColorBlock cardColors = inspect.colors;
        cardColors.normalColor = Color.white;
        cardColors.highlightedColor = new Color(1.08f, 1.04f, 0.94f, 1f);
        cardColors.pressedColor = new Color(0.89f, 0.83f, 0.70f, 1f);
        inspect.colors = cardColors;
        inspect.onClick.AddListener(() => ShowDetails(item));

        AddPanel("Pinned Accent", card, new Vector2(0, 129), new Vector2(386, 9), SourceColor(item.source));
        CreatePin(card);
        RectTransform thumbnail = AddPanel("Evidence Thumbnail", card, new Vector2(-128, 50), new Vector2(96, 96), new Color(0.20f, 0.16f, 0.12f));
        Image icon = thumbnail.GetComponent<Image>();
        icon.sprite = MakeEvidenceIcon(item.category);
        icon.preserveAspect = true;

        AddText("Evidence Title", card, item.title.ToUpperInvariant(), new Vector2(48, 86), new Vector2(242, 52), 18, new Color(0.12f, 0.13f, 0.14f), FontStyles.Bold, TextAlignmentOptions.Left);
        AddText("Evidence Source", card, item.source.ToUpperInvariant() + "  /  FILED", new Vector2(48, 47), new Vector2(242, 28), 12, new Color(0.38f, 0.31f, 0.23f), FontStyles.Bold, TextAlignmentOptions.Left);
        AddPanel("Paper Divider", card, new Vector2(0, 1), new Vector2(338, 2), new Color(0.56f, 0.47f, 0.36f, 0.75f));
        string shortDescription = item.description.Replace("\r", "").Replace("\n", "  •  ");
        AddText("Evidence Summary", card, shortDescription, new Vector2(0, -72), new Vector2(338, 128), 15, new Color(0.16f, 0.16f, 0.15f), FontStyles.Normal, TextAlignmentOptions.TopLeft);
        AddText("Inspect Hint", card, "CLICK TO EXAMINE", new Vector2(0, -124), new Vector2(300, 20), 10, new Color(0.39f, 0.28f, 0.18f), FontStyles.Bold, TextAlignmentOptions.Center);
    }

    private void CreatePin(RectTransform parent)
    {
        RectTransform pin = AddPanel("Red Evidence Pin", parent, new Vector2(0, 125), new Vector2(13, 13), new Color(0.63f, 0.10f, 0.075f));
        pin.GetComponent<Image>().raycastTarget = false;
        pin.gameObject.AddComponent<Shadow>().effectColor = new Color(0.1f, 0.02f, 0.01f, 0.7f);
    }

    private void DrawEvidenceLinks(RectTransform parent, List<EvidenceRecord> records, Dictionary<int, Vector2> positions)
    {
        string[,] links =
        {
            { "case_coffee_mug", "case_stopped_clock" },
            { "case_stopped_clock", "archive_access_record" },
            { "case_computer_records", "archive_research_log" },
            { "case_usb_drive", "archive_financial_ledger" },
            { "case_research_book", "archive_research_log" }
        };

        for (int link = 0; link < links.GetLength(0); link++)
        {
            int first = FindRecord(records, links[link, 0]);
            int second = FindRecord(records, links[link, 1]);
            if (first < 0 || second < 0 || !positions.ContainsKey(first) || !positions.ContainsKey(second)) continue;
            Vector2 a = positions[first];
            Vector2 b = positions[second];
            Vector2 delta = b - a;
            RectTransform thread = AddPanel("Red Thread - " + links[link, 0], parent, (a + b) * 0.5f, new Vector2(delta.magnitude, 5), new Color(0.55f, 0.055f, 0.045f, 0.88f));
            thread.localRotation = Quaternion.Euler(0f, 0f, Mathf.Atan2(delta.y, delta.x) * Mathf.Rad2Deg);
            thread.GetComponent<Image>().raycastTarget = false;
        }
    }

    private int FindRecord(List<EvidenceRecord> records, string id)
    {
        for (int i = 0; i < records.Count; i++)
            if (string.Equals(records[i].evidenceId, id, StringComparison.OrdinalIgnoreCase)) return i;
        return -1;
    }

    private int CountAvailableLinks(List<EvidenceRecord> records)
    {
        string[,] links =
        {
            { "case_coffee_mug", "case_stopped_clock" },
            { "case_stopped_clock", "archive_access_record" },
            { "case_computer_records", "archive_research_log" },
            { "case_usb_drive", "archive_financial_ledger" },
            { "case_research_book", "archive_research_log" }
        };
        int count = 0;
        for (int i = 0; i < links.GetLength(0); i++)
            if (FindRecord(records, links[i, 0]) >= 0 && FindRecord(records, links[i, 1]) >= 0) count++;
        return count;
    }

    private void BuildDetailOverlay(RectTransform parent)
    {
        detailOverlay = AddPanel("Evidence Detail Overlay", parent, Vector2.zero, new Vector2(1920, 1080), new Color(0.015f, 0.02f, 0.03f, 0.82f)).gameObject;
        detailOverlay.GetComponent<Image>().raycastTarget = true;
        RectTransform modal = AddPanel("Evidence Detail Card", detailOverlay.GetComponent<RectTransform>(), Vector2.zero, new Vector2(920, 530), new Color(0.88f, 0.83f, 0.72f));
        modal.gameObject.AddComponent<Shadow>().effectColor = new Color(0, 0, 0, 0.6f);
        AddPanel("Detail Accent", modal, new Vector2(0, 250), new Vector2(920, 12), new Color(0.77f, 0.56f, 0.28f));
        detailTitle = AddText("Detail Title", modal, "EVIDENCE", new Vector2(0, 190), new Vector2(800, 55), 29, new Color(0.12f, 0.13f, 0.14f), FontStyles.Bold, TextAlignmentOptions.Center);
        detailSource = AddText("Detail Source", modal, "SOURCE", new Vector2(0, 142), new Vector2(800, 28), 15, new Color(0.44f, 0.32f, 0.20f), FontStyles.Bold, TextAlignmentOptions.Center);
        detailBody = AddText("Detail Body", modal, "", new Vector2(0, -10), new Vector2(760, 250), 22, new Color(0.14f, 0.15f, 0.15f), FontStyles.Normal, TextAlignmentOptions.TopLeft);
        CreateButton("Close Evidence Detail", modal, "CLOSE FILE", new Vector2(0, -218), new Vector2(200, 52), new Color(0.15f, 0.18f, 0.20f), Color.white, () => detailOverlay.SetActive(false));
        detailOverlay.SetActive(false);
    }

    private void ShowDetails(EvidenceRecord item)
    {
        AudioManager.Instance?.PlayOneShot("sfx_page_turn");
        detailTitle.text = item.title.ToUpperInvariant();
        detailSource.text = item.source.ToUpperInvariant() + "  /  CASE 001";
        detailBody.text = item.description;
        detailOverlay.SetActive(true);
    }

    private void ContinueToConclusion()
    {
        AudioManager.Instance?.PlayOneShot("sfx_ui_click");
        LoadScene(ConclusionScene);
    }

    private void ReturnToMenu()
    {
        AudioManager.Instance?.PlayOneShot("sfx_ui_click");
        GameStateManager.Instance?.ResetState();
        LoadScene("00_MainMenu");
    }

    private static void LoadScene(string scene)
    {
        if (SceneFader.Instance != null) SceneFader.Instance.FadeToScene(scene);
        else SceneManager.LoadScene(scene);
    }

    private RectTransform AddPanel(string name, RectTransform parent, Vector2 position, Vector2 size, Color color)
    {
        GameObject item = new GameObject(name, typeof(RectTransform), typeof(Image));
        RectTransform rect = item.GetComponent<RectTransform>();
        rect.SetParent(parent, false);
        rect.anchorMin = rect.anchorMax = new Vector2(0.5f, 0.5f);
        rect.pivot = new Vector2(0.5f, 0.5f);
        rect.anchoredPosition = position;
        rect.sizeDelta = size;
        Image image = item.GetComponent<Image>();
        image.color = color;
        image.raycastTarget = false;
        return rect;
    }

    private TMP_Text AddText(string name, RectTransform parent, string value, Vector2 position, Vector2 size,
        float fontSize, Color color, FontStyles style, TextAlignmentOptions alignment)
    {
        GameObject item = new GameObject(name, typeof(RectTransform), typeof(TextMeshProUGUI));
        RectTransform rect = item.GetComponent<RectTransform>();
        rect.SetParent(parent, false);
        rect.anchorMin = rect.anchorMax = new Vector2(0.5f, 0.5f);
        rect.pivot = new Vector2(0.5f, 0.5f);
        rect.anchoredPosition = position;
        rect.sizeDelta = size;
        TMP_Text text = item.GetComponent<TMP_Text>();
        text.text = value;
        text.font = TMP_Settings.defaultFontAsset;
        text.fontSize = fontSize;
        text.color = color;
        text.fontStyle = style;
        text.alignment = alignment;
        text.textWrappingMode = TextWrappingModes.Normal;
        text.raycastTarget = false;
        return text;
    }

    private void CreateButton(string name, RectTransform parent, string label, Vector2 position, Vector2 size,
        Color background, Color foreground, UnityEngine.Events.UnityAction action)
    {
        RectTransform rect = AddPanel(name, parent, position, size, background);
        Image image = rect.GetComponent<Image>();
        image.raycastTarget = true;
        Button button = rect.gameObject.AddComponent<Button>();
        button.targetGraphic = image;
        button.onClick.AddListener(action);
        ColorBlock colors = button.colors;
        colors.normalColor = Color.white;
        colors.highlightedColor = new Color(1.1f, 1.02f, 0.9f, 1f);
        colors.pressedColor = new Color(0.8f, 0.8f, 0.8f, 1f);
        button.colors = colors;
        AddText("Button Label", rect, label, Vector2.zero, size - new Vector2(24, 12), 16, foreground, FontStyles.Bold, TextAlignmentOptions.Center);
    }

    private Color SourceColor(string source)
    {
        return source.Contains("Archive") ? new Color(0.29f, 0.52f, 0.62f) : new Color(0.78f, 0.57f, 0.28f);
    }

    private Sprite MakeEvidenceIcon(string category)
    {
        const int size = 96;
        Texture2D texture = new Texture2D(size, size, TextureFormat.RGBA32, false);
        texture.filterMode = FilterMode.Point;
        Color32 clear = new Color32(0, 0, 0, 0);
        Color32 ink = new Color32(232, 217, 181, 255);
        Color32 accent = new Color32(178, 57, 45, 255);
        Color32[] pixels = new Color32[size * size];
        for (int i = 0; i < pixels.Length; i++) pixels[i] = clear;
        string key = (category ?? "").ToLowerInvariant();

        if (key.Contains("clock"))
        {
            Circle(pixels, size, 48, 48, 31, ink, false);
            Line(pixels, size, 48, 48, 48, 27, accent);
            Line(pixels, size, 48, 48, 64, 53, accent);
        }
        else if (key.Contains("coffee"))
        {
            Rect(pixels, size, 23, 28, 53, 58, ink);
            Rect(pixels, size, 28, 34, 43, 45, new Color32(90, 48, 29, 255));
            Circle(pixels, size, 72, 55, 13, ink, false);
            Line(pixels, size, 68, 55, 80, 55, new Color32(34, 29, 25, 255));
        }
        else if (key.Contains("computer"))
        {
            Rect(pixels, size, 13, 22, 70, 56, ink);
            Rect(pixels, size, 20, 29, 56, 41, new Color32(42, 113, 126, 255));
            Line(pixels, size, 48, 22, 48, 12, accent);
            Line(pixels, size, 32, 12, 64, 12, accent);
        }
        else if (key.Contains("book") || key.Contains("research"))
        {
            Rect(pixels, size, 19, 15, 29, 68, ink);
            Rect(pixels, size, 49, 15, 29, 68, new Color32(205, 187, 149, 255));
            Line(pixels, size, 48, 16, 48, 82, accent);
            for (int y = 31; y < 72; y += 12) Line(pixels, size, 25, y, 42, y, accent);
            for (int y = 31; y < 72; y += 12) Line(pixels, size, 55, y, 72, y, accent);
        }
        else if (key.Contains("usb"))
        {
            Rect(pixels, size, 18, 36, 57, 27, new Color32(25, 139, 134, 255));
            Rect(pixels, size, 68, 41, 13, 17, ink);
            Line(pixels, size, 30, 43, 30, 55, accent);
        }
        else if (key.Contains("access"))
        {
            Rect(pixels, size, 19, 13, 58, 70, ink);
            Circle(pixels, size, 48, 60, 12, accent, true);
            Line(pixels, size, 31, 34, 65, 34, accent);
            Line(pixels, size, 31, 25, 65, 25, accent);
        }
        else if (key.Contains("ledger"))
        {
            Rect(pixels, size, 16, 13, 64, 70, ink);
            for (int y = 27; y < 75; y += 14) Line(pixels, size, 22, y, 74, y, accent);
            Line(pixels, size, 45, 20, 45, 76, accent);
            Line(pixels, size, 61, 20, 61, 76, accent);
        }
        else
        {
            Rect(pixels, size, 20, 14, 56, 68, ink);
            for (int y = 29; y < 72; y += 12) Line(pixels, size, 29, y, 68, y, accent);
        }

        texture.SetPixels32(pixels);
        texture.Apply(false, true);
        Sprite sprite = Sprite.Create(texture, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f), size);
        generatedSprites.Add(sprite);
        return sprite;
    }

    private static void Rect(Color32[] pixels, int size, int x, int y, int width, int height, Color32 color)
    {
        for (int py = Mathf.Max(0, y); py < Mathf.Min(size, y + height); py++)
            for (int px = Mathf.Max(0, x); px < Mathf.Min(size, x + width); px++) pixels[py * size + px] = color;
    }

    private static void Line(Color32[] pixels, int size, int x0, int y0, int x1, int y1, Color32 color)
    {
        int dx = Mathf.Abs(x1 - x0), sx = x0 < x1 ? 1 : -1;
        int dy = -Mathf.Abs(y1 - y0), sy = y0 < y1 ? 1 : -1;
        int error = dx + dy;
        while (true)
        {
            if (x0 >= 0 && x0 < size && y0 >= 0 && y0 < size) pixels[y0 * size + x0] = color;
            if (x0 == x1 && y0 == y1) break;
            int twice = 2 * error;
            if (twice >= dy) { error += dy; x0 += sx; }
            if (twice <= dx) { error += dx; y0 += sy; }
        }
    }

    private static void Circle(Color32[] pixels, int size, int cx, int cy, int radius, Color32 color, bool fill)
    {
        for (int y = -radius; y <= radius; y++)
        for (int x = -radius; x <= radius; x++)
        {
            int distance = x * x + y * y;
            if (fill ? distance <= radius * radius : distance <= radius * radius && distance >= (radius - 3) * (radius - 3))
            {
                int px = cx + x, py = cy + y;
                if (px >= 0 && px < size && py >= 0 && py < size) pixels[py * size + px] = color;
            }
        }
    }

    private void Update()
    {
        if (detailOverlay != null && detailOverlay.activeSelf && Input.GetKeyDown(KeyCode.Escape))
            detailOverlay.SetActive(false);
    }

    private void OnDestroy()
    {
        foreach (Sprite sprite in generatedSprites)
        {
            if (sprite != null)
            {
                Texture2D texture = sprite.texture;
                Destroy(sprite);
                if (texture != null) Destroy(texture);
            }
        }
    }
}
