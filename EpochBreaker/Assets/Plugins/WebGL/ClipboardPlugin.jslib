mergeInto(LibraryManager.library, {
    WebGLCopyToClipboard: function(textPtr) {
        var text = UTF8ToString(textPtr);
        if (navigator.clipboard && navigator.clipboard.writeText) {
            navigator.clipboard.writeText(text).catch(function() {});
        }
    }
});
