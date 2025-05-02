using UnityEngine;

public class TimeConversionAttribute : PropertyAttribute
{
    public string ConvertToMinutesLabel;
    public string ConvertToDaysLabel;

    public TimeConversionAttribute(
        string toMinutesLabel = "Convert to Minutes",
        string toDaysLabel = "Convert to Days")
    {
        ConvertToMinutesLabel = toMinutesLabel;
        ConvertToDaysLabel = toDaysLabel;
    }
}
