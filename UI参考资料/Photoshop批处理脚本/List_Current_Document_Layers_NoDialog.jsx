#target photoshop

app.bringToFront();

(function () {
    var lines = [];
    if (app.documents.length === 0) {
        lines.push("No open document.");
        writeLog(lines.join("\n"));
        return;
    }

    var doc = app.activeDocument;
    lines.push("Document: " + doc.name);
    listLayers(doc, lines, "");
    writeLog(lines.join("\n"));
})();

function listLayers(container, lines, prefix) {
    for (var i = 0; i < container.layers.length; i++) {
        var layer = container.layers[i];
        lines.push(prefix + layer.typename + " | " + layer.name + " | visible=" + layer.visible);
        if (layer.typename === "LayerSet") {
            listLayers(layer, lines, prefix + "  ");
        }
    }
}

function writeLog(text) {
    var file = new File(File($.fileName).parent + "/Current_Document_Layers_Log.txt");
    file.encoding = "UTF8";
    file.open("w");
    file.write(text);
    file.close();
}
