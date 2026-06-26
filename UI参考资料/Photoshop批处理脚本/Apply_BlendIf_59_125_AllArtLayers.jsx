#target photoshop

// Batch applies the same Blend If values shown in the reference screenshot.
//
// Blend If: Gray
// This Layer: black split 59 / 125, white 255 / 255
// Underlying Layer: black 0 / 0, white 255 / 255
//
// Default behavior:
// - Processes ArtLayers inside the whole document, including layers inside groups.
// - Skips the Background layer and fully locked layers.
// - Does not rasterize, merge, or permanently delete pixels.
// - Does not overwrite opacity, fill opacity, blend mode, layer masks, or text content.

app.bringToFront();

var __blendIfLayers = [];
var __blendIfResult = { ok: 0, failed: [] };

(function () {
    if (app.documents.length === 0) {
        alert("请先打开一个 PSD 文件。");
        return;
    }

    var doc = app.activeDocument;
    var originalLayer = doc.activeLayer;

    __blendIfLayers = [];
    __blendIfResult = { ok: 0, failed: [] };
    collectArtLayers(doc, __blendIfLayers);

    if (__blendIfLayers.length === 0) {
        alert("没有找到可处理的普通图层。");
        return;
    }

    var message = "将给 " + __blendIfLayers.length + " 个普通图层批量套用：\n\n" +
        "混合颜色带：灰色\n" +
        "当前图层：黑场 59 / 125，白场 255 / 255\n" +
        "下一图层：黑场 0 / 0，白场 255 / 255\n\n" +
        "建议先另存一份 PSD 备份。是否继续？";

    if (!confirm(message)) {
        return;
    }

    try {
        doc.suspendHistory("Apply Blend If 59-125", "__runBlendIfBatch()");
    } catch (e) {
        __runBlendIfBatch();
    }

    try {
        doc.activeLayer = originalLayer;
    } catch (restoreError) {
        // The original layer may have become unavailable. Safe to ignore.
    }

    var done = "完成：已处理 " + __blendIfResult.ok + " 个图层。";
    if (__blendIfResult.failed.length > 0) {
        done += "\n\n有 " + __blendIfResult.failed.length + " 个图层跳过或失败，前 10 个：\n" +
            __blendIfResult.failed.slice(0, 10).join("\n");
    }
    alert(done);
})();

function __runBlendIfBatch() {
    for (var i = 0; i < __blendIfLayers.length; i++) {
        var layer = __blendIfLayers[i];
        try {
            selectLayerById(layer.id);
            applyBlendIfGray(59, 125, 255, 255, 0, 0, 255, 255);
            __blendIfResult.ok++;
        } catch (e) {
            __blendIfResult.failed.push(layer.name + "： " + e.message);
        }
    }
}

function collectArtLayers(container, output) {
    for (var i = 0; i < container.layers.length; i++) {
        var layer = container.layers[i];

        if (layer.typename === "ArtLayer") {
            if (shouldProcessArtLayer(layer)) {
                output.push(layer);
            }
        } else if (layer.typename === "LayerSet") {
            collectArtLayers(layer, output);
        }
    }
}

function shouldProcessArtLayer(layer) {
    try {
        if (layer.isBackgroundLayer) {
            return false;
        }
    } catch (e1) {
    }

    try {
        if (layer.allLocked) {
            return false;
        }
    } catch (e2) {
    }

    return true;
}

function selectLayerById(layerId) {
    var c = charIDToTypeID;
    var desc = new ActionDescriptor();
    var ref = new ActionReference();
    ref.putIdentifier(c("Lyr "), layerId);
    desc.putReference(c("null"), ref);
    desc.putBoolean(c("MkVs"), false);
    executeAction(c("slct"), desc, DialogModes.NO);
}

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
