using TMPro;
using UnityEngine;

namespace SuperNewRoles.RequestInGame;

public class RequestInGameDraftAutoSaver : MonoBehaviour
{
    private const float MinSaveIntervalSeconds = 0.3f;

    private RequestInGameType requestInGameType;
    private TextBoxTMP titleTextBox;
    private TextBoxTMP descriptionTextBox;
    private TextBoxTMP mapTextBox;
    private TextBoxTMP roleTextBox;
    private TextBoxTMP timingTextBox;
    private RequestInGameDraft lastDraft = RequestInGameDraft.Empty;
    private float lastSaveAt;
    private bool initialized;
    private bool savingEnabled = true;

    public void Init(
        RequestInGameType requestInGameType,
        TextBoxTMP titleTextBox,
        TextBoxTMP descriptionTextBox,
        TextBoxTMP mapTextBox,
        TextBoxTMP roleTextBox,
        TextBoxTMP timingTextBox)
    {
        this.requestInGameType = requestInGameType;
        this.titleTextBox = titleTextBox;
        this.descriptionTextBox = descriptionTextBox;
        this.mapTextBox = mapTextBox;
        this.roleTextBox = roleTextBox;
        this.timingTextBox = timingTextBox;
        RefreshSnapshot();
        initialized = true;
    }

    public void StopSaving()
    {
        savingEnabled = false;
    }

    public void RefreshSnapshot()
    {
        lastDraft = CreateCurrentDraft();
    }

    public void Update()
    {
        if (!initialized || !savingEnabled)
            return;

        SaveIfChanged();
    }

    public void OnDestroy()
    {
        if (!initialized || !savingEnabled)
            return;

        SaveIfChanged(true);
    }

    private void SaveIfChanged(bool force = false)
    {
        RequestInGameDraft currentDraft = CreateCurrentDraft();
        if (currentDraft == lastDraft)
            return;
        if (!force && Time.unscaledTime - lastSaveAt < MinSaveIntervalSeconds)
            return;

        RequestInGameDraftStore.Save(requestInGameType, currentDraft);
        lastDraft = currentDraft;
        lastSaveAt = Time.unscaledTime;
    }

    private RequestInGameDraft CreateCurrentDraft()
    {
        return new RequestInGameDraft(
            GetText(titleTextBox),
            GetText(descriptionTextBox),
            GetText(mapTextBox),
            GetText(roleTextBox),
            GetText(timingTextBox));
    }

    private static string GetText(TextBoxTMP textBox)
    {
        return textBox == null ? string.Empty : textBox.text ?? string.Empty;
    }
}
