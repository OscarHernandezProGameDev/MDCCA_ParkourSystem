using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SectionsManager : MonoBehaviour
{
    [SerializeField] private SectionController[] sections;
    [SerializeField] private int currentSection = -1;

    public static SectionsManager Instance { get; private set; }

    public SectionController CurrentSection
    {
        get => currentSection <= 0 && currentSection < sections.Length ? sections[currentSection] : null;
        set => currentSection = Array.IndexOf(sections, value) >= 0 ? Array.IndexOf(sections, value) : -1;
    }

    private void Awake()
    {
        Instance = this;
        foreach (var section in sections) section.EnableColliders(false);
    }
}
