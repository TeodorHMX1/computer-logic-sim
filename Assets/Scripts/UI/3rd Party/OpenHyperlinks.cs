﻿using System;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;

// Script from: https://deltadreamgames.com/unity-tmp-hyperlinks/
// somewhat based upon the TextMesh Pro example script: TMP_TextSelector_B
[RequireComponent(typeof(TextMeshProUGUI))]
public class OpenHyperlinks : MonoBehaviour, IPointerClickHandler
{
    public bool doesColorChangeOnHover = true;
    public Color hoverColor = new Color(60f / 255f, 120f / 255f, 1f);
    private Camera pCamera;
    private Canvas pCanvas;

    private int pCurrentLink = -1;
    private List<Color32[]> pOriginalVertexColors = new List<Color32[]>();

    private TextMeshProUGUI pTextMeshPro;

    public bool isLinkHighlighted => pCurrentLink != -1;

    protected virtual void Awake()
    {
        pTextMeshPro = GetComponent<TextMeshProUGUI>();
        pCanvas = GetComponentInParent<Canvas>();

        // Get a reference to the camera if Canvas Render Mode is not ScreenSpace Overlay.
        if (pCanvas.renderMode == RenderMode.ScreenSpaceOverlay)
            pCamera = null;
        else
            pCamera = pCanvas.worldCamera;
    }

    private void LateUpdate()
    {
        // is the cursor in the correct region (above the text area) and furthermore, in the link region?
        var isHoveringOver =
            TMP_TextUtilities.IsIntersectingRectTransform(pTextMeshPro.rectTransform, Input.mousePosition, pCamera);
        var linkIndex = isHoveringOver
            ? TMP_TextUtilities.FindIntersectingLink(pTextMeshPro, Input.mousePosition, pCamera)
            : -1;

        // Clear previous link selection if one existed.
        if (pCurrentLink != -1 && linkIndex != pCurrentLink)
        {
            // Debug.Log("Clear old selection");
            SetLinkToColor(pCurrentLink, (linkIdx, vertIdx) => pOriginalVertexColors[linkIdx][vertIdx]);
            pOriginalVertexColors.Clear();
            pCurrentLink = -1;
        }

        // Handle new link selection.
        if (linkIndex != -1 && linkIndex != pCurrentLink)
        {
            // Debug.Log("New selection");
            pCurrentLink = linkIndex;
            if (doesColorChangeOnHover)
                pOriginalVertexColors = SetLinkToColor(linkIndex, (_linkIdx, _vertIdx) => hoverColor);
        }

        // Debug.Log(string.Format("isHovering: {0}, link: {1}", isHoveringOver, linkIndex));
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        // Debug.Log("Click at POS: " + eventData.position + "  World POS: " + eventData.worldPosition);

        var linkIndex = TMP_TextUtilities.FindIntersectingLink(pTextMeshPro, Input.mousePosition, pCamera);
        if (linkIndex != -1)
        {
            // was a link clicked?
            var linkInfo = pTextMeshPro.textInfo.linkInfo[linkIndex];

            // Debug.Log(string.Format("id: {0}, text: {1}", linkInfo.GetLinkID(), linkInfo.GetLinkText()));
            // open the link id as a url, which is the metadata we added in the text field
            Application.OpenURL(linkInfo.GetLinkID());
        }
    }

    private List<Color32[]> SetLinkToColor(int linkIndex, Func<int, int, Color32> colorForLinkAndVert)
    {
        var linkInfo = pTextMeshPro.textInfo.linkInfo[linkIndex];

        var oldVertColors = new List<Color32[]>(); // store the old character colors

        for (var i = 0; i < linkInfo.linkTextLength; i++)
        {
            // for each character in the link string
            var characterIndex = linkInfo.linkTextfirstCharacterIndex + i; // the character index into the entire text
            var charInfo = pTextMeshPro.textInfo.characterInfo[characterIndex];
            var meshIndex =
                charInfo
                    .materialReferenceIndex; // Get the index of the material / sub text object used by this character.
            var vertexIndex = charInfo.vertexIndex; // Get the index of the first vertex of this character.

            var vertexColors = pTextMeshPro.textInfo.meshInfo[meshIndex].colors32; // the colors for this character
            oldVertColors.Add(vertexColors.ToArray());

            if (charInfo.isVisible)
            {
                vertexColors[vertexIndex + 0] = colorForLinkAndVert(i, vertexIndex + 0);
                vertexColors[vertexIndex + 1] = colorForLinkAndVert(i, vertexIndex + 1);
                vertexColors[vertexIndex + 2] = colorForLinkAndVert(i, vertexIndex + 2);
                vertexColors[vertexIndex + 3] = colorForLinkAndVert(i, vertexIndex + 3);
            }
        }

        // Update Geometry
        pTextMeshPro.UpdateVertexData(TMP_VertexDataUpdateFlags.All);

        return oldVertColors;
    }
}