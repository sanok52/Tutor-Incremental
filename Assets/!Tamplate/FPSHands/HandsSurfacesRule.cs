using System;
using UnityEngine;

[Serializable]
public class HandsSurfacesRule
{
    public string SurfaceName;
    public bool attached; // если true, то предмет будет прикрепл€тьс€ к поверхности, иначе Ц просто становитьс€ на неЄ (с физикой)

    [Space]
    public Vector2 angleReference; //ƒиапазон углов (в градусах), в котором поверхность считаетс€ этим типом. ”гол считаетс€ от плоскости с нормалью вверх (Vector3.up). Ќапример, дл€ пола это может быть 0-30 градусов, дл€ стен Ц 60-120, дл€ потолка Ц 150-180.

    [Space]
    public string[] containsTag;
}