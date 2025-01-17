﻿using System.Collections.Generic;
using UnityEngine;

public class Pin : MonoBehaviour
{
    public enum PinType
    {
        ChipInput,
        ChipOutput
    }

    public PinType pinType;

    // The chip that this pin is attached to (either as an input or output terminal)
    public Chip chip;
    public string pinName;

    [HideInInspector] public bool cyclic;

    // Index of this pin in its associated chip's input or output pin array
    [HideInInspector] public int index;

    // The pin from which this pin receives its input signal
    // (multiple inputs not allowed in order to simplify simulation)
    [HideInInspector] public Pin parentPin;

    // The pins which this pin forwards its signal to
    [HideInInspector] public List<Pin> childPins = new List<Pin>();

    // Current state of the pin: 0 == LOW, 1 == HIGH

    // Appearance
    private readonly Color defaultCol = Color.black;
    private readonly Color interactCol = new Color(0.7f, 0.7f, 0.7f);
    private Material material;

    public static float radius
    {
        get
        {
            var diameter = 0.215f;
            return diameter / 2;
        }
    }

    public static float interactionRadius => radius * 1.1f;

    // Get the current state of the pin: 0 == LOW, 1 == HIGH
    public int State { get; private set; }

    // Note that for ChipOutput pins, the chip itself is considered the parent, so will always return true
    // Otherwise, only true if the parentPin of this pin has been set
    public bool HasParent => parentPin != null || pinType == PinType.ChipOutput;

    private void Awake()
    {
        material = GetComponent<MeshRenderer>().material;
        material.color = defaultCol;
    }

    private void Start()
    {
        SetScale();
    }

    public void SetScale()
    {
        transform.localScale = Vector3.one * radius * 2;
    }

    // Receive signal: 0 == LOW, 1 = HIGH
    // Sets the current state to the signal
    // Passes the signal on to any connected pins / electronic component
    public void ReceiveSignal(int signal)
    {
        State = signal;

        if (pinType == PinType.ChipInput && !cyclic)
            chip.ReceiveInputSignal(this);
        else if (pinType == PinType.ChipOutput)
            for (var i = 0; i < childPins.Count; i++)
                childPins[i].ReceiveSignal(signal);
    }

    public static void MakeConnection(Pin pinA, Pin pinB)
    {
        if (IsValidConnection(pinA, pinB))
        {
            var parentPin = pinA.pinType == PinType.ChipOutput ? pinA : pinB;
            var childPin = pinA.pinType == PinType.ChipInput ? pinA : pinB;

            parentPin.childPins.Add(childPin);
            childPin.parentPin = parentPin;
        }
    }

    public static void RemoveConnection(Pin pinA, Pin pinB)
    {
        var parentPin = pinA.pinType == PinType.ChipOutput ? pinA : pinB;
        var childPin = pinA.pinType == PinType.ChipInput ? pinA : pinB;

        parentPin.childPins.Remove(childPin);
        childPin.parentPin = null;
    }

    public static bool IsValidConnection(Pin pinA, Pin pinB)
    {
        // Connection is valid if one pin is an output pin, and the other is an input pin
        return pinA.pinType != pinB.pinType;
    }

    public static bool TryConnect(Pin pinA, Pin pinB)
    {
        if (pinA.pinType != pinB.pinType)
        {
            var parentPin = pinA.pinType == PinType.ChipOutput ? pinA : pinB;
            var childPin = parentPin == pinB ? pinA : pinB;
            parentPin.childPins.Add(childPin);
            childPin.parentPin = parentPin;
            return true;
        }

        return false;
    }

    public void MouseEnter()
    {
        transform.localScale = Vector3.one * interactionRadius * 2;
        material.color = interactCol;
    }

    public void MouseExit()
    {
        transform.localScale = Vector3.one * radius * 2;
        material.color = defaultCol;
    }
}