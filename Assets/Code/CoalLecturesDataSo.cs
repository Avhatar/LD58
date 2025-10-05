using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "CoalLecturesDataSo", menuName = "Data/CoalLecturesDataSo")]
public class CoalLecturesDataSo : ScriptableObject
{
    public List<BarkDataSo> lectures = new List<BarkDataSo>();
}
