#target photoshop

// Exports every non-background, non-fully-locked ArtLayer in the active document
// as a separate transparent PNG into a new folder on the Desktop.
//
// It temporarily shows one layer at a time, duplicates the visible result into
// a temporary document, trims transparent pixels, exports PNG-24, then restores
// the original layer visibility states. The original document is not saved.

app.bringToFront();

var __exportState = {
    outputFolder: null,
    exported: [],
    failed: []
};

(function () {
    if (app.documents.length === 0) {
        writeLog("No open document.");
        return;
    }

    var sourceDoc = app.activeDocument;
    var originalLayer = sourceDoc.activeLayer;
    var layers = [];
    collectArtLayers(sourceDoc, layers);

    if (layers.length === 0) {
        writeLog("Document: " + sourceDoc.name + "\nNo exportable ArtLayers found.");
        return;
    }

    var visibilitySnapshot = [];
    snapshotVisibility(sourceDoc, visibilitySnapshot);

    var outputFolder = createOutputFolder(sourceDoc.name);
    __exportState.outputFolder = outputFolder.fsName;

    try {
        setAllVisibility(sourceDoc, false);

        for (var i = 0; i < layers.length; i++) {
            var layer = layers[i];
            var fileName = padNumber(i + 1, 3) + "_" + sanitizeFileName(layer.name) + ".png";
            var outFile = new File(outputFolder.fsName + "/" + fileName);

            try {
                sourceDoc.activeLayer = layer;
                layer.visible = true;
                exportVisibleMergedLayer(sourceDoc, outFile, fileName);
                __exportState.exported.push(outFile.fsName);
                layer.visible = false;
            } catch (e) {
                __exportState.failed.push(layer.name + ": " + e.message);
                try {
                    layer.visible = false;
                } catch (ignoreLayerHide) {
                }
            }
        }
    } finally {
        restoreVisibility(visibilitySnapshot);
        try {
            sourceDoc.activeLayer = originalLayer;
        } catch (ignoreRestoreLayer) {
        }
        app.activeDocument = sourceDoc;
    }

    writeLog(
        "Document: " + sourceDoc.name + "\n" +
        "OutputFolder: " + outputFolder.fsName + "\n" +
        "Exported: " + __exportState.exported.length + "\n" +
        "Failed: " + __exportState.failed.length + "\n\n" +
        "Files:\n" + __exportState.exported.join("\n") + "\n\n" +
        "Failures:\n" + __exportState.failed.join("\n")
    );
})();

function exportVisibleMergedLayer(sourceDoc, outFile, tempName) {
    app.activeDocument = sourceDoc;

    // mergeLayersOnly=true creates a new temporary document from visible content.
    var tempDoc = sourceDoc.duplicate("__export_" + tempName.replace(/\.png$/i, ""), true);
    app.activeDocument = tempDoc;

    try {
        try {
            tempDoc.trim(TrimType.TRANSPARENT, true, true, true, true);
        } catch (trimError) {
            // Keep full canvas if there is nothing trim-able.
        }

        var options = new ExportOptionsSaveForWeb();
        options.format = SaveDocumentType.PNG;
        options.PNG8 = false;
        options.transparency = true;
        options.interlaced = false;
        options.includeProfile = false;

        tempDoc.exportDocument(outFile, ExportType.SAVEFORWEB, options);
    } finally {
        tempDoc.close(SaveOptions.DONOTSAVECHANGES);
        app.activeDocument = sourceDoc;
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

function snapshotVisibility(container, snapshot) {
    for (var i = 0; i < container.layers.length; i++) {
        var layer = container.layers[i];
        snapshot.push({ layer: layer, visible: layer.visible });
        if (layer.typename === "LayerSet") {
            snapshotVisibility(layer, snapshot);
        }
    }
}

function restoreVisibility(snapshot) {
    for (var i = 0; i < snapshot.length; i++) {
        try {
            snapshot[i].layer.visible = snapshot[i].visible;
        } catch (e) {
        }
    }
}

function setAllVisibility(container, visible) {
    for (var i = 0; i < container.layers.length; i++) {
        var layer = container.layers[i];
        try {
            layer.visible = visible;
        } catch (e) {
        }
        if (layer.typename === "LayerSet") {
            setAllVisibility(layer, visible);
        }
    }
}

function createOutputFolder(documentName) {
    var baseName = sanitizeFileName(documentName.replace(/\.[^\.]+$/, ""));
    if (!baseName) {
        baseName = "Photoshop_Layers";
    }

    var folderName = baseName + "_PNG_\u5bfc\u51fa";
    var folder = new Folder(Folder.desktop.fsName + "/" + folderName);

    if (!folder.exists) {
        folder.create();
        return folder;
    }

    var timestamp = makeTimestamp();
    folder = new Folder(Folder.desktop.fsName + "/" + folderName + "_" + timestamp);
    folder.create();
    return folder;
}

function sanitizeFileName(name) {
    var safe = String(name || "Layer");
    safe = safe.replace(/[\\\/:\*\?\"\<\>\|]/g, "_");
    safe = safe.replace(/^\s+|\s+$/g, "");
    safe = safe.replace(/\s+/g, "_");
    if (safe.length > 80) {
        safe = safe.substring(0, 80);
    }
    return safe || "Layer";
}

function padNumber(value, width) {
    var text = String(value);
    while (text.length < width) {
        text = "0" + text;
    }
    return text;
}

function makeTimestamp() {
    var d = new Date();
    return d.getFullYear() +
        padNumber(d.getMonth() + 1, 2) +
        padNumber(d.getDate(), 2) + "_" +
        padNumber(d.getHours(), 2) +
        padNumber(d.getMinutes(), 2) +
        padNumber(d.getSeconds(), 2);
}

function writeLog(text) {
    var file = new File(File($.fileName).parent + "/Export_AllArtLayers_To_Desktop_PNG_Log.txt");
    file.encoding = "UTF8";
    file.open("w");
    file.write(text);
    file.close();
}
