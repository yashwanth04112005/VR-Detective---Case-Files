using System;
using System.Linq;
using System.Reflection;
using NUnit.Framework;
using UnityEditor;
using UnityEditor.Build;
using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Lightweight automated checks for the core VR Detective flow.
/// Run these from Window > General > Test Runner > EditMode.
/// Gameplay input, visuals, and audio perception still need a manual playthrough.
/// </summary>
public class VRDetectiveCoreTests
{
    private GameObject managerObject;
    private GameObject tempObject;
    private Type managerType;
    private object manager;

    [SetUp]
    public void SetUp()
    {
        managerType = FindType("GameStateManager");
        Assert.That(managerType, Is.Not.Null, "GameStateManager script must compile into the project.");
        managerObject = new GameObject("Test_GameStateManager");
        manager = managerObject.AddComponent(managerType);
        managerType.GetProperty("Instance", BindingFlags.Public | BindingFlags.Static)
            .GetSetMethod(true).Invoke(null, new[] { manager });
        managerType.GetMethod("ResetState").Invoke(manager, null);
    }

    [TearDown]
    public void TearDown()
    {
        if (managerType != null)
            managerType.GetProperty("Instance", BindingFlags.Public | BindingFlags.Static)
                .GetSetMethod(true).Invoke(null, new object[] { null });
        if (managerObject != null) UnityEngine.Object.DestroyImmediate(managerObject);
        if (tempObject != null) UnityEngine.Object.DestroyImmediate(tempObject);
    }

    [Test]
    public void T01_MainMenuSceneIsInBuild()
    {
        AssertSceneEnabled("00_MainMenu");
    }

    [Test]
    public void T02_BuildSceneOrderContainsAllFiveScenes()
    {
        string[] expected = { "00_MainMenu", "01_CaseRoom", "02_ResearchArchive", "03_DetectiveBoard", "04_CaseConclusion" };
        string[] actual = EditorBuildSettings.scenes.Where(s => s.enabled)
            .Select(s => System.IO.Path.GetFileNameWithoutExtension(s.path)).ToArray();
        CollectionAssert.AreEqual(expected, actual);
    }

    [Test]
    public void T03_PlayerMovementScriptHasMovementAndLookInputs()
    {
        string source = ReadScript("PlayerMovement.cs");
        StringAssert.Contains("Input.GetAxis(\"Horizontal\")", source);
        StringAssert.Contains("Input.GetAxis(\"Vertical\")", source);
    }

    [Test]
    public void T04_SprintMultiplierIsGreaterThanNormalSpeed()
    {
        FieldInfo field = FindType("PlayerMovement").GetField("sprintMultiplier");
        Assert.That(field, Is.Not.Null);
        Assert.That((float)field.GetValue(AddTemporaryComponent(FindType("PlayerMovement"))), Is.GreaterThan(1f));
    }

    [Test]
    public void T05_CrouchHeightIsConfiguredBelowStandingHeight()
    {
        string source = ReadScript("PlayerMovement.cs");
        StringAssert.Contains("KeyCode.LeftControl", source);
        StringAssert.Contains("crouchHeight", source);
    }

    [Test]
    public void T06_JumpUsesSpaceAndGroundCheck()
    {
        string source = ReadScript("PlayerMovement.cs");
        StringAssert.Contains("KeyCode.Space", source);
        StringAssert.Contains("controller.isGrounded", source);
    }

    [Test]
    public void T07_InteractionUsesConfiguredRaycastDistance()
    {
        string source = ReadScript("PlayerInteraction.cs");
        StringAssert.Contains("Physics.Raycast(ray, out RaycastHit hit, interactionDistance)", source);
    }

    [Test]
    public void T08_EvidenceRegistrationRejectsDuplicateIds()
    {
        bool first = Register("clock", false);
        bool second = Register("clock", false);
        Assert.That(first, Is.True);
        Assert.That(second, Is.False);
        Assert.That(EvidenceCount, Is.EqualTo(1));
    }

    [Test]
    public void T09_CaseRoomClueCategoriesAreMapped()
    {
        string source = ReadScript("PlayerInteraction.cs");
        StringAssert.Contains("case_stopped_clock", source);
        StringAssert.Contains("case_coffee_mug", source);
        StringAssert.Contains("case_computer_records", source);
        StringAssert.Contains("case_research_book", source);
    }

    [Test]
    public void T10_USBRegistrationSetsFlagAndCountsEvidence()
    {
        Register("usb", true);
        Assert.That(GetProperty("USBFound"), Is.True);
        Assert.That(EvidenceCount, Is.EqualTo(1));
    }

    [Test]
    public void T11_USBIsRevealedAfterFourCaseClues()
    {
        string source = ReadScript("PlayerInteraction.cs");
        StringAssert.Contains("EvidenceCollected >= 4", source);
        StringAssert.Contains("usbDrive.SetActive(true)", source);
    }

    [Test]
    public void T12_USBClueIsRegisteredOnlyOnceByInteraction()
    {
        string source = ReadScript("PlayerInteraction.cs");
        StringAssert.Contains("examinedObjects.Add(clue)", source);
        StringAssert.Contains("case_usb_drive", source);
    }

    [Test]
    public void T13_CaseRoomTransitionTargetsResearchArchive()
    {
        string source = ReadScript("PlayerInteraction.cs");
        StringAssert.Contains("02_ResearchArchive", source);
        StringAssert.Contains("KeyCode.F", source);
    }

    [Test]
    public void T14_ArchiveRequiresThreeUniqueRecords()
    {
        FieldInfo required = FindType("ArchiveSceneController").GetField("requiredRecords");
        Assert.That(required, Is.Not.Null);
        Assert.That((int)required.GetValue(AddTemporaryComponent(FindType("ArchiveSceneController"))), Is.EqualTo(3));
        string source = ReadScript("ArchiveSceneController.cs");
        StringAssert.Contains("discoveredRecords.Add(id)", source);
    }

    [Test]
    public void T15_ArchiveTransitionTargetsDetectiveBoard()
    {
        Assert.That(ReadScript("ArchiveSceneController.cs"), Does.Contain("03_DetectiveBoard"));
    }

    [Test]
    public void T16_EvidenceRecordsPersistOnGameStateManagerObject()
    {
        Register("case_clock", false);
        Assert.That(managerObject.GetComponent(managerType), Is.SameAs(manager));
        Assert.That(EvidenceCount, Is.EqualTo(1));
    }

    [Test]
    public void T17_BoardBuildsCardsFromCollectedEvidence()
    {
        string source = ReadScript("DetectiveBoardController.cs");
        StringAssert.Contains("EvidenceRecords", source);
        StringAssert.Contains("CreateEvidenceCard", source);
    }

    [Test]
    public void T18_BoardContinueTargetsCaseConclusion()
    {
        Assert.That(ReadScript("DetectiveBoardController.cs"), Does.Contain("04_CaseConclusion"));
    }

    [Test]
    public void T19_QuizDefinesFourQuestionsAndScoring()
    {
        Type quiz = FindType("CaseConclusionController");
        string[][] choices = (string[][])quiz.GetField("choices", BindingFlags.Instance | BindingFlags.NonPublic)
            .GetValue(AddTemporaryComponent(quiz));
        int[] correct = (int[])quiz.GetField("correctChoices", BindingFlags.Instance | BindingFlags.NonPublic)
            .GetValue(AddTemporaryComponent(quiz));
        Assert.That(choices.Length, Is.EqualTo(4));
        Assert.That(correct.Length, Is.EqualTo(4));
        Assert.That(choices.All(row => row.Length == 3), Is.True);
    }

    [Test]
    public void T20_FinalResultStoresScoreAndQuestionCount()
    {
        managerType.GetMethod("SetDeductionResult").Invoke(manager, new object[] { 3, 4 });
        Assert.That((int)managerType.GetProperty("DeductionScore").GetValue(manager), Is.EqualTo(3));
        Assert.That((int)managerType.GetProperty("DeductionQuestions").GetValue(manager), Is.EqualTo(4));
    }

    private object AddTemporaryComponent(Type type)
    {
        tempObject = new GameObject("Test_" + type.Name);
        return tempObject.AddComponent(type);
    }

    private bool Register(string id, bool isUsb)
    {
        return (bool)managerType.GetMethod("RegisterCollectedEvidence").Invoke(manager,
            new object[] { id, id, "test description", "Test", "test", isUsb });
    }

    private int EvidenceCount => (int)managerType.GetProperty("EvidenceCollected").GetValue(manager);
    private object GetProperty(string name) => managerType.GetProperty(name).GetValue(manager);

    private static Type FindType(string name) => AppDomain.CurrentDomain.GetAssemblies()
        .Select(assembly => assembly.GetType(name, false)).FirstOrDefault(type => type != null);

    private static void AssertSceneEnabled(string sceneName)
    {
        Assert.That(EditorBuildSettings.scenes.Any(s => s.enabled &&
            System.IO.Path.GetFileNameWithoutExtension(s.path) == sceneName), Is.True, sceneName + " must be enabled in Build Settings.");
    }

    private static string ReadScript(string filename)
    {
        string path = "Assets/Scripts/" + filename;
        TextAsset asset = AssetDatabase.LoadAssetAtPath<TextAsset>(path);
        Assert.That(asset, Is.Not.Null, "Could not load " + path);
        return asset.text;
    }
}




