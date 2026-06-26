#target photoshop

// Applies the same Blend If values shown in the reference screenshot
// to the currently active Photoshop layer.
//
// Blend If: Gray
// This Layer: black split 59 / 125, white 255 / 255
// Underlying Layer: black 0 / 0, white 255 / 255

app.bringToFront();

(function () {
    if (app.documents.length === 0) {
        alert("请先打开一个 PSD 文件。");
        return;
    }

    try {
        if (app.activeDocument.activeLayer.typename !== "ArtLayer" &&
            app.activeDocument.activeLayer.typename !== "LayerSet") {
            alert("当前选中的不是普通图层或图层组，请先选中一个 UI 图层。");
            return;
        }

        applyBlendIfGray(59, 125, 255, 255, 0, 0, 255, 255);
        alert("已给当前选中图层套用 Blend If：当前图层 59 / 125 / 255。");
    } catch (e) {
        alert("处理失败：\n" + e.message + "\n\n请确认当前没有打开“图层样式”弹窗，并且选中的是非 Background、非完全锁定的图层。");
    }
})();

function applyBlendIfGray(srcBlackMin, srcBlackMax, srcWhiteMin, srcWhiteMax, dstBlackMin, dstBlackMax, dstWhiteMin, dstWhiteMax) {
    var s = stringIDToTypeID;

    var desc = new ActionDescriptor();
    var layerRef = new ActionReference();
    var layerDesc = new ActionDescriptor();
    var blendRangeList = new ActionList();
    var grayDesc = new ActionDescriptor();
    var grayRef = new ActionReference();

    layerRef.putEnumerated(s("layer"), s("ordinal"), s("targetEnum"));
    desc.putReference(s("null"), layerRef);

    grayRef.putEnumerated(s("channel"), s("channel"), s("gray"));
    grayDesc.putReference(s("channel"), grayRef);

    grayDesc.putInteger(s("srcBlackMin"), srcBlackMin);
    grayDesc.putInteger(s("srcBlackMax"), srcBlackMax);
    grayDesc.putInteger(s("srcWhiteMin"), srcWhiteMin);
    grayDesc.putInteger(s("srcWhiteMax"), srcWhiteMax);
    grayDesc.putInteger(s("destBlackMin"), dstBlackMin);
    grayDesc.putInteger(s("destBlackMax"), dstBlackMax);
    grayDesc.putInteger(s("destWhiteMin"), dstWhiteMin);
    grayDesc.putInteger(s("destWhiteMax"), dstWhiteMax);

    blendRangeList.putObject(s("blendRange"), grayDesc);
    layerDesc.putList(s("blendRange"), blendRangeList);

    desc.putObject(s("to"), s("layer"), layerDesc);
    executeAction(s("set"), desc, DialogModes.NO);
}
