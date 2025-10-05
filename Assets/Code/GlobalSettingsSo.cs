using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "GlobalSettingsSo", menuName = "Data/GlobalSettingsSo")]
public class GlobalSettingsSo : ScriptableObject
{
    public Color              characterSelectColor = Color.green;
    public Color              targetSelectColor    = Color.yellow;
    public Color              targetItemColor      = Color.green;
    public List<AudioClip>    onDigHitSounds;
    public AudioClip          onTileCrushedSound;
    public AudioClip          onItemSold;
    public List<AudioClip>    barkCharSounds;
    public BarkDataSo         barkOnNoCoal;
    public float              freezeInterval = 10f;
    public float              warmInterval   = 10f;
    public CoalLecturesDataSo coalLecturesData;
    public List<AudioClip>    footsteps;
}
