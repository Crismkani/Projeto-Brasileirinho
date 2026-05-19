(function () {
    'use strict';

    var STORAGE_W = 'sbw';
    var STORAGE_C = 'sbc';
    var MIN_W = 160;
    var MAX_W = 460;

    function init() {
        var page    = document.getElementById('page-layout');
        var sidebar = document.getElementById('sidebar');
        var edge    = document.getElementById('sidebar-edge');
        var btn     = document.getElementById('sidebar-toggle');

        if (!page || !sidebar || !edge || !btn) return;

        // Restore saved width
        var savedW = parseInt(localStorage.getItem(STORAGE_W), 10);
        if (savedW >= MIN_W && savedW <= MAX_W) {
            sidebar.style.width = savedW + 'px';
        }

        // Restore collapsed state
        if (localStorage.getItem(STORAGE_C) === '1') {
            page.classList.add('sidebar-collapsed');
            btn.setAttribute('title', 'Expandir menu');
            btn.textContent = '›';
        }

        // Toggle collapse on button click
        btn.addEventListener('click', function (e) {
            e.stopPropagation();
            var collapsed = page.classList.toggle('sidebar-collapsed');
            localStorage.setItem(STORAGE_C, collapsed ? '1' : '0');
            btn.setAttribute('title', collapsed ? 'Expandir menu' : 'Recolher menu');
            btn.textContent = collapsed ? '›' : '‹';
        });

        // Resize drag on edge mousedown (ignore clicks on the toggle button)
        edge.addEventListener('mousedown', function (e) {
            if (btn.contains(e.target)) return;
            if (page.classList.contains('sidebar-collapsed')) return;

            e.preventDefault();
            edge.classList.add('dragging');
            document.body.style.userSelect = 'none';
            document.body.style.cursor = 'col-resize';

            function onMove(ev) {
                var w = Math.min(Math.max(ev.clientX, MIN_W), MAX_W);
                sidebar.style.width = w + 'px';
            }

            function onUp() {
                edge.classList.remove('dragging');
                document.body.style.userSelect = '';
                document.body.style.cursor = '';
                document.removeEventListener('mousemove', onMove);
                document.removeEventListener('mouseup', onUp);
                var w = parseInt(sidebar.style.width, 10);
                if (!isNaN(w)) localStorage.setItem(STORAGE_W, w);
            }

            document.addEventListener('mousemove', onMove);
            document.addEventListener('mouseup', onUp);
        });
    }

    if (document.readyState === 'loading') {
        document.addEventListener('DOMContentLoaded', init);
    } else {
        init();
    }
})();
