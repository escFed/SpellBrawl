using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public sealed class KnockbackTrainingWindow : EditorWindow
{
    [SerializeField] private AttackStats attack;
    [SerializeField] private float charge;
    private Vector2 scroll;

    [MenuItem("Tools/Spell Brawl/Knockback Training")]
    public static void Open() => GetWindow<KnockbackTrainingWindow>("Knockback Training");

    private void OnInspectorUpdate() => Repaint();

    private void OnGUI()
    {
        scroll = EditorGUILayout.BeginScrollView(scroll);
        try
        {
            var training = Object.FindFirstObjectByType<TrainingManager>();
            if (training == null || !training.CanEditSession)
            {
                EditorGUILayout.HelpBox("Entrá en Play Mode en TrainingRoom y quitá la pausa para usar estas herramientas.", MessageType.Info);
                return;
            }

            Dummy dummy = training.TrainingDummy;
            EditorGUILayout.LabelField("Preparar una comparación", EditorStyles.boldLabel);
            EditorGUI.BeginChangeCheck();
            int damage = EditorGUILayout.IntSlider("Porcentaje inicial", training.StartingDamage, 0, 300);
            if (EditorGUI.EndChangeCheck()) training.SetStartingDamage(damage);
            EditorGUILayout.BeginHorizontal();
            foreach (int preset in new[] { 0, 50, 100, 150, 200 })
                if (GUILayout.Button(preset + "%")) training.SetStartingDamage(preset);
            EditorGUILayout.EndHorizontal();

            EditorGUI.BeginChangeCheck();
            var defender = (CharacterStats)EditorGUILayout.ObjectField("Defensor", dummy.TargetCharacter, typeof(CharacterStats), false);
            if (EditorGUI.EndChangeCheck() && defender != null) training.SetDefender(defender);
            EditorGUI.BeginChangeCheck();
            Vector2 direction = EditorGUILayout.Vector2Field("Dirección del defensor", dummy.DirectionalInput);
            if (EditorGUI.EndChangeCheck()) training.SetInfluence(direction);
            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button("Siguiente defensor")) training.CycleDefender();
            if (GUILayout.Button("Siguiente dirección")) training.CycleInfluence();
            if (GUILayout.Button("Repetir posición")) training.ResetTraining();
            EditorGUILayout.EndHorizontal();
            EditorGUILayout.HelpBox("Game view: R repite. F1 cambia porcentaje, F2 defensor y F3 dirección. Los cambios reinician la posición y conservan la última trayectoria para comparar.", MessageType.None);

            attack = (AttackStats)EditorGUILayout.ObjectField("Ataque de prueba", attack, typeof(AttackStats), false);
            if (attack is HeavyAttackStats) charge = EditorGUILayout.Slider("Carga", charge, 0f, 1f);
            using (new EditorGUI.DisabledScope(attack == null))
                if (GUILayout.Button("Aplicar impacto de prueba (sin hitbox)")) training.ApplyTestHit(attack, charge);

            TrainingLaunchTrace trace = dummy.Trace;
            EditorGUILayout.LabelField("Último impacto medido", EditorStyles.boldLabel);
            EditorGUILayout.LabelField($"Daño: {trace.DamageBefore}% → {trace.DamageAfter}% | Velocidad: {trace.InitialVelocity.magnitude:F2}");
            EditorGUILayout.LabelField($"X/Y: {trace.InitialVelocity.x:F2}, {trace.InitialVelocity.y:F2} | Stun: {trace.HitStun:F3} s | Registro: {trace.Elapsed:F2} s");
            EditorGUILayout.LabelField("Trayectoria real: actual en celeste, anterior en gris. Orígenes alineados.", EditorStyles.wordWrappedMiniLabel);
            DrawTrace(GUILayoutUtility.GetRect(200f, 230f, GUILayout.ExpandWidth(true)), trace);
            if (GUILayout.Button("Borrar registros")) trace.Clear();

            if (attack != null && dummy.TargetCharacter != null)
            {
                EditorGUILayout.Space();
                EditorGUILayout.LabelField("Comparación calculada del ataque seleccionado", EditorStyles.boldLabel);
                EditorGUILayout.LabelField("Antes → Después     Velocidad      Ángulo      Stun");
                CombatHit hit = TrainingManager.CreateTestHit(attack, charge, Vector2.zero);
                CharacterStats stats = dummy.TargetCharacter;
                foreach (int before in new[] { 0, 50, 100, 150, 200 })
                {
                    int after = before + Mathf.Max(0, Mathf.RoundToInt(hit.Damage * stats.defenseMultiplier));
                    Vector2 velocity = KnockbackCalculation.CalculateVelocity(hit, after, stats.weight, dummy.DirectionalInput, stats.directionalInfluenceDegrees);
                    float stun = KnockbackCalculation.CalculateHitStun(hit, velocity);
                    float angle = Mathf.Atan2(velocity.y, velocity.x) * Mathf.Rad2Deg;
                    EditorGUILayout.LabelField($"{before}% → {after}%       {velocity.magnitude:F2}       {angle:F1}°       {stun:F3} s");
                }
                EditorGUILayout.HelpBox("La tabla calcula el lanzamiento inicial. No predice combos, colisiones ni porcentajes de KO.", MessageType.None);
            }
        }
        finally { EditorGUILayout.EndScrollView(); }
    }

    private static void DrawTrace(Rect rect, TrainingLaunchTrace trace)
    {
        EditorGUI.DrawRect(rect, new Color(0.09f, 0.1f, 0.12f));
        Vector2 min = new Vector2(-1f, -1f);
        Vector2 max = new Vector2(1f, 1f);
        ExtendBounds(trace.Points, ref min, ref max);
        ExtendBounds(trace.PreviousPoints, ref min, ref max);
        rect = new Rect(rect.x + 12f, rect.y + 12f, rect.width - 24f, rect.height - 24f);
        // Equal world units per pixel preserve the observed launch angles.
        Vector2 center = (min + max) * 0.5f;
        float unitsPerPixel = Mathf.Max((max.x - min.x) / rect.width, (max.y - min.y) / rect.height);
        Vector2 halfRange = new Vector2(rect.width, rect.height) * unitsPerPixel * 0.5f;
        min = center - halfRange;
        max = center + halfRange;
        Handles.BeginGUI();
        DrawPath(rect, trace.PreviousPoints, min, max, Color.gray);
        DrawPath(rect, trace.Points, min, max, Color.cyan);
        Handles.EndGUI();
        GUI.Label(new Rect(rect.x, rect.yMax - 16f, rect.width, 20f), $"Rango X: {min.x:F1} a {max.x:F1} | Y: {min.y:F1} a {max.y:F1} unidades", EditorStyles.miniLabel);
    }

    private static void ExtendBounds(IReadOnlyList<Vector3> points, ref Vector2 min, ref Vector2 max)
    {
        for (int i = 0; i < points.Count; i++)
        {
            Vector2 delta = points[i] - points[0];
            min = Vector2.Min(min, delta);
            max = Vector2.Max(max, delta);
        }
    }

    private static void DrawPath(Rect rect, IReadOnlyList<Vector3> points, Vector2 min, Vector2 max, Color color)
    {
        Handles.color = color;
        for (int i = 1; i < points.Count; i++)
            Handles.DrawLine(Map(points[i - 1] - points[0], rect, min, max), Map(points[i] - points[0], rect, min, max));
    }

    private static Vector3 Map(Vector2 point, Rect rect, Vector2 min, Vector2 max) =>
        new Vector3(Mathf.Lerp(rect.xMin, rect.xMax, Mathf.InverseLerp(min.x, max.x, point.x)),
            Mathf.Lerp(rect.yMax, rect.yMin, Mathf.InverseLerp(min.y, max.y, point.y)), 0f);
}
