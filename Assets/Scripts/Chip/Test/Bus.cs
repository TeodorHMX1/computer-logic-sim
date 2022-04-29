﻿using UnityEngine;

public class Bus : Chip
{
    private const int HighZ = -1;

    public MeshRenderer meshRenderer;
    public Palette palette;

    protected override void ProcessOutput()
    {
        var outputSignal = -1;
        for (var i = 0; i < inputPins.Length; i++)
            if (inputPins[i].HasParent)
                if (inputPins[i].State != HighZ)
                    if (inputPins[i].State == 1)
                        outputSignal = 1;
                    else
                        outputSignal = 0;

        for (var i = 0; i < outputPins.Length; i++) outputPins[i].ReceiveSignal(outputSignal);

        SetCol(outputSignal);
    }

    private void SetCol(int signal)
    {
        meshRenderer.material.color = signal == 1 ? palette.onCol : palette.offCol;
        if (signal == -1) meshRenderer.material.color = palette.highZCol;
    }

    public Pin GetBusConnectionPin(Pin wireStartPin, Vector2 connectionPos)
    {
        Pin connectionPin = null;
        // Wire wants to put data onto bus
        if (wireStartPin != null && wireStartPin.pinType == Pin.PinType.ChipOutput)
            connectionPin = FindUnusedInputPin();
        else // Wire wants to get data from bus
            connectionPin = FindUnusedOutputPin();
        var lineCentre = (Vector2) transform.position;
        var pos = MathUtility.ClosestPointOnLineSegment(lineCentre + Vector2.left * 100,
            lineCentre + Vector2.right * 100, connectionPos);
        connectionPin.transform.position = pos;
        return connectionPin;
    }

    private Pin FindUnusedOutputPin()
    {
        for (var i = 0; i < outputPins.Length; i++)
            if (outputPins[i].childPins.Count == 0)
                return outputPins[i];
        Debug.Log("Ran out of pins");
        return null;
    }

    private Pin FindUnusedInputPin()
    {
        for (var i = 0; i < inputPins.Length; i++)
            if (inputPins[i].parentPin == null)
                return inputPins[i];
        Debug.Log("Ran out of pins");
        return null;
    }
}