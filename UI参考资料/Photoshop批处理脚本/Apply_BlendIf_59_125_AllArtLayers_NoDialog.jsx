#target photoshop

// No-dialog version for automation from Codex/COM.
// Applies Blend If values to all non-background, non-fully-locked ArtLayers.
// Writes a small log next to this script.

app.bringToFront();

var __blendIfLayers = [];
var __blendIfResult = { ok: 0, failed: [] };

(function () {
    if (app.documents.length === 0) {
        writeLog("No open document.");
        return;
    }

    var doc = app.activeDocument;
    var originalLayer = doc.activeLayer;

    collectArtLayers(doc, __blendIfLayers);

    try {
        doc.suspendHistory("Apply Blend If 59-125", "__runBlendIfBatch()");
    } catch (e) {
        __runBlendIfBatch();
    }

    try {
        doc.activeLayer = originalLayer;
    } catch (restoreError) {
    }

    writeLog("Document: " + doc.name + "\nProcessed: " + __blendIfResult.ok + "\nFailed: " + __blendIfResult.failed.length + "\n" + __blendIfResult.failed.join("\n"));
})();

function __runBlendIfBatch() {
    for (var i = 0; i < __blendIfLayers.length; i++) {
        var layer = __blendIfLayers[i];
        try {
            selectLayerById(layer.id);
            applyBlendIfGray(59, 125, 255, 255, 0, 0, 255, 255);
            __blendIfResult.ok++;
        } catch (e) {
            __blendIfResult.failed.push(layer.name + ": " + e.message);
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
    var s = stringIDToTypeID;
    var desc = new ActionDescriptor();
    var ref = new ActionReference();
    ref.putIdentifier(s("layer"), layerId);
    desc.putReference(s("null"), ref);
    desc.putBoolean(s("makeVisible"), false);
    executeAction(s("select"), desc, DialogModes.NO);
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

function writeLog(text) {
    try {
        var file = new File(File($.fileName).parent + "/BlendIf_NoDialog_Log.txt");
        file.encoding = "UTF8";
        file.open("w");
        file.write(text);
        file.close();
    } catch (e) {
    }
}
