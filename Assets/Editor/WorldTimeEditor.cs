using UnityEngine;
using UnityEditor;
using System;

[CustomEditor(typeof(WorldTime))]
public class WorldTimeEditor : Editor
{
    private int day = 0;
    private int hour = 0;
    private int minute = 0;

    private Color _baseColor;

    private void OnEnable()
    {
        EditorApplication.update += Repaint;
    }

    private void OnDisable()
    {
        EditorApplication.update -= Repaint;
    }

    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();
        WorldTime worldTime = (WorldTime)target;

        _baseColor = GUI.backgroundColor;

        // 1. �� ������� ������ ��������� ������� �������� ���� ������� ������
        float realSecondsPerGameMinuteClassic = 1 / worldTime.TimeScaleGame * 60;
        float realSecondsPerGameMinuteSpeedUp = 1 / worldTime.TimeScaleSpeedUp * 60;

        // 2. ������� ������� ����� �������� �� ���� ������ ��������� �������
        float gameMinutesPerRealMinuteClassic = worldTime.TimeScaleGame;
        float gameMinutesPerRealMinuteSpeedUp = worldTime.TimeScaleSpeedUp;

        // 3. �� ������� �������� ����� �������� ������� ���� (24 ������� ����)
        float realMinutesPerGameDayClassic = realSecondsPerGameMinuteClassic * 24;
        float realMinutesPerGameDaySpeedUp = realSecondsPerGameMinuteSpeedUp * 24;

        // ����� ��� ����������
        GUIStyle headerStyle = new GUIStyle(EditorStyles.boldLabel)
        {
            fontSize = 10,
            fontStyle = FontStyle.Bold,
            normal = { textColor = EditorGUIUtility.isProSkin ? Color.white : Color.black }
        };

        // ����� ��� ��������
        GUIStyle valueStyle = new GUIStyle(EditorStyles.label)
        {
            fontSize = 10,
            normal = { textColor = Color.yellow }
        };

        // ����� ��� ������� �������
        GUIStyle timerStyle = new GUIStyle(EditorStyles.label)
        {
            fontSize = 22,
            fontStyle = FontStyle.Bold,
            normal = { textColor = Color.yellow }
        };

        // ���������� ���������� � ����������
        EditorGUILayout.Space();
        EditorGUILayout.LabelField("- �� ������� ������ ��������� ������� �������� ���� ������� ������:", headerStyle);
        EditorGUILayout.LabelField($"������������ ��������: {realSecondsPerGameMinuteClassic:F4} (����. ���)", valueStyle);
        EditorGUILayout.LabelField($"���������� ��������: {realSecondsPerGameMinuteSpeedUp:F4} (����. ���)", valueStyle);

        EditorGUILayout.Space();
        EditorGUILayout.LabelField("- ������� ������� ����� �������� �� ���� ������ ��������� �������:", headerStyle);
        EditorGUILayout.LabelField($"������������ ��������: {gameMinutesPerRealMinuteClassic:F4} (���. ���)", valueStyle);
        EditorGUILayout.LabelField($"���������� ��������: {gameMinutesPerRealMinuteSpeedUp:F4} (���. ���)", valueStyle);

        EditorGUILayout.Space();
        EditorGUILayout.LabelField("- �� ������� �������� ����� �������� ������� ����:", headerStyle);
        EditorGUILayout.LabelField($"������������ ��������: {realMinutesPerGameDayClassic:F4} (����. ���)", valueStyle);
        EditorGUILayout.LabelField($"���������� ��������: {realMinutesPerGameDaySpeedUp:F4} (����. ���)", valueStyle);

        // ����������� �������� �������� �������
        EditorGUILayout.Space();
        EditorGUILayout.LabelField("������� ������� �����:", headerStyle);
        EditorGUILayout.LabelField(worldTime.GetFormattedTime(worldTime.CurrentTime), timerStyle);

        // ������� ��������� ���
        EditorGUILayout.Space();
        float progress = (float)(worldTime.CurrentTime.TotalHours % 24) / 24;
        EditorGUI.ProgressBar(EditorGUILayout.GetControlRect(), progress, "�������� ���");

        // ��������� ������ ��� ��������� �������� �������
        EditorGUILayout.Space();
        EditorGUILayout.LabelField("���������� ��������:", headerStyle);

        // ������ �������� �������� �� �������
        EditorGUILayout.BeginHorizontal();
        {
            GUI.backgroundColor = new Color(1f, 0.8f, 0.4f);
            if (GUILayout.Button("-1 ������"))
            {
                worldTime.CurrentTime = worldTime.CurrentTime.Add(TimeSpan.FromMinutes(-1));
            }
            if (GUILayout.Button("+1 ������"))
            {
                worldTime.CurrentTime = worldTime.CurrentTime.Add(TimeSpan.FromMinutes(1));
            }
            GUILayout.Space(10);
            GUI.backgroundColor = new Color(1f, 0.6f, 0.3f);
            if (GUILayout.Button("-1 ���"))
            {
                worldTime.CurrentTime = worldTime.CurrentTime.Add(TimeSpan.FromHours(-1));
            }
            if (GUILayout.Button("+1 ���"))
            {
                worldTime.CurrentTime = worldTime.CurrentTime.Add(TimeSpan.FromHours(1));
            }
            GUILayout.Space(10);
            GUI.backgroundColor = new Color(1f, 0.4f, 0.2f);
            if (GUILayout.Button("-1 ����"))
            {
                worldTime.CurrentTime = worldTime.CurrentTime.Add(TimeSpan.FromDays(-1));
            }
            if (GUILayout.Button("+1 ����"))
            {
                worldTime.CurrentTime = worldTime.CurrentTime.Add(TimeSpan.FromDays(1));
            }
            GUI.backgroundColor = _baseColor;
        }
        EditorGUILayout.EndHorizontal();

        // �������������� ������ ��� ����� �����
        EditorGUILayout.Space(10);
        EditorGUILayout.LabelField("���������� ������� �����:", headerStyle);
        EditorGUILayout.BeginVertical();
        {
            day = EditorGUILayout.IntField("����", day, GUILayout.ExpandWidth(false));
            hour = EditorGUILayout.IntField("���", hour, GUILayout.ExpandWidth(false));
            minute = EditorGUILayout.IntField("������", minute, GUILayout.ExpandWidth(false));
        }
        EditorGUILayout.EndVertical();
        EditorGUILayout.Space();

        // ������ ��� ���������� ��������
        if (GUILayout.Button("���������� �����"))
        {
            TimeSpan newTime = new TimeSpan(day, hour, minute, 0);
            worldTime.CurrentTime = newTime;
        }
    }
}