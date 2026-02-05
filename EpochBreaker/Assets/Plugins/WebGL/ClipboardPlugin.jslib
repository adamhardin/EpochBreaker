mergeInto(LibraryManager.library, {
    WebGLCopyToClipboard: function(textPtr) {
        var text = UTF8ToString(textPtr);
        if (navigator.clipboard && navigator.clipboard.writeText) {
            navigator.clipboard.writeText(text).catch(function() {
                // Fallback for older browsers
                var textArea = document.createElement("textarea");
                textArea.value = text;
                textArea.style.position = "fixed";
                textArea.style.left = "-9999px";
                document.body.appendChild(textArea);
                textArea.select();
                try { document.execCommand("copy"); } catch(e) {}
                document.body.removeChild(textArea);
            });
        }
    }
});
