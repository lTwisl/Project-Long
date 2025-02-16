using UnityEngine;
using UnityEditor;
using System;

[CustomEditor(typeof(WorldTime))]
public class WorldTimeEditor : Editor
{
    private int day = 0;
    private int hour = 0;
    private int minute = 0;

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

        // 1. �� ������� ������ ��������� ������� �������� ���� ������� ������
        float realSecondsPerGameMinuteClassic = 1 / worldTime.timeScaleClassic * 60;
        float realSecondsPerGameMinuteSpeedUp = 1 / worldTime.timeScaleSpeedUp * 60;

        // 2. ������� ������� ����� �������� �� ���� ������ ��������� �������
        float gameMinutesPerRealMinuteClassic = worldTime.timeScaleClassic;
        float gameMinutesPerRealMinuteSpeedUp = worldTime.timeScaleSpeedUp;

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
            fontSize = 26,
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
        //EditorGUILayout.LabelField(worldTime.CurrentTime.ToString(@"dd  hh\:mm\:ss"), timerStyle);
        string formattedTime = $"{worldTime.CurrentTime.Days:D3}  {worldTime.CurrentTime.Hours:D2}:{worldTime.CurrentTime.Minutes:D2}:{worldTime.CurrentTime.Seconds:D2}";
        EditorGUILayout.LabelField(formattedTime, timerStyle);

        // ������� ��������� ���
        EditorGUILayout.Space();
        float progress = (float)(worldTime.CurrentTime.TotalHours % 24) / 24;
        EditorGUI.ProgressBar(EditorGUILayout.GetControlRect(), progress, "�������� ���");

        // ��������� ������ ��� ��������� �������� �������
        EditorGUILayout.Space();
        EditorGUILayout.LabelField("���������� ������� �����:", headerStyle);

        // �������������� ������ ��� ����� �����
        EditorGUILayout.BeginVertical();
        {
            day = EditorGUILayout.IntField("����", day, GUILayout.ExpandWidth(true));
            hour = EditorGUILayout.IntField("���", hour, GUILayout.ExpandWidth(true));
            minute = EditorGUILayout.IntField("������", minute, GUILayout.ExpandWidth(true));
        }
        EditorGUILayout.EndVertical();
        EditorGUILayout.Space();

        // ������ ��� ���������� ��������
        if (GUILayout.Button("��������� �����"))
        {
            TimeSpan newTime = new TimeSpan(day, hour, minute, 0);
            worldTime.CurrentTime = newTime;
        }

        // ������ �������� �������� �� �������
        EditorGUILayout.Space();
        EditorGUILayout.BeginHorizontal();
        {
            if (GUILayout.Button("-1 ���"))
            {
                worldTime.CurrentTime = worldTime.CurrentTime.Add(TimeSpan.FromHours(-1));
            }
            if (GUILayout.Button("+1 ���"))
            {
                worldTime.CurrentTime = worldTime.CurrentTime.Add(TimeSpan.FromHours(1));
            }
            GUILayout.Space(10);
            if (GUILayout.Button("-1 ����"))
            {
                worldTime.CurrentTime = worldTime.CurrentTime.Add(TimeSpan.FromDays(-1));
            }
            if (GUILayout.Button("+1 ����"))
            {
                worldTime.CurrentTime = worldTime.CurrentTime.Add(TimeSpan.FromDays(1));
            }
        }
        EditorGUILayout.EndHorizontal();
    }
}