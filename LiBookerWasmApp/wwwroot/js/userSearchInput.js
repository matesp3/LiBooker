window.userSearch = {
    setDropdownPosition: function (dropdownId, inputId) {
        try {
            var dd = document.getElementById(dropdownId);
            var input = document.getElementById(inputId);
            if (!dd || !input) return;

            var rect = input.getBoundingClientRect();
            // position dropdown as fixed to avoid parent stacking/overflow issues
            dd.style.position = "fixed";
            dd.style.left = rect.left + "px";
            dd.style.top = rect.bottom + "px";
            dd.style.width = rect.width + "px";
            dd.style.zIndex = "2147483646";
            dd.style.maxHeight = Math.min(window.innerHeight - rect.bottom - 12, 320) + "px";
            dd.style.display = "block";
        } catch (e) {
            console && console.warn && console.warn("userSearch.setDropdownPosition error", e);
        }
    },

    hideDropdownInlineStyles: function (dropdownId) {
        var dd = document.getElementById(dropdownId);
        if (!dd) return;
        // remove only inline positioning we set (keeps CSS classes intact)
        dd.style.position = "";
        dd.style.left = "";
        dd.style.top = "";
        dd.style.width = "";
        dd.style.zIndex = "";
        dd.style.maxHeight = "";
        dd.style.display = "";
    }
};